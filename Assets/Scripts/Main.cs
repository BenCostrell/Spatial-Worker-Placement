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

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    internal override void Init()
    {
        InitializeMainServices();
        Services.MapManager.CreateHexGrid();
        PlaceInitialWorkers();
        CreateSelector();
        roundNum = 0;
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
        selector.Reset();
        currentActivePlayer.workerMovedThisTurn.EndTurn();
        currentActivePlayer.workerMovedThisTurn = null;
        Player nextPlayer = DetermineNextPlayer();
        if (nextPlayer == null) EndRound();
        else {
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
        string toolTipText = "";
        if (tile.containedResource != null)
        {
            toolTipText = "Tile Resources: " + tile.containedResource.numResources;
        }
        else if (tile.containedItem != null)
        {
            toolTipText = "Item - Cost : " + tile.containedItem.cost;
            foreach(KeyValuePair<Item.StatType, int> bonus in tile.containedItem.statBonuses)
            {
                toolTipText += "\n" + Item.StatTypeToString(bonus.Key) + " +" + bonus.Value;
            }
        }
        else if (tile.containedBuilding != null) {
            toolTipText = "Building : \n";
            if (tile.containedBuilding.controller == null)
            {
                toolTipText += "Neutral \n";
            }
            else
            {
                toolTipText += "Controlled by " + tile.containedBuilding.controller.name + "\n" +
                    tile.containedBuilding.turnsLeft + " rounds left \n";
            }
            toolTipText += "Bonuses to all controller's workers: ";
            foreach (KeyValuePair<Item.StatType, int> bonus in tile.containedBuilding.statBonuses)
            {
                toolTipText += "\n" + Item.StatTypeToString(bonus.Key) + " +" + bonus.Value;
            }
        }

        tileTooltip.GetComponent<Text>().text = toolTipText;
    }

    void OnButtonPressed(ButtonPressed e)
    {
        if (e.playerNum == currentActivePlayer.playerNum && (currentActivePlayer.workerMovedThisTurn != null) && 
            e.button == "B")
        {
            EndTurn();
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
