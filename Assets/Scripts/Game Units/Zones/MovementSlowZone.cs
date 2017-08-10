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

    public override void RemoveZoneEffectFromTile(Tile tile)
    {
        for (int i = 0; i < Services.GameManager.numPlayers; i++)
        {
            tile.movementCostPerPlayer[i] = 1;
        }
    }

    public override void ApplyZoneEffectToTile(Tile tile)
    {
        if (controller != null)
        {
            for (int i = 0; i < Services.GameManager.numPlayers; i++)
            {
                if (i != controller.playerNum - 1)
                {
                    tile.movementCostPerPlayer[i] = 2;
                }
            }
        }
    }
}
