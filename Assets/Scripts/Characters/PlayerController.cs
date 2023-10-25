using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>, ICombat
{
    public static Action<ActionData> onSelectAction;
    public static Action<ActionData> onEquipAction;
    public static Action<ActionData> onRemoveAction;
    public static Action onSelectActionToRemove;

    [SerializeField] CharacterClassData allPlayerCharactersData;

    [SerializeField] PlayerInput playerInput;
    [SerializeField] PlayerParty playerParty;
    [SerializeField, ReadOnly] PlayerCharacter[] playerCharacters = new PlayerCharacter[4];
    public PlayerCharacter[] GetAllPlayerCharacters() => playerCharacters;

    PlayerCharacter currentCharacter;
    bool stopInput;

    CharacterBase currentTarget;
    int characterEnteredRoomCount;
    Action charactersEnteredRoomCallback;

    static ActionData selectedActionData;
    int currentReceivingDamage;
    Coroutine selectActionDataRoutine;

    public void ReduceHealth(int reduceAmount, Action callback)
    {
        GameManager.Instance.PauseCombat();

        currentReceivingDamage = reduceAmount;

        onSelectActionToRemove?.Invoke();

        WaitToSelectActionData(()=>
        {
            // update ui to remove UI type with current action data
            onSelectAction?.Invoke(selectedActionData);
        });
    }

    public static void SelectActionData(ActionData target)
    {
        selectedActionData = target;
    }

    public void Initialize()
    {
        SetStopInput(true);
    }

    public void EquipAction(ActionBase _action, ECharacterClass targetClass)
    {
        if(!playerParty.IsCharacterEquipSlotFull(targetClass))
        {
            playerParty.EquipAction(_action, targetClass);
            return;
        }

        WaitToSelectActionData(()=>
        {
            playerParty.RemoveAction(selectedActionData);

            playerParty.EquipAction(_action, targetClass);
            selectedActionData = null;
        });
    }

    void WaitToSelectActionData(Action callback)
    {
        if(selectActionDataRoutine != null)
        {
            StopCoroutine(selectActionDataRoutine);
        }
        selectActionDataRoutine = StartCoroutine(WaitToSelectActionDataUpdate(callback));
    }

    IEnumerator WaitToSelectActionDataUpdate(Action callback)
    {
        while(selectedActionData == null)
        {
            yield return null;
        }
        callback?.Invoke();
    }

    public void ObtainCharacter(ECharacterClass eCharacterClass, bool addToParty = true)
    {
        CharacterClassInfo characterClassInfo = allPlayerCharactersData.GetDefaultCharacterClassInfo(eCharacterClass);
        
        playerParty.ObtainCharacter(characterClassInfo);

        if(addToParty && playerParty.characterInPartyCount < 4)
        {
            AddCharacterIntoParty(characterClassInfo);
        }
    }

    public void AddCharacterIntoParty(ECharacterClass eCharacterClass)
    {
        bool success = playerParty.AddCharacterIntoParty(eCharacterClass);
        if(!success)
        {
            Debug.LogError($"Failed to add {eCharacterClass} into party! Skipping AddCharacterIntoParty()!");
            return;
        }
        CharacterClassInfo characterClassInfo = allPlayerCharactersData.GetDefaultCharacterClassInfo(eCharacterClass);
        PlayerCharacter targetSlot = playerCharacters.Find(x => !x.HasAssigned());
        targetSlot.AssignCharacterClassInfo(characterClassInfo);
        targetSlot.InitializePlayerCharacter(allPlayerCharactersData);
    }

    public void AddCharacterIntoParty(CharacterClassInfo characterClassInfo)
    {
        if(!playerParty.AddCharacterIntoParty(characterClassInfo.characterClassType))
        {
            Debug.LogError("Failed to AddCharacterIntoParty!");
            return;
        }

        PlayerCharacter targetSlot = playerCharacters.Find(x => !x.HasAssigned());
        targetSlot.AssignCharacterClassInfo(characterClassInfo);
        targetSlot.InitializePlayerCharacter(allPlayerCharactersData);
    }

    public void RemoveCharacterFromParty(ECharacterClass eCharacterClass)
    {
        playerParty.RemoveCharacterFromParty(eCharacterClass);
    }

    public int GetPlayerPartyCharactersCount() => playerParty.characterInPartyCount;

    public void ActionTakeDamage(InputAction.CallbackContext callbackContext)
    {
        ActionData currentActionData = selectedActionData;

        bool isActionDead = currentActionData.action.ReduceHealth(currentReceivingDamage);
        currentReceivingDamage = 0;

        if(!isActionDead)
        {
            GameManager.Instance.ResumeCombat();
            return;
        }

        playerParty.RemoveAction(currentActionData);

        if(playerParty.characterInPartyCount <= 0)
        {
            selectedActionData = null;
            KillCharacter();
            return;
        }

        GameManager.Instance.ResumeCombat();
    }

    public void EnterRoom(RoomTile roomTile, Action callback, params Transform[] enterTransforms)
    {
        charactersEnteredRoomCallback = callback;
        characterEnteredRoomCount = playerCharacters.Length;
        for(int i=0; i<playerCharacters.Length; ++i)
        {
            playerCharacters[i].EnterRoom(roomTile, ()=>AllCharacterEnteredRoom(), enterTransforms[i]);
        }
    }

    // enterRoom callback only call once instead of multiple times
    void AllCharacterEnteredRoom()
    {
        if(characterEnteredRoomCount < playerCharacters.Length)
        {
            return;
        }

        charactersEnteredRoomCallback?.Invoke();

        charactersEnteredRoomCallback = null;
        characterEnteredRoomCount = 0;
    }

    void SetStopInput(bool value)
    {
        stopInput = value;
    }

    public void DoAction()
    {
        if(stopInput)
        {
            return;
        }

        if(selectedActionData == null)
        {
            Debug.LogError("There is no selected Action Data!");
            return;
        }

        Debug.Assert(currentTarget != null);
        SetStopInput(true);

        currentCharacter.MoveTo_Speed(currentTarget.forwardPosition, ()=>
        {
            selectedActionData.action.DoAction(currentCharacter);
            selectedActionData = null;
        });
    }

    void KillCharacter()
    {
        currentCharacter.KillCharacter(()=>
        {
            if(playerParty.characterInPartyCount <= 0)
            {
                GameManager.Instance.GameOver();
            }
        });
    }
#region Combat
    public void IStartTurn()
    {
        SetStopInput(false);
    }

    public void ISelectAction()
    {
        SetStopInput(false);
    }

    public void ICleanUpAction()
    {
        currentCharacter.ICleanUpAction();
    }

    public void ITurnEnd()
    {
    }

    public void ICombatStarted()
    {
        SetStopInput(true);
    }
#endregion
}
