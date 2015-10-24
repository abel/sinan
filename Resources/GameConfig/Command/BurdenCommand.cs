using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (13XX)
    /// </summary>
    public class BurdenCommand
    {
        /// <summary>
        /// 得到仓库或包袱列表
        /// </summary>
        public const string BurdenList = "burdenList";
        public const string BurdenListR = "o.burdenListR";


        /// <summary>
        /// 包袱或仓库更新
        ///</summary>
        public const string UpdatePackage = "updatePackage";
        public const string UpdatePackageR = "o.updatePackageR";

        /// <summary>
        /// 取得指定仓库信息
        /// </summary>
        public const string BurdenInfo = "burdenInfo";
        public const string BurdenInfoR = "o.burdenInfoR";

        /// <summary>
        /// 包袱或仓库扩展操作
        /// </summary>
        public const string BurdenExtend = "burdenExtend";
        public const string BurdenExtendR = "o.burdenExtendR";

        /// <summary>
        /// 整理包袱
        /// </summary>
        public const string BurdenFinishing = "burdenFinishing";
        public const string BurdenFinishingR = "o.burdenFinishingR";

        /// <summary>
        /// 扡动
        /// </summary>
        public const string BurdenDrag = "burdenDrag";
        public const string BurdenDragR = "o.burdenDragR";

        /// <summary>
        /// 拆分
        /// </summary>
        public const string BurdenSplit = "burdenSplit";
        public const string BurdenSplitR = "o.burdenSplitR";

        /// <summary>
        /// 物品从仓库取出
        /// </summary>
        public const string BurdenOut = "burdenOut";
        public const string BurdenOutR = "o.burdenOutR";

        /// <summary>
        /// 更新显示编号
        /// </summary>
        public const string SaveShowInfo = "saveShowInfo";
        //public const string UpdateShowInfoR = "o.updateShowInfoR";
    }
    public class BurdenReturn 
    {
        /// <summary>
        /// 已经最大量不能再扩展
        /// </summary>
        public const int BurdenMaxEx = 23500;
        /// <summary>
        /// 扩展晶币不足
        /// </summary>
        public const int NoCoin = 23501;
        /// <summary>
        /// 扩展成功
        /// </summary>
        public const int Success = 23502;

        /// <summary>
        /// 起始格子数据不正确
        /// </summary>
        public const int NoStartGrid = 23503;
        /// <summary>
        /// 起始格子数量小于相折分数量
        /// </summary>
        public const int NoNumber = 23504;
        /// <summary>
        /// 不能扡入该目标包袱
        /// </summary>
        public const int NoDrag = 23505;
        /// <summary>
        /// 不能扡入相同格子上
        /// </summary>
        public const int NoSame = 23506;
        /// <summary>
        /// 大于堆叠数
        /// </summary>
        public const int BigStact = 23507;
        /// <summary>
        /// 对不起，您的包袱剩余空间不足，请在整理包袱后再次操作
        /// </summary>
        public const int BurdenFull = 23508;
        /// <summary>
        /// 任务道具不能放入仓库中
        /// </summary>
        public const int TaskGoods = 23509;

        /// <summary>
        /// 【{0}】因为过期已经被移除\n
        /// </summary>
        public const int BurdenList1 = 23510;


        /// <summary>
        /// 参数不正确
        /// </summary>
        public const int BurdenDrag1 = 23511;
        /// <summary>
        /// 操作格子不能为空
        /// </summary>
        public const int BurdenDrag2 = 23512;

        /// <summary>
        /// 参数不正确
        /// </summary>
        public const int BurdenOut1 = 23513;
        /// <summary>
        /// 物品已经不存在
        /// </summary>
        public const int BurdenOut2 = 23514;
        /// <summary>
        /// 包袱已满，请整理后再操作
        /// </summary>
        public const int BurdenOut3 = 23515;
    }
}
