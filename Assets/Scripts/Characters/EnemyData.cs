using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyCombatData
{
    public float speed = 1f;
    List<CharacterBase> currentTargets = new();
    List<AggressionInfo> aggressionInfos = new();

    class AggressionInfo
    {
        public CharacterBase target;
        public int aggressionValue;
    }

    // temp
    public void RandomlyPickTargets(int count)
    {
        List<CharacterBase> tempList = new();
        foreach(var item in aggressionInfos)
        {
            tempList.Add(item.target);
        }

        for(int i=0; i<count; ++i)
        {
            CharacterBase found = tempList[Random.Range(0, tempList.Count)];
            currentTargets.Add(found);
            tempList.Remove(found);
        }
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

    public void IncreaseAggressionTowards(CharacterBase _target, int _aggressionValue = 1)
    {
        aggressionInfos.Find(x => x.target == _target).aggressionValue += _aggressionValue;
    }

    public int GetCurrentTargetsCount() => currentTargets.Count;
    public List<CharacterBase> GetCurrentTargets() => currentTargets;

    public void AddToCurrentTargets(CharacterBase target)
    {
        Debug.Assert(!currentTargets.Contains(target));

        currentTargets.Add(target);
    }

    public void RemoveFromCurrentTargets(CharacterBase target)
    {
        Debug.Assert(currentTargets.Contains(target));

        currentTargets.Remove(target);
    }

    // ToDo:: better & clearer structure
    // for now it is called when action clean up
    public void ClearCurrentTargets()
    {
        currentTargets.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentPos"></param>
    /// <param name="offsetLength"> length of the vector from target to current position</param>
    /// <returns></returns>
    public Vector3 GetGoToPos(Vector3 currentPos, float offsetLength = 1)
    {
        // CleanUp: Design changes, there is not movement and is complete turn based
        var currentTarget = currentTargets[0];
        Vector3 targetPos = currentTarget.transform.position;
        return targetPos + (currentPos - targetPos).normalized * (offsetLength - currentTarget.forwardLength);
    }

    public CharacterBase GetMostAggressionTarget()
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
}

public class AIPlanner
{
    EnemyCombatData enemyCombatData;

    public void InitializePlanner(EnemyCombatData _enemyCombatData)
    {
        enemyCombatData = _enemyCombatData;
    }

    public void UpdatePlannerGoal()
    {
        // ToDo:: different priorty planner such as healing, killing etc
        // ToDo:: pick targets
        //
        CharacterBase mostAggressionTarget = enemyCombatData.GetMostAggressionTarget();
        enemyCombatData.AddToCurrentTargets(mostAggressionTarget);
    }

    public ActionBase AssignAction(in List<ActionBase> availableActions)
    {
        ActionBase result = FindAction(availableActions);
        // ToDo:: better design
        enemyCombatData.RandomlyPickTargets(result.GetTargetCounts());
        result.SetTargets(enemyCombatData.GetCurrentTargets());
        return result;
    }

    ActionBase FindAction(in List<ActionBase> availableActions)
    {
        // ToDo: better AI Design
        List<ActionBase> possibleRootActions = availableActions;

        ActionBase result = possibleRootActions[Random.Range(0, possibleRootActions.Count)];
/*
        for(int i=1; i<possibleRootActions.Count; ++i)
        {
            if(possibleRootActions[i].GetActionWeight() < result.GetActionWeight())
            {
                result = possibleRootActions[i];
            }
        }
*/
        return result;
    }
}
