using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.AMF3
{
    /// <summary>
    /// 最大值/当前值模式..
    /// </summary>
    public class MVPair : ExternalizableBase
    {
        /// <summary>
        /// 最大值
        /// </summary>
        public int M;
        /// <summary>
        /// 当前值
        /// </summary>
        public int V;

        public MVPair(int m, int v)
        {
            this.M = m;
            this.V = v;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey("M");
            writer.WriteInt(M);

            writer.WriteKey("V");
            writer.WriteInt(V);
        }

        /// <summary>
        /// 以最大值/当前值的方式写入数据
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="m"></param>
        /// <param name="v"></param>
        public static void WritePair(IExternalWriter writer, int m, int v)
        {
            writer.WriteByte(Amf3Type.Object);
            writer.WriteByte(0x0b); //动态对象方式.
            writer.WriteByte(0x01); 
            writer.WriteKey("M");
            writer.WriteInt(m);
            writer.WriteKey("V");
            writer.WriteInt(v);
            writer.WriteByte(0x01);
        }
    }
}