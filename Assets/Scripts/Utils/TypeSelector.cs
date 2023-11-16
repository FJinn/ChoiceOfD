using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using Unity.VisualScripting;

#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
using UnityEditor;
#endif

public class TypeSelectorAttribute : PropertyAttribute
{
    public Type parentType { get; private set; }
    public bool proposeParent { get; private set; }
    public bool proposeAbstract { get; private set; }

    public TypeSelectorAttribute(Type parentType, bool proposeParent = true, bool proposeAbstract = false)
    {
        this.parentType = parentType;
        this.proposeParent = proposeParent;
        this.proposeAbstract = proposeAbstract;
    }
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TypeSelectorAttribute))]
public class TypeSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        TypeSelectorAttribute typeSelector = attribute as TypeSelectorAttribute;

        SerializedProperty selectedTypeNameProperty = property.FindPropertyRelative("m_SelectedTypeName");
        SerializedProperty typeNameProperty = property.FindPropertyRelative("typeName");

		string selectedTypeName = selectedTypeNameProperty.stringValue;
        Assembly assembly = Assembly.GetAssembly(typeSelector.parentType);
        IEnumerable<Type> subclassTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeSelector.parentType));

        List<Type> types = new List<Type>();
        List<string> typesName = new List<string>();
        List<string> typesFull = new List<string>();
        int selectedIndex = 0;
        int index = 0;

        if (typeSelector.proposeParent && (!typeSelector.parentType.IsAbstract || typeSelector.proposeAbstract))
        {
			types.Add(typeSelector.parentType);
            typesName.Add(typeSelector.parentType.Name);
            typesFull.Add(typeSelector.parentType.AssemblyQualifiedName);
            if (typeSelector.parentType.AssemblyQualifiedName == selectedTypeName)
                selectedIndex = index;
            index++;
        }
        foreach (Type type in subclassTypes)
        {
            if (type.IsAbstract && !typeSelector.proposeAbstract) continue;
			types.Add(type);
			typesName.Add(type.Name);
            typesFull.Add(type.AssemblyQualifiedName);
            if (type.AssemblyQualifiedName == selectedTypeName)
                selectedIndex = index;
            index++;
        }

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();
        int newSelectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, typesName.ToArray());
        if (string.IsNullOrEmpty(selectedTypeNameProperty.stringValue) || EditorGUI.EndChangeCheck())
        {
            selectedTypeNameProperty.stringValue = typesFull[newSelectedIndex];
			typeNameProperty.stringValue = UnityTypeSerializationBinder.TypeToTypeName(types[newSelectedIndex]);
			//selectedTypeNameProperty.serializedObject.Update();
		}

        EditorGUI.EndProperty();
    }
}
#endif

[Serializable]
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
public class TypeSelector : ISerializationCallbackReceiver, IEquatable<TypeSelector>
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
    public Guid id;

    public Type selectedType;

    [SerializeField] string m_SelectedTypeName = "";
    [SerializeField] string typeName;

    [SerializeField] bool m_IsInit = false;

    public TypeSelector()
	{
		id = Guid.NewGuid();
	}

    public TypeSelector(TypeSelector original)
    {
        id = original.id;

        selectedType = original.selectedType;
        m_SelectedTypeName = original.m_SelectedTypeName;
    }

    public void OnBeforeSerialize()
    {
        Serialize();
    }

    public void OnAfterDeserialize()
    {
        if (!m_IsInit)
        {
            Serialize();
            m_IsInit = true;
        }
        else
            Deserialize();
    }

    private void Serialize()
    {
        m_SelectedTypeName = selectedType != null ? selectedType.AssemblyQualifiedName : "";

		//typeName = UnityTypeSerializationBinder.TypeToTypeName(selectedType);
    }

    private void Deserialize()
    {
        selectedType = !string.IsNullOrEmpty(m_SelectedTypeName) ? Type.GetType(m_SelectedTypeName) : null;

        /*if (!string.IsNullOrEmpty(typeName))
            selectedType = UnityTypeSerializationBinder.TypeNameToType(typeName);*/
    }

    public bool Equals(TypeSelector other)
    {
        return id == other.id &&
        selectedType == other.selectedType &&
        m_SelectedTypeName == other.m_SelectedTypeName;
    }
    public static bool operator ==(TypeSelector lts, TypeSelector rts)
    {
        if (lts is null)
        {
            if (rts is null)
            {
                return true;
            }

            // Only the left side is null.
            return false;
        }
        // Equals handles case of null on right side.
        return lts.Equals(rts);
    }

    public static bool operator !=(TypeSelector lts, TypeSelector rts)
    {
        return !(lts == rts);
    }

    public static implicit operator Type(TypeSelector typeSelector)
    {
        return typeSelector.selectedType;
    }
}


