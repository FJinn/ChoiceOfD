using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Action_SleepDart : ActionBase
{
    [Header("Sleep Dart")]
    [Range(0f,1f)] public float baseDispellPercentage = 0.2f;
    [Range(0f,1f)] public float dispellPercentageIncrementByTurn = 0.1f;

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
            targets[i].AddCondition(new CharacterBase.ConditionInfo()
            {
                debuff = CharacterBase.EDebuffType.Sleep,
                remainingTurns = 1,
                reduceByTurns = false,
                percentageToBeDispelled = baseDispellPercentage,
                percentageToBeDispelledIncrementByTurn = dispellPercentageIncrementByTurn
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
