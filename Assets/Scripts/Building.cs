using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour {

    public Color defaultColor;
    public Vector2 offset;
    private SpriteRenderer sr;
    private TextMesh textMesh;
    private Player controller_;
    public Player controller
    {
        get { return controller_; }
        private set { controller_ = value; }
    }
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
    private Dictionary<Item.StatType, int> statBonuses_;
    public Dictionary<Item.StatType, int> statBonuses
    {
        get { return statBonuses_; }
        private set { statBonuses_ = value; }
    }

    // Use this for initialization
    public void Init(Tile tile, Dictionary<Item.StatType, int> statBonuses__)
    {
        sr = GetComponent<SpriteRenderer>();
        textMesh = GetComponentInChildren<TextMesh>();
        textMesh.gameObject.GetComponent<Renderer>().sortingOrder = 4;
        sr.color = defaultColor;
        turnsLeft = 0;
        parentTile = tile;
        transform.position = tile.hex.ScreenPos() + offset;
        statBonuses = statBonuses__;
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
            if (turnsLeft == 0) ReturnToNeutral();
        }
    }
}
