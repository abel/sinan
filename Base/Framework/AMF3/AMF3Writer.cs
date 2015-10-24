//#define  ObjectRef
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using Sinan.Log;
using Sinan.Util;
using Sinan.FastJson;

namespace Sinan.AMF3
{
    /// <summary>
    /// AMF3编码类
    /// </summary>
    sealed public class Amf3Writer : IExternalWriter
    {
        const int defaultCount = 5;
        Dictionary<string, int> m_stringReferences = new Dictionary<string, int>(defaultCount);
        //Dictionary<ClassDefinition, int> m_classDefinitionReferences;
#if ObjectRef
        Dictionary<Object, int> m_objectReferences = new Dictionary<Object, int>(defaultCount);
#endif
        int m_index;
        /// <summary>
        /// 最大索引处(可取,不可达)
        /// </summary>
        readonly int m_maxIndex;
        readonly int m_offset;
        readonly byte[] m_bin;

        public int Offset
        {
            get { return m_offset; }
        }

        public byte[] Array
        {
            get { return m_bin; }
        }

        /// <summary>
        /// 已使用的数量
        /// </summary>
        public int Count
        {
            get { return m_index - m_offset; }
        }

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity
        {
            get { return m_maxIndex - m_offset; }
        }

        /// <summary>
        /// Initializes a new instance of the Amf3Writer class
        /// </summary>
        /// <param name="stream"></param>
        internal Amf3Writer(Sinan.Collections.BytesSegment segment)
        {
            m_bin = segment.Array;
            m_offset = segment.Offset;
            m_maxIndex = m_offset + segment.Count;
        }

        public Amf3Writer(int size)
        {
            m_bin = new byte[size];
            m_offset = 0;
            m_maxIndex = m_offset + size;
        }

        public void Reset()
        {
            m_index = m_offset;
            if (m_stringReferences.Count > defaultCount)
            {
                m_stringReferences = new Dictionary<string, int>(defaultCount);
            }
            else
            {
                m_stringReferences.Clear();
            }
#if ObjectRef
            if (m_objectReferences.Count > defaultCount)
            {
                m_objectReferences = new Dictionary<object, int>(defaultCount);
            }
            else
            {
                m_objectReferences.Clear();
            }
#endif
        }

        /// <summary>
        /// 跳过指定长度的字节
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool TrySkip(int length)
        {
            int index = m_index + length;
            if (index <= m_maxIndex)
            {
                m_index = index;
                return true;
            }
            return false;
        }

        #region IDataOutput的实现
        /// <summary>
        /// 写入布尔值
        /// </summary>
        /// <param name="value"></param>
        public void WriteBoolean(bool value)
        {
            WriteByte((byte)(value ? Amf3Type.BooleanTrue : Amf3Type.BooleanFalse));
        }

        /// <summary>
        /// 在字节流中写入一个字节
        /// </summary>
        /// <param name="value"></param>
        public void WriteByte(byte value)
        {
            m_bin[m_index++] = value;
        }

        /// <summary>
        /// 将指定字节数组写入字节流。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset">偏移量</param>
        /// <param name="length">长度</param>
        public void WriteBytes(byte[] bytes, int offset, int length)
        {
            if (bytes != null)
            {
                System.Buffer.BlockCopy(bytes, offset, m_bin, m_index, length);
                m_index += length;
            }
        }

        /// <summary>
        /// 在字节流中写入一个 IEEE 754 双精度（64 位）浮点数
        /// </summary>
        /// <param name="value"></param>
        public void WriteDouble(double value)
        {
            WriteByte(Amf3Type.Number);
            WriteBigEndian(value);
        }

        /// <summary>
        /// 在字节流中写入一个 IEEE 754 单精度（32 位）浮点数。
        /// </summary>
        /// <param name="value"></param>
        public void WriteFloat(float value)
        {
            WriteByte(Amf3Type.Number);
            WriteBigEndian(value);
        }

