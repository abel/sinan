using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using MongoDB.Bson;
using Sinan.Command;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Util;
using Sinan.Observer;
using Sinan.AMF3;

namespace Sinan.GMServerModule
{
    public class GMBusiness
    {
        public static string[] GetCommand(Notification note)
        {
            string[] comm;
            IList s = null;
            if (note.Body != null && note.Body.Count >= 1)
            {
                s = note[0] as IList;
            }
            if (s == null)
            {
                comm = new string[0];
            }
            else
            {
                comm = new string[s.Count];
                for (int i = 0; i < s.Count; i++)
                {
                    comm[i] = s[i].ToString();
                }
            }
            return comm;
        }

        /// <summary>
        /// 添加点券
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        internal static object AddBond(Notification note)
        {
            string[] strs = GetCommand(note);
            if (strs.Length < 2) return null;

            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }
            int bond = Convert.ToInt32(strs[1]);
            if (player.AddBond(bond, FinanceType.GM, "GM"))
            {
                string name = player.Name;
                return string.Format(TipManager.GetMessage(GMReturn.AddBond), name, bond);
            }
            return null;
        }

        /// <summary>
        /// 踢出玩家
        /// </summary>
        /// <param name="note"></param>
        public static object KickUser(Notification note)
        {
            string[] strs = GetCommand(note);
            if (strs.Length < 2) return null;
            string key = strs[0].Trim();
            PlayerBusiness player = PlayersProxy.FindPlayerByName(key);
            if (player == null)
            {
                //检查是否是IP地址
                IPAddress ip;
                if (!System.Net.IPAddress.TryParse(key, out ip))
                {
                    return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
                }
            }
            else
            {
                key = player.UserID;
            }
            int t; //禁止登录的时间
            if (!Int32.TryParse(strs[1], out t))
            {
                t = 60;
            }
            //解除黑名单
            if (t < 0)
            {
                BlackListManager.Instance.Remove(key);
                return string.Format(TipManager.GetMessage(GMReturn.KickUser), strs[0]);
            }
            if (t > 0)
            {
                BlackListManager.Instance.AddBlack(key, DateTime.UtcNow.AddMinutes(t));
            }
            if (player != null && player.Session != null)
            {
                player.Session.Close();
            }
            string str = string.Format(TipManager.GetMessage(GMReturn.Exit), player == null ? key : player.Name);
            return str;
        }

        /// <summary>
        /// 查看在线人数
        /// </summary>
        /// <param name="note"></param>
        public static object Online(Notification note)
        {
            string[] strs = GetCommand(note);
            if (strs == null || strs.Length == 0)
            {
                return new List<int> { UsersProxy.UserCount, PlayersProxy.OnlineCount };
            }
            if (strs.Length == 1)
            {
                if (string.IsNullOrEmpty(strs[0]))
                {
                    Process process = Process.GetCurrentProcess();
                    //查看所有场景人数
                    var v = ScenesProxy.OnlineCount();
                    v["User"] = UsersProxy.UserCount;
                    v["Player"] = PlayersProxy.OnlineCount;
                    v["Memory"] = (int)(process.WorkingSet64 >> 20);
                    return v;
                }
                else
                {
                    SceneBusiness scene;
                    if (ScenesProxy.TryGetScene(strs[0], out scene))
                    {
                        Dictionary<string, int> dic = new Dictionary<string, int>(1);
                        dic.Add(strs[0], scene.Players.Count);
                        return dic;
                    }
                    return string.Format(TipManager.GetMessage(GMReturn.Online), strs[0]);
                }
            }
            return null;
        }

