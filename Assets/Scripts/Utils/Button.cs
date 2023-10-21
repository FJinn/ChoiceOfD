using UnityEngine;

#if UNITY_EDITOR
using System;
using UnityEditor;
#endif

/* How to use
[Button("MyFunction")] public bool myButton;
ou
[Button(nameof(MyFunction))] public bool myButton;

public void MyFunction()
{
    Debug.Log("test");
}
*/

public class ButtonAttribute : PropertyAttribute
{
    public string MethodName { get; }
    public string ConditionMethodName { get; }

    public ButtonAttribute(string methodName, string conditionMethodeName = null)
    {
        MethodName = methodName;
        ConditionMethodName = conditionMethodeName;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ButtonAttribute))]
public class ButtonDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string methodName = (attribute as ButtonAttribute).MethodName;
        object target = property.serializedObject.targetObject;
        System.Type type = target.GetType();
        var splits = property.propertyPath.Split('.');

        for (int i = 0; i < splits.Length - 1; i++)
        {
            var field = type.GetField(splits[i]);

            if (field is null) // PROBLEM !
            {
                if (splits[i].Equals("Array"))  // Array found !
                {
                    var idx = splits[i + 1].Substring(splits[i + 1].IndexOf('[') + 1).TrimEnd(']');
                    int iidx = int.Parse(idx);
                    target = ((System.Collections.IList) target)[iidx];
                    i++;
                }
                else
                {
                    Debug.LogError($"HOUSTON WE HAVE A PROBLEM");
                }
            }
            else
            {
                target = field.GetValue(target);
            }

            type = target.GetType();
        }

        System.Reflection.MethodInfo method = type.GetMethod(methodName);
        if (method == null)
        {
            GUI.Label(position, "Method could not be found. Is it public?");
            return;
        }

        if (method.GetParameters().Length > 0)
        {
            GUI.Label(position, "Method cannot have parameters.");
            return;
        }

        string conditionMethodName = (attribute as ButtonAttribute).ConditionMethodName;
        if (conditionMethodName != null)
        {
            System.Reflection.MethodInfo conditionMethod = type.GetMethod(conditionMethodName);
            if (conditionMethod == null)
            {
                GUI.Label(position, "ConditionMethod could not be found. Is it public?");
                return;
            }

            if (conditionMethod.GetParameters().Length > 0)
            {
                GUI.Label(position, "ConditionMethod cannot have parameters.");
                return;
            }

            if (!(bool) conditionMethod.Invoke(target, null))
            {
                return;
            }
        }

        if (GUI.Button(position, label))
        {
            method.Invoke(target, null);
        }
    }
}
#endif