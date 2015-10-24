using System;
using System.Collections.Generic;
using MongoDB.Bson;
using Sinan.Data;
using Sinan.FrontServer;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Log;
using Sinan.Observer;
using Sinan.Util;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.AuctionModule.Business
{
    public class AuctionBusiness
    {
        static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 得到自己出售和竞标的列表
        /// </summary>
        /// <param name="note"></param>
        public static void AuctionSellOrBidList(UserNote note)
        {
            string soleid = note.PlayerID + "AuctionSellOrBidList";

            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                int total = 0;
                int curIndex = 0;

                string optionType = note.GetString(0);
                int pageSize = 6;//note.GetInt32(1)
                int pageIndex = note.GetInt32(2);

                string npcid = note.GetString(3);//NPC
                if (!note.Player.EffectActive(npcid, ""))
                    return;


                List<Variant> list = AuctionBase.GetAuctionSellList(note.PlayerID, optionType, pageSize, pageIndex, out total, out curIndex);

                note.Call(AuctionCommand.AuctionSellOrBidListR, note.GetString(0), list, total, curIndex);
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 出售
        /// </summary>
        public static void AuctionSell(UserNote note)
        {
            int num = Convert.ToInt32(TipManager.GetMessage(AuctionReturn.AuctionSellerCount));
            if (AuctionAccess.Instance.AuctionSellerCount(note.PlayerID) >= num)
            {
                string msg = string.Format(TipManager.GetMessage(AuctionReturn.SellerCount), num);
                note.Call(AuctionCommand.AuctionSellR, false, msg);
                return;
            }

            Variant d = note.GetVariant(0);

            string npcid = note.GetString(1);//NPC
            if (!note.Player.EffectActive(npcid, ""))
                return;

            PlayerEx b0 = note.Player.B0;
            if (b0 == null)
            {
                note.Call(AuctionCommand.AuctionSellR, false, TipManager.GetMessage(AuctionReturn.BurdenTypeError));
                return;
            }

            if (d.GetStringOrDefault("PackageType") != "B0")
            {
                note.Call(AuctionCommand.AuctionSellR, false, TipManager.GetMessage(AuctionReturn.BurdenTypeError));
                return;
            }


            Variant tmp = BurdenManager.BurdenPlace(b0, d.GetIntOrDefault("Postion"));



            if (tmp == null)
            {
                note.Call(AuctionCommand.AuctionSellR, false, TipManager.GetMessage(AuctionReturn.PostionError));
                return;
            }

            string soleid = tmp.GetStringOrDefault("E");
            string goodsid = tmp.GetStringOrDefault("G");
            int nu = tmp.GetIntOrDefault("A");
            //物品发生变化
            if (soleid != d.GetStringOrDefault("SoleID"))
            {
                note.Call(AuctionCommand.AuctionSellR, false, TipManager.GetMessage(DealReturn.NumberError));
                return;
            }

            //数量发生变化
            if (nu != d.GetIntOrDefault("Number"))
            {
                note.Call(AuctionCommand.AuctionSellR, false, TipManager.GetMessage(DealReturn.NumberError));
                return;
            }

            Variant t = tmp.GetVariantOrDefault("T");
            if (t != null)
            {
                if (t.ContainsKey("EndTime"))
                {
                    note.Call(AuctionCommand.AuctionSellR, false, TipManager.GetMessage(AuctionReturn.AuctionFreezeDate));
                    return;
                }
            }


            if (tmp.GetIntOrDefault("H") == 1)
            {
                note.Call(AuctionCommand.AuctionSellR, false, TipManager.GetMessage(AuctionReturn.NoSellIsBinding));
                return;
            }

            Variant dy = new Variant(20);
            dy.Add("Number", d.GetIntOrDefault("Number"));
            dy.Add("Price", d.GetIntOrDefault("Price"));
            dy.Add("LimitTime", d.GetIntOrDefault("LimitTime"));

            string name = "";
            int startScore = 0;//起拍价

            if (soleid != tmp.GetStringOrDefault("G"))
            {
                Goods g = GoodsAccess.Instance.FindOneById(soleid);
                if (g == null)
                {
                    note.Call(AuctionCommand.AuctionSellR, false, TipManager.GetMessage(DealReturn.NumberError));
                    return;
                }
                dy.Add("SoleID", g.ID);
                dy.Add("GoodsID", g.GoodsID);
                dy.Add("GoodsType", g.Value.GetStringOrDefault("GoodsType"));
                dy.Add("Sort", g.Value.GetIntOrDefault("Sort"));
                Variant Price = g.Value.GetVariantOrDefault("Price");
                startScore = Price.GetIntOrDefault("StallScore");
                name = g.Name;

            }
            else
            {
                GameConfig gc = GameConfigAccess.Instance.FindOneById(soleid);
                if (gc == null)
                {
                    note.Call(AuctionCommand.AuctionSellR, false, TipManager.GetMessage(DealReturn.NumberError));
                    return;
                }
                dy.Add("SoleID", gc.ID);
                dy.Add("GoodsID", gc.ID);
                dy.Add("GoodsType", gc.Value.GetStringOrDefault("GoodsType"));
                dy.Add("Sort", gc.Value.GetIntOrDefault("Sort"));

                Variant Price = gc.Value.GetVariantOrDefault("Price");
                startScore = Price.GetIntOrDefault("StallScore");
                name = gc.Name;
            }


            int number = dy.GetIntOrDefault("Number");

            int startScoreTotal = startScore * number;//系统指导价
            int startPriceClient = d.GetIntOrDefault("StartPrice");//要求起价
            int priceClient = d.GetIntOrDefault("Price");//一口价

            if (startScoreTotal > startPriceClient)
            {
                note.Call(AuctionCommand.AuctionSellR, false, TipManager.GetMessage(AuctionReturn.StartScore));
                return;
            }

            if (startScoreTotal > priceClient || startPriceClient > priceClient)
            {
                note.Call(AuctionCommand.AuctionSellR, false, TipManager.GetMessage(AuctionReturn.StartPriceClient));
                return;
            }


            if (d.GetIntOrDefault("LimitTime") == 6)
            {
                dy["CustodialFees"] = Convert.ToInt32(startScore * 0.01 * number);
            }
            else if (d.GetIntOrDefault("LimitTime") == 12)
            {
                dy["CustodialFees"] = Convert.ToInt32(startScore * 0.02 * number);
            }
            else if (d.GetIntOrDefault("LimitTime") == 24)
            {
                dy["CustodialFees"] = Convert.ToInt32(startScore * 0.03 * number);
            }
            else if (d.GetIntOrDefault("LimitTime") == 48)
            {
                dy["CustodialFees"] = Convert.ToInt32(startScore * 0.05 * number);
            }
            else
            {
                note.Call(AuctionCommand.AuctionSellR, false, TipManager.GetMessage(AuctionReturn.LimitTime));
                return;
            }

            int CustodialFees = dy.GetIntOrDefault("CustodialFees");

            if (note.Player.Score < CustodialFees || (!note.Player.AddScore(-CustodialFees, FinanceType.AuctionSell)))
            {
                note.Call(AuctionCommand.AuctionSellR, false, TipManager.GetMessage(AuctionReturn.CustodialFees));
                return;
            }

            //同时清理包袱中的物品
            BurdenManager.BurdenClear(tmp);
            if (b0.Save())
            {
                //保存成功
                note.Player.UpdateBurden();
                //先扣除保管费
                dy.Add("StartPrice", d.GetIntOrDefault("StartPrice"));
                dy.Add("SellerID", note.PlayerID);
                dy.Add("SellName", note.Player.Name);
                dy.Add("BidderID", "");
                dy.Add("BidPrice", d.GetIntOrDefault("StartPrice"));//起拍价
                dy.Add("BidName", "");
                dy.Add("BidTime", null);


                DateTime dt = DateTime.UtcNow;
                dy.Add("Created", dt);
                dy.Add("EndTime", dt.AddHours(dy.GetIntOrDefault("LimitTime")));
                dy.Add("Status", 0);//0

                Auction auction = new Auction();
                auction.ID = ObjectId.GenerateNewId().ToString();
                auction.Name = name;
                auction.Modified = DateTime.UtcNow;
                auction.Value = dy;
                if (auction.Save())
                {
                    note.Call(AuctionCommand.AuctionSellR, true, "");
                    note.Player.AddLog(Actiontype.GoodsUse, goodsid, nu, GoodsSource.AuctionSell, "", 0);
                    return;
                }
            }
            
            note.Call(AuctionCommand.AuctionSellR, false, "");

        }

        /// <summary>
        /// 购买一口价
        /// </summary>
        public static void AuctionBuy(UserNote note)
        {
            string auctionid = note.GetString(0);
            Auction auction = AuctionAccess.Instance.FindOne(auctionid);
            if (auction == null)
            {
                //物品不存在
                note.Call(AuctionCommand.AuctionBuyR, false, TipManager.GetMessage(AuctionReturn.NoGoods));
                return;
            }
            string soleid = auctionid + note.Name;//表示唯一操作

            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                string npcid = note.GetString(1);//NPC
                if (!note.Player.EffectActive(npcid, ""))
                    return;
                

                Variant v = auction.Value;

                if (v.GetIntOrDefault("Status") != 0)
                {
                    note.Call(AuctionCommand.AuctionBuyR, false, TipManager.GetMessage(AuctionReturn.IsSell));

                    return;
                }

                DateTime dt = DateTime.UtcNow;
                DateTime endTime;
                if (!DateTime.TryParse(v.GetStringOrDefault("EndTime"), out endTime))
                {
                    note.Call(AuctionCommand.AuctionBuyR, false, TipManager.GetMessage(AuctionReturn.Expired));

                    return;
                }
                if (endTime < dt)
                {
                    //过期物品不能竞拍
                    note.Call(AuctionCommand.AuctionBuyR, false, TipManager.GetMessage(AuctionReturn.Expired));
                    return;
                }
                int price = v.GetIntOrDefault("Price");//一口价
                int bidPrice = v.GetIntOrDefault("BidPrice");//前一位竞价金额
                if (price == 0)
                {
                    note.Call(AuctionCommand.AuctionBuyR, false, TipManager.GetMessage(AuctionReturn.NoPrice));

                    return;
                }

                if (note.Player.Score < price || (!note.Player.AddScore(-price, FinanceType.AuctionBuy)))
                {
                    note.Call(AuctionCommand.AuctionBuyR, false, TipManager.GetMessage(AuctionReturn.NoScore));
                    //m_dic.TryRemove(auctionid, out strs);
                    return;
                }


                int fee = Convert.ToInt32(Math.Floor(price * 0.05));

                #region 一口价成功购买者
                //一口价
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("GoodsName", auction.Name);
                dic.Add("SellName", v.GetStringOrDefault("SellName"));
                dic.Add("Name", note.Player.Name);
                dic.Add("Score", price);

                List<Variant> goods = new List<Variant>();
                Variant tmp = new Variant(6);
                string sole = v.GetStringOrDefault("SoleID");
                string goodsid = v.GetStringOrDefault("GoodsID");
                int number = v.GetIntOrDefault("Number");
                tmp.Add("SoleID", sole);
                tmp.Add("GoodsID", goodsid);
                tmp.Add("GoodsType", v.GetStringOrDefault("GoodsType"));
                tmp.Add("Number", v.GetIntOrDefault("Number"));
                tmp.Add("Sort", v.GetIntOrDefault("Sort"));
                goods.Add(tmp);
                AuctionEmail.SendAuctionEmail(dic, AuctionEmail.BidOne, auction.ID, note.PlayerID, note.Player.Name, 0, 0, goods);
                note.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(note.PlayerID));
                #endregion

                #region 一口价成功出售者
                //出售者
                Dictionary<string, object> sell = new Dictionary<string, object>();
                sell.Add("GoodsName", auction.Name);
                sell.Add("BuyName", note.Player.Name);
                sell.Add("SellScore", price);
                sell.Add("Score", (price - fee));
                sell.Add("Fee", fee);

                string sellerID = v.GetStringOrDefault("SellerID");
                string sellName = v.GetStringOrDefault("SellName");
                PlayerBusiness SellOnLine = PlayersProxy.FindPlayerByID(sellerID);
                //PlayersProxy.TryGetOnlinePlayer(SellerID, out SellOnLine);

                AuctionEmail.SendAuctionEmail(sell, AuctionEmail.Sell, auction.ID, sellerID, sellName, price - fee, 0, null);

                if (SellOnLine != null && SellOnLine.Online)
                {
                    SellOnLine.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(sellerID));
                }
                #endregion

                #region 上一位竞价者竞购失败
                //Email BidEmail = null;
                if (v.GetStringOrEmpty("BidderID") != string.Empty)
                {
                    //竞购失败
                    Dictionary<string, object> bidFail = new Dictionary<string, object>();
                    bidFail.Add("GoodsName", auction.Name);
                    bidFail.Add("Name", v.GetStringOrEmpty("BidName"));
                    bidFail.Add("Score", bidPrice.ToString());
                    PlayerBusiness BidOnLine = PlayersProxy.FindPlayerByID(v.GetStringOrDefault("BidderID"));

                    AuctionEmail.SendAuctionEmail(bidFail, AuctionEmail.BidFail, auction.ID, BidOnLine.ID, BidOnLine.Name, bidPrice, 0, null);

                    if (BidOnLine != null && BidOnLine.Online)
                    {
                        BidOnLine.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(auction.Value.GetStringOrEmpty("BidderID")));
                    }
                }
                #endregion

                auction.Value["BidderID"] = note.PlayerID;
                auction.Value["BidPrice"] = price;
                auction.Value["BidName"] = note.Player.Name;
                auction.Value["BidTime"] = dt;
                auction.Value["Status"] = 1;//一口价
                auction.Save();

                note.Call(AuctionCommand.AuctionBuyR, true, TipManager.GetMessage(AuctionReturn.BuySuccess));

                //一口价日志
                note.Player.AddLog(Actiontype.Auction, goodsid, number, GoodsSource.AuctionBuy, sellerID + "," + sole, price);
            }
            finally
            {
                string n;
                m_dic.TryRemove(soleid, out n);
            }
        }

        /// <summary>
        /// 购买列表
        /// </summary>
        /// <param name="note"></param>
        public static void AuctionBuyList(UserNote note)
        {
            string soleid = note.PlayerID + string.Empty + note.Name;
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                int pageSize = note.GetInt32(0);
                int pageIndex = note.GetInt32(1);
                string goodsType = note.GetString(2);
                bool isName = note.GetBoolean(3);

                string npcid = note.GetString(4);//NPC
                if (!note.Player.EffectActive(npcid, ""))
                    return;

                int total = 0;
                int currIndex = 0;
                List<Auction> list = AuctionAccess.Instance.AuctionBuyList(note.PlayerID, goodsType, isName, pageSize, pageIndex, out total, out currIndex);
                List<Variant> msg = new List<Variant>();
                if (list != null)
                {
                    foreach (Auction a in list)
                    {
                        Variant v = new Variant();
                        v.Add("ID", a.ID);
                        v.Add("Name", a.Name);
                        foreach (var k in a.Value)
                        {
                            v.Add(k.Key, k.Value);
                        }
                        msg.Add(v);
                    }
                }
                note.Call(AuctionCommand.AuctionBuyListR, total, currIndex, msg);
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 竞价
        /// </summary>
        public static void AuctionBid(UserNote note)
        {

            string auctionid = note.GetString(0);
            int BidPrice = note.GetInt32(1);//竞价

            string npcid = note.GetString(2);//NPC
            if (!note.Player.EffectActive(npcid, ""))
                return;

            if (note.Player.Score < BidPrice)
            {
                note.Call(AuctionCommand.AuctionBidR, false, TipManager.GetMessage(AuctionReturn.NoScore));
                //游戏不足
                return;
            }
            Auction auction = AuctionAccess.Instance.FindOne(auctionid);
            if (auction == null)
            {
                //物口不存在
                note.Call(AuctionCommand.AuctionBidR, false, TipManager.GetMessage(AuctionReturn.NoGoods));
                return;
            }

            string soleid = auctionid + note.Name;
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                if (auction.Value.GetIntOrDefault("Status") != 0)
                {
                    note.Call(AuctionCommand.AuctionBidR, false, TipManager.GetMessage(AuctionReturn.IsSell));
                    return;
                }
                DateTime dt = DateTime.UtcNow;
                DateTime EndTime;

                if (!DateTime.TryParse(auction.Value.GetStringOrEmpty("EndTime"), out EndTime))
                {
                    note.Call(AuctionCommand.AuctionBidR, false, TipManager.GetMessage(AuctionReturn.Expired));
                    return;
                }

                if (EndTime < dt)
                {
                    //过期物品不能竞拍
                    note.Call(AuctionCommand.AuctionBidR, false, TipManager.GetMessage(AuctionReturn.Expired));
                    return;
                }

                int price = auction.Value.GetIntOrDefault("Price");//一口价

                if (price > 0 && BidPrice > price)
                {
                    note.Call(AuctionCommand.AuctionBidR, false, TipManager.GetMessage(AuctionReturn.MorePrice));
                    return;
                }
                else if (price > 0 && BidPrice == price)
                {
                    //一口价操作
                    UserNote n = new UserNote(note, AuctionCommand.AuctionBuy, new object[] { auction.ID });
                    Notifier.Instance.Publish(n);
                    return;
                }
                int bidPrice = auction.Value.GetIntOrDefault("BidPrice");
                if (string.IsNullOrEmpty(auction.Value.GetStringOrEmpty("BidderID")))
                {
                    if (bidPrice > BidPrice)
                    {
                        note.Call(AuctionCommand.AuctionBidR, false, TipManager.GetMessage(AuctionReturn.LossStartPrice));
                        return;
                    }
                }
                else
                {
                    if (bidPrice >= BidPrice)
                    {
                        //竞拍价格太底
                        note.Call(AuctionCommand.AuctionBidR, false, TipManager.GetMessage(AuctionReturn.NoBidPrice));
                        return;
                    }
                }
                if (!note.Player.AddScore(-BidPrice, FinanceType.AuctionBid))
                {
                    note.Call(AuctionCommand.AuctionBidR, false, TipManager.GetMessage(AuctionReturn.MorePrice));
                    return;
                }
                if (auction.Value.GetStringOrEmpty("BidderID") != string.Empty)
                {
                    //竞购失败
                    Dictionary<string, object> bidFail = new Dictionary<string, object>();
                    bidFail.Add("GoodsName", auction.Name);
                    bidFail.Add("Name", auction.Value.GetStringOrDefault("BidName"));
                    bidFail.Add("Score", bidPrice.ToString());


                    PlayerBusiness BidOnLine = PlayersProxy.FindPlayerByID(auction.Value.GetStringOrDefault("BidderID"));
                    AuctionEmail.SendAuctionEmail(bidFail, AuctionEmail.BidFail, auction.ID, BidOnLine.ID, BidOnLine.Name, bidPrice, 0, null);

                    if (BidOnLine != null && BidOnLine.Online)
                    {
                        BidOnLine.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(auction.Value.GetStringOrDefault("BidderID")));
                    }
                }

                auction.Value["BidderID"] = note.PlayerID;
                auction.Value["BidPrice"] = BidPrice;
                auction.Value["BidName"] = note.Player.Name;
                auction.Value["BidTime"] = dt;
                auction.Save();

                note.Call(AuctionCommand.AuctionBidR, true, TipManager.GetMessage(AuctionReturn.BidSuccess));
            }
            finally
            {
                string n;
                m_dic.TryRemove(soleid, out n);
            }
        }
    }
}
