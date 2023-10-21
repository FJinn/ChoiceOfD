using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardUI : Singleton<CardUI>
{
    [SerializeField] PlayerController character;
    [SerializeField] CardUIItem cardUIItemPrefab;
    [SerializeField] Vector2 cardbaseSize;
    [SerializeField] Vector2 cardSelectedSize;
    
    [Header("Class Card Zone")]
    [SerializeField] CardPanelInfo[] cardPanels;

    [Serializable]
    public class CardPanelInfo
    {
        public Transform transform;
        [ReadOnly] public CharacterClassInfo characterClass;

        public bool IsActivated() => transform.gameObject.activeInHierarchy;

        public void Activate(CharacterClassInfo _characterClass)
        {
            transform.gameObject.SetActive(true);
            characterClass = _characterClass;
        }
    }

    List<CardUIItem> spawnedCards = new List<CardUIItem>();
    CardUIItem currentSelected;
    bool isSelectToRemove;

    void OnEnable()
    {
        character.onSelectAction += OnUpdateCurrentAction;
        character.onEquipAction += OnAddAction;
        character.onRemoveAction += OnRemoveAction;
        character.onSelectActionToRemove += UpdateSelectionType;
        PlayerParty.onAddCharacterToParty += OnCharacterAddedToParty;
        PlayerParty.onRemoveCharacterFromParty += OnCharacterRemovedFromParty;
    }

    void OnDisable()
    {
        character.onSelectAction -= OnUpdateCurrentAction;
        character.onEquipAction -= OnAddAction;
        character.onRemoveAction -= OnRemoveAction;
        character.onSelectActionToRemove -= UpdateSelectionType;
        PlayerParty.onAddCharacterToParty -= OnCharacterAddedToParty;
        PlayerParty.onRemoveCharacterFromParty -= OnCharacterRemovedFromParty;
    }

    void UpdateSelectionType()
    {
        isSelectToRemove = true;
    }

    void OnUpdateCurrentAction(PlayerController.ActionData currentActionData)
    {
        if(currentActionData == null)
        {
            currentSelected?.DeselectAsCurrentAction(cardbaseSize);
            return;
        }

        var found = spawnedCards.Find(x => x.IsActivated() && x.IsCachedActionData(currentActionData));
        
        if(found == null)
        {
            Debug.LogError($"Cannot find {currentActionData.action.actionName} in spawned card! Skipping function!");
            return;
        }
        currentSelected?.DeselectAsCurrentAction(cardbaseSize);
        found.SelectAsCurrentAction(cardSelectedSize, isSelectToRemove);
        currentSelected = found;
    }

    void OnAddAction(PlayerController.ActionData target)
    {
        CardPanelInfo foundPanel = cardPanels.Find(x => x.characterClass.characterClassType == target.belongToCharacterClass);

        CardUIItem card = GetCardUIItem(foundPanel.transform);
        card.Initialize(cardbaseSize, target);
        spawnedCards.Add(card);
    }

    void OnRemoveAction(PlayerController.ActionData target)
    {
        var found = spawnedCards.Find(x => x.IsActivated() && x.IsCachedActionData(target));

        if(found == null)
        {
            Debug.LogError($"Cannot find {target.action.actionName} in spawned card! Skipping function!");
            return;
        }

        found.Deinitialize();
    }

    void OnCharacterAddedToParty(CharacterClassInfo character)
    {
        CardPanelInfo found = cardPanels.Find(x => !x.IsActivated());
        Debug.Assert(found != null);
        found.Activate(character);
    }

    void OnCharacterRemovedFromParty(CharacterClassInfo character)
    {

    }

    CardUIItem GetCardUIItem(Transform panelTransform)
    {
        CardUIItem found = spawnedCards.Find(x => !x.IsActivated());
        if(found != null)
        {
            return found;
        }

        CardUIItem newObj = Instantiate(cardUIItemPrefab, panelTransform);
        spawnedCards.Add(newObj);
        return newObj;
    }
}
