using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "ScriptableObjects/GameEvent", order = 2)]
public class GameEventData : ScriptableObject
{
    public List<GameEventInfo> gameEventInfos;
}

[Serializable]
public class GameEventInfo
{
    public string eventName = "";
    public string eventDescription = "";
    public RoomTileInfo roomTileInfo;

    [HideInInspector] public EGameEvent eventType;

    [Header("Combat")]
    public int basicEnemyAmount;

    [Header("Leave Room")]
    public EventSelectionUI.EventChoiceParams[] leaveRoomEventChoiceParams;
}

public enum EGameEvent
{
    None = 0,
    Scout = 1,
    Campfire = 2,
    Combat = 3
}
