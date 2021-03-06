﻿using UnityEngine;
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

    protected override TaskTree OnRoundEndForWorker(Worker worker)
    {
        TaskTree roundEndTree = new TaskTree(new EmptyTask());
        if (worker.parentPlayer.claimedBuildings.Count > 0)
        {
            foreach(Building building in worker.parentPlayer.claimedBuildings)
            {
                for (int i = 0; i < zoneTypeInfo.EffectMagnitude; i++)
                {
                    roundEndTree.AddChild(new DecrementBuilding(building));
                }
            }
        }
        return roundEndTree;
    }
}
