using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyFormationData", menuName = "ScriptableObjects/EnemyFormationData", order = 3)]
public class EnemyFormationData : ScriptableObject
{
    // ToDo:: better structure
    // currently it is assigned to room, EnemyFormationData should be assigned based on choosing which dungeon etc
    public List<EnemyFormation> enemyFormations;
}

[Serializable]
public class EnemyFormation
{
    /*
    public EEnemyType[] frontSlots = new EEnemyType[3];
    public EEnemyType[] middleSlots = new EEnemyType[3];
    public EEnemyType[] backSlots = new EEnemyType[3];
    */
    // spent hours and cannot bind it to visual element enum field properly
    // ToDo:: better structure
    public EEnemyType frontSlotA;
    public EEnemyType frontSlotB;
    public EEnemyType frontSlotC;

    public EEnemyType middleSlotA;
    public EEnemyType middleSlotB;
    public EEnemyType middleSlotC;

    public EEnemyType backSlotA;
    public EEnemyType backSlotB;
    public EEnemyType backSlotC;

    public int TotalEnemyCount()
    {
        int result = 0;

        result = frontSlotA != EEnemyType.None? result + 1 : result;
        result = frontSlotB != EEnemyType.None? result + 1 : result;
        result = frontSlotC != EEnemyType.None? result + 1 : result;

        result = middleSlotA != EEnemyType.None? result + 1 : result;
        result = middleSlotB != EEnemyType.None? result + 1 : result;
        result = middleSlotC != EEnemyType.None? result + 1 : result;

        result = backSlotA != EEnemyType.None? result + 1 : result;
        result = backSlotB != EEnemyType.None? result + 1 : result;
        result = backSlotC != EEnemyType.None? result + 1 : result;

        return result;
    }
}