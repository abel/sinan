using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.AMF3;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// MountsSimple
    /// (简单的坐骑类)
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class MountsSimple : ExternalizableBase
    {
        Mounts m_mounts;

        /// <summary>
        /// 坐骑列表
        /// </summary>
        /// <param name="mounts"></param>
        public MountsSimple(Mounts mounts)
        {
            m_mounts = mounts;
        }


        protected override void WriteAmf3(IExternalWriter writer)
        {
            if (m_mounts == null)
                return;
            writer.WriteKey("ID");
            writer.WriteUTF(m_mounts.ID);

            writer.WriteKey("Experience");
            writer.WriteInt(m_mounts.Experience);

            writer.WriteKey("Level");
            writer.WriteInt(m_mounts.Level);

            writer.WriteKey("Status");
            writer.WriteInt(m_mounts.Status);

            writer.WriteKey("MountsID");
            writer.WriteUTF(m_mounts.MountsID);

            writer.WriteKey("PlayerID");
            writer.WriteUTF(m_mounts.PlayerID);

            writer.WriteKey("Rank");
            writer.WriteInt(m_mounts.Rank);

            if (m_mounts.Value == null)
                return;
            Variant skill = m_mounts.Value.GetVariantOrDefault("Skill");
            if (skill == null)
                return;

            writer.WriteKey("Skill");
            writer.WriteValue(skill);


            writer.WriteKey("FailCount");
            writer.WriteInt(m_mounts.FailCount);

            writer.WriteKey("FailTime");
            writer.WriteDateTime(m_mounts.FailTime);

            writer.WriteKey("ZhuFu");
            writer.WriteInt(m_mounts.ZhuFu);
        }
    }
}
