using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

/*
*   There is a gameobject that hold all the actions.
*   Character calls the related action when needed.
*/
public abstract class ActionBase : MonoBehaviour
{
    public enum ESelectableTargetType
    {
        Character = 0,
        Action = 1,
        Self
    }

    public enum ETargetRangeType
    {
        Unit = 0,
        FreeUnit = 1,
        Row = 2,
        Column = 3,
        Diagonal = 4,
        All = 5
    }

    public string actionName = "";
    [Header("Player")]
    [SerializeField] int initialHealth;
    [SerializeField] int maxHealth;
    [SerializeField] int defaultCooldownTurn = 1;
    [SerializeField] ESelectableTargetType selectableTargetType = ESelectableTargetType.Character;
    [SerializeField] ETargetRangeType actionRange = ETargetRangeType.Unit;

    [Header("NPC")]
    // how many targets can be selected for this action
    [SerializeField] int targetToBeSelectedCount = 1;

    [SerializeField] int actionBaseWeight = 0;
    [SerializeField, EffectsSelector] List<string> effectsTypes;

    public ESelectableTargetType GetSelectableTargetType() => selectableTargetType;
    public ETargetRangeType GetTargetRangeType() => actionRange;
    public int GetTargetCounts() => targetToBeSelectedCount;

    protected CharacterBase instigator;
    protected List<CharacterBase> targets;
    protected ActionData actionDataInstigator;

    protected bool isMainAction;

    protected abstract void MainAction_Implementation(Action callback);
    protected abstract void SubAction_Implementation(Action callback);

    public int GetInitialHealth() => initialHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetDefaultCooldownTurn() => defaultCooldownTurn;

    void Start()
    {
        ActionsManager.Instance.RegisterAction(this);
    }

    protected virtual void MainAction()
    {
        CombatManager.Instance.ActionStarted(this, ()=>
        {
            MainAction_Implementation(()=> 
            {
                Debug.LogWarning(actionName + " :: callback to ActionEnded");
                CombatManager.Instance.ActionEnded();
            });
        });
    }
    protected virtual void SubAction()
    {
        CombatManager.Instance.ActionStarted(this, ()=>
        {
            SubAction_Implementation(()=> 
            {
                CombatManager.Instance.ActionEnded();
            });
        });
    }

    public void SetTargetCount(int amount)
    {
        if(actionRange == ETargetRangeType.Unit || actionRange == ETargetRangeType.FreeUnit)
        {
            return;
        }

        targetToBeSelectedCount = amount;
    }

    public void SetTargets(List<CharacterBase> _targets)
    {
        Debug.Assert(instigator is not PlayerCharacter && _targets.Count > 0 && _targets.Count <= targetToBeSelectedCount);
        targets = _targets;
    }

    /// <summary>
    /// Set target before this
    /// </summary>
    /// <param name="_instigator"></param>
    public virtual void DoAction(CharacterBase _instigator)
    {
        instigator = _instigator;
        if(!Precondition())
        {
            return;
        }
        
        if(isMainAction)
            MainAction();
        else
            SubAction();
    }

    /// <summary> Initialize when action is given to character </summary>
    public virtual void InitializeAction()
    {
        isMainAction = true;
    }

    public virtual bool Precondition()
    {
        if(instigator is not PlayerCharacter && targets.Count > targetToBeSelectedCount)
        {
            Debug.LogError($"There is more targets {targets.Count} than allowed:: {targetToBeSelectedCount}!");
            return false;
        }
        
        return true;
    }

    public List<string> GetAllEffects()
    {
        return effectsTypes;
    }

    public int GetActionWeight()
    {
        return actionBaseWeight;
    }

    public void SetActionDataInstigator(ActionData _instigator)
    {
        actionDataInstigator = _instigator;
    }
    
}
