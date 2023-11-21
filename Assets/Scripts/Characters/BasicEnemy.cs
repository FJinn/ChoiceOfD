using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : CharacterBase
{
    [Header("Basic Eenemy")]
    [SerializeField, TypeSelector(typeof(ActionBase))] List<TypeSelector> containedActions;

    List<ActionBase> availableActions = new();
    AIPlanner planner;
    EnemyCombatData enemyCombatData;

    public override void Initialize()
    {
        base.Initialize();
         
        foreach(Type t in containedActions)
        {
            ActionBase found = ActionsManager.Instance.GetAction(t);
            found.InitializeAction();
            availableActions.Add(found);
        }

        enemyCombatData = new EnemyCombatData(){speed = speed};
        transform.position = new Vector3(transform.position.x, lengthFromGround, transform.position.z);

        if(planner == null)
        {
            planner = new AIPlanner();
            planner.InitializePlanner(enemyCombatData);
        }
    }
    
    public override void EnterRoom(RoomTile roomTile, Action callback, Transform enterTransform, bool preplacedInRoom)
    {
        base.EnterRoom(roomTile, callback, enterTransform, preplacedInRoom);

        foreach(var item in PlayerController.Instance.GetAllPlayerCharacters())
        {
            enemyCombatData.AddAggressionTarget(item);
        }

        Vector3 targetPos = enterTransform.position;
        targetPos.y = lengthFromGround;
        if(preplacedInRoom)
        {
            movement.SetPositionAndRotation(enterTransform.position, enterTransform.rotation);
            movement.SetHasBoundary(true);
            callback?.Invoke();
            return;
        }

        characterCollider.enabled = false;
        movement.MoveTo_Duration(targetPos, 1f, ()=> 
        {
            movement.SetHasBoundary(true);
            characterCollider.enabled = true;
            callback?.Invoke();
        });
    }
    
    public override void SelectActionImplementation()
    {
        // temp: AssignAction handles everything, pick random action and pick random targets
        ActionBase action = planner.AssignAction(availableActions);

        if(action.GetTargetCounts() > 1)
        {
            hasMovedFromAction = false;
            action.SetTargets(enemyCombatData.GetCurrentTargets());
            action.DoAction(this);
            return;
        }
        // hasMovedFromAction = true;
        Debug.Log("Move To and do action:: " + action.actionName);
        action.DoAction(this);

        return;

        Vector3 goToPos = enemyCombatData.GetGoToPos(transform.position, forwardLength);
        // CleanUp: Design changes, there is not movement and is complete turn based
        movement.MoveTo_Speed(goToPos, enemyCombatData.speed, ()=>
        {
            action.DoAction(this);
        });
    }

    public override void ICleanUpAction()
    {
        base.ICleanUpAction();

        enemyCombatData.ClearCurrentTargets();
    }

    public override void ITurnEnd()
    {

    }

    public override void ICombatStarted()
    {
    }

    public override void KillCharacter(Action callback)
    {
        CombatManager.Instance.UnRegisterFromCombat(this);
        base.KillCharacter(callback);
    }
}
