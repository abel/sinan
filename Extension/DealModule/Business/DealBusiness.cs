using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Util;

namespace Sinan.DealModule.Business
{
    public class DealBusiness
    {
        /// <summary>
        /// 申请交易
        /// </summary>
        /// <param name="note"></param>
        public static void DealApply(UserNote note)
        {
            string playerid = note.GetString(0);

            if (note.Player.Value.ContainsKey("Deal"))
            {
                note.Call(DealCommand.DealApplyR, false, TipManager.GetMessage(DealReturn.DealOther));
                return;
            }
            PlayerBusiness OnlineBusiness = PlayersProxy.FindPlayerByID(playerid);
            if (OnlineBusiness == null || (!OnlineBusiness.Online))
            {
                note.Call(DealCommand.DealApplyR, false, TipManager.GetMessage(DealReturn.NoLine));
                return;
            }

            if (OnlineBusiness.AState == ActionState.Fight)
            {
                note.Call(DealCommand.DealApplyR, false, TipManager.GetMessage(DealReturn.Fighting));
                return;
            }

            if (OnlineBusiness.Value.ContainsKey("Deal"))
            {
                Variant dy = OnlineBusiness.Value.GetVariantOrDefault("Deal");
                if (dy.GetIntOrDefault("Status") >= 0)
                {
                    note.Call(DealCommand.DealApplyR, false, TipManager.GetMessage(DealReturn.DealOther));
                    return;
                }
            }


            Variant d0 = new Variant();
            d0.Add("Status", 0);
            d0.Add("PlayerID", playerid);//被邀请者
            d0.Add("Goods", null);
            if (!note.Player.Value.ContainsKey("Deal"))
                note.Player.Value.Add("Deal", d0);

            Variant d1 = new Variant();
            d1.Add("Status", 0);
            d1.Add("PlayerID", note.PlayerID);//邀请者
            d1.Add("Goods", null);
            if (!OnlineBusiness.Value.ContainsKey("Deal"))
                OnlineBusiness.Value.Add("Deal", d1);

            OnlineBusiness.Call(DealCommand.DealApplyR, true, note.PlayerID);
            note.Call(DealCommand.DealApplyR, true, note.PlayerID);
        }

