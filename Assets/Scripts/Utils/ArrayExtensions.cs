using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayExtensions
{
    public static T[] Add<T>(this T[] source, T itemToAdd)
    {
        T[] dest = new T[source.Length + 1];
        Array.Copy(source, 0, dest, 0, source.Length);
        dest[dest.Length - 1] = itemToAdd;
        return dest;
    }

    public static T[] RemoveAt<T>(this T[] source, int index)
    {
        T[] dest = new T[source.Length - 1];
        if (index > 0)
            Array.Copy(source, 0, dest, 0, index);

        if (index < source.Length - 1)
            Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }

    public static T[] RemoveAll<T>(this T[] _list, Predicate<T> _match)
    {
        for (int i = _list.Length - 1; i >= 0; i--)
        {
            if (_match(_list[i]))
                _list = _list.RemoveAt(i);
        }

        return _list;
    }

    public static string ToString(this IEnumerable enumerable, string separator)
    {
        if (enumerable == null)
            throw new ArgumentException("source can not be null.");

        string outputString = "";
        if (string.IsNullOrEmpty(separator))
        {
            separator = System.Environment.NewLine;
        }

        if (enumerable != null)
        {
            foreach (var msg in enumerable)
            {
                if (msg != null)
                {
                    outputString += msg.ToString();
                    outputString += separator;
                }
            }
        }

        return outputString.Substring(0, Mathf.Max(0, outputString.Length - separator.Length));
    }

    public static void RemoveLast<T>(this List<T> _this)
    {
        _this.RemoveAt(_this.Count - 1);
    }

    public static bool IsNullOrEmpty(this Array _this)
    {
        return _this == null || _this.Length == 0;
    }

    public static bool IsNullOrEmpty<T>(this List<T> _this)
    {
        return _this == null || _this.Count == 0;
    }

    public static int IndexOf(this Array _this, object _value)
    {
        return Array.IndexOf(_this, _value);
    }

    public static int IndexOf(this Array _this, object _value, int _startIndex)
    {
        return Array.IndexOf(_this, _value, _startIndex);
    }

    public static int IndexOf(this Array _this, object _value, int _startIndex, int _count)
    {
        return Array.IndexOf(_this, _value, _startIndex, _count);
    }

    public static int IndexOf<T>(this T[] _this, T _value)
    {
        return Array.IndexOf(_this, _value);
    }

    public static int IndexOf<T>(this T[] _this, T _value, int _startIndex)
    {
        return Array.IndexOf(_this, _value, _startIndex);
    }

    public static int IndexOf<T>(this T[] _this, T _value, int _startIndex, int _count)
    {
        return Array.IndexOf(_this, _value, _startIndex, _count);
    }

    public static bool Contains(this Array _this, object _value)
    {
        return Array.IndexOf(_this, _value) != -1;
    }

    public static bool Contains<T>(this T[] _this, T _value)
    {
        return Array.IndexOf(_this, _value) != -1;
    }

    public static bool Contains<T>(this T[] _this, T _value, int _arraySize)
    {
        return Array.IndexOf(_this, _value, 0, _arraySize) != -1;
    }

    public static T Find<T>(this T[] _this, Predicate<T> _match)
    {
        return Array.Find(_this, _match);
    }

    public static T[] FindAll<T>(this T[] _this, Predicate<T> _match)
    {
        return Array.FindAll(_this, _match);
    }

    public static int FindIndex<T>(this T[] _this, Predicate<T> _match)
    {
        return Array.FindIndex(_this, _match);
    }

    public static bool Exists<T>(this T[] _this, Predicate<T> _match)
    {
        return Array.Exists(_this, _match);
    }

    public static List<T> ToList<T>(this T[] _this)
    {
        return new List<T>(_this);
    }

    public static T[] Populate<T>(this T[] array)
        where T : new()
    {
        for (int i = 0; i < array.Length; i++)
            array[i] = new T();
        return array;
    }
}