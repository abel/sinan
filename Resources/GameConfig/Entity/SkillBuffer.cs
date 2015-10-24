using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.AMF3;
using Sinan.Util;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Data;

namespace Sinan.Entity
{
    /// <summary>
    /// SkillBuffer
    /// </summary>
    public class SkillBuffer : ExternalizableBase, IComparer<SkillBuffer>
    {
        Variant m_gc;
        Variant m_lgc;

        /// <summary>
        /// BufferID
        /// </summary>
        public string ID
        {
            get;
            set;
        }

        /// <summary>
        /// 发出Buffer的玩家ID
        /// </summary>
        public string SenderID
        {
            get;
            set;
        }

        /// <summary>
        /// Buffer等级
        /// </summary>
        public int Level
        {
            get;
            set;
        }

        /// <summary>
        /// 剩余有效次数.
        /// </summary>
        public int RemainingNumber
        {
            get;
            set;
        }

        /// <summary>
        /// 用于计算的值
        /// </summary>
        public double V
        {
            get;
            set;
        }

        /// <summary>
        /// 效果类型.
        /// </summary>
        public string EffectType
        {
            get;
            set;
        }

        /// <summary>
        /// 等级配置
        /// </summary>
        public Variant LevelConfig
        {
            get { return m_lgc; }
        }

        /// <summary>
        /// 配置.....
        /// </summary>
        public Variant Config
        {
            get { return m_gc; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">发出者ID</param>
        /// <param name="level">发出者等级</param>
        /// <param name="number">可持续回合数</param>
        /// <param name="config">配置</param>
        public SkillBuffer(string name, string sender, int level, int number, Variant config, double v = 0)
        {
            this.ID = name.Replace(SkillHelper.PetSkillSuffix, string.Empty);
            this.SenderID = sender;
            this.Level = level;
            this.RemainingNumber = number;
            this.m_gc = config;
            this.m_lgc = config.GetVariantOrDefault(level.ToString());
            V = v;
        }

        public int Compare(SkillBuffer x, SkillBuffer y)
        {
            if (x.Level != y.Level)
            {
                return x.Level - y.Level;
            }
            if (x.RemainingNumber != y.RemainingNumber)
            {
                return x.RemainingNumber - y.RemainingNumber;
            }
            return (int)(x.V - y.V);
        }

        static public bool operator >(SkillBuffer x, SkillBuffer y)
        {
            return x.Level > y.Level || x.RemainingNumber > y.RemainingNumber || x.V > y.V;
        }

        static public bool operator <(SkillBuffer x, SkillBuffer y)
        {
            return x.Level < y.Level || x.RemainingNumber < y.RemainingNumber || x.V < y.V;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            var ui = GameConfigAccess.Instance.FindBuffer(this.ID);
            writer.WriteKey("UI");
            if (ui != null)
            {
                writer.WriteIDictionary(ui.UI);
            }
            else
            {
                writer.WriteNull();
                LogWrapper.Warn("Buffer err:" + this.ID);
            }

            writer.WriteKey("Remaining");
            writer.WriteInt(RemainingNumber);

            writer.WriteKey("ID");
            writer.WriteUTF(ID);
        }

    }
}
