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
    public int takingDamageAmount {private set; get;}
    public float takingDamagePercentageAmount {private set; get;}
    public bool isDamagePercentage {private set; get;}

    static List<ECharacterClass> selectableClasses;
    public bool IsSelectableClass(ECharacterClass target)
    {
        return selectableClasses == null || selectableClasses.Count <= 0 || selectableClasses.Exists(x => x == target);
    }

    void OnEnable()
    {
        PlayerController.onEquipAction += OnAddAction;
        PlayerController.onUnequipAction += OnRemoveAction;
        PlayerController.onSelectActionToTakeDamage += SelectToTakeDamage;
        PlayerController.onSelectActionToTakeDamagePercentage += SelectToTakeDamage;
        PlayerController.onSelectActionToSwap += SelectActionToSwap;
        PlayerController.onSelectActionToUse += SelectActionToUse;
        PlayerParty.onAddCharacterToParty += OnCharacterAddedToParty;
        PlayerParty.onRemoveCharacterFromParty += OnCharacterRemovedFromParty;

        GameEvent.onEnterDungeon += OnEnterDungeon;
    }

    void OnDisable()
    {
        PlayerController.onEquipAction -= OnAddAction;
        PlayerController.onUnequipAction -= OnRemoveAction;
        PlayerController.onSelectActionToTakeDamage -= SelectToTakeDamage;
        PlayerController.onSelectActionToTakeDamagePercentage -= SelectToTakeDamage;
        PlayerController.onSelectActionToSwap -= SelectActionToSwap;
        PlayerController.onSelectActionToUse -= SelectActionToUse;
        PlayerParty.onAddCharacterToParty -= OnCharacterAddedToParty;
        PlayerParty.onRemoveCharacterFromParty -= OnCharacterRemovedFromParty;
        
        GameEvent.onEnterDungeon -= OnEnterDungeon;
    }

    void SelectToTakeDamage(int reduceAmount, List<ECharacterClass> allowedClasses)
    {
        isSelectToTakeDamage = true;
        takingDamageAmount = reduceAmount;
        isDamagePercentage = false;
        selectableClasses = allowedClasses;
        cardUIItem.UpdateSelectableClassPanel(selectableClasses);
    }

    void SelectToTakeDamage(float reducePercentage, List<ECharacterClass> allowedClasses)
    {
        isSelectToTakeDamage = true;
        takingDamagePercentageAmount = reducePercentage;
        isDamagePercentage = true;
        selectableClasses = allowedClasses;

        cardUIItem.UpdateSelectableClassPanel(selectableClasses);
    }

    void SelectActionToSwap(List<ECharacterClass> allowedClasses)
    {
        selectableClasses = allowedClasses;
    }

    void SelectActionToUse(List<ECharacterClass> allowedClasses)
    {
        selectableClasses = allowedClasses;
    }

    // temp till the ui/ux design is confirmed on how to select and use action
    public void ConfirmSelection(ActionData actionData)
    {
        PlayerController.SelectActionData(actionData);
        selectableClasses = null;
        isSelectToTakeDamage = false;
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
        cardUIItem.InitializeCharacterCardUI(characterClass, false);
    }

    void OnCharacterRemovedFromParty(ECharacterClass characterClass)
    {
        cardUIItem.DeinitializeCharacterCardUI(characterClass);
    }

    void OnEnterDungeon()
    {
        cardUIItem.SetAllCharacterPanelsDisplay(DisplayStyle.Flex);
    }
}
