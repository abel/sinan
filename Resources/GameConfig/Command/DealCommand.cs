using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (15XX)
    /// </summary>
    public class DealCommand
    {
        /// <summary>
        /// 申请交易
        /// </summary>
        public const string DealApply = "dealApply";
        public const string DealApplyR = "o.dealApplyR";

        /// <summary>
        /// 申请交易回复
        /// </summary>
        public const string DealApplyBack = "dealApplyBack";
        public const string DealApplyBackR = "o.dealApplyBackR";

        /// <summary>
        /// 锁定交易
        /// </summary>
        public const string LockDeal = "lockDeal";
        public const string LockDealR = "o.lockDealR";

        /// <summary>
        /// 确定交易
        /// </summary>
        public const string EnterDeal = "enterDeal";
        public const string EnterDealR = "o.enterDealR";

        /// <summary>
        /// 退出交易
        /// </summary>
        public const string ExitDeal = "exitDeal";
        public const string ExitDealR = "o.exitDealR";
    }

    public class DealReturn 
    {
        /// <summary>
        /// 申请交易成功
        /// </summary>
        public const int DealApplySuccess = 23300;
        /// <summary>
        /// 交易过程中，请放入要求交易的物品
        /// </summary>
        public const int AgreeDeal = 23301;
        /// <summary>
        /// 对放已经下线
        /// </summary>
        public const int NoLine = 23302;
        /// <summary>
        /// 正在给其它人进行交易
        /// </summary>
        public const int DealOther = 23303;
        /// <summary>
        /// 交易被拒绝
        /// </summary>
        public const int NoDeal = 23304;
        /// <summary>
        /// 回复信息有误
        /// </summary>
        public const int BackMsgError = 23305;
        /// <summary>
        /// 停止交易
        /// </summary>
        public const int StopDeal = 23306;

        /// <summary>
        /// 第一个确定
        /// </summary>
        public const int FristEnter = 23307;
        /// <summary>
        /// 交易成功
        /// </summary>
        public const int DealSuccess = 23308;


        /// <summary>
        /// 已经回复
        /// </summary>
        public const int IsBack = 23309;
        /// <summary>
        /// 包袱格子不够，请先整理你的包袱再确定交易
        /// </summary>
        public const int BurdenFull = 23310;
        /// <summary>
        /// 游戏币不足
        /// </summary>
        public const int NoScore = 23311;
        /// <summary>
        /// 晶币不足
        /// </summary>
        public const int NoCoin = 23312;
        /// <summary>
        /// 上传物品与包袱中物品不相符
        /// </summary>
        public const int GoodsError = 23313;
        /// <summary>
        /// 绑定物品不能交易
        /// </summary>
        public const int IsBinding = 23314;
        /// <summary>
        /// 包袱中可交易的物品与你想交易的物品不符
        /// </summary>
        public const int NoDealGoodsCount = 23315;
        /// <summary>
        /// 请先申请交易或者交易已经完成
        /// </summary>
        public const int IsDeal = 23316;
        /// <summary>
        /// 物品所在包袱中的位置或数量发生变化不能再操作
        /// </summary>
        public const int NumberError = 23317;
        /// <summary>
        /// 您的包袱空间不足，请整理包袱后再进行交易
        /// </summary>
        public const int LockBurdenFull_0 = 23318;
        /// <summary>
        /// 对方包袱空间不足,请对方整理包袱后再进行交易
        /// </summary>
        public const int LockBurdenFull_1 = 23319;
        /// <summary>
        /// 对方正在战斗中不能与你进行交易
        /// </summary>
        public const int Fighting = 23320;
        /// <summary>
        /// 自己正在战斗中交易无效
        /// </summary>
        public const int SelfFighting = 23321;



        /// <summary>
        /// 有时间限制的物品不能交易
        /// </summary>
        public const int IsCheck1 = 23322;
        /// <summary>
        /// 上传数据有重复数据,请重新操作
        /// </summary>
        public const int IsCheck2 = 23323;

    }
}
