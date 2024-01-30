using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

using Random = UnityEngine.Random;

public class CardController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] BoxCollider2D boxCollider;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Transform progressBarTransform;

    [SerializeField, ReadOnly] List<CardController> stackingCardList = new();
    [SerializeField, ReadOnly] CardController rootInList;

    public bool isDragging {private set; get;}
    float mouseMoveSpeed;

    bool dataLoaded;
    
    public static Vector2 boxColliderBoundsSize {private set; get;}
    public static Vector2 boxColliderHalfBoundsSize {private set; get;}
    public Vector2 GetLastStackedCardPosition() => stackingCardList[^1].transform.position;
    public Vector2 GetProgressBarWorldPosition() => progressBarTransform.position;
    public bool IsRootCard() => rootInList == this;
    public void SetCardData(SO_Card _cardData) => cardLocalData.cardData = _cardData;
    public CardLocalData GetCardData() => cardLocalData;

    public bool IsAvailable() => gameObject.activeInHierarchy;

    // Create a Material Property Block
    MaterialPropertyBlock materialPropertyBlock;

    CardController currentInteractingCard;
    SO_Card.CombinationCardInfo spawningCardsInfo;
    CardLocalData cardLocalData;

    Coroutine followParentRoutine;
    Coroutine progressRoutine;

    const int FirstSortingIndexWhenDrag = 100;
    const float InitialMoveSpeed = 40f;
    const float PercentageReductionFromParentMoveSpeed = 0.9f;

    CardLocalData UpdateLocalData()
    {
        cardLocalData.worldPosition = transform.position;

        return cardLocalData;
    }

    public CardLocalData GetLocalDataToSave()
    {
        CardLocalData cardLocalData = UpdateLocalData();

        for(int i=1; i<stackingCardList.Count; ++i)
        {
            cardLocalData.stackCards.Add(stackingCardList[i].UpdateLocalData());
        }

        return cardLocalData;
    }

    void LoadFromLocalData(CardLocalData savedCardLocalData)
    {
        transform.position = savedCardLocalData.worldPosition;
        cardLocalData = savedCardLocalData;
    }

    public void SetLoadedLocalData(CardLocalData savedCardLocalData)
    {
        dataLoaded = true;

        LoadFromLocalData(savedCardLocalData);

        rootInList = this;
        stackingCardList.Add(this);

        for(int i=0; i<savedCardLocalData.stackCards.Count; ++i)
        {
            CardController newCard = CardManager.Instance.SpawnCard(savedCardLocalData.stackCards[i].cardData);
            newCard.LoadFromLocalData(savedCardLocalData.stackCards[i]);
            newCard.SnapTo(this);
        }
    }

    void Start()
    {
        dataLoaded = false;
        
        boxColliderBoundsSize = boxCollider.bounds.size;
        boxColliderHalfBoundsSize = boxCollider.bounds.extents;

        // Initialize the Material Property Block
        materialPropertyBlock = new MaterialPropertyBlock();
        // Apply the Material Property Block to the SpriteRenderer
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);

    }

    void OnEnable()
    {
        TimeManager.onDayEnded += ReduceAliveDay;
        TimeManager.onTimeOfDayChanged += UpdateCardBasedOnDayOfTime;

        if(!dataLoaded)
        {
            rootInList = this;
            stackingCardList.Add(this);
            cardLocalData.InitializeData();
        }

        UpdateCardBasedOnDayOfTime();
        transform.position = GameUtils.CheckForCollisionsAndReposition(transform.position, boxColliderBoundsSize, CardManager.cardInteractLayer, gameObject);
    }

    void OnDisable()
    {
        TimeManager.onDayEnded -= ReduceAliveDay;
        TimeManager.onTimeOfDayChanged -= UpdateCardBasedOnDayOfTime;
        
        stackingCardList.Clear();

        dataLoaded = false;
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void Deactivate(bool onlySelf = false)
    {
        if(onlySelf)
        {
            gameObject.SetActive(false);
            return;
        }

        for(int i=1; i<stackingCardList.Count; ++i)
        {
            stackingCardList[i].Deactivate();
        }

        gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(!cardLocalData.isMovable)
        {
            return;
        }

        UnSnap();

        mouseMoveSpeed = InitialMoveSpeed;

        SelectImplementation();
        isDragging = true;
        CardManager.Instance.SetSelectedCard(this);

        for(int i=1; i<stackingCardList.Count; ++i)
        {
            stackingCardList[i].FollowParentMovement();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        
        Collider2D hitCollider = Physics2D.OverlapBox(transform.position, boxColliderBoundsSize, 0, CardManager.cardInteractLayer);
        currentInteractingCard = hitCollider ? hitCollider.GetComponent<CardController>() : null;
        int stackedCount = currentInteractingCard.cardLocalData.stackCards.Count;
        int stackableCount = currentInteractingCard.cardLocalData.cardData.totalStackableCardAmount;
        if(stackableCount >= 0 && stackedCount >= stackableCount)
        {
            currentInteractingCard = null;
        }
        
        if(currentInteractingCard != null)
        {
            SnapTo(currentInteractingCard);
            currentInteractingCard = null;
        }

        Unselect();

        List<CardController> rootList = rootInList.stackingCardList;
        for(int i=0; i<rootList.Count; ++i)
        {
            rootList[i].SortRenderOrder();
        }

        transform.position = GameUtils.CheckForCollisionsAndReposition(transform.position, boxColliderBoundsSize, CardManager.cardInteractLayer, gameObject);
    }

    void SelectImplementation()
    {
        rootInList = this;
        boxCollider.enabled = false;

        spriteRenderer.sortingOrder = FirstSortingIndexWhenDrag;
        for(int i=1; i<stackingCardList.Count; ++i)
        {
            stackingCardList[i].SortRenderOrder();
        }
    }
    
    void Unselect()
    {
        boxCollider.enabled = true;
    }

    CardController GetPreviousCard()
    {
        if(rootInList == this)
        {
            return null;
        }

        return rootInList.stackingCardList[rootInList.stackingCardList.IndexOf(this) - 1];
    }

    void SnapTo(CardController card)
    {
        CardController root = card.rootInList;
        root.stackingCardList.Add(this);
        rootInList = root;
        int secondLastIndex = root.stackingCardList.Count - 2;
        CardController prevCard = root.stackingCardList[secondLastIndex];
        transform.position = prevCard.transform.position + CardManager.Instance.stackCardOffset;

        EvaluateCombination();
    }

    void UnSnap()
    {
        if(rootInList == this)
        {
            return;
        }
        
        int indexInList = rootInList.stackingCardList.IndexOf(this);
        // add attached children into own list, then become parent
        for(int i=indexInList+1; i<rootInList.stackingCardList.Count; ++i)
        {
            CardController target = rootInList.stackingCardList[i];
            stackingCardList.Add(target);
            target.rootInList = rootInList.stackingCardList[indexInList];
        }
        // remove self and children from previous parent
        rootInList.stackingCardList.RemoveRange(indexInList, rootInList.stackingCardList.Count - indexInList);
        // becomes parent
        rootInList = this;
    }

    void SortRenderOrder()
    {
        if(rootInList == this)
        {
            spriteRenderer.sortingOrder = 0;
            return;
        }

        int indexInList = rootInList.stackingCardList.IndexOf(this);
        spriteRenderer.sortingOrder = rootInList.stackingCardList[indexInList-1].spriteRenderer.sortingOrder + 1;
    }

    void FollowParentMovement()
    {
        if(followParentRoutine != null)
        {
            StopCoroutine(followParentRoutine);
        }
        followParentRoutine = StartCoroutine(FollowParentUpdate());

        boxCollider.enabled = false;
    }

    IEnumerator FollowParentUpdate()
    {
        yield return null;
        int indexInList = rootInList.stackingCardList.IndexOf(this);
        CardController prevCard = rootInList.stackingCardList[indexInList-1];
        float moveSpeed = prevCard.mouseMoveSpeed * PercentageReductionFromParentMoveSpeed;
        mouseMoveSpeed = moveSpeed;

        while(prevCard != null && (CardManager.Instance.isMovingCard || MathUtilities.DistanceSquared(transform.position, prevCard.transform.position + CardManager.Instance.stackCardOffset) > 0))
        {
            transform.position = Vector2.MoveTowards(transform.position, prevCard.transform.position + CardManager.Instance.stackCardOffset, moveSpeed * TimeManager.deltaTime);
            yield return null;
        }

        boxCollider.enabled = true;
    }

    void EvaluateCombination()
    {
        bool found = false;

        foreach(var item in cardLocalData.cardData.possibleCombinations)
        {
            int currentKeyCount = 0;
            for(int i=rootInList.stackingCardList.Count-1; i>=0; --i)
            {
                if(!item.combinations.Contains(rootInList.stackingCardList[i].cardLocalData.cardData))
                {
                    return;
                }
                currentKeyCount += 1;
                if(currentKeyCount == item.combinations.Count)
                {
                    CardController instigator = rootInList.stackingCardList[i];
                    instigator.spawningCardsInfo = item;
                    instigator.RunProgress();
                    found = true;
                    break;
                }
            }

            if(found)
            {
                break;
            }
        }
    }

    void RunProgress()
    {
        if(rootInList != this)
        {
            Debug.LogWarning($"Could not run progress due to {name} is not root! Skipping!");
            return;
        }

        ProgressBarManager.Instance.SetProgressBar(this);

        if(progressRoutine != null)
        {
            StopCoroutine(progressRoutine);
        }
        progressRoutine = StartCoroutine(ProgressUpdate());
    }

    IEnumerator ProgressUpdate()
    {
        while(cardLocalData.currentProgressValue < 1)
        {
            cardLocalData.currentProgressValue += TimeManager.deltaTime / spawningCardsInfo.requiredSecondsToBeSpawned;
            yield return null;
        }
        
        CardManager.Instance.SpawnCards(spawningCardsInfo);
        spawningCardsInfo = null;
        cardLocalData.spawnedCardAmount += 1;

        if(cardLocalData.spawnedCardAmount >= cardLocalData.cardData.normalCardData.totalSpawnableCardAmount)
        {
            Deactivate();
        }

        cardLocalData.currentProgressValue = 0;
    }

    void ReduceAliveDay()
    {
        cardLocalData.aliveDaysLeft -= 1;

        if(cardLocalData.aliveDaysLeft == 0)
        {
            Deactivate(true);
        }
    }

    void UpdateCardBasedOnDayOfTime()
    {
        float visibilityValue = 1f;
        switch(TimeManager.currentTimeOfDay)
        {
            case TimeManager.ETimeOfDay.Daytime:
                visibilityValue = 1f;
                break;
            case TimeManager.ETimeOfDay.Night:
                visibilityValue = cardLocalData.isLightenUp ? 1f : 0f;
                break;
        }

        materialPropertyBlock.SetFloat("_Visibility", visibilityValue);
        // Apply the updated Material Property Block to the SpriteRenderer
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }

}


[Serializable]
public class CardLocalData : ISerializationCallbackReceiver
{
    public Vector2 worldPosition;
    public int spawnedCardAmount;
    public float currentProgressValue;
    public float aliveDaysLeft;
    public SO_Card cardData;
    public bool isMovable;
    public bool isLightenUp;

    public CharacterLocalData characterLocalData;

    public void InitializeData()
    {
        isMovable = true;
        isLightenUp = false;

        switch(cardData.cardType)
        {
            case ECardType.Normal:
                spawnedCardAmount = 0;
                currentProgressValue = 0;
                aliveDaysLeft = cardData.normalCardData.totalAliveDay;
                break;
            case ECardType.Character:
                characterLocalData.characterName = "No Name Yet: TODO";
                characterLocalData.currentHeath = cardData.characterCardData.initialHealth;
                characterLocalData.currentSatiety = cardData.characterCardData.initialSatiety;
                characterLocalData.currentOccupation = cardData.characterCardData.occupation;
                characterLocalData.currentCondition = cardData.characterCardData.condition;
                characterLocalData.currentWeapon = cardData.characterCardData.weapon;
                characterLocalData.currentArmour = cardData.characterCardData.armour;
                
                break;
        }
    }

    [NonSerialized]public List<CardLocalData> stackCards = new();
    
    // Custom serialization logic
    public void OnBeforeSerialize()
    {
        // Do not serialize listOfData directly
    }

    // Custom deserialization logic
    public void OnAfterDeserialize()
    {
        // Clone each item in the list during deserialization
        List<CardLocalData> clonedList = new List<CardLocalData>();
        foreach (CardLocalData item in stackCards)
        {
            CardLocalData clonedItem = new CardLocalData()
            {
                worldPosition = item.worldPosition,
                spawnedCardAmount = item.spawnedCardAmount,
                currentProgressValue = item.currentProgressValue,
                aliveDaysLeft = item.aliveDaysLeft,
                cardData = item.cardData,
                characterLocalData = item.characterLocalData
            };
            clonedItem.OnAfterDeserialize(); // Recursive call for nested lists, if necessary
            clonedList.Add(clonedItem);
        }

        // Set the cloned list as the new list
        stackCards = clonedList;

        // Manually set references to this instance for each item
        foreach (CardLocalData item in stackCards)
        {
            item.stackCards = new List<CardLocalData>(); // Each item should have its own list
        }
    }
}

[Serializable]
public class CharacterLocalData
{
    public Action<int> onHealthChanged;
    public Action<float> onSatietyChanged;
    public Action<EOccupation> onOccupationChanged;
    public Action<ECondition> onConditionChanged;
    public Action<SO_Card> onWeaponChanged;
    public Action<SO_Card> onArmourChanged;

    public string characterName;

    public int currentHeath
    {
        set
        {
            currentHeath = value;
            onHealthChanged?.Invoke(currentHeath);
        }
        get => currentHeath;
    }
    public float currentSatiety
    {
        set
        {
            currentSatiety = value;
            onSatietyChanged?.Invoke(currentSatiety);
        }
        get => currentSatiety;
    }
    public EOccupation currentOccupation
    {
        set
        {
            currentOccupation = value;
            onOccupationChanged?.Invoke(currentOccupation);
        }
        get => currentOccupation;
    }
    public ECondition currentCondition
    {
        set
        {
            currentCondition = value;
            onConditionChanged?.Invoke(currentCondition);
        }
        get => currentCondition;
    }
    public SO_Card currentWeapon
    {
        set
        {
            currentWeapon = value;
            onWeaponChanged?.Invoke(currentWeapon);
        }
        get => currentWeapon;
    }
    public SO_Card currentArmour
    {
        set
        {
            currentArmour = value;
            onArmourChanged?.Invoke(currentArmour);
        }
        get => currentArmour;
    }
}
