using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.FrontServer
{
    public enum SceneType
    {
        /// <summary>
        /// 城市
        /// </summary>
        City = 0,

        /// <summary>
        /// 野外
        /// </summary>
        Outdoor = 1,

        /// <summary>
        /// 副本
        /// </summary>
        Ectype = 2,

        /// <summary>
        /// 子副本
        /// </summary>
        SubEctype = 3,

        /// <summary>
        /// 家园
        /// </summary>
        Home = 4,

        /// <summary>
        /// 战场
        /// </summary>
        Battle = 5,

        /// <summary>
        /// 个人竞技场
        /// </summary>
        ArenaPerson = 6,

        /// <summary>
        /// 队伍竞技场
        /// </summary>
        ArenaTeam = 7,

        /// <summary>
        /// 家族竞技场
        /// </summary>
        ArenaFamily = 8,

        /// <summary>
        /// 守护战争
        /// </summary>
        Pro = 9,

        /// <summary>
        /// 夺宝奇兵
        /// </summary>
        Rob = 10,

        /// <summary>
        /// 组队副本
        /// </summary>
        Instance = 11,

    }
}