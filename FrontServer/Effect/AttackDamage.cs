using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Log;
using Sinan.Util;
using Sinan.AMF3;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 普通攻击/技能/道具/伤害基类
    /// </summary>
    public abstract class AttackDamage : AttackBase
    {
        public AttackDamage(FightObject a)
            : base(a) { }

        /// <summary>
        /// 是否合击
        /// </summary>
        protected bool m_heji;

        /// <summary>
        /// 参数
        /// </summary>
        protected AttackParameter m_Par;

        /// <summary>
        /// 攻击
        /// </summary>
        /// <param name="target">接受攻击者</param>
        /// <returns></returns>
        public override ActionResult Attack(FightObject target)
        {
            ActionResult result = new ActionResult(target.ID);
            bool meingZhong = CheckMingZhong(target);
            if (meingZhong)
            {
                if (m_Par != null)
                {
                    if (m_Par.Config.SubType == SkillSub.Attack)
                    {
                        m_Par.UseC = target.ExistsBuffer(m_Par.BufferID);
                    }
                    else
                    {
                        m_Par.UseC = false;
                    }
                }

                //计算倍率..
                double beiShu = CalculatedRate(target, result);
                //
                int gongJi = GetGongJi(target);
                double xiShou = GetXiShou(target);
                // 战斗公式
                int w = (int)(gongJi * (1 - xiShou) * beiShu);

                w -= target.Life.KangShangHai;

                // B是否进行了防御
                bool fangyu = CheckFangYu(target);
                if (fangyu)
                {
                    result.ActionFlag |= ActionFlag.FangYu;
                    //w = w * 6 / 10;
                    w = w >> 1; //50%
                }
                WriteShangHai(result, target, w);

                bool fanji = CheckFanJi(target);
                if (fanji)
                {
                    result.ActionFlag |= ActionFlag.FanJi;
                    result.Value["FanJi"] = FanAttack(target);
                }
                ChangeShengMing(result, m_a, target);

                // 添加附带的Buffer.
                TryAddBuffer(result, target);
                return result;
            }
            else
            {
                //闪避
                result.ActionFlag |= ActionFlag.ShanBi;
            }
            return result;
        }

        private void WriteShangHai(ActionResult result, FightObject target, int w)
        {
            w = Math.Max(1, w);
            target.HP -= w;
            result.Value["ShangHai"] = w;
        }

        /// <summary>
        /// 计算倍率..
        /// </summary>
        private double CalculatedRate(FightObject target, ActionResult result)
        {
            double beiShu = 1.0;
            // 是否保护别人
            if (target.ProtectOther)
            {
                target.ProtectOther = false;
                result.ActionFlag |= ActionFlag.ProtectOther;
                beiShu = 0.6;
            }
            // 是否受保护
            else if (target.Protect)
            {
                result.ActionFlag |= ActionFlag.Protect;
                beiShu = 0.4;
            }

            // 是否有合击
            if (m_heji)
            {
                result.ActionFlag |= ActionFlag.HeJi;
                beiShu *= m_a.Life.HeJiShangHai;
            }

            // 是否有暴击
            bool baoJi = CheckBaoJi(target);
            if (baoJi)
            {
                result.ActionFlag |= ActionFlag.BaoJi;
                beiShu *= m_a.Life.BaoJiShangHai;
            }
            //beiShu += BufferAddAttack(target);
            beiShu *= (1 + BufferAddAttack(target));
            return beiShu;
        }

        /// <summary>
        /// 检查命中(合击时100%命中)
        /// </summary>
        /// <param name="target">接受攻击者</param>
        /// <returns></returns>
        protected override bool CheckMingZhong(FightObject target)
        {
            if (m_heji) { return true; }
            double addM = 0;
            if (m_Par != null)
            {
                addM = m_Par.AddHitRate;
            }
            double m = 0.9 + (m_a.Life.MingZhongLv - target.Life.ShanBiLv)
                       + 0.01 * (m_a.Level - target.Level);
            m += addM;
            return RandomHit(Math.Max(0.1, m));
        }

        /// <summary>
        /// 检查暴击(合击时不产生暴击)
        /// </summary>
        /// <param name="target">接受攻击者</param>
        /// <returns></returns>
        protected override bool CheckBaoJi(FightObject target)
        {
            if (m_heji) { return false; }
            return base.CheckBaoJi(target);
        }

        /// <summary>
        /// 检查反击
        /// (合击/保护别人/已死亡时,不能产生反击)
        /// 混乱/冰冻/石化时不能反击
        /// </summary>
        /// <param name="target">接受攻击者</param>
        /// <returns></returns>
        protected virtual bool CheckFanJi(FightObject target)
        {
            if (m_heji || target.ProtectOther || target.HP <= 0)
            {
                return false;
            }
            if (target.ExistsBuffer(BufferType.HunLuan)
                || target.ExistsBuffer(BufferType.BingDong)
                || target.ExistsBuffer(BufferType.ShiHua))
            {
                return false;
            }
            return RandomHit(target.Life.FanJiLv);
        }

        /// <summary>
        /// 获取攻击值
        /// </summary>
        /// <param name="target">接受攻击者</param>
        /// <returns></returns>
        protected abstract int GetGongJi(FightObject target);

        /// <summary>
        /// 获取吸收百分比
        /// </summary>
        /// <param name="target">接受攻击者</param>
        /// <returns></returns>
        protected abstract double GetXiShou(FightObject target);

        /// <summary>
        /// 反击....
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual ActionResult FanAttack(FightObject target)
        {
            ActionResult result = new ActionResult(m_a.ID);
            double beiShu = 1;
            // 是否有暴击
            bool baoJi = RandomHit(target.Life.BaoJiLv + 0.02 * (target.Level - m_a.Level));
            if (baoJi)
            {
                beiShu *= target.Life.BaoJiShangHai;
                result.ActionFlag |= ActionFlag.BaoJi;
            }
            int w = (int)(target.Life.GongJi * (1 - m_a.Life.WuLiXiShou) * beiShu) - m_a.Life.KangShangHai;
            WriteShangHai(result, m_a, w);
            ChangeShengMing(result, target, m_a);
            return result;
        }

        /// <summary>
        /// 修改生命值.
        /// </summary>
        /// <param name="target">接受攻击者</param>
        /// <param name="w"></param>
        /// <returns></returns>
        protected int ChangeShengMing(ActionResult result, FightObject sender, FightObject target)
        {
            int xiuShen = 0;
            if (target.HP <= 0)
            {
                SkillBuffer b = target.FindBuffer(BufferType.XiuShen);
                if (b != null)
                {
                    xiuShen = Convert.ToInt32(b.V);
                    result.Value["XiuShen"] = xiuShen;
                    result.ActionFlag |= ActionFlag.XiuShen;
                    target.HP = xiuShen;
                }
                else
                {
                    target.HP = 0;
                    target.Buffers.Clear();
                    //target.Buffers.RemoveAll(x => x.CanRemove);
                    if (target is FightApc)
                    {
                        ((FightApc)target).Killer = sender;
                    }
                }
            }
            result.Value["ShengMing"] = new MVPair(target.Life.ShengMing, target.HP);
            return xiuShen;
        }

        /// <summary>
        /// 添加附带的Buffer
        /// (开刃/沼气/焚烧 (主动) 森严/凝固/凝霜/附骨(被动))
        /// 可能产生(流血/中毒/灼烧/混乱/石化//冰冻/附骨)
        /// </summary>
        /// <param name="target">接受攻击者</param>
        protected virtual void TryAddBuffer(ActionResult r, FightObject target)
        {
            if (target.HP <= 0) return;

            //// 开刃/沼气/焚烧 (主动) 添加(流血/中毒/灼烧)
            //foreach (var buffer in m_a.Buffers)
            //{
            //    if (buffer.Name == BufferType.KaiRen || buffer.Name == BufferType.ZhaoQi || buffer.Name == BufferType.FenShao)
            //    {
            //        if (TryAddBuffer(r, target, buffer.Level, buffer))
            //        {
            //            return;
            //        }
            //    }
            //}

            // TODO:开刃/沼气/焚烧 (主动改为被动) 添加(流血/中毒/灼烧)
            // 森严/凝固/凝霜/附骨(被动) 添加(混乱/石化/冰冻/附骨)
            if (m_a.FixBuffer != null)
            {
                foreach (var skill in m_a.FixBuffer)
                {
                    if (skill.Value.Item2.SubType != SkillSub.AddBuffer) continue;
                    if (TryAddBuffer(r, target, skill.Value.Item1, skill.Value.Item2))
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 开刃/沼气/焚烧 (主动)
        /// 添加(流血/中毒/灼烧)
        /// </summary>
        /// <param name="r"></param>
        /// <param name="target"></param>
        /// <param name="level"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private bool TryAddBuffer(ActionResult r, FightObject target, int level, SkillBuffer buffer)
        {
            Variant levelConfig = buffer.LevelConfig;
            if (levelConfig == null) return false;
            //根据概率计算出是否产生附加Buffer
            if (RandomHit(levelConfig.GetDoubleOrDefault("C")))
            {
                int senderSkillType = buffer.Config.GetIntOrDefault("SkillType");
                bool pass = CheckSkillType(senderSkillType);
                if (pass)
                {
                    //Round:int  Buffer可持续回合数
                    int round = levelConfig.GetIntOrDefault("Round");
                    //A:double  生成的Buffer的值
                    double v = levelConfig.GetDoubleOrDefault("A");
                    if (buffer.ID == BufferType.KaiRen)
                    {
                        IncreaseBuffer(BufferType.DaMo, ref round, ref v);
                    }
                    else if (buffer.ID == BufferType.ZhaoQi)
                    {
                        IncreaseBuffer(BufferType.DiMai, ref round, ref v);
                    }
                    else if (buffer.ID == BufferType.FenShao)
                    {
                        IncreaseBuffer(BufferType.XuanJi, ref round, ref v);
                    }

                    string newID = buffer.Config.GetStringOrDefault("BufferID");
                    GameConfig buffConfig = GameConfigAccess.Instance.FindOneById(newID);
                    SkillBuffer newBuff = new SkillBuffer(buffConfig.Name, m_a.ID, m_Par.Level, round, buffer.Config, v);
                    if (target.AddBuffer(newBuff))
                    {
                        r.Value["AddBuffer"] = newBuff;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 提升Buffer效果的技能
        /// 打磨/地脉/玄机(提升 开刃/沼气/焚烧)
        /// </summary>
        /// <param name="skillName"></param>
        /// <param name="round"></param>
        /// <param name="v"></param>
        private void IncreaseBuffer(string skillName, ref int round, ref double v)
        {
            Tuple<int, GameConfig> damo;
            if (m_a.FixBuffer.TryGetValue(skillName, out damo))
            {
                Variant config = damo.Item2.Value.GetVariantOrDefault(damo.Item1.ToString());
                round = Math.Max(round, config.GetIntOrDefault("Round"));
                double ta = config.GetDoubleOrDefault("A");
                double tb = config.GetDoubleOrDefault("B");
                v += (ta * m_a.Life.LiLiang + tb * m_a.Life.ZhiLi);
            }
        }

        /// <summary>
        // 森严/凝固/凝霜/附骨(被动)
        // 添加(混乱/石化/冰冻/附骨)
        /// </summary>
        /// <param name="r"></param>
        /// <param name="target"></param>
        /// <param name="level"></param>
        /// <param name="gc"></param>
        /// <returns></returns>
        private bool TryAddBuffer(ActionResult r, FightObject target, int level, GameConfig gc)
        {
            Variant levelConfig = gc.Value.GetVariantOrDefault(level.ToString());
            if (levelConfig == null) return false;
            //根据概率计算出是否产生附加Buffer
            if (RandomHit(levelConfig.GetDoubleOrDefault("C")))
            {
                int senderSkillType = gc.Value.GetIntOrDefault("SkillType");
                bool pass = CheckSkillType(senderSkillType);
                if (pass)
                {
                    string newbuffer = gc.Value.GetStringOrDefault("BufferID");
                    GameConfig buffConfig = GameConfigAccess.Instance.FindOneById(newbuffer);
                    //Round:int  Buffer可持续回合数
                    int round = levelConfig.GetIntOrDefault("Round");
                    //A:double  生成的Buffer的值
                    double v = levelConfig.GetDoubleOrDefault("A");
                    //每1点精神会增加的伤害(附骨)
                    double b = levelConfig.GetDoubleOrDefault("B");
                    if (b > 0)
                    {
                        v += (b * m_a.Life.JingShen);
                    }
                    SkillBuffer bf = new SkillBuffer(buffConfig.Name, m_a.ID, level, round, gc.Value, v);
                    if (target.AddBuffer(bf))
                    {
                        r.Value["AddBuffer"] = bf;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查类型技能类型
        /// 所有攻击0，普通攻击1,单体技能2,群体技能3，
        /// </summary>
        /// <param name="senderSkillType"></param>
        /// <returns></returns>
        private bool CheckSkillType(int senderSkillType)
        {
            bool pass = false;
            if (senderSkillType == 0)
            {
                pass = true;
            }
            else if (senderSkillType == 1)
            {
                if (m_Par == null || string.IsNullOrEmpty(m_a.Action.Parameter))
                {
                    pass = true;
                }
            }
            else if (m_Par != null)
            {
                //单个敌人1，全体敌人2, 随机敌人3，横向所有敌人4 ,横向随机敌人5
                int t = m_Par.Config.Value.GetIntOrDefault("TargetType");
                //对单体技能有效
                if (senderSkillType == 2 && t == 1)
                {
                    pass = true;
                }
                //对群体技能有效
                else if (senderSkillType == 3 && t != 1)
                {
                    pass = true;
                }
            }
            return pass;
        }

    }
}