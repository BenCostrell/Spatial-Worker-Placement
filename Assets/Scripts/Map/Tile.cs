using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Tile
{
    public readonly Hex hex;
    [HideInInspector]
    public GameObject obj;
    [HideInInspector]
    public List<Tile> neighbors;
    [HideInInspector]
    public Worker containedWorker;
    [HideInInspector]
    public Resource containedResource;
    [HideInInspector]
    public Building containedBuilding;
    [HideInInspector]
    public Item containedItem;
    public readonly Color moveAvailableColor = new Color(0.5f, 1f, 0.5f);

    public Tile(Hex hex_)
    {
        hex = hex_;
        obj = GameObject.Instantiate(Services.Prefabs.Tile, hex.ScreenPos(Services.MapManager.layout), Quaternion.identity,
            Services.SceneStackManager.CurrentScene.transform);
        neighbors = new List<Tile>();
    }
}
