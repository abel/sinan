using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sinan.Collections
{
    //[Obsolete("可使用ConcurrentQueue<T>代替")]
    public class SafeQueue<T>
    {
        private Queue<T> queue;
        private ReaderWriterLockSlim cacheLock;

        public SafeQueue(int maxLen)
        {
            queue = new Queue<T>(maxLen);
            cacheLock = new ReaderWriterLockSlim();
        }

        public T Dequeue()
        {
            cacheLock.EnterWriteLock();
            try
            {
                return queue.Dequeue();
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        public void Enqueue(T item)
        {
            cacheLock.EnterWriteLock();
            try
            {
                queue.Enqueue(item);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        public T Peek()
        {
            cacheLock.EnterReadLock();
            try
            {
                return queue.Peek();
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        public int Count
        {
            get
            {
                cacheLock.EnterReadLock();
                try
                {
                    return queue.Count;
                }
                finally
                {
                    cacheLock.ExitReadLock();
                }
            }
        }

        public T[] ToArray()
        {
            cacheLock.EnterReadLock();
            try
            {
                return queue.ToArray();
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        public List<T> ToList()
        {
            cacheLock.EnterReadLock();
            try
            {
                return queue.ToList<T>();
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }
    }
}
