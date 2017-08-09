using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Tile
{
    public readonly Hex hex;
    public GameObject obj;
    public List<Tile> neighbors;
    public Worker containedWorker;
    public Resource containedResource;
    public Building containedBuilding;
    public Item containedItem;
    public Zone zone { get; private set; }
    public readonly Color moveAvailableColor = new Color(0.5f, 1f, 0.5f);
    private SpriteRenderer sr;

    public Tile(Hex hex_)
    {
        hex = hex_;
        obj = GameObject.Instantiate(Services.Prefabs.Tile, hex.ScreenPos(Services.MapManager.layout), Quaternion.identity,
            Services.SceneStackManager.CurrentScene.transform);
        sr = obj.GetComponent<SpriteRenderer>();
        neighbors = new List<Tile>();
        sr.sprite = Services.MapManager.defaultTileSprite;
    }

    public void EnterZone(Zone zone_)
    {
        zone = zone_;
        zone.AddTile(this);
        sr.sprite = zone.sprite;
    }

    public void ExitZone()
    {
        zone.RemoveTile(this);
        zone = null;
        sr.sprite = Services.MapManager.defaultTileSprite;
    }
}
