using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CardManager : Singleton<CardManager>
{
    [ReadOnly, SerializeField] CardController currentSelectedCard;

    [SerializeField] CardController cardTemplate;
    [SerializeField] float gapBetweenStackCard = 0.4f;
    [SerializeField] LayerMask _cardInteractLayer;
    
    List<CardController> cardPool = new();
    Camera cam;

    Coroutine moveCardWithMouseRoutine;

    public Vector3 stackCardOffset {private set; get;}
    public static LayerMask cardInteractLayer => Instance._cardInteractLayer;
    public bool isMovingCard {private set; get;}

    const string CardSaveFileName = "AllCards";

    void Start()
    {
        cam = Camera.main;

        stackCardOffset = Vector3.down * gapBetweenStackCard;
    }

    public void SetSelectedCard(CardController card)
    {
        currentSelectedCard = card;

        if(moveCardWithMouseRoutine != null)
        {
            StopCoroutine(moveCardWithMouseRoutine);
        }
        moveCardWithMouseRoutine = StartCoroutine(MoveCardWithMouseUpdate());
    }

    IEnumerator MoveCardWithMouseUpdate()
    {
        yield return null;

        Vector2 initialMousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 grabPosOffset = (Vector2)currentSelectedCard.transform.position - initialMousePos;
        isMovingCard = true;
        float differenceBetweenLastChildAndParent = currentSelectedCard.transform.position.y - currentSelectedCard.GetLastStackedCardPosition().y;
        float adjustedMinBoundY = CardMap.minMapPoint.y + differenceBetweenLastChildAndParent + CardController.boxColliderHalfBoundsSize.y;

        while(currentSelectedCard.isDragging)
        {
            Vector2 resultPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            resultPos = resultPos + grabPosOffset;
            if(resultPos.x + CardController.boxColliderHalfBoundsSize.x > CardMap.maxMapPoint.x)
            {
                resultPos.x = CardMap.maxMapPoint.x - CardController.boxColliderHalfBoundsSize.x;
            }
            else if(resultPos.x - CardController.boxColliderHalfBoundsSize.x < CardMap.minMapPoint.x)
            {
                resultPos.x = CardMap.minMapPoint.x + CardController.boxColliderHalfBoundsSize.x;
            }
            if(resultPos.y + CardController.boxColliderHalfBoundsSize.y > CardMap.maxMapPoint.y)
            {
                resultPos.y = CardMap.maxMapPoint.y - CardController.boxColliderHalfBoundsSize.y;
            }
            if(resultPos.y - CardController.boxColliderHalfBoundsSize.y < adjustedMinBoundY)
            {
                resultPos.y = adjustedMinBoundY;
            }
            currentSelectedCard.transform.position = resultPos;
            
            yield return null;
        }
        isMovingCard = false;
    }

    void SaveAllCardDatas()
    {
        List<CardLocalData> requiredSaveCards = new();
        foreach(CardController item in cardPool)
        {
            if(item.IsAvailable() || item.IsRootCard())
            {
                continue;
            }
            requiredSaveCards.Add(item.GetLocalDataToSave());
        }
        SaveLoadManager.SaveData(CardSaveFileName, requiredSaveCards);
    }

    void LoadAllCardDatas()
    {
        List<CardLocalData> savedCards = SaveLoadManager.LoadData<List<CardLocalData>>(CardSaveFileName);

        for(int i=0; i<savedCards.Count; ++i)
        {
            CardLocalData loadedCardData = savedCards[i];
            CardController newCard = SpawnCard(loadedCardData.cardData);
            newCard.SetLoadedLocalData(loadedCardData);
        }
    }

    public CardController SpawnCard(SO_Card _cardData)
    {
        CardController card = GetCard();
        card.SetCardData(_cardData);

        // temp
        if(_cardData.cardType == ECardType.Character)
        {
            CharacterManager.Instance.RegisterCharacter(card, true);
        }

        return card;
    }

    public void SpawnCards(SO_Card.CombinationCardInfo _cardsInfo)
    {
        int spawnAmount = (int)(_cardsInfo.percentageOfResultsAmount * _cardsInfo.results.Count);
        List<SO_Card> possibleSpawnResult = new List<SO_Card>(_cardsInfo.results);
        GameUtils.ShuffleList(possibleSpawnResult);

        for(int i=0; i<spawnAmount; ++i)
        {
            SpawnCard(possibleSpawnResult[i]);
        }
    }

    CardController GetCard()
    {
        CardController found = cardPool.Find(x => x.IsAvailable());

        if(found != null)
        {
            found.Activate();
            return found;
        }

        CardController newCard = Instantiate(cardTemplate, transform);
        cardPool.Add(newCard);
        newCard.Activate();
        return newCard;
    }
}
