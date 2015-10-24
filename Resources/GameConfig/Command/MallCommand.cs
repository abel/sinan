using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (27XX)
    /// </summary>
    public class MallCommand
    {
        /// <summary>
        /// 得到商城商品信息列表
        /// </summary>
        public const string GetMallList = "getMallList";
        public const string GetMallListR = "o.getMallListR";

        /// <summary>
        /// 购买商城商品
        /// </summary>
        public const string BuyMallGoods = "buyMallGoods";
        public const string BuyMallGoodsR = "o.buyMallGoodsR";

        /// <summary>
        /// 得到商城基本信息
        /// </summary>
        public const string GetMallDetails = "getMallDetails";
        public const string GetMallDetailsR = "o.getMallDetailsR";

        /// <summary>
        /// 1点券购买日志
        /// </summary>
        public const string OneBondLog = "oneBondLog";
        public const string OneBondLogR = "o.oneBondLogR";

        /// <summary>
        /// 1点券购买
        /// </summary>
        public const string OneBondBuy = "oneBondBuy";
        public const string OneBondBuyR = "o.oneBondBuyR";

        /// <summary>
        /// 提交购买晶币订单
        /// </summary>
        public const string CoinOrder = "coinOrder";
        public const string CoinOrderR = "w.coinOrderR";

        /// <summary>
        /// 充值成功
        /// </summary>
        public const string CoinSuccess = "w.coinSuccessR";

        /// <summary>
        /// 腾讯开放平台应用签名
        /// </summary>
        public const string Sign = "sign";
        public const string SignR = "w.signR";

        /// <summary>
        /// 产生订单号
        /// </summary>
        public const string NewToken = "newToken";
        public const string NewTokenR = "w.newTokenR";
    }

    public class MallReturn
    {
        /// <summary>
        /// 购买成功
        /// </summary>
        public const int MallBuySuccess = 22600;
        /// <summary>
        /// 包袱格子数不足
        /// </summary>
        public const int BurdenFull = 22601;
        /// <summary>
        /// 商城物品不存在
        /// </summary>
        public const int MallGoodsNo = 22602;
        /// <summary>
        /// 商品没有上架
        /// </summary>
        public const int MallUp = 22603;
        /// <summary>
        /// 商品已经下架
        /// </summary>
        public const int MallDown = 22604;
        /// <summary>
        /// 类型不正确
        /// </summary>
        public const int MallCoinTypeNo = 22605;

        /// <summary>
        /// 晶币不足
        /// </summary>
        public const int MallCoinNo = 22606;

        /// <summary>
        /// 点券不足
        /// </summary>
        public const int MallBondNo = 22607;

        /// <summary>
        /// 购买物品超过商城该品库存量
        /// </summary>
        public const int MallStock = 22608;

        /// <summary>
        /// 购买物品量超过限量
        /// </summary>
        public const int LimitCount = 22609;

        /// <summary>
        /// 购买失败
        /// </summary>
        public const int MallFail = 22610;
        /// <summary>
        /// 购买成功
        /// </summary>
        public const int MallSuccess = 22611;
        /// <summary>
        /// 不能重复购买
        /// </summary>
        public const int NoRepeat = 22612;
        /// <summary>
        /// 与你的性别不符不能购买
        /// </summary>
        public const int NoSex = 22613;
    }
}
