using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

public class RoomTile : MonoBehaviour
{
    [SerializeField] float radius = 10f;

    [SerializeField] Vector3 center = Vector3.zero;
    [SerializeField] float roomTileHeight = 0.5f;
    [SerializeField] Transform[] playersStartPoints;
    [SerializeField] Transform[] enemyStartPoints;

    List<CharacterBase> charactersInRoom = new();

    public float GetRadius() => radius;
    public Vector3 GetCenter() => center;
    public float halfRoomTileHeight => roomTileHeight * 0.5f;

    public void Initialize()
    {
        gameObject.SetActive(true);
    }

    public void Deinitialize()
    {
        CharactersExitRoom();
        gameObject.SetActive(false);
    }

    public void PlayerCharactersEnterRoom(Action callback)
    {
        PlayerController player = GameManager.Instance.GetPlayer();
        int playerCharactersCount = player.GetPlayerPartyCharactersCount();
        if(playerCharactersCount > playersStartPoints.Length)
        {
            Debug.LogError($"Has more players ({playerCharactersCount}) than players' start points ({playersStartPoints.Length})! Skipping CharactersEnterRoom()!");
            return;
        }
        
        player.EnterRoom(this, ()=>
        {
            callback?.Invoke();
        }, playersStartPoints);
        AddCharactersIntoList(player.GetAllPlayerCharacters());
    }

    void CharactersExitRoom()
    {
        for(int i=0; i<charactersInRoom.Count; ++i)
        {
            charactersInRoom[i].ExitRoom(this);
        }
        charactersInRoom.Clear();
    }

    void AddCharactersIntoList(params CharacterBase[] targets)
    {
        foreach(var target in targets)
        {
            if(charactersInRoom.Contains(target))
            {
                Debug.LogError($"Characters In Room List has contained {target}!");
                return;
            }

            charactersInRoom.Add(target);
        }
    }

    // ToDo:: handle room without enemy
    public void SpawnRoomObjects(Action callback)
    {
        GameEventInfo eventInfo = GameEvent.Instance.GetCurrentGameEventInfo();
        if(eventInfo == null)
        {
            return;
        }
        SpawnEnemiesIntoRoom(eventInfo.basicEnemyAmount, callback);
    }

    void SpawnEnemiesIntoRoom(int amount, Action callback)
    {
        int basicEnemyAmount = amount;
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
            newBasicEnemy.EnterRoom(this, ()=> 
            {
                characterDoneEnterCount+=1;
                if(characterDoneEnterCount == basicEnemyAmount)
                {
                    callback?.Invoke();
                }
            }, enemyStartPoints[i]);
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
        Gizmos.DrawWireSphere(center, radius);
        Gizmos.DrawIcon(center, "centerPoint");
    }
#endif
}
