using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.Entity;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 魔法攻击伤害计算类
    /// </summary>
    sealed public class AttackMagic : AttackDamage
    {
        /// <summary>
        /// 基本的战斗计算
        /// </summary>
        /// <param name="a">攻击者</param>  
        /// <param name="Par"></param>
        /// <param name="heji">是否有合击(触发合击时100%命中目标且无法暴击,目标无法反击)</param>
        public AttackMagic(FightObject a, AttackParameter par, bool heji = false)
            : base(a)
        {
            this.m_Par = par;
            this.m_heji = heji;
        }


        /// <summary>
        /// 计算魔法攻击值
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
            buff = m_a.FindBuffer(BufferType.XiSheng);
            if (buff != null)
            {
                //如提升自己75%的魔法攻击，但会多受到30%的物理伤害
                //则保存为 75030(即前面的为提升..后面三位为多受的伤害)
                int d = Convert.ToInt32(buff.V);
                beiShu += (d / 1000) * 0.01;
                //beiShu += buff.V;
            }
            //目标有神龙.
            buff = target.FindBuffer(BufferType.ShengLong);
            if (buff != null)
            {
                beiShu -= buff.V;
            }
            return m_Par.GetGonJi(m_a.Life.MoFaGongJi * beiShu) + m_a.MG - target.MF;
        }

        /// <summary>
        /// 计算魔法吸收
        /// </summary>
        /// <returns></returns>
        protected override double GetXiShou(FightObject target)
        {
            double xishou = target.Life.MoFaXiShou;
            SkillBuffer o = target.FindBuffer(BufferType.ShengLong);
            if (o != null)
            {
                xishou += o.V;
            }
            return Math.Min(0.8, xishou);
        }

        /// <summary>
        /// 技能攻击,不能反击
        /// </summary>
        /// <returns></returns>
        protected override bool CheckFanJi(FightObject target)
        {
            return false;
        }
    }
}
