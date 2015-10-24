using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (31XX)
    /// </summary>
    public class PaiHangCommand
    {
        /// <summary>
        /// 获取我的名次
        /// </summary>
        public const string GetMyRank = "getMyRank";
        public const string GetMyRankR = "r.getMyRankR";

        /// <summary>
        /// 获取角色排行
        /// </summary>
        public const string GetRoleLev = "getRoleLev";
        public const string GetRoleLevR = "r.getRoleLevR";

        /// <summary>
        /// 获取宠物排行
        /// </summary>
        public const string GetPetLev = "getPetLev";
        public const string GetPetLevR = "r.GetPetLevR";

        /// <summary>
        /// 获取夺宝排行
        /// </summary>
        public const string GetRobRank = "getRobRank";
        public const string GetRobRankR = "r.getRobRankR";
    }
}
