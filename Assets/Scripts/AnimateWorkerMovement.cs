using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AnimateWorkerMovement : Task
{
    private Worker worker;
    private Tile goal;
    private float timeElapsed;
    private float duration;

    private Tile start;

    public AnimateWorkerMovement(Worker worker_, Tile goal_, float duration_)
    {
        worker = worker_;
        goal = goal_;
        duration = duration_;
    }

    protected override void Init()
    {
        start = worker.currentTile;
        timeElapsed = 0;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        worker.transform.position = Vector2.Lerp(start.hex.ScreenPos(), goal.hex.ScreenPos(), 
            Easing.QuadEaseOut(timeElapsed / duration));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        worker.PlaceOnTile(goal);
    }
}
