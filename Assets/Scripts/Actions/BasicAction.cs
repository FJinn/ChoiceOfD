using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAction : ActionBase
{
    protected override void MainAction_Implementation(Action callback)
    { 
        if(instigator == null)
        {
            Debug.LogError($"Instigator: {instigator}");
            return;
        }

        // if instigator is enemy, let player controller handles damage process
        if(instigator is not PlayerCharacter)
        {
            PlayerController.Instance.ReduceHealth(1, callback);

            return;
        }

        // if instigator is player
        for(int i=0; i<targets.Length; ++i)
        {
            targets[i].ReduceHealth(1);
        }
        callback?.Invoke();
    }

    protected override void SubAction_Implementation(Action callback)
    {
        if(instigator == null)
        {
            Debug.LogError($"Instigator: {instigator}");
            return;
        }

        ThrowAwayAction(instigator.transform.position, instigator.transform.forward * 5f);
        callback?.Invoke();
    }
}
