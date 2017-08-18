using UnityEngine;
using System.Collections;

public class DecrementBuilding : Task
{
    private float duration;
    private float timeElapsed;
    private Building building;
    private Vector3 initialScale;
    private bool decremented;
    private Color initialColor;

    public DecrementBuilding(Building building_)
    {
        building = building_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        decremented = false;
        duration = building.decrementAnimTime;
        initialScale = building.transform.localScale;
        initialColor = building.GetComponent<SpriteRenderer>().color;
        building.GetComponent<SpriteRenderer>().color = Color.magenta;
        if (building.controller == null) SetStatus(TaskStatus.Success);
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed <= duration / 2)
        {
            building.transform.localScale = Vector3.Lerp(
                initialScale,
                initialScale * Services.ItemConfig.DecrementScale,
                Easing.QuadEaseOut(timeElapsed / (duration / 2)));
        }
        else
        {
            if (!decremented)
            {
                building.Decrement();
                if(building.GetComponent<SpriteRenderer>().color == building.defaultColor)
                {
                    initialColor = building.defaultColor;
                }
                decremented = true;
            }
            building.transform.localScale = Vector3.Lerp(
                initialScale * Services.ItemConfig.DecrementScale,
                initialScale,
                Easing.QuadEaseOut((timeElapsed - (duration / 2)) / (duration / 2)));
        }

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        building.GetComponent<SpriteRenderer>().color = initialColor;
    }
}
