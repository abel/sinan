using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 家族单人/(多人)副本
    /// </summary>
    public class FamilyInstanceBusiness : TeamInstanceBusiness
    {
        /// <summary>
        /// 所需家族当日贡献度(组队进)
        /// </summary>
        int m_dayDev;

        /// <summary>
        /// 需要完成的任务数(个人进)
        /// </summary>
        int m_familyTask;


        public FamilyInstanceBusiness(GameConfig gc, string difficulty)
            : base(gc, difficulty)
        {
            Variant v = gc.Value.GetVariantOrDefault("Limit");
            int dayDev = v.GetIntOrDefault("DayDev");
            int familyTask = v.GetIntOrDefault("FamilyTask");
            m_dayDev = dayDev;
            m_familyTask = familyTask;
        }

        protected override bool FillPlayers(PlayerTeam team, PlayerBusiness player, out string msg)
        {
            //通过完成家族任务获得此副本
            if (m_gc.SubType == "FamilyTask")
            {
                if (team != null)
                {
                    msg = TipManager.GetMessage(ClientReturn.IntoLimit1);
                    return false;
                }

                //TODO:检查家族任务完成度
                int total = player.TaskTotal(7);
                if (total < m_familyTask)
                {
                    msg = TipManager.GetMessage(FamilyReturn.FamilyTaskNotEnough); //"任务未完成";
                    return false;
                }

                m_members = new PlayerBusiness[1] { player };
                msg = null;
                return true;
            }
            //家族贡献达到指定值获得此副本
            else
            {
                if (team == null)
                {
                    msg = TipManager.GetMessage(ClientReturn.EctypeLimitTeamMsg1);
                    return false;
                }
                PlayerBusiness[] members = team.Members;
                PlayerBusiness caption = members[0];

                //检查是否是同一家族
                string fName = caption.FamilyName;
                if (string.IsNullOrEmpty(fName))
                {
                    msg = TipManager.GetMessage(ClientReturn.EctypeLimitFamilyMsg1);
                    return false;
                }

                //检查家族贡献值
                int dev = caption.FamilyDev();
                if (dev < m_dayDev)
                {
                    msg = TipManager.GetMessage(FamilyReturn.DayDevNotEnough); //"家族贡献度不足";
                    return false;
                }

                for (int i = 1; i < members.Length; i++)
                {
                    PlayerBusiness member = members[i];
                    if (member != null)
                    {
                        if (member.FamilyName != fName)
                        {
                            msg = TipManager.GetMessage(ClientReturn.EctypeLimitFamilyMsg3);
                            return false;
                        }
                    }
                }
                return base.FillPlayers(team, player, out msg);
            }
        }
    }
}
