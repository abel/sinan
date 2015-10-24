using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Sinan.Collections
{
    public class CircularQueue<T> : IEnumerable<T>
    {
        private int head;
        private int tail;
        readonly private int capacity;
        readonly private T[] buffer;

        [NonSerialized]
        readonly private object syncRoot = new object();

        public CircularQueue(int capacity)
        {
            this.capacity = capacity;
            this.buffer = new T[capacity];
        }

        public int Capacity
        {
            get { return capacity; }
        }

        /// <summary>
        /// 快速判断是否为空,不锁定
        /// </summary>
        public bool IsEmpty
        {
            get { return tail == head; }
        }

        public int Count
        {
            get
            {
                lock (syncRoot)
                {
                    return Math.Min(tail - head, capacity);
                }
            }
        }

        public void Enqueue(T item)
        {
            lock (syncRoot)
            {
                buffer[(tail++) % capacity] = item;
                //Console.WriteLine("Buffer:" + Math.Min(tail - head, capacity));
            }
        }

        public bool TryDequeue(out T result)
        {
            lock (syncRoot)
            {
                if (head == tail)
                {
                    result = default(T);
                    return false;
                }
                if (tail - head > capacity)
                {
                    head = tail - capacity;
                }
                int index = (head++) % capacity;
                result = buffer[index];
                buffer[index] = default(T);
            }
            return true;
        }

        public bool TryPeek(out T result)
        {
            lock (syncRoot)
            {
                if (head == tail)
                {
                    result = default(T);
                    return false;
                }
                if (tail - head > capacity)
                {
                    head = tail - capacity;
                }
                result = buffer[head % capacity];
            }
            return true;
        }

        public T[] DequeueArray()
        {
            lock (syncRoot)
            {
                int count = tail - head;
                if (count > capacity)
                {
                    count = capacity;
                    head = tail - capacity;
                }
                T[] r = new T[count];
                for (int i = count - 1; i >= 0; i--)
                {
                    r[i] = buffer[(head + i) % capacity];
                }
                head = tail;
                return r;
            }
        }

        public T[] ToArray()
        {
            lock (syncRoot)
            {
                int count = tail - head;
                if (count > capacity)
                {
                    count = capacity;
                    head = tail - capacity;
                }
                T[] r = new T[count];
                for (int i = count - 1; i >= 0; i--)
                {
                    r[i] = buffer[(head + i) % capacity];
                }
                return r;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (syncRoot)
            {
                int count = tail - head;
                if (count > capacity)
                {
                    count = capacity;
                    head = tail - capacity;
                }
                T[] r = new T[count];
                for (int i = count - 1; i >= 0; i--)
                {
                    yield return buffer[(head + i) % capacity];
                }
            }
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                head = tail = 0;
                Array.Clear(buffer, 0, buffer.Length);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
