using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardUIItem : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    [SerializeField] TMP_Text actionNameOnCard;

    [SerializeField] UITransitions uiTransitions;

    public bool IsActivated() => gameObject.activeInHierarchy;

    PlayerController.ActionData actionDataCache;

    public void SetActionNameOnCard(string _name)
    {
        actionNameOnCard.text = _name;
    }

    void CacheActionData(PlayerController.ActionData actionData) => actionDataCache = actionData;
    public bool IsCachedActionData(PlayerController.ActionData comparedActionData) => actionDataCache == comparedActionData;

    public void Initialize(Vector2 size, PlayerController.ActionData _cacheActionData)
    {
        CacheActionData(_cacheActionData);
        SetActionNameOnCard(_cacheActionData.action.actionName);
        
        rectTransform.sizeDelta = size;
        gameObject.SetActive(true);

        uiTransitions?.Play("Activate", TransitionFlags.ActiveOnBegin);
    }

    public void Deinitialize()
    {
        actionNameOnCard.color = Color.black;
        actionDataCache = null;
        actionNameOnCard.text = "";

        uiTransitions?.Play("Deactivate", TransitionFlags.InactiveOnEnd, (_,_) =>
        {
            gameObject.SetActive(false);
        });
    }

    public void SelectAsCurrentAction(Vector2 size, bool isSelectToRemove)
    {
        rectTransform.sizeDelta = size;
        actionNameOnCard.color = isSelectToRemove ? Color.red : Color.blue;
    }

    public void DeselectAsCurrentAction(Vector2 size)
    {
        rectTransform.sizeDelta = size;
        actionNameOnCard.color = Color.black;
    }
}
