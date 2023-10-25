using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.VisualScripting;

public class CardUI : Singleton<CardUI>
{
    [SerializeField] CardUIItem cardUIItemPrefab;
    [SerializeField] GameObject cardUICanvas;
    [SerializeField] GameObject cardPanelFrame;
    
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

        public void Deactivate()
        {
            transform.gameObject.SetActive(false);
            characterClass = null;
        }
    }

    List<CardUIItem> spawnedCards = new List<CardUIItem>();
    CardUIItem currentSelected;
    public bool isSelectToRemove {private set; get;}

    protected override void Awake()
    {
        base.Awake();

        foreach(var item in cardPanels)
        {
            item.Deactivate();
        }

        cardPanelFrame.SetActive(false);
        cardUICanvas.SetActive(false);
    }

    void OnEnable()
    {
        PlayerController.onSelectAction += OnUpdateCurrentAction;
        PlayerController.onEquipAction += OnAddAction;
        PlayerController.onRemoveAction += OnRemoveAction;
        PlayerController.onSelectActionToRemove += UpdateSelectionType;
        PlayerParty.onAddCharacterToParty += OnCharacterAddedToParty;
        PlayerParty.onRemoveCharacterFromParty += OnCharacterRemovedFromParty;
    }

    void OnDisable()
    {
        PlayerController.onSelectAction -= OnUpdateCurrentAction;
        PlayerController.onEquipAction -= OnAddAction;
        PlayerController.onRemoveAction -= OnRemoveAction;
        PlayerController.onSelectActionToRemove -= UpdateSelectionType;
        PlayerParty.onAddCharacterToParty -= OnCharacterAddedToParty;
        PlayerParty.onRemoveCharacterFromParty -= OnCharacterRemovedFromParty;
    }

    void UpdateSelectionType()
    {
        isSelectToRemove = true;
    }

    // temp till the ui/ux design is confirmed on how to select and use action
    public void ConfrimSelection()
    {
        PlayerController.SelectActionData(currentSelected.GetActionData());
    }

    public void SetCurrentSelectedCardUIItem(CardUIItem target)
    {
        if(currentSelected != null && currentSelected != target)
        {
            currentSelected.Deselect();
        }

        currentSelected = target;
    }

    void OnUpdateCurrentAction(ActionData currentActionData)
    {
        if(currentActionData == null)
        {
            currentSelected?.Deselect();
            return;
        }

        var found = spawnedCards.Find(x => x.IsActivated() && x.IsCachedActionData(currentActionData));
        
        if(found == null)
        {
            Debug.LogError($"Cannot find {currentActionData.action.actionName} in spawned card! Skipping function!");
            return;
        }
        found.Select();
    }

    void OnAddAction(ActionData target)
    {
        CardPanelInfo foundPanel = cardPanels.Find(x => x.characterClass.characterClassType == target.belongToCharacterClass);

        CardUIItem card = GetCardUIItem(foundPanel.transform);
        card.Initialize(target);
        spawnedCards.Add(card);
    }

    void OnRemoveAction(ActionData target)
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
        cardUICanvas.SetActive(true);
        cardPanelFrame.SetActive(true);

        CardPanelInfo found = cardPanels.Find(x => !x.IsActivated());
        Debug.Assert(found != null);
        found.Activate(character);
    }

    void OnCharacterRemovedFromParty(CharacterClassInfo character)
    {
        CardPanelInfo found = cardPanels.Find(x => x.characterClass == character);
        Debug.Assert(found != null);
        found.Deactivate();

        if(!cardPanels.Exists(x => x.IsActivated()))
        {
            cardPanelFrame.SetActive(false);
            cardUICanvas.SetActive(false);
        }
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
