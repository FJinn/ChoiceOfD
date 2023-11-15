using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] Rigidbody body;
    [SerializeField] AnimContoller animContoller;
    Coroutine moveRoutine;
    
    static readonly int runningAnimID = Animator.StringToHash("IsRunning");

    float boundaryRadius;
    float boundaryRadiusSquared;
    Vector3 boundaryCenter;

    bool includeBoundary;

    public void SetBoundaries(Vector3 center, float radius)
    {
        boundaryRadius = radius;
        boundaryRadiusSquared = boundaryRadius * boundaryRadius;
        boundaryCenter = center;
    }

    public void SetHasBoundary(bool value) => includeBoundary = value;
    public void SetHasGravity(bool value)
    {
        body.useGravity = value;
    }

    bool WithinBoundaries(Vector3 targetPos)
    {
        if(MathUtilities.DistanceSquared(targetPos, boundaryCenter) >= boundaryRadiusSquared) return false;

        return true;
    }

    public void SetPositionAndRotation(Vector3 target, Quaternion rotation)
    {
        transform.position = target;
        transform.rotation = rotation;
    }

    /// <summary>
    /// Didn't account for animation and rotation
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="speed"></param>
    public void Move(Vector3 direction, float speed)
    {
        Vector3 newPos = transform.position + direction * speed * Time.deltaTime;
        if(includeBoundary && !WithinBoundaries(newPos))
        {
            return;
        }
        transform.position = newPos;
    }

    public void LookAt(Vector3 direction)
    {
        transform.LookAt(transform.position + direction, Vector3.up);
    }

    public void MoveTo_Speed(Vector3 pos, float speed, Action doneMoveCallback)
    {
        float duration = Vector3.Distance(pos, transform.position) / speed;
        MoveTo_Duration(pos, duration, doneMoveCallback);
    }

    public void MoveTo_Duration(Vector3 pos, float duration, Action doneMoveCallback)
    {
        if(moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        moveRoutine = StartCoroutine(MoveUpdate(pos, duration, doneMoveCallback));
    }

    public void MoveToDirection(Vector3 pos, float speed)
    {
        if(moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        moveRoutine = StartCoroutine(DirectionMoveUpdate((transform.position - pos).normalized, speed));
    }

    public void JumpTo(Vector3 initialPos, Vector3 targetPos, float jumpSpeed = 5f, float height = 1f)
    {
        if(moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }
        moveRoutine = StartCoroutine(JumpUpdate(initialPos, targetPos, jumpSpeed, height));
    }

    IEnumerator MoveUpdate(Vector3 pos, float duration, Action doneMoveCallback)
    {
        float delta = 0;
        Vector3 initialPos = transform.position;
        Vector3 targetPos = pos;
        LookAt(targetPos - initialPos);
        animContoller.SetParamBool(runningAnimID, true);

        while(delta < 1)
        {
            Vector3 newPos = Vector3.Lerp(initialPos, targetPos, delta);

            if(includeBoundary && !WithinBoundaries(newPos))
            {
                yield break;
            }
            transform.position = newPos;
            delta += Time.deltaTime / duration;

            yield return null;
        }
        animContoller.SetParamBool(runningAnimID, false);

        doneMoveCallback?.Invoke();
    }

    IEnumerator DirectionMoveUpdate(Vector3 dir, float speed)
    {
        animContoller.SetParamBool(runningAnimID, true);
        while(gameObject.activeInHierarchy)
        {
            Vector3 newPos = transform.position + dir * Time.deltaTime * speed;

            if(includeBoundary && !WithinBoundaries(newPos))
            {
                animContoller.SetParamBool(runningAnimID, false);
                yield break;
            }
            transform.position = newPos;

            yield return null;
        }
        animContoller.SetParamBool(runningAnimID, false);
    }

    IEnumerator JumpUpdate(Vector3 initialPos, Vector3 targetPos, float jumpSpeed = 5f, float height = 1f)
    {
        bool gravityStatus = body.useGravity;
        body.useGravity = false;

        float z0 = initialPos.z;
        float z1 = targetPos.z;
        float dist = z1 - z0;
        while(MathUtilities.DistanceSquared(transform.position, targetPos) > 1)
        {
            float nextZ = Mathf.MoveTowards(transform.position.z, z1, jumpSpeed * Time.deltaTime);
            float baseY = Mathf.Lerp(initialPos.y, targetPos.y, (nextZ - z0) / dist);
            float arc = height * (nextZ - z0) * (nextZ - z1) / (-0.25f * dist * dist);

            float x0 = initialPos.x;
            float x1 = targetPos.x;
            float nextX = Mathf.MoveTowards(transform.position.x, x1, jumpSpeed * Time.deltaTime);

            Vector3 nPos = new Vector3(nextX, baseY + arc, nextZ);

            if(includeBoundary && !WithinBoundaries(nPos))
            {
                body.useGravity = gravityStatus;
                yield break;
            }

            transform.position = nPos;
            yield return null;
        }
        body.useGravity = gravityStatus;
    }
}
