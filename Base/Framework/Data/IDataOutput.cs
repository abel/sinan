using System;
using System.Collections.Generic;

namespace Sinan.Data
{
    public interface IDataOutput
    {
        void WriteDateTime(DateTime value);
        void WriteBoolean(bool value);
        void WriteByte(byte value);
        void WriteBytes(byte[] bytes, int offset, int length);
        void WriteDouble(double value);
        void WriteFloat(float value);
        void WriteInt(int value);
        void WriteNull();

        /// <summary>
        /// 将对象以AMF3序列化格式写入对象,
        /// 写入时会清理引用的缓存
        /// </summary>
        /// <param name="data"></param>
        void WriteObject(object data);


        void WriteUTF(string value);

        //void WriteU29(int value);
        //void WriteUndefined();
        //void WriteShort(short value);
        //void WriteUnsignedInt(uint value);
        //void WriteUTFBytes(string value);
    }
}
