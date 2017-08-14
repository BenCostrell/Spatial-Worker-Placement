using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public GameObject roundCounter;
    public GameObject roundTracker;
    public GameObject winMessage;
    public GameObject turnBanner;
    public GameObject bannerSandwichTop;
    public GameObject bannerSandwichBottom;
    public float roundTrackerSpacing;
    public int maxTurnsShown;
    public float currTurnScaleUp;
    public float tooltipExpandTime;
    public float bannerScrollTime;
    public Vector2 bannerOffset;
    public Vector2 sandwichOffset;
    [HideInInspector]
    public Transform canvas;
    [HideInInspector]
    public Selector selector { get; private set; }
    private int trackerCurrentTurnNum;
    private int trackerLastRoundNum;

    public void InitUI()
    {
        CreateSelector();
        trackerCurrentTurnNum = -1;
        winMessage.SetActive(false);
        turnBanner.SetActive(false);
        bannerSandwichBottom.SetActive(false);
        bannerSandwichTop.SetActive(false);
    }

    public void UpdateUI()
    {
        roundCounter.GetComponent<Text>().text = "Round " + Services.main.roundNum + "\n" +
            "Player " + Services.main.currentActivePlayer.playerNum + " Turn";
        selector.SetColor();
        trackerCurrentTurnNum += 1;
        SetRoundTracker();
    }

    void CreateSelector()
    {
        selector = Instantiate(Services.Prefabs.Selector, Services.SceneStackManager.CurrentScene.transform)
            .GetComponent<Selector>();
        selector.PlaceOnTile(Services.MapManager.map[new Hex(0, -1, 1)]);
    }

    int CalcPlayerTurn(int turnNum, int roundNum)
    {
        return (trackerCurrentTurnNum + turnNum + roundNum) % 2;
    }

    void SetRoundTracker()
    {
        for (int i = 0; i < roundTracker.transform.childCount; i++)
        {
            Destroy(roundTracker.transform.GetChild(i).gameObject);
        }
        float spacing = 0;
        int tempRoundNum = Services.main.roundNum - 1;
        for (int i = 0; i < maxTurnsShown; i++)
        {
            GameObject turnMarker =
                Instantiate(Services.Prefabs.TurnMarker, roundTracker.transform);
            Image img = turnMarker.GetComponent<Image>();
            turnMarker.transform.localPosition = new Vector3(spacing, 0, 0);
            spacing += roundTrackerSpacing;
            img.color = Services.GameManager.playerColors[CalcPlayerTurn(i, tempRoundNum)];
            if (i == 0) turnMarker.transform.localScale *= currTurnScaleUp;
            else img.color = (img.color + Color.grey) / 2;
            if ((trackerCurrentTurnNum + i) % 4 == 3)
            {
                GameObject roundMarker = Instantiate(Services.Prefabs.RoundMarker,
                    roundTracker.transform);
                roundMarker.transform.localPosition = new Vector3(spacing, 0, 0);
                spacing += roundTrackerSpacing;
                tempRoundNum += 1;
            }
        }
    }

    public void ShowWinMessage(Player winner)
    {
        winMessage.SetActive(true);
        Image image = winMessage.GetComponent<Image>();
        image.color = new Color(winner.color.r, winner.color.g, winner.color.b, image.color.a);
        Text winText = winMessage.GetComponentInChildren<Text>();
        winText.text = winner.name + " WINS";
    }
}
