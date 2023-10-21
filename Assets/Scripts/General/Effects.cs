#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using System.Collections.Generic;

public class Effects : Singleton<Effects>
{
    [ReadOnly] public List<string> effectsNames = new List<string>()
                                                {
                                                    "DamageCharacter",
                                                    "HealCharacter"
                                                };
}
/// <summary/> Use this on an int to select a Effects type
public class EffectsSelectorAttribute : PropertyAttribute
{
    
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EffectsSelectorAttribute))]
public class EffectsPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (Selection.count>1)
        {
            EditorGUI.Popup(position,"Effect Type", 0, new []{" --- "});
            return;
        }

        Effects effects = Effects.Instance;

        if (Effects.Instance == null)
            effects = Object.FindObjectOfType<Effects>();

        List<string> effectsNames = effects.effectsNames;
        if (effectsNames == null)
        {
            EditorGUI.Popup(position,"no effect", 0, new []{"[none]"});
            return;
        }

        int length = effectsNames.Count;
        
        if (length == 0)
        {
            EditorGUI.Popup(position,"no effect", 0, new []{"[none]"});
            return;
        }
        
        string[] options = new string[length+1];
        options[length] = "[none]";
        
        int selection = length;
        
        for (int i = 0; i < length; i++)
        {
            options[i] = effectsNames[i];
            if (effectsNames[i] == property.stringValue)
            {
                selection = i;
            }
        }

        selection = EditorGUI.Popup(position,"Effect Type", selection, options);
        property.stringValue = options[selection];
    }
}
#endif