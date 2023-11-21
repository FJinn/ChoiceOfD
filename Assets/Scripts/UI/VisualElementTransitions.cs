using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using Random = UnityEngine.Random;

public class VisualElementTransitions : Singleton<VisualElementTransitions>
{    
    public void LerpVector(VisualElementVectorParams visualElementPositionParams)
    {
        if(visualElementPositionParams.routineCache != null)
        {
            StopCoroutine(visualElementPositionParams.routineCache);
        }

        visualElementPositionParams.routineCache = StartCoroutine(LerpVectorUpdate(visualElementPositionParams));
    }

    IEnumerator LerpVectorUpdate(VisualElementVectorParams visualElementVectorParams)
    {
        float delta = 0;
        Vector3 initialVector = visualElementVectorParams.vectorType switch
        {
            VisualElementVectorParams.EVectorType.Position => visualElementVectorParams.visualElement.transform.position,
            VisualElementVectorParams.EVectorType.Scale => visualElementVectorParams.visualElement.transform.scale,
            VisualElementVectorParams.EVectorType.Opacity => new Vector3(visualElementVectorParams.visualElement.style.opacity.value,0,0),
            _ => Vector3.zero
        };

        Debug.LogError("started LerpVectorUpdate");
        while(delta < visualElementVectorParams.durationToReach)
        {
            Vector3 newVector = Vector3.Lerp(initialVector, visualElementVectorParams.targetVector, delta / visualElementVectorParams.durationToReach);
            switch(visualElementVectorParams.vectorType)
            {
                case VisualElementVectorParams.EVectorType.Position:
                    visualElementVectorParams.visualElement.transform.position = newVector;
                    break;
                case VisualElementVectorParams.EVectorType.Scale:
                    visualElementVectorParams.visualElement.transform.scale = newVector;
                    break;
                case VisualElementVectorParams.EVectorType.Opacity:
                    visualElementVectorParams.visualElement.style.opacity = newVector.x;
                    break;
            }
            
            delta += Time.deltaTime;
            yield return null;
        }

        switch(visualElementVectorParams.vectorType)
        {
            case VisualElementVectorParams.EVectorType.Position:
                visualElementVectorParams.visualElement.transform.position = visualElementVectorParams.targetVector;
                break;
            case VisualElementVectorParams.EVectorType.Scale:
                visualElementVectorParams.visualElement.transform.scale = visualElementVectorParams.targetVector;
                break;
            case VisualElementVectorParams.EVectorType.Opacity:
                visualElementVectorParams.visualElement.style.opacity = visualElementVectorParams.targetVector.x;
                break;
        }
        Debug.LogError("Done calback");
        visualElementVectorParams.onEndCallback?.Invoke();
        visualElementVectorParams.routineCache = null;
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

public class VisualElementVectorParams
{
    public Coroutine routineCache;
    public VisualElement visualElement;
    public Action onEndCallback;
    /// <summary>
    /// use x axis for opacity value
    /// </summary>
    public Vector3 targetVector;
    public float durationToReach;
    public EVectorType vectorType;
    public enum EVectorType
    {
        Position = 0,
        Scale = 1,
        Opacity = 2
    }
}

public class VisualElementShakeParams
{
    public Coroutine routineCache;
    public VisualElement visualElement;
    public Action onEndCallback;
    public float shakeDuration;
    public float shakeDistance;
}