using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CombatTurnUIItem : VisualElement
{
    public Label characterName;
    public VisualElement characterVisual;

    public string GetCharacterName() => characterName.text;
    public bool IsActive() => characterName.style.display == DisplayStyle.Flex;

    public CombatTurnUIItem(Texture2D bgTexture)
    {
        style.flexGrow = 1;
        style.backgroundImage = bgTexture;
        style.marginLeft = 2f;
        style.marginRight = 2f;
        style.marginTop = 2f;
        style.marginBottom = 2f;

        characterVisual = new VisualElement();
        characterVisual.style.flexGrow = 1f;
        characterVisual.style.justifyContent = Justify.FlexEnd;
        // set to scale-to-fit instead of stretch 
        characterVisual.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
        characterVisual.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
        characterVisual.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
        characterVisual.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
        characterVisual.style.marginLeft = 2f;
        characterVisual.style.marginRight = 2f;
        characterVisual.style.marginTop = 2f;
        characterVisual.style.marginBottom = 2f;
        Add(characterVisual);

        characterName = new Label();
        characterName.style.display = DisplayStyle.None;
        characterName.style.unityTextAlign = TextAnchor.LowerCenter;
        characterName.style.flexWrap = Wrap.Wrap;
        characterName.style.whiteSpace = WhiteSpace.Normal;

        characterVisual.Add(characterName);
        RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);

        AddToClassList("CombatCharacterContainer");
    }

    private void GeometryChangedCallback(GeometryChangedEvent evt)
    {
        UnregisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
        // Do what you need to do here, as geometry should be calculated.
        style.width = resolvedStyle.height * 2 / 3;
    }


    public void Activate(string _name, Texture2D characterIcon)
    {
        characterName.text = _name;
        characterName.style.display = DisplayStyle.Flex;
        characterName.style.backgroundImage = characterIcon;

        characterVisual.style.backgroundImage = characterIcon;
    }

    public void Deactivate(Action callback = null)
    {
        characterName.style.display = DisplayStyle.None;
        callback?.Invoke();
    }
}
