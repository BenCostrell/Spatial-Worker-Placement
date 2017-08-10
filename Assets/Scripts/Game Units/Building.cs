using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class Building : MonoBehaviour {

    public Color defaultColor;
    public Vector2 offset;
    private GameObject tooltip;
    public Color tooltipColor;
    private SpriteRenderer sr;
    private TextMesh textMesh;
    private Color defaultTextColor;
    public Player controller { get; private set; }
    public bool permanentlyControlled { get; private set; }
    public int permanentControlThreshold;
    public float permanentControlScaleIncrease;
    public float decrementAnimTime;
    public float decrementAnimScale;
    private List<int> playerInfluence;
    private List<GameObject> influenceBars;
    private int turnsLeft_;
    public int turnsLeft
    {
        get { return turnsLeft_; }
        private set
        {
            turnsLeft_ = value;
            if (value == 0) textMesh.text = "";
            else textMesh.text = value.ToString();
        }
    }
    private Tile parentTile;
    public Dictionary<Item.StatType, int> statBonuses { get; private set; }
    public bool hoverInfoActive { get; private set; }
    public bool tooltipActive { get; private set; }

    // Use this for initialization
    public void Init(Tile tile, Dictionary<Item.StatType, int> statBonuses_)
    {
        sr = GetComponent<SpriteRenderer>();
        textMesh = GetComponentInChildren<TextMesh>();
        textMesh.gameObject.GetComponent<Renderer>().sortingOrder = 4;
        defaultTextColor = textMesh.color;
        sr.color = defaultColor;
        turnsLeft = 0;
        permanentlyControlled = false;
        parentTile = tile;
        transform.position = tile.hex.ScreenPos() + offset;
        statBonuses = statBonuses_;
        Item.StatType bonus = statBonuses.First().Key;
        GetComponentsInChildren<SpriteRenderer>()[1].sprite =
            Services.ItemConfig.GetItemStatConfig(bonus).Sprite;
        playerInfluence = new List<int>();
        influenceBars = new List<GameObject>();
        SpriteRenderer[] childSrs = GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer csr in childSrs)
        {
            if (csr.tag == "InfluenceBar")
            {
                influenceBars.Add(csr.gameObject);
            }
        }
        for (int i = 0; i < Services.GameManager.numPlayers; i++)
        {
            playerInfluence.Add(0);
            Transform progressBar = influenceBars[i].transform.GetChild(0);
            progressBar.GetComponent<SpriteRenderer>().color =
                Services.GameManager.playerColors[i];
            progressBar.transform.localScale = new Vector3(1, 0, 1);
        }
    }

    public void GetClaimed(Player player, int claimAmount)
    {
        if (claimAmount > 0)
        {
            if (controller == null)
            {
                SuccessfulClaim(player, claimAmount);
            }
            else if (controller == player)
            {
                turnsLeft += claimAmount;
            }
            else if (claimAmount < turnsLeft)
            {
                turnsLeft -= claimAmount;
            }
            else if (claimAmount > turnsLeft)
            {
                SuccessfulClaim(player, claimAmount - turnsLeft);
            }
            else
            {
                ReturnToNeutral();
                turnsLeft = 0;
            }
        }
    }

    void SuccessfulClaim(Player player, int initialTurnsLeft)
    {
        if (controller != null) LoseControl();
        controller = player;
        sr.color = player.color;
        turnsLeft = initialTurnsLeft;
        player.claimedBuildings.Add(this);
        foreach(Worker worker in player.workers) worker.GetTempBonuses(statBonuses);
        Services.main.CheckForWin();
    }

    void LoseControl()
    {
        controller.claimedBuildings.Remove(this);
        foreach (Worker worker in controller.workers) worker.LoseTempBonuses(statBonuses);
    }

    void ReturnToNeutral()
    {
        LoseControl();
        controller = null;
        sr.color = defaultColor;
    }

    public void Decrement()
    {
        if (turnsLeft > 0)
        {
            turnsLeft -= 1;
            IncrementInfluence(controller.playerNum - 1);
            if (playerInfluence[controller.playerNum - 1] >= permanentControlThreshold)
            {
                GainPermanentControl();
            }
            else if (turnsLeft == 0) ReturnToNeutral();
        }
    }
    
    void IncrementInfluence(int playerIndex)
    {
        playerInfluence[playerIndex] += 1;
        influenceBars[playerIndex].transform.GetChild(0).localScale =
            new Vector3(1, playerInfluence[playerIndex] / (float)permanentControlThreshold, 1);
    }

    void GainPermanentControl()
    {
        permanentlyControlled = true;
        turnsLeft = 0;
        transform.localScale *= permanentControlScaleIncrease;
        foreach(GameObject bar in influenceBars)
        {
            bar.SetActive(false);
        }
    }

    public void ShowPotentialClaim(Worker worker)
    {
        hoverInfoActive = true;
        int claimAmount;
        if (worker.resourcesInHand > 0)
        {
            claimAmount = worker.resourcesInHand + worker.bonusClaimPower;
        }
        else claimAmount = 0;

        textMesh.color = Color.cyan;
        int newPotentialTurnsLeftCounter;
        if (controller == null && claimAmount > 0)
        {
            newPotentialTurnsLeftCounter = claimAmount;
            sr.color = worker.parentPlayer.color;
        }
        else if (controller == worker.parentPlayer)
        {
            newPotentialTurnsLeftCounter = turnsLeft + claimAmount;
        }
        else if (claimAmount > turnsLeft)
        {
            newPotentialTurnsLeftCounter = claimAmount - turnsLeft;
            sr.color = worker.parentPlayer.color;
        }
        else if (claimAmount < turnsLeft)
        {
            newPotentialTurnsLeftCounter = turnsLeft - claimAmount;
        }
        else
        {
            newPotentialTurnsLeftCounter = 0;
            sr.color = Color.white;
        }

        if (newPotentialTurnsLeftCounter != 0)
        {
            textMesh.text = newPotentialTurnsLeftCounter.ToString();
        }
        else
        {
            textMesh.text = "";
        }
    }

    public void ResetDisplay()
    {
        hoverInfoActive = false;
        if (turnsLeft != 0)
        {
            textMesh.text = turnsLeft.ToString();
        }
        else
        {
            textMesh.text = "";
        }
        textMesh.color = defaultTextColor;
        if (controller == null) sr.color = Color.white;
        else sr.color = controller.color;
    }

    public void ShowTooltip()
    {
        tooltipActive = true;
        tooltip = GameObject.Instantiate(Services.Prefabs.BuildingTooltip,
            Services.UIManager.canvas);
        RectTransform tooltipRect = tooltip.GetComponent<RectTransform>();
        Vector3 offset = Services.ItemConfig.TooltipOffset;
        if (transform.position.x > 0)
        {
            offset = new Vector3(-offset.x, offset.y, offset.z);
        }
        tooltipRect.anchoredPosition =
            Services.main.mainCamera.WorldToScreenPoint(transform.position + offset);
        string tooltipText = "Bonus to each of controller's units:\n";
        foreach (KeyValuePair<Item.StatType, int> bonus in statBonuses)
        {
            tooltipText += Services.ItemConfig.GetItemStatConfig(bonus.Key).Label
                + "\n +" + bonus.Value + "\n";
        }
        tooltip.GetComponentInChildren<Text>().text = tooltipText;
        tooltip.GetComponentInChildren<Image>().color = tooltipColor;
        Services.main.taskManager.AddTask(new ExpandTooltip(tooltip));
    }

    public void HideTooltip()
    {
        tooltipActive = false;
        GameObject.Destroy(tooltip);
    }
}
