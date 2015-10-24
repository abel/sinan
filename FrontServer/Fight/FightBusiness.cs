using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Log;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 跟暗雷战斗信息
    /// </summary>
    public partial class FightBusiness : FightBase
    {
        protected bool playerWin = false;

        /// <summary>
        /// 队伍平均等级
        /// </summary>
        int m_averagePlayerLev;
        int m_averagePetLev;
        int m_averageApcLev;

        int m_totalPetLev = 0;
        int m_totalPlayerLev = 0;
        int m_totalApcLev = 0;

        int m_petCount;
        int m_playerCount;
        bool m_isEctype;

        protected string m_npcID;


        public FightBusiness(FightObject[] teamA, FightObject[] teamB, List<PlayerBusiness> players, string npcID, bool isEctype = false)
            : base(teamA, teamB, players)
        {
            m_npcID = npcID;
            m_isEctype = isEctype;
            foreach (var v in teamA)
            {
                if (v.FType == FighterType.Player)
                {
                    m_playerCount++;
                    m_totalPlayerLev += v.Level;
                }
                else
                {
                    m_petCount++;
                    m_totalPetLev += v.Level;
                }
            }
            foreach (var v in teamB)
            {
                m_totalApcLev += v.Level;
            }
            m_averagePetLev = m_totalPetLev / Math.Max(1, m_petCount);
            m_averagePlayerLev = m_totalPlayerLev / m_playerCount;
            m_averageApcLev = m_totalApcLev / teamB.Length;
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
                        ProcessBos();
                    }
                    //发送奖励结果.
                    FightEnd(playerWin);

                    //没有打赢而且全部死亡,则转送回城
                    if ((!playerWin) && allOver)
                    {
                        DeadGoHome();
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

        protected void LogDead()
        {
            List<string> names = new List<string>(m_teamB.Length);

            foreach (FightObject f in m_teamB)
            {
                names.Add(f.Name);
            }
            //记录死亡日志
            foreach (FightPlayer f in m_teamA)
            {
                if (f.Online && f.FType == FighterType.Player)
                {
                    LogVariant log = new LogVariant(ServerLogger.zoneid, Actiontype.APCLoss);
                    log.Value["Killer"] = names;
                    log.Value["Scene"] = m_scene.ID;
                    log.Value["SceneName"] = m_scene.Name;
                    f.Player.WriteLog(log);
                }
            }
        }

        /// <summary>
        /// 死亡回城
        /// </summary>
        protected void DeadGoHome()
        {
            foreach (var p in m_players)
            {
                if (p == null) continue;
                if (p.Team == null || p.TeamJob == TeamJob.Captain)
                {
                    m_scene.TownGate(p, TransmitType.Dead);
                    return;
                }
            }
        }

        /// <summary>
        /// 处理副本
        /// </summary>
        protected void ProcessBos()
        {
            bool killboss = m_teamB.Any(x => x.FType == FighterType.Boss);
            //有杀Boss或者是在副本中..
            if (killboss || m_isEctype)
            {
                foreach (var p in m_players)
                {
                    if (p != null)
                    {
                        if (m_isEctype)
                        {
                            WriteEctypeKilled(p);
                            if (killboss)
                            {
                                p.AddAcivity(ActivityType.FuBen, 1);
                            }
                        }
                        if (killboss)
                        {
                            //杀Boss成就
                            p.FinishNote(FinishCommand.Boss, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 记录副本杀怪
        /// </summary>
        /// <param name="p"></param>
        protected void WriteEctypeKilled(PlayerBusiness p)
        {
            if (p.Team == null || p.TeamJob == TeamJob.Captain)
            {
                IList killed = p.Ectype.Value.GetValueOrDefault<IList>("Killed");
                if (!killed.Contains(m_npcID))
                {
                    killed.Add(m_npcID);
                    p.Ectype.Save();
                }
            }
        }

        /// <summary>
        /// 战斗结束,处理结果
        /// </summary>
        /// <param name="win">true:胜利</param>
        protected void FightEnd(bool win)
        {
            int result = win ? (int)FightResult.Win : (int)FightResult.Lose;
            foreach (var p in m_players)
            {
                if (p == null) continue;
                Variant award = null;
                if (win)
                {
                    if (m_allAwards != null)
                    {
                        m_allAwards.TryGetValue(p.ID, out award);
                    }
                    if (p.AwardGoods.Count > 0)
                    {
                        if (award == null)
                        {
                            award = new Variant(1);
                        }
                        award.Add("Goods", p.AwardGoods);
                    }
                }
                p.Call(FightCommand.FightEndR, result, award, m_npcID);
                p.AwardGoods.Clear();
            }
        }

        /// <summary>
        /// 通知任务系统
        /// </summary>
        protected void CheckPlayerTasks()
        {
            Dictionary<string, int> allApc = new Dictionary<string, int>();
            foreach (FightApc target in m_teamB)
            {
                FightPlayer killer = target.Killer as FightPlayer;
                if (killer != null)
                {
                    allApc.SetOrInc(target.APC.ID, 1);
                }
            }
            if (allApc.Count > 0)
            {
                foreach (var p in m_players)
                {
                    if (p == null) continue;
                    //通知任务系统
                    UserNote note2 = new UserNote(p, TaskCommand.FightingTask, new object[] { allApc });
                    Notifier.Instance.Publish(note2);
                }
            }
        }

    }
}
