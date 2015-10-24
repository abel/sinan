using System;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Schedule;
using Sinan.Util;
using Sinan.Log;

namespace Sinan.AuctionModule.Business
{
    public sealed class AuctionTimer : SchedulerBase
    {

        AuctionTimer()
            : base(10 * 60 * 1000, 10 * 60 * 1000)
        {
           
        }

        private string _auctionID;


        /// <summary>
        /// 出售订单号
        /// </summary>
        public string AuctionID
        {
            get { return _auctionID; }
            set { _auctionID = value; }
        }

        /// <summary>
        /// 定时业务
        /// </summary>
        protected override void Exec()
        {
            List<Auction> list = AuctionAccess.Instance.FindExpiredList();
            if (list.Count > 0)
            {
                foreach (Auction auction in list)
                {
                    Variant m = auction.Value;
                    if (m == null) continue;
                    if (m.GetIntOrDefault("Status") == 0)
                    {
                        List<Variant> goods = new List<Variant>();
                        Variant v = new Variant(5);
                        v.Add("SoleID", m.GetStringOrEmpty("SoleID"));
                        v.Add("GoodsID", m.GetStringOrEmpty("GoodsID"));
                        v.Add("GoodsType", m.GetStringOrEmpty("GoodsType"));
                        v.Add("Number", m.GetIntOrDefault("Number"));
                        v.Add("Sort", m.GetIntOrDefault("Sort"));
                        goods.Add(v);

                        string sellerID = m.GetStringOrDefault("SellerID");
                        string sellName = m.GetStringOrDefault("SellName");
                        int startPrice = m.GetIntOrDefault("StartPrice");
                        int limitTime = m.GetIntOrDefault("LimitTime");

                        string bidderID = m.GetStringOrDefault("BidderID");
                        string bidName = m.GetStringOrDefault("BidName");
                        int bidPrice = m.GetIntOrDefault("BidPrice");

                        int fee = Convert.ToInt32(Math.Floor(bidPrice * 0.05));//收取5%的手续费

                        if (string.IsNullOrEmpty(bidderID))
                        {
                            //表示没有人竞价,物品退还给出售者
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            dic.Add("GoodsName", auction.Name);
                            dic.Add("Name", sellName);
                            dic.Add("StartPrice", startPrice);
                            dic.Add("LimitTime", limitTime);
                            auction.Value["Status"] = 3;
                            AuctionEmail.SendAuctionEmail(dic, AuctionEmail.Expire, auction.ID, sellerID, sellName, 0, 0, goods);
                        }
                        else
                        {
                            //卖的费用给出售者

                            //恭喜您，您在拍卖行出售[GoodsName]成功！
                            //您在拍卖行寄卖的[GoodsName] 以SellScore石币被买家[BuyName]买下。
                            //扣除拍卖手续费：Fee石币
                            //本次出售您获得货款：Score石币
                            //请尽快领取您的货款！

                            Dictionary<string, object> sell = new Dictionary<string, object>();
                            sell.Add("GoodsName", auction.Name);
                            sell.Add("BuyName", bidName);
                            sell.Add("SellScore", bidPrice);
                            sell.Add("Score", bidPrice - fee);
                            sell.Add("Fee", fee);
                            AuctionEmail.SendAuctionEmail(sell, AuctionEmail.Sell, auction.ID, sellerID, sellName, (bidPrice - fee), 0, null);


                            //物品给竞价者
                            Dictionary<string, object> biddic = new Dictionary<string, object>();
                            biddic.Add("GoodsName", auction.Name);
                            biddic.Add("SellName", sellName);
                            biddic.Add("Name", bidName);
                            biddic.Add("Score", bidPrice);
                            auction.Value["Status"] = 2;//竞价成功
                            AuctionEmail.SendAuctionEmail(biddic, AuctionEmail.Bid, auction.ID, bidderID, bidName, 0, 0, goods);
                        }
                        auction.Save();
                        //判断出售者在线不
                        PlayerBusiness sellOnLine = PlayersProxy.FindPlayerByID(sellerID);

                        if (sellOnLine != null && sellOnLine.Online)
                        {
                            int sellnum = EmailAccess.Instance.NewTotal(sellerID);
                            if (sellnum > 0)
                            {
                                sellOnLine.Call(EmailCommand.NewEmailTotalR, sellnum);
                            }
                        }

                        if (!string.IsNullOrEmpty(bidderID))
                        {
                            //判断竞价者在线不
                            PlayerBusiness buyOnLine = PlayersProxy.FindPlayerByID(bidderID);
                            if (buyOnLine != null && buyOnLine.Online)
                            {
                                int bidnum = EmailAccess.Instance.NewTotal(bidderID);
                                if (bidnum > 0)
                                {
                                    buyOnLine.Call(EmailCommand.NewEmailTotalR, bidnum);
                                }
                            }
                            //竞价取得物品
                            buyOnLine.AddLog(Actiontype.Auction, m.GetStringOrEmpty("GoodsID"), m.GetIntOrDefault("Number"), GoodsSource.AuctionBuy, sellerID + "," + m.GetStringOrEmpty("SoleID"), bidPrice);
                        }
                    }
                }
            }
            //移除过期
            AuctionAccess.Instance.AutionDel();
        }
    }
}
