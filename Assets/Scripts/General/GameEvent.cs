using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvent : Singleton<GameEvent>
{
    public Action<GameEventInfo> onUpdateCurrentGameEventInfo;
    public Action onRoomReady;

    GameEventData currentEventData;
    GameEventInfo currentGameEventInfo;
    int currentEventDataIndex;

    public GameEventInfo GetCurrentGameEventInfo() => currentGameEventInfo;

    public void RestartEvent()
    {
        currentEventDataIndex = 0;
    }

    void InitializeEvent()
    {
        RoomManager roomManager = RoomManager.Instance;
        roomManager.PlayerCharactersEnterRoom(()=>
        {
            roomManager.SpawnObjectsInRoom(()=> 
            {
                onRoomReady?.Invoke();
            });
        });
    }

    public void ToDungeonEvent(GameEventData gameEventData)
    {
        currentEventData = gameEventData;
        currentGameEventInfo = currentEventData.gameEventInfos[0];
        currentGameEventInfo.eventType = gameEventData.firstEventType;
        onUpdateCurrentGameEventInfo?.Invoke(currentGameEventInfo);

        RoomManager roomManager = RoomManager.Instance;
        roomManager.InitializeCombatRoom();
        InitializeEvent();
    }

    public void StartEvent()
    {
        switch(currentGameEventInfo.eventType)
        {
            case EGameEvent.None:
                break;
            case EGameEvent.Scout:
                StartScoutEvent();
                break;
            case EGameEvent.Campfire:
                StartCampfireEvent();
                break;
            case EGameEvent.Combat:
                StartCombatEvent();
                break;
        }

        if(currentEventData.gameEventInfos.Count <= currentEventDataIndex + 1)
        {
            currentEventData = null;
            currentGameEventInfo = null;
        }
    }

    public void LeaveRoomSelection()
    {
        if(currentEventData == null)
        {
            Debug.LogError("There is no current event data! Skip LeaveRoomSelection()!");
            return;
        }

        int eventChoiceLength = currentGameEventInfo.leaveRoomEventChoiceParams.Length;
        if(eventChoiceLength > 3)
        {
            Debug.LogError($"There is more than 3 event choices! {eventChoiceLength}. Skipping LeaveRoomSelection()!");
            return;
        }
        if(eventChoiceLength <= 0)
        {
            Debug.LogError("There is no leave room event choice! Skipping LeaveRoomSelection()!");
            LeaveRoomImplementation(EGameEvent.None);
            return;
        }

        foreach(var item in currentGameEventInfo.leaveRoomEventChoiceParams)
        {
            item.callback = ()=>LeaveRoomImplementation(item.eventChoice);
        }

        EventSelectionUI eventSelectionUI = EventSelectionUI.Instance;
        eventSelectionUI.AddChoices(currentGameEventInfo.leaveRoomEventChoiceParams);
        eventSelectionUI.Activate();
    }

    public void LeaveRoomImplementation(EGameEvent targetEvent)
    {
        Debug.Assert(targetEvent != EGameEvent.None);
        ProcessEvent(targetEvent);
        RoomManager roomManager = RoomManager.Instance;
        roomManager.InitializeCombatRoom();

        InitializeEvent();
    }

    /// <summary>
    /// Use to choose which game event type should it be next
    /// </summary>
    void ProcessEvent(EGameEvent eventChoice)
    {
        currentEventDataIndex += 1;
        currentGameEventInfo = currentEventData.gameEventInfos[currentEventDataIndex];
        currentGameEventInfo.eventType = eventChoice;
        
        onUpdateCurrentGameEventInfo?.Invoke(currentGameEventInfo);
    }

    void StartCombatEvent()
    {
        RoomManager roomManager = RoomManager.Instance;
        roomManager.TriggerCurrentRoomCombat();
    }

    void StartScoutEvent()
    {
        Debug.Log("StartScoutEvent");

        // ToDo: handle stats
        // eventSelectionUI.onChoiceSelected +=;
    }

    void StartCampfireEvent()
    {
        Debug.Log("StartCampfireEvent");
    }
}
