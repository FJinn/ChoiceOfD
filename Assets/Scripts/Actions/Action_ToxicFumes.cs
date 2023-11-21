using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Action_ToxicFumes : ActionBase
{
    [Header("Toxic Fumes")]
    public int poisonTurn = 1;
    public bool reduceByTurn = false;
    [Range(0f, 1f)]public float poisonReduceHealthPercentageByTurn = 0.1f;

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
                debuff = CharacterBase.EDebuffType.Posion,
                remainingTurns = poisonTurn,
                reduceByTurns = reduceByTurn,
                poisonReducePercentage = poisonReduceHealthPercentageByTurn
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