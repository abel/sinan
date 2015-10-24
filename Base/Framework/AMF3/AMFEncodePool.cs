using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Sinan.AMF3;
using Sinan.Log;

namespace Sinan.AMF3
{
    /// <summary>
    /// 解码器
    /// </summary>
    sealed public class AMFEncodePool : IAmf3Encode
    {
        Amf3Writer[] m_pools;

        //最大客户连接数
        int m_capacity;

        /// <summary>
        /// 当前分配位置
        /// </summary>
        int m_current = 0;


        public AMFEncodePool(int capacity, int bufferSize = 1024 * 64)
        {
            m_capacity = capacity;
            m_pools = new Amf3Writer[m_capacity];
            for (int i = 0; i < m_capacity; i++)
            {
                m_pools[i] = new Amf3Writer(bufferSize);
            }
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="command">方法名</param>
        /// <param name="objs">参数</param>
        /// <returns></returns>
        public Sinan.Collections.BytesSegment Encode(int command, IList objs)
        {
            int index = (Interlocked.Increment(ref m_current) & 0X0FFFFFFF) % m_capacity;
            Amf3Writer writer = m_pools[index];
            try
            {
                int offset = writer.Offset;
                //保留前4字节.前两个字节表示包的总长度,后两个字节表示方法名
                const int headLen = 4;
                writer.TrySkip(headLen);
                //用AMF编码写入objs.
                if (objs != null)
                {
                    for (int i = 0; i < objs.Count; i++)
                    {
                        writer.WriteObject(objs[i]);
                    }
                }

                //填充前4字节
                int len = writer.Count;
                byte[] bin = writer.Array;
                bin[0 + offset] = (byte)(len & 0xFF);
                bin[1 + offset] = (byte)(len >> 8);
                bin[2 + offset] = (byte)(command & 0xFF);
                bin[3 + offset] = (byte)(command >> 8);
                return new Sinan.Collections.BytesSegment(writer.Array, offset, len);
            }
            finally
            {
                writer.Reset();
            }
        }
    }
}