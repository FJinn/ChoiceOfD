using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class RoomTile : MonoBehaviour
{
    [SerializeField] float radius = 10f;
    [SerializeField] CinemachineVirtualCamera roomCam;

    [SerializeField] Vector3 center = Vector3.zero;
    [SerializeField] float roomTileHeight = 0.5f;
    [SerializeField] Transform[] playersStartPoints;
    [SerializeField] TargetLayout enemyLayout;

    List<CharacterBase> charactersInRoom = new();

    public float GetRadius() => radius;
    public Vector3 GetCenter() => center;
    public float halfRoomTileHeight => roomTileHeight * 0.5f;
    CinemachineBasicMultiChannelPerlin perlinNoise;

    Coroutine shakeCameraRoutine;

    int charactersExitedRoomAmount;

    void Awake()
    {
        perlinNoise = roomCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void Initialize(Action callback)
    {
        gameObject.SetActive(true);
        roomCam.m_Priority = 10;
        callback?.Invoke();
    }

    public void Deinitialize(Action callback)
    {
        CharactersExitRoom(()=>
        {
            gameObject.SetActive(false);
            roomCam.m_Priority = 0;
            callback?.Invoke();
        });
    }

    public void PlayerCharactersEnterRoom(bool preplacedInRoom, Action callback)
    {
        PlayerController player = GameManager.Instance.GetPlayer();
        int playerCharactersCount = player.GetPlayerPartyCharactersCount();
        if(playerCharactersCount > playersStartPoints.Length)
        {
            Debug.LogError($"Has more players ({playerCharactersCount}) than players' start points ({playersStartPoints.Length})! Skipping CharactersEnterRoom()!");
            return;
        }
        
        foreach(var item in player.GetAllPlayerCharacters())
        {
            if(item.GetCharacterClassInfo() == null)
            {
                continue;
            }
            AddCharactersIntoList(item);
        }
        
        player.EnterRoom(this, ()=>
        {
            callback?.Invoke();
        }, preplacedInRoom, playersStartPoints);
    }

    void CharactersExitRoom(Action callback)
    {
        charactersExitedRoomAmount = 0;
        for(int i=0; i<charactersInRoom.Count; ++i)
        {
            charactersInRoom[i].ExitRoom(this, ()=>OnCharacterExitsRoom(callback));
        }
    }

    void OnCharacterExitsRoom(Action callback)
    {
        charactersExitedRoomAmount += 1;
        if(charactersExitedRoomAmount < charactersInRoom.Count)
        {
            return;
        }

        charactersInRoom.Clear();
        callback?.Invoke();
    }

    public void AddCharactersIntoList(params CharacterBase[] targets)
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
    public void SpawnRoomObjects(bool isCombat, Action callback)
    {
        GameEventInfo eventInfo = GameEvent.Instance.GetCurrentGameEventInfo();
        if(eventInfo == null)
        {
            callback?.Invoke();
            return;
        }
        SpawnEnemiesIntoRoom(isCombat, callback);
    }

    void SpawnEnemiesIntoRoom(bool isCombat, Action callback)
    {
        if(!isCombat)
        {
            callback?.Invoke();
            return;
        }

        int characterDoneEnterCount = 0;
        for(int i=0; i<enemyLayout.totalSlots; ++i)
        {
            EEnemyType targetType = enemyLayout.GetEnemyTypeOfSlot(i);
            if(targetType == EEnemyType.None)
            {
                continue;
            }
            CharacterBase newEnemy = SpawnManager.Instance.GetEnemyObject(targetType).character;
            newEnemy.Initialize();
            newEnemy.EnterRoom(this, ()=> 
            {
                enemyLayout.LinkSpawnedCharacterToSlot(i, newEnemy);
                characterDoneEnterCount+=1;
                if(characterDoneEnterCount == enemyLayout.GetTotalEnemyCount())
                {
                    callback?.Invoke();
                }
            }, enemyLayout.GetEnemySlotTransform(i), true);
            AddCharactersIntoList(newEnemy);
        }
    }

    public void TriggerCombat()
    {
        CombatManager combatManager = CombatManager.Instance;
        combatManager.SetCombatObjectsAmount(charactersInRoom.Count);
        
        foreach(var item in charactersInRoom)
        {
            combatManager.RegisterIntoCombat(item);
        }
    }

    public void ShakeCamera(float intensity, float shakeTime)
    {
        perlinNoise.m_AmplitudeGain = intensity;
        if(shakeCameraRoutine != null)
        {
            StopCoroutine(shakeCameraRoutine);
        }
        shakeCameraRoutine = StartCoroutine(ShakeWaitTime(shakeTime));
    }

    IEnumerator ShakeWaitTime(float shakeTime)
    {
        float waitTime = 0;
        while(waitTime < shakeTime)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }
        ResetShakeIntensity();
    }

    void ResetShakeIntensity()
    {
        perlinNoise.m_AmplitudeGain = 0;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(center, radius);
        Gizmos.DrawIcon(center, "centerPoint");
    }
#endif
}
