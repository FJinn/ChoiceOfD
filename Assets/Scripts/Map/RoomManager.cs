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
    RoomTileObjectInfo currentRoomInfo;

    class RoomTileObjectInfo
    {
        public GameObject obj;
        public RoomTile roomTile;
        public ERoomType roomType;
    }
    
    // ToDo:: Better structure
    public enum ERoomType
    {
        Tavern = 0,
        CombatRoom = 1
    }

    public float GetCurrentRoomTileGroundWorldPosY()
    {
        return currentRoomInfo.roomTile.halfRoomTileHeight + currentRoomInfo.roomTile.transform.position.y; //currentRoomInfo.roomTile.groundWorldPosY;
    }

    // ToDo:: better structure
    bool isInTavern;
    public bool IsInTavern()
    {
        return isInTavern;
    }

    // ToDo:: better structure
    public void InitializeTavernRoom(Action callback)
    {
        isInTavern = true;
        InitializeRoom(ERoomType.Tavern, callback);
    }

    public void InitializeCombatRoom(Action callback)
    {
        InitializeRoom(ERoomType.CombatRoom, ()=>
        {
            isInTavern = false;
            callback?.Invoke();
        });
    }

    public void InitializeRoom(ERoomType roomType, Action callback)
    {
        if(currentRoomInfo != null)
        {
            DeinitializeRoom(()=> 
            {
                IntializeRoom_Implementation(roomType, callback);
            });
            return;
        }
        IntializeRoom_Implementation(roomType, callback);
    }

    void IntializeRoom_Implementation(ERoomType _roomType, Action callback)
    {
        RoomTileObjectInfo found = spawnedRooms.Find(x => !x.obj.activeInHierarchy && x.roomType == _roomType);

        if(found != null)
        {
            currentRoomInfo = found;
            found.roomTile.Initialize(callback);
            return;
        }

        GameObject targetPrefab = _roomType switch
        {
            ERoomType.Tavern => tavernPrefab,
            ERoomType.CombatRoom => combatRoomPrefab,
            _ => null
        };

        GameObject newObj = Instantiate(targetPrefab, roomGroupParent);
        RoomTile newRoomTile = newObj.GetComponent<RoomTile>();
        RoomTileObjectInfo newInfo = new RoomTileObjectInfo(){obj = newObj, roomTile = newRoomTile, roomType = _roomType};
        spawnedRooms.Add(newInfo);
        currentRoomInfo = newInfo;
        newRoomTile.Initialize(callback);
    }

    void DeinitializeRoom(Action callback)
    {
        currentRoomInfo.roomTile.Deinitialize(()=>
        {
            currentRoomInfo = null;
            callback?.Invoke();
        });
    }

    public void PlayerCharactersEnterRoom(bool preplacedInRoom, Action callback)
    {
        currentRoomInfo.roomTile.PlayerCharactersEnterRoom(preplacedInRoom, callback);
    }

    public void AddCharacterIntoCurrentRoom(params CharacterBase[] characters)
    {
        currentRoomInfo.roomTile.AddCharactersIntoList(characters);
    }

    public void SpawnObjectsInRoom(bool isCombat, Action callback)
    {
        currentRoomInfo.roomTile.SpawnRoomObjects(isCombat,callback);
    }

    public void TriggerCurrentRoomCombat()
    {
        currentRoomInfo.roomTile.TriggerCombat();
    }

    public void ShakeRoomCamera(float intensity, float duration)
    {
        currentRoomInfo.roomTile.ShakeCamera(intensity, duration);
    }
}
