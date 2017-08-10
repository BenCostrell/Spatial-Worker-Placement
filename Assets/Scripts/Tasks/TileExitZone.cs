using UnityEngine;
using System.Collections;

public class TileExitZone : Task
{
    private readonly Tile tile;
    private float timeElapsed;
    private readonly float duration;

    public TileExitZone(Tile tile_)
    {
        tile = tile_;
        duration = Services.ZoneConfig.ExpandTime;
    }

    protected override void Init()
    {
        timeElapsed = 0;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        tile.ExitZone();
    }
}
