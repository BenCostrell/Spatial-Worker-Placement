using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Resource : MonoBehaviour {

    private int numResources_;
    [HideInInspector]
    public int numResources
    {
        get { return numResources_; }
        private set
        {
            numResources_ = value;
            counter.text = value.ToString();
        }
    }
    private Tile tile;
    private TextMesh counter;

    // Use this for initialization
    public void Init(int numResources_, Tile tile_)
    {
        counter = GetComponentInChildren<TextMesh>();
        numResources = numResources_;
        tile = tile_;
        transform.position = tile.hex.ScreenPos();
    }

    // Update is called once per frame
    void Update () {
		
	}

    public int GetClaimed(int claimAmount)
    {
        int yield = Mathf.Min(claimAmount, numResources);
        numResources -= yield;
        return yield;
    }

    public void Increment()
    {
        numResources += 1;
    }
}
