using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Util;
using Sinan.Core;
using Sinan.Log;
using System.IO;

namespace Sinan.GoodsModule
{
    sealed public class MallMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[] {
                MallCommand.GetMallList,
               MallCommand.BuyMallGoods,
               MallCommand.OneBondLog,
               MallCommand.OneBondBuy,
               Application.APPSTART
            };
        }

        public override void Execute(INotification notification)
        {
            if (notification.Name == Application.APPSTART)
            {
                string path = Path.Combine(ConfigLoader.Config.DirBase, "Mall.txt");
                MallAccess.Load(path);
                return;
            }
            UserNote note = notification as UserNote;
            if (note == null || note.Player == null)
                return;

            switch (note.Name)
            {

                case MallCommand.GetMallList:
                    GetMallList(note);
                    break;
                case MallCommand.BuyMallGoods:
                    BuyMallGoods(note);
                    break;
                case MallCommand.OneBondLog:
                    OneBondLog(note);
                    break;
                case MallCommand.OneBondBuy:
                    BondBuy(note);
                    break;
            }
        }


        private void GetMallList(UserNote note)
        {
            HashSet<string> hs = MallAccess.HS;
            List<string> list = new List<string>();
            foreach (string soleid in hs)
            {
                list.Add(soleid);
            }
            note.Call(MallCommand.GetMallListR, list);
        }

        /// <summary>
        /// 取1点券可购买的物品
        /// </summary>
        /// <param name="note"></param>
        private void OneBondLog(UserNote note)
        {
            PlayerEx log = note.Player.Buylog;
            DateTime now = LocalTimeExtention.GetNowDate().ToUniversalTime();
            DateTime day;
            if (!log.Value.TryGetValueT("Date", out day)
                || day != now)
            {
                //生成新的..
                day = now;
                log.Value["Date"] = day;
                log.Value["Goods"] = BondBuyManager.Instance.RandomGoods(3);
                log.Value["Buy"] = string.Empty;
                log.Save();
            }
            //发送结果
            note.Call(MallCommand.OneBondLogR, day, log.Value["Goods"], log.Value["Buy"]);
        }

        /// <summary>
        /// 1点券购买
        /// </summary>
        /// <param name="note"></param>
        private void BondBuy(UserNote note)
        {
            PlayerEx log = note.Player.Buylog;
            //买的道具ID
            string goodsid = note.GetString(0);
            //检查是否可以购买
            IList goods = log.Value.GetValue<IList>("Goods");
            if (!goods.Contains(goodsid))
            {
                note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallGoodsNo));
                return;
            }

            if (log.Value.GetStringOrDefault("Buy") == goodsid)
            {
                note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.LimitCount));
                return;
            }

            GameConfig gc1 = GameConfigAccess.Instance.FindOneById(goodsid);
            if (gc1 == null)
            {
                note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallGoodsNo));
                return;
            }

            PlayerEx burden = note.Player.B0;
            Dictionary<string, Variant> dic = new Dictionary<string, Variant>();

            //需要晶币/点券的总量
            int cost = 1;
            Variant v = new Variant();
            v.Add("Number1", 1);
            GoodsAccess.Instance.TimeLines(gc1, v);
            //if (gc1.Value.ContainsKey("TimeLines"))
            //{
            //    Variant TimeLines = gc1.Value.GetValueOrDefault<Variant>("TimeLines");
            //    int hour = 0;
            //    if (TimeLines != null)
            //    {
            //        if (TimeLines.TryGetValueT("Hour", out hour) && hour > 0)
            //        {
            //            //限时问题
            //            v.Add("EndTime", DateTime.UtcNow.AddHours(hour));
            //        }
            //        else if (TimeLines.ContainsKey("SetDate"))
            //        {
            //            //定时设置
            //            DateTime t;
            //            if (DateTime.TryParse(TimeLines.GetStringOrDefault("SetDate"), out t))
            //            {
            //                v.Add("EndTime", TimeLines.GetDateTimeOrDefault("SetDate"));
            //            }
            //        }
            //    }
            //}
            dic.Add(goodsid, v);

            IList Content = burden.Value.GetValue<IList>("C");
            if (BurdenManager.IsFullBurden(Content, dic))
            {
                note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.BurdenFull));
                return;
            }

            if (note.Player.Bond < cost || (!note.Player.AddBond(-cost, FinanceType.MallBuy, goodsid)))
            {
                note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallBondNo));
                return;
            }

            log.Value["Buy"] = goodsid;
            log.Save();

            note.Player.AddGoods(dic, GoodsSource.BondBuy);
            note.Call(MallCommand.BuyMallGoodsR, true, new List<Variant>() { v });

            note.Call(MallCommand.OneBondLogR, log.Value["Date"], log.Value["Goods"], log.Value["Buy"]);
        }

        /// <summary>
        /// 购买商城产品
        /// </summary>
        /// <param name="note"></param>
        private void BuyMallGoods(UserNote note)
        {
            //买的道具ID
            string id = note.GetString(0);
            //数量
            int number = note.GetInt32(1);
            //货币类型
            string cointype = note.GetString(2);

            //是否通过GM上下架检查
            if (ServerManager.IsMall)
            {
                //物品没有上架
                if (!MallAccess.HS.Contains(id))
                {
                    note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallDown));
                    return;
                }
            }

            GameConfig gc = GameConfigAccess.Instance.FindOneById(id);
            if (gc == null)
            {
                note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallGoodsNo));
                return;
            }

            Variant v = gc.Value;
            DateTime dt = DateTime.UtcNow;

            string upDate = v.GetStringOrDefault("UpDate");
            //上架时间
            if (!string.IsNullOrEmpty(upDate))
            {
                DateTime update = v.GetDateTimeOrDefault("UpDate").ToUniversalTime();
                if (dt < update)
                {
                    note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallUp));
                    return;
                }
            }
            string downDate = v.GetStringOrDefault("DownDate");
            //下架时间
            if (!string.IsNullOrEmpty(downDate))
            {
                DateTime endDate = v.GetDateTimeOrDefault("DownDate").ToUniversalTime();
                if (endDate < dt)
                {
                    note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallDown));
                    return;
                }
            }

            string goodsid = v.GetStringOrDefault("GoodsID");
            int count = v.GetIntOrDefault("Number");

            GameConfig gid = GameConfigAccess.Instance.FindOneById(goodsid);
            if (gid == null)
            {
                note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallGoodsNo));
                return;
            }

            if (gc.SubType == "ShiZhuang")
            {
                PlayerEx wx = note.Player.Wardrobe;
                Variant wv = wx.Value;
                IList wl = wv.GetValue<IList>("WardrobeList");
                if (wl != null)
                {
                    if (wl.Contains(goodsid))
                    {
                        note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.NoRepeat));
                        return;
                    }
                }

                Variant limit = gid.Value.GetVariantOrDefault("UserLimit");
                if (limit != null)
                {
                    if (limit.ContainsKey("Sex"))
                    {
                        int sex = Convert.ToInt32(limit["Sex"]);
                        if (sex != 2 && sex != note.Player.Sex)
                        {
                            note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.NoSex));
                            return;
                        }
                    }
                }

                //需要晶币
                int coin = gc.Value.GetIntOrDefault("ZCoin");
                if (note.Player.Coin < coin)
                {
                    note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallCoinNo));
                    return;
                }

                if (!note.Player.AddCoin(-coin, FinanceType.MallBuy, id))
                {
                    note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallCoinNo));
                    return;
                }

                int n = 0;
                if (wl != null)
                {
                    wl.Add(goodsid);
                    n = wl.Count;
                }
                else
                {
                    wv["WardrobeList"] = new List<string>() { goodsid };
                    n = 1;
                }

                if (!wx.Save())
                {
                    note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallSuccess));
                    return;
                }



                note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(wx));
                note.Call(MallCommand.BuyMallGoodsR, true, TipManager.GetMessage(MallReturn.MallSuccess));


                //购买时装日志
                Variant os = new Variant();
                os["IsMode"] = 0;
                os["ShiZhuang"] = goodsid;
                os["Coin"] = -coin;                
                note.Player.AddLogVariant(Actiontype.MallShiZhuang, null, null, os);

                note.Player.FinishNote(FinishCommand.Wardrobe, n);
                note.Player.AddLog(Actiontype.MallShiZhuang, goodsid, 1, GoodsSource.MallCoin, id, coin);                
                return;
            }



            int sc = BurdenManager.StactCount(gid);
            if (sc <= 0)
            {
                note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallGoodsNo));
                return;
            }

            int isbinding = gc.Value.GetIntOrDefault("IsBinding");

            int zcoin = 0;
            PlayerEx burden = note.Player.B0;
            Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
            Variant t = new Variant();

            if (cointype == "Coin")
            {
                //拆扣晶币
                zcoin = gc.Value.GetIntOrDefault("ZCoin");
                t.Add("Number" + isbinding, number * count);
            }
            else if (cointype == "Bond")
            {
                zcoin = gc.Value.GetIntOrDefault("ZBond");
                t.Add("Number" + isbinding, number * count);
            }
            else if (cointype == "FightValue")
            {
                zcoin = gc.Value.GetIntOrDefault("ZFightValue");
                t.Add("Number" + isbinding, number * count);
            }
            //需要晶币/点券的总量
            int cost = zcoin * number;
            if (cost <= 0 || number <= 0 || zcoin <= 0)
            {
                note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallCoinTypeNo));
                return;
            }
            GoodsAccess.Instance.TimeLines(gid, t);
            dic.Add(goodsid, t);

            if (BurdenManager.IsFullBurden(burden, dic))
            {
                note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.BurdenFull));
                return;
            }
            GoodsSource gs;
            if (cointype == "Coin")
            {
                if (note.Player.Coin < cost || (!note.Player.AddCoin(-cost, FinanceType.MallBuy, id)))
                {
                    note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallCoinNo));
                    return;
                }
                gs = GoodsSource.MallCoin;
            }
            else if (cointype == "Bond")
            {
                if (note.Player.Bond < cost || (!note.Player.AddBond(-cost, FinanceType.MallBuy, id)))
                {
                    note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallBondNo));
                    return;
                }
                gs = GoodsSource.MallBond;
            }
            else if (cointype == "FightValue")
            {
                if (note.Player.FightValue < cost || (!note.Player.AddFightValue(-cost, true, FinanceType.MallBuy, id)))
                {
                    note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallCoinNo));
                    return;
                }
                gs = GoodsSource.MallFightValue;
            }
            else
            {
                note.Call(MallCommand.BuyMallGoodsR, false, TipManager.GetMessage(MallReturn.MallGoodsNo));
                return;
            }
            //购买成功,发送道具
            note.Player.AddGoods(dic, gs, gc.ID, cost);
            note.Call(MallCommand.BuyMallGoodsR, true, TipManager.GetMessage(MallReturn.MallSuccess));
        }
    }
}
