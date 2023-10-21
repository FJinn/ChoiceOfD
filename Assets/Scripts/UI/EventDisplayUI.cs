using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class EventDisplayUI : MonoBehaviour
{
    [SerializeField] GameEvent gameEvent;
    [SerializeField] GameObject eventDisplayCanvas;
    [SerializeField] UITransitions uiTransitions;
    [SerializeField] UITransitions[] textUITransitions;
    [SerializeField] TMP_Text title;
    [SerializeField] TMP_Text description;

    bool isShowingEvent;

    void Awake()
    {
        gameEvent.onUpdateCurrentGameEventInfo += OnUpdateCurrentGameEventInfo;
        gameEvent.onRoomReady += ShowEvent;
        eventDisplayCanvas.SetActive(false);

        title.gameObject.SetActive(false);
        description.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        gameEvent.onUpdateCurrentGameEventInfo -= OnUpdateCurrentGameEventInfo;
        gameEvent.onRoomReady -= ShowEvent;
    }

    void OnUpdateCurrentGameEventInfo(GameEventInfo gameEventInfo)
    {
        title.text = gameEventInfo.eventName;
        description.text = gameEventInfo.eventDescription;
    }

    public void ShowEvent()
    {
        isShowingEvent = true;
        eventDisplayCanvas.SetActive(true);
        uiTransitions?.Play("Activate", TransitionFlags.ActiveOnBegin, (_,_)=>
        {
            foreach(var item in textUITransitions)
                item?.Play("Activate", TransitionFlags.ActiveOnBegin, (_,_) => WaitForKeyPressed());
        });
    }

    public void HideEvent()
    {
        textUITransitions[0]?.Play("Deactivate", TransitionFlags.InactiveOnEnd, (_,_)=>
        {
            uiTransitions?.Play("Deactivate", TransitionFlags.InactiveOnEnd, (_,_)=>
            {
                gameEvent.StartEvent();
                eventDisplayCanvas.SetActive(false);
            });
        });

        for(int i=1; i<textUITransitions.Length; ++i)
        {
            textUITransitions[i]?.Play("Deactivate", TransitionFlags.InactiveOnEnd);
        }
    }

    void WaitForKeyPressed()
    {
        if(!isShowingEvent)
        {
            return;
        }
        Debug.LogError("WaitForKeyPressed");
        isShowingEvent = false;
        InputSystem.onAnyButtonPress.CallOnce(key => HideEvent());
    }
}
