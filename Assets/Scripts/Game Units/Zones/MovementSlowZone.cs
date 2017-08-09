using UnityEngine;
using System.Collections;

public class MovementSlowZone : Zone
{
    public MovementSlowZone(Tile centerTile_) : base(centerTile_)
    {
        type = ZoneType.MovementSlow;
        zoneTypeInfo = Services.ZoneConfig.GetZoneTypeInfo(type);
        sprite = zoneTypeInfo.Sprite;
        centerTile.EnterZone(this);
    }

}
