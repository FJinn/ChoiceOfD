using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Stats", order = 1)]
public class StatsData : ScriptableObject
{
    public List<StatsInfo> characterStats;
}

public class StatsInfo
{
    public string characterName = "";
}
