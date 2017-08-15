using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScrollTurnBanner : Task
{
    private GameObject turnBanner;
    private GameObject bannerSandwichTop;
    private GameObject bannerSandwichBottom;
    private RectTransform bannerRect;
    private RectTransform sandwichTopRect;
    private RectTransform sandwichBottomRect;
    private float duration;
    private float timeElapsed;
    private Vector2 midPos;
    private Vector2 sandwichTopMidPos;
    private Vector2 sandwichBottomMidPos;
    private Player player;
    private Vector2 offset;
    private Vector2 sandwichOffset;

    public ScrollTurnBanner(Player player_)
    {
        player = player_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        turnBanner = Services.UIManager.turnBanner;
        bannerSandwichBottom = Services.UIManager.bannerSandwichBottom;
        bannerSandwichTop = Services.UIManager.bannerSandwichTop;
        duration = Services.UIManager.bannerScrollTime;
        offset = Services.UIManager.bannerOffset;
        sandwichOffset = Services.UIManager.sandwichOffset;

        turnBanner.SetActive(true);
        bannerSandwichBottom.SetActive(true);
        bannerSandwichTop.SetActive(true);

        bannerRect = turnBanner.GetComponent<RectTransform>();
        sandwichBottomRect = bannerSandwichBottom.GetComponent<RectTransform>();
        sandwichTopRect = bannerSandwichTop.GetComponent<RectTransform>();

        midPos = bannerRect.anchoredPosition;
        sandwichBottomMidPos = sandwichBottomRect.anchoredPosition;
        sandwichTopMidPos = sandwichTopRect.anchoredPosition;

        bannerRect.anchoredPosition = midPos - offset;
        sandwichBottomRect.anchoredPosition = sandwichBottomMidPos - sandwichOffset;
        sandwichTopRect.anchoredPosition = sandwichTopMidPos + sandwichOffset;

        Color newSolidColor;
        if (player != null)
        {
            turnBanner.GetComponentInChildren<Text>().text = player.name + " TURN";
            newSolidColor = (player.color * 0.7f) + (Color.white * 0.3f);
        }
        else
        {
            turnBanner.GetComponentInChildren<Text>().text = "ROUND END";
            newSolidColor = (Color.yellow * 0.7f) + (Color.white * 0.3f);
        }
        turnBanner.GetComponent<Image>().color = 
            new Color(newSolidColor.r, newSolidColor.g, newSolidColor.b, 0.9f);
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed <= duration / 2)
        {
            bannerRect.anchoredPosition = Vector2.Lerp(
                midPos - offset,
                midPos,
                Easing.ExpoEaseOut(timeElapsed / (duration / 2)));
            sandwichBottomRect.anchoredPosition = Vector2.Lerp(
                sandwichBottomMidPos - sandwichOffset,
                sandwichBottomMidPos,
                Easing.ExpoEaseOut(timeElapsed / (duration / 2)));
            sandwichTopRect.anchoredPosition = Vector2.Lerp(
                sandwichTopMidPos + sandwichOffset,
                sandwichTopMidPos,
                Easing.ExpoEaseOut(timeElapsed / (duration / 2)));
        }
        else
        {
            bannerRect.anchoredPosition = Vector2.Lerp(
                midPos, 
                midPos + offset,
                Easing.ExpoEaseIn((timeElapsed - (duration / 2)) / (duration / 2)));
            sandwichBottomRect.anchoredPosition = Vector2.Lerp(
                sandwichBottomMidPos,
                sandwichBottomMidPos - sandwichOffset,
                Easing.ExpoEaseIn((timeElapsed - (duration / 2)) / (duration / 2)));
            sandwichTopRect.anchoredPosition = Vector2.Lerp(
                sandwichTopMidPos,
                sandwichTopMidPos + sandwichOffset,
                Easing.ExpoEaseIn((timeElapsed - (duration / 2)) / (duration / 2)));
        }

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        turnBanner.SetActive(false);
        bannerSandwichBottom.SetActive(false);
        bannerSandwichTop.SetActive(false);
   
        bannerRect.anchoredPosition = midPos;
        sandwichBottomRect.anchoredPosition = sandwichBottomMidPos;
        sandwichTopRect.anchoredPosition = sandwichTopMidPos;
   }


}
