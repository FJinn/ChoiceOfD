using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>
{
    public static Action<ActionData> onEquipAction;
    public static Action<ActionData> onUnequipAction;
    public static Action<List<ECharacterClass>> onSelectActionToTakeDamage;
    public static Action<List<ECharacterClass>> onSelectActionToSwap;
    public static Action<List<ECharacterClass>> onSelectActionToUse;
    public static Action<ActionBase> onWaitToSelectTarget;

    [SerializeField] CharacterClassData allPlayerCharactersData;

    [SerializeField] PlayerInput playerInput;
    [SerializeField] PlayerParty playerParty;
    [SerializeField, ReadOnly] PlayerCharacter[] playerCharacters = new PlayerCharacter[4];
    public PlayerCharacter[] GetAllPlayerCharacters() => playerCharacters;
    PlayerCharacter GetPlayerCharacterWithClass(ECharacterClass characterClass) => playerCharacters.Find(x => x.GetCharacterClassInfo().characterClassType == characterClass);

    PlayerCharacter currentCharacter;
    List<CharacterBase> currentTargets = new();
    int characterEnteredRoomCount;
    Action charactersEnteredRoomCallback;
    static bool canSelectCooldownActionData = true;

    static List<ECharacterClass> currentActionSelectionRequiredClasses = new();
    static ActionData selectedActionData;
    int currentReceivingDamage;
    Coroutine selectActionDataRoutine;
    Coroutine selectTargetsRoutine;

    public void AddHealth(int amount, List<ECharacterClass> targetClasses, Action callback)
    {
        GameManager.Instance.PauseCombat();

        currentActionSelectionRequiredClasses.Clear();
        if(targetClasses != null)
        {
            foreach(ECharacterClass item in targetClasses)
            {
                currentActionSelectionRequiredClasses.Add(item);
            }
        }

        WaitToSelectActionData(()=>
        {
            selectedActionData.AddHealth(amount);
            PlayerCharacter selectedCharacter = GetPlayerCharacterWithClass(selectedActionData.belongToCharacterClass);
            VFXManager.Instance.PlayVFX(VFXManager.EGeneralVFXType.GainHealth, selectedCharacter.transform.position, ()=>
            {
                selectedActionData = null;
                GameManager.Instance.ResumeCombat();
                callback?.Invoke();
            });

            /*
            WaitToSelectTargets(()=>
            {
                selectedActionData.AddHealth(amount);
                selectedActionData = null;
                GameManager.Instance.ResumeCombat();
                callback?.Invoke();
            });
            */
        });
    }

    public void ReduceHealth(int reduceAmount, List<ECharacterClass> targetClasses, Action callback)
    {
        GameManager.Instance.PauseCombat();

        currentReceivingDamage = reduceAmount;
        
        currentActionSelectionRequiredClasses.Clear();
        if(targetClasses != null)
        {
            foreach(ECharacterClass item in targetClasses)
            {
                currentActionSelectionRequiredClasses.Add(item);
            }
        }
        onSelectActionToTakeDamage?.Invoke(currentActionSelectionRequiredClasses);

        WaitToSelectActionData(()=>
        {
            ActionTakeDamage();
            callback?.Invoke();
        });
    }

    public static void SelectActionData(ActionData target)
    {
        if(!canSelectCooldownActionData && target.IsInCooldown())
        {
            Debug.LogError("SelectActionData failed:: " + !canSelectCooldownActionData + " :: " + target.IsInCooldown());
            return;
        }
        selectedActionData = target;
    }

    public void Initialize()
    {
    }

    // ToDo:: better structure
    public void AllCharactersSpawnInTavern()
    {
        foreach(PlayerCharacter character in playerCharacters)
        {
            character.SpawnInTavern();
        }
    }

    public void EquipAction(ActionBase _action, ECharacterClass targetClass)
    {
        if(!playerParty.IsCharacterEquipSlotFull(targetClass))
        {
            playerParty.EquipAction(_action, targetClass);
            return;
        }
        
        currentActionSelectionRequiredClasses.Clear();
        currentActionSelectionRequiredClasses.Add(targetClass);
        onSelectActionToSwap?.Invoke(currentActionSelectionRequiredClasses);
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

    public void ActionTakeDamage()
    {
        ActionData currentActionData = selectedActionData;
        bool isActionDead = currentActionData.ReduceHealth(currentReceivingDamage);
        currentReceivingDamage = 0;

        if(!isActionDead)
        {
            PlayerCharacter selectedCharacter = GetPlayerCharacterWithClass(selectedActionData.belongToCharacterClass);
            VFXManager.Instance.PlayVFX(VFXManager.EGeneralVFXType.LoseHealth, selectedCharacter.transform.position, ()=>
            {
                GameManager.Instance.ResumeCombat();
                selectedActionData = null;
            });

            return;
        }

        playerParty.RemoveAction(currentActionData);
        selectedActionData = null;

        if(playerParty.characterInPartyCount <= 0)
        {
            KillCharacter();
            return;
        }

        GameManager.Instance.ResumeCombat();
    }

    public void EnterRoom(RoomTile roomTile, Action callback, bool preplacedInRoom, params Transform[] enterTransforms)
    {
        charactersEnteredRoomCallback = callback;
        characterEnteredRoomCount = 0;
        for(int i=0; i<playerCharacters.Length; ++i)
        {
            // Debug.LogError(playerCharacters[i] + " entering room!");
            playerCharacters[i].EnterRoom(roomTile, ()=>AllCharactersEnteredRoom(), enterTransforms[i], preplacedInRoom);
        }
    }

    // enterRoom callback only call once instead of multiple times
    void AllCharactersEnteredRoom()
    {
        characterEnteredRoomCount += 1;
        int totalEnteredRoom = playerCharacters.FindAll(x => x.GetCharacterClassInfo() != null).Length;
        if(characterEnteredRoomCount < totalEnteredRoom)
        {
            return;
        }

        charactersEnteredRoomCallback?.Invoke();

        charactersEnteredRoomCallback = null;
        characterEnteredRoomCount = 0;
    }

    public void SetCurrentCharacter(PlayerCharacter character)
    {
        currentCharacter = character;
    }

    public void ClearTargets()
    {
        currentTargets.Clear();
    }

    public void SelectAction()
    {
        Debug.Log("Player:: Select Action");
        currentActionSelectionRequiredClasses.Clear();
        onSelectActionToUse?.Invoke(currentActionSelectionRequiredClasses);
        canSelectCooldownActionData = false;
        WaitToSelectActionData(()=>
        {
            switch(selectedActionData.action.GetSelectableTargetType())
            {
                case ActionBase.ESelectableTargetType.Action:
                    // ToDo:: able to select other action
                    DoAction(true);
                    canSelectCooldownActionData = true;
                    break;
                case ActionBase.ESelectableTargetType.Character:
                    WaitToSelectTargets(()=>
                    {
                        DoAction(false);
                        canSelectCooldownActionData = true;
                    });
                    break;
                case ActionBase.ESelectableTargetType.Self:
                    DoAction(true);
                    canSelectCooldownActionData = true;
                    break;
            }
            
        });
    }

    public void WaitToSelectTargets(Action callback)
    {
        Debug.Log("Player:: Select Targets");
        if(selectTargetsRoutine != null)
        {
            StopCoroutine(selectTargetsRoutine);
        }
        selectTargetsRoutine = StartCoroutine(WaitToSelectTargetsUpdate(()=>
        {
            callback?.Invoke();
        }));
    }

    // ToDo:: better structure
    public void ClickToSelectTargets(CharacterBase target)
    {
        if(currentTargets.Contains(target))
        {
            Debug.LogError($"Current targets containted {target}! This should not happen!");
            return;
        }

        if(target == null)
        {
            Debug.Log($"{target.gameObject} is added to player target");
            return;
        }
        currentTargets.Add(target);
    }

    public void ClearSelectTargets()
    {
        currentTargets.Clear();
    }

    IEnumerator WaitToSelectTargetsUpdate(Action callback)
    {
        onWaitToSelectTarget?.Invoke(selectedActionData.action);
        
        int targetNeeded = selectedActionData.action.GetTargetCounts();
        while(currentTargets.Count < targetNeeded)
        {
            yield return null;
        }
        callback?.Invoke();
    }

    public void DoAction(bool playerAsTarget)
    {
        if(selectedActionData == null)
        {
            Debug.LogError("There is no selected Action Data!");
            return;
        }
        Debug.Log("Player:: Do Action");
        if(playerAsTarget)
        {
            ClickToSelectTargets(playerCharacters[0]);
        }
        selectedActionData.action.SetTargets(currentTargets);

        Debug.Log(currentCharacter.name + " move to and do action :: " + selectedActionData.action.actionName);
        selectedActionData.DoAction(currentCharacter);
        selectedActionData = null;
        currentTargets.Clear();
        return;

        currentCharacter.MoveTo_Speed(currentTargets[0].forwardPosition, ()=>
        {
            selectedActionData.action.DoAction(currentCharacter);
            selectedActionData = null;
        });
    }

    public void ReduceAllActionDataCooldown(ECharacterClass characterClass, int amount = 1)
    {
        // playerParty.ReduceAllActionsCooldown(amount);
        playerParty.ReduceCharacterAllActionsCooldown(characterClass, amount);
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
}
