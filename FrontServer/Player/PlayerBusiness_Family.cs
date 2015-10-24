using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Entity;
using Sinan.Util;
using Sinan.GameModule;

namespace Sinan.FrontServer
{
    partial class PlayerBusiness
    {
        /// <summary>
        /// 得到当天完成指定类型任务数量
        /// </summary>
        /// <param name="tasktype">
        /// 任务类型:
        /// 0主线，1支线,2日常,3社交,4环式,5行会,6家族循环任务,7家族贡献
        /// </param>
        /// <returns></returns>
        public int TaskTotal(int tasktype)
        {
            PlayerEx fx = m_taskday;
            if (fx == null)
                return 0;
            Variant fv = fx.Value;
            if (fv == null)
                return 0;
            DateTime dt = DateTime.Now;
            string day = dt.Date.ToString("yyyyMMdd");
            Variant info = fv.GetVariantOrDefault(day);
            if (info == null)
                return 0;
            return info.GetIntOrDefault(tasktype.ToString());
        }

        /// <summary>
        /// 本周任务完成情况
        /// </summary>
        /// <param name="tasktype">4</param>
        /// <returns></returns>
        public int WeekTotal(int tasktype)
        {
            int total = 0;
            PlayerEx fx = m_taskday;
            if (fx == null)
                return total;
            Variant fv = fx.Value;
            if (fv == null)
                return total;

            DateTime dt = DateTime.Now;

            //得到本周偏移量
            int week = (int)dt.DayOfWeek;
            if (week == 0)
            {
                week = 7;
            }
            week = week - 1;            
            for (int i = 0; i <= week; i++)
            {
                string day = dt.Date.AddDays(-i).ToString("yyyyMMdd");
                Variant info = fv.GetVariantOrDefault(day);
                if (info == null)
                    continue;
                total += info.GetIntOrDefault(tasktype.ToString());
            }
            return total;
        }

        /// <summary>
        /// 得到当前角色所在家族的贡献度
        /// </summary>
        /// <returns></returns>
        public int FamilyDev()
        {
            PlayerEx fx = m_family;
            if (fx == null)
                return 0;
            Variant fv = fx.Value;
            string familyid = fv.GetStringOrDefault("FamilyID");
            if (string.IsNullOrEmpty(familyid))
                return 0;
            return FamilyAccess.Instance.FamilyDev(familyid);
        }

        /// <summary>
        /// 发送家族信息
        /// </summary>
        /// <param name="comm">指令</param>
        /// <param name="familyname">家族名称</param>
        /// <param name="pars">参数</param>
        public void FamilyCall(string comm, string familyname, params object[] pars)
        {
            PlayerBusiness[] pbs = PlayersProxy.Players;
            var buffer = AmfCodec.Encode(comm, pars);
            foreach (PlayerBusiness ps in pbs)
            {
                if (ps.FamilyName == familyname)
                {
                    ps.Call(buffer);
                }
            }
        }
    }
}
