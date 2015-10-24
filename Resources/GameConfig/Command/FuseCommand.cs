using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// 合成指令(22XX)
    /// </summary>
    public class FuseCommand
    {
        /// <summary>
        /// 得到可合成的配置列表
        /// </summary>
        public const string FuseList = "fuseList";
        public const string FuseListR = "o.fuseListR";
        /// <summary>
        /// 当前角色可以合成道具列表
        /// </summary>
        public const string FusePossibleList = "fusePossibleList";
        public const string FusePossibleListR = "o.fusePossibleListR";
        /// <summary>
        /// 合成操作
        /// </summary>
        public const string Fuse = "fuse";
        public const string FuseR = "o.fuseR";
    }

    public class FuseReturn 
    {
        /// <summary>
        /// 合成装备不存在
        /// </summary>
        public const int FuseEquipNo = 22100;
        /// <summary>
        /// 合成材料不足
        /// </summary>
        public const int FuseLessNo = 22101;
        /// <summary>
        /// 合成时装备配置有误
        /// </summary>
        public const int FuseEquipError = 22102;
        /// <summary>
        /// 合成时包袱满
        /// </summary>
        public const int FuseBurdenFull = 22103;
        /// <summary>
        /// 合成游戏币不足
        /// </summary>
        public const int FuseScore = 22104;
        /// <summary>
        /// 合成点券不足
        /// </summary>
        public const int FuseBond = 22105;
        /// <summary>
        /// 合成失败
        /// </summary>
        public const int FuseFail = 22106;
        /// <summary>
        /// 提高合成成功率的道具不足
        /// </summary>
        public const int FuseGoodsLv = 22107;
        /// <summary>
        /// 合成成功
        /// </summary>
        public const int FuseSuccess = 22108;
    }
}
