using UnityEngine;
using System.Collections;

public class TileEnterZone : Task
{
    private readonly Tile tile;
    private readonly Zone zone;
    private float timeElapsed;
    private readonly float duration;

    public TileEnterZone(Tile tile_, Zone zone_)
    {
        tile = tile_;
        zone = zone_;
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
        tile.EnterZone(zone);
    }
}
