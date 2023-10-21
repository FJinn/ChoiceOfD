using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyCombatData
{
    public float speed = 1f;
    public float visionRadius;
    public CharacterBase currentTarget;
    Vector3 targetLastSeenPosition;
    List<AggressionInfo> aggressionInfos = new();
    class AggressionInfo
    {
        public CharacterBase target;
        public int aggressionValue;
    }

    public void AddAggressionTarget(CharacterBase _target, int _aggressionValue = 0)
    {
        if(aggressionInfos.Exists(x => x.target == _target))
        {
            Debug.LogWarning($"Aggression info list has contained target! Should not add it twice!");
            return;
        }

        aggressionInfos.Add(new AggressionInfo(){target = _target, aggressionValue = _aggressionValue});
    }

    public void UpdateCurrentTargetToMostAggressionTarget()
    {
        currentTarget = GetMostAggressionTarget();
    }

    public void UpdateTargetLastSeenPosition()
    {
        if (currentTarget != null && currentTarget.GetHealth() > 0)
        {
            targetLastSeenPosition = currentTarget.transform.position;
        }
        else
        {
            UpdateCurrentTargetToMostAggressionTarget();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentPos"></param>
    /// <param name="offsetLength"> length of the vector from target to current position</param>
    /// <returns></returns>
    public Vector3 GetGoToPos(Vector3 currentPos, float offsetLength = 1)
    {
        // ToDo: if there is obstacle
        // currently it is just straight line, need to be more dynamic, to look more intelligent
        if(currentTarget != null && currentTarget.GetHealth() > 0 && DetectCurrentTarget(currentPos))
        {
            Vector3 targetPos = currentTarget.transform.position;
            return targetPos + (currentPos - targetPos).normalized * (offsetLength - currentTarget.forwardLength);
        }
        Vector3 result = targetLastSeenPosition;
        result.y = currentPos.y;
        return result;
    }

    CharacterBase GetMostAggressionTarget()
    {
        AggressionInfo foundInfo = aggressionInfos[0];
        
        for(int i=0; i< aggressionInfos.Count; ++i)
        {
            if(aggressionInfos[i].target.GetHealth() <= 0)
            {
                continue;
            }

            foundInfo = aggressionInfos[i].aggressionValue > foundInfo.aggressionValue ? aggressionInfos[i] : foundInfo;
        }

        return foundInfo.target.GetHealth() <= 0 ? null : foundInfo.target;
    }

    bool DetectCurrentTarget(Vector3 currentPos)
    {
        Vector3 direction = (currentTarget.transform.position - currentPos).normalized;
        return Physics.Raycast(currentPos, direction, out RaycastHit hitInfo) && hitInfo.transform.gameObject == currentTarget.gameObject;
    }
}

public class AIPlanner
{
    EnemyCombatData enemyCombatData;
    public Goal currentGoal;

    public void InitializePlanner(EnemyCombatData _enemyCombatData)
    {
        enemyCombatData = _enemyCombatData;
    }

    public void UpdatePlannerGoal(Goal.EGoal goalType = Goal.EGoal.KillCharacter, int targetHP = 0)
    {
        if(currentGoal == null)
        {
            currentGoal = new Goal();
            currentGoal.SetGoal(goalType, enemyCombatData.currentTarget, targetHP);
            return;
        }

        if(currentGoal.IsGoalAchieved())
        {
            currentGoal.SetGoal(goalType, enemyCombatData.currentTarget, targetHP);
        }
    }

    public ActionBase AssignAction(in List<ActionBase> availableActions)
    {
        ActionBase result = FindAction(availableActions);
        return result;
    }

    ActionBase FindAction(in List<ActionBase> availableActions)
    {
        List<ActionBase> possibleRootActions = null;
        switch(currentGoal.GetGoalType())
        {
            case Goal.EGoal.None:
                break;
            case Goal.EGoal.KillCharacter:
                possibleRootActions = availableActions.FindAll(x => x.GetAllEffects().Contains("DamageCharacter"));
                break;
            case Goal.EGoal.HealCharacter:
                possibleRootActions = availableActions.FindAll(x => x.GetAllEffects().Contains("HealCharacter"));
                break;
        }


        if(possibleRootActions == null || possibleRootActions.Count <= 0)
        {
            Debug.LogError($"Doesn't have any action the can achieve the goal: {currentGoal.GetGoalType()}! Skipping!");
            return null;
        }
        
        ActionBase result = possibleRootActions[0];
        for(int i=1; i<possibleRootActions.Count; ++i)
        {
            if(possibleRootActions[i].GetActionWeight() < result.GetActionWeight())
            {
                result = possibleRootActions[i];
            }
        }

        return result;

/*      // maybe use GOAP to have more complex mechanics and behaviours
        List<KeyValuePair<int, ActionBase>> actionWithWeights = new();
        int currentIndex = 0;
        if(possibleRootActions[currentIndex].Precondition())
        {
            var target = new KeyValuePair<int, ActionBase>(possibleRootActions[currentIndex].GetActionWeight(), possibleRootActions[currentIndex]);
            actionWithWeights.Add(target);
        }
*/
    }
}
