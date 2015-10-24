
namespace Sinan.AMF3
{
    /// <summary>
    ///  AMF3自定义序列化基类(支持静态对象)
    /// </summary>
    public abstract class ExternalizableBase : IExternalizable
    {
        void IExternalizable.WriteExternal(IExternalWriter writer)
        {
            writer.WriteByte(Amf3Type.Object);
            if (writer.WriteReference(this)) return;
            //动态对象方式.
            writer.WriteByte(0x0b);
            writer.WriteByte(0x01);
            this.WriteAmf3(writer);
            writer.WriteByte(0x01);
        }
        abstract protected void WriteAmf3(IExternalWriter writer);
    }
}
