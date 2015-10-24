using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Entity
{
    /// <summary>
    /// 战斗类型
    /// </summary>
    public enum FightType
    {
        /// <summary>
        /// 不能战斗
        /// </summary>
        NotPK = 0,

        /// <summary>
        /// 暗雷
        /// </summary>
        HideAPC = 1,

        /// <summary>
        /// 任务APC
        /// </summary>
        TaskAPC = 2,

        /// <summary>
        /// PK
        /// </summary>
        PK = 3,

        /// <summary>
        /// 切磋
        /// </summary>
        CC = 4,

        /// <summary>
        /// 打明怪
        /// </summary>
        SceneAPC = 5,

        /// <summary>
        /// 夺宝奇兵中PK
        /// </summary>
        RobPK = 6,

        /// <summary>
        /// 守护战斗中PK
        /// </summary>
        ProPK = 7,

        /// <summary>
        /// 夺宝奇兵中打明怪
        /// </summary>
        RobAPC = 8,

        /// <summary>
        /// 守护战斗明怪
        /// </summary>
        ProAPC = 9,

        /// <summary>
        /// 组队秘境中的怪
        /// </summary>
        EctypeApc = 10,

        /// <summary>
        /// 家族Boss
        /// </summary>
        FamilyBoss = 11,

    }
}
