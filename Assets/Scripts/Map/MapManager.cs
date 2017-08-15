using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapManager : MonoBehaviour {

    [SerializeField]
    private int radius;
    [SerializeField]
    private int resourceTilesMin;
    [SerializeField]
    private int resourceTilesMax;
    [SerializeField]
    private int resourceAmtMin;
    [SerializeField]
    private int resourceAmtMax;
    public int resourceAmtIncrement;
    [SerializeField]
    private int maxTriesProcGen;
    [SerializeField]
    private int minDistResourceTiles;
    [SerializeField]
    private int minRadiusResourceTiles;

    [SerializeField]
    private int minRadiusItems;
    [SerializeField]
    private int minDistItems;
    [SerializeField]
    private int numItems;
    [SerializeField]
    private int itemDistAntivariance;
    [SerializeField]
    private int approxMinStartingItemVal;
    [SerializeField]
    private int approxMaxStartingItemVal;

    [SerializeField]
    private int numZones;
    [SerializeField]
    private int minRadiusZones;
    [SerializeField]
    private int minDistZones;
    [SerializeField]
    private int minDistZoneToZone;
    public Sprite defaultTileSprite;

    [SerializeField]
    private int towerStatValue;

    public List<Zone> currentActiveZones { get; private set; }
    public List<Tile> resourceTiles { get; private set; }
    public List<Tile> itemTiles { get; private set; }
    public List<Tile> buildingTiles { get; private set; }
    private List<Tile> occupiedTiles
    {
        get {
            List<Tile> occTiles = new List<Tile>();
            if (buildingTiles != null) occTiles.AddRange(buildingTiles);
            if (resourceTiles != null) occTiles.AddRange(resourceTiles);
            if (itemTiles != null) occTiles.AddRange(itemTiles);
            return occTiles;
        }
    }
    public readonly Layout layout = new Layout(Orientation.pointy, new Vector2(1, 0.6f), Vector2.zero);
    public Dictionary<Hex, Tile> map { get; private set; }
    private List<Hex> keys;


    // Use this for initialization
    void Start() {

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void CreateHexGrid()
    {
        map = new Dictionary<Hex, Tile>();
        keys = new List<Hex>();
        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                Hex hex = new Hex(q, r, -q - r);
                Tile tile = new Tile(hex);
                tile.obj.transform.localScale = layout.size;
                map.Add(hex, tile);
                keys.Add(hex);
            }
        }
        LogAllNeighbors();
        GenerateBuildings();
        GenerateStartingResources();
        GenerateStartingItems();
        GenerateStartingZones();
    }

    void LogAllNeighbors()
    {
        foreach (Tile tile in map.Values) LogNeighbors(tile);
    }

    void LogNeighbors(Tile tile)
    {
        for (int i = 0; i < 6; i++)
        {
            Hex potentialNeighborCoordinate = tile.hex.Neighbor(i);
            if (map.ContainsKey(potentialNeighborCoordinate)) tile.neighbors.Add(map[potentialNeighborCoordinate]);
        }
    }

    void GenerateStartingResources()
    {
        resourceTiles = new List<Tile>();
        int numResourceTiles = Random.Range(resourceTilesMin, resourceTilesMax + 1);
        for (int i = 0; i < numResourceTiles; i++)
        {
            Tile resourceTile = GenerateResourceTile(resourceAmtMin, resourceAmtMax);
            if (resourceTile == null) break;
        }
    }

    void GenerateStartingItems()
    {
        Item.StatType statType;
        List<Item.StatType> statPool = CreateStatPool(2);
        itemTiles = new List<Tile>();
        for (int i = 0; i < numItems; i++)
        {
            statType = statPool[Random.Range(0, statPool.Count)];
            statPool.Remove(statType);
            Item item = GenerateAndPlaceItem(approxMinStartingItemVal, approxMaxStartingItemVal, 
                minRadiusItems, statType);
            if (item == null) break;
        }
    }

    void GenerateStartingZones()
    {
        currentActiveZones = new List<Zone>();
        for (int i = 0; i < numZones; i++)
        {
            GenerateZone(minRadiusZones, minDistZones, minDistZoneToZone);
        }
    }

    Zone GenerateZone(int minRadius, int minDist, int minZoneDist)
    {
        Zone.ZoneType type = GenerateRandomZoneType();
        Tile location = GenerateValidZoneTile(minRadius, minDist, minZoneDist);
        Zone zone;
        switch (type)
        {
            case Zone.ZoneType.ResourceDrain:
                zone = new ResourceDrainZone(location);
                break;
            case Zone.ZoneType.TowerDepreciate:
                zone = new TowerDepreciateZone(location);
                break;
            case Zone.ZoneType.MovementSlow:
                zone = new MovementSlowZone(location);
                break;
            default:
                return null;
        }
        currentActiveZones.Add(zone);
        return zone;
    }

    Zone.ZoneType GenerateRandomZoneType()
    {
        ZoneTypeInfo randomTypeInfo =
            Services.ZoneConfig.Zones[Random.Range(0, Services.ZoneConfig.Zones.Length)];
        return randomTypeInfo.Type;
    }

    Tile GenerateValidZoneTile(int minRadius, int minDist, int minZoneDist)
    {
        Tile tile;
        for (int i = 0; i < maxTriesProcGen; i++)
        {
            tile = GenerateValidTile(minRadius, minDist);
            if (ValidateZoneTile(tile, minZoneDist)) return tile;
        }
        return null;
    }

    bool ValidateZoneTile(Tile tile, int minZoneDist)
    {
        if (currentActiveZones.Count > 0)
        {
            foreach (Zone zone in currentActiveZones)
            {
                foreach (Tile zoneTile in zone.tiles)
                {
                    if (tile.hex.Distance(zoneTile.hex) < minZoneDist) return false;
                }
            }
        }
        return true;
    }

    Item GenerateAndPlaceItem(int approxMinVal, int approxMaxVal, int minRadius, Item.StatType statType)
    {
        Tile tile = GenerateValidTile(minRadius, minDistItems);
        if (tile != null)
        {
            Item item = GenerateItem(approxMinVal, approxMaxVal, tile, statType);
            itemTiles.Add(tile);
            tile.containedItem = item;
            return item;
        }
        Debug.Log("failed to make item");
        return null;
    }

    Tile GenerateResourceTile(int minVal, int maxVal)
    {
        Tile tile = GenerateValidTile(minRadiusResourceTiles, minDistResourceTiles);
        if (tile != null)
        {
            Resource resource = Instantiate(Services.Prefabs.Resource,
                    Services.SceneStackManager.CurrentScene.transform).GetComponent<Resource>();
            int minNumIncrements = minVal / resourceAmtIncrement;
            int maxNumIncrements = maxVal / resourceAmtIncrement;
            int resourceValue = 
                Random.Range(minNumIncrements, maxNumIncrements + 1) * resourceAmtIncrement;
            resource.Init(resourceValue, tile);
            tile.containedResource = resource;
            resourceTiles.Add(tile);
            return tile;
        }
        else return null;
    }

    Tile GenerateValidTile(int minRadius, int minDist)
    {
        Tile tile;
        for (int i = 0; i < maxTriesProcGen; i++)
        {
            tile = GenerateCandidateTile();
            if(ValidateTile(tile, minRadius, minDist)) return tile;
        }
        return null;
    }

    Tile GenerateCandidateTile()
    {
        int index = Random.Range(0, keys.Count);
        Hex hex = keys[index];
        return map[hex];
    }

    bool ValidateTile(Tile candidateTile, int minRadius, int minDist)
    {
        if (candidateTile.hex.Length() < minRadius || candidateTile.containedWorker != null)
            return false;
        if (occupiedTiles.Count == 0)
            return true;
        else foreach (Tile tile in occupiedTiles)
                if (candidateTile.hex.Distance(tile.hex) < minDist)
                    return false;
        return true;
    }

    Item GenerateItem(int approxMinVal, int approxMaxVal, Tile tile, Item.StatType statType)
    {
        Dictionary<Item.StatType, int> bonuses = new Dictionary<Item.StatType, int>();
        int targetValue = Distribution(approxMinVal, approxMaxVal, itemDistAntivariance);
        ItemStatInfo statInfo = Services.ItemConfig.GetItemStatConfig(statType);
        int statCost = statInfo.Cost;
        int bonus = Mathf.Max(((targetValue / statCost) / statInfo.RoundToNearest) 
            * statInfo.RoundToNearest, statInfo.RoundToNearest);
        bonuses[statType] = bonus;
        return new Item(bonuses, tile);
    }

    int Distribution(int min, int max, int antivariance)
    {
        int result = 0;
        int minNumIncrements = min / resourceAmtIncrement;
        int maxNumIncrements = max / resourceAmtIncrement;
        for (int i = 0; i < antivariance; i++)
        {
            result += 
                Random.Range(minNumIncrements, maxNumIncrements + 1) * resourceAmtIncrement;
        }
        result = result / antivariance;
        return result;
    }

    void GenerateBuildings()
    {
        buildingTiles = new List<Tile>();
        Services.main.buildings = new List<Building>();
        Item.StatType statType;
        List<Item.StatType> statPool = CreateStatPool(2);
        for (int i = 0; i < 6; i++)
        {
            Hex hexCoord = Hex.Direction(i).Multiply(radius);
            statType = statPool[Random.Range(0, statPool.Count)];
            statPool.Remove(statType);
            Tile tile = GenerateAndPlaceBuilding(hexCoord, statType);
            buildingTiles.Add(tile);
        }
        statType = statPool[Random.Range(0, statPool.Count)];
        statPool.Remove(statType);
        Tile centerTile = GenerateAndPlaceBuilding(new Hex(0, 0, 0), statType);
        buildingTiles.Add(centerTile);
    }

    Tile GenerateAndPlaceBuilding(Hex coord, Item.StatType statType)
    {
        Building building = Instantiate(Services.Prefabs.Building, 
            Services.SceneStackManager.CurrentScene.transform).GetComponent<Building>();
        Tile tile = map[coord];
        Dictionary<Item.StatType, int> statBonuses = new Dictionary<Item.StatType, int>();
        ItemStatInfo statInfo = Services.ItemConfig.GetItemStatConfig(statType);
        statBonuses[statType] = Mathf.Max(
            ((towerStatValue / statInfo.Cost) / statInfo.RoundToNearest) * statInfo.RoundToNearest,
            statInfo.RoundToNearest);
        building.Init(tile, statBonuses);
        tile.containedBuilding = building;
        Services.main.buildings.Add(building);
        return tile;
    }

    public void SpawnNewItems(int approxMinVal, int approxMaxVal)
    {
        int numNewItems = numItems - itemTiles.Count;
        List<Item.StatType> statPool = CreateStatPool(2);
        Item.StatType statType;
        foreach(Tile tile in itemTiles)
        {
            statPool.Remove(tile.containedItem.statBonuses.Keys.First());
        }
        for (int i = 0; i < numNewItems; i++)
        {
            statType = statPool[Random.Range(0, statPool.Count)];
            statPool.Remove(statType);
            GenerateAndPlaceItem(approxMinVal, approxMaxVal, 0, statType);
        }
    }

    public void SpawnNewResources()
    {
        int numNewResources = resourceTilesMax - resourceTiles.Count;
        for (int i = 0; i < numNewResources; i++)
        {
            GenerateResourceTile(0, 0);
        }
    }

    public void SpawnNewZones()
    {
        int numNewZones = numZones - currentActiveZones.Count;
        for (int i = 0; i < numNewZones; i++)
        {
            GenerateZone(0, minDistZones, minDistZoneToZone);
        }
    }

    List<Item.StatType> CreateStatPool(int numEachStat)
    {
        List<Item.StatType> statPool = new List<Item.StatType>();
        foreach(ItemStatInfo itemStatConfig in Services.ItemConfig.Items)
        {
            for (int i = 0; i < numEachStat; i++)
            {
                statPool.Add(itemStatConfig.StatType);
            }
        }
        return statPool;
    }

    public void RemoveZone(Zone zone)
    {
        currentActiveZones.Remove(zone);
    }
}
