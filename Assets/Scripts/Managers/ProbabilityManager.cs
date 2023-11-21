using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class ProbabilityManager
{
    public static int RollD20()
    {
        return Random.Range(1, 21);
    }

    public static bool RandomPercentage(float target)
    {
        return Random.Range(0, 0.999f) < target;
    }
}
