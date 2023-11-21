using System;
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
    VisualElement actionNamePanel;
    Label actionName;
    List<CombatTurnUIItem> spawnedCombatTurnUIItems = new();

    Coroutine actionNamePanelRoutine;

    static int turnUIItemInTransitionsCount;
    public static bool stillRunning => turnUIItemInTransitionsCount > 0;

    void Awake()
    {
        VisualElement root = uiDocument.rootVisualElement;
        charactersViewContainer = root.Q("CombatCharacterContainer");
        actionNamePanel = root.Q("ActionNamePanel");
        actionName = root.Q<Label>("ActionName");
    }

    void Start()
    {
        CombatManager.Instance.onCombatOrderArranged += InitializeTurnUIItems;
        CombatManager.Instance.onUnregisterOnCombat += RemoveCombatUIItem;
        CombatManager.Instance.onCombatEnded += CombatEndedCleanUp;
        CombatManager.Instance.onUpdateCurrentTurnCombatObject += UpdateCurrentCombatTurnUIItem;
        CombatManager.Instance.onActionStarted += ShowActionNamePanel;
        CombatManager.Instance.onActionEnded += HideActionNamePanel;

        charactersViewContainer.style.display = DisplayStyle.None;
        actionNamePanel.style.display = DisplayStyle.None;
        actionName.style.opacity = 0;
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

    void ShowActionNamePanel(ActionBase action, Action callback)
    {
        actionName.text = action.actionName;
        actionNamePanel.transform.scale = new Vector3(0, 1, 1);
        actionNamePanel.style.display = DisplayStyle.Flex;
        VisualElementTransitions.Instance.LerpVector(new VisualElementVectorParams
        {
            visualElement = actionNamePanel,
            routineCache = actionNamePanelRoutine,
            onEndCallback = ()=>
            {
                ShowActionName(callback);
            },
            targetVector = Vector3.one,
            durationToReach = 0.5f,
            vectorType = VisualElementVectorParams.EVectorType.Scale
        });
    }

    void ShowActionName(Action callback)
    {
        actionName.style.opacity = 0;
        Coroutine actionNameOpacityRoutine = null;
        VisualElementTransitions.Instance.LerpVector(new VisualElementVectorParams
        {
            visualElement = actionName,
            routineCache = actionNameOpacityRoutine,
            onEndCallback = callback,
            targetVector = Vector3.right,
            durationToReach = 0.7f,
            vectorType = VisualElementVectorParams.EVectorType.Opacity
        });
    }

    public void HideActionNamePanel()
    {
        actionNamePanel.style.display = DisplayStyle.None;
        actionName.style.opacity = 0;
    }
}
