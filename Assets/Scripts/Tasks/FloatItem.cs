using UnityEngine;
using System.Collections;

public class FloatItem : Task
{
    private Item item;
    private float moveCycleTimeElapsed;
    private bool goingUp;
    private float noiseMag;
    private float noiseSpeed;
    private int id;
    private float moveCycleDist;
    private float moveCyclePeriod;
    private Vector3 basePosition;

    public FloatItem(Item item_)
    {
        item = item_;
    }

    protected override void Init()
    {
        moveCycleTimeElapsed = 0;
        goingUp = true;
        noiseMag = Services.ItemConfig.FloatNoiseMag;
        noiseSpeed = Services.ItemConfig.FloatNoiseSpeed;
        moveCycleDist = Services.ItemConfig.FloatCycleDist;
        moveCyclePeriod = Services.ItemConfig.FloatCyclePeriod;
        id = item.id;
        basePosition = item.basePosition;
    }

    internal override void Update()
    {
        if (item.destroyed)
        {
            SetStatus(TaskStatus.Success);
            return;
        }
        if (goingUp) moveCycleTimeElapsed += Time.deltaTime;
        else moveCycleTimeElapsed -= Time.deltaTime;
        float horizontalOffset = -noiseMag / 2 + (noiseMag *
            Mathf.PerlinNoise(Time.time * noiseSpeed, 1000f * id));
        float verticalOffset = /*-noiseMag / 2 + (noiseMag *
            Mathf.PerlinNoise(Time.time * noiseSpeed, 5050f + 1000f * id));*/
            Mathf.Lerp(0, moveCycleDist,
            Easing.QuadEaseOut(moveCycleTimeElapsed / moveCyclePeriod));

        item.obj.transform.position = new Vector3(
            basePosition.x + horizontalOffset,
            basePosition.y + verticalOffset,
            basePosition.z);

        if (moveCycleTimeElapsed >= moveCyclePeriod && goingUp)
        {
            goingUp = false;
        }
        if (moveCycleTimeElapsed <= 0 && !goingUp)
        {
            goingUp = true;
        }
    }
}

