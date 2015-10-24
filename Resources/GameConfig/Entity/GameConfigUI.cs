using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.AMF3;
using Sinan.Data;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// 配置的UI部分
    /// </summary>
    [Serializable]
    //[BsonIgnoreExtraElementsAttribute]
    public class GameConfigUI : ExternalizableBase
    {
        GameConfig m_gc;

        /// <summary>
        /// 简单的技能
        /// </summary>
        /// <param name="gc"></param>
        /// </param>
        public GameConfigUI(GameConfig gc)
        {
            this.m_gc = gc;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            if (m_gc != null)
            {
                Variant ui = m_gc.UI;
                if (ui != null)
                {
                    foreach (var item in ui)
                    {
                        writer.WriteKey(item.Key);
                        writer.WriteValue(item.Value);
                    }
                }
                writer.WriteKey("Name");
                writer.WriteUTF(m_gc.Name);
                writer.WriteKey("ID");
                writer.WriteUTF(m_gc.ID);
            }
        }
    }

    /// <summary>
    /// 配置的UI部分
    /// </summary>
    [Serializable]
    //[BsonIgnoreExtraElementsAttribute]
    public class GameConfigValue : ExternalizableBase
    {
        GameConfig m_gc;

        /// <summary>
        /// 简单的技能
        /// </summary>
        /// <param name="gc"></param>
        /// </param>
        public GameConfigValue(GameConfig gc)
        {
            this.m_gc = gc;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            Variant v = m_gc.Value;
            if (v != null)
            {
                foreach (var item in v)
                {
                    writer.WriteKey(item.Key);
                    writer.WriteValue(item.Value);
                }
            }
            writer.WriteKey("Name");
            writer.WriteUTF(m_gc.Name);
            writer.WriteKey("ID");
            writer.WriteUTF(m_gc.ID);
        }
    }
}
