using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using TMPro;
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
    CombatObjectInfo currentCombatObjectInfo;
    Coroutine nextCombatObjectTurnRoutine;
    Coroutine actionCleanUpDelayRoutie;

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
        Debug.LogError("UnRegisterFromCombat");
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
        currentCombatObjectInfo.actionCount -= 1;
        currentCombatObjectInfo.character.ICleanUpAction();
    }

    public void ActionCleanUp()
    {
        if(!isInCombat)
        {
            return;
        }

        Debug.Log("CombatManager::ActionCleanUp");

        if(actionCleanUpDelayRoutie != null)
        {
            StopCoroutine(actionCleanUpDelayRoutie);
        }
        actionCleanUpDelayRoutie = StartCoroutine(ActionCleanUpDelay());
    }

    IEnumerator ActionCleanUpDelay()
    {
        float waitDuration = turnProcessDelay;
        float waitTimer = 0;
        while(waitTimer < waitDuration || pauseCombat)
        {
            waitTimer += Time.deltaTime;
            yield return null;
        }
        Debug.LogError(currentCombatObjectInfo.name + " :: ActionCleanUp After delay");
        if(currentCombatObjectInfo.actionCount <= 0)
        {
            NextCombatObjectTurn();
        }
        else
        {
            Debug.Log("Another turn from the same character.");
            currentCombatObjectInfo.character.ISelectAction();
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
            currentCombatObjectInfo.character.ITurnEnd();
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

       currentTurnCombatObjectIndex = (combatObjectInfos.IndexOf(currentCombatObjectInfo) + 1) % totalCombatObjects;
       currentCombatObjectInfo = combatObjectInfos[currentTurnCombatObjectIndex];

        if(currentTurnCombatObjectIndex == 0)
        {
            combatRoundPassed += 1;
        }

        Debug.Log("Update combat turn UI with :: " + currentCombatObjectInfo.name);
        onUpdateCurrentTurnCombatObject?.Invoke(currentCombatObjectInfo);
        bool skipTurn = !currentCombatObjectInfo.character.IStartTurn();
        if(skipTurn)
        {
            Debug.LogWarning("Skip Turn! :: " + currentCombatObjectInfo.name);
            NextCombatObjectTurn();
            yield break;
        }
        currentCombatObjectInfo.character.ISelectAction();
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
