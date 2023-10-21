using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class PlayerController : CharacterBase
{
    public Action<ActionData> onSelectAction;
    public Action<ActionData> onEquipAction;
    public Action<ActionData> onRemoveAction;
    public Action onSelectActionToRemove;

    [Serializable]
    public class ActionData
    {
        public ActionBase action;
        public ECharacterClass belongToCharacterClass;
        public int obtainedIndex;
    }

    [SerializeField] AnimContoller animContoller;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] PlayerParty playerParty;

    [Header("Variables")]
    [SerializeField] float scrollScale = 1f;

    bool stopInput;
    InputAction moveInputAction;
    InputAction mainIputAction;
    InputAction scrollInputAction;
    float scrollDelta;

    CharacterBase currentTarget;

    static readonly int runningAnimID = Animator.StringToHash("IsRunning");
    static readonly int mainActionAnimID = Animator.StringToHash("MainAction");
    static readonly int defeatedAnimID = Animator.StringToHash("KneelingDefeat");

    List<ActionData> actionList = new List<ActionData>();
    ActionBase currentAction;
    int currentActionIndex;
    int currentReceivingDamage;
    public List<ActionData> actions => actionList;
    public override void ReduceHealth(int reduceAmount)
    {
        GameManager.Instance.PauseCombat();
        mainIputAction.performed -= SelectAction;

        currentReceivingDamage = reduceAmount;

        onSelectActionToRemove?.Invoke();
        // update ui to remove UI type with current action data
        onSelectAction?.Invoke(actionList[currentActionIndex]);
        mainIputAction.performed += ActionTakeDamage;
    }
    public override void AddHealth(int addValue = 1)
    {
    }

    void Start()
    {
        scrollInputAction = playerInput.actions.FindAction("ScrollWheel");
        moveInputAction = playerInput.actions.FindAction("Move");
        mainIputAction = playerInput.actions.FindAction("PrimaryKey");
        mainIputAction.performed += SelectAction;
    }

    void OnDestroy()
    {
        mainIputAction.performed -= SelectAction;
    }

    void Update()
    {
        if(playerInput == null)
        {
            return;
        }

        Vector2 scrollValue = scrollInputAction.ReadValue<Vector2>();
        scrollDelta += scrollValue.y * scrollScale;
        if(scrollDelta >= 1f)
        {
            NextAction();
            scrollDelta = 0f;
        }
        else if(scrollDelta <= -1f)
        {
            NextAction(false);
            scrollDelta = 0f;
        }
    }
/*
    void FixedUpdate()
    {
        if(playerInput == null || stopInput)
        {
            return;
        }
        Vector2 moveValue = moveInputAction.ReadValue<Vector2>();
        Vector3 direction = new Vector3(moveValue.x, 0, moveValue.y);

        bool hasMoveInput = moveValue.SqrMagnitude() > 0;

        if(hasMoveInput)
        {
            movement.Move(direction, speed);
            movement.LookAt(direction);
        }

        animContoller.SetParamBool(runningAnimID, hasMoveInput);
    }
*/
    public override void Initialize()
    {
        transform.position = new Vector3(0, lengthFromGround, 0);
        SetStopInput(true);
    }

    public void EquipAction(ActionBase _action, ECharacterClass targetClass)
    {
        // ToDo: replace if the slot is full
        if(playerParty.IsCharacterEquipSlotFull(targetClass))
        {
            todo
        }

        _action.InitializeAction();
        ActionData actionData = new ActionData();
        actionData.action = _action;
        var sameActions = actionList.FindAll(x => x.action == _action);
        actionData.obtainedIndex = sameActions != null && sameActions.Count > 0 ? sameActions.Count + 1 : 1;

        actionList.Add(actionData);
        onEquipAction?.Invoke(actionData);
        
        if(actionList.Count == 1)
        {
            currentAction = actionData.action;
            currentActionIndex = 0;
            onSelectAction?.Invoke(actionData);
        }
    }

    public void ActionTakeDamage(InputAction.CallbackContext callbackContext)
    {
        if(currentAction == null)
        {
            Debug.LogError("There is no current action to be removed! Skipping LoseAction()!");
            return;
        }

        ActionData currentActionData = actionList[currentActionIndex];

        bool isActionDead = currentActionData.action.ReduceHealth(currentReceivingDamage);
        currentReceivingDamage = 0;

        if(!isActionDead)
        {
            mainIputAction.performed -= ActionTakeDamage;
            mainIputAction.performed += SelectAction;

            GameManager.Instance.ResumeCombat();
            return;
        }

        RemoveAction(currentActionData);

        if(actionList.Count <= 0)
        {
            currentAction = null;
            KillCharacter();
            return;
        }

        currentAction = actionList[currentActionIndex].action;
        onSelectAction?.Invoke(actionList[currentActionIndex]);

        mainIputAction.performed -= ActionTakeDamage;
        mainIputAction.performed += SelectAction;

        GameManager.Instance.ResumeCombat();
    }

    void RemoveAction(ActionData target)
    {
        onRemoveAction?.Invoke(target);
        actionList.Remove(target);

        if(currentActionIndex >= actionList.Count && actionList.Count > 0)
        {
            currentActionIndex = actionList.Count - 1;
        }
    }

    void NextAction(bool isPrevious = false)
    {
        if(actionList.Count <= 1)
            return;
        currentActionIndex = (currentActionIndex + (isPrevious?-1:1)) % actionList.Count;
        currentAction = actionList[currentActionIndex].action;
        // Debug.LogError(currentActionIndex + " :: NextAction:: " + actionList.Count + " :: " + actionList.FindIndex(x => x == currentAction));
        onSelectAction?.Invoke(actionList[currentActionIndex]);
    }

    public override void EnterRoom(RoomTile roomTile, Transform enterTransform, Action callback)
    {
        base.EnterRoom(roomTile, enterTransform, callback);
        movement.SetHasBoundary(false);
        RoomTileInfo roomTileInfo = roomTile.GetCurrentRoomTileInfo();
        
        Vector3 targetPos = enterTransform.position;
        targetPos.y = lengthFromGround;

        movement.MoveTo_Duration(targetPos, 1f, ()=> 
        {
            movement.SetHasBoundary(true);
            callback?.Invoke();

            initialRoomPosition = transform.position;
        });
    }

    public override void ExitRoom(RoomTile roomTile)
    {
        // do transition 
    }

    void SetStopInput(bool value)
    {
        stopInput = value;
    }

    public void SelectAction(InputAction.CallbackContext callbackContext)
    {
        if(stopInput)
        {
            return;
        }

        if(currentAction == null)
        {
            Debug.LogError("There is no current action!");
            return;
        }

        Debug.Assert(currentTarget != null);
        SetStopInput(true);

        movement.MoveTo_Speed(currentTarget.forwardPosition, speed, ()=>
        {
            animContoller.PlayAnimation(mainActionAnimID, null);

            currentAction.DoAction(this);
        });
    }

    public override void IStartTurn()
    {
        SetStopInput(false);
    }

    public override void ISelectAction()
    {
        SetStopInput(false);
    }

    public override void ICleanUpAction()
    {
        base.ICleanUpAction();
    }

    public override void ITurnEnd()
    {
    }

    public override void ICombatStarted()
    {
        SetStopInput(true);
    }

    public override void KillCharacter()
    {
        animContoller.PlayAnimation(defeatedAnimID, ()=>
        {
            CombatManager.Instance.UnRegisterFromCombat(gameObject);
            // GameManager.Instance.ResumeCombat();
            GameManager.Instance.GameOver();
        });
    }
}
