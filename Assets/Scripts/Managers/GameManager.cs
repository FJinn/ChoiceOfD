using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public Action onGameOver;

    [SerializeField] PlayerController player;
    [SerializeField] List<ECharacterClass> initialCharacters;
    bool isGameOver;
    public bool IsGameOver() => isGameOver;
    public PlayerController GetPlayer() => player;

    public void FillParty()
    {
        if(initialCharacters.Count <= 0 || initialCharacters.Contains(ECharacterClass.None))
        {
            Debug.LogError("DebugManager:: Fill with classes contains 0 or None! Skipping!");
            return;
        }

        PlayerController player = PlayerController.Instance;
        int fillAmount = Mathf.Clamp(initialCharacters.Count, 1, 4);
        for(int i=0; i<fillAmount; ++i)
        {
            player.ObtainCharacter(initialCharacters[i]);
        }
    }

    void Start()
    {
        player.Initialize();
        player.AllCharactersSpawnInTavern();

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
        RoomManager.Instance.InitializeTavernRoom(()=>
        {
            RoomManager.Instance.PlayerCharactersEnterRoom(true, ()=>RoomManager.Instance.SpawnObjectsInRoom(false, FillParty));
        });
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
