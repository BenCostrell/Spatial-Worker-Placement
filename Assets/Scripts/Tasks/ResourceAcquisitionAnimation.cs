using UnityEngine;
using System.Collections;

public class ResourceAcquisitionAnimation : Task
{ 
    private readonly float duration = Services.main.resGainAnimDur;
    private float timeElapsed;
    private Tile tile;
    private int resourcesGained;
    private GameObject resourceGainTextObj;
    private TextMesh resourceGainText;
    private readonly Vector2 initialOffset = Services.main.resGainAnimOffset * Vector2.up;
    private readonly Vector2 travelDist = Services.main.resGainAnimDist * Vector2.up;
    private Vector2 initialPos;
    private Color initialColor;

    public ResourceAcquisitionAnimation(Tile tile_, int resourcesGained_)
    {
        tile = tile_;
        resourcesGained = resourcesGained_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        resourceGainTextObj = GameObject.Instantiate(Services.Prefabs.ResourceGainText,
            tile.hex.ScreenPos() + initialOffset, Quaternion.identity);
        initialPos = resourceGainTextObj.transform.position;
        resourceGainText = resourceGainTextObj.GetComponent<TextMesh>();
        resourceGainText.text = "+" + resourcesGained;
        initialColor = resourceGainText.color;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        resourceGainTextObj.transform.position = Vector2.Lerp(initialPos, initialPos + travelDist,
            Easing.QuadEaseOut(timeElapsed / duration));
        resourceGainText.color = new Color(initialColor.r, initialColor.g, initialColor.b,
            Mathf.Lerp(1, 0, Easing.QuadEaseIn(timeElapsed / duration)));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        GameObject.Destroy(resourceGainTextObj);
    }
}
