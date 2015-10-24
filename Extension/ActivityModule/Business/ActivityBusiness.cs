using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Util;
using Sinan.Data;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.ActivityModule.Business
{
    /// <summary>
    /// 活跃度奖励
    /// </summary>
    partial class ActivityBusiness
    {

        static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 创建活跃度
        /// </summary>
        /// <param name="note"></param>
        public static void Activity(UserNote note)
        {
            Activity(note.Player);
        }

        /// <summary>
        /// 创建活跃度
        /// </summary>
        /// <param name="pb"></param>
        public static void Activity(PlayerBusiness pb)
        {
            string soleid = pb.ID + "Activity";
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                DateTime dt = DateTime.Now;//登录时间
                PlayerEx ac;
                Variant v;
                if (!pb.Value.TryGetValueT("Activity", out ac))
                {
                    ac = new PlayerEx(pb.ID, "Activity");
                    v = new Variant();
                    for (int i = 1; i < 8; i++)
                    {
                        v.Add(i.ToString(), 0);
                    }
                    //当前登录时间
                    v.Add("LoginTime", dt);//当前登录时间
                    //v.Add("LoginDays",Convert.ToInt32(dt.ToString("yyyyMMdd")));       //上次登录时间
                    //当日在线时间,秒钟
                    v.Add("OnlineTime", 0);
                    //连续登录天数
                    v.Add("Days", 0);
                    //本周第一次签到时间
                    v.Add("Finish", ActivityManager.FinishClear());
                    v.Add("Award", ActivityManager.AwardClear());
                    ac.Value = v;

                    pb.Value.Add("Activity", ac);
                }
                v = ac.Value;

                bool issign = false;
                int n = 0;
                for (int i = 1; i < 8; i++)
                {
                    string m_info = v.GetStringOrDefault(i.ToString());
                    if (m_info == "1")
                    {
                        if (n == 0)
                        {
                            v[i.ToString()] = 0;
                            v["Days"] = 1;
                            v["Finish"] = ActivityManager.FinishClear();
                            v["Award"] = ActivityManager.AwardClear();
                            v["OnlineTime"] = 0;
                        }
                        n++;
                    }



                    if (m_info != "0")
                    {
                        int t = 0;
                        if (int.TryParse(m_info, out t))
                        {
                            v[i.ToString()] = 0;
                            issign = true;
                            continue;
                        }

                        DateTime st = v.GetDateTimeOrDefault(i.ToString()).ToLocalTime();

                        if (!ActivityManager.LocalWeek(st, DateTime.Now))
                        {
                            //如果不是同周
                            v[i.ToString()] = 0;
                            issign = true;
                        }
                    }
                }

                if (issign && v.ContainsKey("Sign"))
                {
                    v["Sign"] = new List<int>();
                }

                //上一次调的时间
                DateTime updt = v.GetDateTimeOrDefault("LoginTime").ToLocalTime();


                //当前时间
                v["LoginTime"] = dt;
                if (!v.ContainsKey("Vip"))
                {
                    v["Vip"] = ActivityManager.VipClear();
                }
                if ((dt.Date - updt.Date).TotalDays == 1)
                {
                    //记录总的活跃度
                    v.SetOrInc("Total", CurActivity(pb));
                    v.SetOrInc("Days", 1);
                    v["IsDay"] = 0;
                    v["IsVIP"] = 0;
                    v["OnlineTime"] = 0;
                    v["Finish"] = ActivityManager.FinishClear();
                    v["Award"] = ActivityManager.AwardClear();
                    v["Vip"] = ActivityManager.VipClear();
                    v["OnlineAward"] = new Variant();
                }
                else if ((dt.Date - updt.Date).TotalDays > 1)
                {
                    //记录总的活跃度
                    v.SetOrInc("Total", CurActivity(pb));
                    v["Days"] = 1;
                    v["IsDay"] = 0;
                    v["IsVIP"] = 0;
                    v["OnlineTime"] = 0;
                    v["Finish"] = ActivityManager.FinishClear();
                    v["Award"] = ActivityManager.AwardClear();
                    v["Vip"] = ActivityManager.VipClear();
                    v["OnlineAward"] = new Variant();
                }
                else
                {
                    //当日登录
                    Dictionary<string, Variant> d = ActivityManager.GetFinish();
                    Variant finish = v.GetValueOrDefault<Variant>("Finish");
                    if (finish == null)
                    {
                        //记录总的活跃度
                        v.SetOrInc("Total", CurActivity(pb));
                        v["Finish"] = ActivityManager.FinishClear();
                        v["OnlineAward"] = new Variant();
                    }
                    else
                    {
                        foreach (string key in d.Keys)
                        {
                            if (finish.ContainsKey(key))
                                continue;
                            finish.Add(key, 0);
                        }
                    }
                }
                ac.Save();

                pb.Call(ClientCommand.UpdateActorR, new PlayerExDetail(ac));
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 每日签到
        /// </summary>
        /// <param name="note"></param>
        public static void Sign(UserNote note)
        {
            PlayerEx ac;
            if (!note.Player.Value.TryGetValueT("Activity", out ac))
                return;

            Variant v = ac.Value;

            string week = Week();
            if (v.ContainsKey(week))
            {
                if (v.GetStringOrDefault(week) == "0")
                {
                    v[week] = DateTime.UtcNow;
                }
                else
                {
                    note.Player.Call(ActivityCommand.ActivitySignR, false, TipManager.GetMessage(ActivityReturn.SignMsg1));
                    return;
                }
            }
            if (ac.Save())
            {
                note.Player.AddAcivity(ActivityType.Sign, 1);
                note.Player.Call(ActivityCommand.ActivitySignR, true, TipManager.GetMessage(ActivityReturn.SignMsg2));
                note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(ac));
            }
        }

        /// <summary>
        /// 签到奖励
        /// </summary>
        /// <param name="note"></param>
        public static void SignAward(UserNote note)
        {
            string sole = note.PlayerID + "SignAward";
            if (!m_dic.TryAdd(sole, sole))
                return;

            try
            {
                int p = note.GetInt32(0);
                PlayerEx ac;
                if (!note.Player.Value.TryGetValueT("Activity", out ac))
                {
                    note.Player.Call(ActivityCommand.SignAwardR, false, TipManager.GetMessage(ActivityReturn.SignAward1));
                    return;
                }

                Variant v = ac.Value;
                //得到签到次数
                int count = 0;//得到本周签到次数
                for (int i = 1; i < 8; i++)
                {
                    object o;
                    if (v.TryGetValue(i.ToString(), out o))
                    {
                        if (o is DateTime)
                        {
                            count++;
                        }
                    }

                    //if (v.ContainsKey(i.ToString()))
                    //{

                    //    if (v.GetIntOrDefault(i.ToString()) != 0 && v.GetIntOrDefault(i.ToString()) != 1)
                    //    {
                    //        count++;
                    //    }
                    //}
                }
                if (count < p)
                {
                    note.Player.Call(ActivityCommand.SignAwardR, false, TipManager.GetMessage(ActivityReturn.SignAward2));
                    return;
                }
                Dictionary<int, string> dic = ActivityManager.GetSignAward();

                string goodsid;
                if (dic.TryGetValue(p, out goodsid))
                {
                    IList sign = v.GetValueOrDefault<IList>("Sign");
                    if (sign != null)
                    {
                        if (sign.Contains(p))
                        {
                            note.Player.Call(ActivityCommand.SignAwardR, false, TipManager.GetMessage(ActivityReturn.SignAward3));
                            return;
                        }
                    }

                    Dictionary<string, Variant> goods = new Dictionary<string, Variant>();
                    Variant tmp = new Variant();
                    tmp.Add("Number1", 1);
                    goods.Add(goodsid, tmp);
                    if (BurdenManager.IsFullBurden(note.Player.B0, goods))
                    {
                        note.Player.Call(ActivityCommand.SignAwardR, false, TipManager.GetMessage(ActivityReturn.SignAward4));
                        return;
                    }
                    if (sign == null)
                    {
                        v["Sign"] = new List<int> { p };
                        //v.Add("Sign", new List<int> { p });                        
                    }
                    else
                    {
                        sign.Add(p);
                    }
                    if (ac.Save())
                    {
                        note.Player.AddGoods(goods, GoodsSource.SignAward);
                        note.Call(ActivityCommand.SignAwardR, true, TipManager.GetMessage(ActivityReturn.SignAward6));
                        note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(ac));
                    }
                }
                else
                {
                    note.Player.Call(ActivityCommand.SignAwardR, false, TipManager.GetMessage(ActivityReturn.SignAward5));
                    return;
                }
            }
            finally
            {
                m_dic.TryRemove(sole, out sole);
            }
        }


        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="note"></param>
        public static void UserDisconnected(UserNote note)
        {
            PlayerEx ac;
            if (note.Player.Value.TryGetValueT("Activity", out ac))
            {
                //退出时间
                DateTime CurDate = DateTime.Now;
                Variant v = ac.Value;
                //上次登录时间
                DateTime dt = v.GetDateTimeOrDefault("LoginTime");
                if ((CurDate.Date - dt.Date).TotalDays >= 1)
                {
                    v["OnlineTime"] = TotalSeconds(CurDate, CurDate.Date);
                }
                else
                {
                    v["OnlineTime"] = v.GetDoubleOrDefault("OnlineTime") + TotalSeconds(CurDate, dt);
                }

                ac.Save();
            }
        }

        /// <summary>
        /// 活跃领奖
        /// </summary>
        /// <param name="note"></param>
        public static void ActivityAward(UserNote note)
        {
            string soleid = note.PlayerID + "ActivityAward"; ;
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                //奖励需要活跃度
                int value = note.GetInt32(0);
                PlayerEx ac;
                if (!note.Player.Value.TryGetValueT("Activity", out ac))
                {
                    note.Call(ActivityCommand.ActivityAwardR, false, TipManager.GetMessage(ActivityReturn.ActivityAward1));
                    return;
                }
                Variant v = ac.Value;
                Variant award = v.GetValueOrDefault<Variant>("Award");

                //Dictionary<int, string> d = ActivityAccess.GetAward();
                Dictionary<int, string> d = ActivityManager.GetAward();
                if (!d.ContainsKey(value))
                {
                    note.Call(ActivityCommand.ActivityAwardR, false, TipManager.GetMessage(ActivityReturn.ActivityAward2));
                    return;
                }
                if (award.GetIntOrDefault(value.ToString()) > 0)
                {
                    note.Call(ActivityCommand.ActivityAwardR, false, TipManager.GetMessage(ActivityReturn.ActivityAward3));
                    return;
                }
                //完成度
                Variant fm = v.GetValueOrDefault<Variant>("Finish");
                //当日在线时间
                int OnlineTime = v.GetIntOrDefault("OnlineTime") + Convert.ToInt32((DateTime.UtcNow - note.Player.LoginTime).TotalSeconds);

                int cur = 0;//当前活跃度
                Dictionary<string, Variant> dic = ActivityManager.GetFinish();
                foreach (Variant k in dic.Values)
                {
                    string id = k.GetStringOrDefault("id");
                    int fv = k.GetIntOrDefault("finish");//需要完成度

                    if (k.GetIntOrDefault("type") == 1)
                    {
                        if (OnlineTime > fv * 60)
                        {
                            cur += k.GetIntOrDefault("value");
                        }
                    }
                    else
                    {
                        if (fm.ContainsKey(id))
                        {
                            if (fm.GetIntOrDefault(id) >= fv)
                            {
                                cur += k.GetIntOrDefault("value");
                            }
                        }
                    }
                }
                if (cur < value)
                {
                    note.Call(ActivityCommand.ActivityAwardR, false, TipManager.GetMessage(ActivityReturn.ActivityAward4));

                    return;
                }


                string goodsid = string.Empty;
                if (d.TryGetValue(value, out goodsid))
                {
                    Dictionary<string, Variant> goods = new Dictionary<string, Variant>();
                    Variant tmp = new Variant();
                    tmp.Add("Number1", 1);
                    goods.Add(goodsid, tmp);

                    if (BurdenManager.IsFullBurden(note.Player.B0, goods))
                    {
                        note.Call(ActivityCommand.ActivityAwardR, false, TipManager.GetMessage(ActivityReturn.ActivityAward5));
                        return;
                    }
                    //添加道具                    
                    award[value.ToString()] = 1;
                    if (ac.Save())
                    {
                        note.Player.AddGoods(goods, GoodsSource.ActivityAward);
                        note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(ac));
                        note.Call(ActivityCommand.ActivityAwardR, true, TipManager.GetMessage(ActivityReturn.ActivityAward8));
                    }
                    else
                    {
                        note.Call(ActivityCommand.ActivityAwardR, false, TipManager.GetMessage(ActivityReturn.ActivityAward6));
                    }
                }
                else
                {
                    note.Call(ActivityCommand.ActivityAwardR, false, TipManager.GetMessage(ActivityReturn.ActivityAward7));
                }
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 登录奖励
        /// </summary>
        /// <param name="note"></param>
        public static void LoginAward(UserNote note)
        {
            string soleid = note.PlayerID + "LoginAward";
            if (!m_dic.TryAdd(soleid, soleid))
            {
                return;
            }
            try
            {
                PlayerEx ac;
                if (!note.Player.Value.TryGetValueT("Activity", out ac))
                {
                    note.Player.Call(ActivityCommand.LoginAwardR, false, TipManager.GetMessage(ActivityReturn.LoginAward1));
                    return;
                }

                Variant v = ac.Value;
                //连续登录天数
                int days = v.GetIntOrDefault("Days");
                Dictionary<int, string> dic = ActivityManager.GetDayAward();
                int max = 0;//最大天数

                foreach (int key in dic.Keys)
                {
                    max = max > key ? max : key;
                }

                int m = days >= max ? max : days;


                string goodsid;
                if (dic.TryGetValue(m, out goodsid))
                {
                    //如果存在
                    if (goodsid == "0")
                    {
                        note.Player.Call(ActivityCommand.LoginAwardR, false, TipManager.GetMessage(ActivityReturn.LoginAward2));
                        return;
                    }
                    int isday = v.GetIntOrDefault("IsDay");
                    if (isday >= 1)
                    {
                        note.Player.Call(ActivityCommand.LoginAwardR, false, TipManager.GetMessage(ActivityReturn.LoginAward3));
                        return;
                    }

                    Dictionary<string, Variant> goods = new Dictionary<string, Variant>();
                    Variant tmp = new Variant();
                    tmp.Add("Number1", 1);
                    goods.Add(goodsid, tmp);
                    if (BurdenManager.IsFullBurden(note.Player.B0, goods))
                    {
                        note.Call(ActivityCommand.LoginAwardR, false, TipManager.GetMessage(ActivityReturn.LoginAward4));

                        return;
                    }
                    v.SetOrInc("IsDay", 1);
                    if (ac.Save())
                    {
                        //添加道具
                        note.Player.AddGoods(goods, GoodsSource.LoginAward);
                        note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(ac));
                        note.Player.Call(ActivityCommand.LoginAwardR, true, TipManager.GetMessage(ActivityReturn.LoginAward6));
                    }
                }
                else
                {
                    note.Player.Call(ActivityCommand.LoginAwardR, false, TipManager.GetMessage(ActivityReturn.LoginAward5));
                }
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// VIP每日登录奖励
        /// </summary>
        /// <param name="note"></param>
        public static void VIPDayAward(UserNote note)
        {
            string soleid = note.PlayerID + "VIPDayAward";
            if (!m_dic.TryAdd(soleid, soleid))
            {
                return;
            }

            try
            {
                //note.Player.IsVIP();
                //if (note.Player.VIP != 1)
                //{
                //    note.Call(ActivityCommand.VIPDayAwardR, false, TipManager.GetMessage(ActivityReturn.VIPDayAward1));
                //    return;
                //}

                PlayerEx ac;
                if (!note.Player.Value.TryGetValueT("Activity", out ac))
                {
                    note.Player.Call(ActivityCommand.VIPDayAwardR, false, TipManager.GetMessage(ActivityReturn.VIPDayAward2));
                    return;
                }

                Variant v = ac.Value;
                if (v == null)
                {
                    note.Call(ActivityCommand.VIPDayAwardR, false, TipManager.GetMessage(ActivityReturn.VIPDayAward4));
                    return;
                }
                int isvip = v.GetIntOrDefault("IsVIP");
                if (isvip == 1)
                {
                    note.Call(ActivityCommand.VIPDayAwardR, false, TipManager.GetMessage(ActivityReturn.VIPDayAward4));
                    return;
                }

                Dictionary<int, string> dayvip = ActivityManager.GetDayVipAward();
                
                string goodsid = "";
                if (!dayvip.TryGetValue(note.Player.MLevel, out goodsid))
                {
                    note.Player.Call(ActivityCommand.VIPDayAwardR, false, TipManager.GetMessage(ActivityReturn.VIPDayAward1));
                    return;
                }

                if (string.IsNullOrEmpty(goodsid))
                {
                    note.Player.Call(ActivityCommand.VIPDayAwardR, false, TipManager.GetMessage(ActivityReturn.VIPDayAward1));
                    return;
                }

                Dictionary<string, Variant> goods = new Dictionary<string, Variant>();
                Variant tmp = new Variant();
                tmp.Add("Number1", 1);
                goods.Add(goodsid, tmp);

                if (BurdenManager.IsFullBurden(note.Player.B0, goods))
                {
                    note.Call(ActivityCommand.VIPDayAwardR, false, TipManager.GetMessage(ActivityReturn.VIPDayAward3));
                    return;
                }



                v.SetOrInc("IsVIP", 1);
                if (ac.Save())
                {
                    //添加道具
                    note.Player.AddGoods(goods, GoodsSource.VIPDayAward);
                    note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(ac));
                    note.Call(ActivityCommand.VIPDayAwardR, true, TipManager.GetMessage(ActivityReturn.VIPDayAward6));
                }
                else
                {
                    note.Call(ActivityCommand.VIPDayAwardR, false, TipManager.GetMessage(ActivityReturn.VIPDayAward5));
                }
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// VIP兑换
        /// </summary>
        /// <param name="note"></param>
        public static void VipExchange(UserNote note)
        {
            string soleid = note.PlayerID + "VipExchange"; ;
            if (!m_dic.TryAdd(soleid, soleid))
            {
                return;
            }
            try
            {
                note.Player.IsVIP();
                if (note.Player.VIP != 1)
                {
                    note.Call(ActivityCommand.VipExchangeR, false, TipManager.GetMessage(ActivityReturn.VipExchange1));
                    return;
                }

                string id = note.GetString(0);
                Dictionary<string, Variant> dic = ActivityManager.GetVipAward();
                Variant tmp;
                if (!dic.TryGetValue(id, out tmp))
                {
                    note.Call(ActivityCommand.VipExchangeR, false, TipManager.GetMessage(ActivityReturn.VipExchange2));
                    return;
                }

                PlayerEx ac;
                if (!note.Player.Value.TryGetValueT("Activity", out ac))
                {
                    note.Call(ActivityCommand.VipExchangeR, false, TipManager.GetMessage(ActivityReturn.VipExchange2));
                    return;
                }
                Variant v = ac.Value;
                if (v == null)
                {
                    note.Call(ActivityCommand.VipExchangeR, false, TipManager.GetMessage(ActivityReturn.VipExchange2));
                    return;
                }
                Variant vip = v.GetValueOrDefault<Variant>("Vip");
                if (vip == null)
                {
                    note.Call(ActivityCommand.VipExchangeR, false, TipManager.GetMessage(ActivityReturn.VipExchange2));
                    return;
                }

                if (vip.GetIntOrDefault(id) >= tmp.GetIntOrDefault("number"))
                {
                    note.Call(ActivityCommand.VipExchangeR, false, TipManager.GetMessage(ActivityReturn.VipExchange3));
                    return;
                }

                string goodsid = tmp.GetStringOrDefault("goodsid");
                if (string.IsNullOrEmpty(goodsid))
                {
                    note.Call(ActivityCommand.VipExchangeR, false, TipManager.GetMessage(ActivityReturn.VipExchange2));
                    return;
                }

                string vipid = tmp.GetStringOrDefault("vipid");
                int count = tmp.GetIntOrDefault("count");
                if (string.IsNullOrEmpty(vipid) || count <= 0)
                {
                    note.Call(ActivityCommand.VipExchangeR, false, TipManager.GetMessage(ActivityReturn.VipExchange2));
                    return;
                }

                Dictionary<string, Variant> goods = new Dictionary<string, Variant>();
                Variant m = new Variant();
                m.Add("Number1", 1);
                goods.Add(goodsid, m);

                if (BurdenManager.IsFullBurden(note.Player.B0, goods))
                {
                    note.Call(ActivityCommand.VipExchangeR, false, TipManager.GetMessage(ActivityReturn.VipExchange4));
                    return;
                }

                if (note.Player.RemoveGoods(vipid, count, GoodsSource.VipExchange))
                {
                    note.Player.AddGoods(goods, GoodsSource.VipExchange);
                    vip.SetOrInc(id, 1);
                    if (ac.Save())
                    {
                        note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(ac));
                        note.Call(ActivityCommand.VipExchangeR, true, TipManager.GetMessage(ActivityReturn.VipExchange7));
                    }
                    else
                    {
                        note.Call(ActivityCommand.VipExchangeR, false, TipManager.GetMessage(ActivityReturn.VipExchange6));
                    }
                }
                else
                {
                    note.Call(ActivityCommand.VipExchangeR, false, TipManager.GetMessage(ActivityReturn.VipExchange5));
                }
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 在线奖励
        /// </summary>
        /// <param name="note"></param>
        public static void OnlineAward(UserNote note)
        {
            PlayerBusiness pb = note.Player;
            string mouth = note.GetString(0);

            int m = 0;
            if (!int.TryParse(mouth, out m))
            {
                //参数不对
                pb.Call(ActivityCommand.OnlineAwardR, false, TipManager.GetMessage(ActivityReturn.OnlineAward1));
                return;
            }

            if (!OnlineTime(pb, m))
            {
                //在线时间不足
                pb.Call(ActivityCommand.OnlineAwardR, false, TipManager.GetMessage(ActivityReturn.OnlineAward2));
                return;
            }


            GameConfig gc = GameConfigAccess.Instance.FindOneById("OL_000001");
            if (gc == null)
            {
                //配置有问题
                pb.Call(ActivityCommand.OnlineAwardR, false, TipManager.GetMessage(ActivityReturn.OnlineAward3));
                return;
            }

            Variant v = gc.Value;
            if (v == null)
            {
                //配置问题
                pb.Call(ActivityCommand.OnlineAwardR, false, TipManager.GetMessage(ActivityReturn.OnlineAward3));
                return;
            }

            IList list = v.GetValue<IList>(mouth);

            Variant info = null;
            foreach (Variant k in list)
            {
                string[] level = k.GetStringOrDefault("Level").Split('-');
                if (level.Length != 2)
                    continue;
                int min = Convert.ToInt32(level[0]);
                int max = Convert.ToInt32(level[1]);
                if (pb.Level >= min && pb.Level <= max)
                {
                    info = k;
                    break;
                }
            }
            if (info == null)
            {
                pb.Call(ActivityCommand.OnlineAwardR, false, TipManager.GetMessage(ActivityReturn.OnlineAward3));
                return;
            }
            PlayerEx ex = pb.Value["Activity"] as PlayerEx;
            if (ex == null)
            {
                pb.Call(ActivityCommand.OnlineAwardR, false, TipManager.GetMessage(ActivityReturn.OnlineAward3));
                return;
            }
            Variant oa = ex.Value.GetVariantOrDefault("OnlineAward");
            if (oa != null)
            {
                if (oa.ContainsKey(mouth.ToString()))
                {
                    //已经领取
                    pb.Call(ActivityCommand.OnlineAwardR, false, TipManager.GetMessage(ActivityReturn.OnlineAward4));
                    return;
                }
            }

            string goodsid = info.GetStringOrDefault("GoodsID");
            GameConfig gc1 = GameConfigAccess.Instance.FindOneById(goodsid);
            if (gc1 == null)
            {
                //道具配置有问题
                pb.Call(ActivityCommand.OnlineAwardR, false, TipManager.GetMessage(ActivityReturn.OnlineAward3));
                return;
            }


            int number = info.GetIntOrDefault("Number");
            int isbinding = info.GetIntOrDefault("IsBinding");

            Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
            Variant goods = new Variant();
            goods.Add("Number" + isbinding, number);
            dic.Add(goodsid, goods);
            if (BurdenManager.IsFullBurden(pb.B0, dic))
            {
                //包袱满,不能进行该操作
                pb.Call(ActivityCommand.OnlineAwardR, false, TipManager.GetMessage(ActivityReturn.OnlineAward5));
                return;
            }
            if (oa == null)
            {
                ex.Value["OnlineAward"] = new Variant();
                oa = ex.Value.GetVariantOrDefault("OnlineAward");
            }
            oa[mouth] = DateTime.UtcNow;
            if (!ex.Save())
            {
                pb.Call(ActivityCommand.OnlineAwardR, false, TipManager.GetMessage(ActivityReturn.OnlineAward3));
                return;
            }
            note.Player.AddGoods(dic, GoodsSource.OnlineAward);
            pb.Call(ClientCommand.UpdateActorR, new PlayerExDetail(ex));
            pb.Call(ActivityCommand.OnlineAwardR, true, "");
        }

        /// <summary>
        /// 得到角色当前活跃度
        /// </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        private static int CurActivity(PlayerBusiness pb)
        {
            int cur = 0;//当前活跃度
            PlayerEx ac;
            if (pb.Value.TryGetValueT("Activity", out ac))
            {
                Variant v = ac.Value;
                Variant award = v.GetValueOrDefault<Variant>("Award");
                Dictionary<int, string> d = ActivityManager.GetAward();
                //完成度
                Variant fm = v.GetValueOrDefault<Variant>("Finish");
                //当日在线时间
                int onlineTime = v.GetIntOrDefault("OnlineTime") + Convert.ToInt32((DateTime.UtcNow - pb.LoginTime).TotalSeconds);
                Dictionary<string, Variant> dic = ActivityManager.GetFinish();
                foreach (Variant k in dic.Values)
                {
                    string id = k.GetStringOrDefault("id");
                    int fv = k.GetIntOrDefault("finish");//需要完成度

                    if (k.GetIntOrDefault("type") == 1)
                    {
                        if (onlineTime > fv * 60)
                        {
                            cur += k.GetIntOrDefault("value");
                        }
                    }
                    else
                    {
                        if (fm.ContainsKey(id))
                        {
                            if (fm.GetIntOrDefault(id) >= fv)
                            {
                                cur += k.GetIntOrDefault("value");
                            }
                        }
                    }
                }
            }
            return cur;
        }

        /// <summary>
        /// 得到当天在线时长
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        private static bool OnlineTime(PlayerBusiness pb, int minute)
        {
            int cur = 0;
            PlayerEx ac = pb.Value["Activity"] as PlayerEx;
            if (ac != null)
            {
                Variant v = ac.Value;
                if (v != null)
                {
                    int ot = v.GetIntOrDefault("OnlineTime") + Convert.ToInt32((DateTime.UtcNow - pb.LoginTime).TotalSeconds);
                    cur = Convert.ToInt32(ot / 60);
                }
            }

            return cur >= minute;
        }




        /// <summary>
        /// 得到星期几
        /// </summary>
        /// <returns></returns>
        private static string Week()
        {
            DateTime dt = DateTime.Now;
            string n = "0";
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    n = "1";
                    break;
                case DayOfWeek.Tuesday:
                    n = "2";
                    break;
                case DayOfWeek.Wednesday:
                    n = "3";
                    break;
                case DayOfWeek.Thursday:
                    n = "4";
                    break;
                case DayOfWeek.Friday:
                    n = "5";
                    break;
                case DayOfWeek.Saturday:
                    n = "6";
                    break;
                case DayOfWeek.Sunday:
                    n = "7";
                    break;
            }
            return n;
        }

        /// <summary>
        /// 判断两个时间是否为同周
        /// 两个时间必须大于(1969, 12, 29)
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private static bool SameWeek(DateTime startTime, DateTime endTime)
        {
            DateTime dt = new DateTime(1969, 12, 29);

            TimeSpan ts1 = startTime - dt;
            TimeSpan ts2 = endTime - dt;

            int m1 = 0;
            int n1 = Math.DivRem(Convert.ToInt32(ts1.TotalDays), 7, out m1);

            int m2 = 0;
            int n2 = Math.DivRem(Convert.ToInt32(ts2.TotalDays), 7, out m2);

            if (n1 == n2)
                return false;
            return false;
        }

        /// <summary>
        /// 得到两时间差
        /// </summary>
        /// <param name="EndTime">结束时间</param>
        /// <param name="BeginTime">开始时间</param>
        /// <returns></returns>
        private static int TotalSeconds(DateTime EndTime, DateTime BeginTime)
        {
            TimeSpan ts = EndTime - BeginTime;
            return Convert.ToInt32(ts.TotalSeconds);
        }
    }
}
