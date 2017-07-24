using UnityEngine;
using System.Collections;

public class WaitForAnimations : Task
{
    internal override void Update()
    {
        bool done = true;
        foreach(Player player in Services.GameManager.players)
        {
            foreach(Worker worker in player.workers)
            {
                if (worker.midAnimation)
                {
                    done = false;
                    break;
                }
            }
        }
        if (done) SetStatus(TaskStatus.Success);
    }
}
