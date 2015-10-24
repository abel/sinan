using System;
using System.Collections.Generic;
using System.Linq;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 获取动作目标
    /// </summary>
    partial class FightBase
    {
        /// <summary>
        /// 过滤出所有可攻击的目标
        /// </summary>
        /// <param name="fighter"></param>
        /// <param name="opponent">是对方还是自方.true:对方,false:友方</param>
        /// <returns>所有可攻击的目标(主目标保存在列表最前)</returns>
        protected List<FightObject> FilterTargets(FightObject fighter, bool opponent = true)
        {
            //真真/假假为teamA,真假/假真为teamB
            List<FightObject> live = (fighter.Team == m_teamA ^ opponent) ?
                m_teamA.Where(x => x.IsLive).ToList() : m_teamB.Where(x => x.IsLive).ToList();

            if (live.Count == 0)
            {
                return live;
            }
            string targetID = fighter.Action.Target;
            int index = 0;
            if (live.Count > 1)
            {
                if (string.IsNullOrEmpty(targetID))
                {
                    index = NumberRandom.Next(live.Count);
                }
                else
                {
                    index = live.FindIndex(x => x.ID == targetID);
                    if (index < 0)
                    {
                        index = NumberRandom.Next(live.Count);
                    }
                }
            }
            //主目标移动到最前
            if (index > 0)
            {
                FightObject target = live[0];
                live[0] = live[index];
                live[index] = target;
            }
            fighter.Action.Target = live[0].ID;
            return live;
        }

        /// <summary>
        /// 生成所有敌方对象..
        /// </summary>
        /// <param name="fighter"></param>
        /// <param name="targetType"></param>
        /// <param name="addTargets"></param>
        /// <returns></returns>
        protected List<FightObject> EnemyTargets(FightObject fighter, TargetType targetType, AttackParameter par)
        {
            List<FightObject> targets = FilterTargets(fighter);
            if (targets.Count <= 1 || targetType == TargetType.All)
            {
                return targets;
            }
            //横向目标(先移除与主目标不在同一排的)
            if (targetType == TargetType.HorizontalAll || targetType == TargetType.HorizontalRandom)
            {
                //targets[0]处为主攻目标,位置编号从0开始,小于等于4的为第1列.大于4的为第2列.
                if (targets[0].P > 4)
                {
                    targets.RemoveAll(x => x.P <= 4);
                }
                else
                {
                    targets.RemoveAll(x => x.P > 4);
                }
            }
            //随机目标(随机移除多余目标)
            if (targetType == TargetType.Random || targetType == TargetType.HorizontalRandom)
            {
                //附加目标数量:-1时,表示所有活的;
                int addTargets = par.LevelConfig.GetIntOrDefault("AddTargets");
                if (addTargets++ > 0)
                {
                    while (targets.Count > addTargets)
                    {
                        int index = NumberRandom.Next(1, targets.Count);
                        targets.RemoveAt(index);
                    }
                }
            }
            return targets;
        }


        protected List<FightObject> FriendTargets(FightObject fighter, TargetType targetType, AttackParameter par)
        {
            List<FightObject> targets = FilterTargets(fighter, false);
            if (targets.Count <= 1 || targetType == TargetType.TeamAll)
            {
                return targets;
            }
            if (targetType == TargetType.TeamSingle)
            {
                while (targets.Count > 1)
                {
                    targets.RemoveAt(targets.Count - 1);
                }
            }
            //随机队友(随机移除多余目标)
            if (targetType == TargetType.TeamRandom)
            {
                //附加目标数量:-1时,表示所有活的;
                int addTargets = par.LevelConfig.GetIntOrDefault("AddTargets");
                if (addTargets++ > 0)
                {
                    while (targets.Count > addTargets)
                    {
                        int index = NumberRandom.Next(1, targets.Count);
                        targets.RemoveAt(index);
                    }
                }
            }
            return targets;
        }
    }
}
