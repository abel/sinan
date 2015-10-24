using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Schedule;

namespace Sinan.Cache
{
    public abstract class ObjectCache<TKey, TValue>
         where TValue : class
    {

        /// <summary>
        /// 弱引用玩家,用于缓存
        /// </summary>
        readonly Dictionary<TKey, WeakReference<TValue>> m_buffer;

        protected ObjectCache(int capacity = 100)
        {
            m_buffer = new Dictionary<TKey, WeakReference<TValue>>(capacity);
        }

        /// <summary>
        /// 缓存数量,弱引用不准确
        /// </summary>
        public int Count
        {
            get { return m_buffer.Count; }
        }

        /// <summary>
        /// 获取与指定的键相关联的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            WeakReference<TValue> refValue;
            lock (m_buffer)
            {
                if (!m_buffer.TryGetValue(key, out refValue))
                {
                    value = CreateValue(key);
                    if (value == null)
                    {
                        return false;
                    }
                    refValue = new WeakReference<TValue>(value);
                    m_buffer.Add(key, refValue);
                }
                else
                {
                    value = refValue.Target;
                    if (value == null)
                    {
                        value = CreateValue(key);
                        if (value == null)
                        {
                            m_buffer.Remove(key);
                            return false;
                        }
                        refValue.Target = value;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// 移出指定键的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(TKey key)
        {
            lock (m_buffer)
            {
                return m_buffer.Remove(key);
            }
        }

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract TValue CreateValue(TKey key);

    }
}

