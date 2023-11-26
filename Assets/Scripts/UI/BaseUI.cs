using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BaseUI : MonoBehaviour
{
    public UIDocument uiDocument;
    [SerializeField] GameEventData gameEventData;

    Button startButton;

    const string BaseButtonOnPointerEnterUSS = "baseButtonPointerEnter";

    void Awake()
    {
        VisualElement root = uiDocument.rootVisualElement;

        startButton = root.Q<Button>("StartButton");

        startButton.clicked += OnStartButtonClicked;

        startButton.RegisterCallback<PointerEnterEvent>(e =>
        {
            OnPointerEnterButton();
        });
        startButton.RegisterCallback<PointerLeaveEvent>(e =>
        {
            OnPointerExitButton();
        });
    }

    void Start()
    {
        GameEvent.onEnterDungeon += OnEnterDungeon;
    }

    void OnPointerEnterButton()
    {
        startButton.AddToClassList(BaseButtonOnPointerEnterUSS);
    }

    void OnPointerExitButton()
    {
        startButton.RemoveFromClassList(BaseButtonOnPointerEnterUSS);
    }

    void OnStartButtonClicked()
    {
        GameEvent.Instance.ToDungeonEvent(gameEventData);
    }

    void OnEnterDungeon()
    {
        startButton.style.display = DisplayStyle.None;
    }
}
