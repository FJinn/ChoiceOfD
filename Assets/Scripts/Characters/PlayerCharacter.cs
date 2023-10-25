using System;
using System.Collections;
using System.Collections.Generic;
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

    static readonly int runningAnimID = Animator.StringToHash("IsRunning");
    static readonly int mainActionAnimID = Animator.StringToHash("MainAction");
    static readonly int defeatedAnimID = Animator.StringToHash("KneelingDefeat");
    static readonly int spawnAnimID = Animator.StringToHash("Spawn");

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

        GameObject targetPrefab = characterClassData.GetPlayerCharacterPrefabWithClassType(characterClassInfo.characterClassType); 
        SpawnVisualObject(targetPrefab);
    }
#region Spawn Object
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
        animContoller.PlayAnimation(spawnAnimID, ()=>MoveToInitialRoomPos(null));
    }
#endregion

#region Combat
    public override void IStartTurn()
    {
        Debug.LogWarning($"{characterClassInfo.characterClassType} started turn!");
    }

    public override void ISelectAction()
    {
        Debug.LogWarning($"{characterClassInfo.characterClassType} selected action!");
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

    public override void EnterRoom(RoomTile roomTile, Action callback, Transform enterTransform)
    {
        base.EnterRoom(roomTile, callback, enterTransform);

        Vector3 targetPos = enterTransform.position;
        targetPos.y = lengthFromGround;

        initialRoomPosition = targetPos;

        if(visualObject == null)
        {
            return;
        }
        MoveToInitialRoomPos(callback);
    }

    public override void ExitRoom(RoomTile roomTile)
    {
        base.ExitRoom(roomTile);

        isAtInitialRoomPos = false;
        isMovingToInitialRoomPos = false;
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
            callback?.Invoke();
        });
    }
    
    public override void KillCharacter(Action callback)
    {
        animContoller.PlayAnimation(defeatedAnimID, ()=>
        {
            CombatManager.Instance.UnRegisterFromCombat(gameObject);
            
            callback?.Invoke();
        });
    }

}
