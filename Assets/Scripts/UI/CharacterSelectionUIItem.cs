using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionUIItem : MonoBehaviour
{
    [SerializeField] UITransitions uiTransitions;
    [SerializeField] Image bgImage;
    [SerializeField] Button button;
    [SerializeField] TMP_Text displayText;
    [SerializeField, ReadOnly] ECharacterClass characterClass;

    public bool IsActivated() => gameObject.activeInHierarchy;
    public ECharacterClass GetCachedCharacterClass() => characterClass;

    bool isSelected;

    void Awake()
    {
        button.onClick.AddListener(Select);
    }

    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    public void Initialize(ECharacterClass eCharacterClass)
    {
        characterClass = eCharacterClass;
        displayText.text = characterClass.ToString();
    }

    void Select()
    {
        if(isSelected)
        {
            return;
        }

        isSelected = true;
        uiTransitions?.Play("Select");
        bgImage.color = Color.green;
        CharacterSelectionUI characterSelectionUI = CharacterSelectionUI.instance;
        characterSelectionUI.SelectCurrentSelection(this);
    }
    
    public void Deselect()
    {
        isSelected = false;
        bgImage.color = Color.grey;
        uiTransitions?.Play("Deselect");
    }
}
