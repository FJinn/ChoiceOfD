using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEditor.Graphs;
using System.ComponentModel;

[CustomPropertyDrawer(typeof(EnemyFormation))]
public class EnemyFormationEditor : PropertyDrawer
{
    /*
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        root.style.flexDirection = FlexDirection.Row;

        var frontVE = new VisualElement();
        frontVE.style.flexGrow = 1f;
        frontVE.style.flexDirection = FlexDirection.Column;
        root.Add(frontVE);

        EnumField[] fronts = new EnumField[3];
        for(int i=0; i<3; ++i)
        {
            fronts[i] = new EnumField(EEnemyType.None)
            {
                bindingPath = $"frontSlots[{i}]"
            };
            frontVE.Add(fronts[i]);
        }

        var middleVE = new VisualElement();
        middleVE.style.flexGrow = 1f;
        middleVE.style.flexDirection = FlexDirection.Column;
        root.Add(middleVE);

        EnumField[] middles = new EnumField[3];
        for(int i=0; i<3; ++i)
        {
            middles[i] = new EnumField(EEnemyType.None)
            {
                bindingPath = $"middleSlots[{i}]"
            };
            middleVE.Add(middles[i]);
        }

        var backVE = new VisualElement();
        backVE.style.flexGrow = 1f;
        backVE.style.flexDirection = FlexDirection.Column;
        root.Add(backVE);

        EnumField[] backs = new EnumField[3];
        for(int i=0; i<3; ++i)
        {
            backs[i] = new EnumField(EEnemyType.None)
            {
                bindingPath = $"backSlots[{i}]"
            };
            backVE.Add(backs[i]);
        }

        return root;
    }*/

    VisualElement root;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        root = new VisualElement();
        root.style.flexDirection = FlexDirection.Row;

        var front = new VisualElement();
        front.style.flexGrow = 1f;
        front.style.flexDirection = FlexDirection.Column;
        front.Add(new PropertyField(property.FindPropertyRelative("frontSlotA"), ""));
        front.Add(new PropertyField(property.FindPropertyRelative("frontSlotB"), ""));
        front.Add(new PropertyField(property.FindPropertyRelative("frontSlotC"), ""));
        
        var middle = new VisualElement();
        middle.style.flexGrow = 1f;
        middle.style.flexDirection = FlexDirection.Column;
        middle.Add(new PropertyField(property.FindPropertyRelative("middleSlotA"), ""));
        middle.Add(new PropertyField(property.FindPropertyRelative("middleSlotB"), ""));
        middle.Add(new PropertyField(property.FindPropertyRelative("middleSlotC"), ""));
        
        var back = new VisualElement();
        back.style.flexGrow = 1f;
        back.style.flexDirection = FlexDirection.Column;
        back.Add(new PropertyField(property.FindPropertyRelative("backSlotA"), ""));
        back.Add(new PropertyField(property.FindPropertyRelative("backSlotB"), ""));
        back.Add(new PropertyField(property.FindPropertyRelative("backSlotC"), ""));

        root.Add(front);
        root.Add(middle);
        root.Add(back);

        return root;
    }
}
