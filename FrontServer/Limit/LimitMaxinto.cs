using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Util;
using Sinan.Extensions.FluentDate;
using Sinan.Entity;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 每日最多进入次数限制
    /// </summary>
    public class LimitMaxinto : ILimit
    {
        readonly protected string m_ectypeID;
        readonly protected string m_maxIntoMsg;

        /// <summary>
        /// 计时周期内每个用户最多进入次数
        /// </summary>
        readonly protected int m_maxInto;

        public LimitMaxinto(string ectypeID, string name, int maxInto)
        {
            m_ectypeID = ectypeID;
            m_maxInto = maxInto;
            if (m_maxInto > 0)
            {
                //很遗憾您无法进入，【{0}】每天最多可进入【{1}】次
                m_maxIntoMsg = string.Format(TipManager.GetMessage(ClientReturn.IntoLimit4), name, m_maxInto);
            }
        }

        public bool Check(PlayerBusiness player, out string msg)
        {
            //次数限制
            if (this.m_maxInto > 0)
            {
                int addC = 0;
                int c = player.ReadDaily(PlayerBusiness.DailyMap, m_ectypeID);

                var d = GameConfigAccess.Instance.FindOneById(m_ectypeID);
                if (d != null)
                {
                    if (d.MainType == MainType.Map ||
                        (d.MainType == MainType.Ectype && (d.SubType == "Personal" || d.SubType == "Team")))
                    {
                        Variant v = MemberAccess.MemberInfo(player.MLevel);
                        if (v != null)
                        {
                            addC = v.GetIntOrDefault("MapSecret");
                        }
                    }
                }
                if (c >= this.m_maxInto + addC)
                {
                    msg = m_maxIntoMsg;
                    return false;
                }
            }
            msg = null;
            return true;
        }

        public bool Execute(PlayerBusiness player, out string msg)
        {
            msg = null;
            return true;
        }

        public bool Rollback(PlayerBusiness player)
        {
            return true;
        }

        public static LimitMaxinto Create(string ectypeID, string name, Variant v)
        {
            int maxInto = v.GetIntOrDefault("MaxInto");
            if (maxInto > 0)
            {
                return new LimitMaxinto(ectypeID, name, maxInto);
            }
            return null;
        }
    }
}
