using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Sinan.Util
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Int64Union
    {
        [FieldOffset(0)]
        public Int64 All;

        /// <summary>
        /// 低位
        /// </summary>
        [FieldOffset(0)]
        public Int32 Low;

        /// <summary>
        /// 高位
        /// </summary>
        [FieldOffset(4)]
        public Int32 High;

        public static explicit operator Int64Union(Int64 b)
        {
            Int64Union u = new Int64Union();
            u.All = b;
            return u;
        }

        public static explicit operator Int64(Int64Union b)
        {
            return b.All;
        }

        //public static implicit operator Int64Union(Int64 b)
        //{
        //    Int64Union u = new Int64Union();
        //    u.All = b;
        //    return u;
        //}

        //public static implicit operator Int64(Int64Union b)
        //{
        //    return b.All;
        //}
    }



}
