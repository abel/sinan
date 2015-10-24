using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.Extensions;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 攻击参数
    /// </summary>
    sealed public class AttackParameter
    {
        GameConfig m_gc;
        Variant m_gcl;
        public AttackParameter(GameConfig gc, int level)
        {
            m_gc = gc;
            this.Level = level;
            this.BufferID = gc.Value.GetStringOrDefault("BufferID");
            this.AddHitRate = gc.Value.GetDoubleOrDefault("AddHitRate");

            m_gcl = gc.Value.GetVariantOrDefault(level.ToString());
            this.A = m_gcl.GetDoubleOrDefault("A");
            this.B = m_gcl.GetDoubleOrDefault("B");
            this.C = m_gcl.GetDoubleOrDefault("C");
        }

        /// <summary>
        /// 等级
        /// </summary>
        public int Level
        { get; set; }

        /// <summary>
        /// 参数A
        /// </summary>
        public double A
        { get; set; }

        /// <summary>
        /// 参数B
        /// </summary>
        public double B
        { get; set; }

        /// <summary>
        /// 参数C
        /// </summary>
        public double C
        { get; set; }

        /// <summary>
        /// 能增强的Buffer
        /// </summary>
        public string BufferID
        { get; set; }

        /// <summary>
        /// 增加命中率
        /// </summary>
        public double AddHitRate
        { get; set; }

        /// <summary>
        /// 是否使用增强系数C
        /// </summary>
        public bool UseC
        { get; set; }

        public GameConfig Config
        {
            get { return m_gc; }
        }

        public Variant LevelConfig
        {
            get { return m_gcl; }
        }

        /// <summary>
        /// 计算攻击值
        /// </summary>
        /// <param name="gongji"></param>
        /// <returns></returns>
        public int GetGonJi(double gongji)
        {
            return (int)gongji + NumberRandom.Next((int)A, (UseC ? (int)C : (int)B) + 1);
            //return (int)(A + gongji * (UseC ? C : B));
        }

        /// <summary>
        /// 计算治疗值
        /// </summary>
        /// <param name="shengMing"></param>
        /// <param name="jingShen"></param>
        /// <returns></returns>
        public int GetCure(double shengMing, double jingShen)
        {
            return (int)(shengMing * A + jingShen * B);
        }
    }
}
