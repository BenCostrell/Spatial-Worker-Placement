using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : MonoBehaviour {

    [HideInInspector]
    public Player parentPlayer;
    [HideInInspector]
    public Tile currentTile;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlaceOnTile(Tile tile)
    {
        currentTile = tile;
        transform.position = tile.hex.ScreenPos();
    }
}