        /// <summary>
        /// 回复交易
        /// </summary>
        /// <param name="note"></param>
        public static void DealApplyBack(UserNote note)
        {
            IList o = note[0] as IList;
            bool IsSame = false;

            PlayerBusiness OnlineBusiness = PlayersProxy.FindPlayerByID(o[1].ToString());
            if (OnlineBusiness == null || (!OnlineBusiness.Online))
            {
                note.Call(DealCommand.DealApplyBackR, false, TipManager.GetMessage(DealReturn.NoLine));
                return;
            }

            if (!OnlineBusiness.Value.ContainsKey("Deal"))
            {
                note.Call(DealCommand.DealApplyBackR, false, TipManager.GetMessage(DealReturn.BackMsgError));
                return;
            }
            Variant v0 = note.Player.Value.GetVariantOrDefault("Deal");
            Variant v1 = OnlineBusiness.Value.GetVariantOrDefault("Deal");


            if (!bool.TryParse(o[0].ToString(), out IsSame))
            {
                if (OnlineBusiness.Value.ContainsKey("Deal"))
                    OnlineBusiness.Value.Remove("Deal");
                OnlineBusiness.Call(DealCommand.DealApplyBackR, false, TipManager.GetMessage(DealReturn.NoDeal));

                //note.Call(DealCommand.DealApplyBackR, false, TipAccess.GetMessage(DealReturn.NoDeal));
                return;
            }

            if (!note.Player.Value.ContainsKey("Deal"))
            {
                if (OnlineBusiness.Value.ContainsKey("Deal"))
                    OnlineBusiness.Value.Remove("Deal");
                OnlineBusiness.Call(DealCommand.DealApplyBackR, false, TipManager.GetMessage(DealReturn.NoDeal));
                //note.Call(DealCommand.DealApplyBackR, false, TipAccess.GetMessage(DealReturn.NoDeal));
                return;
            }

            if (!IsSame)
            {
                if (OnlineBusiness.Value.ContainsKey("Deal"))
                    OnlineBusiness.Value.Remove("Deal");
                OnlineBusiness.Call(DealCommand.DealApplyBackR, false, TipManager.GetMessage(DealReturn.NoDeal));

                if (note.Player.Value.ContainsKey("Deal"))
                    note.Player.Value.Remove("Deal");
                //note.Call(DealCommand.DealApplyBackR, false, TipAccess.GetMessage(DealReturn.NoDeal));
                return;
            }

            v0["Status"] = 1;
            v1["Status"] = 1;
            OnlineBusiness.Call(DealCommand.DealApplyBackR, true, DealReturn.AgreeDeal);
            note.Call(DealCommand.DealApplyBackR, true, DealReturn.AgreeDeal);
        }
        /// <summary>
        /// 锁定交易
        /// </summary>
        /// <param name="note"></param>
        public static void LockDeal(UserNote note)
        {
            Variant goods = note.GetVariant(0);
            int Score = goods.GetIntOrDefault("Score");
            //int Coin = goods.GetIntOrDefault("Coin");

            if (note.Player.Score < Score)
            {
                note.Call(DealCommand.LockDealR, false, TipManager.GetMessage(DealReturn.NoScore));
                return;
            }

            //if (note.Player.Coin < Coin)
            //{
            //    note.Call(DealCommand.LockDealR, false, TipManager.GetMessage(DealReturn.NoCoin));
            //    return;
            //}

            IList gs = goods.GetValue<IList>("Goods");
            if (!note.Player.Value.ContainsKey("Deal"))
            {
                note.Call(DealCommand.LockDealR, false, TipManager.GetMessage(DealReturn.StopDeal));
                return;
            }

            Variant d = note.Player.Value.GetVariantOrDefault("Deal");

            PlayerBusiness OnLineBusiness = PlayersProxy.FindPlayerByID(d.GetStringOrDefault("PlayerID"));
            if (OnLineBusiness == null || (!OnLineBusiness.Online))
            {
                note.Player.Value.Remove("Deal");
                note.Call(DealCommand.LockDealR, false, TipManager.GetMessage(DealReturn.NoLine));
                return;
            }
            //接收物品的一方
            PlayerEx burdenOther = OnLineBusiness.B0;
            if (BurdenManager.BurdenDealFull(burdenOther, gs))
            {
                OnLineBusiness.Call(DealCommand.LockDealR, false, TipManager.GetMessage(DealReturn.LockBurdenFull_0));
                note.Call(DealCommand.LockDealR, false, TipManager.GetMessage(DealReturn.LockBurdenFull_1));
                return;
            }

            //对方包袱已满，请先整理才能交易            
            PlayerEx burden = note.Player.B0;
            IList c = burden.Value.GetValue<IList>("C");

            if (!IsCheck(gs, c, note.Player))
                return;

            d["Goods"] = goods;
            d["Status"] = 2;
            Variant deal = OnLineBusiness.Value.GetVariantOrDefault("Deal");
            OnLineBusiness.Call(DealCommand.LockDealR, true, goods);
            note.Call(DealCommand.LockDealR, true, 0);
        }

