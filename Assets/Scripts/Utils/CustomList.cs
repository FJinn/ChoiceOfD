using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
using System.Linq;
#endif

[Serializable]
public class CustomList<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
    [SerializeReference]
    List<T> internalList;
#if UNITY_EDITOR
	[SerializeField]
    string m_Type;
	[SerializeField]
	string m_SelectedTypeName;
#pragma warning disable CS0414
	[SerializeField]
    bool isOpen = true;
#pragma warning restore CS0414
#endif

	public delegate void ListChangedCallback(List<T> previousList, List<T> currentList);
	public event ListChangedCallback onListChanged;


	void Init()
    {
#if UNITY_EDITOR
        m_Type = typeof(T).FullName;
#endif
    }

	public CustomList()
	{
        Init();

        internalList = new List<T>(); 
	}
    public CustomList(IEnumerable<T> collection)
	{
        Init();

        internalList = new List<T>(collection); 
	}
    public CustomList(int capacity)
	{
        Init();

        internalList = new List<T>(capacity); 
	}

    public T this[int index] 
    {
        get 
        {
            return internalList[index];
        }
        set
        {
			List<T> previous = new List<T>(internalList);

			internalList[index] = value;
			onListChanged(previous, new List<T>(internalList));
		}
    }

    public int Count { get { return internalList.Count; } }
    public int Capacity 
    {
        get 
        { 
            return internalList.Capacity;
        }
        set
        {
			int previousCapacity = internalList.Capacity;
			List<T> previous = new List<T>(internalList);

			internalList.Capacity = value;
			if(internalList.Capacity < previousCapacity)
				onListChanged?.Invoke(previous, new List<T>(internalList));
        }
    }

	public bool IsReadOnly => ((ICollection<T>)internalList).IsReadOnly;

	public bool IsSynchronized => ((ICollection)internalList).IsSynchronized;

	public object SyncRoot => ((ICollection)internalList).SyncRoot;

	public bool IsFixedSize => ((IList)internalList).IsFixedSize;

	public void Add(T item)
	{
		List<T> previous = new List<T>(internalList);

		internalList.Add(item);

		onListChanged?.Invoke(previous, new List<T>(internalList));
	}
    public void AddRange(IEnumerable<T> collection)
	{
		List<T> previous = new List<T>(internalList);

		internalList.AddRange(collection); 

		onListChanged?.Invoke(previous, new List<T>(internalList));
	}
    public ReadOnlyCollection<T> AsReadOnly() { return internalList.AsReadOnly(); }
    public int BinarySearch(int index, int count, T item, IComparer<T> comparer) { return internalList.BinarySearch(index, count, item, comparer); }
    public int BinarySearch(T item) { return internalList.BinarySearch(item); }
    public int BinarySearch(T item, IComparer<T> comparer) { return internalList.BinarySearch( item, comparer); }
    public void Clear()
	{
		List<T> previous = new List<T>(internalList);

		internalList.Clear();

		onListChanged?.Invoke(previous, new List<T>(internalList));
	}
    public bool Contains(T item) { return internalList.Contains(item); }
    public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) { return internalList.ConvertAll<TOutput>(converter); }
    public void CopyTo(T[] array, int arrayIndex) { internalList.CopyTo(array, arrayIndex); }
    public void CopyTo(T[] array) { internalList.CopyTo(array); }
    public void CopyTo(int index, T[] array, int arrayIndex, int count) { internalList.CopyTo(index, array, arrayIndex, count); }
    public bool Exists(Predicate<T> match) { return internalList.Exists(match); }
    public T Find(Predicate<T> match) { return internalList.Find(match); }
    public List<T> FindAll(Predicate<T> match) { return internalList.FindAll(match); }
    public int FindIndex(int startIndex, int count, Predicate<T> match) { return internalList.FindIndex(startIndex, count, match); }
    public int FindIndex(int startIndex, Predicate<T> match) { return internalList.FindIndex(startIndex, match); }
    public int FindIndex(Predicate<T> match) { return internalList.FindIndex(match); }
    public T FindLast(Predicate<T> match) { return internalList.FindLast(match); }
    public int FindLastIndex(int startIndex, int count, Predicate<T> match) { return internalList.FindLastIndex(startIndex, count, match); }
    public int FindLastIndex(int startIndex, Predicate<T> match) { return internalList.FindLastIndex(startIndex, match); }
    public int FindLastIndex(Predicate<T> match) { return internalList.FindLastIndex(match); }
    public void ForEach(Action<T> action) { internalList.ForEach(action); }
    public List<T>.Enumerator GetEnumerator() { return internalList.GetEnumerator(); }
    public List<T> GetRange(int index, int count) { return internalList.GetRange(index, count); }
    public int IndexOf(T item, int index, int count) { return internalList.IndexOf(item, index, count); }
    public bool IsNullOrEmpty() { return internalList == null || internalList.Count == 0; }
    public int IndexOf(T item, int index) { return internalList.IndexOf(item, index); }
    public int IndexOf(T item) { return internalList.IndexOf(item); }
    public void Insert(int index, T item)
	{
		List<T> previous = new List<T>(internalList);

		internalList.Insert(index, item); 

		onListChanged?.Invoke(previous, new List<T>(internalList));
	}
    public void InsertRange(int index, IEnumerable<T> collection)
	{
		List<T> previous = new List<T>(internalList);

		internalList.InsertRange(index, collection); 

		onListChanged?.Invoke(previous, new List<T>(internalList));
	}
    public int LastIndexOf(T item) { return internalList.LastIndexOf(item); }
    public int LastIndexOf(T item, int index) { return internalList.LastIndexOf(item, index); }
    public int LastIndexOf(T item, int index, int count) { return internalList.LastIndexOf(item, index, count); }
    public bool Remove(T item)
	{
		List<T> previous = new List<T>(internalList);

		bool success = internalList.Remove(item); 

		if(success)
			onListChanged?.Invoke(previous, new List<T>(internalList));

		return success;
	}
    public int RemoveAll(Predicate<T> match)
	{
		List<T> previous = new List<T>(internalList);

		int i = internalList.RemoveAll(match); 

		if(i > 0)
			onListChanged?.Invoke(previous, new List<T>(internalList));
		return i;
	}
    public void RemoveAt(int index)
	{
		List<T> previous = new List<T>(internalList);

		internalList.RemoveAt(index);

		onListChanged?.Invoke(previous, new List<T>(internalList));
	}
    public void RemoveRange(int index, int count)
	{
		List<T> previous = new List<T>(internalList);

		internalList.RemoveRange(index, count);

		onListChanged?.Invoke(previous, new List<T>(internalList));
	}
    public void Reverse(int index, int count)
	{
		List<T> previous = new List<T>(internalList);

		internalList.Reverse(index, count);

		onListChanged?.Invoke(previous, new List<T>(internalList));
	}
    public void Reverse()
	{
		List<T> previous = new List<T>(internalList); 

		internalList.Reverse();

		onListChanged?.Invoke(previous, new List<T>(internalList));
	}
    public void Sort(Comparison<T> comparison)
	{
		List<T> previous = new List<T>(internalList);

		internalList.Sort(comparison);

		onListChanged?.Invoke(previous, new List<T>(internalList));
	}
    public void Sort(int index, int count, IComparer<T> comparer) 
	{
		List<T> previous = new List<T>(internalList); 

		internalList.Sort(index, count, comparer);

		onListChanged?.Invoke(previous, new List<T>(internalList));
	}
    public void Sort()
	{
		List<T> previous = new List<T>(internalList); 

		internalList.Sort();

		onListChanged?.Invoke(previous, new List<T>(internalList));
	}
    public void Sort(IComparer<T> comparer)
	{
		List<T> previous = new List<T>(internalList); 

		internalList.Sort(comparer);

		onListChanged?.Invoke(previous, new List<T>(internalList));
	}
    public T[] ToArray() { return internalList.ToArray(); }
    public void TrimExcess() { internalList.TrimExcess(); }
    public bool TrueForAll(Predicate<T> match) { return internalList.TrueForAll(match); }

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)internalList).GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return ((IEnumerable<T>)internalList).GetEnumerator();
	}

    public static implicit operator List<T>(CustomList<T> customList)
    {
        return customList.internalList;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(CustomList<>), true)]
