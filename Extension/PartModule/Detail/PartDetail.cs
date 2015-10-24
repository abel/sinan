using Sinan.AMF3;
using Sinan.Entity;

namespace Sinan.PartModule.Detail
{
    public class PartDetail : ExternalizableBase
    {
        PartBase m_pb;
        public PartDetail(PartBase model)
        {
            m_pb = model;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey("ID");
            writer.WriteUTF(m_pb.ID);

            writer.WriteKey("PartID");
            writer.WriteUTF(m_pb.PartID);

            writer.WriteKey("PlayerID");
            writer.WriteUTF(m_pb.PlayerID);

            writer.WriteKey("SubType");
            writer.WriteUTF(m_pb.SubType);

            writer.WriteKey("Value");
            writer.WriteValue(m_pb.Value);
        }
    }
}
