using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[Flags]
public enum TransitionFlags
{
    None,
    ActiveOnBegin = (1 << 0),
    InactiveOnEnd = (1 << 1),
    ActiveCanvasOnBegin = (1 << 2),
    InactiveCanvasOnEnd = (1 << 3),
    DirectToEnd = (1 << 5),
    SetReferentialOnBegin = (1 << 6),
    RebuildParentLayout = (1 << 7),
    IgnoreSounds = (1 << 8)
}

public class UITransitions : MonoBehaviour
{
    public GameObject targetGao;

    [Serializable]
    public class OptionalData<T> : Optional
    {
        public T origin;
        public T target;
        public AnimationCurve curve;

        [Interval(0.0f, 1.0f)] public Vector2 range = new Vector2(0.0f, 1.0f);

        public OptionalData()
        {

        }

        public OptionalData(T origin, T target)
        {
            this.origin = origin;
            this.target = target;
        }

        public float NormalizeCursor(float cursor)
        {
            cursor = (cursor - range.x) / (range.y - range.x);
            cursor = Mathf.Clamp01(cursor);
            return cursor;
        }
    }

    [Serializable]
    public class TransitionData
    {
        public string name;
        public float duration = 0.25f;
        public bool useGlobalReferential = false;
        public bool useMyReferential = false;
        public OptionalData<Vector3> scale = new OptionalData<Vector3>();
        public OptionalData<float> alpha = new OptionalData<float>(1.0f, 1.0f);
        public OptionalData<Vector3> position = new OptionalData<Vector3>();
        public OptionalData<Vector3> rotation = new OptionalData<Vector3>();

        public void OnValidate()
        {
            if(duration==0.0f && string.IsNullOrEmpty(name)) // Init
            {
                scale.range = new Vector2(0.0f, 1.0f);
                alpha.range = new Vector2(0.0f, 1.0f);
            }
        }
    }

    public List<TransitionData> transitionDatas = new List<TransitionData>();
    public string currentTransitionName => currentTransition?.name;

    private TransitionData currentTransition;
    private TransitionFlags currentFlags;
    private float currentTime;
    private UnityAction<GameObject, string> currentCallback;

    private bool isInit;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Graphic graphicComponent;
    private SpriteRenderer spriteRenderer;
    private Vector3 referentialScale = Vector3.one;
    private float referentialAlpha = 1.0f;
    private Quaternion referentialLocalRotation = Quaternion.identity;
    private Vector3 referentialLocalPosition = Vector3.zero;
    private Vector3 referentialAnchoredPosition = Vector3.zero;

    private void OnValidate()
    {
        foreach(var transitionData in transitionDatas)
        {
            transitionData.OnValidate();
        }
    }

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (isInit)
            return;

        if (targetGao == null)
            targetGao = gameObject;

        canvas = targetGao.GetComponentInParent<Canvas>(true);
        canvasGroup = targetGao.GetComponent<CanvasGroup>();
        graphicComponent = targetGao.GetComponent<Graphic>();
        spriteRenderer = targetGao.GetComponent<SpriteRenderer>();
        rectTransform = targetGao.GetComponent<RectTransform>();
        isInit = true;
        enabled = false;

