using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.AMF3;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 行动结果..
    /// </summary>
    public class ActionResult : VariantExternalizable
    {
        /// <summary>
        /// 接受攻击者ID
        /// </summary>
        public string Target
        {
            get;
            set;
        }

        /// <summary>
        /// 行动标识
        /// </summary>
        public ActionFlag ActionFlag
        {
            get;
            set;
        }

        public ActionResult(string target)
        {
            this.Target = target;
            m_value = new Variant();
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey("Target");
            writer.WriteUTF(Target);

            writer.WriteKey("ActionFlag");
            writer.WriteInt((int)(this.ActionFlag));

            base.WriteAmf3(writer);
        }
    }

    /// <summary>
    /// 战斗标记
    /// </summary>
    [Flags]
    public enum ActionFlag
    {
        /// <summary>
        /// 是否闪避
        /// </summary>
        ShanBi = 1,

        /// <summary>
        /// 是否合击
        /// </summary>
        HeJi = 2,

        /// <summary>
        /// 是否暴击
        /// </summary>
        BaoJi = 4,

        /// <summary>
        /// 目标是否进行了防御
        /// </summary>
        FangYu = 8,

        /// <summary>
        /// 是否受保护
        /// </summary>
        Protect = 16,

        /// <summary>
        /// 是否保护别人
        /// </summary>
        ProtectOther = 32,

        /// <summary>
        /// 是否进行了反击
        /// </summary>
        FanJi = 64,

        /// <summary>
        /// 是否有修身
        /// </summary>
        XiuShen = 128,

        /// <summary>
        /// 补充HP或MP
        /// </summary>
        Supply = 256,

        /// <summary>
        /// 添加Buff
        /// </summary>
        AddBuff = 512,
    }
}
