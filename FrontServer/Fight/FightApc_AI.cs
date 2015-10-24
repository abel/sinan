using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.Log;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 战斗怪AI部分
    /// </summary>
    partial class FightApc : FightObject
    {
        /// <summary>
        /// 强力
        /// </summary>
        public override int WG
        {
            get { return 0; }
        }

        /// <summary>
        /// 魔能
        /// </summary>
        public override int MG
        {
            get { return 0; }
        }

        /// <summary>
        /// 坚韧
        /// </summary>
        public override int WF
        {
            get { return 0; }
        }

        /// <summary>
        /// 坚韧
        /// </summary>
        public override int MF
        {
            get { return 0; }
        }

        /// <summary>
        /// 创建说话
        /// </summary>
        /// <param name="talk"></param>
        /// <returns></returns>
        static int NewTalk(Variant talk)
        {
            int say = 0;
            if (talk != null)
            {
                double tp = talk.GetDoubleOrDefault("P");
                if (Sinan.Extensions.NumberRandom.RandomHit(tp))
                {
                    object s;
                    if (talk.TryGetValue("Say", out s))
                    {
                        if (s is int)
                        {
                            say = (int)s;
                        }
                        else
                        {
                            say = Convert.ToInt32(s);
                        }
                    }
                }
            }
            return say;
        }

        /// <summary>
        /// 创建招式(AI部分)
        /// </summary>
        /// <param name="myTeam"></param>
        /// <param name="?"></param>
        /// <param name="?"></param>
        /// <param name="fightCount"></param>
        /// <returns></returns>
        public override void CreateAction(FightObject[] targetTeam, int fightCount)
        {
            if (m_ai != null && m_ai.Count > 0)
            {
                try
                {
                    foreach (Variant ai in m_ai)
                    {
                        //出招概率
                        double p = ai.GetDoubleOrDefault("P");
                        if (Sinan.Extensions.NumberRandom.RandomHit(p))
                        {
                            //Target  1	自己  2	队友n  3	敌人n
                            int checkTarget = ai.GetIntOrDefault("CheckTarget");
                            int n = ai.GetIntOrDefault("AddTargets");

                            Variant condition = ai.GetVariantOrDefault("Condition");
                            if (checkTarget <= 1) //检查自己
                            {
                                if (checkCondition(this, condition, fightCount))
                                {
                                    //生成招式.
                                    CreateAction(targetTeam, ai, fightCount);
                                    return;
                                }
                            }
                            else if (checkTarget == 2)
                            {
                                int count = 0;
                                foreach (var f in Team)
                                {
                                    if (checkCondition(f, condition, fightCount))
                                    {
                                        count++;
                                    }
                                }
                                if (count >= n)
                                {
                                    //生成招式.
                                    CreateAction(targetTeam, ai, fightCount);
                                    return;
                                }
                            }
                            else if (checkTarget == 3)
                            {
                                int count = 0;
                                foreach (var f in targetTeam)
                                {
                                    if (checkCondition(f, condition, fightCount))
                                    {
                                        count++;
                                    }
                                }
                                if (count >= n)
                                {
                                    //生成招式.
                                    CreateAction(targetTeam, ai, fightCount);
                                    return;
                                }
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    LogWrapper.Warn("AI error:" + m_apc.ID, ex);
                }
            }
            base.CreateAction(targetTeam, fightCount);
        }

        private void CreateAction(FightObject[] targetTeam, Variant v, int fightCount)
        {
            int actionType = v.GetIntOrDefault("ActionType");
            FightAction action = new FightAction((ActionType)actionType, this.ID, 0);
            action.Parameter = v.GetStringOrDefault("Parameter");
            action.SkillLev = v.GetIntOrDefault("Level", 1);

            //Target  1	自己
            //Target  2	队友n
            //Target  3	敌人n
            //Target  4	随机友方(n)
            //Target  5	随机敌人(n)
            //Target  6	血量最少的敌人(n)
            int actionTarget = v.GetIntOrDefault("ActionTarget");
            if (actionTarget <= 1)
            {
                action.Target = this.ID;
            }
            else if (actionTarget == 4 || actionTarget == 2)
            {
                int index = Sinan.Extensions.NumberRandom.Next(Team.Length);
                action.Target = Team[index].ID;
            }
            else if (actionTarget == 5 || actionTarget == 3)
            {
                int index = Sinan.Extensions.NumberRandom.Next(targetTeam.Length);
                action.Target = targetTeam[index].ID;
            }
            else if (actionTarget == 6)
            {
                int min = Int32.MaxValue;
                foreach (var target in targetTeam)
                {
                    if (target.HP > 0 && target.HP < min)
                    {
                        action.Target = target.ID;
                    }
                }
            }
            else
            {
                action.Target = string.Empty;
            }
            action.Say = NewTalk(v.GetVariantOrDefault("Talk"));
            action.FightCount = fightCount;
            this.m_action = action;
        }

        /// <summary>
        /// 检查对象是否满足条件
        /// </summary>
        /// <param name="f"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        static bool checkCondition(FightObject f, Variant condition, int round)
        {
            Variant caseHp = condition.GetVariantOrDefault("CaseHP");
            Variant caseMp = condition.GetVariantOrDefault("CaseMP");
            IList caseBuff = condition.GetValueOrDefault<IList>("CaseBuff");
            IList caseRound = condition.GetValueOrDefault<IList>("CaseRound");
            if (caseHp != null)
            {
                double a = caseHp.GetDoubleOrDefault("A");
                double b = caseHp.GetDoubleOrDefault("B");
                if (a > 0 && a < 1) //百分比
                {
                    double test = (f.HP * 1.0) / f.Life.ShengMing;
                    if (test <= a || test > b)
                    {
                        return false;
                    }
                }
                else
                {
                    if (f.HP <= a || f.HP > b)
                    {
                        return false;
                    }
                }
            }

            if (caseMp != null)
            {
                double a = caseMp.GetDoubleOrDefault("A");
                double b = caseMp.GetDoubleOrDefault("B");
                if (a > 0 && a < 1) //百分比
                {
                    double test = (f.MP * 1.0) / f.Life.MoFa;
                    if (test <= a || test > b)
                    {
                        return false;
                    }
                }
                else
                {
                    if (f.MP <= a || f.MP > b)
                    {
                        return false;
                    }
                }
            }

            if (caseBuff != null)
            {
                bool find = false;
                foreach (var b in f.Buffers)
                {
                    if (caseBuff.Contains(b.ID))
                    {
                        find = true;
                        break;
                    }
                }
                if (!find) return false;
            }

            if (caseRound != null && caseRound.Count > 0)
            {
                return caseRound.Contains(round);
            }
            return true;
        }
    }
}
