using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.UIElements;

using Random = UnityEngine.Random;

public class VisualElementTransitions : Singleton<VisualElementTransitions>
{
    List<RoutineInfo> routineList = new List<RoutineInfo>();

    class RoutineInfo
    {
        public Coroutine routineCache;
        public string id;
        public ETransitionType transitionType;
    }

    enum ETransitionType
    {
        Position = 0,
        Scale = 1,
        Opacity = 2,
        Shake = 3
    }

    public void LerpVector(VisualElementVectorParams visualElementVectorParams)
    {
        ETransitionType _transitionType = GetTransitionType(visualElementVectorParams.vectorType);
        string targetID = visualElementVectorParams.id;

        RoutineInfo foundRoutine = routineList.Find(x => x.id == targetID && x.transitionType == _transitionType);
        if(foundRoutine != null)
        {
            StopCoroutine(foundRoutine.routineCache);
        }
        else
        {
            foundRoutine = new RoutineInfo()
            {
                id = targetID,
                transitionType = _transitionType
            };
            routineList.Add(foundRoutine);
        }

        foundRoutine.routineCache = StartCoroutine(LerpVectorUpdate(foundRoutine, visualElementVectorParams));
    }

    IEnumerator LerpVectorUpdate(RoutineInfo routineInfo, VisualElementVectorParams visualElementVectorParams)
    {
        float delta = 0;
        Vector3 initialVector = visualElementVectorParams.vectorType switch
        {
            VisualElementVectorParams.EVectorType.Position => visualElementVectorParams.visualElement.transform.position,
            VisualElementVectorParams.EVectorType.Scale => visualElementVectorParams.visualElement.transform.scale,
            VisualElementVectorParams.EVectorType.Opacity => new Vector3(visualElementVectorParams.visualElement.style.opacity.value,0,0),
            _ => Vector3.zero
        };

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

        visualElementVectorParams.onEndCallback?.Invoke();
        routineList.Remove(routineInfo);
    }

    public void ShakePosition(VisualElementShakeParams visualElementShakeParams)
    {
        ETransitionType _transitionType = GetTransitionType(VisualElementVectorParams.EVectorType.Shake);
        string targetID = visualElementShakeParams.id;

        RoutineInfo foundRoutine = routineList.Find(x => x.id == targetID && x.transitionType == _transitionType);
        if(foundRoutine != null)
        {
            StopCoroutine(foundRoutine.routineCache);
        }
        else
        {
            foundRoutine = new RoutineInfo()
            {
                id = targetID,
                transitionType = _transitionType
            };
            
            routineList.Add(foundRoutine);
        }

        foundRoutine.routineCache = StartCoroutine(ShakeUpdate(foundRoutine, visualElementShakeParams));
    }

    private IEnumerator ShakeUpdate(RoutineInfo routineInfo, VisualElementShakeParams visualElementShakeParams)
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
        routineList.Remove(routineInfo);
    }

    ETransitionType GetTransitionType(VisualElementVectorParams.EVectorType vectorType)
    {
        return vectorType switch
        {
            VisualElementVectorParams.EVectorType.Position => ETransitionType.Position,
            VisualElementVectorParams.EVectorType.Scale => ETransitionType.Scale,
            VisualElementVectorParams.EVectorType.Opacity => ETransitionType.Opacity,
            _ => ETransitionType.Shake
        };
    }
}

public class VisualElementVectorParams
{
    public string id;
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
        Shake = -1,
        Position = 0,
        Scale = 1,
        Opacity = 2
    }
}

public class VisualElementShakeParams
{
    public string id;
    public VisualElement visualElement;
    public Action onEndCallback;
    public float shakeDuration;
    public float shakeDistance;
}