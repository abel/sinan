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
    /// 个人秘境
    /// </summary>
    public class PersonalFam : TeamInstanceBusiness
    {
        public PersonalFam(GameConfig gc, string difficulty)
            : base(gc, difficulty)
        {
            Variant v = gc.Value.GetVariantOrDefault("Limit");
            int dayDev = v.GetIntOrDefault("DayDev");
            int familyTask = v.GetIntOrDefault("FamilyTask");
        }

        protected override bool FillPlayers(PlayerTeam team, PlayerBusiness player, out string msg)
        {
            //通过完成家族任务获得此副本
            if (team != null)
            {
                msg = TipManager.GetMessage(ClientReturn.IntoLimit1);
                return false;
            }
            m_members = new PlayerBusiness[1] { player };
            msg = null;
            return true;
        }
    }
}
