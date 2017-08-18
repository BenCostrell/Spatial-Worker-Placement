using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{

    private Tile hoveredTile;
    private int inputLastFrame;
    private float timeToWaitBeforeRepeatingInput {
        get { return 1 / currentSpeed; }
    }
    public float baseSpeed;
    public float accel;
    public float maxSpeed;
    private float currentSpeed;
    private float timeSinceLastUniqueInput;
    [HideInInspector]
    public Worker hoveredWorker;
    private Worker selectedWorker;
    private Tile lastHoveredTile;
    [SerializeField]
    private float pulseCyclePeriod;
    [SerializeField]
    private float pulseSize;
    private float pulseCycleTime;
    private bool pulseGrow;
    private Vector3 baseScale;

    // Use this for initialization
    void Start()
    {
        timeSinceLastUniqueInput = 0;
        Services.EventManager.Register<ButtonPressed>(OnButtonPressed);
        transform.localScale = Services.MapManager.layout.size;
        currentSpeed = baseSpeed;
        baseScale = transform.localScale;
        pulseGrow = true;
    }

    // Update is called once per frame
    void Update()
    {
        Pulse();
        ProcessInput();
    }

    public void PlaceOnTile(Tile tile)
    {
        transform.position = tile.hex.ScreenPos();
        hoveredTile = tile;
        ShowAppropriateTooltip();
        HideLastHoveredInfo();
        ShowOnHoverInfo();
        lastHoveredTile = tile;
    }

    void Pulse()
    {
        pulseCycleTime += Time.deltaTime;

        if (pulseGrow)
        {
            transform.localScale = Vector3.Lerp(baseScale, pulseSize * baseScale,
                Easing.QuadEaseOut(pulseCycleTime / pulseCyclePeriod));
        }
        else
        {
            transform.localScale = Vector3.Lerp(pulseSize * baseScale, baseScale,
                Easing.QuadEaseOut(pulseCycleTime / pulseCyclePeriod));
        }

        if (pulseCycleTime >= pulseCyclePeriod)
        {
            pulseGrow = !pulseGrow;
            pulseCycleTime = 0;
        }
    }

    void ProcessInput()
    {
        float xInput = Input.GetAxis("Horizontal_P" + Services.main.currentActivePlayer.playerNum);
        float yInput = Input.GetAxis("Vertical_P" + Services.main.currentActivePlayer.playerNum);
        float mag = new Vector2(xInput, yInput).magnitude;
        if (mag > 0.1f)
        {
            MoveSelector(xInput, yInput);
            if (selectedWorker != null) HighlightPath(selectedWorker.currentTile, hoveredTile);
            currentSpeed += accel;
            if (currentSpeed > maxSpeed) currentSpeed = maxSpeed;
        }
        else
        {
            inputLastFrame = -1;
            currentSpeed = baseSpeed;
        }
    }

    void MoveSelector(float xInput, float yInput)
    {
        float angle = Mathf.Atan2(yInput, xInput) * Mathf.Rad2Deg;
        float convertedAngle = (angle + 330) % 360;
        int directionIndex = 5 - Mathf.FloorToInt(convertedAngle / 60f);
        if (timeSinceLastUniqueInput > timeToWaitBeforeRepeatingInput)
        {
            Tile tile;
            if (Services.MapManager.map.TryGetValue(hoveredTile.hex.Neighbor(directionIndex), out tile))
            {
                PlaceOnTile(tile);
            }
            timeSinceLastUniqueInput = 0;
        }
        else timeSinceLastUniqueInput += Time.deltaTime;
        inputLastFrame = directionIndex;
    }

    public void ShowAppropriateTooltip()
    {
        if (hoveredTile.containedWorker != null && selectedWorker == null)
        {
            if (hoveredWorker != hoveredTile.containedWorker)
            {
                if (hoveredWorker != null) hoveredWorker.HideTooltip();
                hoveredWorker = hoveredTile.containedWorker;
                hoveredWorker.ShowToolTip();
            }
        }
        else if (hoveredWorker != null)
        {
            hoveredWorker.HideTooltip();
            hoveredWorker = null;
        }
        if (selectedWorker != null)
        {
            selectedWorker.ShowToolTip();
        }
    }

    void ShowOnHoverInfo()
    {
        if (selectedWorker != null)
        {
            if (hoveredTile.containedBuilding != null && 
                !hoveredTile.containedBuilding.hoverInfoActive
                && !hoveredTile.containedBuilding.permanentlyControlled)
            {
                hoveredTile.containedBuilding.ShowPotentialClaim(selectedWorker);
            }
            else if (hoveredTile.containedItem != null &&
                !hoveredTile.containedItem.hoverInfoActive)
            {
                hoveredTile.containedItem.ShowPotentialPurchasePrice(selectedWorker);
            }
        }

        if (hoveredTile.containedItem != null 
            && !hoveredTile.containedItem.tooltipActive)
        {
            hoveredTile.containedItem.ShowTooltip();
        }
        if (hoveredTile.containedBuilding != null 
            && !hoveredTile.containedBuilding.tooltipActive)
        {
            hoveredTile.containedBuilding.ShowTooltip();
        }
        if (hoveredTile.zone != null)
        {
            if (hoveredTile.zone != lastHoveredTile.zone)
                hoveredTile.zone.ShowTooltip();
        }
    }

    void HideLastHoveredInfo()
    {
        if (lastHoveredTile != null)
        {
            if (lastHoveredTile.containedBuilding != null)
            {
                if (lastHoveredTile.containedBuilding.hoverInfoActive)
                {
                    lastHoveredTile.containedBuilding.ResetDisplay();
                }
                if (lastHoveredTile.containedBuilding.tooltipActive)
                {
                    lastHoveredTile.containedBuilding.HideTooltip();
                }
            }
            else if (lastHoveredTile.containedItem != null)
            {
                if (lastHoveredTile.containedItem.hoverInfoActive)
                {
                    lastHoveredTile.containedItem.ResetDisplay();
                }
                if (lastHoveredTile.containedItem.tooltipActive)
                {
                    lastHoveredTile.containedItem.HideTooltip();
                }
            }
            if (lastHoveredTile.zone != null)
            {
                if (hoveredTile.zone != lastHoveredTile.zone)
                    lastHoveredTile.zone.HideTooltip();
            }
        }
    }

    void OnButtonPressed (ButtonPressed e)
    {
        if (e.playerNum == Services.main.currentActivePlayer.playerNum)
        {
            switch (e.button)
            {
                case "A":
                    SelectTile();
                    break;
                case "B":
                    if (selectedWorker != null) UnselectWorker();
                    break;
                case "RB":
                    CycleToNextWorker();
                    break;
                default:
                    break;

            }
        }
    }

    void CycleToNextWorker()
    {
        if (selectedWorker != null) UnselectWorker();
        if (hoveredTile.containedWorker == null)
        {
            foreach(Worker worker in Services.main.currentActivePlayer.workers)
            {
                if (!worker.movedThisRound)
                {
                    PlaceOnTile(worker.currentTile);
                    break;
                }
            }
        }
        else
        {
            List<Worker> playerWorkers = Services.main.currentActivePlayer.workers;
            int index = playerWorkers.IndexOf(hoveredTile.containedWorker);
            index = (index + 1) % playerWorkers.Count;
            PlaceOnTile(playerWorkers[index].currentTile);
        }
    }

    void SelectTile()
    {
        if (selectedWorker != null)
        {
            selectedWorker.TryToMove(hoveredTile);
            UnselectWorker();
        }
        else {
            if (hoveredTile.containedWorker != null)
            {
                Worker worker = hoveredTile.containedWorker;
                if (!worker.movedThisRound && 
                    worker.parentPlayer == Services.main.currentActivePlayer &&
                    ((worker.parentPlayer.workerMovedThisTurn == null) || 
                    (worker.parentPlayer.workerMovedThisTurn == worker)))
                {
                    if (selectedWorker != null) UnselectWorker();
                    SelectWorker(worker);
                }
            }
        }
    }

    public void SelectWorker(Worker worker)
    {
        worker.Select();
        selectedWorker = worker;
    }

    void UnselectWorker()
    {
        selectedWorker.Unselect();
        selectedWorker = null;
        HideLastHoveredInfo();
    }

    void HighlightPath(Tile start, Tile goal)
    {
        List<Tile> path = AStarSearch.ShortestPath(start, goal, selectedWorker.parentPlayer, false);
        if (selectedWorker.CanMoveAlongPath(path))
        {
            selectedWorker.ShowPathArrow(path);
        }
    }

    public void Reset()
    {
        if (selectedWorker != null) UnselectWorker();
        PlaceOnTile(Services.MapManager.map[new Hex(0, -1, 1)]);
        ShowAppropriateTooltip();
    }

    public void SetColor()
    {
        GetComponent<SpriteRenderer>().color = Services.main.currentActivePlayer.color;
    }
}