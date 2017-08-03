using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : Scene<TransitionData> {


    [HideInInspector]
    public Player currentActivePlayer { get; private set; }
    [HideInInspector]
    public int roundNum { get; private set; }
    [HideInInspector]
    public List<Building> buildings;
    public int numBuildingClaimsToWin;
    public float resGainAnimOffset;
    public float resGainAnimDur;
    public float resGainAnimDist;
    [HideInInspector]
    public TaskManager taskManager { get; private set; }
    [HideInInspector]
    public Camera mainCamera { get; private set; }

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        taskManager.Update();
	}

    internal override void Init()
    {
        InitializeMainServices();
        taskManager = new TaskManager();
        Services.MapManager.CreateHexGrid();
        PlaceInitialWorkers();
        roundNum = 0;
        Services.UIManager.canvas = GetComponentInChildren<Canvas>().transform;
        mainCamera = GetComponentInChildren<Camera>();
        Services.UIManager.InitUI();
        Services.EventManager.Register<ButtonPressed>(OnButtonPressed);
        StartRound();
    }

    void InitializeMainServices()
    {
        Services.MapManager = Services.GameManager.sceneRoot.GetComponentInChildren<MapManager>();
        Services.main = this;
        Services.UIManager = Services.GameManager.sceneRoot.GetComponentInChildren<UIManager>();
    }

    void PlaceInitialWorkers()
    {
        for (int i = 0; i < Services.GameManager.players.Count; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Vector3 spawnPoint = Services.GameManager.workerSpawns[i+(j*2)];
                Services.GameManager.players[i].AddWorker(
                    Services.MapManager.map[new Hex(
                        Mathf.RoundToInt(spawnPoint.x),
                        Mathf.RoundToInt(spawnPoint.y),
                        Mathf.RoundToInt(spawnPoint.z))]);
            }  
        }
    }



    void StartRound()
    {
        roundNum += 1;
        currentActivePlayer = Services.GameManager.players[(roundNum - 1) % Services.GameManager.numPlayers];
        foreach (Player player in Services.GameManager.players) player.Refresh();
        Services.UIManager.UpdateUI();
    }

    void EndRound()
    {
        DecrementBuildings();
        IncrementResources();
        DecrementItemCosts();
        Services.MapManager.SpawnNewItems(2, 5);
        Services.MapManager.SpawnNewResources();
        StartRound();
    }

    void IncrementResources()
    {
        foreach (Tile tile in Services.MapManager.resourceTiles)
        {
            if (tile.containedWorker == null) tile.containedResource.Increment();
        }
    }

    void DecrementItemCosts()
    {
        for (int i = Services.MapManager.itemTiles.Count -1; i >= 0; i--)
        {
            Services.MapManager.itemTiles[i].containedItem.DecrementCost();
        }
    }

    void DecrementBuildings()
    {
        foreach(Tile tile in Services.MapManager.buildingTiles)
        {
            tile.containedBuilding.Decrement();
        }
    }

    public void EndTurn()
    {
        WaitForAnimations waitForAnimations = new WaitForAnimations();
        waitForAnimations.Then(new ActionTask(ActuallyEndTurn));
        taskManager.AddTask(waitForAnimations);
    }

    void ActuallyEndTurn()
    {
        Services.UIManager.selector.Reset();
        currentActivePlayer.workerMovedThisTurn.EndTurn();
        currentActivePlayer.workerMovedThisTurn = null;
        Player nextPlayer = DetermineNextPlayer();
        if (nextPlayer == null) EndRound();
        else
        {
            currentActivePlayer = nextPlayer;
            Services.UIManager.UpdateUI();
        }
    }

    Player DetermineNextPlayer()
    {
        Player nextPlayer = null;
        Player[] orderedCandidateNextPlayers = new Player[Services.GameManager.numPlayers];
        int playerIndex = currentActivePlayer.playerNum % Services.GameManager.numPlayers;
        for (int i = 0; i < Services.GameManager.numPlayers; i++)
        {
            orderedCandidateNextPlayers[i] = Services.GameManager.players[playerIndex];
            playerIndex = (playerIndex + 1) % Services.GameManager.numPlayers;
        }
        for (int i = 0; i < orderedCandidateNextPlayers.Length; i++)
        {
            bool hasWorkersRemaining = false;
            foreach (Worker worker in orderedCandidateNextPlayers[i].workers)
                if (!worker.movedThisRound) hasWorkersRemaining = true;
            if (hasWorkersRemaining)
            {
                nextPlayer = orderedCandidateNextPlayers[i];
                break;
            }
        }
        return nextPlayer;
    }

    

    void OnButtonPressed(ButtonPressed e)
    {
        if (e.playerNum == currentActivePlayer.playerNum){
            if ((currentActivePlayer.workerMovedThisTurn != null) && e.button == "Y")
            {
                EndTurn();
            }
        }
    }

    public void CheckForWin()
    {
        foreach(Player player in Services.GameManager.players)
        {
            if (player.claimedBuildings.Count >= numBuildingClaimsToWin)
            {
                GameWin(player);
            }
        }
    }

    void GameWin(Player player)
    {
        Debug.Log("player " + player.playerNum + " has won");
    }
}
