using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public Action onGameOver;

    [SerializeField] List<PlayerController> allPlayers;
    bool isGameOver;
    public bool IsGameOver() => isGameOver;

    void Start()
    {
        // temp game flow -> add character, spawn a room, fight
        for(int i=0; i<allPlayers.Count; ++i)
        {
            allPlayers[i].Initialize();
            if(allPlayers[i] is PlayerController player)
            {
                // player.EquipAction(ActionsManager.Instance.GetAction("Basic Action"));
                // player.EquipAction(ActionsManager.Instance.GetAction("Basic Action"));
            }
        }
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
            GameEvent.Instance.LeaveRoomSelection();
        }
    }

    public List<PlayerController> GetAllPlayers()
    {
        return allPlayers;
    }

    public void GameStart()
    {
        GameEvent.Instance.ToFirstEvent();
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