public class CustomListDrawerUIE : PropertyDrawer
{
	#region tools

	FieldInfo GetField(Type type, string name)
	{
		FieldInfo f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

		if(f == null && type.BaseType != null)
		{
			Type baseType = type.BaseType;

			f = GetField(baseType, name);
		}

		return f;
	}

	PropertyInfo GetProperty(Type type, string name)
	{
		PropertyInfo p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

		if (p == null && type.BaseType != null)
		{
			Type baseType = type.BaseType;

			p = GetProperty(baseType, name);
		}

		return p;
	}

	public object GetValue(object source, string name)
	{
		if (source == null)
			return null;
		Type type = source.GetType();
		FieldInfo f = GetField(type, name);
		if (f == null)
		{
			PropertyInfo p = GetProperty(type, name);
			if (p == null)
				return null;
			return p.GetValue(source, null);
		}
		return f.GetValue(source);
	}

	public object GetValue(object source, string name, int index)
	{
		IEnumerable enumerable = GetValue(source, name) as IEnumerable;
		IEnumerator enm = enumerable.GetEnumerator();
		while (index-- >= 0)
			enm.MoveNext();
		return enm.Current;
	}

	void AddEltOfType(object target, Type type)
	{
		dynamic elt = Activator.CreateInstance(type);
		object[] args = new object[] { elt };
		System.Type targetType = target.GetType();
		MethodInfo method = targetType.GetMethod("Add");
		method.Invoke(target, args);
	}
	#endregion

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		object target = property.serializedObject.targetObject;
		string[] splits = property.propertyPath.Replace(".Array.data[", "[").Split('.');
		for (int i = 0; i < splits.Length; i++)
		{
			string element = splits[i]; ;
			if (element.Contains("[")) 
			{
				string elementName = element.Substring(0, element.IndexOf("["));
				int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
				target = GetValue(target, elementName, index);
			}
			else
			{
				target = GetValue(target, element);
			}
		}

