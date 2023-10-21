using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

public class RoomTile : MonoBehaviour
{
    [SerializeField, TypeSelector(typeof(ActionBase))] List<TypeSelector> containedActions;
    [SerializeField] Transform[] playersStartPoints;
    [SerializeField] Transform[] enemyStartPoints;

    RoomTileInfo currentRoomTileInfo;
    List<CharacterBase> charactersInRoom = new();

    public void Initialize(RoomTileInfo _roomTileInfo)
    {
        currentRoomTileInfo = _roomTileInfo;
        gameObject.SetActive(true);
    }

    public void Deinitialize()
    {
        CharactersExitRoom();
        gameObject.SetActive(false);
    }

    public RoomTileInfo GetCurrentRoomTileInfo() => currentRoomTileInfo;

    public Type GetPossibleActionTypeInRoomTile()
    {
        return containedActions[Random.Range(0, containedActions.Count)].selectedType;
    }

    public void PlayerCharactersEnterRoom(Action callback)
    {
        List<PlayerController> players = GameManager.Instance.GetAllPlayers();

        if(players.Count > playersStartPoints.Length)
        {
            Debug.LogError($"Has more players ({players.Count}) than players' start points ({playersStartPoints.Length})! Skipping CharactersEnterRoom()!");
            return;
        }
        int characterDoneEnterCount = 0;

        for(int i=0; i<players.Count; ++i)
        {
            players[i].EnterRoom(this, playersStartPoints[i], ()=>
            {
                characterDoneEnterCount+=1;
                if(characterDoneEnterCount == players.Count)
                {
                    callback?.Invoke();
                }
            });
            AddCharactersIntoList(players[i]);
        }
    }

    void CharactersExitRoom()
    {
        for(int i=0; i<charactersInRoom.Count; ++i)
        {
            charactersInRoom[i].ExitRoom(this);
        }
        charactersInRoom.Clear();
    }

    void AddCharactersIntoList(CharacterBase target)
    {
        if(charactersInRoom.Contains(target))
        {
            Debug.LogError($"Characters In Room List has contained {target}!");
            return;
        }

        charactersInRoom.Add(target);
    }

    public List<PlayerController> GetAllPlayersInRoom()
    {
        return GameManager.Instance.GetAllPlayers();
    }

    public void SpawnRoomObjects(Action callback)
    {
        int basicEnemyAmount = GameEvent.Instance.GetCurrentGameEventInfo().basicEnemyAmount;
        if(basicEnemyAmount <= 0)
        {
            callback?.Invoke();
            return;
        }

        int characterDoneEnterCount = 0;
        for(int i=0; i<basicEnemyAmount; ++i)
        {
            if(i > enemyStartPoints.Length-1)
            {
                Debug.LogError($"Haven't have the design for where spawning enemies more than enemy start points! At index: {i}");
                return;
            }
            BasicEnemy newBasicEnemy = SpawnManager.Instance.GetBasicEnemy();
            newBasicEnemy.Initialize();
            newBasicEnemy.EnterRoom(this, enemyStartPoints[i], ()=> 
            {
                characterDoneEnterCount+=1;
                if(characterDoneEnterCount == basicEnemyAmount)
                {
                    callback?.Invoke();
                }
            });
            AddCharactersIntoList(newBasicEnemy);
        }
    }

    public void TriggerCombat()
    {
        CombatManager combatManager = CombatManager.Instance;
        combatManager.SetCombatObjectsAmount(charactersInRoom.Count);
        
        foreach(var item in charactersInRoom)
        {
            combatManager.RegisterIntoCombat(item.gameObject);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(currentRoomTileInfo.center, currentRoomTileInfo.radius);
    }
#endif
}

[Serializable]
public class RoomTileInfo
{
    public float radius = 10f;

    public Vector3 center = Vector3.zero;
}