        /// <summary>
        /// 在字节流中写入一个带符号的 32 位整数。
        /// </summary>
        /// <param name="value"></param>
        public void WriteInt(int value)
        {
            //check valid range for 29bits
            if (value >= -268435456 && value <= 268435455)
            {
                WriteByte(Amf3Type.Integer);
                WriteU29(value);
            }
            else
            {
                WriteDouble((double)value);
            }
        }

        /// <summary>
        /// 将对象以AMF3序列化格式写入对象,
        /// 写入时会清理引用的缓存
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void WriteObject(object data)
        {
            m_stringReferences.Clear();
            WriteValue(data);
        }

        /// <summary>
        /// 将UTF-8 字符串(带标识写入)写入文件流、字节流或字节数组中
        /// </summary>
        /// <param name="value"></param>
        public void WriteUTF(string value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                WriteByte(Amf3Type.String);
                WriteUTFBytes(value);
            }
        }

        /// <summary>
        /// 写入空字符串,相当于: WriteUTFBytes(string.Empty) 或者 WriteU29(1)
        /// </summary>
        public void WriteEmptyString()
        {
            this.WriteByte(0x01); //同WriteU29(1);
        }

        /// <summary>
        /// 写入一个 UTF-8 字符串(不写对象标识位)
        /// </summary>
        /// <param name="value"></param>
        public void WriteUTFBytes(string value)
        {
            if (value == string.Empty)
            {
                WriteByte(0x01);
                return;
            }
            int handle;
            if (m_stringReferences.TryGetValue(value, out handle))
            {
                WriteU29(handle << 1);
                return;
            }
            m_stringReferences.Add(value, m_stringReferences.Count);
            WriteUtf8String(value);
        }

        public void WriteKey(string key)
        {
            WriteUTFBytes(key);
        }
        #endregion

        /// <summary>
        /// 写入未定义的对象
        /// </summary>
        public void WriteUndefined()
        {
            WriteByte(Amf3Type.Undefined);
        }

        /// <summary>
        /// 写入NULL对象
        /// </summary>
        public void WriteNull()
        {
            WriteByte(Amf3Type.Null);
        }

        public void WriteU29(int value)
        {
            //Clear 3 bits
            value &= 0x1fffffff;
            if (value < 0x80)
            {
                this.WriteByte((byte)value);
            }
            else if (value < 0x4000)
            {
                this.WriteByte((byte)(value >> 7 & 0x7f | 0x80));
                this.WriteByte((byte)(value & 0x7f));
            }
            else if (value < 0x200000)
            {
                this.WriteByte((byte)(value >> 14 & 0x7f | 0x80));
                this.WriteByte((byte)(value >> 7 & 0x7f | 0x80));
                this.WriteByte((byte)(value & 0x7f));
            }
            else
            {
                this.WriteByte((byte)(value >> 22 & 0x7f | 0x80));
                this.WriteByte((byte)(value >> 15 & 0x7f | 0x80));
                this.WriteByte((byte)(value >> 8 & 0x7f | 0x80));
                this.WriteByte((byte)(value & 0xff));
            }
        }

        /// <summary>
        /// 按对象引用写入.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteReference(object value)
        {
#if ObjectRef
            if (value != null)
            {
                int handle;
                if (m_objectReferences.TryGetValue(value, out handle))
                {
                    WriteU29(handle << 1);
                    return true;
                }
                m_objectReferences.Add(value, m_objectReferences.Count);
            }
#endif
            return false;
        }