		float spacing = EditorGUIUtility.standardVerticalSpacing;
		float lineHeight = EditorGUIUtility.singleLineHeight;
		float height = 0;
		EditorGUI.BeginProperty(position, label, property);

        SerializedProperty listProperty = property.FindPropertyRelative("internalList");
        SerializedProperty typeProp = property.FindPropertyRelative("m_Type");
		SerializedProperty selectedTypeNameProperty = property.FindPropertyRelative("m_SelectedTypeName");
		SerializedProperty isOpenProperty = property.FindPropertyRelative("isOpen");
		string typeName = typeProp.stringValue;
		string selectedTypeName = selectedTypeNameProperty.stringValue;

        if(!string.IsNullOrEmpty(typeName))
        {
		    Type type = Type.GetType(typeName);
		    Assembly assembly = Assembly.GetAssembly(type);
		    IEnumerable<Type> subclassTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(type));

		    //draw Background
		    Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
		    Color labelBackgroundColor = backgroundColor * 0.8f;
            labelBackgroundColor.a = 1.0f;
            Color eltBackgroundColor = 1.5f * backgroundColor;
            eltBackgroundColor.a = 1.0f;
		    Color eltdisplayColor = 1.25f * eltBackgroundColor;
            eltdisplayColor.a = 1.0f;

		    //foldout ~ Draw label
            Rect labelBackgroundRect = EditorGUI.IndentedRect(position);
            labelBackgroundRect.height = lineHeight + 2 * spacing;

