using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour {

    public Color defaultColor;
    private SpriteRenderer sr;
    private TextMesh textMesh;
    private Player controller;
    private int turnsLeft;

    // Use this for initialization
    public void Init()
    {
        sr = GetComponent<SpriteRenderer>();
        textMesh = GetComponentInChildren<TextMesh>();
        sr.color = defaultColor;
        textMesh.text = "";
    }

    public void GetClaimed(Player player, int claimAmount)
    {
        if (controller == null)
        {
            controller = player;
            turnsLeft = claimAmount;
            sr.color = player.color;
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
            controller = player;
            turnsLeft = claimAmount - turnsLeft;
            sr.color = player.color;
        }
        else
        {
            ReturnToNeutral();
            turnsLeft = 0;
        }
    }

    void ReturnToNeutral()
    {
        controller = null;
        sr.color = defaultColor;
    }

    public void Decrement()
    {
        turnsLeft -= 1;
        if (turnsLeft == 0) ReturnToNeutral();
    }
}
