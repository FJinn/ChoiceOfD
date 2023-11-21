using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CombatTurnUI : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] Texture2D frameTexture;

    VisualElement charactersViewContainer;
    List<CombatTurnUIItem> spawnedCombatTurnUIItems = new();

    static int turnUIItemInTransitionsCount;
    public static bool stillRunning => turnUIItemInTransitionsCount > 0;

    void Awake()
    {
        VisualElement root = uiDocument.rootVisualElement;

        charactersViewContainer = root.Q("CombatCharacterContainer");
        
    }

    void Start()
    {
        CombatManager.Instance.onCombatOrderArranged += InitializeTurnUIItems;
        CombatManager.Instance.onUnregisterOnCombat += RemoveCombatUIItem;
        CombatManager.Instance.onCombatEnded += CombatEndedCleanUp;
        CombatManager.Instance.onUpdateCurrentTurnCombatObject += UpdateCurrentCombatTurnUIItem;

        charactersViewContainer.style.display = DisplayStyle.None;
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

    void InitializeTurnUIItems(List<CombatManager.CombatObjectInfo> _allCombatObjectInfos)
    {
        charactersViewContainer.style.display = DisplayStyle.Flex;

        foreach(var item in spawnedCombatTurnUIItems)
        {
            item.Deactivate();
        }

        for(int i=0; i<_allCombatObjectInfos.Count; ++i)
        {
            CombatManager.CombatObjectInfo currentInfo = _allCombatObjectInfos[i];
            CombatTurnUIItem item = GetCombatTurnUIItem();
            item.Activate(currentInfo, currentInfo.character.GetTexture2D());
            charactersViewContainer.Add(item);
        }
    }

    void CombatEndedCleanUp()
    {
        charactersViewContainer.style.display = DisplayStyle.None;
    }

    void RemoveCombatUIItem(CombatManager.CombatObjectInfo target)
    {
        var found = spawnedCombatTurnUIItems.Find(x => x.combatInfo == target);
        if(found != null)
        {
            turnUIItemInTransitionsCount += 1;
            found.Deactivate(()=> 
            {
                turnUIItemInTransitionsCount -= 1;
                charactersViewContainer.Remove(found);
            });
        }
    }

    void UpdateCurrentCombatTurnUIItem(CombatManager.CombatObjectInfo combatObjectInfo)
    {
        if(spawnedCombatTurnUIItems[0].combatInfo.character == combatObjectInfo.character)
        {
            Debug.LogError(spawnedCombatTurnUIItems[0].combatInfo.name);
            Debug.LogError("Skipping this: " + combatObjectInfo.name + " since it is the first item!");
            return;
        }

        var enumerator = charactersViewContainer.Children().GetEnumerator();
        enumerator.MoveNext();
        enumerator.Current.BringToFront();
        enumerator.Dispose();
    }

    CombatTurnUIItem GetCombatTurnUIItem()
    {
        CombatTurnUIItem found = spawnedCombatTurnUIItems.Find(x => !x.IsActive());

        if(found != null)
        {
            return found;
        }

        found = new CombatTurnUIItem(frameTexture);
        spawnedCombatTurnUIItems.Add(found);
        return found;
    }
}
