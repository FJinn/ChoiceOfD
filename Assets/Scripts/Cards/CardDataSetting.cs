using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(fileName = "CardSetting", menuName = "ScriptableObjects/CardSetting", order = 1)]
public class CardDataSetting : ScriptableObject
{
    public List<SO_Card.CombinationCardInfo> combinationList;

    [Button("SetUpAllSOCards")]
    public bool RunSetup;

    public void SetUpAllSOCards()
    {
        SO_Card[] allSOCards = FindAssetsByType<SO_Card>();
        foreach(SO_Card item in allSOCards)
        {
            foreach(var element in combinationList)
            {
                if(element.results.Contains(item) || !element.combinations.Contains(item))
                {
                    continue;
                }

                item.possibleCombinations.Add(new SO_Card.CombinationCardInfo()
                {
                    requiredSecondsToBeSpawned = element.requiredSecondsToBeSpawned,
                    percentageOfResultsAmount = element.percentageOfResultsAmount,
                    combinations = element.combinations,
                    results = element.results
                });
            }
            
            EditorUtility.SetDirty(item);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    // Helper method to find ScriptableObjects of a specific type using AssetDatabase
    T[] FindAssetsByType<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
        T[] assets = new T[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        return assets;
    }
}
#endif
