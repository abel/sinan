using System;
using System.Collections;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.GoodsModule.Business
{
    public class GoodsBusiness
    {
        /// <summary>
        /// 移除过期坐骑和时装
        /// </summary>
        /// <param name="note"></param>
        public static PlayerEx RemoveEquips(UserNote note)
        {
            return RemoveEquips(note.Player);
        }

        /// <summary>
        /// 检查坐骑和时装是否过期
        /// </summary>
        /// <param name="pb"></param>
        public static PlayerEx RemoveEquips(PlayerBusiness pb)
        {
            if (pb == null) 
                return null;
            Variant shengTi = RoleManager.Instance.GetAllRoleConfig(pb.RoleID);
            PlayerEx panel = pb.Equips;
            if (panel == null) 
                return null;
            string[] strs = new string[] { "P10", "P11" };
            bool isexpired = false;
            for (int i = 0; i < strs.Length; i++)
            {
                if (panel.Value == null)
                    continue;

                Variant v;
                if (panel.Value.TryGetValueT(strs[i], out v))
                {
                    string soleID = v.GetStringOrDefault("E");
                    if (string.IsNullOrEmpty(soleID))
                        continue;
                    Goods g = GoodsAccess.Instance.FindOneById(soleID);
                    if (g == null || (!GoodsIsExpired(pb, g, strs[i])))
                        continue;

                    isexpired = true;
                    string name = strs[i] == "P10" ? "Mount" : "Coat";
                    string value = shengTi.GetStringOrDefault(name);

                    if (name == "Mount")
                    {
                        pb.Mount = value;
                    }
                    else
                    {
                        pb.Coat = value;
                    }
                    pb.RefreshPlayer(name, value);
                    BurdenManager.BurdenClear(v);
                    //日志记录
                    pb.RemoveEquips(g.GoodsID, GoodsSource.RemoveEquips);
                    
                }
            }

            if (isexpired)
            {
                panel.Save();
                pb.SaveClothing();
                pb.Call(GoodsCommand.GetEquipPanelR, true, panel);
            }
            return panel;
        }

        /// <summary>
        /// 判断装备是否过期
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static bool GoodsIsExpired(PlayerBusiness pb, Goods g, string str)
        {
            Variant v = g.Value;
            if (v == null || (!v.ContainsKey("TimeLines")))
                return false;
            Variant timelines = v.GetVariantOrDefault("TimeLines");
            if (timelines == null)
                return false;
            if (!v.ContainsKey("EndTime"))
                return false;

            DateTime endTime = v.GetDateTimeOrDefault("EndTime");
            DateTime dt = DateTime.UtcNow;
            if (dt < endTime)
                return false;
            string msg = str == "P10" ? TipManager.GetMessage(GoodsReturn.GoodsIsExpired1) : TipManager.GetMessage(GoodsReturn.GoodsIsExpired2);
           

            //if (ts.TotalHours < Convert.ToDouble(timelines["Hour"]))
            //    return false;

            //移除相关物品
            GoodsAccess.Instance.Remove(g.ID);

            Variant t = new Variant();
            string a = "<f size=\"12\" color=\"ff3300\"><Send/><msg>" + msg + "</msg></f>";
            t.Add("msg", a);
            t.Add("level", 0);
            t.Add("showType", 0);
            t.Add("date", DateTime.UtcNow);
            pb.Call(ClientCommand.SendMsgToAllPlayerR, string.Empty, t);
            return true;
        }

        /// <summary>
        /// 当前答题记录
        /// </summary>
        /// <param name="pb">重置答题记录</param>
        public static void ResetAnswer(PlayerBusiness pb)
        {
            if (pb != null)
            {
                string[] strs = TipManager.GetMessage(GoodsReturn.Answer).Split('|');
                if (strs.Length < 2) return;

                int Total = Convert.ToInt32(strs[0]);
                int Max = Convert.ToInt32(strs[1]);

                PlayerEx answer;
                Variant v = null;
                if (pb.Value.ContainsKey("Answer"))
                {
                    answer = pb.Value["Answer"] as PlayerEx;
                    v = answer.Value;

                    if (v.GetLocalTimeOrDefault("NowData").Date != DateTime.Now.Date)
                    {
                        v["Cur"] = 0;
                        v["Total"] = Total;
                        v["Max"] = Max;
                        v["NowData"] = DateTime.UtcNow;
                    }
                }
                else
                {
                    answer = new PlayerEx(pb.ID, "Answer");
                    v = new Variant();
                    v.Add("Cur", 0);//当前完成数量
                    v.Add("Total", Total);//默认可以执行次数
                    v.Add("Max", Max);//每天最多允许执行次数
                    v.Add("NowData", DateTime.UtcNow);//谋一天
                    answer.Value = v;
                    answer.Save();
                    pb.Value.Add("Answer", answer);
                }
                pb.Call(ClientCommand.UpdateActorR, new PlayerExDetail(answer));
            }
        }


        /// <summary>
        /// VIP功能道具
        /// </summary>
        /// <param name="note"></param>
        public static void VIPGoods(UserNote note, GameConfig gc)
        {
            Variant tmp = gc.Value;
            if (tmp == null)
            {
                note.Call(GoodsCommand.UseGoodsR, false, GoodsReturn.UseGoods2);
                return;
            }

            if (!note.Player.RemoveGoods(gc.ID,GoodsSource.DoubleUse))
            {
                note.Call(GoodsCommand.UseGoodsR, false, GoodsReturn.UseGoods2);
                return;
            }


  

            int vipDays = tmp.GetIntOrDefault("VIPDays");

            PlayerEx vipBase;
            if (!note.Player.Value.TryGetValueT("VIPBase", out vipBase))
            {
                vipBase = new PlayerEx(note.PlayerID, "VIPBase");
                Variant m = new Variant();
                m.Add("VIP", 0);//VIP等级
                m.Add("EndTime", DateTime.UtcNow.AddDays(vipDays));//过期时间
                m.Add("TotalDays", vipDays);//累计vip天数
                vipBase.Value = m;
                vipBase.Save();
                note.Player.Value.Add("VIPBase", vipBase);                
            }
            else
            {
                Variant v = vipBase.Value;

                DateTime dt = v.GetDateTimeOrDefault("EndTime");
                if (dt >= DateTime.UtcNow)
                {
                    v["EndTime"] = dt.AddDays(vipDays);//过期时间
                }
                else 
                {
                    v["EndTime"] = DateTime.UtcNow.AddDays(vipDays);
                }
                v["TotalDays"] = v.GetIntOrDefault("TotalDays") + vipDays;//时间累计
                vipBase.Save();
            }
            note.Player.IsVIP();
            //VIP通知
            note.Player.CallAll(ClientCommand.UpdateActorR, new PlayerExDetail(vipBase));
            note.Call(GoodsCommand.UseGoodsR, true, gc.ID);            
        }
    }
}
