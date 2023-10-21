using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeManager : Singleton<EscapeManager>
{
    public delegate void callback();

    struct CbInfos
    {
        public callback cb;
    }

    List<CbInfos> cbInfos = new();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var nb = Instance.cbInfos.Count;
            if (nb > 0)
            {
                var cbstr = Instance.cbInfos[nb - 1];
                cbstr.cb.Invoke();
            }
        }
    }

    public static void Register(callback cb)
    {
        Instance.cbInfos.Add(new CbInfos() {cb = cb});
    }

    public static void UnRegister(callback cb)
    {
        for (int num = Instance.cbInfos.Count - 1; num >= 0; num--)
        {
            if (Instance.cbInfos[num].cb.Equals(cb))
            {
                Instance.cbInfos.RemoveAt(num);
                break;
            }
        }
    }
}