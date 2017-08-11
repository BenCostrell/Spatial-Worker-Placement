using UnityEngine;
using System.Collections;

public class TileExitZone : Task
{
    private readonly Tile tile;
    private float timeElapsed;
    private readonly float duration;
    private readonly Zone zone;

    public TileExitZone(Tile tile_, Zone zone_)
    {
        tile = tile_;
        zone = zone_;
        duration = Services.ZoneConfig.ExpandTime;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        if (tile.zone != zone) SetStatus(TaskStatus.Success);
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        if (tile.zone == zone) tile.ExitZone();
    }
}
