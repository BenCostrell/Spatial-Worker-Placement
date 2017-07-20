using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {

    public readonly int playerNum;
    public readonly Color color;
    public readonly string name;
    public List<Worker> workers;
    public Worker workerMovedThisTurn;
    public List<Building> claimedBuildings;

    public Player(int playerNum_)
    {
        playerNum = playerNum_;
        color = Services.GameManager.playerColors[playerNum - 1];
        name = Services.GameManager.playerNames[playerNum - 1];
        workers = new List<Worker>();
        claimedBuildings = new List<Building>();
        workerMovedThisTurn = null;
    }

    public void AddWorker(Tile tile)
    {
        Worker worker = GameObject.Instantiate(Services.Prefabs.Worker, 
            Services.SceneStackManager.CurrentScene.transform).GetComponent<Worker>();
        worker.Init(this, tile);
        workers.Add(worker);
    }

    public void Refresh()
    {
        foreach (Worker worker in workers) worker.Refresh();
        workerMovedThisTurn = null;
    }
}
