using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Action_SparkleBlast : ActionBase
{
    [Header("Sparkle Blast")]
    public int damage = 5;
    [Range(0f, 1f)] public float blindPercentage = 0.5f;
    public int blindTurns = 2;

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
                debuff = CharacterBase.EDebuffType.Blind,
                remainingTurns = blindTurns,
                reduceByTurns = true
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
