using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMap : MonoBehaviour
{
    [SerializeField] Vector2 mapSize;
    [SerializeField] Transform backgroundTransform;

    public static Vector2 halfMapSize {private set; get;}
    public static Vector2 minMapPoint {private set; get;}
    public static Vector2 maxMapPoint {private set; get;}

    void Start()
    {
        halfMapSize = mapSize * 0.5f;
        minMapPoint = (Vector2)transform.position - halfMapSize;
        maxMapPoint = (Vector2)transform.position + halfMapSize;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, mapSize);
    }

    private void OnValidate()
    {
        backgroundTransform.localScale = mapSize;
    }
}
