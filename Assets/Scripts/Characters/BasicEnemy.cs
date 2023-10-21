using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : CharacterBase
{
    [SerializeField, TypeSelector(typeof(ActionBase))] List<TypeSelector> containedActions;

    [SerializeField] float visionRadius;

    List<ActionBase> availableActions = new();
    AIPlanner planner;
    EnemyCombatData enemyCombatData;

    public override void ReduceHealth(int reduceAmount)
    {
        base.ReduceHealth(reduceAmount);

        health -= reduceAmount;

        if(health <= 0)
        {
            KillCharacter();
        }
    }

    public override void Initialize()
    {
        foreach(Type t in containedActions)
        {
            ActionBase found = ActionsManager.Instance.GetAction(t);
            found.InitializeAction();
            availableActions.Add(found);
        }

        enemyCombatData = new EnemyCombatData(){visionRadius = visionRadius, speed = speed};
        transform.position = new Vector3(transform.position.x, lengthFromGround, transform.position.z);

        if(planner == null)
        {
            planner = new AIPlanner();
            planner.InitializePlanner(enemyCombatData);
        }
    }
    
    public override void EnterRoom(RoomTile roomTile, Transform enterTransform, Action callback)
    {
        base.EnterRoom(roomTile, enterTransform, callback);

        foreach(var item in roomTile.GetAllPlayersInRoom())
        {
            enemyCombatData.AddAggressionTarget(item);
        }
        enemyCombatData.UpdateCurrentTargetToMostAggressionTarget();

        Vector3 targetPos = enterTransform.position;
        targetPos.y = lengthFromGround;
        characterCollider.enabled = false;
        movement.MoveTo_Duration(targetPos, 1f, ()=> 
        {
            movement.SetHasBoundary(true);
            characterCollider.enabled = true;
            callback?.Invoke();
        });
    }

    public override void ExitRoom(RoomTile roomTile)
    {
        base.ExitRoom(roomTile);
    }

    public override void IStartTurn()
    {
        Debug.LogWarning("IStartTurn not yet implemented!");
    }

    public override void ISelectAction()
    {
        enemyCombatData.UpdateCurrentTargetToMostAggressionTarget();

        Goal.EGoal eGoal = Goal.EGoal.KillCharacter;
        if(enemyCombatData.currentTarget == null)
        {
            eGoal = Goal.EGoal.HealCharacter;
            enemyCombatData.currentTarget = this;
        }
        planner.UpdatePlannerGoal(eGoal);
        // Debug.LogError(planner.currentGoal.GetGoalType());
        ActionBase action = planner.AssignAction(availableActions);
        float distanceToTarget = action.GetRequiredDistanceToTarget();
        if(distanceToTarget <= 0)
        {
            action.DoAction(this);
            return;
        }

        Vector3 goToPos = enemyCombatData.GetGoToPos(transform.position, forwardLength + distanceToTarget);
        // ToDo: Handle the case where goToPos is last seen position -> update goal and reselect action!
        movement.MoveTo_Speed(goToPos, enemyCombatData.speed, ()=>
        {
            // Debug.LogError($"Use Action: {action.actionName}!");
            // ToDo: check if player within sight, then handle the situation
            //    planner.UpdatePlannerGoal(Goal.EGoal.HealCharacter);
            //    action = planner.AssignAction(availableActions);
            action.DoAction(this);
        });
    }

    public override void ICleanUpAction()
    {
        Debug.LogWarning("IActionDone not yet implemented!");
    }

    public override void ITurnEnd()
    {
        enemyCombatData.UpdateTargetLastSeenPosition();
    }

    public override void ICombatStarted()
    {
    }

    public override void KillCharacter()
    {
        CombatManager.Instance.UnRegisterFromCombat(gameObject);
    }
}
