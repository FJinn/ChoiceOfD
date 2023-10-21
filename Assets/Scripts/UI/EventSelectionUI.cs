using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventSelectionUI : Singleton<EventSelectionUI>
{
    public Action onEventSelectionUIItemDeactivate;
    public Action<ChoiceParams> onChoiceSelected;

    [SerializeField] GameObject canvas;
    [SerializeField] EventSelectionUIItem[] eventItems;

    [Serializable]
    public class ChoiceParams
    {
        public string displayText;
    }

    [Serializable]
    public class EventChoiceParams : ChoiceParams
    {
        public EGameEvent eventChoice;
    }


    int activatedEventSelectionUIItemCount;

    void Start()
    {
        onEventSelectionUIItemDeactivate += CallToDeactivateCanvas;
    }

    void OnDestroy()
    {
        onEventSelectionUIItemDeactivate -= CallToDeactivateCanvas;
    }

    public void AddChoices(params ChoiceParams[] choiceParams)
    {
        if(choiceParams.Length > eventItems.Length)
        {
            Debug.LogError($"Could not add event choices due to actions length ({choiceParams.Length}) is more than eventItems length ({eventItems.Length}). Skipping AddEventChoices()!");
            return;
        }

        activatedEventSelectionUIItemCount = 0;
        for(int i=0; i<choiceParams.Length; ++i)
        {
            eventItems[i].SetUpChoice(choiceParams[i]);
            activatedEventSelectionUIItemCount += 1;
        }
    }

    /// <summary>
    /// Add Event Choices before activate
    /// </summary>
    public void Activate()
    {
        onEventSelectionUIItemDeactivate += CallToDeactivateCanvas;
        canvas.SetActive(true);
        
        for(int i=0; i<eventItems.Length; ++i)
        {
            eventItems[i].Activate();
        }
    }

    public void Deactivate()
    {
        for(int i=0; i<eventItems.Length; ++i)
        {
            eventItems[i].Deactivate();
        }
    }

    void CallToDeactivateCanvas()
    {
        activatedEventSelectionUIItemCount -= 1;
        if(activatedEventSelectionUIItemCount > 0)
        {
            return;
        }
        canvas.SetActive(false);
    }
}
