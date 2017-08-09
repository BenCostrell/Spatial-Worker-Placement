using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Zone 
{
    public List<Tile> tiles { get; protected set; }
    public Player controller { get; private set; }
    protected int expansionLevel;
    protected readonly Tile centerTile;
    public enum ZoneType { ResourceDrain, TowerDepreciate, MovementSlow }
    protected ZoneType type;
    protected ZoneTypeInfo zoneTypeInfo;
    public Sprite sprite { get; protected set; }

    public Zone(Tile centerTile_)
    {
        tiles = new List<Tile>();
        centerTile = centerTile_;
    }

    public void GetClaimed(Worker worker)
    {
        controller = worker.parentPlayer;
        Expand(worker.zoneExpandPower);
        foreach(Tile tile in tiles)
        {
            tile.SetBaseColorFromZone(this);
        }
    }

    public void Expand(int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            ExpandOneStep();
        }
    }

    void ExpandOneStep()
    {
        expansionLevel += 1;
        List<Tile> currentTiles = new List<Tile>(tiles);
        foreach (Tile tile in currentTiles)
        {
            foreach(Tile neighbor in tile.neighbors)
            {
                if (!tiles.Contains(neighbor))
                {
                    neighbor.EnterZone(this);
                }
            }
        }
    }

    public void Decrement()
    {
        expansionLevel -= 1;
        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            if (tiles[i].hex.Distance(centerTile.hex) > expansionLevel)
            {
                tiles[i].ExitZone();
            }
        }
        if (expansionLevel < 0) Services.MapManager.RemoveZone(this);
    }

    public void RemoveTile(Tile tile)
    {
        tiles.Remove(tile);
    }

    public void AddTile(Tile tile)
    {
        tiles.Add(tile);
    }

    public void OnRoundEnd()
    {
        foreach(Tile tile in tiles)
        {
            if (tile.containedWorker != null && 
                tile.containedWorker.parentPlayer != controller)
            {
                OnRoundEndForWorker(tile.containedWorker);
            }
        }
    }

    protected virtual void OnRoundEndForWorker(Worker worker)
    {

    }

    public Color GetColorTint()
    {
        return Color.white * (1 - Services.ZoneConfig.ColorTintProportion) +
                controller.color * Services.ZoneConfig.ColorTintProportion;
    }
}