        /// <summary>
        /// 确定交易
        /// </summary>
        /// <param name="note"></param>
        public static void EnterDeal(UserNote note)
        {

            Variant deal0 = note.Player.Value.GetVariantOrDefault("Deal");
            if (deal0 == null)
            {
                note.Call(DealCommand.EnterDealR, false, TipManager.GetMessage(DealReturn.IsDeal));
                return;
            }

            PlayerBusiness pb = PlayersProxy.FindPlayerByID(deal0.GetStringOrDefault("PlayerID"));
            if (pb == null || (!pb.Online))
            {
                note.Player.Value.Remove("Deal");
                note.Call(DealCommand.EnterDealR, false, TipManager.GetMessage(DealReturn.NoLine));
                return;
            }
            //对家
            Variant deal1 = pb.Value.GetVariantOrDefault("Deal");
            if (deal1 == null)
            {
                note.Call(DealCommand.EnterDealR, false, TipManager.GetMessage(DealReturn.IsDeal));
                return;
            }
            if (deal1.GetIntOrDefault("Status") != 3)
            {
                deal0["Status"] = 3;
                pb.Call(DealCommand.EnterDealR, true, note.PlayerID);
                note.Call(DealCommand.EnterDealR, true, note.PlayerID);
                return;
            }

            Variant v0 = deal0.GetVariantOrDefault("Goods");

            Variant v1 = deal1.GetVariantOrDefault("Goods");

            PlayerEx burden0 = note.Player.B0;
            IList c0 = burden0.Value.GetValue<IList>("C");



            PlayerEx burden1 = pb.B0;
            IList c1 = burden1.Value.GetValue<IList>("C");

            //交易双方的道具信息
            IList goods0 = v0.GetValue<IList>("Goods");
            IList goods1 = v1.GetValue<IList>("Goods");

            if (!IsCheck(goods0, c0, note.Player))
            {
                return;
            }

            if (!IsCheck(goods1, c1, pb))
            {
                return;
            }

            //判断包袱是否已经满
            if (BurdenManager.BurdenDealFull(burden0, goods1))
            {
                pb.Call(DealCommand.EnterDealR, false, TipManager.GetMessage(DealReturn.BurdenFull));
                return;
            }

            //判断包袱是否已经满
            if (BurdenManager.BurdenDealFull(burden1, goods0))
            {
                note.Call(DealCommand.EnterDealR, false, TipManager.GetMessage(DealReturn.BurdenFull));
                return;
            }
            //int Coin = v0.GetIntOrDefault("Coin") - v1.GetIntOrDefault("Coin");
            int Score = v0.GetIntOrDefault("Score") - v1.GetIntOrDefault("Score");

            if (!note.Player.AddScore(-Score, FinanceType.EnterDeal))
            {
                note.Call(DealCommand.EnterDealR, false, TipManager.GetMessage(DealReturn.NoScore));
                return;
            }

            pb.AddScore(Score, FinanceType.EnterDeal);


            //移除成功后
            if (!BurdenManager.Remove(burden0, goods0))
            {
                note.Call(DealCommand.EnterDealR, false, TipManager.GetMessage(DealReturn.NumberError));
                return;
            }

            //移除成功后
            if (!BurdenManager.Remove(burden1, goods1))
            {
                note.Call(DealCommand.EnterDealR, false, TipManager.GetMessage(DealReturn.NumberError));
                return;
            }

            BurdenManager.BurdenInsert(burden0, goods1);
            BurdenManager.BurdenInsert(burden1, goods0);

            foreach (Variant v in goods1)
            {
                //新加道具
                note.Player.UpdateTaskGoods(v.GetStringOrDefault("G"));
                //移除的道具
                pb.UpdateTaskGoods(v.GetStringOrDefault("G"));

                //面对交易日志
                PlayerLog log = new PlayerLog(ServerLogger.zoneid, Actiontype.AddGoods);
                log.itemcnt = v.GetIntOrDefault("Count");
                log.itemtype = v.GetStringOrDefault("G");
                log.touid = note.PID;//
                log.toopenid = pb.UserID;
                log.reserve_1 = (int)(FinanceType.EnterDeal);
                log.remark = pb.PID.ToString();//交易者

                note.Player.WriteLog(log);
            }



            foreach (Variant v in goods0)
            {
                //新加道具
                note.Player.UpdateTaskGoods(v.GetStringOrDefault("G"));
                //移除的道具
                pb.UpdateTaskGoods(v.GetStringOrDefault("G"));


                //面对交易日志
                PlayerLog log1 = new PlayerLog(ServerLogger.zoneid, Actiontype.AddGoods);
                log1.itemcnt = v.GetIntOrDefault("Count");
                log1.itemtype = v.GetStringOrDefault("G");
                log1.touid = pb.PID;
                log1.toopenid = note.Player.UserID;
                log1.reserve_1 = (int)(FinanceType.EnterDeal);
                log1.remark = note.PID.ToString();
                pb.WriteLog(log1);
            }


            if (note.Player.Value.ContainsKey("Deal"))
            {
                note.Player.Value.Remove("Deal");
            }

            if (pb.Value.ContainsKey("Deal"))
            {
                pb.Value.Remove("Deal");
            }


            //Variant list0 = new Variant();
            //list0.Add("B0",burden0);
            //note.Call(BurdenCommand.BurdenListR, list0);
            note.Player.UpdateBurden();

            note.Call(DealCommand.EnterDealR, true, note.PlayerID);

            //通知更新另一位的包袱
            //Variant list1 = new Variant();
            //list1.Add("B0",burden1);
            //OnLineBusiness.Call(BurdenCommand.BurdenListR, list1);
            pb.UpdateBurden();

            pb.Call(DealCommand.EnterDealR, true, note.PlayerID);
        }

