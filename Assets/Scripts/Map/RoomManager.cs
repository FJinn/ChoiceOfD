using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : Singleton<RoomManager>
{
    [SerializeField] GameObject combatRoomPrefab;
    [SerializeField] GameObject tavernPrefab;
    [SerializeField] Transform roomGroupParent;

    List<RoomTileObjectInfo> spawnedRooms = new();
    RoomTile currentRoom;

    class RoomTileObjectInfo
    {
        public GameObject obj;
        public RoomTile roomTile;
    }

    public float GetCurrentRoomTileHalfHeight()
    {
        Debug.Assert(currentRoom != null);
        return currentRoom.halfRoomTileHeight;
    }

    // ToDo:: better structure
    public void InitializeTavernRoom()
    {
        InitializeRoom(tavernPrefab);
    }

    public void InitializeCombatRoom()
    {
        InitializeRoom(combatRoomPrefab);
    }

    public void InitializeRoom(GameObject roomPrefab)
    {
        if(currentRoom != null)
        {
            DeinitializeRoom();
        }

        RoomTileObjectInfo found = spawnedRooms.Find(x => !x.obj.activeInHierarchy && x.obj == roomPrefab);

        if(found != null)
        {
            found.roomTile.Initialize();
            currentRoom = found.roomTile;
            return;
        }

        GameObject newObj = Instantiate(roomPrefab, roomGroupParent);
        RoomTile newRoomTile = newObj.GetComponent<RoomTile>();
        RoomTileObjectInfo newInfo = new RoomTileObjectInfo(){obj = newObj, roomTile = newRoomTile};
        spawnedRooms.Add(newInfo);
        newRoomTile.Initialize();
        currentRoom = newRoomTile;
    }

    void DeinitializeRoom()
    {
        currentRoom.Deinitialize();
        currentRoom = null;
    }

    public RoomTile GetCurrentRoom() => currentRoom;
    public void PlayerCharactersEnterRoom(Action callback)
    {
        currentRoom.PlayerCharactersEnterRoom(callback);
    }

    public void SpawnObjectsInRoom(Action callback)
    {
        currentRoom.SpawnRoomObjects(callback);
    }

    public void TriggerCurrentRoomCombat()
    {
        currentRoom.TriggerCombat();
    }
}
