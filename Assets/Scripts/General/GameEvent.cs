using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvent : Singleton<GameEvent>
{
    public Action<GameEventInfo> onUpdateCurrentGameEventInfo;
    public Action onRoomReady;

    public GameEventData gameEventData;
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

    public void ToFirstEvent()
    {
        currentGameEventInfo = gameEventData.gameEventInfos[0];
        onUpdateCurrentGameEventInfo?.Invoke(currentGameEventInfo);

        RoomManager roomManager = RoomManager.Instance;
        roomManager.InitializeRoom(currentGameEventInfo.roomTileInfo);
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
    }

    public void LeaveRoomSelection()
    {
        int eventChoiceLength = currentGameEventInfo.leaveRoomEventChoiceParams.Length;
        if(eventChoiceLength > 3)
        {
            Debug.LogError($"There is more than 3 event choices! {eventChoiceLength}. Skipping LeaveRoomSelection()!");
            return;
        }
        if(eventChoiceLength <= 0)
        {
            Debug.LogError("There is no leave room event choice! Skipping LeaveRoomSelection()!");
            LeaveRoomImplementation(null);
            return;
        }

        EventSelectionUI eventSelectionUI = EventSelectionUI.Instance;
        eventSelectionUI.AddChoices(currentGameEventInfo.leaveRoomEventChoiceParams);
        eventSelectionUI.Activate();
        eventSelectionUI.onChoiceSelected += LeaveRoomImplementation;
    }

    public void LeaveRoomImplementation(EventSelectionUI.ChoiceParams choiceParams)
    {
        EventSelectionUI.EventChoiceParams targetParams = choiceParams as EventSelectionUI.EventChoiceParams;
        EGameEvent targetEvent = targetParams == null ? EGameEvent.None : targetParams.eventChoice;
        ProcessEvent(targetEvent);
        EventSelectionUI.Instance.onChoiceSelected -= LeaveRoomImplementation;
        RoomManager roomManager = RoomManager.Instance;
        roomManager.InitializeRoom(currentGameEventInfo.roomTileInfo);

        InitializeEvent();
    }

    /// <summary>
    /// Use to choose which game event type should it be next
    /// </summary>
    void ProcessEvent(EGameEvent eventChoice)
    {
        currentEventDataIndex += 1;
        currentGameEventInfo = gameEventData.gameEventInfos[currentEventDataIndex];
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
