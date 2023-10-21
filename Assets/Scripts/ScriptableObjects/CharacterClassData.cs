using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterClass", menuName = "ScriptableObjects/CharacterClass", order = 1)]
public class CharacterClassData : ScriptableObject
{
    public CharacterClassInfo[] allCharacterClassesInfo;
}

[SerializeField]
public class CharacterClassInfo
{
    public ECharacterClass characterClassType;
    [TypeSelector(typeof(ActionBase))] public TypeSelector[] defaultActions;
    [ReadOnly] public PlayerController.ActionData[] equippedActions = new PlayerController.ActionData[3];

    public void EquipDefaultActions(PlayerController playerController)
    {
        for(int i=0; i< defaultActions.Length; ++i)
        {
            if(defaultActions[i] == null)
            {
                continue;
            }

            playerController.EquipAction(ActionsManager.Instance.GetAction(defaultActions[i]), characterClassType);
        }
    }

    public void EquipAction(PlayerController.ActionData actionData)
    {
        for(int i=0; i<equippedActions.Length; ++i)
        {
            if(equippedActions[i] == null)
            {
                equippedActions[i] = actionData;
                return;
            }
        }
    }

    public void UnEquipAction(PlayerController.ActionData actionData)
    {
        for(int i=0; i<equippedActions.Length; ++i)
        {
            if(equippedActions[i] == actionData)
            {
                equippedActions[i] = null;
                return;
            }
        }
    }

    public bool IsEquippedActionEmpty()
    {
        for(int i=0; i<equippedActions.Length; ++i)
        {
            if(equippedActions[i] != null)
            {
                return false;
            }
        }

        return true;
    }

    public bool IsEquippedActionsFull()
    {
        for(int i=0; i<equippedActions.Length; ++i)
        {
            if(equippedActions[i] == null)
            {
                return false;
            }
        }

        return true;
    }
}

public enum ECharacterClass
{
    None = 0,
    Fighter = 1,
    Hunter = 2,
    Mage = 3,
    Priestess = 4,
    Rogue = 5,
    Cleric = 6
}
