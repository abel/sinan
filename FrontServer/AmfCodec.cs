//#define FlowLog
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using Sinan.AMF3;
using Sinan.FastSocket;
using Sinan.Log;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 解码器
    /// </summary>
    public class AmfCodec
    {
        static readonly Sinan.Collections.BytesSegment emptyArray = new Sinan.Collections.BytesSegment(new byte[0], 0, 0);
        static readonly Tuple<int, List<object>>[] emptyTupleList = new Tuple<int, List<object>>[0];

        static readonly IAmf3Encode[] pools = new IAmf3Encode[256 * 256];

        static AmfStringZip m_zip;
        static AMFEncodePool m_bigEncoder;
        static AMFEncodePool m_defaultEncoder;

        static public void Init(int maxClient, AmfStringZip commandZip)
        {
            m_zip = commandZip;

            m_defaultEncoder = new AMFEncodePool(maxClient * 8, 1024 * 2, true);
            for (int i = 0; i < pools.Length; i++)
            {
                pools[i] = m_defaultEncoder;
            }
            m_bigEncoder = new AMFEncodePool(maxClient * 2, 256 * 256, false);
            //1302返回包袱
            pools[1302] = m_bigEncoder;
        }

        static public List<object> Decode(byte[] buffer, int offset, int count)
        {
            const int headLen = 4;
            if (count > headLen)
            {
                List<object> objs = new List<object>();
                Amf3Reader<Variant> reader = new Amf3Reader<Variant>(buffer, offset + headLen, count - headLen);
                while (reader.Unfinished)
                {
                    objs.Add(reader.ReadObject());
                }
                return objs;
            }
            return null;
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="name">方法名</param>
        /// <param name="objs">参数</param>
        /// <returns></returns>
        static public Sinan.Collections.BytesSegment Encode(string name, IList objs)
        {
            //查找命令
            int commandCode;
            if (!m_zip.ReadIndex(name, out commandCode))
            {
                LogWrapper.Warn("Unknown Command:" + name);
                return emptyArray;
            }
            try
            {
                return pools[commandCode].Encode(commandCode, objs);
            }
            catch (Exception err)
            {
                LogWrapper.Warn("Coding error:" + name, err);
                throw;
            }
        }

        /// <summary>
        /// 编码,使用大缓存
        /// </summary>
        /// <param name="name">方法名</param>
        /// <param name="objs">参数</param>
        /// <returns></returns>
        static public Sinan.Collections.BytesSegment EncodeBig(string name, IList objs)
        {
            int commandCode;  //查找命令
            if (!m_zip.ReadIndex(name, out commandCode))
            {
                LogWrapper.Warn("Unknown Command:" + name);
                return emptyArray;
            }
            try
            {
                return m_bigEncoder.Encode(commandCode, objs);
            }
            catch (Exception err)
            {
                LogWrapper.Warn("Coding error:", err);
                throw;
            }
        }

        #region 对参数加密
        /// <summary>
        /// 加密编码后的数组
        /// </summary>
        /// <param name="seed">加密种子</param>
        /// <param name="buffer">待发送的数组</param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static bool EncodeParams(int seed, byte[] buffer, int offset, int count)
        {
            int max = offset + 4;
            for (int i = offset; i < max; i++)
            {
                seed = (seed << 3) - seed + buffer[i];
            }

            max = offset + count;
            for (int i = offset + 4; i < max; i++)
            {
                byte old = buffer[i];
                byte newb = (byte)(old + seed);
                buffer[i] = newb;
                seed = (seed << 3) - seed + old;
            }
            return true;
        }

        /// <summary>
        /// 解密接收到的数据
        /// </summary>
        /// <param name="seed">加密种子</param>
        /// <param name="buffer">已加密的数据</param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static bool DecodeParams(int seed, byte[] buffer, int offset, int count)
        {
            int max = offset + 4;
            for (int i = offset; i < max; i++)
            {
                seed = (seed << 3) - seed + buffer[i];
            }
            max = offset + count;
            for (int i = offset + 4; i < max; i++)
            {
                byte b = buffer[i];
                byte newb = (byte)(b - seed);
                buffer[i] = newb;
                seed = (seed << 3) - seed + newb;
            }
            return true;
        }
        #endregion
    }
}
