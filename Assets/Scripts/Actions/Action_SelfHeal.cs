using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_SelfHeal : ActionBase
{
    protected override void MainAction_Implementation(Action callback)
    { 
        if(instigator == null)
        {
            Debug.LogError($"Instigator: {instigator}");
            return;
        }
        instigator.AddHealth(1);
        Debug.Log($"{instigator.name} uses Action: {actionName}.");
        callback?.Invoke();
    }

    protected override void SubAction_Implementation(Action callback)
    {
        Debug.Log($"There is not SubAction for this action: {actionName}.");
        callback?.Invoke();
    }
}
