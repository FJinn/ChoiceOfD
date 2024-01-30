using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterCardUI : MonoBehaviour
{
    public TMP_Text displayName;
    public TMP_Text displayHealth;

    public bool IsAvailable() => !gameObject.activeInHierarchy;
    public bool IsLinkedWithCharacter(CharacterLocalData _character) => characterLocalData == _character;
    
    CharacterLocalData characterLocalData; 

    public void LinkToCharacter(CharacterLocalData _character)
    {
        characterLocalData = _character;

        characterLocalData.onHealthChanged += OnHealthChanged;
        characterLocalData.onSatietyChanged += OnSatietyChanged;
        characterLocalData.onOccupationChanged += OnOccupationChanged;
        characterLocalData.onConditionChanged += OnConditionChanged;
        characterLocalData.onWeaponChanged += OnWeaponChanged;
        characterLocalData.onArmourChanged += OnArmourChanged;

        Activate();
    }

    void UnlinkCharacter()
    {
        characterLocalData.onHealthChanged -= OnHealthChanged;
        characterLocalData.onSatietyChanged -= OnSatietyChanged;
        characterLocalData.onOccupationChanged -= OnOccupationChanged;
        characterLocalData.onConditionChanged -= OnConditionChanged;
        characterLocalData.onWeaponChanged -= OnWeaponChanged;
        characterLocalData.onArmourChanged -= OnArmourChanged;

        characterLocalData = null;

        Deactivate();
    }

    void Activate()
    {
        displayName.text = characterLocalData.characterName;
        displayHealth.text = characterLocalData.currentHeath.ToString();

        gameObject.SetActive(true);
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }

    void OnHealthChanged(int newHealth)
    {
        displayHealth.text = newHealth.ToString();
    }

    void OnSatietyChanged(float newSatiety)
    {

    }

    void OnOccupationChanged(EOccupation newOccupation)
    {

    }

    void OnConditionChanged(ECondition newCondition)
    {

    }

    void OnWeaponChanged(SO_Card newWeapon)
    {

    }

    void OnArmourChanged(SO_Card newArmour)
    {

    }
}
