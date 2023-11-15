using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Rendering;
using UnityEditor.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "CharacterClass", menuName = "ScriptableObjects/CharacterClass", order = 1)]
public class CharacterClassData : ScriptableObject
{
    [Serializable]
    public class CharacterPrefabInfo
    {
        public GameObject characterVisualPrefab;
        public ECharacterClass classOf;
        public Texture2D profileTexture;
    }

    public CharacterPrefabInfo[] allPlayerCharacterPrefabs;
    public CharacterClassInfoScriptableObject[] characterClassInfoScriptableObjects;

    // ToDo: better structure
    public GameObject GetPlayerCharacterPrefabWithClassType(ECharacterClass targetClass) => allPlayerCharacterPrefabs.Find(x => x.classOf == targetClass).characterVisualPrefab;
    public Texture2D GetPlayerCharacterTexture2DWithClassType(ECharacterClass targetClass) => allPlayerCharacterPrefabs.Find(x => x.classOf == targetClass).profileTexture;
    public CharacterClassInfo GetDefaultCharacterClassInfo(ECharacterClass targetClass) => characterClassInfoScriptableObjects.Find(x => x.characterClassInfo.characterClassType == targetClass).characterClassInfo;
}

[CreateAssetMenu(fileName = "CharacterClassInfo", menuName = "ScriptableObjects/CharacterClassInfo", order = 1)]
public class CharacterClassInfoScriptableObject : ScriptableObject
{
    public CharacterClassInfo characterClassInfo;
}

[Serializable]
public class CharacterClassInfo
{
    public ECharacterClass characterClassType;
    [TypeSelector(typeof(ActionBase))] public TypeSelector[] defaultActions = new TypeSelector[3];
    [ReadOnly] public ActionData[] equippedActions = new ActionData[3];
    [SerializeField, ReadOnly] List<TypeSelector> obtainedActions = new List<TypeSelector>();

    public void EquipDefaultActions()
    {
        for(int i=0; i< defaultActions.Length; ++i)
        {
            EquipAction(ActionsManager.Instance.GetAction(defaultActions[i]), true);
        }
    }

    public void ObtainAction(TypeSelector actionType, bool equip = false)
    {
        // ToDo:: better and clearer design
        obtainedActions.Add(actionType);

        if(!equip)
        {
            return;
        }

        EquipAction(ActionsManager.Instance.GetAction(actionType), true);
    }

    public void EquipAction(ActionBase _action, bool initializeActionData = false)
    {
        int targetIndex = -1;
        for(int i=0; i<equippedActions.Length; ++i)
        {
            if(equippedActions[i].action == null)
            {
                targetIndex = i;
                equippedActions[targetIndex].action = _action;
                break;
            }
        }

        if(initializeActionData)
        {
            equippedActions[targetIndex].Initialize();
        }
        equippedActions[targetIndex].action.InitializeAction();
        equippedActions[targetIndex].belongToCharacterClass = characterClassType;

        var sameActions = equippedActions.FindAll(x => x.action == equippedActions[targetIndex].action);
        equippedActions[targetIndex].obtainedIndex = sameActions != null && sameActions.Length > 0 ? sameActions.Length + 1 : 1;

        PlayerController.onEquipAction?.Invoke(equippedActions[targetIndex]);
    }

    public void UnEquipAction(ActionData actionData)
    {
        for(int i=0; i<equippedActions.Length; ++i)
        {
            if(equippedActions[i] == actionData)
            {
                equippedActions[i].action = null;
                equippedActions[i].belongToCharacterClass = ECharacterClass.None;
                
                PlayerController.onUnequipAction?.Invoke(actionData);
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

    public void ReduceEquippedActionsCooldown(int amount)
    {
        foreach(var item in equippedActions)
        {
            item.ReduceCooldownTurn(amount);
        }
    }
}

[Serializable]
public class ActionData
{
    public Action onHealthUpdate;
    public Action<int> onCooldownTurnChanged;

    public ActionBase action;
    public ECharacterClass belongToCharacterClass;
    public int obtainedIndex;

    public bool canBeSelected {get; private set;}
    public int currentHealth {get; private set;}
    public int currentCooldownTurn {get; private set;} = 0;
    public bool IsInCooldown() => currentCooldownTurn > 0;

    public void Initialize()
    {
        currentHealth = action.GetInitialHealth();
        canBeSelected = true;
    }

    public void SetCanBeSelected(bool value)
    {
        canBeSelected = value;
    }

    public void DoAction(CharacterBase _instigator)
    {
        action.DoAction(_instigator);
        currentCooldownTurn = action.GetDefaultCooldownTurn();
        onCooldownTurnChanged?.Invoke(currentCooldownTurn);
    }

    public void ReduceCooldownTurn(int amount = 1)
    {
        currentCooldownTurn -= amount;
        onCooldownTurnChanged?.Invoke(currentCooldownTurn);
    }

#region Health
    public bool AddHealth(int value)
    {
        Debug.Assert(value > 0);
        int resultValue = currentHealth + value;
        if(resultValue > action.GetMaxHealth())
        {
            return false;
        }
        currentHealth = resultValue;
        onHealthUpdate?.Invoke();
        return true;
    }

    /// <summary>
    /// return true if health > 0
    /// return false if health <= 0
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool ReduceHealth(int value)
    {
        Debug.Assert(value > 0);
        currentHealth -= value;
        onHealthUpdate?.Invoke();
        return currentHealth <= 0;
    }

#endregion
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
/*
#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(CharacterClassInfo))]
public class CharacterClassInfoUIE: PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement root = new VisualElement();

        PropertyField classType = new PropertyField(property.FindPropertyRelative("characterClassType"));
        PropertyField defaultActions = new PropertyField(property.FindPropertyRelative("defaultActions"));
        PropertyField equippedActions = new PropertyField(property.FindPropertyRelative("equippedActions"));

        root.Add(classType);
        root.Add(defaultActions);
        root.Add(equippedActions);

        return root;
    }
}
#endif
*/