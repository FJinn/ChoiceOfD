using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : Singleton<RoomManager>
{
    [SerializeField] GameObject roomPrefab;
    [SerializeField] Transform roomGroupParent;
    [SerializeField] float roomTileHeight;

    List<RoomTileObjectInfo> spawnedRooms = new();
    RoomTile currentRoom;

    public float halfRoomTileHeight => roomTileHeight * 0.5f;

    class RoomTileObjectInfo
    {
        public GameObject obj;
        public RoomTile roomTile;
    }

    public void InitializeRoom(RoomTileInfo roomTileInfo)
    {
        if(currentRoom != null)
        {
            DeinitializeRoom();
        }

        RoomTileObjectInfo found = spawnedRooms.Find(x => !x.obj.activeInHierarchy);

        if(found != null)
        {
            found.roomTile.Initialize(roomTileInfo);
            currentRoom = found.roomTile;
            return;
        }

        GameObject newObj = Instantiate(roomPrefab, roomGroupParent);
        RoomTile newRoomTile = newObj.GetComponent<RoomTile>();
        RoomTileObjectInfo newInfo = new RoomTileObjectInfo(){obj = newObj, roomTile = newRoomTile};
        spawnedRooms.Add(newInfo);
        newRoomTile.Initialize(roomTileInfo);
        currentRoom = newRoomTile;
    }

    void DeinitializeRoom()
    {
        currentRoom.Deinitialize();
        currentRoom = null;
    }

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
