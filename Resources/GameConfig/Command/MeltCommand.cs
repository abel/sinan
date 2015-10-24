using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// 熔炼指令(28XX)
    /// </summary>
    public class MeltCommand
    {
        /// <summary>
        /// 得到熔炼的信息
        /// </summary>
        public const string MeltInfo = "meltInfo";
        public const string MeltInfoR = "o.meltInfoR";
        /// <summary>
        /// 熔炼
        /// </summary>
        public const string Melt = "melt";
        public const string MeltR = "o.meltR";
    }
    public class MeltReturn 
    {
        /// <summary>
        /// 熔炼装备不存在
        /// </summary>
        public const int MeltEquipNo = 22300;
        /// <summary>
        /// 熔炼装备配置不正确
        /// </summary>
        public const int MeltEquipError = 22301;
        /// <summary>
        /// 熔炼装备不能改造
        /// </summary>
        public const int MeltNoChange = 22302;
        /// <summary>
        /// 熔炼绑定游戏币不足
        /// </summary>
        public const int MeltNoScoreB = 22303;
        /// <summary>
        /// 熔炼配置信息有误
        /// </summary>
        public const int MeltConfigError = 22304;
        /// <summary>
        /// 熔炼时包袱已满
        /// </summary>
        public const int MeltBurdenFull = 22305;
    }
}
