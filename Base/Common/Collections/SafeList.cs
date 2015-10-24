using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

namespace Sinan.Collections
{
    /// <summary>
    /// 线程安全的列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SafeList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private readonly ReaderWriterLockSlim m_cacheLock = new ReaderWriterLockSlim();
        private readonly List<T> m_innerList;

        public SafeList()
        {
            m_innerList = new List<T>();
        }

        public SafeList(int capacity)
        {
            m_innerList = new List<T>(capacity);
        }

        public ReaderWriterLockSlim SyncRoot
        {
            get { return m_cacheLock; }
        }

        #region IList<T> 成员

        public int IndexOf(T item)
        {
            m_cacheLock.EnterReadLock();
            try
            {
                int count = this.m_innerList.Count;
                for (int i = 0; i < count; i++)
                {
                    if (object.Equals(this.m_innerList[i], item))
                    {
                        return i;
                    }
                }
                return -1;
            }
            finally
            {
                m_cacheLock.ExitReadLock();
            }
        }

        public void Insert(int index, T item)
        {
            m_cacheLock.EnterWriteLock();
            try
            {
                this.m_innerList.Insert(index, item);
            }
            finally
            {
                m_cacheLock.ExitWriteLock();
            }
        }

        public void RemoveAt(int index)
        {
            m_cacheLock.EnterWriteLock();
            try
            {
                this.m_innerList.RemoveAt(index);
            }
            finally
            {
                m_cacheLock.ExitWriteLock();
            }
        }

        public T this[int index]
        {
            get
            {
                m_cacheLock.EnterReadLock();
                try
                {
                    return this.m_innerList[index];
                }
                finally
                {
                    m_cacheLock.ExitReadLock();
                }
            }
            set
            {
                m_cacheLock.EnterWriteLock();
                try
                {
                    this.m_innerList[index] = value;
                }
                finally
                {
                    m_cacheLock.ExitWriteLock();
                }
            }
        }

        #endregion

        #region ICollection<T> 成员

        public void Add(T item)
        {
            m_cacheLock.EnterWriteLock();
            try
            {
                this.m_innerList.Add(item);
            }
            finally
            {
                m_cacheLock.ExitWriteLock();
            }

        }

        public void Clear()
        {
            m_cacheLock.EnterWriteLock();
            try
            {
                this.m_innerList.Clear();
            }
            finally
            {
                m_cacheLock.ExitWriteLock();
            }

        }

        public bool Contains(T item)
        {
            m_cacheLock.EnterReadLock();
            try
            {
                return m_innerList.Contains(item);
            }
            finally
            {
                m_cacheLock.ExitReadLock();
            }
        }

        public void CopyTo(T[] array, int index)
        {
            m_cacheLock.EnterReadLock();
            try
            {
                this.m_innerList.CopyTo(array, index);
            }
            finally
            {
                m_cacheLock.ExitReadLock();
            }
        }

        public int Count
        {
            get
            {
                m_cacheLock.EnterReadLock();
                try
                {
                    return m_innerList.Count;
                }
                finally
                {
                    m_cacheLock.ExitReadLock();
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            m_cacheLock.EnterWriteLock();
            try
            {
                return m_innerList.Remove(item);
            }
            finally
            {
                m_cacheLock.ExitWriteLock();
            }
        }

        #endregion

        #region IEnumerable<T> 成员

        public IEnumerator<T> GetEnumerator()
        {
            m_cacheLock.EnterReadLock();
            try
            {
                foreach (T item in m_innerList)
                {
                    yield return item;
                }
            }
            finally
            {
                m_cacheLock.ExitReadLock();
            }
        }

        #endregion

        #region IEnumerable 成员

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region 扩展方法
        public int RemoveAll(Func<T, bool> func)
        {
            int count = 0;
            m_cacheLock.EnterWriteLock();
            try
            {
                for (int i = m_innerList.Count - 1; i >= 0; i--)
                {
                    if (func(m_innerList[i]))
                    {
                        m_innerList.RemoveAt(i);
                        count++;
                    }
                }
            }
            finally
            {
                m_cacheLock.ExitWriteLock();
            }
            return count;
        }

        public int RemoveLeft(Func<T, bool> func)
        {
            int count = 0;
            m_cacheLock.EnterWriteLock();
            try
            {
                while (m_innerList.Count > 0 && func(m_innerList[0]))
                {
                    m_innerList.RemoveAt(0);
                    count++;
                }
            }
            finally
            {
                m_cacheLock.ExitWriteLock();
            }
            return count;
        }

        public int RemoveRight(Func<T, bool> func)
        {
            int count = 0;
            m_cacheLock.EnterWriteLock();
            try
            {
                int index = m_innerList.Count - 1;
                while (index >= 0 && func(m_innerList[index]))
                {
                    m_innerList.RemoveAt(index--);
                    count++;
                }
            }
            finally
            {
                m_cacheLock.ExitWriteLock();
            }
            return count;
        }


        public int RemoveAll(T item)
        {
            int count = 0;
            m_cacheLock.EnterWriteLock();
            try
            {
                for (int i = m_innerList.Count - 1; i >= 0; i--)
                {
                    if (object.Equals(item, m_innerList[i]))
                    {
                        m_innerList.RemoveAt(i);
                        count++;
                    }
                }
            }
            finally
            {
                m_cacheLock.ExitWriteLock();
            }
            return count;
        }

        public AddOrUpdateStatus AddOnlyOne(T item)
        {
            m_cacheLock.EnterUpgradeableReadLock();
            try
            {
                if (m_innerList.Contains(item))
                {
                    return AddOrUpdateStatus.Unchanged;
                }
                else
                {
                    m_cacheLock.EnterWriteLock();
                    try
                    {
                        m_innerList.Add(item);
                    }
                    finally
                    {
                        m_cacheLock.ExitWriteLock();
                    }
                    return AddOrUpdateStatus.Added;
                }
            }
            finally
            {
                m_cacheLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// 添加到最后一项
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public AddOrUpdateStatus AddOnlyOneToLast(T item)
        {
            m_cacheLock.EnterWriteLock();
            try
            {
                if (m_innerList.Remove(item))
                {
                    m_innerList.Add(item);
                    return AddOrUpdateStatus.Updated;
                }
                m_innerList.Add(item);
            }
            finally
            {
                m_cacheLock.ExitWriteLock();
            }
            return AddOrUpdateStatus.Added;
        }

        public List<T> FindAll(Func<T, bool> func)
        {
            List<T> newList = new List<T>();
            m_cacheLock.EnterReadLock();
            try
            {
                int count = this.m_innerList.Count;
                for (int i = 0; i < count; i++)
                {
                    if (func(m_innerList[i]))
                    {
                        newList.Add(m_innerList[i]);
                    }
                }
            }
            finally
            {
                m_cacheLock.ExitReadLock();
            }
            return newList;
        }

        public List<T> ToNewList()
        {
            List<T> newList = new List<T>();
            m_cacheLock.EnterReadLock();
            try
            {
                int count = this.m_innerList.Count;
                for (int i = 0; i < count; i++)
                {
                    newList.Add(m_innerList[i]);
                }
            }
            finally
            {
                m_cacheLock.ExitReadLock();
            }
            return newList;
        }
        #endregion
    }
}
