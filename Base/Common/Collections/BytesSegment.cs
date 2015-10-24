using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Collections
{
    [Serializable]
    public class BytesSegment
    {
        readonly byte[] array;
        readonly int offset;
        readonly int count;

        public BytesSegment(byte[] array, int offset, int count)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Non-negative number required.");

            if (offset > array.Length)
                throw new ArgumentException("out of bounds");

            // now offset is valid, or just beyond the end.
            // Check count -- do it this way to avoid overflow on 'offset + count'
            if (array.Length - offset < count)
                throw new ArgumentException("out of bounds", "offset");

            this.array = array;
            this.offset = offset;
            this.count = count;
        }

        public BytesSegment(byte[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            this.array = array;
            this.offset = 0;
            this.count = array.Length;
        }

        public byte[] Array
        {
            get { return array; }
        }

        public int Offset
        {
            get { return offset; }
        }

        public int Count
        {
            get { return count; }
        }

        public override bool Equals(Object obj)
        {
            if (obj is Sinan.Collections.BytesSegment)
            {
                return this.Equals((Sinan.Collections.BytesSegment)obj);
            }
            return false;
        }

        public bool Equals(Sinan.Collections.BytesSegment obj)
        {
            if (!object.ReferenceEquals(null, obj))
            {
                if (object.ReferenceEquals(this, obj)
                    || ((this.array == obj.Array) && (this.offset == obj.Offset) && (this.count == obj.Count)))
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ((this.array.GetHashCode() ^ this.offset) ^ this.count);
        }

        public static bool operator ==(Sinan.Collections.BytesSegment a, Sinan.Collections.BytesSegment b)
        {
            return object.ReferenceEquals(a, null) ? object.ReferenceEquals(b, null) : a.Equals(b);
        }

        public static bool operator !=(Sinan.Collections.BytesSegment a, Sinan.Collections.BytesSegment b)
        {
            return !(a == b);
        }
    }
}
