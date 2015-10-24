using System;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.FrontServer
{
    public class FightBusinessPro : FightBusinessPK
    {
        //战败有效守护凭证总量
        int m_total = 0;
        ProBusiness m_pro;

        /// <summary>
        /// 活动开始
        /// </summary>
        /// <param name="teamA"></param>
        /// <param name="teamB"></param>
        /// <param name="players"></param>
        public FightBusinessPro(FightObject[] teamA, FightObject[] teamB, List<PlayerBusiness> players)
            : base(teamA, teamB, players)
        {
            m_lost = 0;
            m_protectTime = 0;
            m_pro = PartProxy.TryGetPart(Part.Pro) as ProBusiness;
        }

        protected override bool PlayerExit(PlayerBusiness player)
        {
            //断开，移除所有道具
            int count = player.RemoveGoodsAll(m_pro.Elements[0], GoodsSource.ProExit);
            m_total += count;
            return base.PlayerExit(player);
        }

        /// <summary>
        /// 战斗结束
        /// </summary>
        /// <param name="winTeam">赢方</param>
        /// <param name="lostTeam">输方</param>
        protected override void FightEnd(FightObject[] winTeam, FightObject[] lostTeam)
        {
            #region 胜利方人数
            List<Variant> list = new List<Variant>();
            foreach (FightPlayer v in winTeam)
            {
                if (v.Online)
                {
                    if (v.FType == FighterType.Player)
                    {
                        v.Player.MoreFight++;
                        Variant tmp = new Variant();
                        tmp.Add("ID", v.Player.ID);
                        tmp.Add("Name", v.Player.Name);
                        tmp.Add("Number", v.Player.MoreFight);
                        list.Add(tmp);
                    }
                }
            }
            #endregion

            #region 失败方

            string goodsid = m_pro.Elements[0];
            int minTotal = 0;
            foreach (FightPlayer v in lostTeam)
            {
                if (v.FType == FighterType.Player)
                {
                    minTotal++;
                    if (v.Player.Fight == this)
                    {
                        int num = BurdenManager.GoodsCount(v.Player.B0, goodsid, m_pro.EndTime.AddHours(1));
                        if (num > 0)
                        {
                            m_total += num;
                            if (v.Player.RemoveGoods(goodsid, m_pro.EndTime.AddHours(1), num, GoodsSource.ProPK))
                            {
                                Variant inof = new Variant();
                                inof.Add("B0", v.Player.B0);
                                v.Player.Call(ActivityCommand.PKLoseR);
                            }
                        }
                        v.Player.NoFight = 0;//重置非战斗时间
                        //死亡回城
                        m_scene.TownGate(v.Player, TransmitType.Dead);
                    }
                }
            }
            #endregion
            if (m_total < minTotal)
            {
                m_total = minTotal;
            }
            #region 胜利方
            int count = list.Count;
            if (count > 0)
            {
                int y = 0;//余数
                int p = Math.DivRem(m_total, count, out y);//商
                int r = 0;//余数已经分配数

                if (p > 0 || y > 0)
                {
                    //守护战争,胜利方物品分配
                    foreach (FightPlayer v in winTeam)
                    {
                        if (v.Online)
                        {
                            if (v.FType == FighterType.Player)
                            {
                                ScenePro sp = v.Player.Scene as ScenePro;
                                Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
                                Variant m = new Variant();

                                if (p > 0)
                                {
                                    if (y > r)
                                    {
                                        m.Add("Number1", p + 1);
                                        r++;
                                    }
                                    else
                                    {
                                        m.Add("Number1", p);
                                    }
                                }
                                else
                                {
                                    if (y > r)
                                    {
                                        m.Add("Number1", 1);
                                        r++;
                                    }
                                    break;
                                }

                                m.Add("EndTime", sp.EndTime.AddHours(1));
                                if (m.Count == 2)
                                {
                                    dic.Add(goodsid, m);
                                    v.Player.AddGoods(dic, GoodsSource.FightEnd);
                                }
                            }
                        }
                    }
                }
                PlayersProxy.CallAll(ActivityCommand.FightProEndR, new object[] { list, m_total });
            }
            #endregion
            base.FightEnd(winTeam, lostTeam);
        }
    }
}
