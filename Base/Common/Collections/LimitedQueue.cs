using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sinan.Collections
{
    /// <summary>
    /// (环形队列)受限队列.(只保存最近插入的一定数量的记录)
    /// </summary>
    public class LimitedQueue<T>
    {
        readonly private int m_maxLen;
        readonly private Queue<T> m_queue;
        readonly private ReaderWriterLockSlim m_cacheLock;

        public LimitedQueue(int maxLen)
        {
            m_queue = new Queue<T>(maxLen);
            this.m_maxLen = maxLen;
            m_cacheLock = new ReaderWriterLockSlim();
        }

        public T Dequeue()
        {
            m_cacheLock.EnterWriteLock();
            try
            {
                return m_queue.Dequeue();
            }
            finally
            {
                m_cacheLock.ExitWriteLock();
            }
        }

        public void Enqueue(T item)
        {
            m_cacheLock.EnterWriteLock();
            try
            {
                if (m_queue.Count == m_maxLen)
                {
                    m_queue.Dequeue();
                }
                m_queue.Enqueue(item);
            }
            finally
            {
                m_cacheLock.ExitWriteLock();
            }
        }

        public T Peek()
        {
            m_cacheLock.EnterReadLock();
            try
            {
                return m_queue.Peek();
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
                    return m_queue.Count;
                }
                finally
                {
                    m_cacheLock.ExitReadLock();
                }
            }
        }

        public T[] ToArray()
        {
            m_cacheLock.EnterReadLock();
            try
            {
                return m_queue.ToArray();
            }
            finally
            {
                m_cacheLock.ExitReadLock();
            }
        }

        public List<T> ToList()
        {
            m_cacheLock.EnterReadLock();
            try
            {
                return m_queue.ToList<T>();
            }
            finally
            {
                m_cacheLock.ExitReadLock();
            }
        }
    }
}
