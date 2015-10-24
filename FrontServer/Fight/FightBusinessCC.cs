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
    /// 玩家切磋
    /// </summary>
    public class FightBusinessCC : FightBase
    {
        //扣经验点数
        protected double m_lost;

        public FightBusinessCC(FightObject[] teamA, FightObject[] teamB, List<PlayerBusiness> players)
            : base(teamA, teamB, players)
        {
            m_changeLife = false;
            m_maxRound = 200;
        }

        protected override void GameOver()
        {
            try
            {
                if (Interlocked.Exchange(ref m_fightState, 11) < 11)
                {
                    FightObject[] lostTeam;
                    FightObject[] winTeam;

                    if (m_fugitive != null)
                    {
                        lostTeam = m_fugitive.Team;
                        winTeam = lostTeam == m_teamA ? m_teamB : m_teamA;
                    }
                    else
                    {
                        if (m_teamB.All(x => x.Over))
                        {
                            winTeam = m_teamA;
                            lostTeam = m_teamB;
                        }
                        else
                        {
                            winTeam = m_teamB;
                            lostTeam = m_teamA;
                        }
                    }
                    FightEnd(winTeam, lostTeam);
                }
            }
            finally
            {
                base.Close();
            }
        }

        /// <summary>
        /// 处理结果(PK)
        /// </summary>
        /// <param name="winTeam">获胜的一方</param>
        /// <param name="lostTeam">失败的一方</param>
        protected virtual void FightEnd(FightObject[] winTeam, FightObject[] lostTeam)
        {
            SendToTeam(winTeam, FightCommand.FightEndR, (int)FightResult.Win, null, null);
            SendToTeam(lostTeam, FightCommand.FightEndR, (int)FightResult.Lose, null, null);

            if (m_changeLife)
            {
                List<string> names = new List<string>(winTeam.Length);
                foreach (FightPlayer f in winTeam)
                {
                    if (f.FType == FighterType.Player)
                    {
                        names.Add(f.Player.Name);
                        if (f.Online)
                        {
                            //记录PK胜利次数
                            f.Player.FinishNote(FinishCommand.PKWin, new object[] { 1 });
                        }
                    }
                }

                //记录死亡日志
                foreach (FightPlayer f in lostTeam)
                {
                    if (f.Online && f.FType == FighterType.Player)
                    {
                        LogVariant log = new LogVariant(ServerLogger.zoneid, Actiontype.PKLoss);
                        log.Value["Killer"] = names;           //对方(名字列表)
                        log.Value["Scene"] = m_scene.ID;       //场景ID
                        log.Value["SceneName"] = m_scene.Name; //场景名字
                        f.Player.WriteLog(log);
                    }
                }
            }
        }
    }
}
