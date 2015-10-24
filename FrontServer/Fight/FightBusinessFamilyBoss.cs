using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Util;
using Sinan.Log;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 家族Boss
    /// </summary>
    public class FightBusinessFamilyBoss : FightBusiness
    {
        string m_familyID;
        public FightBusinessFamilyBoss(FightObject[] teamA, FightObject[] teamB, List<PlayerBusiness> players, string npcID, string familyID) :
            base(teamA, teamB, players, npcID, false)
        {
            m_familyID = familyID;
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
                        //取打怪奖励
                        GiveAwards();
                        //记副本打怪
                        ProcessBos();
                    }
                    else
                    {
                        //还原状态...
                        FamilyBossAccess.Instance.ChangeState(m_familyID + m_npcID, 1, 0);
                    }
                    //发送奖励结果.
                    FightEnd(playerWin);
                    if (playerWin)
                    {
                        Variant playerIDs = CreateBossAward();
                        //保存奖励
                        FamilyBossAccess.Instance.Win(m_familyID + m_npcID, playerIDs);

                        PlayerBusiness leader = m_teamA[0].Player;
                        try
                        {
                            // 通知所有家族成员
                            // [{0}]家族领袖[{1}]带队击杀了家族Boss，全族成员都获得了一次抽奖机会，赶快打开家族按钮，去抽取奖励吧！
                            string msg = string.Format(TipManager.GetMessage(FamilyReturn.KillFamilyBoss), leader.FamilyName, leader.Name);
                            PlayersProxy.CallAll(ClientCommand.SendActivtyR, new object[] { "T02", msg });
                        }
                        catch { }
                        finally
                        {
                            //写日志
                            LogVariant log = new LogVariant(ServerLogger.zoneid, Actiontype.KillFamilyBoss);
                            var names = m_players.Where(x => x != null).Select(x => x.Name).ToList();
                            log.Value["Killer"] = names;
                            var p = GameConfigAccess.Instance.FindOneById(m_npcID);
                            log.Value["Boss"] = p.Name;
                            log.Value["Npc"] = m_npcID;
                            leader.WriteLog(log);
                        }
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

        private Variant CreateBossAward()
        {
            //取家族成员ID
            var f = FamilyAccess.Instance.FindOneById(m_familyID);
            var p = GameConfigAccess.Instance.FindOneById(m_npcID);
            int count = p.Value.GetIntOrDefault("SelCount", 6);
            Variant playerIDs = new Variant();
            IList listP = f.Value.GetValue<IList>("Persons");
            if (listP != null)
            {
                var award = p.Value.GetVariantOrDefault("Award");
                List<string> keys = award.Keys.ToList();
                List<int> values = award.Values.Cast<int>().ToList();

                foreach (var item in listP)
                {
                    Variant v = item as Variant;
                    if (v != null)
                    {
                        string pid = v.GetStringOrDefault("PlayerID");
                        //生成奖品
                        playerIDs.Add(pid, GetAward(keys, values, count));
                    }
                }
            }
            return playerIDs;
        }

        /// <summary>
        /// 生成奖励
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="values"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<string> GetAward(List<string> keys, List<int> values, int count)
        {
            List<string> r = new List<string>();
            int total = values.Sum();
            int find = Sinan.Extensions.NumberRandom.Next(total);
            total = 0;
            string goods = string.Empty;
            for (int i = 0; i < values.Count; i++)
            {
                total += values[i];
                if (find < total)
                {
                    r.Add(keys[i]);
                    break;
                }
            }
            int maxCount = Math.Min(count, keys.Count);
            while (r.Count < maxCount)
            {
                string g = keys[Sinan.Extensions.NumberRandom.Next(keys.Count)];
                if (!r.Contains(g))
                {
                    r.Add(g);
                }
            }
            return r;
        }
    }
}
