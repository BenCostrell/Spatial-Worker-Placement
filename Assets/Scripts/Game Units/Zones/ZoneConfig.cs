using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Zone Config")]
public class ZoneConfig : ScriptableObject
{
    [SerializeField]
    private ZoneTypeInfo[] zones;
    public ZoneTypeInfo[] Zones { get { return zones; } }

    [SerializeField]
    private float colorTintProportion;
    public float ColorTintProportion { get { return colorTintProportion; } }

    [SerializeField]
    private Color tooltipColor;
    public Color TooltipColor { get { return tooltipColor; } }

    [SerializeField]
    private float expandTime;
    public float ExpandTime { get { return expandTime; } }

    [SerializeField]
    private int expansionRate;
    public int ExpansionRate { get { return expansionRate; } }

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
