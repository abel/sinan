using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Entity
{
    /// <summary>
    /// 传送原因
    /// </summary>
    public enum TransmitType
    {
        /// <summary>
        /// 传送阵传送
        /// </summary>
        Pin = 1,

        /// <summary>
        /// NPC传送
        /// </summary>
        Npc = 2,

        /// <summary>
        /// 使用回城道具
        /// </summary>
        UseProp = 3,

        /// <summary>
        /// 参加活动
        /// </summary>
        Part = 4,

        /// <summary>
        /// 超时传出
        /// </summary>
        OverTime = 10,

        /// <summary>
        /// 角色死亡
        /// </summary>
        Dead = 11,
    }
}
