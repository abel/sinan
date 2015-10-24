using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Log;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 玩家PK
    /// </summary>
    public class FightBusinessPK : FightBusinessCC
    {
        protected int m_protectTime = 300; //PK后的保护时间

        public FightBusinessPK(FightObject[] teamA, FightObject[] teamB, List<PlayerBusiness> players)
            : base(teamA, teamB, players)
        {
            m_changeLife = true;
            m_maxRound = 50;
            m_lost = 0.1;
        }

        protected override void FightEnd(FightObject[] winTeam, FightObject[] lostTeam)
        {
            base.FightEnd(winTeam, lostTeam);

            if (m_lost > 0)
            {
                foreach (FightPlayer f in lostTeam)
                {
                    if (f.Online && f.FType == FighterType.Player)
                    {
                        f.Player.LostExperience(m_lost, FinanceType.PKLost);
                        if (m_protectTime > 0 && winTeam == m_teamA)
                        {
                            f.Player.LastPK = DateTime.UtcNow.AddSeconds(m_protectTime);
                        }
                        //你被击败,损失了10%的角色经验
                        f.Player.Call(ClientCommand.SendActivtyR, "T05", TipManager.GetMessage(ClientReturn.FightBusinessPK1));
                    }
                }
            }

            //强制PK方取得胜利,添加红/黄名BUFF
            if (m_lost > 0 && winTeam == m_teamA)
            {
                int count = 0;
                foreach (FightPlayer f in lostTeam)
                {
                    if (f.FType == FighterType.Player && (!f.Player.RedName))
                    {
                        count++;
                    }
                }
                if (count > 0)
                {
                    foreach (FightPlayer f in winTeam)
                    {
                        if (f.Online && f.FType == FighterType.Player)
                        {
                            f.Player.SetPKKill(count);
                        }
                    }
                }
            }
        }

        protected override bool PlayerExit(PlayerBusiness player)
        {
            //扣除玩家经验.
            if (m_lost > 0)
            {
                player.LostExperience(m_lost, FinanceType.PKLost);
            }
            if (m_protectTime > 0)
            {
                player.LastPK = DateTime.UtcNow.AddSeconds(m_protectTime);
            }
            return base.PlayerExit(player);
        }


        protected override bool DaoJu(FightPlayer a)
        {
            return false;
        }
    }
}
