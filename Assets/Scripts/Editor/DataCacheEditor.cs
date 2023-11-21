using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(DataCache.BuffUIData))]
public class BuffUIDataEditor: PropertyDrawer
{
    VisualElement root;
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        root = new VisualElement();
        root.Add(new PropertyField(property.FindPropertyRelative("buffType")));
        root.Add(new PropertyField(property.FindPropertyRelative("sprite")));

        return root;
    }
}

[CustomPropertyDrawer(typeof(DataCache.DebuffUIData))]
public class DebuffUIDataEditor: PropertyDrawer
{
    VisualElement root;
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        root = new VisualElement();
        root.Add(new PropertyField(property.FindPropertyRelative("debuffType")));
        root.Add(new PropertyField(property.FindPropertyRelative("sprite")));

        return root;
    }
}
