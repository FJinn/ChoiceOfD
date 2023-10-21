using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatTurnUIItem : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] TMP_Text displayName;
    [SerializeField] UITransitions uiTransitions;

    public bool IsActive() => gameObject.activeInHierarchy;
    public string GetName() => displayName.text;

    public void Activate(Sprite sprite, string _name)
    {
        // image.sprite = sprite;
        displayName.text = _name;
        gameObject.SetActive(true);
        uiTransitions.Play("Activate", TransitionFlags.ActiveOnBegin);
    }

    public void Deactivate(Action callback)
    {
        uiTransitions.Play("Deactivate", TransitionFlags.InactiveOnEnd, (_,_) =>
        {
            displayName.text = "";
            callback?.Invoke();
            
            gameObject.SetActive(false);
        });
    }
}
