using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using Random = UnityEngine.Random;

public class VisualElementTransitions : Singleton<VisualElementTransitions>
{    
    public void LerpPosition(VisualElementPositionParams visualElementPositionParams)
    {
        if(visualElementPositionParams.routineCache != null)
        {
            StopCoroutine(visualElementPositionParams.routineCache);
        }

        visualElementPositionParams.routineCache = StartCoroutine(LerpPositionUpdate(visualElementPositionParams));
    }

    IEnumerator LerpPositionUpdate(VisualElementPositionParams visualElementPositionParams)
    {
        float delta = 0;
        Vector3 initialPos = visualElementPositionParams.visualElement.transform.position;

        while(delta < 1)
        {
            visualElementPositionParams.visualElement.transform.position = Vector3.Lerp(initialPos, visualElementPositionParams.targetPosition, delta / visualElementPositionParams.durationToReach);
            delta += Time.deltaTime;
            yield return null;
        }

        visualElementPositionParams.visualElement.transform.position = visualElementPositionParams.targetPosition;
        visualElementPositionParams.onEndCallback?.Invoke();
        visualElementPositionParams.routineCache = null;
    }

    public void ShakePosition(VisualElementShakeParams visualElementShakeParams)
    {
        if(visualElementShakeParams.routineCache != null)
        {
            StopCoroutine(visualElementShakeParams.routineCache);
        }

        visualElementShakeParams.routineCache = StartCoroutine(ShakeUpdate(visualElementShakeParams));
    }

    private IEnumerator ShakeUpdate(VisualElementShakeParams visualElementShakeParams)
    {
        float timer = 0f;
        Vector3 initialPos = visualElementShakeParams.visualElement.transform.position;
        Vector3 startPos = initialPos;
        Vector3 randomPos;
        float delta = 0;
        while (timer < visualElementShakeParams.shakeDuration)
        {
            timer += Time.deltaTime;

            startPos = visualElementShakeParams.visualElement.transform.position;
            randomPos = initialPos + (Random.insideUnitSphere * visualElementShakeParams.shakeDistance);
            randomPos.z = 0;
            delta = 0f;
            while(delta < 0.01f)
            {
                delta += Time.deltaTime;
                visualElementShakeParams.visualElement.transform.position = Vector3.Lerp(startPos, randomPos, delta);

                yield return null;
            }
        }

        visualElementShakeParams.visualElement.transform.position = initialPos;

        visualElementShakeParams.onEndCallback?.Invoke();
        visualElementShakeParams.routineCache = null;
    }
 
}

public class VisualElementPositionParams
{
    public Coroutine routineCache;
    public VisualElement visualElement;
    public Action onEndCallback;
    public Vector3 targetPosition;
    public float durationToReach;
}

public class VisualElementShakeParams
{
    public Coroutine routineCache;
    public VisualElement visualElement;
    public Action onEndCallback;
    public float shakeDuration;
    public float shakeDistance;
}