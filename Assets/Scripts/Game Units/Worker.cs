using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Worker : MonoBehaviour {

    public Player parentPlayer { get; private set; }
    public Tile currentTile { get; private set; }
    private LineRenderer lr;
    private GameObject arrowHead;
    private TextMesh resourceHudText;
    private List<Tile> availableGoals;
    public float tileHopTime;
    public Vector3 tooltipOffset;
    public int extraTooltipHeightPerLine;
    private GameObject tooltip;
    private TaskManager taskManager;
    private SpriteRenderer sr;
    public Vector2 offset;
    public int startingMaxMovement;
    public int maxMovementPerTurn { get; private set; }
    public int movesRemaining { get; private set; }
    public int startingCarryingCapacity;
    private int carryingCapacity_;
    public int carryingCapacity {
        get { return carryingCapacity_; }
        private set
        {
            carryingCapacity_ = value;
            resourceHudText.text = resourcesInHand_ + "/" + carryingCapacity_;
        }
    }
    private int resourcesInHand_;
    public int resourcesInHand
    {
        get { return resourcesInHand_; }
        private set
        {
            resourcesInHand_ = value;
            resourceHudText.text = resourcesInHand_ + "/" + carryingCapacity_;
        }
    }
    private int bonusResourcePerPickup;
    public int itemDiscount { get; private set; }
    public int bonusClaimPower { get; private set; }
    public int defaultBumpPower;
    private int bumpPower;
    public int zoneExpandPower { get; private set; }
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
    public int activeAnimations;
    [HideInInspector]
    public int mostRecentResourceAcquisition;


    // Use this for initialization
    public void Init (Player parent, Tile tile) {
        parentPlayer = parent;
        taskManager = new TaskManager();
        sr = GetComponent<SpriteRenderer>();
        lr = GetComponentInChildren<LineRenderer>();
        resourceHudText = GetComponentInChildren<TextMesh>();
        resourceHudText.gameObject.GetComponent<Renderer>().sortingOrder = 7;
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
        zoneExpandPower = 1;
        activeAnimations = 0;
        bumpPower = defaultBumpPower;
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
        transform.position = tile.hex.ScreenPos() + offset;
    }

    void AnimateMovementAlongPath(List<Tile> path, bool forcedMovement)
    {
        Services.main.activeAnimations += 1;
        TaskQueue movementTasks = new TaskQueue();

        for (int i = path.Count - 1; i >= 0; i--)
        {
            movementTasks.Add(new AnimateWorkerMovement(this, path[i], tileHopTime));
        }
        if (forcedMovement) movementTasks.Add(new ActionTask(EndForcedMovement));
        else movementTasks.Add(new ActionTask(EndUnforcedMovement));


        taskManager.AddTask(movementTasks);
        path.Add(currentTile);
        lastDirectionMoved = path[0].hex.Subtract(path[1].hex);
    }

    void InitiateMovement(List<Tile> path)
    {
        parentPlayer.workerMovedThisTurn = this;
        movesRemaining -= PathCost(path);
        AnimateMovementAlongPath(path, false);
    }

    public bool TryToMove(Tile goal)
    {
        List<Tile> path = AStarSearch.ShortestPath(currentTile, goal, parentPlayer, false);
        if (path.Count > 0 && CanMoveAlongPath(path)) {
            InitiateMovement(path);
            return true;
        }
        else return false;
    }

    public bool CanMoveAlongPath(List<Tile> path)
    {
        return (PathCost(path) <= movesRemaining);
    }

    int PathCost(List<Tile> path)
    {
        int totalMovementCost = 0;
        foreach (Tile tile in path)
            totalMovementCost += tile.movementCostPerPlayer[parentPlayer.playerNum - 1];
        return totalMovementCost;
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
        Services.main.activeAnimations -= 1;
        if (currentTile.containedResource != null && resourcesInHand < carryingCapacity)
        {
            ClaimResources(currentTile.containedResource);
        }
        if (currentTile.containedItem != null && resourcesInHand >= AdjustedItemCost(currentTile.containedItem))
        {
            AcquireItem(currentTile.containedItem);
        }
        if (currentTile.containedBuilding != null && 
            !currentTile.containedBuilding.permanentlyControlled)
        {
            ClaimBuilding(currentTile.containedBuilding);
        }
        if (currentTile.zone != null && currentTile.zone.controller == null)
        {
            currentTile.zone.GetClaimed(this);
        }
        Services.UIManager.selector.ShowAppropriateTooltip();
        if (Services.main.gameOver)
        {
            return;
        }
        if (!AnyAvailableActions() && !forcedMovement) Services.main.EndTurn();
        if (AnyAvailableActions() && !forcedMovement) Services.UIManager.selector.SelectWorker(this);
    }

    public void EndTurn()
    {
        Unselect();
        movedThisRound = true;
        sr.color = (parentPlayer.color + Color.black) / 2;
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
        if (movedThisRound) sr.color = (parentPlayer.color + Color.black) / 2;
        else sr.color = parentPlayer.color;
        ClearAvailableMoves();
        if (Services.UIManager.selector.hoveredWorker != this) HideTooltip();
        arrowHead.SetActive(false);
    }

    public void ShowToolTip()
    {
        string tooltipText = "Moves: " + movesRemaining + "/" + maxMovementPerTurn;

        int extraLines = 0;

        if (bonuses.Count > 0)
        {
            tooltipText += "\nItem Bonuses:";
            extraLines += 1;
            foreach (KeyValuePair<Item.StatType, int> bonus in bonuses)
            {
                tooltipText += "\n" + Services.ItemConfig.GetItemStatConfig(bonus.Key).Label + 
                    " +" + bonus.Value;
                extraLines += 1;
            }
        }

        if (tempBonuses.Count > 0)
        {
            tooltipText += "\nBuilding Bonuses:";
            extraLines += 1;
            foreach (KeyValuePair<Item.StatType, int> bonus in tempBonuses)
            {
                tooltipText += "\n" + Services.ItemConfig.GetItemStatConfig(bonus.Key).Label
                    + " +" + bonus.Value;
                extraLines += 1;
            }
        }

        bool animate = false;

        if (tooltip != null) Destroy(tooltip);
        else animate = true;
        tooltip = Instantiate(Services.Prefabs.Tooltip, Services.UIManager.canvas);

        tooltip.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        Color tooltipColor = (parentPlayer.color + Color.white) / 2;
        tooltipColor = new Color(tooltipColor.r, tooltipColor.g, tooltipColor.b, 0.85f);
        Image tooltipImage = tooltip.GetComponent<Image>();
        Text tooltipTextComp = tooltip.GetComponentInChildren<Text>();
        Vector2 imageSize = tooltipImage.rectTransform.sizeDelta;
        Vector2 textboxSize = tooltipTextComp.rectTransform.sizeDelta;
        tooltipImage.color = tooltipColor;
        tooltipImage.rectTransform.sizeDelta = new Vector2(imageSize.x, 
            imageSize.y + (extraTooltipHeightPerLine * extraLines));
        tooltipTextComp.text = tooltipText;
        tooltipTextComp.rectTransform.sizeDelta = new Vector2(textboxSize.x, 
            textboxSize.y + (extraTooltipHeightPerLine * extraLines));
        if (animate) Services.main.taskManager.AddTask(new ExpandTooltip(tooltip));
    }

    public void HideTooltip()
    {
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
            otherWorker.AnimateMovementAlongPath(
                AStarSearch.ShortestPath(otherWorker.currentTile, targetTile, parentPlayer, true), 
                true);
        }

    }

    void ClaimResources(Resource resource)
    {
        int openRoom = carryingCapacity - resourcesInHand;
        int resourceClaimAmount = Mathf.Max(openRoom - bonusResourcePerPickup, 1);
        int resourcesAvailable = resource.numResources;
        int yield = resource.GetClaimed(resourceClaimAmount, this);
        int resourcesGained;
        if (yield > 0) resourcesGained = Mathf.Min(yield + bonusResourcePerPickup, openRoom);
        else resourcesGained = 0;
        GetResources(resourcesGained);
        if (resourcesAvailable != yield)
            taskManager.AddTask(new ResourceAcquisitionAnimation(this));
    }

    public void GetResources(int numResources)
    {
        if (numResources > 0)
        {
            resourcesInHand += numResources;
            mostRecentResourceAcquisition = numResources;
        }
    }

    int AdjustedItemCost(Item item)
    {
        return Mathf.Max(1, item.cost - itemDiscount);
    }

    public void AcquireItem(Item item)
    {
        item.GetAcquired(this);
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
        availableGoals = 
            AStarSearch.FindAllAvailableGoals(currentTile, movesRemaining, parentPlayer, false);
        if (availableGoals.Count > 0)
        {
            //foreach (Tile tile in availableGoals)
            //{
            //    tile.sr.color = tile.moveAvailableTint * tile.moveAvailableTintProportion + 
            //        Color.white * (1-tile.moveAvailableTintProportion);
            //}
            foreach(KeyValuePair<Hex, Tile> space in Services.MapManager.map)
            {
                Tile tile = space.Value;
                if (!availableGoals.Contains(tile))
                {
                    tile.sr.color = tile.moveUnavailableTint * tile.moveAvailableTintProportion +
                    Color.white * (1 - tile.moveUnavailableTintProportion);
                }
            }
        }
    }

    void ClearAvailableMoves()
    {
        if (availableGoals.Count > 0)
        {
            //foreach (Tile tile in availableGoals)
            //{
            //    tile.sr.color = Color.white;
            //}
            foreach (KeyValuePair<Hex, Tile> space in Services.MapManager.map)
            {
                Tile tile = space.Value;
                if (!availableGoals.Contains(tile))
                {
                    tile.sr.color = Color.white;
                }
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
        List<Tile> availableMoves = 
            AStarSearch.FindAllAvailableGoals(currentTile, movesRemaining, parentPlayer, false);
        return availableMoves.Count > 0;
    }

    public void DrainResources(int resourcesToDrain)
    {
        resourcesInHand = Mathf.Max(0, resourcesInHand - resourcesToDrain);
    }
}
