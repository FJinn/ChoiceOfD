using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionsManager : Singleton<ActionsManager>
{
    [SerializeField] List<ActionBase> allActions;

    public ActionBase GetAction(string actionName)
    {
        ActionBase found = allActions.Find(x => x.actionName == actionName);
        if(found != null)
        {
            return found;
        }

        Debug.LogError($"Could not find the action with name {actionName}! Returning null!");
        return null;
    }

    public ActionBase GetAction(Type actionType)
    {
        ActionBase found = allActions.Find(x => x.GetType() == actionType);
        if(found != null)
        {
            return found;
        }

        Debug.LogError($"Could not find the action with type {actionType}! Returning null!");
        return null;
    }
}
