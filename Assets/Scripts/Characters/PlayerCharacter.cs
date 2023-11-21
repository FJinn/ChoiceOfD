using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCharacter : CharacterBase
{
    [SerializeField, ReadOnly] CharacterClassInfo characterClassInfo = null;
    [SerializeField, ReadOnly] GameObject visualObject;

    public CharacterClassInfo GetCharacterClassInfo() => characterClassInfo;
    public void AssignCharacterClassInfo(CharacterClassInfo classInfo) => characterClassInfo = classInfo;
    public bool HasAssigned() => characterClassInfo != null;

    bool isAtInitialRoomPos;
    bool isMovingToInitialRoomPos;

    static readonly int idleAnimID = Animator.StringToHash("HappyIdle");
    static readonly int runningAnimID = Animator.StringToHash("IsRunning");
    static readonly int mainActionAnimID = Animator.StringToHash("MainAction");
    static readonly int defeatedAnimID = Animator.StringToHash("KneelingDefeat");
    static readonly int sitIdleAnimID = Animator.StringToHash("SitIdle");
    static readonly int sitToStandAnimID = Animator.StringToHash("SitToStand");
    static readonly int waveHipHopDanceAnimID = Animator.StringToHash("WaveHipHopDance");
    int spawnAnimID;
    int leaveRoomAnimID;

    void Awake()
    {
        characterCollider.enabled = true;
        body.isKinematic = true;
        characterClassInfo = null;
    }
#region Movement
    public void MoveTo_Speed(Vector3 pos, Action callback)
    {
        movement.MoveTo_Speed(pos, speed, callback);
    }

    public void MoveTo_Duration(Vector3 pos, float duration, Action callback)
    {
        movement.MoveTo_Duration(pos, duration, callback);
    }

    public void SetBoundaries(Vector3 center, float radius)
    {
        movement.SetBoundaries(center, radius);
    }

    public void SetHasBoundary(bool value)
    {
        movement.SetHasBoundary(value);
    }
#endregion
    // use InitializePlayerCharacter
    public override void Initialize(){}

    /// <summary>
    /// should be called after room is spawned and ready
    /// </summary>
    /// <param name="characterClassData"></param>
    public void InitializePlayerCharacter(CharacterClassData characterClassData)
    {
        transform.position = new Vector3(transform.position.x, lengthFromGround, transform.position.z);

        SetTexture2D(characterClassData.GetPlayerCharacterTexture2DWithClassType(characterClassInfo.characterClassType));

        GameObject targetPrefab = characterClassData.GetPlayerCharacterPrefabWithClassType(characterClassInfo.characterClassType); 
        SpawnVisualObject(targetPrefab);
    }
#region Spawn
    void SpawnVisualObject(GameObject targetPrefab)
    {
        if(visualObject != null)
        {
            Destroy(visualObject);
        }

        visualObject = Instantiate(targetPrefab, transform);
        if(!visualObject.TryGetComponent(out Animator animator))
        {
            Debug.LogError($"Could not find Animator component in {this} with class:: {characterClassInfo.characterClassType}!");
            return;
        }
        animContoller.SetAnimator(animator);
        RoomManager.Instance.AddCharacterIntoCurrentRoom(this);

        animContoller.PlayAnimation(spawnAnimID, ()=>
        {
            if(spawnAnimID == sitIdleAnimID)
            {
                return;
            }
            MoveToInitialRoomPos(null);
        });
    }

    // ToDo:: better stucture
    public void SpawnInTavern()
    {
        spawnAnimID = sitIdleAnimID;
        leaveRoomAnimID = sitToStandAnimID;
    }
#endregion

#region Combat
    public override void IStartTurn(Action<bool> callback)
    {
        PlayerController playerController = PlayerController.Instance;
        playerController.SetCurrentCharacter(this);
        playerController.ReduceAllActionDataCooldown(characterClassInfo.characterClassType);
        base.IStartTurn(callback);
    }

    public override void SelectActionImplementation()
    {
        PlayerController.Instance.SelectAction();
    }

    public override void ICleanUpAction()
    {
        base.ICleanUpAction();
        Debug.LogWarning($"{characterClassInfo.characterClassType} clean up action!");
        PlayerController.Instance.SetCurrentCharacter(null);
        PlayerController.Instance.ClearTargets();
    }

    public override void ITurnEnd()
    {
        Debug.LogWarning($"{characterClassInfo.characterClassType} ended turn!");
    }

    public override void ICombatStarted()
    {
        Debug.LogWarning($"{characterClassInfo.characterClassType} started combat!");
    }
#endregion

    public override void EnterRoom(RoomTile roomTile, Action callback, Transform enterTransform, bool preplacedInRoom)
    {
        base.EnterRoom(roomTile, callback, enterTransform, preplacedInRoom);
        if(preplacedInRoom)
        {
            movement.SetPositionAndRotation(enterTransform.position, enterTransform.rotation);
            callback?.Invoke();
            return;
        }

        if(visualObject == null)
        {
            callback?.Invoke();
            return;
        }
        MoveToInitialRoomPos(callback);
    }

    public override void ExitRoom(RoomTile roomTile, Action callback)
    {
        // Debug.LogError($"{gameObject.name} exits room!");

        isAtInitialRoomPos = false;
        isMovingToInitialRoomPos = false;

        if(RoomManager.Instance.IsInTavern())
        {
            animContoller.PlayAnimation(leaveRoomAnimID, ()=>
            {
                animContoller.PlayAnimation(idleAnimID, null);
                callback?.Invoke();
            });
            return;
        }
        movement.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        callback?.Invoke();
    }

    void MoveToInitialRoomPos(Action callback)
    {
        if(isAtInitialRoomPos || isMovingToInitialRoomPos)
        {
            return;
        }
        SetHasBoundary(false);
        // ToDo: better structure
        isMovingToInitialRoomPos = true;
        MoveTo_Duration(initialRoomPosition, 1f, ()=> 
        {
            isAtInitialRoomPos = true;
            isMovingToInitialRoomPos = false;
            SetHasBoundary(true);
            movement.SetPositionAndRotation(initialRoomPosition, initialRoomRotation);
            callback?.Invoke();
        });
    }

    public override int GetHealth()
    {
        int totalHealth = 0;
        foreach(var item in characterClassInfo.equippedActions)
        {
            totalHealth += item.currentHealth;
        }
        return totalHealth;
    }

    public override void AddHealth(int addValue, List<ECharacterClass> specificClasses = null, Action callback = null)
    {
        PlayerController.Instance.AddHealth(addValue, specificClasses, callback);
    }

    public override void ReduceHealth(int reduceAmount, List<ECharacterClass> specificClasses = null, Action callback = null)
    {
        if(SkipReduceHealthEvaluation(reduceAmount))
        {
            callback?.Invoke();
            return;
        }
        PlayerController.Instance.ReduceHealth(reduceAmount, specificClasses, callback);
    }

    public override void ReduceHealthByPercentage(float percentage, List<ECharacterClass> specificClasses = null, Action callback = null)
    {
        PlayerController.Instance.ReduceHealthByPercentage(percentage, specificClasses, callback);
    }

    public override void KillCharacter(Action callback)
    {
        animContoller.PlayAnimation(defeatedAnimID, ()=>
        {
            CombatManager.Instance.UnRegisterFromCombat(this);
            
            callback?.Invoke();
        });
    }
}