        /// <summary>
        /// 设置玩家状态
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static object SetPlayerState(Notification note)
        {
            string[] strs = GetCommand(note);
            if (strs.Length != 2) return null;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }
            int state;
            Int32.TryParse(strs[1], out state);
            if (state < 0 || state > 4)
            {
                return null;
            }
            if (player.State != state)
            {
                if (state == 3)
                {
                    //删除玩家
                    PlayersProxy.DeletePlayer(player.PID, false);
                }
                if (PlayerAccess.Instance.ChangeState(player.PID, state))
                {
                    player.State = state;
                    if (state != 1)
                    {
                        UserSession us = player.Session;
                        if (us != null) us.Close();
                    }
                }
            }
            return string.Format(TipManager.GetMessage(GMReturn.SetPlayerState), player.Name, player.State);
        }

        /// <summary>
        /// 查看玩家信息
        /// </summary>
        /// <param name="note"></param>
        /// <param name="comm"></param>
        internal static object ViewPlayer(Notification note)
        {
            string[] strs = GetCommand(note);
            if (strs.Length != 1) return null;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }
            var v = new PlayerDetail(player, 2);
            Amf3Writer writer = new Amf3Writer(65535, false);
            writer.WriteObject(v);
            Amf3Reader<Variant> reader = new Amf3Reader<Variant>(writer.Array, 0, writer.Count);
            object b = reader.ReadObject();
            return new object[] { player.Online, b };
        }

        internal static object SetTalk(Notification note)
        {
            string[] strs = GetCommand(note);
            if (strs.Length != 2) return null;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }
            int min;
            Int32.TryParse(strs[1], out min);
            if (min > 0)
            {
                player.Danned = DateTime.UtcNow.AddMinutes(min);
                return string.Format(TipManager.GetMessage(GMReturn.SetTalk1), strs[0]);
            }
            else
            {
                player.Danned = DateTime.MinValue;
                return string.Format(TipManager.GetMessage(GMReturn.SetTalk2), strs[0]);
            }
        }


        #region 公告
        /// <summary>
        /// 公告
        /// </summary>
        /// <param name="note"></param>
        public static object Notice(Notification note)
        {
            Variant v = note.GetVariant(0);
            if (v == null)
            {
                return null;
            }

            Notice model = new Entity.Notice();
            model.ID = ObjectId.GenerateNewId().ToString();
            model.Name = v.GetStringOrDefault("Name");
            model.StartTime = v.GetUtcTimeOrDefault("StartTime");
            model.EndTime = v.GetUtcTimeOrDefault("EndTime");
            model.Sort = v.GetIntOrDefault("Sort");
            model.Content = v.GetStringOrDefault("Content");
            model.Count = v.GetIntOrDefault("Count");
            model.Rate = v.GetIntOrDefault("Rate");
            model.Status = v.GetIntOrDefault("Status");
            model.Place = v.GetStringOrDefault("Place");
            if (model.Save())
            {
                NoticeAccess.Instance.AddNotice(model);
                return TipManager.GetMessage(GMReturn.Notice1);
            }
            return string.Empty;
        }


        /// <summary>
        /// 取得公告列表
        /// </summary>
        /// <param name="note"></param>
        public static object NoticeList(Notification note)
        {
            List<Notice> list = NoticeAccess.Instance.GetNotices();
            List<Variant> tmp = new List<Variant>();
            foreach (Notice model in list)
            {
                Variant t = new Variant();
                t.Add("ID", model.ID);
                t.Add("Sort", model.Sort);
                t.Add("Name", model.Name);
                t.Add("StartTime", model.StartTime);
                t.Add("EndTime", model.EndTime);
                t.Add("Content", model.Content);
                t.Add("Rate", model.Rate);
                t.Add("Count", model.Count);
                t.Add("Status", model.Status);
                t.Add("Place", model.Place);
                tmp.Add(t);
            }
            return tmp;
        }

        /// <summary>
        /// 公告更新
        /// </summary>
        /// <param name="note"></param>
        public static object UpdateNotice(Notification note)
        {
            Variant v = note.GetVariant(0);
            if (v == null)
            {
                return null;
            }
            string id = v.GetStringOrDefault("ID");
            Notice model;
            if (NoticeAccess.Instance.GetNotice(id, out model))
            {
                bool ischange = false;
                if (v.ContainsKey("Name") && model.Name != v.GetStringOrDefault("Name"))
                {
                    model.Name = v.GetStringOrDefault("Name");
                    ischange = true;
                }

                if (v.ContainsKey("StartTime") && model.StartTime != v.GetUtcTimeOrDefault("StartTime"))
                {
                    model.StartTime = v.GetUtcTimeOrDefault("StartTime");
                    ischange = true;
                }

                if (v.ContainsKey("EndTime") && model.EndTime != v.GetUtcTimeOrDefault("EndTime"))
                {
                    model.EndTime = v.GetUtcTimeOrDefault("EndTime");
                    ischange = true;
                }

                if (v.ContainsKey("Sort") && model.Sort != v.GetIntOrDefault("Sort"))
                {
                    model.Sort = v.GetIntOrDefault("Sort");
                    ischange = true;
                }
                if (v.ContainsKey("Content") && model.Content != v.GetStringOrDefault("Content"))
                {
                    model.Content = v.GetStringOrDefault("Content");
                    ischange = true;
                }
                if (v.ContainsKey("Count") && model.Count != v.GetIntOrDefault("Count"))
                {
                    model.Count = v.GetIntOrDefault("Count");
                    ischange = true;
                }
                if (v.ContainsKey("Rate") && model.Rate != v.GetIntOrDefault("Rate"))
                {
                    model.Rate = v.GetIntOrDefault("Rate");
                    ischange = true;
                }
                if (v.ContainsKey("Status") && model.Status != v.GetIntOrDefault("Status"))
                {
                    model.Status = v.GetIntOrDefault("Status");
                    ischange = true;
                }
                if (v.ContainsKey("Place") && model.Place != v.GetStringOrDefault("Place"))
                {
                    model.Place = v.GetStringOrDefault("Place");
                    ischange = true;
                }
                if (v.ContainsKey("Cur") && model.Cur != v.GetIntOrDefault("Cur"))
                {
                    model.Cur = v.GetIntOrDefault("Cur");
                    ischange = true;
                }
                if (ischange)
                {
                    model.Save();
                    return true;
                }
            }
            return false;
        }
        #endregion

        /// <summary>
        /// 开启多倍经验活动
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static object DoubleExp(Notification note)
        {
            string[] strs = GetCommand(note);
            if (strs.Length != 4) return null;
            DateTime start;
            if (DateTime.TryParse(strs[0], out start))
            {
                start = start.ToUniversalTime();
            }
            else
            {
                return null;
            }
            DateTime end;
            if (DateTime.TryParse(strs[1], out end))
            {
                end = end.ToUniversalTime();
            }
            else
            {
                return null;
            }
            double x;
            if (!double.TryParse(strs[2], out x))
            {
                return null;
            }
            ExperienceControl.Instance.SetExp(start, end, x, strs[3]);
            return string.Format(TipManager.GetMessage(GMReturn.DoubleExp), ExperienceControl.ExpCoe);
            //("设置成功,当前经验:" + ExperienceControl.ExpCoe));
        }

        public static object OpenPart(Notification note)
        {
            string[] strs = GetCommand(note);
            if (strs.Length != 4) return null;
            DateTime start;
            if (!DateTime.TryParse(strs[0], out start))
            {
                return null;
            }

            DateTime end;
            if (!DateTime.TryParse(strs[1], out end))
            {
                return null;
            }
            if (end < DateTime.Now)
            {
                return null;
            }

            string name = strs[2];
            Part part = null;
            if (name == Part.Pro)
            {
                part = PartManager.Instance.FindOne(Part.Pro);
            }
            else if (name == Part.Rob)
            {
                part = PartManager.Instance.FindOne(Part.Rob);
            }
            if (part == null)
            {
                return null;
            }

            Period p = new Period(start, end);
            part.Periods.Add(p);
            //("活动开启成功:" + name)
            return string.Format(TipManager.GetMessage(GMReturn.OpenPart), name);
        }

        /// <summary>
        /// 活动更新
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public static object UpdatePart(Notification note)
        {
            Notifier.Instance.Publish(new Notification(WebCommand.GetPartList), false);
            return string.Format(TipManager.GetMessage(GMReturn.UpdatePart));
        }
    }
}
