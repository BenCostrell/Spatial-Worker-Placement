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

    [HideInInspector]
    public bool movedThisRound;
    [HideInInspector]
    public bool selected;

    // Use this for initialization
    public void Init (Player parent, Tile tile) {
        parentPlayer = parent;
        taskManager = new TaskManager();
        sr = GetComponent<SpriteRenderer>();
        sr.color = parentPlayer.color;
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

    public void AnimateMovementAlongPath(List<Tile> path)
    {
        TaskQueue movementTasks = new TaskQueue();

        for (int i = path.Count - 1; i >= 0; i--)
        {
            movementTasks.Add(new AnimateWorkerMovement(this, path[i], tileHopTime));
        }

        taskManager.AddTaskQueue(movementTasks);
    }

    public void EndTurn()
    {
        movedThisRound = true;
        sr.color = (parentPlayer.color + Color.gray) / 2;
        Services.main.TurnEnd();
    }

    public void Refresh()
    {
        movedThisRound = false;
        sr.color = parentPlayer.color;
    }

    public void Select()
    {
        selected = true;
        sr.color = (parentPlayer.color + Color.white) / 2;
        Services.EventManager.Register<ButtonPressed>(OnButtonPressed);
    }

    public void Unselect()
    {
        selected = false;
        if (movedThisRound) sr.color = (parentPlayer.color + Color.gray) / 2;
        else sr.color = parentPlayer.color;
    }

    void OnButtonPressed(ButtonPressed e)
    {
        if (e.playerNum == Services.main.currentActivePlayer.playerNum && e.button == "B" && selected)
        {
            Unselect();
            EndTurn();
        }
    }
}
