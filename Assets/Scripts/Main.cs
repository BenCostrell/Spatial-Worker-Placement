using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : Scene<TransitionData> {

    [HideInInspector]
    public Selector selector;
    [HideInInspector]
    public Player currentActivePlayer;
    public GameObject roundCounter;
    public GameObject workerTooltip;
    public GameObject tileTooltip;
    private int roundNum;
    [HideInInspector]
    public List<Building> buildings;
    public int numBuildingClaimsToWin;
    public float resGainAnimOffset;
    public float resGainAnimDur;
    public float resGainAnimDist;
    private TaskManager taskManager;
    [HideInInspector]
    public Transform canvas;
    [HideInInspector]
    public Camera mainCamera;

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
        Services.MapManager.CreateHexGrid();
        PlaceInitialWorkers();
        CreateSelector();
        roundNum = 0;
        taskManager = new TaskManager();
        canvas = GetComponentInChildren<Canvas>().transform;
        mainCamera = GetComponentInChildren<Camera>();
        HideWorkerTooltip();
        SetTileTooltip(Services.MapManager.map[new Hex(0, 0, 0)]);
        Services.EventManager.Register<ButtonPressed>(OnButtonPressed);
        StartRound();
    }

    void InitializeMainServices()
    {
        Services.MapManager = Services.GameManager.sceneRoot.GetComponentInChildren<MapManager>();
        Services.main = this;
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

    void CreateSelector()
    {
        selector = Instantiate(Services.Prefabs.Selector, Services.SceneStackManager.CurrentScene.transform)
            .GetComponent<Selector>();
        selector.PlaceOnTile(Services.MapManager.map[new Hex(0, 0, 0)]);
    }

    void StartRound()
    {
        roundNum += 1;
        currentActivePlayer = Services.GameManager.players[(roundNum - 1) % Services.GameManager.numPlayers];
        foreach (Player player in Services.GameManager.players) player.Refresh();
        UpdateUI();
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
        foreach (Tile tile in Services.MapManager.itemTiles)
        {
            tile.containedItem.DecrementCost();
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
        selector.Reset();
        currentActivePlayer.workerMovedThisTurn.EndTurn();
        currentActivePlayer.workerMovedThisTurn = null;
        Player nextPlayer = DetermineNextPlayer();
        if (nextPlayer == null) EndRound();
        else
        {
            currentActivePlayer = nextPlayer;
            UpdateUI();
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

    void UpdateUI()
    {
        roundCounter.GetComponent<Text>().text = "Round " + roundNum + "\n" + 
            "Player " + currentActivePlayer.playerNum + " Turn";
        selector.SetColor();
    }

    public void ShowWorkerTooltip(string tooltipText)
    {
        workerTooltip.SetActive(true);
        
        workerTooltip.GetComponent<Text>().text = tooltipText;
    }

    public void HideWorkerTooltip()
    {
        workerTooltip.SetActive(false);
    }

    public void SetTileTooltip(Tile tile)
    {
        tileTooltip.GetComponent<Text>().text = tile.TooltipText();
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
