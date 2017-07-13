using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {

    public int radius;
    public int resourceTilesMin;
    public int resourceTilesMax;
    public int resourceAmtMin;
    public int resourceAmtMax;
    public int maxTriesResourceGen;
    public int minDistResourceTiles;
    public int minRadiusResourceTiles;

    [HideInInspector]
    public List<Tile> resourceTiles;
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

    Tile GenerateResourceTile()
    {
        Tile candidateTile;
        for (int i = 0; i < maxTriesResourceGen; i++)
        {
            candidateTile = GenerateCandidateResourceTile();
            bool isValid = ValidateResourceTile(candidateTile);
            if (isValid)
            {
                Resource resource = Instantiate(Services.Prefabs.Resource).GetComponent<Resource>();
                int resourceValue = Random.Range(resourceAmtMin, resourceAmtMax + 1);
                resource.Init(resourceValue, candidateTile);
                candidateTile.containedResource = resource;
                return candidateTile;
            }
        }
        return null;
    }

    Tile GenerateCandidateResourceTile()
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
}
