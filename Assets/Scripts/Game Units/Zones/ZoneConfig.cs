using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Zone Config")]
public class ZoneConfig : ScriptableObject
{
    [SerializeField]
    private ZoneTypeInfo[] zones;
    public ZoneTypeInfo[] Zones { get { return zones; } }
    public ZoneTypeInfo GetZoneTypeInfo(Zone.ZoneType zoneType)
    {
        foreach (ZoneTypeInfo zoneTypeInfo in zones)
        {
            if (zoneTypeInfo.Type == zoneType) return zoneTypeInfo;
        }
        Debug.Assert(false); //should never get here if zones are properly configured
        return null;
    }
}
