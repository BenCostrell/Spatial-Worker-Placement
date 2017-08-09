using UnityEngine;
using System.Collections;

public class ExpandTooltip : Task
{
    private float timeElapsed;
    private float duration;
    private GameObject tooltip;
    private Vector3 targetScale;

    public ExpandTooltip(GameObject tooltip_)
    {
        tooltip = tooltip_;
        tooltip.SetActive(false);
    }

    protected override void Init()
    {
        if (tooltip == null)
        {
            SetStatus(TaskStatus.Aborted);
            return;
        }
        targetScale = tooltip.transform.localScale;
        timeElapsed = 0;
        duration = Services.UIManager.tooltipExpandTime;
        tooltip.transform.localScale = Vector3.zero;
        tooltip.SetActive(true);
    }

    internal override void Update()
    {
        if (tooltip == null)
        {
            SetStatus(TaskStatus.Aborted);
            return;
        }
        timeElapsed += Time.deltaTime;

        tooltip.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale,
            Easing.QuadEaseOut(timeElapsed / duration));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}
