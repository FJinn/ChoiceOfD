using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Action_SelfHeal : ActionBase
{
    protected override void MainAction_Implementation(Action callback)
    {
        instigator.AddHealth(1, null, callback);
    }

    protected override void SubAction_Implementation(Action callback)
    {
        Debug.Log($"There is not SubAction for this action: {actionName}.");
        callback?.Invoke();
    }
}
