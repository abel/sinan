using System;
using System.Collections.Generic;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 战斗奖励
    /// </summary>
    partial class FightBusiness
    {
        Dictionary<string, Variant> m_allAwards;
        private void WriteAward(string playerID, string key, int count)
        {
            if (m_allAwards == null)
            {
                m_allAwards = new Dictionary<string, Variant>();
            }
            Variant award;
            if (!m_allAwards.TryGetValue(playerID, out award))
            {
                award = new Variant(3);
                m_allAwards.Add(playerID, award);
            }
            award.SetOrInc(key, count);
        }

        /// <summary>
        /// 分配奖励
        /// </summary>
        protected virtual void GiveAwards()
        {
            //获取所有奖励
            Dictionary<string, int> awards = new Dictionary<string, int>();
            foreach (FightApc target in m_teamB)
            {
                target.GetAwards(awards);
            }

            //分配角色经验
            int P1exp;
            if (awards.TryGetValue("P1exp", out P1exp))
            {
                awards.Remove("P1exp");
                AwardPlayersExp(P1exp);
            }

            //分配宠物经验
            int P2exp;
            if (awards.TryGetValue("P2exp", out P2exp))
            {
                awards.Remove("P2exp");
                AwardPetsExp(P2exp);
            }

            int bond = 0;
            if (awards.TryGetValue("Bond", out bond))
            {
                awards.Remove("Bond");
            }
            int score;
            if (awards.TryGetValue("Score", out score))
            {
                awards.Remove("Score");
            }
            //分配钱
            AwardPlayersMoney(bond, score);

            //必须最后分配物品
            AwardPlayersGoods(awards);
        }

        /// <summary>
        /// 分配角色经验
        /// </summary>
        /// <param name="awards"></param>
        private void AwardPlayersExp(int totalExp)
        {
            if (totalExp <= 0) return;
            //组队加成系数
            double teamP = (m_playerCount - 1) * 0.1;
            //收益递减系数
            double teamC = GetSYDJ(m_averagePlayerLev - m_averageApcLev);
            //角色的每1个等级可以获得的经验..
            double exp = ((teamP + teamC) * totalExp) / m_totalPlayerLev;

            foreach (FightPlayer fp in m_teamA)
            {
                if (fp.IsLive && fp.FType == FighterType.Player)
                {
                    PlayerBusiness player = fp.Player;
                    int p = 0;
                    if (!player.YellowName)
                    {
                        p = Convert.ToInt32(exp * player.Level * (player.GetDoubleExp()));
                    }

                    //会员战斗加成
                    Variant mv = MemberAccess.MemberInfo(fp.Player.MLevel);
                    if (mv != null)
                    {
                        p += Convert.ToInt32(p * mv.GetDoubleOrDefault("Fight"));
                    }

                    if (p < 10) p = 10;
                    p += fp.Player.UseClapnet * (fp.Level * 20 + 10);
                    player.AddExperience(p, FinanceType.KillApc);
                    WriteAward(player.ID, "PExp", p);
                }
            }
        }

        /// <summary>
        /// 分配宠物经验
        /// </summary>
        /// <param name="awards"></param>
        private void AwardPetsExp(int totalExp2)
        {
            if (totalExp2 <= 0) return;
            if (m_petCount <= 0) return;

            //组队加成系数
            double teamP = (m_petCount - 1) * 0.1;
            //收益递减系数
            double teamC = GetSYDJ(m_averagePetLev - m_averageApcLev);
            //宠物的每1个等级获得的经验..
            double exp = totalExp2 * (teamP + teamC) / (m_playerCount * m_averagePetLev);

            foreach (FightPlayer fp in m_teamA)
            {
                if (fp.IsLive && fp.FType == FighterType.Pet)
                {
                    PlayerBusiness player = fp.Player;
                    int p = 0;
                    if (!player.YellowName)
                    {
                        p = Convert.ToInt32(exp * fp.Level * (player.GetDoubleExp()));
                        p += player.UseClapnet * (fp.Level * 20 + 10);
                        if (p < 10) p = 10;


                        //会员战斗加成
                        Variant mv = MemberAccess.MemberInfo(fp.Player.MLevel);
                        if (mv != null)
                        {
                            p += Convert.ToInt32(p * mv.GetDoubleOrDefault("Fight"));
                        }

                        fp.Player.AddPetExp(fp.Player.Pet, p, true, (int)GoodsSource.FightEnd);

                        //战斗取得坐骑经验
                        fp.Player.AddMounts(p, GoodsSource.FightEnd);
                    }
                    WriteAward(fp.Player.ID, "RExp", p);
                }
            }
        }

        /// <summary>
        /// 分配钱
        /// </summary>
        /// <param name="bond">所有点券</param>
        /// <param name="score">所有石币</param>
        private void AwardPlayersMoney(int bond, int score)
        {
            //if (score <= 0 && bond <= 0) return;
            if (score <= 0 || m_playerCount <= 0) return;

            //组队加成系数
            double teamP = (m_playerCount - 1) * 0.1;
            //收益递减系数
            double teamC = GetSYDJ(m_averagePlayerLev - m_averageApcLev);
            //角色的每1个等级获得的份数
            double exp = (teamP + teamC) / (m_totalPlayerLev);

            foreach (var fp in m_teamA)
            {
                if (fp.IsLive && fp.FType == FighterType.Player)
                {
                    var player = (fp as FightPlayer).Player;
                    double p = exp * player.Level;
                    //int newbond = (int)(p * bond);
                    int newscore = (int)(p * score);
                    player.AddScore(newscore, FinanceType.KillApc);

                    //WriteAward(player.ID, "Bond", newbond);
                    WriteAward(player.ID, "Score", newscore);
                }
            }
        }

        /// <summary>
        /// 分配物品
        /// </summary>
        /// <param name="awards"></param>
        protected virtual void AwardPlayersGoods(Dictionary<string, int> awards)
        {
            foreach (var award in awards)
            {
                for (int i = 0; i < award.Value; i++)
                {
                    int index = NumberRandom.Next(m_teamA.Length);
                    if (index < m_teamA.Length)
                    {
                        FightPlayer fp = m_teamA[index] as FightPlayer;
                        if (fp != null && fp.IsLive)
                        {
                            PlayerBusiness player = fp.Player;
                            if (player.AddGoodsNobingOne(award.Key, GoodsSource.KillApc))
                            {
                                player.AwardGoods.SetOrInc(award.Key, 1);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 收益递减系统
        /// </summary>
        /// <param name="petc"></param>
        /// <returns></returns>
        private static double GetSYDJ(int petc)
        {
            return petc <= 5 ? 1 : petc >= 12 ? 0.1 : (1.3 - petc * 0.1);
        }
    }
}