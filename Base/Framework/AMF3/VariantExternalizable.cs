using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;

namespace Sinan.AMF3
{
    /// <summary>
    ///  AMF3自定义序列化基类(支持半动态,半静态对象)
    /// </summary>
    public class VariantExternalizable : VariantWapper, IExternalizable
    {
        void IExternalizable.WriteExternal(IExternalWriter writer)
        {
            writer.WriteByte(Amf3Type.Object);
            if (writer.WriteReference(this)) return;
            //动态对象方式
            writer.WriteByte(0x0b);
            writer.WriteByte(0x01);
            this.WriteAmf3(writer);
            writer.WriteByte(0x01);
        }

        /// <summary>
        /// 写动态部分..
        /// </summary>
        /// <param name="writer"></param>
        protected virtual void WriteAmf3(IExternalWriter writer)
        {
            if (m_value != null)
            {
                foreach (var kv in m_value)
                {
                    writer.WriteKey(kv.Key);
                    writer.WriteValue(kv.Value);
                }
            }
        }
    }
}
