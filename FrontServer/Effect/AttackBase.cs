using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 基础攻击
    /// </summary>
    public abstract class AttackBase : IAttack
    {
        /// <summary>
        /// 随机命中
        /// </summary>
        /// <param name="m">命中率</param>
        /// <returns></returns>
        protected bool RandomHit(double m)
        {
            return NumberRandom.NextDouble() < m;
        }

        /// <summary>
        /// 基本的战斗计算
        /// </summary>
        /// <param name="a">攻击者</param>
        public AttackBase(FightObject a)
        {
            this.m_a = a;
        }

        /// <summary>
        /// 攻击者
        /// </summary>
        protected FightObject m_a;

        /// <summary>
        /// 攻击
        /// </summary>
        /// <returns></returns>
        public abstract ActionResult Attack(FightObject target);

        /// <summary>
        /// 检查暴击
        /// </summary>
        /// <param name="target">接受攻击者</param>
        /// <returns></returns>
        protected virtual bool CheckBaoJi(FightObject target)
        {
            return RandomHit(m_a.Life.BaoJiLv + 0.02 * (m_a.Level - target.Level));
        }

        /// <summary>
        /// 检查命中(合击时100%命中)
        /// <param name="target">接受攻击者</param>
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckMingZhong(FightObject target)
        {
            return RandomHit(0.9 + (m_a.Life.MingZhongLv - target.Life.ShanBiLv) +
                             0.01 * (m_a.Level - target.Level));
        }

        /// <summary>
        /// 检查防御
        /// </summary>
        /// <param name="target">接受攻击者</param>
        /// <returns></returns>
        protected virtual bool CheckFangYu(FightObject target)
        {
            if (target.ProtectOther) return false;
            return target.Action != null && target.Action.ActionType == ActionType.FangYu;
        }

        /// <summary>
        /// 处理成对的(技能+Buffer)对倍率的影响
        /// a的感染+b的流血
        /// a的蔓延+b的中毒
        /// a的趁热+b的灼烧
        /// a的劈石(碎石击/断岩击)+b的石化
        /// a的崩裂(石锥/钟乳)+b的冰冻
        /// a的淡定(冰刃箭/玄冰箭)+b的混乱
        /// </summary>
        /// <param name="target">接受攻击者</param>
        /// <returns></returns>
        protected virtual double BufferAddAttack(FightObject target)
        {
            if (m_a.FixBuffer == null || m_a.FixBuffer.Count == 0)
            {
                return 0;
            }
            double p = 0;
            foreach (var buffer in m_a.FixBuffer)
            {
                GameConfig gc = buffer.Value.Item2;
                if (gc.SubType != SkillSub.AddAttack) continue;
                int level = buffer.Value.Item1;
                //受影响的BufferID(灼烧/石化/中毒/混乱/流血/冰冻)
                string id = gc.Value.GetStringOrDefault("BufferID");
                if (target.ExistsBuffer(id))
                {
                    var d = gc.Value.GetVariantOrDefault(level.ToString());
                    if (d != null)
                    {
                        object bei;
                        //AllAttack:double.所有攻击都可提升的伤害倍数
                        if (d.TryGetValue("AllAttack", out bei))
                        {
                            p += Convert.ToDouble(bei);
                        }
                        else if ((m_a.Action.Parameter != null)
                            && d.TryGetValue(m_a.Action.Parameter, out bei))
                        {
                            p += Convert.ToDouble(bei);
                        }
                    }
                }
            }
            return p;
        }
    }
}
