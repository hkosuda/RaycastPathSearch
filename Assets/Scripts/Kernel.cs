using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kernel : MonoBehaviour
{
    static public GameObject Root { get; private set; }
    static public int UsedSeed { get; private set; } = -1;

    static int counter = 0;

    void Start()
    {
        counter = 0;
    }

    static public void Begin(bool mute)
    {
        counter++;

        SetSeed(mute);
        Setup(mute);

        FieldManager.Setup(mute);
        UnitManager.Setup(mute);
    }

    static void Setup(bool mute)
    {
        if (Root != null) { Destroy(Root); }

        if (mute) { return; }

        Root = new GameObject("Root");
    }

    static void SetSeed(bool mute)
    {
        int settingSeed = SettingSystem.Seed;

        if (mute)
        {
            UsedSeed = 3000 + counter;
        }

        else
        {
            if (settingSeed < 0)
            {
                UsedSeed = System.DateTime.Now.Millisecond;
            }

            else
            {
                UsedSeed = settingSeed;
            }
        }
        

        UnityEngine.Random.InitState(UsedSeed);
    }
}
