using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class Item
{
    public GameObject obj { get; private set; }
    private GameObject tooltip;
    public bool tooltipActive { get; private set; }
    private TextMesh costText;
    private Color defaultTextColor;
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
    public bool destroyed { get; private set; }
    public bool hoverInfoActive { get; private set; }

    public Item(Dictionary<StatType, int> statBonuses_, Tile tile)
    {
        statBonuses = statBonuses_;
        obj = GameObject.Instantiate(Services.Prefabs.Item, Services.SceneStackManager.CurrentScene.transform);
        obj.transform.position = tile.hex.ScreenPos() + Services.ItemConfig.Offset;
        parentTile = tile;
        costText = obj.GetComponentInChildren<TextMesh>();
        costText.gameObject.GetComponent<Renderer>().sortingOrder = 4;
        defaultTextColor = costText.color;
        cost = Services.ItemConfig.GetValue(statBonuses) 
            + Services.ItemConfig.StartingPriceBump;
        obj.SetActive(false);
        ItemStatInfo config = Services.ItemConfig.GetItemStatConfig(statBonuses.First().Key);
        obj.GetComponent<SpriteRenderer>().sprite = config.Sprite;
        Services.main.taskManager.AddTask(new ItemSpawnAnimation(this));
    }

    public void GetAcquired()
    {
        DestroyThis();
    }

    public void DecrementCost()
    {
        cost -= 1;
        if (cost <= 0) DestroyThis();
    }

    void DestroyThis()
    {
        GameObject.Destroy(obj);
        Services.MapManager.itemTiles.Remove(parentTile);
        parentTile.containedItem = null;
        parentTile = null;
        destroyed = true;
    }

    public void ShowPotentialPurchasePrice(Worker worker)
    {
        costText.text = Mathf.Max(1, cost - worker.itemDiscount).ToString();
        costText.color = Color.cyan;
        hoverInfoActive = true;
    }

    public void ResetDisplay()
    {
        costText.text = cost.ToString();
        costText.color = defaultTextColor;
        hoverInfoActive = false;
    }

    public void ShowTooltip()
    {
        tooltipActive = true;
        tooltip = GameObject.Instantiate(Services.Prefabs.ItemTooltip, 
            Services.UIManager.canvas);
        RectTransform tooltipRect = tooltip.GetComponent<RectTransform>();
        Vector3 offset = Services.ItemConfig.TooltipOffset;
        if (obj.transform.position.x > 0)
        {
            offset = new Vector3(-offset.x, offset.y, offset.z);
        }
        tooltipRect.anchoredPosition = 
            Services.main.mainCamera.WorldToScreenPoint( 
                obj.transform.position + offset);
        string tooltipText = "";
        foreach (KeyValuePair<Item.StatType, int> bonus in statBonuses)
        {
            tooltipText += Services.ItemConfig.GetItemStatConfig(bonus.Key).Label
                + "\n +" + bonus.Value + "\n";
        }
        tooltip.GetComponentInChildren<Text>().text = tooltipText;
        tooltip.GetComponentInChildren<Image>().color = Services.ItemConfig.TooltipColor;
        Services.main.taskManager.AddTask(new ExpandTooltip(tooltip));
    }

    public void HideTooltip()
    {
        tooltipActive = false;
        GameObject.Destroy(tooltip);
    }
}
