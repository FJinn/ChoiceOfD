using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
[System.Diagnostics.Conditional("UNITY_EDITOR")]
public class IntervalAttribute : PropertyAttribute
{
    public float Min { get; private set; }
    public float Max { get; private set; }
    public float Round { get; set; } = 0.0f;

    public IntervalAttribute() : this(0, 0)
    {
    }

    public IntervalAttribute(float min, float max)
    {
        Min = min;
        Max = max;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(IntervalAttribute))]
public class IntervalDrawer : PropertyDrawer
{
    private static readonly GUIContent[] MinMaxLabels = new GUIContent[] { new GUIContent("Min"), new GUIContent("Max") };
    private static readonly float[]      FloatValues  = new float[2];
    private static readonly int[]        IntValues    = new int[2];

    private float ComputeFieldWidth(float min, float max, float round)
    {
        float rangeMax    = Mathf.Max(Mathf.Abs(min), Mathf.Abs(max));
        int   numDigits   = Mathf.FloorToInt(Mathf.Log10(rangeMax) + 1);
        int   numDecimals = Mathf.FloorToInt(Mathf.Log10(round));
        if(numDecimals < 0)
            numDigits -= numDecimals;
        if(round == 0.0f)
            numDigits = Math.Max(numDigits, 7);

        string style    = new string('0', numDigits);
        return EditorGUIUtility.pixelsPerPoint * EditorStyles.numberField.CalcSize(new GUIContent(style)).x;
    }

    private float Round(float value, float prec)
    {
        return (prec == 0.0f) ? value : Mathf.Round(value / prec) * prec;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var interval = attribute as IntervalAttribute;

        float min, max;
        float round = interval.Round;
        switch(property.propertyType)
        {
        case SerializedPropertyType.Vector2:
            min = property.vector2Value.x;
            max = property.vector2Value.y;
            break;

        case SerializedPropertyType.Vector2Int:
            min = property.vector2IntValue.x;
            max = property.vector2IntValue.y;
            round = 1.0f;
            break;

        default:
            EditorGUI.HelpBox(position, "The attribute interval only supports Vector2 and Vector2Int.", MessageType.Error);
            return;
        }

        Rect basePos = position;
        int  id      = GUIUtility.GetControlID(label, FocusType.Keyboard, position);
        position     = EditorGUI.PrefixLabel(position, id, label);
        if(!EditorGUIUtility.wideMode)
        {
            float height     = base.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
            position.x       = basePos.x;
            position.y      += height;
            position.width   = basePos.width;
            position.height -= height;
        }

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        if (interval.Min == interval.Max)
        {
            if(round == 1.0f)
            {
                IntValues[0] = Mathf.RoundToInt(min);
                IntValues[1] = Mathf.RoundToInt(max);
                EditorGUI.MultiIntField(position, MinMaxLabels, IntValues);
                min = IntValues[0];
                max = IntValues[1];
            }
            else
            {
                FloatValues[0] = min;
                FloatValues[1] = max;
                EditorGUI.MultiFloatField(position, MinMaxLabels, FloatValues);
                min = Round(FloatValues[0], round);
                max = Round(FloatValues[1], round);
            }

            max = Mathf.Max(min, max);
        }
        else
        {
            float fieldWidth  = ComputeFieldWidth(interval.Min, interval.Max, round);
            float spacing     = EditorGUIUtility.pixelsPerPoint * 2.0f;
            Rect sliderRect   = position;
            sliderRect.x     +=  fieldWidth + spacing;
            sliderRect.width -= (fieldWidth + spacing) * 2.0f;
            EditorGUI.MinMaxSlider(sliderRect, GUIContent.none, ref min, ref max, interval.Min, interval.Max);

            Rect minRect  = position;
            minRect.width = fieldWidth;
            Rect maxRect  = minRect;
            maxRect.x    += position.width - fieldWidth;

            if(round == 1.0f)
            {
                min = EditorGUI.DelayedIntField(minRect, Mathf.RoundToInt(min));
                max = EditorGUI.DelayedIntField(maxRect, Mathf.RoundToInt(max));
            }
            else
            {
                min = Round(EditorGUI.DelayedFloatField(minRect, min), round);
                max = Round(EditorGUI.DelayedFloatField(maxRect, max), round);
            }

            min = Mathf.Clamp(min, interval.Min, interval.Max);
            max = Mathf.Clamp(max, interval.Min, interval.Max);
            max = Mathf.Max(min, max);
        }

        switch (property.propertyType)
        {
        case SerializedPropertyType.Vector2:
            property.vector2Value = new Vector2(min, max);
            break;

        case SerializedPropertyType.Vector2Int:
            property.vector2IntValue = new Vector2Int(Mathf.RoundToInt(min), Mathf.RoundToInt(max));
            break;
        }

        EditorGUI.indentLevel = indent;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float baseHeight = base.GetPropertyHeight(property, label);
        if(EditorGUIUtility.wideMode)
            return baseHeight;
        else
            return 2 * baseHeight + EditorGUIUtility.standardVerticalSpacing;
    }
}
#endif