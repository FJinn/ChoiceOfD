using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    [SerializeField] BasicEnemy basicEnemy;
    [SerializeField] Transform basicEnemyParentTransform;

    List<BasicEnemy> spawnedBasicEnemies = new();

    public BasicEnemy GetBasicEnemy()
    {
        BasicEnemy found = spawnedBasicEnemies.Find(x => !x.gameObject.activeInHierarchy);

        if(found != null)
        {
            return found;
        }

        found = Instantiate(basicEnemy, Vector3.zero, Quaternion.identity, basicEnemyParentTransform);
        found.gameObject.name += spawnedBasicEnemies.Count.ToString();
        spawnedBasicEnemies.Add(found);
        return found;
    }
}
