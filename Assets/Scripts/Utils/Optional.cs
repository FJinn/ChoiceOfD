using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Optional
{
	public bool Enabled = false;
	public bool Expanded = false;
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
[Conditional("UNITY_EDITOR")]
public class OptionalAttribute : PropertyAttribute
{
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(OptionalAttribute))]
[CustomPropertyDrawer(typeof(Optional), true)]
public class OptionalDrawer : PropertyDrawer
{
    private GUIStyle headerStyle;
    private GUIStyle toggleStyle;

    public OptionalDrawer()
    {
        headerStyle = new GUIStyle("ShurikenModuleTitle");
        toggleStyle = new GUIStyle("ShurikenToggle");
    }


    protected virtual void DrawContent(Rect position, SerializedProperty property, GUIContent label)
    {
        //float height = 0.0f;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        SerializedProperty enabled = property.FindPropertyRelative("Enabled");
        SerializedProperty expanded = property.FindPropertyRelative("Expanded");

        int indentOffset = EditorGUI.indentLevel - property.depth;
        SerializedProperty propertyEnd = property.GetEndProperty();
        bool first = true;
        while (property.NextVisible(first) && !SerializedProperty.EqualContents(property, propertyEnd))
        {
            first = false;
            if (SerializedProperty.EqualContents(property, enabled))
                continue;

            if (SerializedProperty.EqualContents(property, expanded))
                continue;

            position.height = EditorGUI.GetPropertyHeight(property, label);
            EditorGUI.indentLevel = property.depth + indentOffset;
            EditorGUI.PropertyField(position, property, property.isExpanded);
            position.y += position.height + spacing;
        }
        //return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty enabled = property.FindPropertyRelative("Enabled");
        if(enabled == null)
        {
            EditorGUI.HelpBox(position, $"The field {label.text} has an Optional attribute but no field named Enabled.", MessageType.Error);
            return;
        }
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        position.height = EditorGUI.GetPropertyHeight(property, label, false);
        Rect checkboxPosition = new Rect(position.x + 2.0f, position.y + 1.0f, 13.0f, 13.0f);
        if(GUI.Button(checkboxPosition, GUIContent.none, GUIStyle.none))
            enabled.boolValue = !enabled.boolValue;

        EditorGUI.BeginProperty(position, label, enabled);
        property.isExpanded = GUI.Toggle(position, property.isExpanded, label, headerStyle);
        EditorGUI.EndProperty();
        GUI.Toggle(checkboxPosition, enabled.boolValue, GUIContent.none, toggleStyle);
        position.y += position.height + spacing;


        SerializedProperty isExpanded = property.FindPropertyRelative("Expanded");
        isExpanded.boolValue = property.isExpanded;

        if (property.isExpanded)
		{
			int lastIntentLevel = EditorGUI.indentLevel;
			bool savedEnabled = GUI.enabled;
            GUI.enabled = enabled.boolValue;

            DrawContent(position, property, label);

            GUI.enabled = savedEnabled;
			EditorGUI.indentLevel = lastIntentLevel;

		}

	}

    protected virtual float GetContentHeight(SerializedProperty property, GUIContent label)
    {
        float height = 0.0f;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        SerializedProperty enabled = property.FindPropertyRelative("Enabled");
        SerializedProperty expanded = property.FindPropertyRelative("Expanded");

        SerializedProperty propertyEnd = property.GetEndProperty();
        bool first = true;
        bool firstDrawed = true;
        while (property.NextVisible(first) && !SerializedProperty.EqualContents(property, propertyEnd))
        {
            first = false;

            if (SerializedProperty.EqualContents(property, enabled))
                continue;

            if (SerializedProperty.EqualContents(property, expanded))
                continue;

            height += EditorGUI.GetPropertyHeight(property, label);

            if (!firstDrawed)
                height += spacing;

            firstDrawed = false;
        }

        return height;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUI.GetPropertyHeight(property, label, false);
        if (property.isExpanded)
        {
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            height += GetContentHeight(property, label) + spacing;
        }
        return height;
    }
}
#endif