using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardUIItem : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    [SerializeField] TMP_Text actionNameOnCard;

    [SerializeField] Button button;

    [SerializeField] UITransitions uiTransitions;

    public bool IsActivated() => gameObject.activeInHierarchy;

    bool isSelected;

    ActionData actionDataCache;

    void Awake()
    {
        button.onClick.AddListener(()=>Select());
    }

    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    public void SetActionNameOnCard(string _name)
    {
        actionNameOnCard.text = _name;
    }

    void CacheActionData(ActionData actionData) => actionDataCache = actionData;
    public ActionData GetActionData() => actionDataCache;
    public bool IsCachedActionData(ActionData comparedActionData) => actionDataCache == comparedActionData;

    public void Initialize(ActionData _cacheActionData)
    {
        CacheActionData(_cacheActionData);
        SetActionNameOnCard(_cacheActionData.action.actionName);

        gameObject.SetActive(true);
        button.interactable = false;

        uiTransitions?.Play("Activate", TransitionFlags.ActiveOnBegin, (_,_)=> button.interactable = true);
    }

    public void Deinitialize()
    {
        actionNameOnCard.color = Color.black;
        actionDataCache = null;
        actionNameOnCard.text = "";

        uiTransitions?.Play("Deactivate", TransitionFlags.InactiveOnEnd, (_,_) =>
        {
            button.interactable = false;
            gameObject.SetActive(false);
            isSelected = false;
        });
    }

    public void Select()
    {
        if(isSelected)
        {
            return;
        }
        isSelected = true;
        uiTransitions?.Play("Select");
        CardUI.Instance.SetCurrentSelectedCardUIItem(this);
        actionNameOnCard.color = CardUI.Instance.isSelectToRemove ? Color.red : Color.blue;
    }

    public void Deselect()
    {
        isSelected = false;
        uiTransitions?.Play("Deselect");
        actionNameOnCard.color = Color.black;
    }
}