        /// <summary>
        /// 写AMF3对象.不清空三个缓存.用于自定义方式写入对象
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool WriteValue(object data)
        {
            if (data == null || data is DBNull)
            {
                WriteNull();
                return true;
            }
            if (data is String)
            {
                WriteUTF((string)data);
                return true;
            }

            if (data is Int32 || data is Byte || data is SByte || data is Int16 || data is UInt16 || data is UInt32 || data is Char)
            {
                WriteInt((Int32)data);
                return true;
            }

            if (data is Int64 || data is UInt64 || data is Double || data is Single || data is Decimal)
            {
                WriteDouble(Convert.ToDouble(data));
                return true;
            }

            if (data is Boolean)
            {
                WriteBoolean((Boolean)data);
                return true;
            }

            if (data is DateTime)
            {
                WriteDateTime((DateTime)data);
                return true;
            }

            if (data is IExternalizable)
            {
                ((IExternalizable)data).WriteExternal(this);
                return true;
            }

            if (data is IDictionary<string, object>)
            {
                WriteIDictionary((IDictionary<string, object>)data);
                return true;
            }

            if (data is byte[])
            {
                WriteAmf3ByteArray((byte[])data);
                return true;
            }

            if (data is IDictionary<string, int>)
            {
                WriteIDictionary((IDictionary<string, int>)data);
                return true;
            }

            if (data is IList)
            {
                WriteAmf3Array((IList)data);
                return true;
            }

            if (data is IEnumerable)
            {
                WriteAmf3Array((IEnumerable)data);
                return true;
            }
            //Type type = data.GetType();
            //if (type.IsArray || type.GetInterface("System.Collections.IList") != null
            //    || type.GetInterface("System.Collections.Generic.IList`1") != null)
            //{
            //    WriteAmf3Array((IList)data);
            //    return true;
            //}

            //if (type.GetInterface("System.Collections.IEnumerable") != null)
            //{
            //    WriteAmf3Array((IEnumerable)data);
            //    return true;
            //}

            return WriteClass(data);
            //WriteUTF(data.ToString());
            //LogWrapper.Error("不支持的类型,编码错误:" + data.ToString());
            //return false;
        }

        private bool WriteClass(object value)
        {
            WriteByte(Amf3Type.Object);
            if (WriteReference(value))
            {
                return true;
            }
            //动态对象
            WriteByte(0x0b);
            WriteByte(0x01);
            Type t = value.GetType();
            List<Getters> g = JSON.Instance.GetGetters(t);
            foreach (var p in g)
            {
                WriteKey(p.Name);
                WriteValue(p.Getter(value));
            }
            WriteByte(0x01);
            return true;
        }

        /// <summary>
        /// 写对象
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteIDictionary(IDictionary<string, object> value)
        {
            if (value == null)
            {
                WriteNull();
                return true;
            }

            WriteByte(Amf3Type.Object);

            if (WriteReference(value))
            {
                return true;
            }

            WriteByte(0x0b); //动态对象
            //object typeName;
            //if (value.TryGetValue(Amf3Type.TypeName, out typeName))
            //{
            //    WriteUTFBytes(typeName.ToString());
            //}
            //else
            //{
            WriteByte(0x01);
            //}
            foreach (var kv in value)
            {
                WriteUTFBytes(kv.Key);
                WriteValue(kv.Value);
            }
            WriteByte(0x01);
            return true;
        }

        /// <summary>
        /// 写对象
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteIDictionary(IDictionary<string, int> value)
        {
            if (value == null)
            {
                WriteNull();
                return true;
            }

            WriteByte(Amf3Type.Object);
            if (WriteReference(value))
            {
                return true;
            }

            //动态对象
            WriteByte(0x0b);
            WriteByte(0x01);
            foreach (var kv in value)
            {
                WriteUTFBytes(kv.Key);
                WriteInt(kv.Value);
            }
            WriteByte(0x01);
            return true;
        }

        /// <summary>
        /// 写入时间
        /// </summary>
        /// <param name="value"></param>
        public void WriteDateTime(DateTime value)
        {
            WriteByte(Amf3Type.DateTime);
            if (WriteReference(value))
            {
                return;
            }
            WriteByte(0x01);
            double totalMilliseconds = (value.ToUniversalTime().Ticks - Amf3Type.UnixEpochTicks) * 0.0001d;
            WriteBigEndian(totalMilliseconds);
            //TimeSpan span = value.ToUniversalTime().Subtract(Amf3Type.UnixEpoch);
            //WriteBigEndian(span.TotalMilliseconds);
        }

