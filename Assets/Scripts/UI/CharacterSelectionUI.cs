using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterSelectionUI : MonoBehaviour, IPointerClickHandler
{
    public static CharacterSelectionUI instance;

    [SerializeField] GameObject uiCanvas;
    [SerializeField] CharacterSelectionUIItem characterSelectionUIItemPrefab;
    [SerializeField] Transform characterSelectionUIItemParent;
    List<CharacterSelectionUIItem> spawnedItems = new List<CharacterSelectionUIItem>();

    CharacterSelectionUIItem currentSelection;

    public void OnPointerClick(PointerEventData eventData)
    {
        Activate();
    }

    void Awake()
    {
        PlayerParty.onObtainedCharacter += OnObtainedCharacter;
        instance = this;
        uiCanvas.SetActive(false);
    }

    void OnDestroy()
    {
        PlayerParty.onObtainedCharacter -= OnObtainedCharacter;
    }

    public void Activate()
    {
        uiCanvas.SetActive(true);
    }

    void Deactivate()
    {
        uiCanvas.SetActive(false);
    }

    public void SelectCurrentSelection(CharacterSelectionUIItem target)
    {
        currentSelection?.Deselect();
        currentSelection = target;
    }

    public void AddToParty()
    {
        PlayerController.Instance.AddCharacterIntoParty(currentSelection.GetCachedCharacterClass());
    }

    public void RemoveFromParty()
    {
        PlayerController.Instance.RemoveCharacterFromParty(currentSelection.GetCachedCharacterClass());
    }

    void OnObtainedCharacter(CharacterClassInfo characterClassInfo)
    {
        ECharacterClass eCharacterClass = characterClassInfo.characterClassType;
        CharacterSelectionUIItem found = GetCharacterSelectionUIItem(eCharacterClass);
        found.Initialize(eCharacterClass);
    }
    
    CharacterSelectionUIItem GetCharacterSelectionUIItem(ECharacterClass targetClass)
    {
        CharacterSelectionUIItem found = spawnedItems.Find(x => x.GetCachedCharacterClass() == targetClass);
        if(found != null)
        {
            return found;
        }

        CharacterSelectionUIItem newObj = Instantiate(characterSelectionUIItemPrefab, characterSelectionUIItemParent);
        spawnedItems.Add(newObj);
        return newObj;
    }
}
