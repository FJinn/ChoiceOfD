using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_ConfuseRay : ActionBase
{
    [Header("Confuse Ray")]
    [Range(0f, 1f)] public float dispelledByTurnPercentage = 0.5f;

    protected override void MainAction_Implementation(Action callback)
    { 
        if(instigator == null)
        {
            Debug.LogError($"Instigator: {instigator}");
            return;
        }
Debug.LogError(targets.Count);
        // if instigator is player
        for(int i=0; i<targets.Count; ++i)
        {
            targets[i].AddCondition(new CharacterBase.ConditionInfo
            {
                debuff = CharacterBase.EDebuffType.Confuse,
                percentageToBeDispelled = dispelledByTurnPercentage,
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
