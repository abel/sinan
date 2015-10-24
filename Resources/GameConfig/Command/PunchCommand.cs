using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// 打孔指令(34XX)
    /// </summary>
    public class PunchCommand
    {
        /// <summary>
        /// 打孔需求
        /// </summary>
        public const string PunchNeed = "punchNeed";
        public const string PunchNeedR = "o.punchNeedR";
        /// <summary>
        /// 打孔操作
        /// </summary>
        public const string Punch = "punch";
        public const string PunchR = "o.punchR";
    }
    public class PunchReturn 
    {
        /// <summary>
        /// 装备不存在
        /// </summary>
        public const int PunchEquipNo = 22500;
        /// <summary>
        /// 装备不能够改造
        /// </summary>
        public const int PunchNoChange = 22501;
        /// <summary>
        /// 装备不能打孔
        /// </summary>
        public const int NoPunch = 22502;
        /// <summary>
        /// 装备配置不正确
        /// </summary>
        public const int PunchEquipError = 22503;
        /// <summary>
        /// 打孔已经满不能够再打孔
        /// </summary>
        public const int PunchFull = 22504;
        /// <summary>
        /// 打孔游戏币不正确
        /// </summary>
        public const int PunchScoreNo = 22505;
        /// <summary>
        /// 打孔所需材料不足
        /// </summary>
        public const int PunchNeedGoodsNo = 22506;
        /// <summary>
        /// 打孔失败
        /// </summary>
        public const int PunchFail = 22507;
    }
}
