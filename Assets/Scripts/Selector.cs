using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{

    private Tile hoveredTile;
    private List<Tile> tilePath;
    private int inputLastFrame;
    public float timeToWaitBeforeRepeatingInput;
    private float timeSinceLastUniqueInput;
    private Worker hoveredWorker;
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
        if (mag > 0.1f)
        {
            MoveSelector(xInput, yInput);
            if (selectedWorker != null) HighlightPath(selectedWorker.currentTile, hoveredTile);
            ShowAppropriateTooltip();
        }
        else inputLastFrame = -1;
    }

    void MoveSelector(float xInput, float yInput)
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

    void ShowAppropriateTooltip()
    {
        if (hoveredTile.containedWorker != null && selectedWorker == null)
        {
            if (hoveredWorker != hoveredTile.containedWorker)
            {
                if (hoveredWorker != null) hoveredWorker.HideTooltip();
                hoveredWorker = hoveredTile.containedWorker;
                hoveredWorker.ShowToolTip();
            }
        }
        else if (hoveredWorker != null)
        {
            hoveredWorker.HideTooltip();
            hoveredWorker = null;
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
            selectedWorker.TryToMove(hoveredTile);
            UnselectWorker();
            ClearPath();
            
        }
        else {
            if (hoveredTile.containedWorker != null)
            {
                Worker worker = hoveredTile.containedWorker;
                if (!worker.movedThisRound && 
                    worker.parentPlayer == Services.main.currentActivePlayer &&
                    (!worker.parentPlayer.movedWorkerThisTurn || worker.movedThisTurn))
                {
                    if (selectedWorker != null) UnselectWorker();
                    SelectWorker(worker);
                }
            }
        }
    }

    void SelectWorker(Worker worker)
    {
        worker.Select();
        selectedWorker = worker;
    }

    void UnselectWorker()
    {
        selectedWorker.Unselect();
        selectedWorker = null;
    }

    void HighlightPath(Tile start, Tile goal)
    {
        List<Tile> newPath = AStarSearch.ShortestPath(start, goal);
        if (selectedWorker.CanMoveAlongPath(newPath))
        {
            ClearPath();
            tilePath = newPath;
            foreach (Tile tile in tilePath)
            {
                tile.obj.GetComponent<SpriteRenderer>().color = Color.gray;
            }
        }
    }

    public void Reset()
    {
        ClearPath();
        if (selectedWorker != null) UnselectWorker();
    }

    public void ClearPath()
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

    public void SetColor()
    {
        GetComponent<SpriteRenderer>().color = Services.main.currentActivePlayer.color;
    }
}