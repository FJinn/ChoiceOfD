using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class VFXManager : Singleton<VFXManager>
{
    public enum EGeneralVFXType
    {
        GainHealth = 0,
        LoseHealth = 1,
        Feather = 2,
        Star = 3
    }

    [SerializeField] GameObject gainHealthVFXPrefab;
    [SerializeField] GameObject loseHealthVFXPrefab;
    [SerializeField] GameObject featherVFXPrefab;
    [SerializeField] GameObject starVFXPrefab;

    List<GameObject> gainHealthVFXs = new();
    List<GameObject> loseHealthVFXs = new();
    List<GameObject> featherVFXs = new();
    List<GameObject> starVFXs = new();

    public void PlayVFX(EGeneralVFXType vfxType, Vector3 pos, Action callback)
    {
        GameObject vfxObj = GetVFXObject(vfxType);
        vfxObj.transform.position = pos;
        vfxObj.SetActive(true);
        Debug.LogError("Called PlayVFX:: " + vfxType);
        StartCoroutine(VFXActiveUpdate(vfxObj, callback));
    }

    IEnumerator VFXActiveUpdate(GameObject targetObject, Action callback)
    {
        while(targetObject.activeInHierarchy)
        {
            yield return null;
        }

        callback?.Invoke();
    }

    GameObject GetVFXObject(EGeneralVFXType targetType)
    {
        List<GameObject> targetList = targetType switch
        {
            EGeneralVFXType.GainHealth => gainHealthVFXs,
            EGeneralVFXType.LoseHealth => loseHealthVFXs,
            EGeneralVFXType.Feather => featherVFXs,
            EGeneralVFXType.Star => starVFXs,
            _ => null
        };

        var found = targetList.Find(x => !x.activeInHierarchy);
        if(found != null)
        {
            return found;
        }

        GameObject targetPrefab = targetType switch
        {
            EGeneralVFXType.GainHealth => gainHealthVFXPrefab,
            EGeneralVFXType.LoseHealth => loseHealthVFXPrefab,
            EGeneralVFXType.Feather => featherVFXPrefab,
            EGeneralVFXType.Star => starVFXPrefab,
            _ => null
        };
        GameObject newObj = Instantiate(targetPrefab, transform);
        targetList.Add(newObj);
        return newObj;
    }
}
