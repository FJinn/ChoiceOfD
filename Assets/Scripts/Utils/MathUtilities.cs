using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtilities
{
    public static float DistanceSquared(Vector3 firstVector, Vector3 secondVector)
    {
        float distanceSquared;
        Vector3 difference;
        difference.x = firstVector.x - secondVector.x;
        difference.y = firstVector.y - secondVector.y;
        difference.z = firstVector.z - secondVector.z;
   
        distanceSquared = difference.x * difference.x + difference.y * difference.y + difference.z * difference.z;
        return distanceSquared;
    }

    public static float DistanceSquared(Vector2 firstVector, Vector2 secondVector)
    {
        float distanceSquared;
        Vector3 difference;
        difference.x = firstVector.x - secondVector.x;
        difference.y = firstVector.y - secondVector.y;
   
        distanceSquared = difference.x * difference.x + difference.y * difference.y;
        return distanceSquared;
    }

    public static int RemainderOfFour(int value)
    {
        return value & 3;
    }
    
    public static int RemainderOfTwo(int value)
    {
        return value & 1;
    }

    public static float Smallest(float a, float b)
    {
        return a < b ? a : b;
    }

    public static bool ASmallerThanB(float a, float b)
    {
        return a < b;
    }

    public static bool FastApproximately(float a, float b, float threshold)
    {
        if (threshold > 0f)
        {
            return Mathf.Abs(a - b) <= threshold;
        }
        else
        {
            return Mathf.Approximately(a, b);
        }
    }
}
