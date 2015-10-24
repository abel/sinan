using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Entity
{
    /// <summary>
    /// 战斗结果
    /// </summary>
    public enum FightResult
    {
        /// <summary>
        /// 平局
        /// </summary>
        Tie = 0,

        /// <summary>
        /// 胜利
        /// </summary>
        Win = 1,

        /// <summary>
        /// 失败
        /// </summary>
        Lose = 2,

        /// <summary>
        /// 逃跑
        /// </summary>
        Flee = 3,
    }
}
