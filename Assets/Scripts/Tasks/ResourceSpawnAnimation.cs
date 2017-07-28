using UnityEngine;
using System.Collections;

public class ResourceSpawnAnimation : Task
{

    private float timeElapsed;
    private float duration;
    private Vector3 baseScale;
    private Resource resource;

    public ResourceSpawnAnimation(Resource resource_)
    {
        resource = resource_;
    }

    protected override void Init()
    {
        baseScale = resource.transform.localScale;
        resource.transform.localScale = Vector3.zero;
        timeElapsed = 0;
        duration = resource.spawnGrowTime;
        resource.sr.enabled = true;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;
        resource.transform.localScale = Vector3.Lerp(Vector3.zero, baseScale, 
            Easing.QuadEaseOut(timeElapsed / duration));
        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}
