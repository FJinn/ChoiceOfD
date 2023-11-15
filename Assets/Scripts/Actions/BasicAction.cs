using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAction : ActionBase
{
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
            targets[i].ReduceHealth(1, null, callback);
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
