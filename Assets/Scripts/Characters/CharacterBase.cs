using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterBase : MonoBehaviour, ICombat
{
    [SerializeField] protected Movement movement;
    [SerializeField] protected Collider characterCollider;
    [SerializeField] protected float speed = 4f;
    protected StatsInfo stats;
    protected int health = 0;
    protected Vector3 initialRoomPosition;

    public int GetLayer() => gameObject.layer;
    public float lengthFromGround => characterCollider.bounds.extents.y + RoomManager.Instance.halfRoomTileHeight;

    public float forwardLength => Vector3.Scale(characterCollider.bounds.extents, transform.forward).magnitude;
    public Vector3 forwardPosition => transform.position + transform.forward * forwardLength;

    public StatsInfo GetStats() => stats;
    public virtual int GetHealth(){return health;}
    public virtual void ReduceHealth(int reduceAmount){}
    public virtual void AddHealth(int addValue){}
    public abstract void Initialize();
    public virtual void EnterRoom(RoomTile roomTile, Transform enterTransform, Action callback)
    {
        RoomTileInfo roomTileInfo = roomTile.GetCurrentRoomTileInfo();
        movement.SetBoundaries(roomTileInfo.center, roomTileInfo.radius);
    }
    public virtual void ExitRoom(RoomTile roomTile){gameObject.SetActive(false);}
    public virtual void KillCharacter(){gameObject.SetActive(false);}
    
    public abstract void IStartTurn();
    public abstract void ISelectAction();
    public virtual void ICleanUpAction()
    {
        movement.MoveTo_Speed(initialRoomPosition, speed, ()=>
        {
            CombatManager.Instance.ActionDone();
        });
    }
    public abstract void ITurnEnd();
    public abstract void ICombatStarted();
}
