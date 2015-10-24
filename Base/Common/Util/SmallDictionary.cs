using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Sinan.Util
{
    internal static class HashHelpers
    {
        // Fields
        internal static readonly int[] primes = new int[] { 
        3, 7, 11, 0x11, 0x17, 0x1d, 0x25, 0x2f, 0x3b, 0x47, 0x59, 0x6b, 0x83, 0xa3, 0xc5, 0xef, 
        0x125, 0x161, 0x1af, 0x209, 0x277, 0x2f9, 0x397, 0x44f, 0x52f, 0x63d, 0x78b, 0x91d, 0xaf1, 0xd2b, 0xfd1, 0x12fd, 
        0x16cf, 0x1b65, 0x20e3, 0x2777, 0x2f6f, 0x38ff, 0x446f, 0x521f, 0x628d, 0x7655, 0x8e01, 0xaa6b, 0xcc89, 0xf583, 0x126a7, 0x1619b, 
        0x1a857, 0x1fd3b, 0x26315, 0x2dd67, 0x3701b, 0x42023, 0x4f361, 0x5f0ed, 0x72125, 0x88e31, 0xa443b, 0xc51eb, 0xec8c1, 0x11bdbf, 0x154a3f, 0x198c4f, 
        0x1ea867, 0x24ca19, 0x2c25c1, 0x34fa1b, 0x3f928f, 0x4c4987, 0x5b8b6f, 0x6dda89
     };


        internal static int GetPrime(int min)
        {
            for (int i = 0; i < primes.Length; i++)
            {
                int num2 = primes[i];
                if (num2 >= min)
                {
                    return num2;
                }
            }
            for (int j = min | 1; j < 0x7fffffff; j += 2)
            {
                if (IsPrime(j))
                {
                    return j;
                }
            }
            return min;
        }

        internal static bool IsPrime(int candidate)
        {
            if ((candidate & 1) == 0)
            {
                return (candidate == 2);
            }
            int num = (int)Math.Sqrt((double)candidate);
            for (int i = 3; i <= num; i += 2)
            {
                if ((candidate % i) == 0)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// 精减版字典
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SmallDictionary<TKey, TValue>
    {
        // Nested Types
        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            public int hashCode;
            public int next;
            public TKey key;
            public TValue value;
        }

        // Fields
        private int[] buckets;
        private int count;
        private Entry[] entries;
        private int freeCount;
        private int freeList;

        // Methods
        public SmallDictionary()
            : this(0)
        {
        }

        public SmallDictionary(int capacity)
        {
            if (capacity > 0)
            {
                this.Initialize(capacity);
            }
        }

        public bool TryAdd(TKey key, TValue value)
        {
            return this.Insert(key, value, true);
        }

        public void Clear()
        {
            if (this.count > 0)
            {
                for (int i = 0; i < this.buckets.Length; i++)
                {
                    this.buckets[i] = -1;
                }
                Array.Clear(this.entries, 0, this.count);
                this.freeList = -1;
                this.count = 0;
                this.freeCount = 0;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return (this.FindEntry(key) >= 0);
        }

        private int FindEntry(TKey key)
        {
            if (this.buckets != null)
            {
                int num = key.GetHashCode() & 0x7fffffff;
                for (int i = this.buckets[num % this.buckets.Length]; i >= 0; i = this.entries[i].next)
                {
                    if ((this.entries[i].hashCode == num) && this.entries[i].key.Equals(key))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        internal TValue GetValueOrDefault(TKey key)
        {
            int index = this.FindEntry(key);
            if (index >= 0)
            {
                return this.entries[index].value;
            }
            return default(TValue);
        }

        private void Initialize(int capacity)
        {
            int prime = HashHelpers.GetPrime(capacity);
            this.buckets = new int[prime];
            for (int i = 0; i < this.buckets.Length; i++)
            {
                this.buckets[i] = -1;
            }
            this.entries = new Entry[prime];
            this.freeList = -1;
        }

        private bool Insert(TKey key, TValue value, bool add)
        {
            int freeList;
            if (this.buckets == null)
            {
                this.Initialize(0);
            }
            int num = key.GetHashCode() & 0x7fffffff;
            int index = num % this.buckets.Length;
            for (int i = this.buckets[index]; i >= 0; i = this.entries[i].next)
            {
                if ((this.entries[i].hashCode == num) && this.entries[i].key.Equals(key))
                {
                    if (add)
                    {
                        return false;
                    }
                    this.entries[i].value = value;
                    return true;
                }
            }
            if (this.freeCount > 0)
            {
                freeList = this.freeList;
                this.freeList = this.entries[freeList].next;
                this.freeCount--;
            }
            else
            {
                if (this.count == this.entries.Length)
                {
                    this.Resize();
                    index = num % this.buckets.Length;
                }
                freeList = this.count;
                this.count++;
            }
            this.entries[freeList].hashCode = num;
            this.entries[freeList].next = this.buckets[index];
            this.entries[freeList].key = key;
            this.entries[freeList].value = value;
            this.buckets[index] = freeList;
            return true;
        }

        public bool Remove(TKey key)
        {
            if (this.buckets != null)
            {
                int num = key.GetHashCode() & 0x7fffffff;
                int index = num % this.buckets.Length;
                int num3 = -1;
                for (int i = this.buckets[index]; i >= 0; i = this.entries[i].next)
                {
                    if ((this.entries[i].hashCode == num) && this.entries[i].key.Equals(key))
                    {
                        if (num3 < 0)
                        {
                            this.buckets[index] = this.entries[i].next;
                        }
                        else
                        {
                            this.entries[num3].next = this.entries[i].next;
                        }
                        this.entries[i].hashCode = -1;
                        this.entries[i].next = this.freeList;
                        this.entries[i].key = default(TKey);
                        this.entries[i].value = default(TValue);
                        this.freeList = i;
                        this.freeCount++;
                        return true;
                    }
                    num3 = i;
                }
            }
            return false;
        }

        private void Resize()
        {
            int prime = HashHelpers.GetPrime(this.count * 2);
            int[] numArray = new int[prime];
            for (int i = 0; i < numArray.Length; i++)
            {
                numArray[i] = -1;
            }
            Entry[] destinationArray = new Entry[prime];
            Array.Copy(this.entries, 0, destinationArray, 0, this.count);
            for (int j = 0; j < this.count; j++)
            {
                int index = destinationArray[j].hashCode % prime;
                destinationArray[j].next = numArray[index];
                numArray[index] = j;
            }
            this.buckets = numArray;
            this.entries = destinationArray;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = this.FindEntry(key);
            if (index >= 0)
            {
                value = this.entries[index].value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        public int Count
        {
            get
            {
                return (this.count - this.freeCount);
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                int index = this.FindEntry(key);
                if (index >= 0)
                {
                    return this.entries[index].value;
                }
                return default(TValue);
            }
            set
            {
                this.Insert(key, value, false);
            }
        }
    }
}
