using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Entity;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 物理攻击伤害计算类
    /// </summary>
    sealed public class AttackPhysical : AttackDamage
    {
        /// <summary>
        /// 物理类伤害计算类
        /// </summary>
        /// <param name="a">攻击者</param>
        /// <param name="Par"></param>
        /// <param name="heji">是否有合击(触发合击时100%命中目标且无法暴击,目标无法反击)</param>
        public AttackPhysical(FightObject a, AttackParameter par, bool heji = false)
            : base(a)
        {
            this.m_Par = par;
            this.m_heji = heji;
        }

        /// <summary>
        /// 计算物理攻击值
        /// </summary>
        /// <returns></returns>
        protected override int GetGongJi(FightObject target)
        {
            double beiShu = 1;
            SkillBuffer buff = m_a.FindBuffer(BufferType.ShenTuo);
            if (buff != null)
            {
                beiShu += buff.V;
            }
            //目标有牺牲
            buff = target.FindBuffer(BufferType.XiSheng);
            if (buff != null)
            {
                int d = Convert.ToInt32(buff.V);
                beiShu += (d % 1000) * 0.01;
                //beiShu += buff.V;
            }
            //目标有石壁
            buff = target.FindBuffer(BufferType.ShiBi);
            if (buff != null)
            {
                beiShu -= buff.V;
            }
            int gongji = (int)(m_a.Life.GongJi * beiShu);

            if (m_Par == null || m_Par.Level == 0)
            {
                return gongji + m_a.WG - target.WF;
            }
            else
            {
                return m_Par.GetGonJi(gongji) + m_a.WG - target.WF;
            }
        }

        /// <summary>
        /// 计算物理吸收比
        /// </summary>
        /// <returns></returns>
        protected override double GetXiShou(FightObject target)
        {
            double xishou = target.Life.WuLiXiShou;
            SkillBuffer o = target.FindBuffer(BufferType.ShiBi);
            if (o != null)
            {
                xishou += o.V;
            }
            o = target.FindBuffer(BufferType.XiSheng);
            if (o != null)
            {
                int d = Convert.ToInt32(o.V);
                xishou -= (d % 1000) * 0.01;
            }
            return Math.Max(0, Math.Min(0.8, xishou));
        }

        /// <summary>
        /// 技能攻击,不能反击
        /// </summary>
        /// <returns></returns>
        protected override bool CheckFanJi(FightObject target)
        {
            //Level大于0,为非普通攻击,不能反击.
            if (m_Par != null && m_Par.Level > 0) return false;
            return base.CheckFanJi(target);
        }
    }
}
