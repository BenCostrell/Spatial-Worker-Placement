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
    public readonly Color moveAvailableTint = new Color(1f, 1f, 0f);
    public readonly float moveAvailableTintProportion = 1f;
    public readonly Color moveUnavailableTint = Color.black;
    public readonly float moveUnavailableTintProportion = 0.5f;
    public Color currentBaseColor { get; private set; }
    public SpriteRenderer sr { get; private set; }
    private GameObject border;
    public SpriteRenderer borderSr { get; private set; }
    public int[] movementCostPerPlayer;

    public Tile(Hex hex_)
    {
        hex = hex_;
        obj = GameObject.Instantiate(Services.Prefabs.Tile, hex.ScreenPos(Services.MapManager.layout), Quaternion.identity,
            Services.SceneStackManager.CurrentScene.transform);
        sr = obj.GetComponent<SpriteRenderer>();
        neighbors = new List<Tile>();
        sr.sprite = Services.MapManager.defaultTileSprite;
        currentBaseColor = Color.white;
        border = obj.transform.GetChild(0).gameObject;
        borderSr = border.GetComponent<SpriteRenderer>();
        border.SetActive(false);
        movementCostPerPlayer = new int[Services.GameManager.numPlayers];
        for (int i = 0; i < Services.GameManager.numPlayers; i++)
        {
            movementCostPerPlayer[i] = 1;
        }
    }

    public void EnterZone(Zone zone_)
    {
        if (zone != null) ExitZone();
        zone = zone_;
        zone.AddTile(this);
        sr.sprite = zone.sprite;
        if (zone.controller != null) SetBaseColorFromZone(zone);
    }

    public void ExitZone()
    {
        zone.RemoveTile(this);
        zone = null;
        sr.sprite = Services.MapManager.defaultTileSprite;
        currentBaseColor = Color.white;
        border.SetActive(false);
    }

    public void SetBaseColorFromZone(Zone zone)
    {
        border.SetActive(true);
        currentBaseColor = zone.GetColorTint();
        borderSr.color = currentBaseColor;
    }
}
