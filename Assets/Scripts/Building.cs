using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour {

    public Color defaultColor;
    private SpriteRenderer sr;
    private TextMesh textMesh;
    private Player controller;
    private int turnsLeft_;
    private int turnsLeft
    {
        get { return turnsLeft_; }
        set
        {
            turnsLeft_ = value;
            if (value == 0) textMesh.text = "";
            else textMesh.text = value.ToString();
        }
    }
    private Tile parentTile;

    // Use this for initialization
    public void Init(Tile tile)
    {
        sr = GetComponent<SpriteRenderer>();
        textMesh = GetComponentInChildren<TextMesh>();
        sr.color = defaultColor;
        turnsLeft = 0;
        parentTile = tile;
        transform.position = tile.hex.ScreenPos();
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
        if (controller != null) controller.claimedBuildings.Remove(this);
        controller = player;
        sr.color = player.color;
        turnsLeft = initialTurnsLeft;
        player.claimedBuildings.Add(this);
        Services.main.CheckForWin();
    }

    void ReturnToNeutral()
    {
        controller.claimedBuildings.Remove(this);
        controller = null;
        sr.color = defaultColor;
    }

    public void Decrement()
    {
        if (turnsLeft > 0)
        {
            turnsLeft -= 1;
            if (turnsLeft == 0) ReturnToNeutral();
        }
    }
}
