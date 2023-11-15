using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetSelectionSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] ParticleSystem slotPS;
    [SerializeField] TargetLayout.ELayerPosition layerPosition;

    public TargetLayout.ELayerPosition GetLayerPosition() => layerPosition;
    public int GetSlotIndex() => slotIndex;
    int slotIndex;
    public int rowIndex {private set; get;}
    public int columnIndex {private set; get;}
    public int diagonalIndex {private set; get;}
    bool allRange;
    CharacterBase characterInThisSlot;
    TargetLayout targetLayout;

    public bool HasCharacter() => characterInThisSlot != null;

    public void Initialize(TargetLayout _targetLayout, int index)
    {
        targetLayout = _targetLayout;
        slotIndex = index;
    }

    public void SetRowIndex(int value) => rowIndex = value;
    public void SetColumnIndex(int value) => columnIndex = value;
    public void SetDiagonalIndex(int value) => diagonalIndex = value;

    public void Activate(bool isAllRange = false)
    {
        if(characterInThisSlot == null || characterInThisSlot.GetHealth() <= 0)
        {
            characterInThisSlot = null;
            return;
        }

        allRange = isAllRange;

        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        if(!gameObject.activeInHierarchy)
        {
            return;
        }

        slotPS.Stop();
        gameObject.SetActive(false);
    }

    public void LinkCharacterToSlot(CharacterBase character)
    {
        characterInThisSlot = character;
    }

    public void PointerEnter()
    {
        if(!gameObject.activeInHierarchy)
        {
            return;
        }
        slotPS.Play();
    }

    public void PointerExit()
    {
        slotPS.Stop();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayerController.Instance.ClickToSelectTargets(characterInThisSlot);
        targetLayout.DeactivateAllSelection();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetLayout.PointerEnterSelectionRange(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetLayout.PointerExitSelectionRange(this);
    }
}
