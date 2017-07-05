﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {

    public int playerNum;

    public Player(int playerNum_)
    {
        playerNum = playerNum_;
        color = Services.GameManager.playerColors[playerNum - 1];
        workers = new List<Worker>();
    }

    public Color color;
    public List<Worker> workers;

    public void AddWorker(Tile tile)
    {
        Worker worker = GameObject.Instantiate(Services.Prefabs.Worker, 
            Services.SceneStackManager.CurrentScene.transform).GetComponent<Worker>();
        worker.PlaceOnTile(tile);
        worker.GetComponent<SpriteRenderer>().color = color;
        workers.Add(worker);
    }
}
