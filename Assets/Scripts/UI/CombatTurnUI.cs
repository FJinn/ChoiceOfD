using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatTurnUI : MonoBehaviour
{
    [SerializeField] GameObject combatUICanvas;
    [SerializeField] CombatTurnUIItem combatTurnUIItemPrefab;
    [SerializeField] Transform combatTurnUIItemParent;

    List<CombatTurnUIItem> spawnedCombatTurnUIItems = new();

    static int turnUIItemInTransitionsCount;
    public static bool stillRunning => turnUIItemInTransitionsCount > 0;

    void Start()
    {
        CombatManager.Instance.onCombatOrderArranged += InitializeTurnUIItems;
        CombatManager.Instance.onUnregisterOnCombat += RemoveCombatUIItem;
        CombatManager.Instance.onCombatEnded += CombatEndedCleanUp;
        CombatManager.Instance.onUpdateCurrentTurnCombatObject += UpdateCurrentCombatTurnUIItem;

        combatUICanvas.SetActive(false);
    }

    void OnDestroy()
    {
        if(CombatManager.Instance == null)
        {
            return;
        }

        CombatManager.Instance.onCombatOrderArranged -= InitializeTurnUIItems;
        CombatManager.Instance.onUnregisterOnCombat -= RemoveCombatUIItem;
        CombatManager.Instance.onCombatEnded -= CombatEndedCleanUp;
        CombatManager.Instance.onUpdateCurrentTurnCombatObject -= UpdateCurrentCombatTurnUIItem;
    }

    void InitializeTurnUIItems(List<CombatManager.CombatObjectInfo> allCombatObjectInfos)
    {
        combatUICanvas.SetActive(true);

        foreach(var item in spawnedCombatTurnUIItems)
        {
            item.Deactivate(null);
        }

        for(int i=0; i<allCombatObjectInfos.Count; ++i)
        {
            CombatManager.CombatObjectInfo currentInfo = allCombatObjectInfos[i];
            CombatTurnUIItem item = GetCombatTurnUIItem();
            item.Activate(currentInfo.sprite, currentInfo.name);
        }
    }

    void CombatEndedCleanUp()
    {
        combatUICanvas.SetActive(false);
    }

    void RemoveCombatUIItem(CombatManager.CombatObjectInfo target)
    {
        var found = spawnedCombatTurnUIItems.Find(x => x.GetName() == target.name);
        if(found)
        {
            turnUIItemInTransitionsCount += 1;
            found.Deactivate(()=> turnUIItemInTransitionsCount -= 1);
        }
    }

    void UpdateCurrentCombatTurnUIItem(CombatManager.CombatObjectInfo combatObjectInfo)
    {
        if(spawnedCombatTurnUIItems[0].GetName() == combatObjectInfo.name)
        {
            Debug.LogError("Skipping this: " + combatObjectInfo.name);
            return;
        }
        int targetSiblingIndex = -1;
        for(int i=spawnedCombatTurnUIItems.Count-1; i>=0; --i)
        {
            if(spawnedCombatTurnUIItems[i].IsActive())
            {
                targetSiblingIndex = spawnedCombatTurnUIItems[i].transform.GetSiblingIndex();
                break;
            }
        }
        combatTurnUIItemParent.GetChild(0).transform.SetSiblingIndex(targetSiblingIndex);
    }

    CombatTurnUIItem GetCombatTurnUIItem()
    {
        CombatTurnUIItem found = spawnedCombatTurnUIItems.Find(x => !x.IsActive());

        if(found != null)
        {
            return found;
        }

        found = Instantiate(combatTurnUIItemPrefab, combatTurnUIItemParent);
        spawnedCombatTurnUIItems.Add(found);
        return found;
    }
}
