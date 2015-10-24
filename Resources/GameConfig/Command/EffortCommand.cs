using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// 成就系统(16XX)
    /// </summary>
    public class EffortCommand
    {
        /// <summary>
        /// 获取所有成就信息
        /// </summary>
        public const string GetEfforts = "getEfforts";
        public const string GetEffortsR = "t.getEffortsR";

        /// <summary>
        /// 获取单个成就信息
        /// </summary>
        public const string GetEffort = "getEffort";
        public const string GetEffortR = "t.getEffortR";

        /// <summary>
        /// 达成成就
        /// </summary>
        public const string DaCheng = "t.getSuccessR";

        /// <summary>
        /// 查看成就
        /// </summary>
        public const string ViewEffort = "viewEffort";
        public const string ViewEffortR = "t.viewEffortR";


        /// <summary>
        /// 得到称号
        /// </summary>
        public const string GetActTitleR = "t.getActTitleR";
    }
}
