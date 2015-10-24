using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 家族限制
    /// </summary>
    public class LimitFamily //: ILimit
    {
        /// <summary>
        /// 所需家族当日贡献度(组队进)
        /// </summary>
        int m_dayDev;

        /// <summary>
        /// 需要完成的任务数(个人进)
        /// </summary>
        int m_familyTask;

        readonly string m_familyMsg1;
        readonly string m_familyMsg2;
        readonly string m_familyMsg3;


        public LimitFamily(string name, int dayDev, int familyTask)
        {
            m_dayDev = dayDev;
            m_familyTask = familyTask;
            m_familyMsg1 = TipManager.GetMessage(ClientReturn.EctypeLimitFamilyMsg1);
            if (m_dayDev > 0)
            {
                m_familyMsg2 = String.Format(TipManager.GetMessage(ClientReturn.EctypeLimitFamilyMsg2), m_dayDev);
            }
            if (m_familyTask > 0)
            {
                m_familyMsg3 = TipManager.GetMessage(ClientReturn.EctypeLimitFamilyMsg3);
            }
        }


        public bool Check(PlayerBusiness player, out string msg)
        {
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

        public static LimitFamily Create(string name, Variant v)
        {
            int dayDev = v.GetIntOrDefault("DayDev");
            int familyTask = v.GetIntOrDefault("FamilyTask");
            if (familyTask > 0 || dayDev > 0)
            {
                return new LimitFamily(name, dayDev, familyTask);
            }
            return null;
        }
    }
}
