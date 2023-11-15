using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class CharacterBase : MonoBehaviour, ICombat
{
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

    Coroutine healthBarRoutine;

    public float lengthFromGround => characterCollider.bounds.extents.y + RoomManager.Instance.GetCurrentRoomTileGroundWorldPosY();

    public float forwardLength => Vector3.Scale(characterCollider.bounds.extents, transform.forward).magnitude;
    public Vector3 forwardPosition => transform.position + transform.forward * forwardLength;

    public enum EBuffType
    {
        None = 0,
        DoubleDamage = 1
    }

    public enum EDebuffType
    {
        None = 0,
        Blind = 1,
        Confuse = 2,
        Posion = 3,
        Sleep = 4
    }

    public virtual int GetHealth(){return health;}
    public virtual void ReduceHealth(int reduceAmount, List<ECharacterClass> specificClasses = null, Action callback = null)
    {
        health -= reduceAmount;
        UpdateHealthBar(()=>
        {
            if(health <= 0)
            {
                KillCharacter(null);
            }
        });

        VFXManager.Instance.PlayVFX(VFXManager.EGeneralVFXType.LoseHealth, transform.position, callback);
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
    public virtual void KillCharacter(Action callback){gameObject.SetActive(false);}
#region Combat
    public virtual bool IStartTurn()
    {
        // ToDo:: process debuff
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
            CombatManager.Instance.ActionDone();
            return;
        }
        movement.MoveTo_Speed(initialRoomPosition, speed, ()=>
        {
            CombatManager.Instance.ActionDone();
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
        float endValue = newHealth / maxHealth;
        Debug.LogError(endValue);
        while(delta < 1)
        {
            delta += Time.deltaTime;
            healthBar.fillAmount = Mathf.Lerp(startValue, endValue, delta);
            yield return null;
        }

        callback?.Invoke();
    }
}