        #region 私有方法
#if unsafe
        private unsafe void WriteBigEndian(float value)
        {
            int v = *((int*)&value);
            for (int i = 3; i >= 0; i--)
            {
                m_bin[m_index++] = (byte)(v >> (i << 3));
            }
        }

        private unsafe void WriteBigEndian(double value)
        {
            long v = *((long*)&value);
            for (int i = 7; i >= 0; i--)
            {
                m_bin[m_index++] = (byte)(v >> (i << 3));
            }
        }
#else
        private void WriteBigEndian(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                WriteByte(bytes[i]);
            }
        }

        private void WriteBigEndian(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                WriteByte(bytes[i]);
            }
        }
#endif

        private void WriteLength(int len)
        {
            //0-63         1字节
            //64-8191      2字节
            //8192-1048575 3字节
            //1048576-     4字节
            WriteU29((len << 1) | 1); //count*2+1
        }

        private void WriteUtf8String(string value)
        {
            //guess:估计长度*2+1(U29)占用的字节
            int strLen = value.Length;
            int guess = strLen < 0x20 ? 1 : strLen < 0x1000 ? 2 : strLen < 0x80000 ? 3 : 4;
            int len = Encoding.UTF8.GetBytes(value, 0, value.Length, m_bin, m_index + guess);

            //实际长度*2+1(U29)占用的字节
            //0-63         1字节
            //64-8191      2字节
            //8192-1048575 3字节
            //1048576---   4字节
            int actual = len < 0x40 ? 1 : len < 0x2000 ? 2 : len < 0x100000 ? 3 : 4;
            if (actual != guess)
            {
                Buffer.BlockCopy(m_bin, m_index + guess, m_bin, m_index + actual, len);
            }
            WriteLength(len);
            m_index += len;
        }

        private void WriteXmlDocument(XmlDocument value)
        {
            WriteByte(Amf3Type.Xml);
            string xml = string.Empty;
            if (value.DocumentElement != null && value.DocumentElement.OuterXml != null)
            {
                xml = value.DocumentElement.OuterXml;
            }
            if (xml == string.Empty)
            {
                WriteByte(0x01);
                return;
            }
            if (WriteReference(value))
            {
                return;
            }
            WriteUtf8String(xml);
        }


        private void WriteAmf3Array(IList value)
        {
            WriteByte(Amf3Type.Array);
            if (WriteReference(value))
            {
                return;
            }
            WriteLength(value.Count);
            WriteByte(0x01); //hash name
            for (int i = 0; i < value.Count; i++)
            {
                WriteValue(value[i]);
            }
        }

        private void WriteAmf3Array(IEnumerable value)
        {
            WriteByte(Amf3Type.Array);
            if (WriteReference(value))
            {
                return;
            }
            int count = 0;
            foreach (var v in value)
            {
                count++;
            }
            WriteLength(count);
            WriteByte(0x01);//hash name
            foreach (var v in value)
            {
                WriteValue(v);
            }
        }

        private void WriteAmf3AssociativeArray(IDictionary<string, object> value)
        {
            WriteByte(Amf3Type.Array);
            if (WriteReference(value))
            {
                return;
            }
            WriteByte(0x01);
            foreach (var entry in value)
            {
                WriteUTFBytes(entry.Key);
                WriteValue(entry.Value);
            }
            WriteByte(0x01);
        }

        private void WriteAmf3ByteArray(byte[] value)
        {
            WriteByte(Amf3Type.ByteArray);
            if (WriteReference(value))
            {
                return;
            }
            int len = value.Length;
            WriteLength(len);
            System.Buffer.BlockCopy(value, 0, m_bin, m_index, len);
            m_index += len;
        }
        #endregion

    }
}
