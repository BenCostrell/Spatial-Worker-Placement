using UnityEngine;
using System.Collections;

public class ItemSpawnAnimation : Task
{

    private float timeElapsed;
    private float duration;
    private Vector3 baseScale;
    private Item item;

    public ItemSpawnAnimation(Item item_)
    {
        item = item_;
    }

    protected override void Init()
    {
        baseScale = item.obj.transform.localScale;
        item.obj.transform.localScale = Vector3.zero;
        timeElapsed = 0;
        duration = item.spawnGrowTime;
        item.obj.SetActive(true);
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;
        item.obj.transform.localScale = Vector3.Lerp(Vector3.zero, baseScale,
            Easing.QuadEaseOut(timeElapsed / duration));
        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}