            Rect labelBackgroundButtonRect = new Rect(labelBackgroundRect);
            labelBackgroundButtonRect.width -= 60;


            Rect labelFoldoutRect = new Rect(position);
            labelFoldoutRect.height = lineHeight + 2 * spacing;

            Rect labelRect = new Rect(labelBackgroundRect);
            labelRect.y += spacing;
            labelRect.height = lineHeight;

            EditorGUI.DrawRect(labelBackgroundRect, backgroundColor);


            /*if(isOpenProperty.boolValue)
			    EditorGUI.DrawRect(labelBackgroundRect, labelBackgroundColor);*/
            if (GUI.Button(labelBackgroundButtonRect, "", GUIStyle.none))
                isOpenProperty.boolValue = !isOpenProperty.boolValue;

            isOpenProperty.boolValue = EditorGUI.Foldout(labelFoldoutRect, isOpenProperty.boolValue, label);

            int selectedIndex = 0;
		    List<Type> types = new List<Type>();
		    List<string> typesName = new List<string>();
		    if (!type.IsAbstract)
		    {
			    types.Add(type);
			    typesName.Add(type.Name);
		    }
		    Type[] subclassList = subclassTypes.ToArray<Type>();
		    for (int index = 0; index < subclassList.Length; ++index)
		    {
			    Type subType = subclassList[index];
			    if (subType.IsAbstract) continue;

			    types.Add(subType);
			    typesName.Add(subType.Name);
			    if (subType.FullName == selectedTypeName)
				    selectedIndex = types.Count - 1;
		    }

		    if (isOpenProperty.boolValue)
		    {
			    Rect addButtonRect = EditorGUI.IndentedRect(position);
                addButtonRect.height = lineHeight;
			    addButtonRect.width = 3 * lineHeight;
			    addButtonRect.y += position.height - lineHeight;
			    Rect selectorRect = EditorGUI.IndentedRect(position);
                selectorRect.height = lineHeight;
                selectorRect.width -= 3 * lineHeight;
                selectorRect.x += 3 * lineHeight;
                selectorRect.y += position.height - lineHeight;
			    EditorGUI.BeginChangeCheck();

                int indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                int newSelectedIndex = EditorGUI.Popup(selectorRect, "", selectedIndex, typesName.ToArray());
                EditorGUI.indentLevel = indentLevel;

                if (string.IsNullOrEmpty(selectedTypeNameProperty.stringValue) || EditorGUI.EndChangeCheck())
			    {
				    selectedIndex = newSelectedIndex;
				    selectedTypeNameProperty.stringValue = types[selectedIndex].FullName;
			    }
                if (GUI.Button(addButtonRect, "Add"))
                {
                    AddEltOfType(target, types[selectedIndex]);
                    listProperty.serializedObject.Update();
                }
            }

		    //Rect labelPosition = EditorGUI.PrefixLabel(labelRect, GUIUtility.GetControlID(FocusType.Passive), label);
		    Rect countRect = new Rect(labelRect.x + labelRect.width - 60 - spacing, labelRect.y, 60, labelRect.height);
		    EditorGUI.BeginChangeCheck();
		    int wantedElementSize = EditorGUI.IntField(countRect, listProperty.arraySize);
		    if(EditorGUI.EndChangeCheck())
		    {
			    if (wantedElementSize < listProperty.arraySize)
			    {
				    for (int i = listProperty.arraySize - 1; i >= wantedElementSize; --i)
				    {
					    listProperty.DeleteArrayElementAtIndex(i);
                    }
                }
			    else if (wantedElementSize > listProperty.arraySize)
			    {
				    int eltNumToAdd = wantedElementSize - listProperty.arraySize;
				    for (int i = 0; i < eltNumToAdd; ++i)
				    {
					    AddEltOfType(target, types[selectedIndex]);
                    }
                }
                listProperty.serializedObject.Update();
		    }
		    height += labelBackgroundRect.height;