        /// <summary>
        /// 退出交易
        /// </summary>
        /// <param name="note"></param>
        public static void ExitDeal(UserNote note)
        {
            if (note.Player.Value.ContainsKey("Deal"))
            {
                Variant v = note.Player.Value.GetVariantOrDefault("Deal");
                note.Player.Value.Remove("Deal");
                PlayerBusiness OnlineBusiness = PlayersProxy.FindPlayerByID(v.GetStringOrDefault("PlayerID"));
                if (OnlineBusiness == null || (!OnlineBusiness.Online))
                {
                    note.Call(DealCommand.ExitDealR);
                    return;
                }

                if (OnlineBusiness.Value.ContainsKey("Deal"))
                {
                    OnlineBusiness.Value.Remove("Deal");
                }

                OnlineBusiness.Call(DealCommand.ExitDealR);
                note.Call(DealCommand.ExitDealR);
            }
        }

        /// <summary>
        /// 判断道具是否合法
        /// </summary>
        /// <param name="gs"></param>
        /// <param name="c"></param>
        /// <param name="pb"></param>
        /// <returns></returns>
        private static bool IsCheck(IList gs, IList c, PlayerBusiness pb)
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();
            foreach (Variant n in gs)
            {
                foreach (Variant k in c)
                {
                    int p = n.GetIntOrDefault("P");
                    if (p == k.GetIntOrDefault("P"))
                    {
                        if (k.GetIntOrDefault("H") == 1)
                        {
                            pb.Call(DealCommand.LockDealR, false, TipManager.GetMessage(DealReturn.IsBinding));
                            return false;
                        }

                        if (n.GetStringOrDefault("ID") != k.GetStringOrDefault("E"))
                        {
                            pb.Call(DealCommand.LockDealR, false, TipManager.GetMessage(DealReturn.NumberError));
                            return false;
                        }

                        if (n.GetIntOrDefault("Count") != k.GetIntOrDefault("A"))
                        {
                            pb.Call(DealCommand.LockDealR, false, TipManager.GetMessage(DealReturn.NumberError));
                            return false;
                        }


                        Variant t = k.GetVariantOrDefault("T");
                        if (t != null)
                        {
                            if (t.ContainsKey("EndTime"))
                            {
                                if (t.GetDateTimeOrDefault("EndTime") < DateTime.UtcNow)
                                {
                                    pb.Call(DealCommand.LockDealR, false, TipManager.GetMessage(DealReturn.IsCheck1));
                                    return false;
                                }
                            }
                        }

                        if (dic.ContainsKey(p))
                        {
                            pb.Call(DealCommand.LockDealR, false, TipManager.GetMessage(DealReturn.IsCheck2));
                            return false;
                        }
                        dic.Add(p, p);
                        break;
                    }
                }
            }
            return true;
        }
    }
}
