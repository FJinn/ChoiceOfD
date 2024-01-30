using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Card", menuName = "ScriptableObjects/CardData", order = 1)]
public class SO_Card : ScriptableObject
{
    public ECardType cardType;

    public int totalStackableCardAmount;
    
    public NormalCardData normalCardData;
    public CharacterCardData characterCardData;

    //ToDo:: specific user, card type for different uses etc

    [HideInInspector, SerializeField] public List<CombinationCardInfo> possibleCombinations;

    [Serializable]
    public class CombinationCardInfo
    {
        public float requiredSecondsToBeSpawned;
        public float percentageOfResultsAmount = 1f;

        public List<SO_Card> results;
        public List<SO_Card> combinations;
    }

#if UNITY_EDITOR
    public void CheckCombinations()
    {
        foreach(var item in possibleCombinations)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var element in item.combinations)
            {
                stringBuilder.Append(element.name).Append(",");
            }
            // Check if there are characters in the StringBuilder
            if (stringBuilder.Length > 0)
            {
                // Remove the last character
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            Debug.Log($"{stringBuilder} ==> {item.results}");
        }
    }
#endif
}

public enum ECardType
{
    Normal = 0,
    Character = 1
}

[Serializable]
public class NormalCardData
{
    public int totalAliveDay;
    public int totalSpawnableCardAmount;
}

[Serializable]
public class CharacterCardData
{
    public int initialHealth;
    public int maxHealth;
    public float speed;
    public float stealth;
    public float initialSatiety;
    public ECondition condition;
    public EOccupation occupation;
    public float basePower;
    public SO_Card weapon;
    public SO_Card armour;
}

public enum ECondition
{
    None = 0,
    Satiated = 1,
    Sick = 2,
    Injured = 3
}

public enum EOccupation
{
    None = 0
}
