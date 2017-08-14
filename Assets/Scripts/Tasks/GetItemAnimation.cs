using UnityEngine;
using System.Collections;

public class GetItemAnimation : Task
{
    private float timeElapsed;
    private float duration;
    private Item item;
    private Vector3 baseScale;
    private Vector3 initialPos;
    private Worker worker;

    public GetItemAnimation(Item item_, Worker worker_)
    {
        item = item_;
        worker = worker_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Services.ItemConfig.AcquireAnimTime;
        baseScale = item.obj.transform.localScale;
        initialPos = item.obj.transform.position;
        //resource.animating = true;
        Services.main.activeAnimations += 1;
    }

    internal override void Update()
    {
        timeElapsed = Mathf.Min(duration, timeElapsed + Time.deltaTime);
        Vector3 startPos;
        Vector3 targetPos;

        item.obj.transform.localScale = Vector3.LerpUnclamped(baseScale, Vector3.zero,
            Easing.BackEaseIn(timeElapsed / duration));
        if (timeElapsed <= duration / 2)
        {
            startPos = initialPos;
            targetPos = initialPos + Services.ItemConfig.AcquireOffset;
        }
        else
        {
            startPos = initialPos + Services.ItemConfig.AcquireOffset;
            targetPos = initialPos;
        }
        item.obj.transform.position = Vector3.Lerp(startPos, targetPos,
            Easing.QuadEaseOut(timeElapsed % (duration / 2) / (duration / 2)));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        //resource.animating = false;
        Services.main.activeAnimations -= 1;
    }
}
