using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Item
{
    private GameObject obj;
    private readonly Vector2 offset = 0.3f * Vector2.up;
    private TextMesh costText;
    private Tile parentTile;
    public enum StatType { MovementSpeed, CarryingCapacity, ExtraResourcePickup, ItemDiscount, BonusClaimPower,
        BumpPower }
    public readonly Dictionary<StatType, int> statBonuses;
    private int cost_;
    public int cost
    {
        get { return cost_; }
        private set
        {
            cost_ = value;
            costText.text = value.ToString();
        }
    }
    public static readonly Dictionary<StatType, int> costPerStat = new Dictionary<StatType, int>()
    {
        { StatType.MovementSpeed, 5 },
        { StatType.CarryingCapacity, 2 },
        { StatType.ExtraResourcePickup, 3 },
        { StatType.ItemDiscount, 2 },
        { StatType.BonusClaimPower, 3 },
        { StatType.BumpPower, 2 }
    };
    public static readonly List<StatType> statTypes = new List<StatType>(costPerStat.Keys);
    public const int startingPriceBump = 2;

    public Item(Dictionary<StatType, int> statBonuses_, Tile tile)
    {
        statBonuses = statBonuses_;
        obj = GameObject.Instantiate(Services.Prefabs.Item, Services.SceneStackManager.CurrentScene.transform);
        obj.transform.position = tile.hex.ScreenPos() + offset;
        parentTile = tile;
        costText = obj.GetComponentInChildren<TextMesh>();
        costText.gameObject.GetComponent<Renderer>().sortingOrder = 4;
        cost = GetValue(statBonuses) + startingPriceBump;
    }

    public static int GetValue(Dictionary<StatType, int> bonuses)
    {
        int value = 0;
        foreach(KeyValuePair<StatType, int> bonus in bonuses)
        {
            value += costPerStat[bonus.Key] * bonus.Value;
        }
        return value;
    }

    public static string StatTypeToString(StatType statType)
    {
        switch (statType)
        {
            case StatType.MovementSpeed:
                return "Movement Speed";
            case StatType.CarryingCapacity:
                return "Carrying Capacity";
            case StatType.ExtraResourcePickup:
                return "Extra Resource Pickup";
            case StatType.ItemDiscount:
                return "Item Discount";
            case StatType.BonusClaimPower:
                return "Bonus Claim Power";
            case StatType.BumpPower:
                return "Bump Power";
            default:
                return "";
        }
    }

    public void GetAcquired()
    {
        obj.SetActive(false);
        Services.MapManager.itemTiles.Remove(parentTile);
        parentTile.containedItem = null;
        parentTile = null;
    }

    public void DecrementCost()
    {
        if (cost > 1) cost -= 1;
    }
}
