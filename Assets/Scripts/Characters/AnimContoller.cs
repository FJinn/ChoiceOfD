using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimContoller : MonoBehaviour
{
    [SerializeField] Animator animator;

    Coroutine animationRoutine;

    public void SetAnimator(Animator _animator)
    {
        animator = _animator;
    }

    public void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }

    public void PlayAnimation(int animID, Action callback)
    {
        animator.Play(animID);

        if(animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }
        animationRoutine = StartCoroutine(AnimationUpdate(callback));
    }

    public void SetParamBool(int animID, bool value)
    {
        animator.SetBool(animID, value);
    }

    IEnumerator AnimationUpdate(Action callback)
    {
        yield return null;
        while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }
        callback?.Invoke();
    }
}
