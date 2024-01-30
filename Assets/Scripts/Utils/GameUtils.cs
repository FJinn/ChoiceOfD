using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUtils
{
    // Fisher-Yates shuffle algorithm
    public static void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public static Vector2 CheckForCollisionsAndReposition(Vector2 position, Vector2 boxSize, LayerMask collisionLayer, GameObject instigator)
    {
        if(Physics2D.OverlapBox(position, boxSize, collisionLayer))
        {
            // Adjust the position of the current object to prevent stacking
            Vector2 newPosition = FindEmptyPosition(10, position, collisionLayer, instigator);
            return newPosition;
        }

        return position;

        /*
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, boxColliderBoundsSize, CardManager.cardInteractLayer);

        foreach (var collider in colliders)
        {
            if (collider.gameObject != gameObject)
            {
                // Adjust the position of the current object to prevent stacking
                Vector2 newPosition = GetRandomPosition();
                transform.position = newPosition;
            }
        }
        */
    }

    static Vector2 GetRandomPosition()
    {
        // Modify this method to return a random position where you want to place your objects
        return new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f));
    }

    static Vector2 FindEmptyPosition(int maxAttempts, Vector2 failedPosition, LayerMask collisionLayer, GameObject instigator)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 newCandidatePosition = GetRandomPosition();

            // Check if the new position doesn't overlap with any colliders
            Collider2D[] colliders = Physics2D.OverlapCircleAll(newCandidatePosition, 0.1f, collisionLayer);

            if (ArrayDoesNotContainSelf(colliders, instigator))
            {
                return newCandidatePosition;
            }
        }

        // If after maxAttempts, no suitable position is found, return the original position
        return failedPosition;
    }

    static bool ArrayDoesNotContainSelf(Collider2D[] colliders, GameObject instigator)
    {
        foreach (var collider in colliders)
        {
            if (collider.gameObject != instigator)
            {
                return false;
            }
        }
        return true;
    }
}
