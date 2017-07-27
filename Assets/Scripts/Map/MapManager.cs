using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapManager : MonoBehaviour {

    public int radius;
    public int resourceTilesMin;
    public int resourceTilesMax;
    public int resourceAmtMin;
    public int resourceAmtMax;
    public int maxTriesProcGen;
    public int minDistResourceTiles;
    public int minRadiusResourceTiles;

    public int minRadiusItems;
    public int minDistItems;
    public int numItems;
    public int itemDistAntivariance;
    public int approxMinStartingItemVal;
    public int approxMaxStartingItemVal;

    [HideInInspector]
    public List<Tile> resourceTiles;
    [HideInInspector]
    public List<Tile> itemTiles;
    [HideInInspector]
    public List<Tile> buildingTiles;
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
    [HideInInspector]
    public readonly Layout layout = new Layout(Orientation.pointy, new Vector2(1, 0.6f), Vector2.zero);
    [HideInInspector]
    public Dictionary<Hex, Tile> map;
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
            int resourceValue = Random.Range(minVal, maxVal + 1);
            resource.Init(resourceValue, tile);
            tile.containedResource = resource;
            resourceTiles.Add(tile);
            return tile;
        }
        else return null;
    }

    Tile GenerateValidTile(int minRadius, int minDist)
    {
        Tile tile = GenerateCandidateTile();
        for (int i = 0; i < maxTriesProcGen; i++)
        {
            tile = GenerateCandidateTile();
            bool isValid = ValidateTile(tile, minRadius, minDist);
            if (isValid) return tile;
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
        if (candidateTile.hex.Length() < minRadius || candidateTile.containedWorker != null) return false;
        if (occupiedTiles.Count == 0) return true;
        else foreach (Tile tile in occupiedTiles) if (candidateTile.hex.Distance(tile.hex) < minDist) return false;
        return true;
    }

    Item GenerateItem(int approxMinVal, int approxMaxVal, Tile tile, Item.StatType statType)
    {
        Dictionary<Item.StatType, int> bonuses = new Dictionary<Item.StatType, int>();
        int targetValue = Distribution(approxMinVal, approxMaxVal, itemDistAntivariance);
        int cost = 0;
        while(cost < targetValue)
        {
            if (!bonuses.ContainsKey(statType)) bonuses[statType] = 1;
            else bonuses[statType] += 1;
            cost += Item.costPerStat[statType];
        }
        return new Item(bonuses, tile);
    }

    int Distribution(int min, int max, int antivariance)
    {
        int result = 0;
        for (int i = 0; i < antivariance; i++)
        {
            result += Random.Range(min, max + 1);
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
        statBonuses[statType] = 6/Item.costPerStat[statType];
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

    List<Item.StatType> CreateStatPool(int numEachStat)
    {
        List<Item.StatType> statPool = new List<Item.StatType>();
        foreach(Item.StatType type in Item.statTypes)
        {
            for (int i = 0; i < numEachStat; i++)
            {
                statPool.Add(type);
            }
        }
        return statPool;
    }
}
