using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Worker : MonoBehaviour {

    [HideInInspector]
    public Player parentPlayer;
    [HideInInspector]
    public Tile currentTile;
    private LineRenderer lr;
    private GameObject arrowHead;
    private List<Tile> availableGoals;
    public float tileHopTime;
    public Vector3 tooltipOffset;
    private GameObject tooltip;
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
    private int bonusClaimPower;
    private int bumpPower;
    private List<Item> items;
    private Dictionary<Item.StatType, int> bonuses;
    private Dictionary<Item.StatType, int> tempBonuses;


    [HideInInspector]
    public bool movedThisRound;
    [HideInInspector]
    public bool selected;
    [HideInInspector]
    public Hex lastDirectionMoved;
    [HideInInspector]
    public bool midAnimation;


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
        bonusClaimPower = 0;
        bumpPower = 1;
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
        if (currentTile != null && currentTile.containedWorker == this)
        {
            currentTile.containedWorker = null;
        }
        currentTile = tile;
        Worker prevWorker = tile.containedWorker;
        tile.containedWorker = this;
        if (prevWorker != null) BumpWorker(prevWorker, lastDirectionMoved);
        transform.position = tile.hex.ScreenPos();
    }

    void AnimateMovementAlongPath(List<Tile> path, bool forcedMovement)
    {
        midAnimation = true;
        TaskQueue movementTasks = new TaskQueue();

        for (int i = path.Count - 1; i >= 0; i--)
        {
            movementTasks.Add(new AnimateWorkerMovement(this, path[i], tileHopTime));
        }
        if (forcedMovement) movementTasks.Add(new ActionTask(EndForcedMovement));
        else movementTasks.Add(new ActionTask(EndUnforcedMovement));


        taskManager.AddTaskQueue(movementTasks);
        path.Add(currentTile);
        lastDirectionMoved = path[0].hex.Subtract(path[1].hex);
    }

    void InitiateMovement(List<Tile> path)
    {
        parentPlayer.workerMovedThisTurn = this;
        movesRemaining -= path.Count;
        AnimateMovementAlongPath(path, false);
    }

    public bool TryToMove(Tile goal)
    {
        List<Tile> path = AStarSearch.ShortestPath(currentTile, goal);
        if (CanMoveAlongPath(path) && path.Count > 0) {
            InitiateMovement(path);
            return true;
        }
        else return false;
    }

    public bool CanMoveAlongPath(List<Tile> path)
    {
        return (path.Count <= movesRemaining);
    }

    void EndForcedMovement()
    {
        EndMovement(true);
    }

    void EndUnforcedMovement()
    {
        EndMovement(false);
    }

    void EndMovement(bool forcedMovement)
    {
        midAnimation = false;
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
        if (!AnyAvailableActions() && !forcedMovement) Services.main.EndTurn();
        if (AnyAvailableActions() && !forcedMovement) Services.main.selector.SelectWorker(this);
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
            "Resources: " + resourcesInHand + "/" + carryingCapacity + "\n" +
            "Bump Power: " + bumpPower;

        int extraLines = 0;

        if (bonuses.Count > 0)
        {
            tooltipText += "\nItem Bonuses:";
            extraLines += 1;
            foreach (KeyValuePair<Item.StatType, int> bonus in bonuses)
            {
                tooltipText += "\n" + Item.StatTypeToString(bonus.Key) + " +" + bonus.Value;
                extraLines += 1;
            }
        }

        if (tempBonuses.Count > 0)
        {
            tooltipText += "\nBuilding Bonuses:";
            extraLines += 1;
            foreach (KeyValuePair<Item.StatType, int> bonus in tempBonuses)
            {
                tooltipText += "\n" + Item.StatTypeToString(bonus.Key) + " +" + bonus.Value;
                extraLines += 1;
            }
        }

        if (tooltip == null)
        {
            tooltip = Instantiate(Services.Prefabs.Tooltip, Services.main.canvas);
        }
        tooltip.transform.position =
            Services.main.camera.WorldToScreenPoint(transform.position + tooltipOffset);
        Color tooltipColor = (parentPlayer.color + Color.white) / 2;
        tooltipColor = new Color(tooltipColor.r, tooltipColor.g, tooltipColor.b, 0.85f);
        Image tooltipImage = tooltip.GetComponent<Image>();
        Text tooltipTextComp = tooltip.GetComponentInChildren<Text>();
        Vector2 imageSize = tooltipImage.rectTransform.sizeDelta;
        Vector2 textboxSize = tooltipTextComp.rectTransform.sizeDelta;
        tooltipImage.color = tooltipColor;
        tooltipImage.rectTransform.sizeDelta = new Vector2(imageSize.x, 
            imageSize.y + (20 * extraLines));
        tooltipTextComp.text = tooltipText;
        tooltipTextComp.rectTransform.sizeDelta = new Vector2(textboxSize.x, 
            textboxSize.y + (20 * extraLines));
        Services.main.ShowWorkerTooltip(tooltipText);
    }

    public void HideTooltip()
    {
        Services.main.HideWorkerTooltip();
        if (tooltip != null) Destroy(tooltip);
    }

    void BumpWorker(Worker otherWorker, Hex direction)
    {
        Hex target = otherWorker.currentTile.hex;
        Hex tempTarget;
        Tile targetTile = null;
        for (int i = 1; i < bumpPower + 1; i++)
        {
            tempTarget = otherWorker.currentTile.hex.Add(direction.Multiply(i));
            if (Services.MapManager.map.TryGetValue(tempTarget, out targetTile))
            {
                if (targetTile.containedWorker != null)
                {
                    targetTile = Services.MapManager.map[target];
                    break;
                }
                else target = tempTarget;
            }
            else
            {
                targetTile = Services.MapManager.map[target];
                break;
            }
        }
        if (target == otherWorker.currentTile.hex)
        {
            BumpWorker(otherWorker, direction.Multiply(-1));
        }
        else
        {
            otherWorker.AnimateMovementAlongPath(AStarSearch.ShortestPath(otherWorker.currentTile, targetTile), 
                true);
        }

    }

    public void GetResources(int numResources)
    {
        if (numResources > 0)
        {
            int gain = numResources + bonusResourcePerPickup;
            resourcesInHand += gain;
            taskManager.AddTask(new ResourceAcquisitionAnimation(currentTile, gain));
        }
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
        if (resourcesInHand > 0)
        {
            building.GetClaimed(parentPlayer, resourcesInHand + bonusClaimPower);
            resourcesInHand = 0;
        }
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
            case Item.StatType.BonusClaimPower:
                bonusClaimPower += amount;
                break;
            case Item.StatType.BumpPower:
                bumpPower += amount;
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
        if (path.Count > 0)
        {
            path.Add(currentTile);
            arrowHead.SetActive(true);
            Vector3[] positions = new Vector3[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 basePos = path[i].hex.ScreenPos();
                positions[i] = new Vector3(basePos.x, basePos.y, -1);
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
