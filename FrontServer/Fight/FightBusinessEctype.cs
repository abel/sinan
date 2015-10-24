using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 组队副本打怪
    /// </summary>
    public class FightBusinessEctype : FightBusiness
    {
        protected EctypeApc m_apc;
        protected TeamInstanceBusiness m_tb;

        public FightBusinessEctype(FightObject[] teamA, FightObject[] teamB, List<PlayerBusiness> players,
            TeamInstanceBusiness tb, EctypeApc apc) :
            base(teamA, teamB, players, apc.Apc.ID, true)
        {
            tb.Astate = ActionState.Fight;
            m_tb = tb;
            m_apc = apc;
        }


        /// <summary>
        /// 处理副本
        /// </summary>
        protected void KillBos()
        {
            bool killbos = m_teamB.Any(x => x.FType == FighterType.Boss);
            if (killbos)
            {
                foreach (var p in m_players)
                {
                    if (p != null)
                    {
                        p.AddAcivity(ActivityType.FuBen, 1);
                        //杀Boss成就
                        p.FinishNote(FinishCommand.Boss, null);
                    }
                }
            }
        }

        protected override void GameOver()
        {
            try
            {
                if (Interlocked.Exchange(ref m_fightState, 11) < 11)
                {
                    //玩家是否胜利
                    bool allOver = m_teamA.All(x => x.Over);
                    if ((!m_taoPao) && m_teamB.All(x => (x.Over || x.FType == FighterType.BB)))
                    {
                        playerWin = true;
                        //需先取任务奖励
                        CheckPlayerTasks();
                        //再取打怪奖励
                        GiveAwards();
                        //记副本打怪
                        KillBos();
                    }
                    //发送奖励结果
                    FightEnd(playerWin);
                    if (playerWin)
                    {
                        m_apc.State = 2;
                        var buffer = AmfCodec.Encode(ClientCommand.KillApcR, new object[] { string.Empty, m_apc.ID });
                        foreach (var p in m_players)
                        {
                            if (p != null)
                            {
                                p.Call(buffer);
                            }
                        }
                        m_tb.Astate = ActionState.Standing;
                        m_tb.TryOver(true);
                        return;
                    }
                    else
                    {
                        //没有打赢则转送回城
                        base.Close();
                        m_tb.Over();
                        m_tb.Astate = ActionState.Standing;
                        m_apc.State = 0;
                    }
                    if (allOver)
                    {
                        LogDead();
                    }
                }
            }
            finally
            {
                base.Close();
            }
        }
    }
}
