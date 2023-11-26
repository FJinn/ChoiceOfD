using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using Button = UnityEngine.UIElements.Button;

public class CardUIItem : MonoBehaviour
{
    [SerializeField] CardUI cardUI;

    CardData[] cardDatas = new CardData[12];

    bool inSelectedTransition;

    [Serializable]
    public class CardData
    {
        public string id;
        public VisualElement parentPanel;
        public VisualElement[] cooldownBlocks = new VisualElement[5];
        public ECharacterClass characterClass;
        public Button card;
        public Label healthPoint;
        public Label damageAmount;
        public ActionData actionData;
        public Vector3 initialParentPosition;
        public Vector3 initialCardPosition;

        public Vector2 adjustmentToScreenValue;

        public bool hasCardFocused;

        public void RegisterUpdateHealth() => actionData.onHealthUpdate += UpdateHealth;
        public void UnregisterUpdateHealth() => actionData.onHealthUpdate -= UpdateHealth;

        public void RegisterUpdateCooldownBlock() => actionData.onCooldownTurnChanged += UpdateCooldown;
        public void UnregisterUpdateCooldownBlock() => actionData.onCooldownTurnChanged -= UpdateCooldown;

        public void UpdateHealth()
        {
            healthPoint.text = actionData.currentHealth.ToString();
            VisualElementShakeParams shakeParams = new VisualElementShakeParams()
            {
                id = id + healthPoint.name,
                visualElement = healthPoint,
                onEndCallback = null,
                shakeDuration = 0.3f,
                shakeDistance = 50f
            };
            VisualElementTransitions.Instance.ShakePosition(shakeParams);
        }

        public void UpdateCooldown(int remainingAmount)
        {
            for(int i=cooldownBlocks.Length-1; i>=0; --i)
            {
                cooldownBlocks[i].style.display = remainingAmount-1 < i ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }
        
        public void Initialize()
        {
            adjustmentToScreenValue.x = 0;
            adjustmentToScreenValue.y = 0;
            parentPanel.RegisterCallback<GeometryChangedEvent>(SetUpParent);
            card.RegisterCallback<GeometryChangedEvent>(SetUpCard);
        }

        void SetUpParent(GeometryChangedEvent geometryChangedEvent)
        {
            card.UnregisterCallback<GeometryChangedEvent>(SetUpParent);
            adjustmentToScreenValue.y += Screen.height - parentPanel.resolvedStyle.height;
        }

        void SetUpCard(GeometryChangedEvent geometryChangedEvent)
        {
            card.UnregisterCallback<GeometryChangedEvent>(SetUpCard);
            
            Vector2 worldBoundDifference =  parentPanel.worldBound.position - card.worldBound.position;
            Vector2 targetPos = worldBoundDifference;
            card.style.position = Position.Absolute;
            targetPos.x += card.resolvedStyle.width + (card.resolvedStyle.marginLeft + card.resolvedStyle.marginRight) * 2;
            targetPos.y = 0; //Screen.height - targetPos.y - card.resolvedStyle.marginBottom;
            card.transform.position = targetPos;

            adjustmentToScreenValue.x += card.worldBound.position.x + card.resolvedStyle.width * 0.5f;
            adjustmentToScreenValue.y -= worldBoundDifference.y - card.resolvedStyle.height * 0.5f;
            initialCardPosition = card.transform.position;
        }

        public Vector2 GetCardPositionFromScreenPosition(Vector2 screenPos)
        {
            Vector2 targetPos = RuntimePanelUtils.ScreenToPanel(card.panel, screenPos);
            targetPos.x -= adjustmentToScreenValue.x;
            targetPos.y = Screen.height - targetPos.y - adjustmentToScreenValue.y;
            return targetPos;
        }
    }

    void Awake()
    {
        VisualElement root = cardUI.uiDocument.rootVisualElement;

        for(int i=0; i<12; i+=3)
        {
            VisualElement characterPanel = root.Q("CharacterCardPanel"+((i/3)+1));

            for(int j=0; j<3; ++j)
            {
                CardData target = new CardData
                {
                    parentPanel = characterPanel, //characterPanel.Q("Character"),
                    initialParentPosition = characterPanel.transform.position,
                    card = characterPanel.Q<Button>("Card" + (j+1)),
                };
                target.healthPoint = target.card.Q<Label>("HealthPoint");
                target.damageAmount = target.card.Q<Label>("DamageAmount");
                target.damageAmount.style.display = DisplayStyle.None;
                target.parentPanel.style.display = DisplayStyle.None;
                cardDatas[j+i] = target;
                cardDatas[j+i].id = "CardData" + (j+i);

                cardDatas[j+i].Initialize();
            }
        }

        for(int i=0; i<12; ++i)
        {
            for(int k=0; k<5; ++k)
            {
                cardDatas[i].cooldownBlocks[k] = cardDatas[i].card.Q("CooldownBlock"+k);
                cardDatas[i].cooldownBlocks[k].style.display = DisplayStyle.None;
            }
        }
    }

