using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sinan.Collections
{
    /// <summary>
    /// (环形队列)
    /// </summary>
    public class CircularQueue2<T>
    {
        struct Node
        {
            public T Value;

            /// <summary>
            /// 0:空
            /// 1:写入中
            /// 2:写成功
            /// 3:读取中(读取完成,还原到0)
            /// </summary>
            public int State;
        }


        private int head;
        private int tail;
        readonly private int capacity;
        readonly private Node[] buffer;

        public CircularQueue2(int capacity)
        {
            buffer = new Node[capacity];
        }


        public void Enqueue(T item)
        {
            int index = Interlocked.Add(ref tail, 1) % capacity;
            while (true)
            {
                if (Interlocked.CompareExchange(ref buffer[index].State, 1, 0) == 0)
                {
                    buffer[index].Value = item;
                    buffer[index].State = 0;
                    return;
                }
            }
        }

        //public bool TryDequeue(out T result)
        //{
        //    lock (syncRoot)
        //    {
        //        if (head == tail)
        //        {
        //            result = default(T);
        //            return false;
        //        }
        //        if (tail - head > capacity)
        //        {
        //            head = tail - capacity;
        //        }
        //        int index = (head++) % capacity;
        //        result = buffer[index];
        //        buffer[index] = default(T);
        //    }
        //    return true;
        //}

        //public bool TryPeek(out T result)
        //{
        //    lock (syncRoot)
        //    {
        //        if (head == tail)
        //        {
        //            result = default(T);
        //            return false;
        //        }
        //        if (tail - head > capacity)
        //        {
        //            head = tail - capacity;
        //        }
        //        result = buffer[head % capacity];
        //    }
        //    return true;
        //}

        //public T[] DequeueArray()
        //{
        //    lock (syncRoot)
        //    {
        //        int count = tail - head;
        //        if (count > capacity)
        //        {
        //            count = capacity;
        //            head = tail - capacity;
        //        }
        //        T[] r = new T[count];
        //        for (int i = count - 1; i >= 0; i--)
        //        {
        //            r[i] = buffer[(head + i) % capacity];
        //        }
        //        head = tail;
        //        return r;
        //    }
        //}

        //public T[] ToArray()
        //{
        //    lock (syncRoot)
        //    {
        //        int count = tail - head;
        //        if (count > capacity)
        //        {
        //            count = capacity;
        //            head = tail - capacity;
        //        }
        //        T[] r = new T[count];
        //        for (int i = count - 1; i >= 0; i--)
        //        {
        //            r[i] = buffer[(head + i) % capacity];
        //        }
        //        return r;
        //    }
        //}

        //public IEnumerator<T> GetEnumerator()
        //{
        //    lock (syncRoot)
        //    {
        //        int count = tail - head;
        //        if (count > capacity)
        //        {
        //            count = capacity;
        //            head = tail - capacity;
        //        }
        //        T[] r = new T[count];
        //        for (int i = count - 1; i >= 0; i--)
        //        {
        //            yield return buffer[(head + i) % capacity];
        //        }
        //    }
        //}

        //public void Clear()
        //{
        //    lock (syncRoot)
        //    {
        //        head = tail = 0;
        //        Array.Clear(buffer, 0, buffer.Length);
        //    }
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return this.GetEnumerator();
        //}

        #region old
        //static readonly T[] Empty = new T[0];

        //private int m_count;
        //private int m_readIndex;

        //readonly private T[] m_queue;
        //readonly private ReaderWriterLockSlim m_cacheLock;

        ///// <summary>
        ///// 可读数量
        ///// </summary>
        //public int Count
        //{
        //    get
        //    {
        //        m_cacheLock.EnterReadLock();
        //        try
        //        {
        //            return m_count;
        //        }
        //        finally
        //        {
        //            m_cacheLock.ExitReadLock();
        //        }
        //    }
        //}

        //public CircularQueue2(int maxLen)
        //{
        //    m_queue = new T[maxLen];
        //    m_cacheLock = new ReaderWriterLockSlim();
        //}

        ///// <summary>
        ///// 移除并返回位于开始处的对象
        ///// </summary>
        ///// <returns></returns>
        //public bool TryDequeue(out T t)
        //{
        //    m_cacheLock.EnterWriteLock();
        //    try
        //    {
        //        if (m_count == 0)
        //        {
        //            t = default(T);
        //            return false;
        //        }
        //        t = m_queue[m_readIndex];
        //        m_queue[m_readIndex] = default(T);
        //        if (++m_readIndex == m_queue.Length)
        //        {
        //            m_readIndex = 0;
        //        }
        //        m_count--;
        //        return true;
        //    }
        //    finally
        //    {
        //        m_cacheLock.ExitWriteLock();
        //    }
        //}

        ///// <summary>
        ///// 将对象添加到环形队列
        ///// 如果队列已满.则覆盖最早添加的
        ///// </summary>
        ///// <param name="item"></param>
        //public void Enqueue(T item)
        //{
        //    m_cacheLock.EnterWriteLock();
        //    try
        //    {
        //        if (m_count < m_queue.Length)
        //        {
        //            int index = m_readIndex + (m_count++);
        //            if (index >= m_queue.Length) index -= m_queue.Length;
        //            m_queue[index] = item;
        //        }
        //        else //已满
        //        {
        //            m_queue[m_readIndex++] = item;
        //            if (m_readIndex == m_queue.Length)
        //            {
        //                m_readIndex = 0;
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        m_cacheLock.ExitWriteLock();
        //    }
        //}

        ///// <summary>
        ///// 将对象添加到环形队列
        ///// 如果队列已满.则覆盖最早添加的
        ///// </summary>
        ///// <param name="item"></param>
        ///// <param name="old">如果有覆盖,则返回覆盖前的值</param>
        ///// <returns></returns>
        //public bool TryEnqueue(T item, out T old)
        //{
        //    m_cacheLock.EnterWriteLock();
        //    try
        //    {
        //        if (m_count < m_queue.Length)
        //        {
        //            int index = m_readIndex + (m_count++);
        //            if (index >= m_queue.Length) index -= m_queue.Length;
        //            m_queue[index] = item;
        //            old = default(T);
        //            return false;
        //        }
        //        else //已满
        //        {
        //            old = m_queue[m_readIndex];
        //            m_queue[m_readIndex++] = item;
        //            if (m_readIndex == m_queue.Length)
        //            {
        //                m_readIndex = 0;
        //            }
        //            return true;
        //        }
        //    }
        //    finally
        //    {
        //        m_cacheLock.ExitWriteLock();
        //    }
        //}

        //public bool TryPeek(out T t)
        //{
        //    m_cacheLock.EnterWriteLock();
        //    try
        //    {
        //        if (m_count == 0)
        //        {
        //            t = default(T);
        //            return false;
        //        }
        //        t = m_queue[m_readIndex];
        //        return true;
        //    }
        //    finally
        //    {
        //        m_cacheLock.ExitWriteLock();
        //    }
        //}

        //public T[] DequeueAll()
        //{
        //    m_cacheLock.EnterWriteLock();
        //    try
        //    {
        //        T[] ts = ReadQueue();
        //        for (int i = 0; i < m_queue.Length; i++)
        //        {
        //            m_queue[i] = default(T);
        //        }
        //        m_count = 0;
        //        m_readIndex = 0;
        //        return ts;
        //    }
        //    finally
        //    {
        //        m_cacheLock.ExitWriteLock();
        //    }
        //}

        //private T[] ReadQueue()
        //{
        //    if (m_count == 0) return Empty;
        //    T[] ts = new T[m_count];
        //    if (m_count + m_readIndex < m_queue.Length)
        //    {
        //        Array.Copy(m_queue, m_readIndex, ts, 0, m_count);
        //    }
        //    else
        //    {
        //        int count = m_queue.Length - m_readIndex;
        //        Array.Copy(m_queue, m_readIndex, ts, 0, count);
        //        Array.Copy(m_queue, m_readIndex + count, ts, 0, m_count - count);
        //    }
        //    return ts;
        //}

        //public T[] ToArray()
        //{
        //    m_cacheLock.EnterReadLock();
        //    try
        //    {
        //        return ReadQueue();
        //    }
        //    finally
        //    {
        //        m_cacheLock.ExitReadLock();
        //    }
        //}

        //public void Clear()
        //{
        //    m_cacheLock.EnterWriteLock();
        //    try
        //    {
        //        Array.Clear(m_queue, 0, m_queue.Length);
        //        m_count = 0;
        //        m_readIndex = 0;
        //    }
        //    finally
        //    {
        //        m_cacheLock.ExitWriteLock();
        //    }
        //}
        #endregion
    }
}
