using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public Action onGameOver;

    [SerializeField] PlayerController player;
    bool isGameOver;
    public bool IsGameOver() => isGameOver;
    public PlayerController GetPlayer() => player;

    void Start()
    {
        player.Initialize();
        //player.ObtainCharacter(ECharacterClass.Fighter);
/*
        RoomTileInfo roomTileInfo = new RoomTileInfo(){radius = 10f, basicEnemyAmount = 5};
        RoomManager.Instance.InitializeRoom(roomTileInfo);
        RoomManager.Instance.SpawnObjectsInRoom(null);
        */
        GameStart();
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.P))
        {

        EventSelectionUI eventSelectionUI = EventSelectionUI.Instance;
        EventSelectionUI.ChoiceParams fighter = new EventSelectionUI.ChoiceParams(){displayText = "Fighter", callback = ()=>player.ObtainCharacter(ECharacterClass.Fighter, false)};
        EventSelectionUI.ChoiceParams priestess = new EventSelectionUI.ChoiceParams(){displayText = "Priestess", callback = ()=>player.ObtainCharacter(ECharacterClass.Priestess, false)};
        EventSelectionUI.ChoiceParams rogue = new EventSelectionUI.ChoiceParams(){displayText = "Rogue", callback = ()=>player.ObtainCharacter(ECharacterClass.Rogue, false)};
        eventSelectionUI.AddChoices(fighter, priestess, rogue);
        eventSelectionUI.Activate();
            // GameEvent.Instance.LeaveRoomSelection();
        }
    }

    public void GameStart()
    {
        RoomManager.Instance.InitializeTavernRoom();
        RoomManager.Instance.PlayerCharactersEnterRoom(()=>RoomManager.Instance.SpawnObjectsInRoom(null));
    }

    public void GameOver()
    {
        Debug.LogError("GAME OVER!");
        isGameOver = true;
        onGameOver?.Invoke();

        // ToDo: find a way to restart instead of reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PauseCombat()
    {
        CombatManager.Instance.PauseCombat(true);
    }

    public void ResumeCombat()
    {
        CombatManager.Instance.PauseCombat(false);
    }
}
