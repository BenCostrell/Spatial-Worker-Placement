using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : MonoBehaviour {

    [HideInInspector]
    public Player parentPlayer;
    [HideInInspector]
    public Tile currentTile;
    private LineRenderer lr;
    private GameObject arrowHead;
    private List<Tile> availableGoals;
    public float tileHopTime;
    private TaskManager taskManager;
    private SpriteRenderer sr;
    public int startingMaxMovement;
    [HideInInspector]
    public int maxMovementPerTurn;
    [HideInInspector]
    public int movesRemaining;
    public int startingCarryingCapacity;
    [HideInInspector]
    public int carryingCapacity;
    [HideInInspector]
    public int resourcesInHand;
    private int bonusResourcePerPickup;
    private int itemDiscount;
    private List<Item> items;
    private Dictionary<Item.StatType, int> bonuses;
    private Dictionary<Item.StatType, int> tempBonuses;


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
        lr = GetComponentInChildren<LineRenderer>();
        arrowHead = lr.gameObject;
        arrowHead.SetActive(false);
        sr.color = parentPlayer.color;
        maxMovementPerTurn = startingMaxMovement;
        movesRemaining = maxMovementPerTurn;
        carryingCapacity = startingCarryingCapacity;
        resourcesInHand = 0;
        bonusResourcePerPickup = 0;
        itemDiscount = 0;
        items = new List<Item>();
        availableGoals = new List<Tile>();
        bonuses = new Dictionary<Item.StatType, int>();
        tempBonuses = new Dictionary<Item.StatType, int>();
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

        movementTasks.Add(new ActionTask(EndMovement));

        taskManager.AddTaskQueue(movementTasks);
    }

    void InitiateMovement(List<Tile> path)
    {
        movesRemaining -= (path.Count - 1);
        AnimateMovementAlongPath(path);
        if (path.Count > 0)
        {
            parentPlayer.workerMovedThisTurn = this;
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

    void EndMovement()
    {
        if (currentTile.containedResource != null && resourcesInHand < carryingCapacity)
        {
            GetResources(currentTile.containedResource.GetClaimed(
                carryingCapacity - resourcesInHand - bonusResourcePerPickup));
        }
        if (currentTile.containedItem != null && resourcesInHand >= AdjustedItemCost(currentTile.containedItem))
        {
            AcquireItem(currentTile.containedItem);
        }
        if (currentTile.containedBuilding != null)
        {
            ClaimBuilding(currentTile.containedBuilding);
        }
        Services.main.selector.ShowAppropriateTooltip();

        if (!AnyAvailableActions()) Services.main.EndTurn();
    }

    public void EndTurn()
    {
        Unselect();
        movedThisRound = true;
        sr.color = (parentPlayer.color + Color.gray) / 2;
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
        HighlightAvailableMoves();
        ShowToolTip();
    }

    public void Unselect()
    {
        selected = false;
        if (movedThisRound) sr.color = (parentPlayer.color + Color.gray) / 2;
        else sr.color = parentPlayer.color;
        ClearAvailableMoves();
        HideTooltip();
        arrowHead.SetActive(false);
    }

    public void ShowToolTip()
    {
        string tooltipText = "Moves: " + movesRemaining + "/" + maxMovementPerTurn + "\n" +
            "Resources: " + resourcesInHand + "/" + carryingCapacity;

        if (bonuses.Count > 0)
        {
            tooltipText += "\nItem Bonuses:";
            foreach (KeyValuePair<Item.StatType, int> bonus in bonuses)
            {
                tooltipText += "\n" + Item.StatTypeToString(bonus.Key) + " +" + bonus.Value;
            }
        }
        
        if (tempBonuses.Count > 0)
        {
            tooltipText += "\nBuilding Bonuses:";
            foreach (KeyValuePair<Item.StatType, int> bonus in tempBonuses)
            {
                tooltipText += "\n" + Item.StatTypeToString(bonus.Key) + " +" + bonus.Value;
            }
        }
        

        Services.main.ShowWorkerTooltip(tooltipText);
    }

    public void HideTooltip()
    {
        Services.main.HideWorkerTooltip();
    }

    public void GetResources(int numResources)
    {
        int gain = numResources + bonusResourcePerPickup;
        resourcesInHand += gain;
        taskManager.AddTask(new ResourceAcquisitionAnimation(currentTile, gain));
    }

    int AdjustedItemCost(Item item)
    {
        return Mathf.Max(1, item.cost - itemDiscount);
    }

    public void AcquireItem(Item item)
    {
        item.GetAcquired();
        resourcesInHand -= AdjustedItemCost(item);
        items.Add(item);
        foreach (KeyValuePair<Item.StatType, int> entry in item.statBonuses)
        {
            if (!bonuses.ContainsKey(entry.Key)) bonuses[entry.Key] = entry.Value;
            else bonuses[entry.Key] += entry.Value;
            AlterStat(entry.Key, entry.Value);
        }
    }

    public void GetTempBonuses(Dictionary<Item.StatType, int> tempBonuses_)
    {
        foreach (KeyValuePair<Item.StatType, int> entry in tempBonuses_)
        {
            if (!tempBonuses.ContainsKey(entry.Key)) tempBonuses[entry.Key] = entry.Value;
            else tempBonuses[entry.Key] += entry.Value;
            AlterStat(entry.Key, entry.Value);
        }
    }

    public void LoseTempBonuses(Dictionary<Item.StatType, int> tempBonuses_)
    {
        List<Item.StatType> completelyLostStats = new List<Item.StatType>();
        foreach (KeyValuePair<Item.StatType, int> entry in tempBonuses_)
        {
            tempBonuses[entry.Key] -= entry.Value;
            if (tempBonuses[entry.Key] == 0) completelyLostStats.Add(entry.Key);
            AlterStat(entry.Key, -entry.Value);
        }
        if (completelyLostStats.Count > 0) {
            foreach(Item.StatType stat in completelyLostStats)
            {
                tempBonuses.Remove(stat);
            }
        }
    }

    public void ClaimBuilding(Building building)
    {
        building.GetClaimed(parentPlayer, resourcesInHand);
        resourcesInHand = 0;
    }

    public void AlterStat(Item.StatType statType, int amount)
    {
        switch (statType)
        {
            case Item.StatType.MovementSpeed:
                maxMovementPerTurn += amount;
                break;
            case Item.StatType.CarryingCapacity:
                carryingCapacity += amount;
                break;
            case Item.StatType.ExtraResourcePickup:
                bonusResourcePerPickup += amount;
                break;
            case Item.StatType.ItemDiscount:
                itemDiscount += amount;
                break;
            default:
                break;
        }
    }

    void HighlightAvailableMoves()
    {
        ClearAvailableMoves();
        availableGoals = AStarSearch.FindAllAvailableGoals(currentTile, movesRemaining);
        if (availableGoals.Count > 0)
        {
            foreach (Tile tile in availableGoals)
            {
                tile.obj.GetComponent<SpriteRenderer>().color = tile.moveAvailableColor;
            }
        }
    }

    void ClearAvailableMoves()
    {
        if (availableGoals.Count > 0)
        {
            foreach (Tile tile in availableGoals)
            {
                tile.obj.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
        availableGoals = new List<Tile>();
    }

    public void ShowPathArrow(List<Tile> path)
    {
        if (path.Count > 1)
        {
            arrowHead.SetActive(true);
            Vector3[] positions = new Vector3[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 basePos = path[i].hex.ScreenPos();
                positions[i] = new Vector3(basePos.x, basePos.y, -1);
                Debug.Log(positions[i]);
            }
            lr.positionCount = positions.Length;
            lr.SetPositions(positions);
            Vector3 arrowheadBasePos = path[0].hex.ScreenPos();
            arrowHead.transform.position = new Vector3(arrowheadBasePos.x, arrowheadBasePos.y, -2);
            Hex direction = Hex.Subtract(path[0].hex, path[1].hex);
            float angle = Hex.DirectionToAngle(direction) - 30;
            arrowHead.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            arrowHead.SetActive(false);
        }
    }

    bool AnyAvailableActions()
    {
        return movesRemaining > 0;
    }
}
