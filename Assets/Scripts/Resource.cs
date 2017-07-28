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
    private float moveCycleTimeElapsed;
    public float moveCyclePeriod;
    public float moveCycleDist;
    private Vector3 basePosition;
    private bool goingUp;
    public Vector2 offset;
    public float spawnGrowTime;
    private TaskManager taskManager;
    [HideInInspector]
    public SpriteRenderer sr;

    // Use this for initialization
    public void Init(int numResources_, Tile tile_)
    {
        counter = GetComponentInChildren<TextMesh>();
        counter.gameObject.GetComponent<Renderer>().sortingOrder = 4;
        numResources = numResources_;
        tile = tile_;
        transform.position = tile.hex.ScreenPos() + offset;
        basePosition = transform.position;
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
        taskManager = new TaskManager();
        taskManager.AddTask(new ResourceSpawnAnimation(this));
    }

    // Update is called once per frame
    void Update () {
        Float();
        taskManager.Update();
	}

    void Float()
    {
        if (goingUp) moveCycleTimeElapsed += Time.deltaTime;
        else moveCycleTimeElapsed -= Time.deltaTime;

        transform.position = Vector3.Lerp(basePosition, 
            basePosition + moveCycleDist * Vector3.up,
            Easing.QuadEaseOut(moveCycleTimeElapsed / moveCyclePeriod));
        if (moveCycleTimeElapsed >= moveCyclePeriod && goingUp)
        {
            goingUp = false;
        }
        if (moveCycleTimeElapsed <= 0 && !goingUp)
        {
            goingUp = true;
        }
    }

    public int GetClaimed(int claimAmount)
    {
        int yield = Mathf.Min(claimAmount, numResources);
        if (yield > 0)
        {
            numResources -= yield;
            if (numResources == 0) DestroyThis();
        }
        return yield;
    }

    public void Increment()
    {
        numResources += 1;
    }

    void DestroyThis()
    {
        Destroy(gameObject);
        Services.MapManager.resourceTiles.Remove(tile);
    }
}