    public void InitializeCharacterCardUI(ECharacterClass characterClass, bool showPanel = true)
    {
        int foundIndex = cardDatas.FindIndex(x => x.characterClass == ECharacterClass.None);
        for(int i=0; i<3; ++i)
        {
            int targetIndex = foundIndex+i;
            cardDatas[targetIndex].characterClass = characterClass;
            cardDatas[targetIndex].parentPanel.style.display = showPanel ? DisplayStyle.Flex : DisplayStyle.None;
            cardDatas[targetIndex].card.style.display = DisplayStyle.None;
        }
    }

    public void DeinitializeCharacterCardUI(ECharacterClass characterClass)
    {
        int foundIndex = cardDatas.FindIndex(x => x.characterClass == characterClass);

        for(int i=0; i<3; ++i)
        {
            CardData target = cardDatas[foundIndex+i];
            target.UnregisterUpdateHealth();
            target.UnregisterUpdateCooldownBlock();
            target.characterClass = ECharacterClass.None;
            target.actionData = null;
            target.card.style.display = DisplayStyle.None;
            target.parentPanel.style.display = DisplayStyle.None;
        }
    }

    public void SetAllCharacterPanelsDisplay(DisplayStyle display = DisplayStyle.Flex)
    {
        for(int i=0; i<12; i+=3)
        {
            if(cardDatas[i].characterClass == ECharacterClass.None)
            {
                continue;
            }
            cardDatas[i].parentPanel.style.display = display;
        }
    }

    public void AddCard(ActionData newActionData)
    {
        CardData found = cardDatas.Find(x => x.characterClass == newActionData.belongToCharacterClass && x.actionData == null);

        UpdateCardData(found, newActionData);
    }

    public void RemoveCard(ActionData _cacheActionData)
    {
        var found = cardDatas.Find(x => x.actionData == _cacheActionData);
        found.UnregisterUpdateHealth();
        found.UnregisterUpdateCooldownBlock();
        // found.card.UnregisterCallback<MouseDownEvent>(e => {OnPointerClick(found);});
        found.card.UnregisterCallback<MouseEnterEvent>(e => {OnPointerEnter(found);});
        found.card.UnregisterCallback<MouseLeaveEvent>(e => {OnPointerExit(found);});
        found.characterClass = ECharacterClass.None;
        found.actionData = null;
        found.card.style.display = DisplayStyle.None;
    }

    void UpdateCardData(CardData target, ActionData targetActionData)
    {
        target.actionData = targetActionData;
        target.RegisterUpdateHealth();
        target.RegisterUpdateCooldownBlock();
        target.card.text = targetActionData.action.actionName;
        target.card.clicked += ()=>
        {
            OnPointerClick(target);
        };
        // target.card.UnregisterCallback<MouseDownEvent>(e => {OnPointerClick(target);});
        target.card.RegisterCallback<MouseEnterEvent>(e => {OnPointerEnter(target);});
        target.card.RegisterCallback<MouseLeaveEvent>(e => {OnPointerExit(target);});

        target.card.style.display = DisplayStyle.Flex;

        target.healthPoint.text = target.actionData.currentHealth.ToString();
    }

    public void UpdateSelectableClassPanel(List<ECharacterClass> characterClass)
    {
        for(int i=0; i<12; i+=3)
        {
            VisualElementTransitions transitions = VisualElementTransitions.Instance;
            if(cardDatas[i].actionData != null && characterClass.Contains(cardDatas[i].actionData.belongToCharacterClass))
            {
                VisualElementVectorParams parameters = new VisualElementVectorParams
                {
                    id = cardDatas[i].id + cardDatas[i].parentPanel.name,
                    visualElement = cardDatas[i].parentPanel,
                    onEndCallback = null,
                    targetVector = cardDatas[i].parentPanel.transform.position - new Vector3(0, cardDatas[i].parentPanel.resolvedStyle.height * 0.1f, 0),
                    durationToReach = 0.2f,
                    vectorType = VisualElementVectorParams.EVectorType.Position
                };
                transitions.LerpVector(parameters);
            }
            else if(cardDatas[i].parentPanel.transform.position.y != cardDatas[i].initialParentPosition.y)
            {
                VisualElementVectorParams parameters = new VisualElementVectorParams
                {
                    id = cardDatas[i].id + cardDatas[i].parentPanel.name,
                    visualElement = cardDatas[i].parentPanel,
                    onEndCallback = null,
                    targetVector = cardDatas[i].initialParentPosition,
                    durationToReach = 0.2f,
                    vectorType = VisualElementVectorParams.EVectorType.Position
                };
                transitions.LerpVector(parameters);
            }
        }
    }

    void OnPointerClick(CardData selectingCard)
    {
        if(inSelectedTransition || !CombatManager.Instance.isInCombat || !cardUI.IsSelectableClass(selectingCard.actionData.belongToCharacterClass) || !selectingCard.actionData.canBeSelected)
        {
            return;
        }
        
        SelectCardTransitions(selectingCard, ()=>
        {
            cardUI.ConfirmSelection(selectingCard.actionData);
            selectingCard.damageAmount.style.display = DisplayStyle.None;
            selectingCard.hasCardFocused = false;
        });
        // selectingCard.card.style.backgroundColor = cardUI.isSelectToTakeDamage ? Color.red : Color.blue;
    }

