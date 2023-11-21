using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

using Button = UnityEngine.UIElements.Button;
using System.Linq;
using System;

public class CardUIItem : MonoBehaviour
{
    [SerializeField] CardUI cardUI;

    CardData[] cardDatas = new CardData[12];

    CardData selectedCardData;

    public ActionData GetCurrentSelectedActionData() => selectedCardData.actionData;

    [Serializable]
    public class CardData
    {
        public VisualElement parentPanel;
        public VisualElement[] cooldownBlocks = new VisualElement[5];
        public ECharacterClass characterClass;
        public Button card;
        public Label healthPoint;
        public Label damageAmount;
        public ActionData actionData;
        public Vector3 initialParentPosition;
        public Coroutine shakeRoutine = null;

        public void RegisterUpdateHealth() => actionData.onHealthUpdate += UpdateHealth;
        public void UnregisterUpdateHealth() => actionData.onHealthUpdate -= UpdateHealth;

        public void RegisterUpdateCooldownBlock() => actionData.onCooldownTurnChanged += UpdateCooldown;
        public void UnregisterUpdateCooldownBlock() => actionData.onCooldownTurnChanged -= UpdateCooldown;

        public void UpdateHealth()
        {
            healthPoint.text = actionData.currentHealth.ToString();
            VisualElementShakeParams shakeParams = new VisualElementShakeParams()
            {
                routineCache = shakeRoutine,
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

    public void InitializeCharacterCardUI(ECharacterClass characterClass)
    {
        int foundIndex = cardDatas.FindIndex(x => x.characterClass == ECharacterClass.None);
        for(int i=0; i<3; ++i)
        {
            int targetIndex = foundIndex+i;
            cardDatas[targetIndex].characterClass = characterClass;
            cardDatas[targetIndex].parentPanel.style.display = DisplayStyle.Flex;
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
        found.card.UnregisterCallback<MouseEnterEvent>(e => {OnPointerEnter(found);});
        found.card.UnregisterCallback<MouseLeaveEvent>(e => {OnPointerExit(found);});
        found.characterClass = ECharacterClass.None;
        found.actionData = null;
        found.card.style.display = DisplayStyle.None;
    }

    void Select(CardData cardData)
    {
        if(!cardUI.IsSelectableClass(cardData.actionData.belongToCharacterClass) || !cardData.actionData.canBeSelected || cardData == selectedCardData)
        {
            return;
        }
        if(selectedCardData != null)
        {
            Deselect();
        }
        selectedCardData = cardData;
        cardData.card.style.backgroundColor = cardUI.isSelectToTakeDamage ? Color.red : Color.blue;
    }

    public void Deselect()
    {
        selectedCardData.card.style.backgroundColor = Color.grey;
        selectedCardData = null;
    }

    void UpdateCardData(CardData target, ActionData targetActionData)
    {
        target.actionData = targetActionData;
        target.RegisterUpdateHealth();
        target.RegisterUpdateCooldownBlock();
        target.card.text = targetActionData.action.actionName;
        target.card.clicked += ()=>
        {
            Select(target);
        };
        target.card.RegisterCallback<MouseEnterEvent>(e => {OnPointerEnter(target);});
        target.card.RegisterCallback<MouseLeaveEvent>(e => {OnPointerExit(target);});

        target.card.style.display = DisplayStyle.Flex;

        target.healthPoint.text = target.actionData.currentHealth.ToString();
    }

    public void UpdateSelectableClassPanel(ECharacterClass characterClass)
    {
        for(int i=0; i<12; i+=3)
        {
            VisualElementTransitions transitions = VisualElementTransitions.Instance;
            Coroutine _routineCache = null;
            if(cardDatas[i].actionData.belongToCharacterClass != characterClass)
            {
                VisualElementVectorParams parameters = new VisualElementVectorParams
                {
                    visualElement = cardDatas[i].parentPanel,
                    routineCache = _routineCache,
                    onEndCallback = null,
                    targetVector = cardDatas[i].parentPanel.transform.position + new Vector3(0, cardDatas[i].parentPanel.resolvedStyle.height, 0),
                    durationToReach = 1f,
                    vectorType = VisualElementVectorParams.EVectorType.Position
                };
                transitions.LerpVector(parameters);
            }
            else if(cardDatas[i].parentPanel.transform.position.y != cardDatas[i].initialParentPosition.y)
            {
                VisualElementVectorParams parameters = new VisualElementVectorParams
                {
                    visualElement = cardDatas[i].parentPanel,
                    routineCache = _routineCache,
                    onEndCallback = null,
                    targetVector = cardDatas[i].initialParentPosition,
                    durationToReach = 1f,
                    vectorType = VisualElementVectorParams.EVectorType.Position
                };
                transitions.LerpVector(parameters);
            }
        }
    }

    void OnPointerEnter(CardData selectingCard)
    {
        if(cardUI.isSelectToTakeDamage)
        {
            int reduceAmount = cardUI.isDamagePercentage ? (int)(cardUI.takingDamagePercentageAmount * selectingCard.actionData.currentHealth) : cardUI.takingDamageAmount;
            selectingCard.damageAmount.text = "-" + reduceAmount;
            selectingCard.damageAmount.style.display = DisplayStyle.Flex;
        }
    }

    void OnPointerExit(CardData selectingCard)
    {
        if(cardUI.isSelectToTakeDamage)
        {
            selectingCard.damageAmount.style.display = DisplayStyle.None;
        }
    }
}
