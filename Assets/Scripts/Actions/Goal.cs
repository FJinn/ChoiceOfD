using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal
{
    public enum EGoal
    {
        None = 0,
        KillCharacter = 1,
        HealCharacter = 2
    }

    EGoal goalType;
    CharacterBase targetCharacter;
    int targetHealthPoint;

    public EGoal GetGoalType()
    {
        return goalType;
    }

    public void SetGoal(EGoal goal, CharacterBase character, int targetHP)
    {
        goalType = goal;
        targetCharacter = character;
        targetHealthPoint = targetHP;
    }

    public bool IsGoalAchieved()
    {
        switch(goalType)
        {
            case EGoal.None:
                break;
            case EGoal.KillCharacter:
                return targetCharacter.GetHealth() <= targetHealthPoint;
            case EGoal.HealCharacter:
                return targetCharacter.GetHealth() >= targetHealthPoint;
        }
        return false;
    }
}
