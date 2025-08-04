using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 字典优化类 (空间换时间)
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class DictionaryExtend<TKey, TValue> : Dictionary<TKey, TValue>
{
    public List<TKey> mList = new List<TKey>();

    public DictionaryExtend()
    {
        
    }
    
    /// <summary>
    /// 增加
    /// </summary>
    /// <param name="tkey"></param>
    /// <param name="tvalue"></param>
    public new void Add(TKey tkey, TValue tvalue)
    {
        mList.Add(tkey);
        base.Add(tkey, tvalue);
    }
    
    /// <summary>
    /// 移除
    /// </summary>
    /// <param name="tkey"></param>
    /// <returns></returns>
    public new bool Remove(TKey tkey)
    {
        mList.Remove(tkey);
        return base.Remove(tkey);
    }
    
    /// <summary>
    /// 方便取存值
    /// </summary>
    /// <param name="tkey"></param>
    /// <returns></returns>
    public new TValue this[TKey tkey]
    {
        get
        {
            return base[tkey];
        }
        set
        {
            if (ContainsKey(tkey))
            {
                base[tkey] = value;
            }
            else
            {
                Add(tkey, value);
            }
        }
    }
    
    /// <summary>
    /// 查询是否有键名
    /// </summary>
    /// <param name="tkey"></param>
    /// <returns></returns>
    public new bool ContainsKey(TKey tkey)
    {
        return mList.Contains(tkey);
    }
    
    /// <summary>
    /// 清除数据
    /// </summary>
    public new void Clear()
    {
        mList.Clear();
        base.Clear();
    }
}


/// <summary>
/// 序列化KeyValuePair
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public abstract class SerializedKeyValuePair<TKey, TValue> where TKey : IEquatable<TKey>
{
    public abstract TKey Key { get; set; }
    public abstract TValue Value { get; set; }
    public SerializedKeyValuePair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
    public KeyValuePair<TKey, TValue> AsKeyValuePair()
    {
        return new KeyValuePair<TKey, TValue>(Key, Value);
    }
}


/// <summary>
/// 序列化字典
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public abstract class SerializedDictionary<TKeyValuePair, TKey, TValue> : IDictionary<TKey, TValue> where TKeyValuePair : SerializedKeyValuePair<TKey, TValue> where TKey : IEquatable<TKey>
{
    protected abstract List<TKeyValuePair> KeyValuePairs { get; }

    public abstract void Add(TKey key, TValue value);

    public bool ContainsKey(TKey key)
    {
        foreach (TKeyValuePair kvp in KeyValuePairs)
        {
            if (kvp.Key.Equals(key))
            {
                return true;
            }
        }
        return false;
    }

    public bool ContainsValue(TValue value)
    {
        EqualityComparer<TValue> equalityComparer = EqualityComparer<TValue>.Default;

        foreach (TKeyValuePair kvp in KeyValuePairs)
        {
            if (equalityComparer.Equals(kvp.Value, value))
            {
                return true;
            }
        }
        return false;
    }

    public bool Remove(TKey key)
    {
        for (int i = 0; i < KeyValuePairs.Count; i++)
        {
            TKeyValuePair kvp = KeyValuePairs[i];

            if (kvp.Key.Equals(key))
            {
                KeyValuePairs.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        foreach (TKeyValuePair kvp in KeyValuePairs)
        {
            if (kvp.Key.Equals(key))
            {
                value = kvp.Value;
                return true;
            }
        }
        value = default(TValue);
        return false;
    }

    public TValue GetValue(TKey key)
    {
        foreach (TKeyValuePair kvp in KeyValuePairs)
        {
            if (kvp.Key.Equals(key))
            {
                return kvp.Value;
            }
        }
        throw new ArgumentException("No value was found for the given key");
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        KeyValuePairs.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        foreach (TKeyValuePair kvp in KeyValuePairs)
        {
            if (kvp.Key.Equals(item.Key))
            {
                return EqualityComparer<TValue>.Default.Equals(kvp.Value, item.Value);
            }
        }
        return false;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        for (int i = 0; i < KeyValuePairs.Count; i++)
        {
            TKeyValuePair kvp = KeyValuePairs[i];
            array[i + arrayIndex] = new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value);
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (Contains(item))
        {
            Remove(item.Key);
            return true;
        }
        return false;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (TKeyValuePair kvp in KeyValuePairs)
        {
            yield return new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value);
        }
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public TValue this[TKey key]
    {
        get { return GetValue(key); }
        set
        {
            for (int i = 0; i < KeyValuePairs.Count; i++)
            {
                if (KeyValuePairs[i].Key.Equals(key))
                {
                    KeyValuePairs[i].Value = value;
                    return;
                }
            }
            Add(key, value);
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            List<TKey> list = new List<TKey>();
            foreach (TKeyValuePair kvp in KeyValuePairs)
            {
                list.Add(kvp.Key);
            }
            return list;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            List<TValue> list = new List<TValue>();
            foreach (TKeyValuePair kvp in KeyValuePairs)
            {
                list.Add(kvp.Value);
            }
            return list;
        }
    }

    public int Count { get { return KeyValuePairs.Count; } }

    public bool IsReadOnly { get { return false; } }

    public Dictionary<TKey, TValue> ToDictionary()
    {
        Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
        foreach (TKeyValuePair kvp in KeyValuePairs)
        {
            dictionary.Add(kvp.Key, kvp.Value);
        }
        return dictionary;
    }
}