		    if (isOpenProperty.boolValue)
		    {
			    Rect eltGroupRect = EditorGUI.IndentedRect(position);
			    eltGroupRect.y += height;
			    eltGroupRect.height -= height;

                float localHeight = spacing;
                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    SerializedProperty prop = listProperty.GetArrayElementAtIndex(i);

                    float eltHeight = EditorGUI.GetPropertyHeight(prop, true);
                    float itemHeight = 2 * spacing + Mathf.Max(3 * lineHeight + 2 * spacing, eltHeight);
                    height += spacing;
                    Rect eltRect = new Rect(eltGroupRect);
                    eltRect.x += spacing;
                    eltRect.width -= 2 * spacing;
                    eltRect.y += localHeight;
                    localHeight += spacing + itemHeight;
                    eltRect.height = itemHeight;
                    EditorGUI.DrawRect(eltRect, eltBackgroundColor);
                    height += itemHeight;

                    Rect eltDisplayRect = new Rect(eltRect);
                    eltDisplayRect.x += spacing;
                    eltDisplayRect.width -= lineHeight + 2 * spacing;
                    eltDisplayRect.y += spacing;
                    eltDisplayRect.height -= 2 * spacing;
                    EditorGUI.DrawRect(eltDisplayRect, eltdisplayColor);

                    var indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 1;
                    EditorGUI.PropertyField(eltDisplayRect, prop, true);
                    EditorGUI.indentLevel = indent;

                    Rect removeButtonRect = new Rect(eltRect);
                    removeButtonRect.height = lineHeight;
                    removeButtonRect.width = lineHeight;
                    removeButtonRect.x += eltRect.width - removeButtonRect.width;
                    removeButtonRect.y += spacing;
                    if (GUI.Button(removeButtonRect, "x"))
                    {
                        listProperty.DeleteArrayElementAtIndex(i);
                    }

                    Rect upButtonRect = new Rect(removeButtonRect);
                    upButtonRect.y += lineHeight + spacing;
                    bool wasEnabled = GUI.enabled;
                    GUI.enabled = wasEnabled && i > 0;
                    if (GUI.Button(upButtonRect, "↑"))
                    {
                        listProperty.MoveArrayElement(i, i - 1);
                    }

                    GUI.enabled = wasEnabled && i < (listProperty.arraySize - 1);
                    Rect downButtonRect = new Rect(upButtonRect);
                    downButtonRect.y += lineHeight + spacing;
                    if (GUI.Button(downButtonRect, "↓"))
                    {
                        listProperty.MoveArrayElement(i, i + 1);
                    }

                    GUI.enabled = wasEnabled;
                }
		    }
        }
        else
        {
            EditorGUI.LabelField(position, "CustomList has not been initialized properly");
        }
        EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float spacing = EditorGUIUtility.standardVerticalSpacing;
		float lineHeight = EditorGUIUtility.singleLineHeight;
		float height = lineHeight + spacing; 

		SerializedProperty listProperty = property.FindPropertyRelative("internalList");
        SerializedProperty typeProp = property.FindPropertyRelative("m_Type");
        SerializedProperty isOpenProperty = property.FindPropertyRelative("isOpen");
        string typeName = typeProp.stringValue;

        if (!string.IsNullOrEmpty(typeName) && isOpenProperty.boolValue)
        {
			float localHeight = spacing;
			for (int i = 0; i < listProperty.arraySize; i++)
			{
				SerializedProperty prop = listProperty.GetArrayElementAtIndex(i);

				float eltHeight = EditorGUI.GetPropertyHeight(prop, true);
				float itemHeight = 2 * spacing + Mathf.Max(3 * lineHeight + 2 * spacing, eltHeight);
				localHeight += spacing + itemHeight;
			}
			height += localHeight + lineHeight;
		}
		return height;
	}
}
#endif