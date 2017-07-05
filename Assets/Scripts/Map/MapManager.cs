using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {

    public int radius;
    [HideInInspector]
    public readonly Layout layout = new Layout(Orientation.pointy, Vector2.one, Vector2.zero);
    public List<Tile> map;

    // Use this for initialization
    void Start() {

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void CreateHexGrid()
    {
        map = new List<Tile>();
        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                map.Add(new Tile(new Hex(q, r, -q - r)));
            }
        }
    }
}
