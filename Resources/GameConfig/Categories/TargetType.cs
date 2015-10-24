using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Entity
{
    /// <summary>
    /// 技能攻击目标类型.
    /// </summary>
    public enum TargetType
    {
        /// <summary>
        /// 无需选择
        /// </summary>
        None = 0,

        /// <summary>
        /// 单个敌人
        /// </summary>
        Single = 1,

        /// <summary>
        /// 全体敌人
        /// </summary>
        All = 2,

        /// <summary>
        /// 随机敌人
        /// </summary>
        Random = 3,

        /// <summary>
        /// 横向所有敌人
        /// </summary>
        HorizontalAll = 4,

        /// <summary>
        /// 横向随机敌人
        /// </summary>
        HorizontalRandom = 5,

        /// <summary>
        /// 单个队友(包含自己)
        /// </summary>
        TeamSingle = 6,

        /// <summary>
        /// 全体队友
        /// </summary>
        TeamAll = 7,

        /// <summary>
        /// 随机队友
        /// </summary>
        TeamRandom = 8,
    }
}