public class UnityTypeSerializationBinder : ISerializationBinder
{
    private List<Type> arrayTypes = new() { typeof(List<>), typeof(CustomList<>) };
    private List<Type> ignoredTypes = new() { typeof(object), typeof(ValueType), typeof(UnityEngine.Object), typeof(UnityEngine.ScriptableObject) };

    public void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
        assemblyName = null;
        typeName = ConvertTypeToTypeScript(serializedType);
    }

    public Type BindToType(string assemblyName, string typeName)
    {
        return Type.GetType(typeName);
    }

    public static string TypeToTypeName(Type serializedType)
    {
        return serializedType?.Name;
    }

    public static Type TypeNameToType(string typeName)
    {
        return typeName!=null ? Type.GetType(typeName) : null;
    }

    public bool IsIgnoredType(Type t)
    {
        return ignoredTypes.Contains(t) || arrayTypes.Contains(t);
    }

    public string ConvertTypeToTypeScript(Type type, bool referenceResolver = false)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            // Convert nullable types to their underlying types
            type = Nullable.GetUnderlyingType(type);
        }

        if (type.IsGenericType)
        {
            // Handle generic types
            var genericTypeName = type.Name.Split('`')[0];
            var genericTypeArguments = string.Join(", ", type.GetGenericArguments().Select((t) => ConvertTypeToTypeScript(t, referenceResolver)));
            var typeName = $"{genericTypeName}<{genericTypeArguments}>";

            if (arrayTypes != null && arrayTypes.Contains(type.GetGenericTypeDefinition()))
                typeName = $"{genericTypeArguments}[]";

            if (referenceResolver && UnityObjectReferenceResolver.IsReferencedType(type))
                typeName = $"Ref<{typeName}>";

            return typeName;
        }

        if (type.IsArray)
        {
            var eltName = ConvertTypeToTypeScript(type.GetElementType(), referenceResolver);
            return $"{eltName}[]";
        }

        if (type.IsEnum)
        {
            var eltName = TypeToTypeName(type);
            return eltName;
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean:
                return "boolean";
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
                return "number";
            case TypeCode.Char:
            case TypeCode.String:
                return "string";
            default:
                string typeName = type.Name;
                if (referenceResolver && UnityObjectReferenceResolver.IsReferencedType(type))
                    typeName = $"Ref<{typeName}>";
                return typeName;
        }
    }
}

public interface IValidableAsset
{
    bool IsValidAsset();
}

public class UnityObjectReferenceResolver : IReferenceResolver
{
    bool rootPassed;

    public void AddReference(object context, string reference, object value)
    {
        
    }

    public string GetReference(object context, object value)
    {
        var unityObject = value as UnityEngine.Object;
        if (unityObject == null)
            return null;

        var validableAsset = unityObject as IValidableAsset;
        if (validableAsset != null && !validableAsset.IsValidAsset())
        {
            throw new Exception($"The Object {unityObject.GetType().Name}:{unityObject.name} should not be referenced because it's not a ValidAsset.");
        }

        string guid = default;
#if UNITY_EDITOR
        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(unityObject);
        guid = UnityEditor.AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
#else
Debug.LogError("UnityObjectReferenceResolver.GetReference() function works only in editor");
#endif

        return guid;
    }

    public bool IsReferenced(object context, object value)
    {
        if (!rootPassed)
        {
            rootPassed = true;
            return false;
        }

        //if (UnityEditor.AssetDatabase.IsMainAsset(value as UnityEngine.Object))
        if (value is UnityEngine.Object)
        {
            return true;
        }

        return false;
    }

    static public bool IsReferencedType(Type type)
    {
        if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            return true;

        return false;
    }

    public object ResolveReference(object context, string reference)
    {
        object obj = null;
#if UNITY_EDITOR
        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(reference);
        obj = UnityEditor.AssetDatabase.LoadMainAssetAtPath(path);
#else
Debug.LogError("UnityObjectReferenceResolver.ResolveReference() function works only in editor");
#endif
        return obj;
    }
}
