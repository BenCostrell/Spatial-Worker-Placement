using UnityEngine;
using System.Collections;

public class DecrementItem : Task
{
    private float duration;
    private float timeElapsed;
    private Item item;
    private Vector3 initialPos;
    private Vector3 initialScale;
    private bool decremented;

    public DecrementItem(Item item_)
    {
        item = item_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        decremented = false;
        duration = Services.ItemConfig.DecrementAnimTime;
        initialPos = item.obj.transform.position;
        initialScale = item.obj.transform.localScale;
        item.obj.GetComponent<SpriteRenderer>().color = Color.yellow;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed <= duration / 2)
        {
            item.obj.transform.position = Vector3.Lerp(
                initialPos,
                initialPos + Services.ItemConfig.DecrementDist * Vector3.up,
                Easing.QuadEaseOut(timeElapsed / (duration / 2)));
            item.obj.transform.localScale = Vector3.Lerp(
                initialScale,
                initialScale * Services.ItemConfig.DecrementScale,
                Easing.QuadEaseOut(timeElapsed / (duration / 2)));
        }
        else
        {
            if (!decremented)
            {
                item.DecrementCost();
                decremented = true;
                if (item.destroyed) SetStatus(TaskStatus.Success);
            }
            item.obj.transform.position = Vector3.Lerp(
                initialPos + Services.ItemConfig.DecrementDist * Vector3.up,
                initialPos,
                Easing.QuadEaseOut((timeElapsed - (duration / 2)) / (duration / 2)));
            item.obj.transform.localScale = Vector3.Lerp(
                initialScale * Services.ItemConfig.DecrementScale,
                initialScale,
                Easing.QuadEaseOut((timeElapsed - (duration / 2)) / (duration / 2)));
        }

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        if (!item.destroyed) item.obj.GetComponent<SpriteRenderer>().color = Color.white;
    }
}
