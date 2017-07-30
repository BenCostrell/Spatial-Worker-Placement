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
    private static int nextId_;
    private static int nextId
    {
        get {
            nextId_ += 1;
            return nextId_;
        }
    }
    private int id;
    public float noiseSpeed;
    public float noiseMag;
    public float acquireAnimDur;
    public float acquireAnimOffsetY;
    [HideInInspector]
    public bool animating;

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
        id = nextId;
    }

    // Update is called once per frame
    void Update () {
        if (!animating) Float();
        taskManager.Update();
	}

    void Float()
    {
        if (goingUp) moveCycleTimeElapsed += Time.deltaTime;
        else moveCycleTimeElapsed -= Time.deltaTime;
        float horizontalOffset = -noiseMag/2 + (noiseMag * 
            Mathf.PerlinNoise(Time.time * noiseSpeed, 1000f * id));
        float verticalOffset = /*-noiseMag / 2 + (noiseMag *
            Mathf.PerlinNoise(Time.time * noiseSpeed, 5050f + 1000f * id));*/
            Mathf.Lerp(0, moveCycleDist,
            Easing.QuadEaseOut(moveCycleTimeElapsed / moveCyclePeriod));

        transform.position = new Vector3(
            basePosition.x + horizontalOffset, 
            basePosition.y + verticalOffset, 
            transform.position.z);

        if (moveCycleTimeElapsed >= moveCyclePeriod && goingUp)
        {
            goingUp = false;
        }
        if (moveCycleTimeElapsed <= 0 && !goingUp)
        {
            goingUp = true;
        }
    }

    public int GetClaimed(int claimAmount, Worker worker)
    {
        int yield = Mathf.Min(claimAmount, numResources);
        if (yield > 0)
        {
            numResources -= yield;
            if (numResources == 0)
            {
                numResources = yield; // just so the text is correct during the animation
                GetFullyAcquired(worker);
            }
        }
        return yield;
    }

    public void Increment()
    {
        numResources += 1;
    }

    void GetFullyAcquired(Worker worker)
    { 
        TaskQueue animationSequence = new TaskQueue(new List<Task>() {
            new GetResourceAnimation(this),
            new ActionTask(DestroyThis),
            new ResourceAcquisitionAnimation(worker)
            });
        Services.main.taskManager.AddTaskQueue(animationSequence);
    }

    void DestroyThis()
    {
        Destroy(gameObject);
        Services.MapManager.resourceTiles.Remove(tile);
    }
}
