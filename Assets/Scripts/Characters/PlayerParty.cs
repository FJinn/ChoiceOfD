using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerParty : MonoBehaviour
{
    public static Action<CharacterClassInfo> onObtainedCharacter;
    public static Action<CharacterClassInfo> onAddCharacterToParty;
    public static Action<CharacterClassInfo> onRemoveCharacterFromParty;

    CharacterClassInfo[] charactersInParty = new CharacterClassInfo[4]{null, null, null, null};
    List<CharacterClassInfo> obtainedCharacters = new();

    public int characterInPartyCount {private set; get;} = 0;

    public void ObtainCharacter(CharacterClassInfo characterClassInfo)
    {
        Debug.Assert(!obtainedCharacters.Contains(characterClassInfo));
        if(obtainedCharacters.Contains(characterClassInfo))
        {
            Debug.LogError($"{characterClassInfo.characterClassType} has already existed in ObtainedCharacters list! Skipping ObtainCharacter()!");
            return;
        }

        obtainedCharacters.Add(characterClassInfo);
        onObtainedCharacter?.Invoke(characterClassInfo);
    }

    public List<CharacterClassInfo> GetObtainedCharacters() => obtainedCharacters;

    public bool AddCharacterIntoParty(ECharacterClass targetClass)
    {
        CharacterClassInfo found = obtainedCharacters.Find(x => x.characterClassType == targetClass);
        if(found == null)
        {
            Debug.LogError($"Could not find {targetClass} in obtainedCharacters! Skipping AddCharacterIntoParty()!");
            return false;
        }

        if(charactersInParty.Exists(x => x != null && x.characterClassType == targetClass))
        {
            Debug.LogError($"{targetClass} has already existed in the party! Returning false from AddCharacterIntoParty()!");
            return false;
        }

        for(int i=0; i<charactersInParty.Length; ++i)
        {
            if(charactersInParty[i] == null)
            {
                characterInPartyCount += 1;
                charactersInParty[i] = found;
                onAddCharacterToParty?.Invoke(found);
                charactersInParty[i].EquipDefaultActions();
                return true;
            }
        }

        return false;
    }

    public void RemoveCharacterFromParty(ECharacterClass eCharacterClass, bool isDead = false)
    {
        Debug.Assert(charactersInParty.Exists(x => x != null && x.characterClassType == eCharacterClass));

        if(isDead)
        {
            obtainedCharacters.RemoveAll(x => x.characterClassType == eCharacterClass);
        }

        for(int i=0; i<charactersInParty.Length; ++i)
        {
            if(charactersInParty[i] != null && charactersInParty[i].characterClassType == eCharacterClass)
            {
                characterInPartyCount -= 1;
                onRemoveCharacterFromParty?.Invoke(charactersInParty[i]);
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

    public CharacterClassInfo GetCharacterClass(ECharacterClass eCharacterClass)
    {
        return charactersInParty.Find(x => x.characterClassType == eCharacterClass);
    }

    public void EquipAction(ActionBase _action, ECharacterClass character)
    { 
        CharacterClassInfo foundInfo = charactersInParty.Find(x => x.characterClassType == character);
        Debug.Assert(foundInfo != null);

        foundInfo.EquipAction(_action);
    }

    public void RemoveAction(ActionData actionData)
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
