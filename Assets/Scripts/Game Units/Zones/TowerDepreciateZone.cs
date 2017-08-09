using UnityEngine;
using System.Collections;

public class TowerDepreciateZone : Zone
{
    public TowerDepreciateZone(Tile centerTile_) : base(centerTile_)
    {
        type = ZoneType.TowerDepreciate;
        zoneTypeInfo = Services.ZoneConfig.GetZoneTypeInfo(type);
        sprite = zoneTypeInfo.Sprite;
        centerTile.EnterZone(this);
    }

    protected override void OnRoundEndForWorker(Worker worker)
    {
        if(worker.parentPlayer.claimedBuildings.Count > 0)
        {
            foreach(Building building in worker.parentPlayer.claimedBuildings)
            {
                building.Decrement();
            }
        }
    }
}
