using UnityEngine;

public class ProgressBar2D : MonoBehaviour
{
    CardController linkedCard;

    [SerializeField] SpriteRenderer fillSpriteRenderer;

    public bool IsAvailable() => !gameObject.activeInHierarchy;

    void Update()
    {
        if(linkedCard == null)
        {
            Deactivate();
            return;
        }

        transform.position = linkedCard.GetProgressBarWorldPosition();

        // Adjust the width of the sprite based on the progress
        float newWidth = Mathf.Lerp(0f, 1f, linkedCard.GetCardData().currentProgressValue);
        fillSpriteRenderer.size = new Vector2(newWidth, fillSpriteRenderer.size.y);

        if(linkedCard.GetCardData().currentProgressValue >= 1)
        {
            Deactivate();
        }
    }

    public void Activate(CardController card)
    {
        linkedCard = card;

        gameObject.SetActive(true);
    }

    void Deactivate()
    {
        linkedCard = null;
        gameObject.SetActive(false);
    }
}