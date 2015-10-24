using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Sinan.Pool
{
    public sealed class ObjectPool<T> where T : IDynamicObject, new()
    {
        private Int32 m_capacity;
        private Int32 m_currentSize;
        private Hashtable m_objects;
        private ArrayList m_listFreeIndex;
        private ArrayList m_listUsingIndex;
        private Object m_createParam;

        public ObjectPool(Object create_param, Int32 init_size, Int32 capacity)
        {
            if (init_size < 0 || capacity < 1 || init_size > capacity)
            {
                throw (new Exception("Invalid parameter!"));
            }

            m_capacity = capacity;
            m_objects = new Hashtable(capacity);
            m_listFreeIndex = new ArrayList(capacity);
            m_listUsingIndex = new ArrayList(capacity);

            m_createParam = create_param;

            for (int i = 0; i < init_size; i++)
            {
                PoolItem<T> pitem = new PoolItem<T>(create_param);
                m_objects.Add(pitem.InnerObjectHashcode, pitem);
                m_listFreeIndex.Add(pitem.InnerObjectHashcode);
            }

            m_currentSize = m_objects.Count;
        }

        public void Release()
        {
            lock (this)
            {
                foreach (DictionaryEntry de in m_objects)
                {
                    ((PoolItem<T>)de.Value).Release();
                }
                m_objects.Clear();
                m_listFreeIndex.Clear();
                m_listUsingIndex.Clear();
            }
        }

        public Int32 CurrentSize
        {
            get { return m_currentSize; }
        }

        public Int32 ActiveCount
        {
            get { return m_listUsingIndex.Count; }
        }

        public Object GetOne()
        {
            lock (this)
            {
                if (m_listFreeIndex.Count == 0)
                {
                    if (m_currentSize == m_capacity)
                    {
                        return null;
                    }
                    PoolItem<T> pnewitem = new PoolItem<T>(m_createParam);
                    m_objects.Add(pnewitem.InnerObjectHashcode, pnewitem);
                    m_listFreeIndex.Add(pnewitem.InnerObjectHashcode);
                    m_currentSize++;
                }

                Int32 nFreeIndex = (Int32)m_listFreeIndex[0];
                PoolItem<T> pitem = (PoolItem<T>)m_objects[nFreeIndex];
                m_listFreeIndex.RemoveAt(0);
                m_listUsingIndex.Add(nFreeIndex);

                if (!pitem.IsValidate)
                {
                    pitem.Recreate();
                }

                pitem.Using = true;
                return pitem.InnerObject;
            }
        }

        public void FreeObject(Object obj)
        {
            lock (this)
            {
                int key = obj.GetHashCode();
                if (m_objects.ContainsKey(key))
                {
                    PoolItem<T> item = (PoolItem<T>)m_objects[key];
                    item.Using = false;
                    m_listUsingIndex.Remove(key);
                    m_listFreeIndex.Add(key);
                }
            }
        }

        public Int32 DecreaseSize(Int32 size)
        {
            Int32 nDecrease = size;
            lock (this)
            {
                if (nDecrease <= 0)
                {
                    return 0;
                }
                if (nDecrease > m_listFreeIndex.Count)
                {
                    nDecrease = m_listFreeIndex.Count;
                }

                for (int i = 0; i < nDecrease; i++)
                {
                    m_objects.Remove(m_listFreeIndex[i]);
                }

                m_listFreeIndex.Clear();
                m_listUsingIndex.Clear();

                foreach (DictionaryEntry de in m_objects)
                {
                    PoolItem<T> pitem = (PoolItem<T>)de.Value;
                    if (pitem.Using)
                    {
                        m_listUsingIndex.Add(pitem.InnerObjectHashcode);
                    }
                    else
                    {
                        m_listFreeIndex.Add(pitem.InnerObjectHashcode);
                    }
                }
            }
            m_currentSize -= nDecrease;
            return nDecrease;
        }
    }
}


