using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// 镶嵌指令(30XX)
    /// </summary>
    public class MosaicCommand
    {
        /// <summary>
        /// 镶嵌操作
        /// </summary>
        public const string Mosaic = "mosaic";
        public const string MosaicR = "o.mosaicR";

        /// <summary>
        /// 镶嵌需求
        /// </summary>
        public const string MosaicNeed = "mosaicNeed";
        public const string MosaicNeedR = "o.mosaicNeedR";
    }

    public class MosaicReturn 
    {
        /// <summary>
        /// 镶嵌装备不存在
        /// </summary>
        public const int MosaicEquipNo = 22400;
        /// <summary>
        /// 不允许镶嵌
        /// </summary>
        public const int MosaicNoLet = 22401;
        /// <summary>
        /// 不允许改造
        /// </summary>
        public const int MosaicNoChange = 22402;
        /// <summary>
        /// 提升成功率的道具不足
        /// </summary>
        public const int MosaicUpGoods = 22403;
        /// <summary>
        /// 表示镶嵌位置没有打孔
        /// </summary>
        public const int MosaicNoPunch = 22404;
        /// <summary>
        /// 宝石不存在
        /// </summary>
        public const int MosaicNoBaoShi = 22405;
        /// <summary>
        /// 镶嵌游戏币不足
        /// </summary>
        public const int MosaicNoScoreB = 22406;
        /// <summary>
        /// 镶嵌失败
        /// </summary>
        public const int MosaicFail = 22407;
        /// <summary>
        /// 表示该位置已经镶嵌宝石，但新宝石要求必须大于旧宝石
        /// </summary>
        public const int MosaicNewOrOld = 22408;
    }
}
