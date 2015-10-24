using System;
using System.Collections.Generic;
using System.Linq;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;
using Sinan.AMF3;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 技能攻击
    /// </summary>
    partial class FightBase
    {
        /// <summary>
        /// 技能攻击
        /// </summary>
        /// <param name="fighter"></param>
        /// <param name="par"></param>
        /// <returns></returns>
        private bool AttackJiNeng(FightObject fighter, int level, GameConfig gc)
        {
            //伤害类型 物理0  魔法1 
            int injuryType = gc.Value.GetIntOrDefault("InjuryType");

            List<ActionResult> results = SkillAttack(fighter, level, gc, injuryType);
            if (results.Count > 0)
            {
                FightAction action = fighter.Action;
                action.Result = results;
                int fightCount = action.FightCount;
                action.FightCount = FightAction.HasAction;
                m_actions.Add(action);

                //蓄力,第二次攻击..
                SkillBuffer buff = fighter.FindBuffer(BufferType.XuLi);
                if (buff != null)
                {
                    action = action.CopyNew();
                    action.FightCount = fightCount;
                    fighter.Action = action;
                    List<ActionResult> results2 = SkillAttack(fighter, level, gc, injuryType);
                    if (results2.Count > 0)
                    {
                        action.Result = results2;
                        action.FightCount = FightAction.HasAction;
                        m_actions.Add(action);
                    }
                }
                return true;
            }
            return false;
        }

        private List<ActionResult> SkillAttack(FightObject fighter, int level, GameConfig gc, int injuryType)
        {
            List<ActionResult> results = new List<ActionResult>();
            AttackDamage attack;
            AttackParameter par = new AttackParameter(gc, level);
            if (injuryType == 0)
            {
                attack = new AttackPhysical(fighter, par);
            }
            else
            {
                attack = new AttackMagic(fighter, par);
            }

            TargetType targetType = (TargetType)gc.Value.GetIntOrDefault("TargetType");
            if (targetType == TargetType.Single)
            {
                var targets = FilterTargets(fighter);
                if (targets.Count > 0)
                {
                    ActionResult result = Attack(attack, targets[0]);
                    results.Add(result);
                }
            }
            else
            {
                var targets = EnemyTargets(fighter, targetType, par);
                foreach (var v in targets)
                {
                    ActionResult result2 = Attack(attack, v, targets);
                    results.Add(result2);
                }
            }
            return results;
        }

        /// <summary>
        /// 在攻击敌方时(冲锋/封尘/冻结)
        /// 直接产生Buffer的技能
        /// </summary>
        /// <param name="fighter"></param>
        private bool SkillCreateBuffer(FightObject fighter, int level, GameConfig gc)
        {
            if (fighter == null || gc == null || gc.Value == null)
            {
                return false;
            }
            string newbuffer = gc.Value.GetStringOrDefault("BufferID");
            GameConfig g = GameConfigAccess.Instance.FindOneById(newbuffer);
            if (g == null)
            {
                return false;
            }
            List<FightObject> targets;
            TargetType targetType = (TargetType)gc.Value.GetIntOrDefault("TargetType", 1);
            if (targetType == TargetType.Single)
            {
                targets = FilterTargets(fighter);
            }
            else
            {
                targets = EnemyTargets(fighter, targetType, new AttackParameter(gc, level));
            }

            //产生石化/混乱/冰冻
            fighter.Action.Value = new Variant();
            if (targets.Count == 0)
            {
                return false;
            }

            SkillBuffer buf = new SkillBuffer(g.Name, fighter.ID, level, 1, gc.Value);
            //修正次数.
            int round = buf.LevelConfig.GetIntOrDefault("Round", 1);
            buf.RemainingNumber = round;

            targets[0].AddBuffer(buf);
            ActionResult result = new ActionResult(targets[0].ID);
            result.ActionFlag |= ActionFlag.AddBuff;
            result.Value["AddBuffer"] = buf;
            fighter.Action.Result = new List<ActionResult> { result };

            if (targetType != TargetType.Single)
            {
                for (int i = 1; i < targets.Count; i++)
                {
                    SkillBuffer bufi = new SkillBuffer(g.Name, fighter.ID, level, 1, gc.Value);
                    bufi.RemainingNumber = round;

                    targets[i].AddBuffer(bufi);
                    ActionResult resulti = new ActionResult(targets[i].ID);
                    resulti.ActionFlag |= ActionFlag.AddBuff;
                    resulti.Value["AddBuffer"] = bufi;
                    fighter.Action.Result.Add(resulti);
                }
            }

            fighter.Action.FightCount = FightAction.HasAction;
            m_actions.Add(fighter.Action);
            return true;
        }

        ///// <summary>
        ///// 自身产生Buffer(开刃/沼气/焚烧)
        ///// </summary>
        ///// <param name="fighter"></param>
        ///// <param name="level"></param>
        ///// <param name="gc"></param>
        //private bool SkillAddBuffer(FightObject fighter, int level, GameConfig gc)
        //{
        //    SkillBuffer buf = new SkillBuffer(gc.Name, fighter.ID, level, Byte.MaxValue, gc.Value);
        //    fighter.AddBuffer(buf);
        //    fighter.Action.Value = new Variant();

        //    ActionResult result = new ActionResult(fighter.ID);
        //    result.ActionFlag |= ActionFlag.AddBuff;
        //    result.Value["AddBuffer"] = buf;

        //    fighter.Action.Result = new List<ActionResult>() { result };
        //    fighter.Action.FightCount = FightAction.HasAction;
        //    m_actions.Add(fighter.Action);
        //    return true;
        //}

        /// <summary>
        /// 治疗的技能
        /// 回春/母育/祭祀/神谕
        /// </summary>
        /// <param name="fighter"></param>
        /// <param name="level"></param>
        /// <param name="gc"></param>
        private bool SkillCure(FightObject fighter, int level, GameConfig gc)
        {
            TargetType targetType = (TargetType)gc.Value.GetIntOrDefault("TargetType");
            AttackParameter par = new AttackParameter(gc, level);
            var targets = FriendTargets(fighter, targetType, par);

            if (targets.Count == 0) return false;
            var config = par.LevelConfig;
            if (config == null) return false;
            double a = config.GetDoubleOrDefault("A");
            double b = config.GetDoubleOrDefault("B");
            double c = config.GetDoubleOrDefault("C");
            //double d = config.GetDoubleOrDefault("D");
            int r = config.GetIntOrDefault("Round");
            List<ActionResult> results = new List<ActionResult>();
            foreach (var target in targets)
            {
                ActionResult result = new ActionResult(target.ID);
                result.ActionFlag |= ActionFlag.Supply;
                int hp = target.TryHP(a) + (int)(fighter.Life.JingShen * b);
                target.AddHPAndMP(hp, 0);
                result.Value["HP"] = hp;
                //result.Value["ShengMing"] = target.HP;
                result.Value["ShengMing"] = new MVPair(target.Life.ShengMing, target.HP);
                //添加Buffer
                if (r > 0)
                {
                    SkillBuffer buf = new SkillBuffer(gc.Name, fighter.ID, level, r, gc.Value, c);
                    target.AddBuffer(buf);
                    result.ActionFlag |= ActionFlag.AddBuff;
                    result.Value["AddBuffer"] = buf;
                }
                results.Add(result);
            }
            fighter.Action.Result = results;
            fighter.Action.FightCount = FightAction.HasAction;
            m_actions.Add(fighter.Action);
            return true;
        }

        /// <summary>
        /// 复活的技能(回生)
        /// </summary>
        /// <param name="fighter"></param>
        /// <param name="level"></param>
        /// <param name="gc"></param>
        private bool SkillRevive(FightObject fighter, int level, GameConfig gc)
        {
            TargetType targetType = (TargetType)gc.Value.GetIntOrDefault("TargetType");
            List<FightObject> targets = null;
            if (targetType == TargetType.TeamAll)
            {
                targets = fighter.Team.Where(x => x.HP == 0).ToList();
            }
            else
            {
                FightObject t;
                if (fighter.FType == FighterType.Pet || fighter.FType == FighterType.Player)
                {
                    string targetID = fighter.Action.Target;
                    t = fighter.Team.FirstOrDefault(x => x.ID == targetID);
                }
                else
                {
                    t = fighter.Team.FirstOrDefault(x => x.HP == 0);
                }
                if (t != null && t.HP == 0)
                {
                    targets = new List<FightObject>(1);
                    targets.Add(t);
                }
            }

            if (targets == null || targets.Count == 0)
            {
                return false;
            }
            var results = new List<ActionResult>(targets.Count);
            foreach (FightObject target in targets)
            {
                var config = gc.Value.GetVariantOrDefault(level.ToString());
                if (config == null) return false;

                double a = config.GetDoubleOrDefault("A");
                double b = config.GetDoubleOrDefault("B");

                ActionResult result = new ActionResult(target.ID);
                result.ActionFlag |= ActionFlag.Supply;
                int hp = target.TryHP(a);
                int mp = target.TryMP(b);
                target.AddHPAndMP(hp, mp);
                result.Value["HP"] = hp;
                result.Value["MP"] = mp;
                result.Value["ShengMing"] = new MVPair(target.Life.ShengMing, target.HP);
                result.Value["MoFa"] = new MVPair(target.Life.MoFa, target.MP);
                results.Add(result);
            }
            fighter.Action.Result = results;
            fighter.Action.FightCount = FightAction.HasAction;
            m_actions.Add(fighter.Action);
            return true;
        }

        /// <summary>
        /// 增强防御的技能
        /// 石壁/神龙(主动)
        /// </summary>
        /// <param name="fighter"></param>
        /// <param name="level"></param>
        /// <param name="gc"></param>
        private bool SkillIncreaseDefense(FightObject fighter, int level, GameConfig gc)
        {
            TargetType targetType = (TargetType)gc.Value.GetIntOrDefault("TargetType");
            AttackParameter par = new AttackParameter(gc, level);
            var targets = FriendTargets(fighter, targetType, par);

            if (targets.Count == 0) return false;
            var config = par.LevelConfig;
            if (config == null) return false;
            double a = config.GetDoubleOrDefault("A");
            double b = config.GetDoubleOrDefault("B");
            if (a == 0 && b == 0) return false;
            int r = config.GetIntOrDefault("Round");
            if (r <= 0) return false;

            double c;
            if (gc.Name == BufferType.ShiBi)
            {
                c = a + ((fighter.Life.TiZhi / 50) * b);
            }
            else
            {
                c = a + ((fighter.Life.JingShen / 50) * b);
            }
            if (c <= 0) return false;

            List<ActionResult> results = new List<ActionResult>();
            foreach (var target in targets)
            {
                //添加Buffer
                ActionResult result = new ActionResult(target.ID);
                SkillBuffer buf = new SkillBuffer(gc.Name, fighter.ID, level, r, gc.Value, c);
                target.AddBuffer(buf);
                result.ActionFlag |= ActionFlag.AddBuff;
                result.Value["AddBuffer"] = buf;
                results.Add(result);
            }
            fighter.Action.Result = results;
            fighter.Action.FightCount = FightAction.HasAction;
            m_actions.Add(fighter.Action);
            return true;
        }

        /// <summary>
        /// 免死的技能(修身)
        /// </summary>
        /// <param name="fighter"></param>
        /// <param name="level"></param>
        /// <param name="gc"></param>
        private bool SkillNoDeath(FightObject fighter, int level, GameConfig gc)
        {
            TargetType targetType = (TargetType)gc.Value.GetIntOrDefault("TargetType");
            AttackParameter par = new AttackParameter(gc, level);
            var targets = FriendTargets(fighter, targetType, par);

            if (targets.Count == 0) return false;
            var config = par.LevelConfig;
            if (config == null) return false;
            double a = config.GetDoubleOrDefault("A");
            int r = config.GetIntOrDefault("Round");
            if (r <= 0) return false;
            List<ActionResult> results = new List<ActionResult>();
            foreach (var target in targets)
            {
                ActionResult result = new ActionResult(target.ID);
                SkillBuffer buf = new SkillBuffer(gc.Name, fighter.ID, level, r, gc.Value, a);
                target.AddBuffer(buf);
                result.ActionFlag |= ActionFlag.AddBuff;
                result.Value["AddBuffer"] = buf;
                results.Add(result);
            }
            fighter.Action.Result = results;
            fighter.Action.FightCount = FightAction.HasAction;
            m_actions.Add(fighter.Action);
            return true;
        }

        /// <summary>
        /// 增加攻击次数(蓄力)
        /// </summary>
        /// <param name="fighter"></param>
        /// <param name="level"></param>
        /// <param name="gc"></param>
        private bool SkillXuli(FightObject fighter, int level, GameConfig gc)
        {
            var config = gc.Value.GetVariantOrDefault(level.ToString());
            if (config == null) return false;
            int r = config.GetIntOrDefault("Round");

            SkillBuffer buf = new SkillBuffer(gc.Name, fighter.ID, level, r + 1, gc.Value);
            fighter.AddBuffer(buf);
            fighter.Action.Value = new Variant();

            ActionResult result = new ActionResult(fighter.ID);
            result.ActionFlag |= ActionFlag.AddBuff;
            result.Value["AddBuffer"] = buf;

            fighter.Action.Result = new List<ActionResult>() { result };
            fighter.Action.FightCount = FightAction.HasAction;
            m_actions.Add(fighter.Action);
            return true;
        }

        /// <summary>
        /// 增强目标魔法攻击(渗透)
        /// </summary>
        /// <param name="fighter"></param>
        /// <param name="level"></param>
        /// <param name="gc"></param>
        private bool SkillShenTou(FightObject fighter, int level, GameConfig gc)
        {
            var config = gc.Value.GetVariantOrDefault(level.ToString());
            if (config == null) return false;
            double a = config.GetDoubleOrDefault("A");
            int r = config.GetIntOrDefault("Round");

            SkillBuffer buf = new SkillBuffer(gc.Name, fighter.ID, level, r + 1, gc.Value, a);
            fighter.AddBuffer(buf);
            fighter.Action.Value = new Variant();

            ActionResult result = new ActionResult(fighter.ID);
            result.ActionFlag |= ActionFlag.AddBuff;
            result.Value["AddBuffer"] = buf;

            fighter.Action.Result = new List<ActionResult>() { result };
            fighter.Action.FightCount = FightAction.HasAction;
            m_actions.Add(fighter.Action);
            return true;
        }

        /// <summary>
        /// 牺牲
        /// </summary>
        /// <param name="fighter"></param>
        /// <param name="level"></param>
        /// <param name="gc"></param>
        private bool SkillXiSheng(FightObject fighter, int level, GameConfig gc)
        {
            var config = gc.Value.GetVariantOrDefault(level.ToString());
            if (config == null) return false;
            double a = config.GetDoubleOrDefault("A");
            int r = config.GetIntOrDefault("Round");

            SkillBuffer buf = new SkillBuffer(gc.Name, fighter.ID, level, r, gc.Value, a);
            fighter.AddBuffer(buf);
            fighter.Action.Value = new Variant();

            ActionResult result = new ActionResult(fighter.ID);
            result.ActionFlag |= ActionFlag.AddBuff;
            result.Value["AddBuffer"] = buf;

            fighter.Action.Result = new List<ActionResult>() { result };
            fighter.Action.FightCount = FightAction.HasAction;
            m_actions.Add(fighter.Action);
            return true;
        }
    }
}
