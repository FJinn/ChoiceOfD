using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventSelectionUIItem : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] TMP_Text eventDisplay;
    [SerializeField] UITransitions uiTransitions;

    EGameEvent eventTypeCache;
    EventSelectionUI.ChoiceParams choiceParamsCache;

    void Awake()
    {
        button.onClick.AddListener(()=> OnSelected());
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    public void SetUpChoice(EventSelectionUI.ChoiceParams choiceParams)
    {
        choiceParamsCache = choiceParams;
        eventDisplay.text = choiceParams.displayText;
    }

    public void Activate()
    {
        button.interactable = false;
        uiTransitions?.Play("Activate", TransitionFlags.ActiveOnBegin, (_,_) => button.interactable = true);
    }

    public void Deactivate()
    {
        button.interactable = false;
        uiTransitions?.Play("Deactivate", TransitionFlags.InactiveOnEnd, (_,_) => EventSelectionUI.Instance.onEventSelectionUIItemDeactivate?.Invoke());
    }

    void OnSelected()
    {
        EventSelectionUI eventSelectionUI = EventSelectionUI.Instance;
        choiceParamsCache.callback?.Invoke();

        eventSelectionUI.Deactivate();
    }
}
