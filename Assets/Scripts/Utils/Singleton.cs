using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    protected static bool bQuitting { get; private set; }

    private static T _instance;

    public static T Instance
    {
        get
        {
            if (bQuitting)
            {
                return null;
            }

            return _instance;
        }
    }

    protected virtual void Awake() 
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else
        {
            Destroy(this.gameObject);
        }
        
        // DontDestroyOnLoad(gameObject);
    }
    
    private void OnApplicationQuit()
    {
        bQuitting = true;
    }
}
