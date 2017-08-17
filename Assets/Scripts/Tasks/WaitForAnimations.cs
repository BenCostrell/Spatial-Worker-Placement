using UnityEngine;
using System.Collections;

public class WaitForAnimations : Task
{
    internal override void Update()
    {
        if (Services.main.activeAnimations == 0) SetStatus(TaskStatus.Success);
    }
}