        SetReferential();
    }

    private void OnDisable()
    {
        if(currentTransition!=null)
        {
            Apply(currentTransition.duration);
        }
    }

    public void SetReferential()
    {
        if (!isInit)
            return;

        referentialScale = targetGao.transform.localScale;
        referentialLocalPosition = targetGao.transform.localPosition;
        referentialLocalRotation = targetGao.transform.rotation;

        if (rectTransform)
        {
            referentialAnchoredPosition = rectTransform.anchoredPosition3D;
        }

        if (canvasGroup)
        {
            referentialAlpha = canvasGroup.alpha;
        }
        else if (graphicComponent)
        {
            referentialAlpha = graphicComponent.color.a;
        }
        else if (spriteRenderer)
        {
            referentialAlpha = spriteRenderer.color.a;
        }
    }

    public void RestoreReferential()
    {
        if (!isInit)
            return;

        targetGao.transform.localScale = referentialScale;
        targetGao.transform.localRotation = referentialLocalRotation;

        if (rectTransform)
        {
            rectTransform.anchoredPosition3D = referentialAnchoredPosition;
        }
        else
        {
            targetGao.transform.localPosition = referentialLocalPosition;
        }

        if (canvasGroup)
        {
            canvasGroup.alpha = referentialAlpha;
        }
        else if (graphicComponent)
        {
            Color resultColor = graphicComponent.color;
            resultColor.a = referentialAlpha;
            graphicComponent.color = resultColor;
        }
        else if (spriteRenderer)
        {
            Color resultColor = spriteRenderer.color;
            resultColor.a = referentialAlpha;
            spriteRenderer.color = resultColor;
        }
    }

    public bool IsAtReferential()
    {
        if (!isInit)
            return true;

        if (targetGao.transform.localScale != referentialScale)
            return false;

        if (targetGao.transform.localRotation !=referentialLocalRotation)
            return false;

        if (rectTransform)
        {
            if (referentialAnchoredPosition != rectTransform.anchoredPosition3D)
                return false;
        }
        else
        {
            if (targetGao.transform.localPosition != referentialLocalPosition)
                return false;
        }

        if (canvasGroup)
        {
            if (canvasGroup.alpha != referentialAlpha)
                return false;
        }
        else if (graphicComponent)
        {
            if (graphicComponent.color.a != referentialAlpha)
                return false;
        }
        else if (spriteRenderer)
        {
            if (spriteRenderer.color.a != referentialAlpha)
                return false;
        }

        return true;
    }

    public TransitionData GetTransition(string transitionName)
    {
        TransitionData data = transitionDatas.Find(x => x.name == transitionName);
        return data;
    }

    public void AddTransition(TransitionData transitionData)
    {
        if (transitionData == null)
            return;

        transitionDatas.Add(transitionData);
    }
    
    /// <summary>
    /// to use with unityevent in inspector
    /// </summary>
    /// <param name="transitionName">transition name</param>
    public void PlayTransition_ActiveOnBegin(string transitionName)=>
        Play(transitionName,TransitionFlags.ActiveOnBegin);
    
    /// <summary>
    /// to use with unityevent in inspector
    /// </summary>
    /// <param name="transitionName">transition name</param>
    public void PlayTransition_InactiveOnEnd(string transitionName)=>
        Play(transitionName,TransitionFlags.InactiveOnEnd);    
    
    /// <summary>
    /// to use with unityevent in inspector
    /// </summary>
    /// <param name="transitionName">transition name</param>
    public void PlayTransition(string transitionName)=>
        Play(transitionName,TransitionFlags.None);

    public bool Play(string transitionName, TransitionFlags transitionFlags = TransitionFlags.None, UnityAction<GameObject, string> endCallback = null)
    {
        Init();

        TransitionData transitionData = GetTransition(transitionName);
        if (transitionData == null)
        {
            Debug.LogWarning($"Transition with name \"{transitionName}\" not found", this);
            Stop();

            if (transitionFlags.HasFlag(TransitionFlags.ActiveCanvasOnBegin) && canvas != null)
            {
                canvas.gameObject.SetActive(true);
            }

            if (transitionFlags.HasFlag(TransitionFlags.ActiveOnBegin))
            {
                targetGao.SetActive(true);
            }

            if (transitionFlags.HasFlag(TransitionFlags.InactiveOnEnd))
            {
                targetGao.SetActive(false);
            }

            if (transitionFlags.HasFlag(TransitionFlags.InactiveCanvasOnEnd) && canvas != null)
            {
                canvas.gameObject.SetActive(false);
            }

            if (endCallback != null)
            {
                endCallback.Invoke(gameObject, transitionName);
            }

            return false;
        }

        currentTransition = transitionData;
        currentFlags = transitionFlags;
        currentTime = 0.0f;
        currentCallback = endCallback;
        enabled = true;

        if (currentFlags.HasFlag(TransitionFlags.SetReferentialOnBegin))
        {
            SetReferential();
        }

        if (currentFlags.HasFlag(TransitionFlags.ActiveOnBegin))
        {
            targetGao.SetActive(true);
        }

        if (currentFlags.HasFlag(TransitionFlags.ActiveCanvasOnBegin) && canvas!=null)
        {
            canvas.gameObject.SetActive(true);
        }

        if (currentFlags.HasFlag(TransitionFlags.DirectToEnd))
        {
            currentTime = currentTransition.duration;
        }

        if(!currentFlags.HasFlag(TransitionFlags.IgnoreSounds))
        {
            // transitionData.startSoundEvent?.Post(targetGao);
        }
        
        Apply(currentTime);

        
        return true;
    }

    public void Stop()
    {
        currentTransition = null;
        enabled = false;
    }

    public bool IsInTransition(params string[] transitionNames)
    {
        if (currentTransition != null && (transitionNames.Length==0 || transitionNames.Contains(currentTransition.name)))
            return true;

        return false;
    }

    void Apply(float time)
    {

        float cursor = currentTransition.duration > 0.0f ? time / currentTransition.duration : 1.0f;

        if (currentTransition.scale.Enabled)
        {
            float t = currentTransition.scale.NormalizeCursor(cursor);
            t = currentTransition.scale.curve.Evaluate(t);
            Vector3 scale = Vector3.LerpUnclamped(currentTransition.scale.origin, currentTransition.scale.target, t);
            Vector3 refScale = currentTransition.useGlobalReferential ? Vector3.one : referentialScale;
            targetGao.transform.localScale = Vector3.Scale(refScale, scale);
        }

        if (currentTransition.alpha.Enabled && (canvasGroup != null || graphicComponent != null || spriteRenderer!=null))
        {
            float t = currentTransition.alpha.NormalizeCursor(cursor);
            t = currentTransition.alpha.curve.Evaluate(t);
            float alpha = Mathf.LerpUnclamped(currentTransition.alpha.origin, currentTransition.alpha.target, t);
            float refAlpha = currentTransition.useGlobalReferential ? 1.0f : referentialAlpha;
            if (canvasGroup)
                canvasGroup.alpha = refAlpha * alpha;
            else if (graphicComponent)
            {
                Color targetColor = graphicComponent.color;
                targetColor.a = refAlpha * alpha;
                graphicComponent.color = targetColor;
            }
            else if (spriteRenderer)
            {
                Color targetColor = spriteRenderer.color;
                targetColor.a = refAlpha * alpha;
                spriteRenderer.color = targetColor;
            }
        }

        if (currentTransition.position.Enabled)
        {
            float t = currentTransition.position.NormalizeCursor(cursor);
            t = currentTransition.position.curve.Evaluate(cursor);
            Vector3 position = Vector3.LerpUnclamped(currentTransition.position.origin, currentTransition.position.target, t);

            if (currentTransition.useMyReferential)
                position = referentialLocalRotation * position;

            if (rectTransform!=null)
            {
                Vector3 refPos = currentTransition.useGlobalReferential ? Vector3.zero : referentialAnchoredPosition;
                rectTransform.anchoredPosition3D = refPos + position;
            }
            else
            {
                Vector3 refPos = currentTransition.useGlobalReferential ? Vector3.zero : referentialLocalPosition;
                targetGao.transform.localPosition = refPos + position;
            }
        }

        if(currentTransition.rotation.Enabled)
        {
            float t = currentTransition.rotation.NormalizeCursor(cursor);
            t = currentTransition.rotation.curve.Evaluate(cursor);
            Quaternion rot = Quaternion.Euler(Vector3.LerpUnclamped(currentTransition.rotation.origin, currentTransition.rotation.target, t));
            Quaternion refRot = currentTransition.useGlobalReferential ? Quaternion.identity : referentialLocalRotation;
            targetGao.transform.localRotation = refRot * rot;
        }

        if(currentFlags.HasFlag(TransitionFlags.RebuildParentLayout) && targetGao.transform.parent!=null)
        {
            LayoutRebuilder.MarkLayoutForRebuild(targetGao.transform.parent.GetComponent<RectTransform>());
        }

        if (cursor >= 1.0f)
        {
            TransitionData transitionData = currentTransition;
            currentTransition = null;

            if (currentFlags.HasFlag(TransitionFlags.InactiveCanvasOnEnd) && canvas != null)
            {
                canvas.gameObject.SetActive(false);
            }

            if (currentFlags.HasFlag(TransitionFlags.InactiveOnEnd))
            {
                targetGao.SetActive(false);
            }

            enabled = false; // Be carefull where it's called because OnDisabled() code

            if (currentCallback != null)
            {
                currentCallback.Invoke(gameObject, transitionData.name);
            }            
        }
    }

    void Update()
    {
        if (currentTransition == null)
            return;

        currentTime = Mathf.Min(currentTime + Time.unscaledDeltaTime, currentTransition.duration);
        Apply(currentTime);
    }

    public static bool Play(GameObject gao, string transitionName, TransitionFlags transitionFlags = TransitionFlags.None, UnityAction<GameObject, string> endCallback = null)
    {
        if (gao == null)
            return false;

        var simpleTransitions = gao.GetComponent<UITransitions>();
        if (simpleTransitions == null)
            simpleTransitions = gao.AddComponent<UITransitions>();// In order to handle transitionFlags and endCallback

        return simpleTransitions.Play(transitionName, transitionFlags, endCallback);
    }

    public static bool Play(Component component, string transitionName, TransitionFlags transitionFlags = TransitionFlags.None, UnityAction<GameObject, string> endCallback = null)
    {
        if (component == null)
            return false;

        return Play(component.gameObject, transitionName, transitionFlags, endCallback);
    }

    public static bool IsInTransition(GameObject gao, params string[] transitionNames)
    {
        var simpleTransitions = gao.GetComponent<UITransitions>();
        if (simpleTransitions != null && simpleTransitions.IsInTransition(transitionNames))
            return true;

        return false;
    }

    public static bool IsInTransition(Component component, params string[] transitionNames)
    {
        if (component == null)
            return false;

        return IsInTransition(component.gameObject, transitionNames);
    }

    public static void RestoreReferential(GameObject gao)
    {
        if (gao == null)
            return;

        var simpleTransitions = gao.GetComponent<UITransitions>();
        if (simpleTransitions != null)
            simpleTransitions.RestoreReferential();
    }

    public static void RestoreReferential(Component component)
    {
        if (component == null)
            return;

        RestoreReferential(component.gameObject);
    }

    public static void SetReferential(GameObject gao)
    {
        if (gao == null)
            return;

        var simpleTransitions = gao.GetComponent<UITransitions>();
        if (simpleTransitions != null)
            simpleTransitions.SetReferential();
    }

    public static void SetReferential(Component component)
    {
        if (component == null)
            return;

        SetReferential(component.gameObject);
    }
}
