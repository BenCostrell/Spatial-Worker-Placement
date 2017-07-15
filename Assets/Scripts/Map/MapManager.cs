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

    [HideInInspector]
    public List<Tile> resourceTiles;
    [HideInInspector]
    public List<Tile> itemTiles;
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
            if (resourceTile != null) resourceTiles.Add(resourceTile);
            else break;
        }
    }

    void GenerateStartingItems()
    {
        itemTiles = new List<Tile>();
        for (int i = 0; i < numItems; i++)
        {
            Item item = GenerateAndPlaceItem();
            if (item == null) break;
        }
    }

    Item GenerateAndPlaceItem()
    {
        for (int i = 0; i < maxTriesProcGen; i++)
        {
            Tile tile = GenerateCandidateTile();
            bool isValid = ValidateItemTile(tile);
            if (isValid)
            {
                Item item = GenerateItem(2, 5, tile);
                itemTiles.Add(tile);
                tile.containedItem = item;
                return item;
            }
        }
        Debug.Log("stopping after too many tries");
        return null;
    }

    Tile GenerateResourceTile()
    {
        Tile candidateTile;
        for (int i = 0; i < maxTriesProcGen; i++)
        {
            candidateTile = GenerateCandidateTile();
            bool isValid = ValidateResourceTile(candidateTile);
            if (isValid)
            {
                Resource resource = Instantiate(Services.Prefabs.Resource, 
                    Services.SceneStackManager.CurrentScene.transform).GetComponent<Resource>();
                int resourceValue = Random.Range(resourceAmtMin, resourceAmtMax + 1);
                resource.Init(resourceValue, candidateTile);
                candidateTile.containedResource = resource;
                return candidateTile;
            }
        }
        return null;
    }

    Tile GenerateCandidateTile()
    {
        int index = Random.Range(0, keys.Count);
        Hex hex = keys[index];
        return map[hex];
    }

    bool ValidateResourceTile(Tile candidateTile)
    {
        if (candidateTile.hex.Length() < minRadiusResourceTiles) return false;
        if (resourceTiles.Count == 0) return true;
        else {
            foreach (Tile tile in resourceTiles)
            {
                if (candidateTile.hex.Distance(tile.hex) < minDistResourceTiles) return false;
            }
        }
        return true;
    }

    bool ValidateItemTile(Tile candidateTile)
    {
        if (candidateTile.hex.Length() < minRadiusItems) return false;
        List<Tile> tilesWithStuff = new List<Tile>(resourceTiles.Concat(itemTiles));
        foreach(Tile tile in tilesWithStuff)
        {
            if (candidateTile.hex.Distance(tile.hex) < minDistItems) return false;
        }
        return true;
    }

    Item GenerateItem(int min, int max, Tile tile)
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
        int targetValue = Distribution(min, max, itemDistAntivariance);
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
}
