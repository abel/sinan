using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Entity
{
    /// <summary>
    /// 玩家在游戏中的动作
    /// (如行走/跑/打座/战斗/站立等....)
    /// </summary>
    public enum ActionState
    {
        ///// <summary>
        ///// 初始状态..(站立)
        ///// </summary>
        //None = 0,

        ///// <summary>
        ///// 行走
        ///// </summary>
        //Walk = 1,

        ///// <summary>
        ///// 奔跑
        ///// </summary>
        //Run = 2,
        
        /// <summary>
        /// 站立
        /// </summary>
        Standing = 4,

        /// <summary>
        /// 战斗
        /// </summary>
        Fight = 100,
        /// <summary>
        /// 正在竞技场中
        /// </summary>
        Arena=101,
        /// <summary>
        /// 冥想
        /// </summary>
        Meditation=102
    }
}
