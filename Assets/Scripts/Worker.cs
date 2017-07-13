using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : MonoBehaviour {

    [HideInInspector]
    public Player parentPlayer;
    [HideInInspector]
    public Tile currentTile;
    public float tileHopTime;
    private TaskManager taskManager;
    private SpriteRenderer sr;
    public int defaultStartingMaxMovement;
    private int maxMovementPerTurn;
    private int movesRemaining;

    [HideInInspector]
    public bool movedThisRound;
    [HideInInspector]
    public bool movedThisTurn;
    [HideInInspector]
    public bool selected;

    // Use this for initialization
    public void Init (Player parent, Tile tile) {
        parentPlayer = parent;
        taskManager = new TaskManager();
        sr = GetComponent<SpriteRenderer>();
        sr.color = parentPlayer.color;
        maxMovementPerTurn = defaultStartingMaxMovement;
        movesRemaining = maxMovementPerTurn;
        PlaceOnTile(tile);
    }

    // Update is called once per frame
    void Update () {
        taskManager.Update();
	}

    public void PlaceOnTile(Tile tile)
    {
        if (currentTile != null) currentTile.containedWorker = null;
        currentTile = tile;
        tile.containedWorker = this;
        transform.position = tile.hex.ScreenPos();
    }

    void AnimateMovementAlongPath(List<Tile> path)
    {
        TaskQueue movementTasks = new TaskQueue();

        for (int i = path.Count - 1; i >= 0; i--)
        {
            movementTasks.Add(new AnimateWorkerMovement(this, path[i], tileHopTime));
        }

        taskManager.AddTaskQueue(movementTasks);
    }

    void InitiateMovement(List<Tile> path)
    {
        movesRemaining -= (path.Count - 1);
        AnimateMovementAlongPath(path);
        if (path.Count > 0)
        {
            movedThisTurn = true;
            parentPlayer.movedWorkerThisTurn = true;
        }
    }

    public bool TryToMove(Tile goal)
    {
        List<Tile> path = AStarSearch.ShortestPath(currentTile, goal);
        if (CanMoveAlongPath(path)) {
            InitiateMovement(path);
            return true;
        }
        else return false;
    }

    public bool CanMoveAlongPath(List<Tile> path)
    {
        return ((path.Count - 1) <= movesRemaining);
    }

    public void EndTurn()
    {
        movedThisRound = true;
        movedThisTurn = false;
        sr.color = (parentPlayer.color + Color.gray) / 2;
        Services.main.TurnEnd();
    }

    public void Refresh()
    {
        movedThisRound = false;
        movesRemaining = maxMovementPerTurn;
        sr.color = parentPlayer.color;
    }

    public void Select()
    {
        selected = true;
        sr.color = (parentPlayer.color + Color.white) / 2;
        Services.EventManager.Register<ButtonPressed>(OnButtonPressed);
        Services.main.SetWorkerTooltip(movesRemaining, maxMovementPerTurn);
    }

    public void Unselect()
    {
        selected = false;
        if (movedThisRound) sr.color = (parentPlayer.color + Color.gray) / 2;
        else sr.color = parentPlayer.color;
        Services.EventManager.Unregister<ButtonPressed>(OnButtonPressed);
        Services.main.HideWorkerTooltip();
    }

    void OnButtonPressed(ButtonPressed e)
    {
        if (e.playerNum == Services.main.currentActivePlayer.playerNum && e.button == "B" && selected)
        {
            Unselect();
            EndTurn();
        }
    }

    public void ShowToolTip()
    {
        Services.main.SetWorkerTooltip(movesRemaining, maxMovementPerTurn);
    }

    public void HideTooltip()
    {
        Services.main.HideWorkerTooltip();
    }
}
