using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(SO_Card))]
public class SO_CardEditor : Editor
{
    private VisualElement root;
    private SerializedObject _serializedObject;
    private SO_Card card;

    private EnumField enumField;
    private VisualElement normalCardContent;
    private VisualElement characterCardContent;

    private void OnEnable()
    {
        // Retrieve serialized object
        _serializedObject = new SerializedObject(target);
        card = (SO_Card)target;
    }

    public override VisualElement CreateInspectorGUI()
    {
        root = new VisualElement();
        root.Add(CreateEnumField());
        root.Add(CreateNormalCardContent());
        root.Add(CreateCharacterCardContent());
        
        Button checkCombinationsButton = new Button(card.CheckCombinations);
        checkCombinationsButton.style.height = 30f;
        checkCombinationsButton.text = "Log Combinations";
        root.Add(checkCombinationsButton);

        return root;
    }

    private EnumField CreateEnumField()
    {
        enumField = new EnumField("Card Type", card.cardType);
        enumField.RegisterValueChangedCallback(e =>
        {
            // Handle enum value change
            card.cardType = (ECardType)e.newValue;
            RefreshUI();
        });

        return enumField;
    }

    private VisualElement CreateNormalCardContent()
    {
        normalCardContent = new VisualElement();
        normalCardContent.Add(new PropertyField(_serializedObject.FindProperty("normalCardData"), "Normal Card Properties"));
/*        
        SerializedProperty normalCardProperty = serializedObject.FindProperty("normalCardData");

        normalCardContent.Add(new PropertyField(normalCardProperty.FindPropertyRelative("totalAliveDay")));
        normalCardContent.Add(new PropertyField(normalCardProperty.FindPropertyRelative("requiredSecondsToBeSpawned")));
        normalCardContent.Add(new PropertyField(normalCardProperty.FindPropertyRelative("requiredSecondsToBeUsed")));
        normalCardContent.Add(new PropertyField(normalCardProperty.FindPropertyRelative("totalSpawnableCardAmount")));
*/
        normalCardContent.style.display = card.cardType == ECardType.Normal ? DisplayStyle.Flex : DisplayStyle.None;

        return normalCardContent;
    }

    private VisualElement CreateCharacterCardContent()
    {
        characterCardContent = new VisualElement();
        characterCardContent.Add(new PropertyField(_serializedObject.FindProperty("characterCardData"), "Character Card Properties"));
        characterCardContent.style.display = card.cardType == ECardType.Character ? DisplayStyle.Flex : DisplayStyle.None;

        return characterCardContent;
    }

    private void RefreshUI()
    {
        normalCardContent.style.display = card.cardType == ECardType.Normal ? DisplayStyle.Flex : DisplayStyle.None;
        characterCardContent.style.display = card.cardType == ECardType.Character ? DisplayStyle.Flex : DisplayStyle.None;
    }
}