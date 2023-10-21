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

        var hits = DetectStraightLine(requiredDistanceToTarget);
        if(hits != null && hits.Length > 0)
        {
            CharacterBase target = hits[0].transform.GetComponent<CharacterBase>();
            target.ReduceHealth(1);

            callback?.Invoke();
            return;
        }

        if(instigator is PlayerController)
        {
            callback?.Invoke();
        }
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
