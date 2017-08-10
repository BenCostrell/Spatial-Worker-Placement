using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

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
    public bool tooltipActive { get; private set; }
    private GameObject tooltip;

    public Zone(Tile centerTile_)
    {
        tiles = new List<Tile>();
        centerTile = centerTile_;
    }

    public void GetClaimed(Worker worker)
    {
        controller = worker.parentPlayer;
        foreach (Tile tile in tiles) ApplyControl(tile);
        Services.main.taskManager.AddTask(Expand(worker.zoneExpandPower));
    }

    void ApplyControl(Tile tile)
    {
        tile.SetBaseColorFromZone(this);
        ApplyZoneEffectToTile(tile);
    }

    public TaskTree Expand(int steps)
    {
        TaskTree fullExpandTaskTree = new TaskTree(new EmptyTask());
        for (int i = 0; i < steps; i++)
        {
            fullExpandTaskTree.Then (ExpandOneStep());
        }
        return fullExpandTaskTree;
    }

    TaskTree ExpandOneStep()
    {
        TaskTree expandTaskTree = new TaskTree(new EmptyTask());
        expansionLevel += 1;
        List<Tile> currentTiles = new List<Tile>(tiles);
        List<Tile> newlyAddedTiles = new List<Tile>();
        foreach (Tile tile in currentTiles)
        {
            foreach(Tile neighbor in tile.neighbors)
            {
                if (!currentTiles.Contains(neighbor) && !newlyAddedTiles.Contains(neighbor))
                {
                    expandTaskTree.Then(new TileEnterZone(neighbor, this));
                    newlyAddedTiles.Add(neighbor);
                }
            }
        }
        return expandTaskTree;
    }

    public TaskTree Decrement()
    {
        TaskTree decrementTaskTree = new TaskTree(new EmptyTask());
        expansionLevel -= 1;
        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            if (tiles[i].hex.Distance(centerTile.hex) > expansionLevel)
            {
                decrementTaskTree.Then(new TileExitZone(tiles[i]));
            }
        }
        if (expansionLevel < 0) Services.MapManager.RemoveZone(this);
        return decrementTaskTree;
    }

    public void RemoveTile(Tile tile)
    {
        tiles.Remove(tile);
        RemoveZoneEffectFromTile(tile);
    }

    public void AddTile(Tile tile)
    {
        tiles.Add(tile);
        if (controller != null) ApplyControl(tile);
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

    protected virtual void OnRoundEndForWorker(Worker worker) { }

    public Color GetColorTint()
    {
        return Color.white * (1 - Services.ZoneConfig.ColorTintProportion) +
                controller.color * Services.ZoneConfig.ColorTintProportion;
    }

    public virtual void ApplyZoneEffectToTile(Tile tile) { }

    public virtual void RemoveZoneEffectFromTile(Tile tile) { }

    public void ShowTooltip()
    {
        tooltipActive = true;
        tooltip = GameObject.Instantiate(Services.Prefabs.ZoneTooltip,
            Services.UIManager.canvas);
        RectTransform tooltipRect = tooltip.GetComponent<RectTransform>();
        tooltipRect.anchoredPosition = new Vector2(0, 0);
        tooltip.GetComponentInChildren<Text>().text = zoneTypeInfo.Label;
        tooltip.GetComponentInChildren<Image>().color = Services.ZoneConfig.TooltipColor;
        Services.main.taskManager.AddTask(new ExpandTooltip(tooltip));
    }

    public void HideTooltip()
    {
        if (tooltip != null) GameObject.Destroy(tooltip);
    }
}
