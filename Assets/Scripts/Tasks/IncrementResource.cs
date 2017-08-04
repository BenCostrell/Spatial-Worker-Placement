using UnityEngine;
using System.Collections;

public class IncrementResource : Task
{
    private float duration;
    private float timeElapsed;
    private Resource resource;
    private Vector3 initialPos;
    private Vector3 initialScale;
    private bool incremented;

    public IncrementResource(Resource resource_)
    {
        resource = resource_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        incremented = false;
        duration = resource.incrementAnimTime;
        initialPos = resource.transform.position;
        initialScale = resource.transform.localScale;
        resource.animating = true;
        resource.sr.color = Color.green;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed <= duration / 2)
        {
            resource.transform.position = Vector3.Lerp(
                initialPos,
                initialPos + resource.incrementDist * Vector3.up,
                Easing.QuadEaseOut(timeElapsed / (duration / 2)));
            resource.transform.localScale = Vector3.Lerp(
                initialScale,
                initialScale * resource.incrementScale,
                Easing.QuadEaseOut(timeElapsed / (duration / 2)));
        }
        else
        {
            if (!incremented)
            {
                resource.Increment();
                incremented = true;
            }
            resource.transform.position = Vector3.Lerp(
                initialPos + resource.incrementDist * Vector3.up,
                initialPos,
                Easing.QuadEaseOut((timeElapsed - (duration / 2)) / (duration / 2)));
            resource.transform.localScale = Vector3.Lerp(
                initialScale * resource.incrementScale,
                initialScale,
                Easing.QuadEaseOut((timeElapsed - (duration / 2)) / (duration / 2)));
        }

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        resource.animating = false;
        resource.sr.color = Color.white;
    }
}
