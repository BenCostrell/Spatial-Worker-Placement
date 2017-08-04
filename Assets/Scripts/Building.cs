﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Building : MonoBehaviour {

    public Color defaultColor;
    public Vector2 offset;
    private SpriteRenderer sr;
    private TextMesh textMesh;
    public Player controller { get; private set; }
    public bool permanentlyControlled { get; private set; }
    public int permanentControlThreshold;
    public float permanentControlScaleIncrease;
    private List<int> playerInfluence;
    private int turnsLeft_;
    public int turnsLeft
    {
        get { return turnsLeft_; }
        private set
        {
            turnsLeft_ = value;
            if (value == 0) textMesh.text = "";
            else textMesh.text = value.ToString();
        }
    }
    private Tile parentTile;
    public Dictionary<Item.StatType, int> statBonuses { get; private set; }

    // Use this for initialization
    public void Init(Tile tile, Dictionary<Item.StatType, int> statBonuses_)
    {
        sr = GetComponent<SpriteRenderer>();
        textMesh = GetComponentInChildren<TextMesh>();
        textMesh.gameObject.GetComponent<Renderer>().sortingOrder = 4;
        sr.color = defaultColor;
        turnsLeft = 0;
        permanentlyControlled = false;
        parentTile = tile;
        transform.position = tile.hex.ScreenPos() + offset;
        statBonuses = statBonuses_;
        Item.StatType bonus = statBonuses.First().Key;
        GetComponentsInChildren<SpriteRenderer>()[1].sprite =
            Services.ItemConfig.GetItemStatConfig(bonus).Sprite;
        playerInfluence = new List<int>();
        for (int i = 0; i < Services.GameManager.numPlayers; i++) playerInfluence.Add(0);
    }

    public void GetClaimed(Player player, int claimAmount)
    {
        if (claimAmount > 0)
        {
            if (controller == null)
            {
                SuccessfulClaim(player, claimAmount);
            }
            else if (controller == player)
            {
                turnsLeft += claimAmount;
            }
            else if (claimAmount < turnsLeft)
            {
                turnsLeft -= claimAmount;
            }
            else if (claimAmount > turnsLeft)
            {
                SuccessfulClaim(player, claimAmount - turnsLeft);
            }
            else
            {
                ReturnToNeutral();
                turnsLeft = 0;
            }
        }
    }

    void SuccessfulClaim(Player player, int initialTurnsLeft)
    {
        if (controller != null) LoseControl();
        controller = player;
        sr.color = player.color;
        turnsLeft = initialTurnsLeft;
        player.claimedBuildings.Add(this);
        foreach(Worker worker in player.workers) worker.GetTempBonuses(statBonuses);
        Services.main.CheckForWin();
    }

    void LoseControl()
    {
        controller.claimedBuildings.Remove(this);
        foreach (Worker worker in controller.workers) worker.LoseTempBonuses(statBonuses);
    }

    void ReturnToNeutral()
    {
        LoseControl();
        controller = null;
        sr.color = defaultColor;
    }

    public void Decrement()
    {
        if (turnsLeft > 0)
        {
            turnsLeft -= 1;
            playerInfluence[controller.playerNum - 1] += 1;
            if (playerInfluence[controller.playerNum - 1] >= permanentControlThreshold)
            {
                GainPermanentControl();
            }
            else if (turnsLeft == 0) ReturnToNeutral();
        }
    }

    void GainPermanentControl()
    {
        permanentlyControlled = true;
        turnsLeft = 0;
        transform.localScale *= permanentControlScaleIncrease;
    }
}
