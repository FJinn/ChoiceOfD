using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.UIElements;

using Button = UnityEngine.UIElements.Button;

public class CardUI : MonoBehaviour
{
    public UIDocument uiDocument;
    [SerializeField] CardUIItem cardUIItem;

    public bool isSelectToTakeDamage {private set; get;}

    Button confirmButton;

    static List<ECharacterClass> selectableClasses;
    public bool IsSelectableClass(ECharacterClass target)
    {
        return selectableClasses == null || selectableClasses.Count <= 0 || selectableClasses.Exists(x => x == target);
    }

    void Awake()
    {
        confirmButton = uiDocument.rootVisualElement.Q<Button>("ConfirmButton");
        confirmButton.style.display = DisplayStyle.None;
    }

    void OnEnable()
    {
        PlayerController.onEquipAction += OnAddAction;
        PlayerController.onUnequipAction += OnRemoveAction;
        PlayerController.onSelectActionToTakeDamage += SelectToTakeDamage;
        PlayerController.onSelectActionToSwap += SelectActionToSwap;
        PlayerController.onSelectActionToUse += SelectActionToUse;
        PlayerParty.onAddCharacterToParty += OnCharacterAddedToParty;
        PlayerParty.onRemoveCharacterFromParty += OnCharacterRemovedFromParty;
    }

    void OnDisable()
    {
        PlayerController.onEquipAction -= OnAddAction;
        PlayerController.onUnequipAction -= OnRemoveAction;
        PlayerController.onSelectActionToTakeDamage -= SelectToTakeDamage;
        PlayerController.onSelectActionToSwap -= SelectActionToSwap;
        PlayerController.onSelectActionToUse -= SelectActionToUse;
        PlayerParty.onAddCharacterToParty -= OnCharacterAddedToParty;
        PlayerParty.onRemoveCharacterFromParty -= OnCharacterRemovedFromParty;
    }

    void SelectToTakeDamage(List<ECharacterClass> allowedClasses)
    {
        isSelectToTakeDamage = true;
        selectableClasses = allowedClasses;
        SetConfirmButton(true, "Take Damage");
    }

    void SelectActionToSwap(List<ECharacterClass> allowedClasses)
    {
        selectableClasses = allowedClasses;
        SetConfirmButton(true, "Swap");
    }

    void SelectActionToUse(List<ECharacterClass> allowedClasses)
    {
        selectableClasses = allowedClasses;
        SetConfirmButton(true, "Use");
    }

    // temp till the ui/ux design is confirmed on how to select and use action
    public void ConfirmSelection()
    {
        PlayerController.SelectActionData(cardUIItem.GetCurrentSelectedActionData());
        selectableClasses = null;
        cardUIItem.Deselect();
        SetConfirmButton(false, "");
    }
    
    public void SetConfirmButton(bool active, string displayText)
    {
        if(confirmButton.style.display == DisplayStyle.None && active)
        {
            confirmButton.clicked += ConfirmSelection;
        }
        else if(confirmButton.style.display == DisplayStyle.Flex && !active)
        {
            confirmButton.clicked -= ConfirmSelection;
        }

        confirmButton.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        confirmButton.text = displayText;
    }

    void OnAddAction(ActionData target)
    {
        cardUIItem.AddCard(target);
    }

    void OnRemoveAction(ActionData target)
    {
        cardUIItem.RemoveCard(target);
    }

    void OnCharacterAddedToParty(ECharacterClass characterClass)
    {
        cardUIItem.InitializeCharacterCardUI(characterClass);
    }

    void OnCharacterRemovedFromParty(ECharacterClass characterClass)
    {
        cardUIItem.DeinitializeCharacterCardUI(characterClass);
    }
}
