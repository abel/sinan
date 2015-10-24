using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    public class WebCommand
    {
        /// <summary>
        /// 激合码奖励(45xx)
        /// </summary>
        public const string CodeAward = "codeAward";
        public const string CodeAwardR = "v.codeAwardR";

        /// <summary>
        /// 取得活动列表
        /// </summary>
        public const string GetPartList = "getPartList";
        /// <summary>
        /// 活动列表更新
        /// </summary>
        public const string UpdatePartListR = "v.updatePartListR";
    }
}
