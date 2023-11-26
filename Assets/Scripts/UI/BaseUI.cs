using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BaseUI : MonoBehaviour
{
    public UIDocument uiDocument;
    [SerializeField] GameEventData gameEventData;

    Button startButton;

    void Awake()
    {
        VisualElement root = uiDocument.rootVisualElement;

        startButton = root.Q<Button>("StartButton");

        startButton.clicked += OnStartButtonClicked;
    }

    void Start()
    {
        GameEvent.onEnterDungeon += OnEnterDungeon;
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
