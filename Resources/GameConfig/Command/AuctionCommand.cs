using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (12XX)
    /// </summary>
    public class AuctionCommand
    {
        /// <summary>
        /// 得到出售和竞标列表
        /// </summary>
        public const string AuctionSellOrBidList = "auctionSellOrBidList";
        public const string AuctionSellOrBidListR = "o.auctionSellOrBidListR";
        /// <summary>
        /// 出售
        /// </summary>
        public const string AuctionSell = "auctionSell";
        public const string AuctionSellR = "o.auctionSellR";
        /// <summary>
        /// 得到购买道具所有列表
        /// </summary>
        public const string AuctionBuyList = "auctionBuyList";
        public const string AuctionBuyListR = "o.auctionBuyListR";
        /// <summary>
        /// 购买一口价
        /// </summary>
        public const string AuctionBuy = "auctionBuy";
        public const string AuctionBuyR = "o.auctionBuyR";
        /// <summary>
        /// 竞标
        /// </summary>
        public const string AuctionBid = "auctionBid";
        public const string AuctionBidR = "o.auctionBidR";
        /// <summary>
        /// 取消拍卖
        /// </summary>
        public const string ExitAuction = "exitAuction";
        public const string ExitAuctionR = "o.exitAuctionR";
    }


    public class AuctionReturn 
    {
        /// <summary>
        /// 出售操作成功
        /// </summary>
        public const int SellSuccess = 23400;
        /// <summary>
        /// 时限不正确
        /// </summary>
        public const int LimitTime = 23401;
        /// <summary>
        /// 余额不足交保管费
        /// </summary>
        public const int CustodialFees = 23402;
        /// <summary>
        /// 包袱类型不正确
        /// </summary>
        public const int BurdenTypeError = 23403;
        /// <summary>
        /// 与对应位置道具不符
        /// </summary>
        public const int PostionError = 23405;
        /// <summary>
        /// 起价太底
        /// </summary>
        public const int StartScore = 23406;
        /// <summary>
        /// 不能出售绑定道具
        /// </summary>
        public const int NoSellIsBinding = 23407;

        /// <summary>
        /// 游戏币不足
        /// </summary>
        public const int NoScore = 23408;
        /// <summary>
        /// 竞拍的物品不存在
        /// </summary>
        public const int NoGoods = 23409;
        /// <summary>
        /// 竞拍物品已经过期
        /// </summary>
        public const int Expired = 23410;
        /// <summary>
        /// 竞拍价格底于前者,竞拍失败
        /// </summary>
        public const int NoBidPrice = 23411;
        /// <summary>
        /// 竞拍成功
        /// </summary>
        public const int BidSuccess = 23412;
        /// <summary>
        /// 购买成功
        /// </summary>
        public const int BuySuccess = 23413;
        /// <summary>
        /// 已经销售
        /// </summary>
        public const int IsSell = 23414;
        /// <summary>
        /// 不能一口价
        /// </summary>
        public const int NoPrice = 23415;
        /// <summary>
        /// 竞价不能超过一口价
        /// </summary>
        public const int MorePrice = 23416;
        /// <summary>
        /// 竞价不能底于最小竞价
        /// </summary>
        public const int LossStartPrice = 23417;

        /// <summary>
        /// 同时可拍卖的条数
        /// </summary>
        public const int AuctionSellerCount = 23418;
        /// <summary>
        /// 达到上限提示
        /// </summary>
        public const int SellerCount = 23419;
        /// <summary>
        /// 有时间限制的物品不能拍卖
        /// </summary>
        public const int AuctionFreezeDate = 23420;

        /// <summary>
        /// 起拍价格限制
        /// </summary>
        public const int StartPriceClient = 23421;

    }
}
