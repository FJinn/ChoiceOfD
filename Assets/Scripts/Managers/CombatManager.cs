using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatManager : Singleton<CombatManager>
{
    public float turnProcessDelay = 0.5f;

    public Action<List<CombatObjectInfo>> onCombatOrderArranged;
    public Action<CombatObjectInfo> onUnregisterOnCombat;
    public Action onCombatEnded;
    public Action<CombatObjectInfo> onUpdateCurrentTurnCombatObject;
    public bool isInCombat {get; private set;}

    int combatRoundPassed = 0;
    int enemyCount;
    
    public class CombatObjectInfo
    {
        public CharacterBase character;
        public string name;
        public int initiateModifier;
        public int initiate;
        public int actionCount = 1;
    }

    List<CombatObjectInfo> combatObjectInfos = new();
    int totalCombatObjects;
    int currentTurnCombatObjectIndex;
    bool pauseCombat;
    Coroutine nextCombatObjectTurnRoutine;
    Coroutine actionDoneDelayRoutie;

    public void SetCombatObjectsAmount(int value)
    {
        totalCombatObjects = value;
    }

    public void PauseCombat(bool value)
    {
        pauseCombat = value;
    }

    void Start()
    {
        GameManager.Instance.onGameOver += HandleGameOver;
    }

    void OnDestroy()
    {
        GameManager gameManager = GameManager.Instance;
        if(gameManager == null)
        {
            return;
        }
        gameManager.onGameOver += HandleGameOver;
    }

    /// <summary>
    /// SetCombatObjectsAmount before adding
    /// </summary>
    /// <param name="_obj"></param>
    /// <param name="_initiateModifier"></param>
    /// <param name="_actionCount"></param>
    /// <param name="initializeTurn"></param>
    public void RegisterIntoCombat(CharacterBase _character, int _initiateModifier = 0, int _actionCount = 1, bool initializeTurn = true)
    {
        CombatObjectInfo newInfo = new CombatObjectInfo(){character = _character, initiateModifier = _initiateModifier, actionCount = _actionCount, name = _character.name};
        newInfo.initiate = ProbabilityManager.RollD20() + newInfo.initiateModifier;
        combatObjectInfos.Add(newInfo);

        if(combatObjectInfos.Count == totalCombatObjects)
        {
            // ToDo: move this to start combat and differentiate initialize combat or register in middle of combat
            ProcessCombatObjectsInitiate(initializeTurn);

            enemyCount = totalCombatObjects - PlayerController.Instance.GetPlayerPartyCharactersCount();
        }
    }

    public void UnRegisterFromCombat(CharacterBase _character)
    {
        CombatObjectInfo found = combatObjectInfos.Find(x => x.character == _character);
        if(found == null)
        {
            Debug.LogError($"Could not find object {_character.name} in combatObjectInfos! Skipping UnRegisterFromCombat()!");
            return;
        }
        onUnregisterOnCombat?.Invoke(found);
        combatObjectInfos.Remove(found);
        totalCombatObjects -= 1;

        enemyCount = _character is PlayerCharacter ? enemyCount : enemyCount - 1;

        if(enemyCount <= 0)
        {
            currentTurnCombatObjectIndex = -1;
            totalCombatObjects = 0;
            EndCombat();
        }
    }

    void EndCombat()
    {
        Debug.LogError("End combat");
        combatObjectInfos.Clear();
        onCombatEnded?.Invoke();
        isInCombat = false;
    }

    public void ActionStarted()
    {
        Debug.Log("CombatManager::ActionStarted");
    }

    public void ActionEnded()
    {
        if(!isInCombat)
        {
            return;
        }

        Debug.Log("CombatManager::ActionEnded");
        combatObjectInfos[currentTurnCombatObjectIndex].actionCount -= 1;
        combatObjectInfos[currentTurnCombatObjectIndex].character.ICleanUpAction();
    }

    public void ActionDone()
    {
        if(!isInCombat)
        {
            return;
        }

        Debug.Log("CombatManager::ActionDone");

        if(actionDoneDelayRoutie != null)
        {
            StopCoroutine(actionDoneDelayRoutie);
        }
        actionDoneDelayRoutie = StartCoroutine(ActionDoneDelay());
    }

    IEnumerator ActionDoneDelay()
    {
        float waitDuration = turnProcessDelay;
        float waitTimer = 0;
        while(waitTimer < waitDuration || pauseCombat)
        {
            waitTimer += Time.deltaTime;
            yield return null;
        }
        if(combatObjectInfos[currentTurnCombatObjectIndex].actionCount <= 0)
        {
            NextCombatObjectTurn();
        }
        else
        {
            combatObjectInfos[currentTurnCombatObjectIndex].character.ISelectAction();
        }
    }

    void NextCombatObjectTurn()
    {
        if(GameManager.Instance.IsGameOver())
        {
            Debug.Log("Game is over! Skip NextCombatObjectTurn() called.");
            return;
        }

        if(combatRoundPassed >= 0)
        {
            combatObjectInfos[currentTurnCombatObjectIndex].character.ITurnEnd();
        }
        
        if(nextCombatObjectTurnRoutine != null)
        {
            StopCoroutine(nextCombatObjectTurnRoutine);
        }
        nextCombatObjectTurnRoutine = StartCoroutine(NextCombatObjectTurnImplementation());
    }

    IEnumerator NextCombatObjectTurnImplementation()
    {
        // wait for everything clean up and unregister if dead etc
        // have a design where the turn end will wait for something to run before triggered
        while(CombatTurnUI.stillRunning)
        {
            yield return null;
        }
       currentTurnCombatObjectIndex = (currentTurnCombatObjectIndex + 1) % totalCombatObjects;

        if(currentTurnCombatObjectIndex == 0)
        {
            combatRoundPassed += 1;
        }

        onUpdateCurrentTurnCombatObject?.Invoke(combatObjectInfos[currentTurnCombatObjectIndex]);
        bool skipTurn = !combatObjectInfos[currentTurnCombatObjectIndex].character.IStartTurn();
        if(skipTurn)
        {
            NextCombatObjectTurn();
            yield break;
        }
        combatObjectInfos[currentTurnCombatObjectIndex].character.ISelectAction();
    }

    void ProcessCombatObjectsInitiate(bool initializeTurn = true)
    {
        combatObjectInfos.Sort((a,b)=>SortByInitiate(a,b));
        onCombatOrderArranged?.Invoke(combatObjectInfos);
        if(initializeTurn)
        {
            isInCombat = true;
            for(int i=0; i<combatObjectInfos.Count; ++i)
            {
                combatObjectInfos[i].character.ICombatStarted();
            }

            combatRoundPassed = -1;

            currentTurnCombatObjectIndex = -1;
            NextCombatObjectTurn();
        }
    }

    int SortByInitiate(CombatObjectInfo infoA, CombatObjectInfo infoB)
    {
        int result = infoA.initiate.CompareTo(infoB.initiate);
        if(result == 0)
        {
            result = infoA.initiateModifier.CompareTo(infoB.initiateModifier);
        }
        // if result still the same, just let it go (for now :') )
        return result;
    }

    void HandleGameOver()
    {
        EndCombat();
        if(nextCombatObjectTurnRoutine != null)
        {
            StopCoroutine(nextCombatObjectTurnRoutine);
        }
    }
}

public interface ICombat
{
    public bool IStartTurn();
    public void ISelectAction();
    public void ICleanUpAction();
    public void ITurnEnd();
    public void ICombatStarted();
}