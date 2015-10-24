using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 战斗信息
    /// </summary>
    partial class FightBase
    {
        /// <summary>
        /// 使用道具
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        protected virtual bool DaoJu(FightPlayer a)
        {
            if (a == null) return false;
            //格子..
            int p;
            if (string.IsNullOrEmpty(a.Action.Parameter) || (!Int32.TryParse(a.Action.Parameter, out p)))
            {
                return false;
            }
            PlayerEx package = a.Player.B0;
            string goodsid = (package.Value.GetValueOrDefault<IList>("C")[p] as Variant).GetStringOrDefault("E");
            if (string.IsNullOrEmpty(goodsid)) return false;
            GameConfig c = GameConfigAccess.Instance.FindOneById(goodsid);
            if (c == null) return false;

            //加血
            if (c.SubType == GoodsSub.Supply)
            {
                return DaoJuSupply(a, p, package, c);
            }
            //复活
            else if (c.SubType == GoodsSub.Revive)
            {
                return DaoJuRevive(a, p, package, c);
            }
            return false;
        }

        //复活
        private bool DaoJuRevive(FightPlayer a, int p, PlayerEx package, GameConfig c)
        {
            string target = a.Action.Target;
            FightPlayer t = m_teamA.FirstOrDefault(x => x.ID == target) as FightPlayer;
            if (t == null) t = m_teamB.FirstOrDefault(x => x.ID == target) as FightPlayer;
            if (t == null || t.IsLive) return false;
            //检查使用限制..
            //if (!SupplyLimit(note, c)) return false;
            if (a.Player.RemoveGoods(p, GoodsSource.DaoJuRevive))
            {
                ActionResult result = new ActionResult(target);
                //百分比复活生命
                int hp = (int)(t.Life.ShengMing * c.Value.GetDoubleOrDefault("A"));
                int mp = (int)(t.Life.MoFa * c.Value.GetDoubleOrDefault("B")) - t.MP;
                if (mp < 0) mp = 0;
                t.AddHPAndMP(hp, mp);
                if (hp > 0)
                {
                    result.Value["HP"] = hp;
                    result.Value["ShengMing"] = t.HP;
                }
                if (mp > 0)
                {
                    result.Value["MP"] = mp;
                    result.Value["MoFa"] = t.MP;
                }
                result.ActionFlag |= ActionFlag.Supply;
                a.Action.Result = new List<ActionResult>() { result };
                a.Action.FightCount = FightAction.HasAction;
                m_actions.Add(a.Action);
                return true;
            }
            return false;
        }

        //加血
        private bool DaoJuSupply(FightPlayer a, int p, PlayerEx package, GameConfig c)
        {
            string target = a.Action.Target;
            FightPlayer t = m_teamA.FirstOrDefault(x => x.ID == target) as FightPlayer;
            if (t == null) t = m_teamB.FirstOrDefault(x => x.ID == target) as FightPlayer;
            if (t == null || t.Over) return false;
            //检查使用限制..
            //if (!SupplyLimit(note, c)) return false;
            int hp = t.TryHP(c.Value.GetDoubleOrDefault("HP"));
            int mp = t.TryMP(c.Value.GetDoubleOrDefault("MP"));
            if (hp == 0 && mp == 0) return false;
            bool needHP = t.Life.ShengMing > t.HP;
            bool needMP = t.Life.MoFa > t.MP;
            if ((needHP && hp > 0) || (needMP && mp > 0))
            {
                if (a.Player.RemoveGoods(p, GoodsSource.DaoJuSupply))
                {
                    t.AddHPAndMP(hp, mp);
                    ActionResult result = new ActionResult(target);
                    if (hp > 0)
                    {
                        result.Value["HP"] = hp;
                        result.Value["ShengMing"] = t.HP;
                    }
                    if (mp > 0)
                    {
                        result.Value["MP"] = mp;
                        result.Value["MoFa"] = t.MP;
                    }
                    result.ActionFlag |= ActionFlag.Supply;
                    a.Action.Result = new List<ActionResult>() { result };
                    a.Action.FightCount = FightAction.HasAction;
                    m_actions.Add(a.Action);
                    return true;
                }
            }
            return false;
        }

    }
}
