using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerParty : MonoBehaviour
{
    public static Action<CharacterClassInfo> onAddCharacterToParty;
    public static Action<CharacterClassInfo> onRemoveCharacterFromParty;

    public PlayerController playerController;

    CharacterClassInfo[] charactersInParty = new CharacterClassInfo[4];
    List<CharacterClassInfo> obtainedCharacters = new();

    public int characterInPartyCount {private set; get;} = 0;

    void Awake()
    {
        playerController.onEquipAction += OnActionEquipped;
        playerController.onRemoveAction += OnActionRemoved;
    }

    void OnDestroy()
    {
        playerController.onEquipAction -= OnActionEquipped;
        playerController.onRemoveAction -= OnActionRemoved;
    }

    public void ObtainCharacter(CharacterClassInfo characterClassInfo)
    {
        Debug.Assert(!obtainedCharacters.Contains(characterClassInfo));

        obtainedCharacters.Add(characterClassInfo);
    }

    public bool AddCharacterIntoParty(CharacterClassInfo characterClassInfo)
    {
        for(int i=0; i<charactersInParty.Length; ++i)
        {
            if(charactersInParty[i] == null)
            {
                characterInPartyCount += 1;
                charactersInParty[i] = characterClassInfo;
                charactersInParty[i].EquipDefaultActions(playerController);
                return true;
            }
        }

        return false;
    }

    public void RemoveCharacterFromParty(ECharacterClass eCharacterClass, bool isDead = false)
    {
        Debug.Assert(charactersInParty.Contains(eCharacterClass));

        if(isDead)
        {
            obtainedCharacters.RemoveAll(x => x.characterClassType == eCharacterClass);
        }

        for(int i=0; i<charactersInParty.Length; ++i)
        {
            if(charactersInParty[i].characterClassType == eCharacterClass)
            {
                characterInPartyCount -= 1;
                charactersInParty[i] = null;
                return;
            }
        }
    }

    public bool IsCharacterEquipSlotFull(ECharacterClass eCharacterClass)
    {
        CharacterClassInfo foundInfo = charactersInParty.Find(x => x.characterClassType == eCharacterClass);
        Debug.Assert(foundInfo != null);

        return foundInfo.IsEquippedActionsFull();
    }

    void OnActionEquipped(PlayerController.ActionData actionData)
    { 
        CharacterClassInfo foundInfo = charactersInParty.Find(x => x.characterClassType == actionData.belongToCharacterClass);
        Debug.Assert(foundInfo != null);

        foundInfo.EquipAction(actionData);
    }

    void OnActionRemoved(PlayerController.ActionData actionData)
    {
        CharacterClassInfo foundInfo = charactersInParty.Find(x => x.characterClassType == actionData.belongToCharacterClass);
        Debug.Assert(foundInfo != null);

        foundInfo.UnEquipAction(actionData);
    
        // kill character if runs out of equipped actions
        if(foundInfo.IsEquippedActionEmpty())
        {
            RemoveCharacterFromParty(actionData.belongToCharacterClass, true);
        }
    }
}
