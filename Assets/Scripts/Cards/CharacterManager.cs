using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : Singleton<CharacterManager>
{
    [SerializeField] CharacterCardUI characterCardUITemplate;
    [SerializeField] Transform characterCardUIParent;

    List<CharacterCardUI> cardUIPool = new();
    List<CardController> allCharacters = new();
    List<CardController> charactersInICU = new();

    public void AddCharacterIntoICU(CardController _character)
    {
        charactersInICU.Add(_character);

        CharacterCardUI cardUI = GetCardUI();
        cardUI.LinkToCharacter(_character.GetCardData().characterLocalData);
    }

    public void RemoveCharacterFromICU(CardController _character)
    {
        charactersInICU.Remove(_character);
        cardUIPool.Find(x => x.IsLinkedWithCharacter(_character.GetCardData().characterLocalData));
    }

    public void RegisterCharacter(CardController _character, bool addIntoICU = false)
    {
        allCharacters.Add(_character);
        if(addIntoICU)
        {
            AddCharacterIntoICU(_character);
        }
    }

    public void UnregisterCharacter(CardController _character)
    {
        allCharacters.Remove(_character);
    }
    
    CharacterCardUI GetCardUI()
    {
        CharacterCardUI found = cardUIPool.Find(x => x.IsAvailable());

        if(found != null)
        {
            return found;
        }

        CharacterCardUI newCardUI = Instantiate(characterCardUITemplate, characterCardUIParent);
        cardUIPool.Add(newCardUI);
        return newCardUI;
    }
}
