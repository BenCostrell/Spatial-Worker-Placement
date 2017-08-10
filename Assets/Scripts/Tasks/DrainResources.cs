using UnityEngine;
using System.Collections;

public class DrainResources : Task
{
    private readonly float duration = Services.main.resGainAnimDur;
    private float timeElapsed;
    private Worker worker;
    private int resourcesDrained;
    private GameObject resourceDrainTextObj;
    private TextMesh resourceDrainText;
    private readonly Vector2 initialOffset = Services.main.resGainAnimOffset * Vector2.up;
    private readonly Vector2 travelDist = Services.main.resGainAnimDist * Vector2.up;
    private Vector2 initialPos;
    private Color initialColor;

    public DrainResources(Worker worker_, int resourcesDrained_)
    {
        worker = worker_;
    }

    protected override void Init()
    {
        if (resourcesDrained == 0)
        {
            SetStatus(TaskStatus.Success);
            return;
        }
        timeElapsed = 0;
        resourceDrainTextObj = GameObject.Instantiate(Services.Prefabs.ResourceGainText,
            worker.currentTile.hex.ScreenPos() + initialOffset, Quaternion.identity);
        initialPos = resourceDrainTextObj.transform.position;
        resourceDrainText = resourceDrainTextObj.GetComponent<TextMesh>();
        resourceDrainText.text = "-" + worker.mostRecentResourceAcquisition;
        resourceDrainText.color = Color.red;
        initialColor = resourceDrainText.color;
        worker.DrainResources(resourcesDrained);
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        resourceDrainTextObj.transform.position = Vector2.Lerp(initialPos, initialPos + travelDist,
            Easing.QuadEaseOut(timeElapsed / duration));
        resourceDrainText.color = new Color(initialColor.r, initialColor.g, initialColor.b,
            Mathf.Lerp(1, 0, Easing.QuadEaseIn(timeElapsed / duration)));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        GameObject.Destroy(resourceDrainTextObj);
    }
}
