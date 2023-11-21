using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public abstract class CharacterBase : MonoBehaviour, ICombat
{
    public Action<EBuffType> onBuffAdded;
    public Action<EBuffType> onBuffRemoved;
    public Action<EDebuffType> onDebuffAdded;
    public Action<EDebuffType> onDebuffRemoved;

    [SerializeField] protected Movement movement;
    [SerializeField] protected Collider characterCollider;
    [SerializeField] protected Rigidbody body;
    [SerializeField] protected AnimContoller animContoller;
    [SerializeField] protected float speed = 4f;

    [Header("Not Player")]
    [SerializeField] protected int initialHealth = 3;
    [SerializeField] protected int maxHealth = 5;
    [SerializeField] Image healthBar;

    protected int health = 0;
    protected Vector3 initialRoomPosition;
    protected Quaternion initialRoomRotation;
    protected bool hasMovedFromAction;
    protected Texture2D characterTexture2D;

    [SerializeField, ReadOnly] protected List<ConditionInfo> conditions;

    Coroutine healthBarRoutine;

    public float lengthFromGround => characterCollider.bounds.extents.y + RoomManager.Instance.GetCurrentRoomTileGroundWorldPosY();

    public float forwardLength => Vector3.Scale(characterCollider.bounds.extents, transform.forward).magnitude;
    public Vector3 forwardPosition => transform.position + transform.forward * forwardLength;

    [Serializable]
    public class ConditionInfo
    {
        public EBuffType buff;
        public EDebuffType debuff;

        public int remainingTurns = 0;
        public bool reduceByTurns = true;

        public float percentageToBeDispelled
        {
            set
            {
                currentPercentageToBeDispelled = value;
            }
            get
            {
                return currentPercentageToBeDispelled;
            }
        }

        public float percentageToBeDispelledIncrementByTurn = 0f;
        float currentPercentageToBeDispelled = 0f;

        public int remainingHealthPoint;

        public void ReduceTurn(int amount = 1)
        {
            remainingTurns -= amount;
            if(remainingTurns <= 0)
            {
                buff = EBuffType.None;
                debuff = EDebuffType.None;
            }
        }

        public void TryDispelledByTurn()
        {
            currentPercentageToBeDispelled += percentageToBeDispelledIncrementByTurn;
            if(ProbabilityManager.RandomPercentage(currentPercentageToBeDispelled))
            {
                buff = EBuffType.None;
                debuff = EDebuffType.None;
            }
        }

        public void ReduceHealthPoint(int amount)
        {
            remainingHealthPoint -= amount;
            if(remainingHealthPoint <= 0)
            {
                buff = EBuffType.None;
                debuff = EDebuffType.None;
            }
        }

        public bool IsDirty() => buff == EBuffType.None && debuff == EDebuffType.None;
    }

    public enum EBuffType
    {
        None = 0,
        DoubleDamage = 1,
        Shield = 2,
        TemporaryHP = 3
    }

    public enum EDebuffType
    {
        None = 0,
        Blind = 1,
        Confuse = 2,
        Posion = 3,
        Sleep = 4
    }

    protected bool HasShield()
    {
        ConditionInfo found = conditions.Find(x => x.buff == EBuffType.Shield);
        if(found == null)
        {
            return false;
        }

        found.ReduceTurn();
        if(found.IsDirty())
        {
            conditions.Remove(found);
        }
        return true;
    }

    protected bool HasTemporaryHealthPoint(int reduceAmount)
    {
        ConditionInfo found = conditions.Find(x => x.buff == EBuffType.TemporaryHP);
        if(found == null)
        {
            return false;
        }

        found.ReduceHealthPoint(reduceAmount);
        if(found.IsDirty())
        {
            conditions.Remove(found);
        }
        return true;
    }

    protected bool SkipReduceHealthEvaluation(int reduceAmount)
    {
        bool skipReduceHealth = HasShield() || HasTemporaryHealthPoint(reduceAmount);
        return skipReduceHealth;
    }
    public virtual int GetHealth(){return health;}
    public virtual void ReduceHealth(int reduceAmount, List<ECharacterClass> specificClasses = null, Action callback = null)
    {
        if(SkipReduceHealthEvaluation(reduceAmount))
        {
            callback?.Invoke();
            return;
        }
        health -= reduceAmount;
        UpdateHealthBar(null);

        VFXManager.Instance.PlayVFX(VFXManager.EGeneralVFXType.LoseHealth, transform.position, ()=>
        {
            if(health <= 0)
            {
                KillCharacter(callback);
                return;
            }
            callback?.Invoke();
        });
    }
    public virtual void AddHealth(int addValue, List<ECharacterClass> specificClasses = null, Action callback = null)
    {
        health = Mathf.Clamp(health + addValue, 0, maxHealth);
        UpdateHealthBar(null);
        VFXManager.Instance.PlayVFX(VFXManager.EGeneralVFXType.GainHealth, transform.position, callback);
    }
    public virtual void Initialize()
    {
        health = initialHealth;
    }
    public virtual void SetTexture2D(Texture2D value) => characterTexture2D = value;
    public virtual Texture2D GetTexture2D() => characterTexture2D;
    public virtual void EnterRoom(RoomTile roomTile, Action callback, Transform enterTransform, bool preplacedInRoom)
    {
        Vector3 targetPos = enterTransform.position;
        targetPos.y = lengthFromGround;

        initialRoomPosition = targetPos;
        initialRoomRotation = enterTransform.rotation;

        movement.SetBoundaries(roomTile.GetCenter(), roomTile.GetRadius());
        gameObject.SetActive(true);
    }
    public virtual void ExitRoom(RoomTile roomTile, Action callback)
    {
        gameObject.SetActive(false);
        callback?.Invoke();
    }
    public virtual void KillCharacter(Action callback)
    {
        gameObject.SetActive(false);
        callback?.Invoke();
    }
#region Combat
    public virtual bool IStartTurn()
    {
        for(int i=conditions.Count-1; i>=0; --i )
        {
            ConditionInfo item = conditions[i];
            if(item.reduceByTurns)
            {
                item.ReduceTurn();
            }
            if(item.percentageToBeDispelled > 0f)
            {
                item.TryDispelledByTurn();
            }

            if(item.IsDirty())
            {
                conditions.Remove(item);
            }
        }

        bool skipTurn = false;
        if(skipTurn)
        {
            ICleanUpAction();
            return false;
        }

        return true;
    }
    public abstract void ISelectAction();
    public virtual void ICleanUpAction()
    {
        if(!hasMovedFromAction)
        {
            CombatManager.Instance.ActionCleanUp();
            return;
        }
        movement.MoveTo_Speed(initialRoomPosition, speed, ()=>
        {
            CombatManager.Instance.ActionCleanUp();
        });
    }
    public abstract void ITurnEnd();
    public abstract void ICombatStarted();
#endregion

    void UpdateHealthBar(Action callback)
    {
        if(healthBarRoutine != null)
        {
            StopCoroutine(healthBarRoutine);
        }
        healthBarRoutine = StartCoroutine(HealthBarUpdate(health,callback));
    }

    IEnumerator HealthBarUpdate(int newHealth, Action callback)
    {
        float delta = 0;
        float startValue = healthBar.fillAmount;
        float endValue = newHealth / (float)maxHealth;

        while(delta < 1)
        {
            delta += Time.deltaTime;
            healthBar.fillAmount = Mathf.Lerp(startValue, endValue, delta);
            yield return null;
        }

        callback?.Invoke();
    }

    public void AddCondition(ConditionInfo conditionInfo)
    {
        if(conditionInfo.buff != EBuffType.None)
        {
            onBuffAdded?.Invoke(conditionInfo.buff);
        }
        if(conditionInfo.debuff != EDebuffType.None)
        {
            onDebuffAdded?.Invoke(conditionInfo.debuff);
        }

        ConditionInfo found = conditions.Find(x => x.buff == conditionInfo.buff || x.debuff == conditionInfo.debuff);
        if(found != null)
        {
            found.remainingTurns = conditionInfo.remainingTurns;
            found.reduceByTurns = conditionInfo.reduceByTurns;
            found.percentageToBeDispelled = conditionInfo.percentageToBeDispelled;
            found.percentageToBeDispelledIncrementByTurn = conditionInfo.percentageToBeDispelledIncrementByTurn;
            return;
        }

        conditions.Add(conditionInfo);
    }

    public void RemoveCondition(EDebuffType debuffType)
    {
        onDebuffRemoved?.Invoke(debuffType);
        conditions.RemoveAll(x => x.debuff == debuffType);
    }
    public void RemoveCondition(EBuffType buffType)
    {
        onBuffRemoved?.Invoke(buffType);
        conditions.RemoveAll(x => x.buff == buffType);
    }
    public void RemoveAllDebuffConditions()
    {
        for(int i=conditions.Count-1; i>=0; --i)
        {
            if(conditions[i].debuff != EDebuffType.None)
            {
                onDebuffRemoved?.Invoke(conditions[i].debuff);
                conditions.Remove(conditions[i]);
            }
        }
    }
    public void RemoveAllBuffConditions()
    {
        for(int i=conditions.Count-1; i>=0; --i)
        {
            if(conditions[i].buff != EBuffType.None)
            {
                onBuffRemoved?.Invoke(conditions[i].buff);
                conditions.Remove(conditions[i]);
            }
        }
    }
    public void RemoveRandomDebuffCondition()
    {
        var debuffs = conditions.FindAll(x => x.debuff != EDebuffType.None);
        int randomIndex = Random.Range(0,debuffs.Count);
        conditions.Remove(debuffs[randomIndex]);

        onDebuffRemoved?.Invoke(debuffs[randomIndex].debuff);
    }
}
