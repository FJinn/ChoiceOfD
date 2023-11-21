using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class TargetLayout : MonoBehaviour
{
    public enum ELayerPosition
    {
        Front = 0,
        Middle = 1,
        Back = 2
    }

    [SerializeField] EnemyFormationData enemyFormationData;
    [ReadOnly] public int totalSlots = 9;

    [SerializeField] TargetSelectionSlot[] targetSelectionSlots;

    EnemyFormation currentFormation;
    ActionBase.ETargetRangeType currentActionRange;

    void Awake()
    {
        SetupLayer();
        // ToDo:: Assign current formation
        currentFormation = enemyFormationData.enemyFormations[0];
        
        PlayerController.onWaitToSelectTarget += ActivateSlots;
    }

    void OnDestroy()
    {
        PlayerController.onWaitToSelectTarget -= ActivateSlots;
    }

    public void SetupLayer()
    {
        int rowIndex = 0;
        int columnIndex = 0;
        int diagonalIndex = 0;
        for(int i=0; i<targetSelectionSlots.Length; ++i)
        {
            targetSelectionSlots[i].Initialize(this, i);
            targetSelectionSlots[i].SetRowIndex(rowIndex);
            targetSelectionSlots[i].SetColumnIndex(columnIndex);
            if(MathUtilities.RemainderOfTwo(i) == 0)
            {
                targetSelectionSlots[i].SetDiagonalIndex(diagonalIndex);
                diagonalIndex = diagonalIndex + 1 > 2 ? 0 : diagonalIndex + 1;
            }
            else
            {
                targetSelectionSlots[i].SetDiagonalIndex(-1);
            }
            rowIndex = rowIndex + 1 >= 3 ? 0 : rowIndex + 1;
            columnIndex = (i + 1) % 3 == 0 ? columnIndex + 1 : columnIndex;
        }
    }

    // Maybe have a better structure
    public EEnemyType GetEnemyTypeOfSlot(int index) => index switch
    {
        // front
        0 => currentFormation.frontSlotA,
        1 => currentFormation.frontSlotB,
        2 => currentFormation.frontSlotC,
        // mid
        3 => currentFormation.middleSlotA,
        4 => currentFormation.middleSlotB,
        5 => currentFormation.middleSlotC,
        // back
        6 => currentFormation.backSlotA,
        7 => currentFormation.backSlotB,
        8 => currentFormation.backSlotC,
        // invalid
        _ => EEnemyType.None
    };

    public Transform GetEnemySlotTransform(int index) => targetSelectionSlots[index].transform;

    public int GetTotalEnemyCount() => currentFormation.TotalEnemyCount();

    public void LinkSpawnedCharacterToSlot(int index, CharacterBase character)
    {
        targetSelectionSlots[index].LinkCharacterToSlot(character);
    }

    public void DeactivateAllSelection()
    {
        foreach(var item in targetSelectionSlots)
        {
            item.Deactivate();
        }
    }

    public void PointerEnterSelectionRange(TargetSelectionSlot targetSlot)
    {
        switch(currentActionRange)
        {
            case ActionBase.ETargetRangeType.Unit:
            case ActionBase.ETargetRangeType.FreeUnit:
                targetSlot.PointerEnter();
                break;
            case ActionBase.ETargetRangeType.Diagonal:
                foreach(var item in targetSelectionSlots)
                {
                    if(item.diagonalIndex == targetSlot.diagonalIndex || item.diagonalIndex == 2)
                    {
                        item.PointerEnter();
                    }
                }
                break;
            case ActionBase.ETargetRangeType.Row:
                foreach(var item in targetSelectionSlots)
                {
                    if(item.rowIndex == targetSlot.rowIndex)
                    {
                        item.PointerEnter();
                    }
                }
                break;
            case ActionBase.ETargetRangeType.Column:
                foreach(var item in targetSelectionSlots)
                {
                    if(item.columnIndex == targetSlot.columnIndex)
                    {
                        item.PointerEnter();
                    }
                }
                break;
            case ActionBase.ETargetRangeType.All:
                foreach(var item in targetSelectionSlots)
                {
                    item.PointerEnter();
                }
                break;
        }
    }

    public void PointerExitSelectionRange(TargetSelectionSlot targetSlot)
    {
        switch(currentActionRange)
        {
            case ActionBase.ETargetRangeType.Unit:
            case ActionBase.ETargetRangeType.FreeUnit:
                targetSlot.PointerExit();
                break;
            case ActionBase.ETargetRangeType.Diagonal:
                foreach(var item in targetSelectionSlots)
                {
                    if(item.diagonalIndex == targetSlot.diagonalIndex || item.diagonalIndex == 2)
                    {
                        item.PointerExit();
                    }
                }
                break;
            case ActionBase.ETargetRangeType.Row:
                foreach(var item in targetSelectionSlots)
                {
                    if(item.rowIndex == targetSlot.rowIndex)
                    {
                        item.PointerExit();
                    }
                }
                break;
            case ActionBase.ETargetRangeType.Column:
                foreach(var item in targetSelectionSlots)
                {
                    if(item.columnIndex == targetSlot.columnIndex)
                    {
                        item.PointerExit();
                    }
                }
                break;
            case ActionBase.ETargetRangeType.All:
                foreach(var item in targetSelectionSlots)
                {
                    item.PointerExit();
                }
                break;
        }
    }

    void ActivateSlots(ActionBase action)
    {
        currentActionRange = action.GetTargetRangeType();
        switch(currentActionRange)
        {
            case ActionBase.ETargetRangeType.Unit:
                GetRightSlot(-3)?.Activate();
                GetRightSlot(-2)?.Activate();
                GetRightSlot(-1)?.Activate();
                break;
            case ActionBase.ETargetRangeType.Diagonal:
                for(int i=0; i<targetSelectionSlots.Length; i+=2)
                {
                    targetSelectionSlots[i].Activate();
                }
                break;
            case ActionBase.ETargetRangeType.Row:
            case ActionBase.ETargetRangeType.Column:
            case ActionBase.ETargetRangeType.FreeUnit:
            case ActionBase.ETargetRangeType.All:
                for(int i=0; i<targetSelectionSlots.Length; ++i)
                {
                    targetSelectionSlots[i].Activate(true);
                }
                break;
        }
    }

    public void PointerClicked(TargetSelectionSlot clickedSlot)
    {
        PlayerController playerController = PlayerController.Instance;
        playerController.ClearTargets();

        switch(currentActionRange)
        {
            case ActionBase.ETargetRangeType.Unit:
            case ActionBase.ETargetRangeType.FreeUnit:
                playerController.ClickToSelectTargets(clickedSlot.GetCharacter());
                break;
            case ActionBase.ETargetRangeType.Diagonal:
                foreach(var item in targetSelectionSlots)
                {
                    if(item.diagonalIndex == clickedSlot.diagonalIndex || item.diagonalIndex == 2)
                    {
                        playerController.ClickToSelectTargets(item.GetCharacter());
                    }
                }
                break;
            case ActionBase.ETargetRangeType.Row:
                foreach(var item in targetSelectionSlots)
                {
                    if(item.rowIndex == clickedSlot.rowIndex)
                    {
                        playerController.ClickToSelectTargets(item.GetCharacter());
                    }
                }
                break;
            case ActionBase.ETargetRangeType.Column:
                foreach(var item in targetSelectionSlots)
                {
                    if(item.columnIndex == clickedSlot.columnIndex)
                    {
                        playerController.ClickToSelectTargets(item.GetCharacter());
                    }
                }
                break;
            case ActionBase.ETargetRangeType.All:
                for(int i=0; i<targetSelectionSlots.Length; ++i)
                {
                    CharacterBase target = targetSelectionSlots[i].GetCharacter();
                    if(target == null)
                    {
                        continue;
                    }
                    playerController.ClickToSelectTargets(target);
                }
                break;
        }
        DeactivateAllSelection();
    }

#region get neighbour
    // ToDo:: better structure
    public TargetSelectionSlot GetLeftSlot(int instigatorIndex, bool withCharacter = true)
    {
        int targetIndex = instigatorIndex - 3;
        if(targetIndex < 0)
        {
            return null;
        }

        TargetSelectionSlot result = targetSelectionSlots[targetIndex];
        if(!result.HasCharacter())
        {
            return GetLeftSlot(targetIndex);
        }
        return result;
    }
    
    public TargetSelectionSlot GetRightSlot(int instigatorIndex, bool withCharacter = true)
    {
        int targetIndex = instigatorIndex + 3;
        if(targetIndex >= targetSelectionSlots.Length)
        {
            return null;
        }

        TargetSelectionSlot result = targetSelectionSlots[targetIndex];
        if(!result.HasCharacter())
        {
            return GetRightSlot(targetIndex);
        }
        return result;
    }
    
    public TargetSelectionSlot GetTopSlot(int instigatorIndex, ELayerPosition layerPosition, bool withCharacter = true)
    {
        int targetIndex = instigatorIndex - 1;
        switch (layerPosition)
        {
            case ELayerPosition.Front:
                targetIndex = targetIndex >= 0 ? targetIndex : -1;
                break;
            case ELayerPosition.Middle:
                targetIndex = targetIndex >= 3 ? targetIndex : -1;
                break;
            case ELayerPosition.Back:
                targetIndex = targetIndex >= 6 ? targetIndex : -1;
                break;

        }

        TargetSelectionSlot result = targetIndex < 0 ? null : targetSelectionSlots[targetIndex];
        if(result != null && !result.HasCharacter())
        {
            return GetTopSlot(targetIndex, layerPosition);
        }
        return result;
    }
    
    public TargetSelectionSlot GetBottomSlot(int instigatorIndex, ELayerPosition layerPosition, bool withCharacter = true)
    {
        int targetIndex = instigatorIndex + 1;
        switch (layerPosition)
        {
            case ELayerPosition.Front:
                targetIndex = targetIndex < 3 ? targetIndex : -1;
                break;
            case ELayerPosition.Middle:
                targetIndex = targetIndex < 6 ? targetIndex : -1;
                break;
            case ELayerPosition.Back:
                targetIndex = targetIndex < 9 ? targetIndex : -1;
                break;

        }

        TargetSelectionSlot result = targetIndex < 0 ? null : targetSelectionSlots[targetIndex];
        if(result != null && !result.HasCharacter())
        {
            return GetBottomSlot(targetIndex, layerPosition);
        }
        return result;
    }
#endregion

}
