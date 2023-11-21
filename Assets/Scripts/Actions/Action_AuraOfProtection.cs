using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Action_AuraOfProtection : ActionBase
{
    [Header("Aura of Protection")]
    public int temporaryHealthPoint = 20;

    protected override void MainAction_Implementation(Action callback)
    { 
        if(instigator == null)
        {
            Debug.LogError($"Instigator: {instigator}");
            return;
        }

        // if instigator is player
        for(int i=0; i<targets.Count; ++i)
        {
            targets[i].AddCondition(new CharacterBase.ConditionInfo
            {
                buff = CharacterBase.EBuffType.TemporaryHP,
                remainingHealthPoint = temporaryHealthPoint,
                reduceByTurns = false
            });
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

        callback?.Invoke();
    }
}
