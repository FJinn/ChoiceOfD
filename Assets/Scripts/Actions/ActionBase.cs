using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/*
*   There is a gameobject that hold all the actions.
*   Character calls the related action when needed.
*/

public abstract class ActionBase : MonoBehaviour
{
    public string actionName = "";
    public int initialHealth;
    public int maxHealth;
    public int currentHealth {get; private set;}
    [SerializeField] GameObject actionPrefab;
    [SerializeField] ECharacterClass[] belongsToClasses;
    [SerializeField] LayerMask detectionLayerMask;
    [SerializeField] bool ignoreOwnerLayer = true;
    [SerializeField] protected float requiredDistanceToTarget = 2f;

    [SerializeField] int actionBaseWeight = 0;
    [SerializeField, EffectsSelector] List<string> effectsTypes;

    public ECharacterClass[] GetBelongsToClasses() => belongsToClasses;

    static List<ActionObjectInfo> actionObjects = new List<ActionObjectInfo>();

    protected CharacterBase instigator;

    protected bool isMainAction;

    protected abstract void MainAction_Implementation(Action callback);
    protected abstract void SubAction_Implementation(Action callback);

    protected virtual void MainAction()
    {
        CombatManager.Instance.ActionStarted();
        MainAction_Implementation(()=> 
        {
            CombatManager.Instance.ActionEnded();
        });
    }
    protected virtual void SubAction()
    {
        CombatManager.Instance.ActionStarted();
        SubAction_Implementation(()=> 
        {
            CombatManager.Instance.ActionEnded();
        });
    }

    public virtual void DoAction(CharacterBase _instigator)
    {
        if(!Precondition())
        {
            return;
        }
        instigator = _instigator;
        
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

    public float GetRequiredDistanceToTarget()
    {
        return requiredDistanceToTarget;
    }

    class ActionObjectInfo
    {
        public GameObject obj;
        public Movement movement;
    }

    protected virtual void CleanUp()
    {
    }

    protected void ThrowAwayAction(Vector3 initialPos, Vector3 targetPos)
    {
        ActionObjectInfo info = GetActionObjectInfo();
        info.obj.transform.position = initialPos;
        float existDuration = 1f;
        float moveSpeed = 2f;
        info.movement.MoveToDirection(targetPos, moveSpeed);
        StartCoroutine(ActionObjectSelfDisappearUpdate(info.obj, existDuration));
    }

    ActionObjectInfo GetActionObjectInfo()
    {
        ActionObjectInfo found = actionObjects.Find(x => !x.obj.activeInHierarchy);
        if(found != null)
        {
            return found;
        }

        GameObject newObj = Instantiate(actionPrefab, transform);
        Movement _movement = newObj.GetComponent<Movement>();
        _movement.SetHasBoundary(false);
        ActionObjectInfo newInfo = new ActionObjectInfo(){obj = newObj, movement = _movement};
        actionObjects.Add(newInfo);
        return newInfo;
    }

    IEnumerator ActionObjectSelfDisappearUpdate(GameObject self, float disappearDuration)
    {
        float counter = 0;
        
        while(counter < disappearDuration)
        {
            counter += Time.deltaTime;
            yield return null;
        }
        self.SetActive(false);
    }
    
    public virtual RaycastHit[] DetectStraightLine(float maxDistance)
    {
        LayerMask modifiedLayerMask = detectionLayerMask;
        if(ignoreOwnerLayer) modifiedLayerMask &= ~(1 << instigator.GetLayer());
        Vector3 initialPos = instigator.transform.position;
        Vector3 direction = instigator.transform.forward;
        // Debug.DrawLine(initialPos, initialPos + direction * maxDistance, Color.red, 10f);
        // Debug.LogError(Physics.RaycastAll(initialPos, direction, maxDistance, modifiedLayerMask).Length);
        return Physics.RaycastAll(initialPos, direction, maxDistance, modifiedLayerMask);
    }

#region Health
    public bool AddHealth(int value)
    {
        Debug.Assert(value > 0);
        int resultValue = currentHealth + value;
        if(resultValue > maxHealth)
        {
            return false;
        }
        currentHealth = resultValue;
        return true;
    }

    /// <summary>
    /// return true if health > 0
    /// return false if health <= 0
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool ReduceHealth(int value)
    {
        Debug.Assert(value < 0);
        currentHealth += value;
        return currentHealth > 0;
    }

#endregion
}
