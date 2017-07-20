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
    private List<Tile> occupiedTiles;
    [HideInInspector]
    public readonly Layout layout = new Layout(Orientation.pointy, Vector2.one, Vector2.zero);
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
        occupiedTiles = new List<Tile>();
        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                Hex hex = new Hex(q, r, -q - r);
                map.Add(hex, new Tile(hex));
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
            Tile resourceTile = GenerateResourceTile();
            if (resourceTile != null)
            {
                resourceTiles.Add(resourceTile);
                occupiedTiles.Add(resourceTile);
            }
            else break;
        }
    }

    void GenerateStartingItems()
    {
        itemTiles = new List<Tile>();
        for (int i = 0; i < numItems; i++)
        {
            Item item = GenerateAndPlaceItem(approxMinStartingItemVal, approxMaxStartingItemVal, minRadiusItems);
            if (item == null) break;
        }
    }

    Item GenerateAndPlaceItem(int approxMinVal, int approxMaxVal, int minRadius)
    {
        Tile tile = GenerateValidTile(minRadius, minDistItems);
        if (tile != null)
        {
            Item item = GenerateItem(approxMinVal, approxMaxVal, tile);
            itemTiles.Add(tile);
            occupiedTiles.Add(tile);
            tile.containedItem = item;
            Debug.Log("made item");
            return item;
        }
        Debug.Log("failed to make item");
        return null;
    }

    Tile GenerateResourceTile()
    {
        Tile tile = GenerateValidTile(minRadiusResourceTiles, minDistResourceTiles);
        if (tile != null)
        {
            Resource resource = Instantiate(Services.Prefabs.Resource,
                    Services.SceneStackManager.CurrentScene.transform).GetComponent<Resource>();
            int resourceValue = Random.Range(resourceAmtMin, resourceAmtMax + 1);
            resource.Init(resourceValue, tile);
            tile.containedResource = resource;
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

    Item GenerateItem(int approxMinVal, int approxMaxVal, Tile tile)
    {
        int typeNumRandomizer = Random.Range(0, 100);
        int numTypes;
        if (typeNumRandomizer < 66) numTypes = 1;
        else numTypes = 2;
        List<Item.StatType> statTypes = new List<Item.StatType>();
        for (int i = 0; i < numTypes; i++)
        {
            int randomIndex = Random.Range(0, Item.statTypes.Count);
            statTypes.Add(Item.statTypes[randomIndex]);
        }
        Dictionary<Item.StatType, int> bonuses = new Dictionary<Item.StatType, int>();
        int targetValue = Distribution(approxMinVal, approxMaxVal, itemDistAntivariance);
        int cost = 0;
        while(cost < targetValue)
        {
            int index = Random.Range(0, numTypes);
            Item.StatType stat = statTypes[index];
            if (!bonuses.ContainsKey(stat)) bonuses[stat] = 1;
            else bonuses[stat] += 1;
            cost += Item.costPerStat[stat];
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
        for (int i = 0; i < 6; i++)
        {
            Hex hexCoord = Hex.Direction(i).Multiply(radius);
            Tile tile = GenerateAndPlaceBuilding(hexCoord);
            buildingTiles.Add(tile);
            occupiedTiles.Add(tile);
        }
    }

    Tile GenerateAndPlaceBuilding(Hex coord)
    {
        Building building = Instantiate(Services.Prefabs.Building, 
            Services.SceneStackManager.CurrentScene.transform).GetComponent<Building>();
        Tile tile = map[coord];
        Dictionary<Item.StatType, int> statBonuses = new Dictionary<Item.StatType, int>();
        Item.StatType statType = Item.statTypes[Random.Range(0, Item.statTypes.Count)];
        statBonuses[statType] = 1;
        building.Init(tile, statBonuses);
        tile.containedBuilding = building;
        Services.main.buildings.Add(building);
        return tile;
    }

    public void SpawnNewItems(int approxMinVal, int approxMaxVal)
    {
        int numNewItems = numItems - itemTiles.Count;
        Debug.Log(numNewItems);
        if (numNewItems > 0) for (int i = 0; i < numNewItems; i++)
                GenerateAndPlaceItem(approxMinVal, approxMaxVal, 0);
    }
}
