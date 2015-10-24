using System;
using System.Collections.Generic;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 物理攻击
    /// </summary>
    partial class FightBase
    {
        /// <summary>
        ///  普通物理攻击
        /// </summary>
        /// <param name="fighter"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool AttackWuLi(FightObject fighter)
        {
            //获取被攻击的对象..
            var targets = FilterTargets(fighter);
            if (targets.Count > 0)
            {
                AttackPhysical attack = new AttackPhysical(fighter, null);
                ActionResult result = Attack(attack, targets[0]);
                fighter.Action.Result = new List<ActionResult>() { result };
                fighter.Action.FightCount = FightAction.HasAction;
                m_actions.Add(fighter.Action);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 攻击时计算单个目标
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="target"></param>
        /// <param name="targets">所有被攻击的对象</param>
        /// <returns></returns>
        private ActionResult Attack(AttackDamage attack, FightObject target, List<FightObject> targets = null)
        {
            //查找保护者.保护者成为群攻目标时不能进行保护
            FightObject protecter;
            if (targets == null || targets.Count <= 1)
            {
                protecter = m_protecter.Find(x => x.IsLive && x.CanActive && x.Action.Target == target.ID);
            }
            else
            {
                protecter = m_protecter.Find(x => x.IsLive && x.CanActive && x.Action.Target == target.ID && (!targets.Contains(x)));
            }
            target.Protect = (protecter != null);
            ActionResult result = attack.Attack(target);
            if (protecter != null)
            {
                protecter.ProtectOther = true;
                ActionResult result2 = attack.Attack(protecter);
                result.Value["Protecter"] = result2;
            }
            return result;
        }

        ///// <summary>
        ///// 计算合击
        ///// </summary>
        ///// <param name="list"></param>
        //private static void createHeji(List<FightObject> list)
        //{
        //    for (int i = 1; i < list.Count; i++)
        //    {
        //        FightObject v1 = list[i - 1];
        //        FightObject v2 = list[i];
        //        if ((v1.TeamID == v2.TeamID) && (v1.Target == v2.Target)
        //            && (v1.TeamID != v1.Target.TeamID) && (Math.Abs(v1.Sudu - v2.Sudu) < 5))
        //        {
        //            if (random.Next(0, 100) < Math.Max(v1.FightProperty.HeJiLv, v2.FightProperty.HeJiLv) * 100)
        //            {
        //                //产生合击..
        //                v1.Action.ActionID = i;
        //                v2.Action.ActionID = i;
        //                i++;
        //                continue;
        //            }
        //        }
        //        v2.Action.ActionID = i;
        //    }
        //}

    }
}
