using UnityEngine;
using System.Collections;

public class ResourceDrainZone : Zone
{
    public ResourceDrainZone(Tile centerTile_) : base(centerTile_)
    {
        type = ZoneType.ResourceDrain;
        zoneTypeInfo = Services.ZoneConfig.GetZoneTypeInfo(type);
        sprite = zoneTypeInfo.Sprite;
        centerTile.EnterZone(this);
    }

    protected override void OnRoundEndForWorker(Worker worker)
    {
        worker.DrainResources(1);
    }
}
