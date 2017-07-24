using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Tile
{
    public readonly Hex hex;
    public GameObject obj;
    public List<Tile> neighbors;
    public Worker containedWorker;
    public Resource containedResource;
    public Building containedBuilding;
    public Item containedItem;
    public readonly Color moveAvailableColor = new Color(0.5f, 1f, 0.5f);

    public Tile(Hex hex_)
    {
        hex = hex_;
        obj = GameObject.Instantiate(Services.Prefabs.Tile, hex.ScreenPos(Services.MapManager.layout), Quaternion.identity,
            Services.SceneStackManager.CurrentScene.transform);
        neighbors = new List<Tile>();
    }

    public string TooltipText()
    {
        string toolTipText = "";
        //if (containedWorker != null)
        //{
        //    toolTipText += "contains worker from player number " + containedWorker.parentPlayer.playerNum;
        //}
        if (containedResource != null)
        {
            toolTipText += "Tile Resources: " + containedResource.numResources;
        }
        else if (containedItem != null)
        {
            toolTipText += "Item - Cost : " + containedItem.cost;
            foreach (KeyValuePair<Item.StatType, int> bonus in containedItem.statBonuses)
            {
                toolTipText += "\n" + Item.StatTypeToString(bonus.Key) + " +" + bonus.Value;
            }
        }
        else if (containedBuilding != null)
        {
            toolTipText += "Building : \n";
            if (containedBuilding.controller == null)
            {
                toolTipText += "Neutral \n";
            }
            else
            {
                toolTipText += "Controlled by " + containedBuilding.controller.name + "\n" +
                    containedBuilding.turnsLeft + " rounds left \n";
            }
            toolTipText += "Bonuses to all controller's workers: ";
            foreach (KeyValuePair<Item.StatType, int> bonus in containedBuilding.statBonuses)
            {
                toolTipText += "\n" + Item.StatTypeToString(bonus.Key) + " +" + bonus.Value;
            }
        }
        return toolTipText;
    }
}
