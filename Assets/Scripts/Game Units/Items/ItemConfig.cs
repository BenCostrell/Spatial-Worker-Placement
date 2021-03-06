﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Item Config")]
public class ItemConfig : ScriptableObject
{
    [SerializeField]
    private ItemStatInfo[] items;
    public ItemStatInfo[] Items { get { return items; } }

    [SerializeField]
    private float spawnGrowTime;
    public float SpawnGrowTime { get { return spawnGrowTime; } }

    [SerializeField]
    private Vector2 offset;
    public Vector2 Offset { get { return offset; } }

    [SerializeField]
    private int startingPriceBump;
    public int StartingPriceBump { get { return startingPriceBump; } }

    [SerializeField]
    private int discountDecrementRate;
    public int DiscountDecrementRate { get { return discountDecrementRate; } }

    [SerializeField]
    private float decrementAnimTime;
    public float DecrementAnimTime { get { return decrementAnimTime; } }

    [SerializeField]
    private float decrementScale;
    public float DecrementScale { get { return decrementScale; } }

    [SerializeField]
    private float decrementDist;
    public float DecrementDist { get { return decrementDist; } }

    [SerializeField]
    private Vector3 tooltipOffset;
    public Vector3 TooltipOffset { get { return tooltipOffset; } }

    [SerializeField]
    private Color tooltipColor;
    public Color TooltipColor { get { return tooltipColor; } }

    [SerializeField]
    private float acquireAnimTime;
    public float AcquireAnimTime { get { return acquireAnimTime; } }

    [SerializeField]
    private Vector3 acquireOffset;
    public Vector3 AcquireOffset { get { return acquireOffset; } }

    [SerializeField]
    private float floatNoiseMag;
    public float FloatNoiseMag { get { return floatNoiseMag; } }

    [SerializeField]
    private float floatNoiseSpeed;
    public float FloatNoiseSpeed { get { return floatNoiseSpeed; } }

    [SerializeField]
    private float floatCycleDist;
    public float FloatCycleDist { get { return floatCycleDist; } }

    [SerializeField]
    private float floatCyclePeriod;
    public float FloatCyclePeriod { get { return floatCyclePeriod; } }

    public ItemStatInfo GetItemStatConfig(Item.StatType statType)
    {
        foreach(ItemStatInfo itemStatConfig in items)
        {
            if (itemStatConfig.StatType == statType) return itemStatConfig;
        }
        Debug.Assert(false); //should never get here if items are properly configured
        return null;
    }

    public int GetValue(Dictionary<Item.StatType, int> bonuses)
    {
        int value = 0;
        foreach (KeyValuePair<Item.StatType, int> bonus in bonuses)
        {
            value += GetItemStatConfig(bonus.Key).Cost * bonus.Value;
        }
        return value;
    }
}
