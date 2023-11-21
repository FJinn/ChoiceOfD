using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Action_ShieldSelf : ActionBase
{
    [SerializeField] int defaultRemainingTurns;

    protected override void MainAction_Implementation(Action callback)
    {
        instigator.AddCondition(new CharacterBase.ConditionInfo()
        {
            buff = CharacterBase.EBuffType.Shield,
            remainingTurns = defaultRemainingTurns
        });
        callback?.Invoke();
    }

    protected override void SubAction_Implementation(Action callback)
    {
        Debug.Log($"There is not SubAction for this action: {actionName}.");
        callback?.Invoke();
    }
}