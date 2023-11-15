using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    [SerializeField] List<EnemyObject> enemyObjects;
    [SerializeField] Transform basicEnemyParentTransform;

    List<EnemyObject> spawnedEnemies = new();

    public EnemyObject GetEnemyObject(EEnemyType _enemyType)
    {
        EnemyObject found = spawnedEnemies.Find(x => !x.IsActivated());

        if(found != null)
        {
            return found;
        }

        EnemyObject targetObject = enemyObjects.Find(x => x.enemyType == _enemyType);
        CharacterBase newCharacter = Instantiate(targetObject.character, Vector3.zero, Quaternion.identity, basicEnemyParentTransform);
        EnemyObject newSpawnedObject = new EnemyObject()
        {
            character = newCharacter,
            enemyType = _enemyType,
            enemyTexture2D = targetObject.enemyTexture2D
        };
        newCharacter.SetTexture2D(newSpawnedObject.enemyTexture2D);
        spawnedEnemies.Add(newSpawnedObject);
        return newSpawnedObject;
    }
}

[Serializable]
public class EnemyObject
{
    public CharacterBase character;
    public EEnemyType enemyType;
    public Texture2D enemyTexture2D;

    public bool IsActivated() => character.gameObject.activeInHierarchy;
}

public enum EEnemyType
{
    None = 0,
    Goblin = 1,
    HobGoblin = 2,
    Bugbear = 3
}