    void OnPointerEnter(CardData selectingCard)
    {
        if(inSelectedTransition)
        {
            return;
        }
        
        FocusOnCard(selectingCard);
        
        if(cardUI.isSelectToTakeDamage)
        {
            int reduceAmount = cardUI.isDamagePercentage ? (int)(cardUI.takingDamagePercentageAmount * selectingCard.actionData.currentHealth) : cardUI.takingDamageAmount;
            selectingCard.damageAmount.text = "-" + reduceAmount;
            selectingCard.damageAmount.style.display = DisplayStyle.Flex;
        }
    }

    void OnPointerExit(CardData selectingCard)
    {
        if(inSelectedTransition)
        {
            return;
        }
        
        UnfocusOnCard(selectingCard);

        if(cardUI.isSelectToTakeDamage)
        {
            selectingCard.damageAmount.style.display = DisplayStyle.None;
        }
    }

    void FocusOnCard(CardData selectingCard)
    {
        selectingCard.card.BringToFront();

        VisualElementTransitions transitions = VisualElementTransitions.Instance;
        VisualElementVectorParams positionParameters = new VisualElementVectorParams
        {
            id = selectingCard.id + selectingCard.card.name,
            visualElement = selectingCard.card,
            onEndCallback = null,
            targetVector = selectingCard.initialCardPosition - new Vector3(0, selectingCard.card.resolvedStyle.height * 0.1f, 0),
            durationToReach = 0.2f,
            vectorType = VisualElementVectorParams.EVectorType.Position
        };
        VisualElementVectorParams scaleParameters = new VisualElementVectorParams
        {
            id = selectingCard.id + selectingCard.card.name,
            visualElement = selectingCard.card,
            onEndCallback = null,
            targetVector = Vector3.one * 1.5f,
            durationToReach = 0.2f,
            vectorType = VisualElementVectorParams.EVectorType.Scale
        };
        transitions.LerpVector(positionParameters);
        transitions.LerpVector(scaleParameters);

        selectingCard.hasCardFocused = true;
    }

    void UnfocusOnCard(CardData selectingCard, bool forceUnfocus = false)
    {
        if(!forceUnfocus && !selectingCard.hasCardFocused)
        {
            return;
        }

        VisualElementTransitions transitions = VisualElementTransitions.Instance;
        VisualElementVectorParams positionParameters = new VisualElementVectorParams
        {
            id = selectingCard.id + selectingCard.card.name,
            visualElement = selectingCard.card,
            onEndCallback = null,
            targetVector = selectingCard.initialCardPosition,
            durationToReach = 0.2f,
            vectorType = VisualElementVectorParams.EVectorType.Position
        };
        VisualElementVectorParams scaleParameters = new VisualElementVectorParams
        {
            id = selectingCard.id + selectingCard.card.name,
            visualElement = selectingCard.card,
            onEndCallback = null,
            targetVector = Vector3.one,
            durationToReach = 0.2f,
            vectorType = VisualElementVectorParams.EVectorType.Scale
        };
        transitions.LerpVector(positionParameters);
        transitions.LerpVector(scaleParameters);
        
        selectingCard.hasCardFocused = false;
    }

    void SelectCardTransitions(CardData selectingCard, Action onTransitionsEnd)
    {
        inSelectedTransition = true;
        Vector3 screenCenter = selectingCard.GetCardPositionFromScreenPosition(new Vector2(Screen.width, Screen.height) * 0.5f);
        VisualElementTransitions transitions = VisualElementTransitions.Instance;
        VisualElementVectorParams positionParameters = new VisualElementVectorParams
        {
            id = selectingCard.id + selectingCard.card.name,
            visualElement = selectingCard.card,
            targetVector = screenCenter, //new Vector3(-Screen.width * 0.5f, -Screen.height * 0.5f, selectingCard.card.transform.position.z),
            durationToReach = 0.4f,
            vectorType = VisualElementVectorParams.EVectorType.Position,
            onEndCallback = ()=>
            {
                VisualElementVectorParams scaleParameters = new VisualElementVectorParams
                {
                    id = selectingCard.id + selectingCard.card.name,
                    visualElement = selectingCard.card,
                    targetVector = Vector3.one * 3f,
                    durationToReach = 1f,
                    vectorType = VisualElementVectorParams.EVectorType.Scale,
                    onEndCallback = ()=>
                    {
                        inSelectedTransition = false;
                        UnfocusOnCard(selectingCard, true);
                        onTransitionsEnd?.Invoke();
                    }
                };
                transitions.LerpVector(scaleParameters);
            }
        };
        transitions.LerpVector(positionParameters);

        selectingCard.hasCardFocused = false;
    }
}
