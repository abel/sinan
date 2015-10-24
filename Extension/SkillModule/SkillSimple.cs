using System;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.AMF3;
using Sinan.Data;
using Sinan.Util;

namespace Sinan.SkillModule
{
    /// <summary>
    /// SkillSimple(用于技能列表)
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class SkillSimple : ExternalizableBase
    {
        GameConfig m_gc;

        /// <summary>
        /// 简单的技能
        /// </summary>
        /// <param name="gc"></param>
        /// </param>
        public SkillSimple(GameConfig gc)
        {
            this.m_gc = gc;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            Variant ui = m_gc.UI;
            if (ui != null)
            {
                int lev = ui.GetIntOrDefault("Level");
                writer.WriteKey("Level");
                writer.WriteInt(lev);
                for (int i = 0; i <= lev; i++)
                {
                    string k = i.ToString();
                    if (ui.ContainsKey(k))
                    {
                        writer.WriteKey(k);
                        writer.WriteIDictionary(ui.GetVariantOrDefault(k));
                    }
                }

                writer.WriteKey("HitTop");
                writer.WriteUTF(ui.GetStringOrDefault("HitTop"));
                writer.WriteKey("LaunchTop");
                writer.WriteUTF(ui.GetStringOrDefault("LaunchTop"));
                writer.WriteKey("RoleLimit");
                writer.WriteUTF(ui.GetStringOrDefault("RoleLimit"));

                writer.WriteKey("IsMove");
                writer.WriteInt(ui.GetIntOrDefault("IsMove"));
                writer.WriteKey("MaxUse");
                writer.WriteInt(ui.GetIntOrDefault("MaxUse"));
                writer.WriteKey("TargetType");
                writer.WriteInt(ui.GetIntOrDefault("TargetType"));
                writer.WriteKey("TypeFight");
                writer.WriteInt(ui.GetIntOrDefault("TypeFight"));
                writer.WriteKey("TypeNoFight");
                writer.WriteInt(ui.GetIntOrDefault("TypeNoFight"));

                writer.WriteKey("StudyLev");
                writer.WriteValue(ui.GetIntOrDefault("StudyLev"));

                if (ui.ContainsKey("BodyAction"))
                {
                    writer.WriteKey("BodyAction");
                    writer.WriteValue(ui.GetStringOrDefault("BodyAction"));
                }
            }

            writer.WriteKey("Name");
            writer.WriteUTF(m_gc.Name);
            writer.WriteKey("ID");
            writer.WriteUTF(m_gc.ID);
        }
    }
}
