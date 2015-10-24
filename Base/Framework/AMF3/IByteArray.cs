using System;

namespace Sinan.AMF3
{
    public interface IByteArray : IExternalWriter
    {
        uint Length { get; }
        int Position { get; set; }

        /// <summary>
        /// 清除字节数组的内容，并将 length 和 position 属性重置为 0。
        /// </summary>
        void Clear();

        /// <summary>
        /// 压缩字节数组
        /// </summary>
        void Compress();

        /// <summary>
        /// 解压缩字节数组
        /// </summary>
        void Uncompress();

        /// <summary>
        /// 使用deflate压缩算法压缩字节数组
        /// </summary>
        void Deflate();

        /// <summary>
        /// 使用deflate压缩算法将字节数组解压缩
        /// </summary>
        void Inflate();

        byte[] GetBuffer();


        //bool ReadBoolean();
        //byte ReadByte();
        //void ReadBytes(byte[] bytes, uint offset, uint length);
        //double ReadDouble();
        //float ReadFloat();
        //int ReadInt();
        //object ReadObject();
        //short ReadShort();
        //byte ReadUnsignedByte();
        //uint ReadUnsignedInt();
        //ushort ReadUnsignedShort();
        //string ReadUTF();
        //string ReadUTFBytes(uint length);

        //void WriteBoolean(bool value);
        //void WriteByte(byte value);
        //void WriteBytes(byte[] bytes, int offset, int length);
        //void WriteDouble(double value);
        //void WriteFloat(float value);
        //void WriteInt(int value);
        //void WriteObject(object value);
        //void WriteShort(short value);
        //void WriteUnsignedInt(uint value);
        //void WriteUTF(string value);
        //void WriteUTFBytes(string value);
    }
}
