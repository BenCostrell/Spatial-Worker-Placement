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
    private bool turnEnding;
    public bool gameOver { get; private set; }
    public int activeAnimations;

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
        activeAnimations = 0;
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
        TaskTree roundStartTasks = new TaskTree(new WaitTask(0.5f));
        roundStartTasks
            .Then(new ScrollTurnBanner(currentActivePlayer));
        taskManager.AddTask(roundStartTasks);
    }

    void EndRound()
    {
        TaskTree roundEndTasks = new TaskTree(new EmptyTask());
        roundEndTasks
            .Then(new ScrollTurnBanner(null))
            .Then(DecrementBuildings())
            .Then(IncrementResources())
            .Then(DecrementItemCosts())
            .Then(ApplyZoneEffects())
            .Then(IncrementZones())
            .Then(DecrementClaimedZones())
            .Then(new ActionTask(TempSpawnNewItems))
            .Then(new ActionTask(Services.MapManager.SpawnNewResources))
            .Then(new ActionTask(Services.MapManager.SpawnNewZones))
            .Then(new ActionTask(StartRound));
        taskManager.AddTask(roundEndTasks);
    }

    TaskTree IncrementZones()
    {
        TaskTree incrementAllZones = new TaskTree(new EmptyTask());
        if (Services.MapManager.currentActiveZones.Count > 0)
        {
            foreach (Zone zone in Services.MapManager.currentActiveZones)
            {
                if (zone.controller == null)
                    incrementAllZones.AddChild(zone.Expand(Services.ZoneConfig.ExpansionRate));
            }
        }
        return incrementAllZones;
    }

    TaskTree DecrementClaimedZones()
    {
        TaskTree decrementAllClaimedZones = new TaskTree(new EmptyTask());
        if (Services.MapManager.currentActiveZones.Count > 0)
        {
            for (int i = Services.MapManager.currentActiveZones.Count - 1; i >= 0; i--)
            {
                Zone zone = Services.MapManager.currentActiveZones[i];
                if (zone.controller != null)
                    decrementAllClaimedZones.AddChild(zone.Decrement());
            }
        }
        return decrementAllClaimedZones;
    }

    TaskTree ApplyZoneEffects()
    {
        TaskTree zoneEffects = new TaskTree(new EmptyTask());
        if (Services.MapManager.currentActiveZones.Count > 0)
        {
            foreach(Zone zone in Services.MapManager.currentActiveZones)
            {
                if (zone.controller != null) zoneEffects.AddChild(zone.OnRoundEnd());
            }
        }
        return zoneEffects;
    }

    void TempSpawnNewItems()
    {
        Services.MapManager.SpawnNewItems(2, 5);
    }

    TaskTree IncrementResources()
    {
        TaskTree incrementEachResource = new TaskTree(new EmptyTask());
        if (Services.MapManager.resourceTiles.Count > 0)
        {
            foreach (Tile tile in Services.MapManager.resourceTiles)
            {
                if (tile.containedWorker == null)
                {
                    incrementEachResource.AddChild(new IncrementResource(tile.containedResource));
                }
            }
        }
        return incrementEachResource;
    }

    TaskTree DecrementItemCosts()
    {
        TaskTree decrementEachItem = new TaskTree(new EmptyTask());
        for (int i = Services.MapManager.itemTiles.Count -1; i >= 0; i--)
        {
            decrementEachItem.AddChild(
                new DecrementItem(Services.MapManager.itemTiles[i].containedItem));
        }
        return decrementEachItem;
    }

    TaskTree DecrementBuildings()
    {
        TaskTree decrementEachBuilding = new TaskTree(new EmptyTask());
        foreach (Tile tile in Services.MapManager.buildingTiles)
        {
            if (!tile.containedBuilding.permanentlyControlled && 
                tile.containedBuilding.claimAmountLeft > 0)
            {
                decrementEachBuilding.AddChild(new DecrementBuilding(tile.containedBuilding));
            }
        }
        return decrementEachBuilding;
    }

    public void EndTurn()
    {
        if (!turnEnding)
        {
            currentActivePlayer.workerMovedThisTurn.EndTurn();
            currentActivePlayer.workerMovedThisTurn = null;
            Player nextPlayer = DetermineNextPlayer();
            TaskQueue turnEndTasks = new TaskQueue();
            turnEndTasks.Add(new EmptyTask());
            turnEndTasks.Add(new WaitForAnimations());
            if (nextPlayer != null)
            {
                turnEndTasks.Add(new ScrollTurnBanner(nextPlayer));
            }
            turnEndTasks.Add(new ActionTask(ActuallyEndTurn));
            taskManager.AddTask(turnEndTasks);
            turnEnding = true;
        }
    }

    void ActuallyEndTurn()
    {
        Services.UIManager.selector.Reset();
        Player nextPlayer = DetermineNextPlayer();
        if (nextPlayer == null) EndRound();
        else
        {
            currentActivePlayer = nextPlayer;
            Services.UIManager.UpdateUI();
        }
        turnEnding = false;
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

    public void CheckForWin(Player player)
    {
        if (player.claimedBuildings.Count >= numBuildingClaimsToWin)
        {
            GameWin(player);
        }
    }

    void GameWin(Player player)
    {
        gameOver = true;
        Services.UIManager.selector.gameObject.SetActive(false);
        Services.UIManager.ShowWinMessage(player);

        Debug.Log("player " + player.playerNum + " has won");
    }
}
