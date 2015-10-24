using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

namespace Sinan.Collections
{
    /// <summary>
    /// 线程安全的字典,使用读写锁实现.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        readonly private object m_cacheLock = new object();
        readonly private IDictionary<TKey, TValue> m_innerCache;

        #region 构造函数
        public ConcurrentDictionary()
        {
            m_innerCache = new Dictionary<TKey, TValue>();
        }

        public ConcurrentDictionary(int p, int c)
        {
            m_innerCache = new Dictionary<TKey, TValue>(p * c);
        }

        public ConcurrentDictionary(IDictionary<TKey, TValue> dictionary)
        {
            m_innerCache = dictionary;
        }
        public ConcurrentDictionary(int capacity)
        {
            m_innerCache = new Dictionary<TKey, TValue>(capacity);
        }
        #endregion

        public void Add(TKey key, TValue value)
        {
            lock (m_cacheLock)
            {
                m_innerCache.Add(key, value);
            }
        }

        public bool TryAdd(TKey key, TValue value)
        {
            lock (m_cacheLock)
            {
                if (m_innerCache.ContainsKey(key))
                {
                    return false;
                }
                m_innerCache.Add(key, value);
            }
            return true;
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue oldValue;
            lock (m_cacheLock)
            {
                if (!m_innerCache.TryGetValue(key, out oldValue))
                {
                    oldValue = valueFactory(key);
                    m_innerCache.Add(key, oldValue);
                }
            }
            return oldValue;
        }


        public TValue GetOrAdd(TKey key, TValue value)
        {
            TValue oldValue;
            lock (m_cacheLock)
            {
                if (!m_innerCache.TryGetValue(key, out oldValue))
                {
                    m_innerCache.Add(key, value);
                    return value;
                }
            }
            return oldValue;
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue oldValue;
            lock (m_cacheLock)
            {
                if (m_innerCache.TryGetValue(key, out oldValue))
                {
                    TValue value = updateValueFactory(key, oldValue);
                    if (!oldValue.Equals(value))
                    {
                        m_innerCache[key] = value;
                    }
                    return value;
                }
                else
                {
                    m_innerCache.Add(key, addValue);
                    return addValue;
                }
            }
        }

        public AddOrUpdateStatus AddOrUpdateAndGetOld(TKey key, TValue value, out TValue oldValue)
        {
            lock (m_cacheLock)
            {
                if (m_innerCache.TryGetValue(key, out oldValue))
                {
                    if (object.Equals(oldValue, value))
                    {
                        return AddOrUpdateStatus.Unchanged;
                    }
                    m_innerCache[key] = value;
                    return AddOrUpdateStatus.Updated;
                }
                else
                {
                    m_innerCache.Add(key, value);
                    return AddOrUpdateStatus.Added;
                }
            }
        }

        public bool Remove(TKey key)
        {
            lock (m_cacheLock)
            {
                return m_innerCache.Remove(key);
            }
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            lock (m_cacheLock)
            {
                if (m_innerCache.TryGetValue(key, out value))
                {
                    return m_innerCache.Remove(key);
                }
            }
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            lock (m_cacheLock)
            {
                return m_innerCache.ContainsKey(key);
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (m_cacheLock)
                {
                    return m_innerCache.Keys.ToArray();
                }
            }
        }

        public List<TKey> GetKeyList()
        {
            lock (m_cacheLock)
            {
                return m_innerCache.Keys.ToList();
            }
        }

        public TKey[] GetKeyArray()
        {
            lock (m_cacheLock)
            {
                return m_innerCache.Keys.ToArray<TKey>();
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (m_cacheLock)
                {
                    return m_innerCache.Values.ToArray();
                }
            }
        }

        public List<TValue> GetValueList()
        {
            lock (m_cacheLock)
            {
                return m_innerCache.Values.ToList();
            }
        }

        public TValue[] GetValueArray()
        {
            lock (m_cacheLock)
            {
                return m_innerCache.Values.ToArray<TValue>();
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (m_cacheLock)
            {
                return m_innerCache.TryGetValue(key, out value);
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (m_cacheLock)
                {
                    return m_innerCache[key];
                }
            }
            set
            {
                lock (m_cacheLock)
                {
                    if (this.m_innerCache.ContainsKey(key))
                    {
                        this.m_innerCache[key] = value;
                    }
                    else
                    {
                        this.m_innerCache.Add(key, value);
                    }
                }
            }
        }

        #region ICollection<KeyValuePair<TKey,TValue>> 成员

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            lock (m_cacheLock)
            {
                this.m_innerCache.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            TValue value;
            lock (m_cacheLock)
            {
                if (this.m_innerCache.TryGetValue(item.Key, out value))
                {
                    return object.Equals(value, item.Value);
                }
                return false;
            }
        }

        public bool Contains(TKey key, TValue item)
        {
            TValue value;
            lock (m_cacheLock)
            {
                if (this.m_innerCache.TryGetValue(key, out value))
                {
                    return object.Equals(value, item);
                }
                return false;
            }
        }

        public int Count
        {
            get
            {
                lock (m_cacheLock)
                {
                    return this.m_innerCache.Count;
                }
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            TValue value;
            lock (m_cacheLock)
            {
                if (this.m_innerCache.TryGetValue(item.Key, out value))
                {
                    if (object.Equals(value, item.Value))
                    {
                        return this.m_innerCache.Remove(item.Key);
                    }
                }
            }
            return false;
        }

        public bool Remove(TKey key, TValue value)
        {
            TValue oldV;
            lock (m_cacheLock)
            {
                if (this.m_innerCache.TryGetValue(key, out oldV))
                {
                    if (object.Equals(oldV, value))
                    {
                        return this.m_innerCache.Remove(key);
                    }
                }
            }
            return false;
        }

        public bool Remove<T>(TKey key, T value, Func<TValue, T, bool> func)
        {
            TValue oldV;
            lock (m_cacheLock)
            {
                if (this.m_innerCache.TryGetValue(key, out oldV))
                {
                    if (func(oldV, value))
                    {
                        return this.m_innerCache.Remove(key);
                    }
                }
            }
            return false;
        }
        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> 成员

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (m_cacheLock)
            {
                foreach (KeyValuePair<TKey, TValue> pair in this.m_innerCache)
                {
                    yield return pair;
                }
            }
        }

        #endregion

        #region IEnumerable 成员
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> 成员
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int startIndex)
        {
            int index = 0;
            lock (m_cacheLock)
            {
                foreach (KeyValuePair<TKey, TValue> pair in this.m_innerCache)
                {
                    if (index >= startIndex && array.Length > index)
                    {
                        array[index++] = pair;
                    }
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion
    }

    /// <summary>
    /// 添加更更新的结果
    /// </summary>
    public enum AddOrUpdateStatus
    {
        /// <summary>
        /// 添加
        /// </summary>
        Added,
        /// <summary>
        /// 更新
        /// </summary>
        Updated,
        /// <summary>
        /// 未改变
        /// </summary>
        Unchanged
    };
}
