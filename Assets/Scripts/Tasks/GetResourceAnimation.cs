using UnityEngine;
using System.Collections;

public class GetResourceAnimation : Task
{
    private float timeElapsed;
    private float duration;
    private Resource resource;
    private Vector3 baseScale;
    private Vector3 initialPos;
    private Worker worker;

    public GetResourceAnimation(Resource resource_, Worker worker_)
    {
        resource = resource_;
        worker = worker_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = resource.acquireAnimDur;
        baseScale = resource.transform.localScale;
        initialPos = resource.transform.position;
        resource.animating = true;
        Services.main.activeAnimations += 1;
    }

    internal override void Update()
    {
        timeElapsed = Mathf.Min(duration, timeElapsed + Time.deltaTime);
        Vector3 startPos;
        Vector3 targetPos;

        resource.transform.localScale = Vector3.LerpUnclamped(baseScale, Vector3.zero,
            Easing.BackEaseIn(timeElapsed / duration));
        if (timeElapsed <= duration / 2)
        {
            startPos = initialPos;
            targetPos = initialPos + resource.acquireAnimOffsetY * Vector3.up;
        }
        else
        {
            startPos = initialPos + resource.acquireAnimOffsetY * Vector3.up;
            targetPos = initialPos;
        }
        resource.transform.position = Vector3.Lerp(startPos, targetPos,
            Easing.QuadEaseOut(timeElapsed % (duration / 2) / (duration / 2)));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        resource.animating = false;
        Services.main.activeAnimations -= 1;
    }
}
