using System.Collections.Generic;
/// <summary>
/// 字典替代类
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class DictionaryExtend<TKey, TValue> : Dictionary<TKey, TValue>
{
    public List<TKey> mList = new List<TKey>();
    public DictionaryExtend() {}
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
