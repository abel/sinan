using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Entity
{
    /// <summary>
    /// 技能子类型
    /// </summary>
    public class SkillSub
    {
        /// <summary>
        ///  攻击类
        /// </summary>
        public const string Attack = "Attack";

        /// <summary>
        /// 属性加成类(跟服装相似)
        /// </summary>
        public const string Addition = "Addition";

        /// <summary>
        /// 家族属性加成类(跟服装相似)
        /// </summary>
        public const string AdditionJ = "AdditionJ";

        /// <summary>
        /// 在攻击敌方时,有机会产生Buffer的技能
        /// 开刃/沼气/焚烧 (主动)
        /// 森严/凝固/凝霜/附骨(被动)
        /// </summary>
        public const string AddBuffer = "AddBuffer";

        /// <summary>
        /// 在某种状态下加强攻击倍数(被动)
        /// 劈石/趁热  淡定/蔓延 崩裂/感染
        /// </summary>
        public const string AddAttack = "AddAttack";

        /// <summary>
        /// 宠物用.附加属性类.功能与准备类似
        /// </summary>
        public const string PetAddition = "PetAddition";

        /// <summary>
        /// 提升Buffer效果的技能(打磨/地脉/玄机)(被动)
        /// 能被提升的Buffer(开刃/沼气/焚烧)
        /// </summary>
        public const string IncreaseBuffer = "IncreaseBuffer";

        /// <summary>
        /// 治疗的技能
        /// 回春/母育/祭祀/神谕
        /// </summary>
        public const string Cure = "Cure";

        /// <summary>
        /// 回生(复活)
        /// </summary>
        public const string Revive = "Revive";

        /// <summary>
        /// 增强防御的技能
        /// 石壁/神龙(主动)
        /// </summary>
        public const string IncreaseDefense = "IncreaseDefense";

        /// <summary>
        /// 立即产生Buffer的技能
        /// 冲锋/封尘/冻结(主动)
        /// </summary>
        public const string CreateBuffer = "CreateBuffer";

        /// <summary>
        /// 免死的技能(修身)
        /// </summary>
        public const string NoDeath = "NoDeath";

        /// <summary>
        /// 蓄力
        /// </summary>
        public const string XuLi = "XuLi";

        /// <summary>
        /// 渗透
        /// </summary>
        public const string ShenTuo = "ShenTuo";

        /// <summary>
        /// 牺牲
        /// </summary>
        public const string XiSheng = "XiSheng";

    }
}
