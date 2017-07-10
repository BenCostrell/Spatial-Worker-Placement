using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{

    private Tile hoveredTile;
    private Tile lastHoveredTile;
    private List<Tile> tilePath;
    private int inputLastFrame;
    public float timeToWaitBeforeRepeatingInput;
    private float timeSinceLastUniqueInput;
    private Worker selectedWorker;

    // Use this for initialization
    void Start()
    {
        timeSinceLastUniqueInput = 0;
        Services.EventManager.Register<ButtonPressed>(OnButtonPressed);
        tilePath = new List<Tile>();
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
        if (selectedWorker != null && hoveredTile != lastHoveredTile) HighlightPath(selectedWorker.currentTile, hoveredTile);
        lastHoveredTile = hoveredTile;
    }

    public void PlaceOnTile(Tile tile)
    {
        transform.position = tile.hex.ScreenPos();
        hoveredTile = tile;
    }

    void ProcessInput()
    {
        float xInput = Input.GetAxis("Horizontal_P" + Services.main.currentActivePlayer.playerNum);
        float yInput = Input.GetAxis("Vertical_P" + Services.main.currentActivePlayer.playerNum);
        float mag = new Vector2(xInput, yInput).magnitude;
        if (mag < 0.1f) inputLastFrame = -1;
        else 
        {
            float angle = Mathf.Atan2(yInput, xInput) * Mathf.Rad2Deg;
            float convertedAngle = (angle + 330) % 360;
            int directionIndex = 5 - Mathf.FloorToInt(convertedAngle / 60f);
            if (directionIndex != inputLastFrame || timeSinceLastUniqueInput > timeToWaitBeforeRepeatingInput)
            {
                Tile tile;
                if (Services.MapManager.map.TryGetValue(hoveredTile.hex.Neighbor(directionIndex), out tile))
                {
                    PlaceOnTile(tile);
                }
                timeSinceLastUniqueInput = 0;
            }
            else timeSinceLastUniqueInput += Time.deltaTime;
            inputLastFrame = directionIndex;
        }
    }

    void OnButtonPressed (ButtonPressed e)
    {
        if (e.playerNum == Services.main.currentActivePlayer.playerNum && e.button == "A") SelectTile();
    }

    void SelectTile()
    {
        if (selectedWorker != null && hoveredTile.containedWorker == null)
        {
            //selectedWorker.PlaceOnTile(hoveredTile);
            selectedWorker.AnimateMovementAlongPath(AStarSearch.ShortestPath(selectedWorker.currentTile, hoveredTile));
            selectedWorker = null;
            ClearPath();
        }
        else {
            if (hoveredTile.containedWorker != null)
            {
                selectedWorker = hoveredTile.containedWorker;
            }
        }
    }

    void HighlightPath(Tile start, Tile goal)
    {
        ClearPath();
        tilePath = AStarSearch.ShortestPath(start, goal);
        foreach(Tile tile in tilePath)
        {
            tile.obj.GetComponent<SpriteRenderer>().color = Color.gray;
        }
    }

    void ClearPath()
    {
        if (tilePath.Count > 0)
        {
            foreach (Tile tile in tilePath)
            {
                tile.obj.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
        tilePath = new List<Tile>();
    }
}