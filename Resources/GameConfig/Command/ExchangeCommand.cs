using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// 移炼指令(18XX)
    /// </summary>
    public class ExchangeCommand
    {
        /// <summary>
        /// 移炼需求
        /// </summary>
        public const string ExchangNeed = "exchangNeed";
        public const string ExchangNeedR = "o.exchangNeedR";

        /// <summary>
        /// 移炼
        /// </summary>
        public const string ExchangeBaoShi = "exchangeBaoShi";
        public const string ExchangeBaoShiR = "o.exchangeBaoShiR";
    }

    public class ExchangReturn 
    {
        /// <summary>
        /// 新装备等级小于旧装备,不能移炼
        /// </summary>
        public const int NewEquipNoLevel = 22200;
        /// <summary>
        /// 旧装备宝石等级不足，不能移炼
        /// </summary>
        public const int OldBaoShiNoLevel = 22201;
        /// <summary>
        /// 新装备孔个数不够,不能移炼
        /// </summary>
        public const int NewPunchNo = 22202;
        /// <summary>
        /// 宝石信息不存在
        /// </summary>
        public const int BaoShiNo = 22203;
        /// <summary>
        /// 装备不存在
        /// </summary>
        public const int EquipNo = 22204;
        /// <summary>
        /// 装备不能改造
        /// </summary>
        public const int EquipNoChange = 22205;
        /// <summary>
        /// 移炼游戏币不足
        /// </summary>
        public const int ExchangeNoScore = 22206;
    }
}
