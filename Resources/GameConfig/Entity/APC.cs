using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Util;
using Sinan.GameModule;

namespace Sinan.Entity
{
    public class Apc
    {
        Variant m_value;
        PlayerProperty m_life;

        /// <summary>
        /// 编号
        /// </summary>
        public string ID
        {
            get;
            private set;
        }

        /// <summary>
        /// 名字
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 等级
        /// </summary>
        public int Level
        {
            get;
            private set;
        }

        public string Skin
        {
            get;
            private set;
        }

        public FighterType ApcType
        {
            get;
            private set;
        }

        public Variant Value
        {
            get { return m_value; }
        }


        public PlayerProperty Life
        {
            get { return m_life; }
        }

        public Apc(Variant config)
        {
            ID = config.GetStringOrDefault("_id");
            this.Name = config.GetStringOrDefault("Name");
            Name = config.GetStringOrDefault("Name");
            m_value = config.GetVariantOrDefault("Value");
            m_life = new PlayerProperty();
            if (m_value != null)
            {
                m_life.Add(m_value);
                this.Level = m_value.GetIntOrDefault("Level");
            }
            this.ApcType = FighterType.APC;
            Variant ui = config.GetVariantOrDefault("UI");
            if (ui != null)
            {
                this.Skin = ui.GetStringOrDefault("Skin");
                string apcType = ui.GetStringOrDefault("Type");
                if (apcType == "BB")
                {
                    this.ApcType = FighterType.BB;
                }
                else if (apcType == "Boss")
                {
                    this.ApcType = FighterType.Boss;
                }
            }
        }
    }
}
