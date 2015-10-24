using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 等级限制
    /// </summary>
    public class LimitLevel : ILimit
    {
        /// <summary>
        /// 提示
        /// </summary>
        readonly protected string m_maxLevMsg;
        readonly protected string m_minLevMsg;

        /// <summary>
        /// 进入所需最小等级
        /// </summary>
        readonly protected int m_minLev;

        /// <summary>
        /// 最大等级限制
        /// </summary>
        readonly protected int m_maxLev;


        public LimitLevel(string name, int minLev, int maxLev)
        {
            m_minLev = minLev;
            if (m_minLev > 0)
            {
                //进入或途经【{0}】需要{1}级，您的等级不足,可以通过活动、支线任务，日常任务，每日答题，经验副本，组队刷怪等方式快速提升等级。
                m_minLevMsg = string.Format(TipManager.GetMessage(ClientReturn.IntoLimit2), name, m_minLev);
            }
            m_maxLev = maxLev;
            if (m_maxLev > 0)
            {
                //进入【{0}】最大{1}级，您的等级超过限制
                m_maxLevMsg = string.Format(TipManager.GetMessage(ClientReturn.IntoLimit3), name, m_maxLev);
            }
        }

        public static LimitLevel Create(string name, Variant v)
        {
            int minLev = v.GetIntOrDefault("MinLevel");
            int maxLev = v.GetIntOrDefault("MaxLevel");
            if (minLev > 1 || maxLev > 0)
            {
                return new LimitLevel(name, minLev, maxLev);
            }
            return null;
        }

        public bool Check(PlayerBusiness player, out string msg)
        {
            if (m_minLev > player.Level)
            {
                msg = m_minLevMsg;
                return false;
            }
            if (m_maxLev > 0 && m_maxLev < player.Level)
            {
                msg = m_maxLevMsg;
                return false;
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
    }
}
