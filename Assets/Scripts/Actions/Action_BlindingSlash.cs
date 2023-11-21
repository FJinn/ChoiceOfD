using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Action_BlindingSlash : ActionBase
{
    [Header("Blinding Slash")]
    public int damage = 5;
    [Range(0f, 1f)]public float blindPercentage = 0.3f;
    public int blindTurn = 1;

    [Header("Player")]
    public float hitShakeCameraIntensity = 1f;
    public float hitShakeCameraDuration = 0.2f;

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
            targets[i].ReduceHealth(damage, null, callback);

            if(ProbabilityManager.RandomPercentage(blindPercentage))
            {
                targets[i].AddCondition(new CharacterBase.ConditionInfo()
                {
                    debuff = CharacterBase.EDebuffType.Blind,
                    remainingTurns = blindTurn
                });
            }
        }

        if(instigator is PlayerCharacter)
        {
            RoomManager.Instance.ShakeRoomCamera(hitShakeCameraIntensity, hitShakeCameraDuration);
        }
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
