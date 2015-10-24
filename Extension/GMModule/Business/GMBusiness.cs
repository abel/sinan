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

namespace Sinan.GMModule.Business
{
    public class GMBusiness
    {
        /// <summary>
        /// 添加点券
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        internal static void AddBond(GMNote note, string[] strs)
        {
            if (strs.Length < 2) return;

            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]));
                return;
            }
            int bond = Convert.ToInt32(strs[1]);
            if (player.AddBond(bond, FinanceType.GM, "GM"))
            {
                string name = player.Name;
                //note.Call(GMCommand.GMR, ("为【" + name + "】充入点券:" + bond));
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.AddBond), name, bond));
            }
        }

        /// <summary>
        /// 踢出玩家
        /// </summary>
        /// <param name="note"></param>
        public static void KickUser(GMNote note, string[] strs)
        {
            if (strs.Length < 2) return;
            string key = strs[0].Trim();
            PlayerBusiness player = PlayersProxy.FindPlayerByName(key);
            if (player == null)
            {
                //检查是否是IP地址
                IPAddress ip;
                if (!System.Net.IPAddress.TryParse(key, out ip))
                {
                    note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]));
                    return;
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
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.KickUser), strs[0]));
                //"解除成功:" + strs[0]);
                return;
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
            note.Call(GMCommand.GMR, str);
        }

        /// <summary>
        /// 查看在线人数
        /// </summary>
        /// <param name="note"></param>
        public static void Online(GMNote note, string[] strs)
        {
            if (strs == null || strs.Length == 0)
            {
                note.Call(GMCommand.OnlineR, new List<int> { UsersProxy.UserCount, PlayersProxy.OnlineCount });
                return;
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
                    note.Call(GMCommand.OnlineR, v);
                }
                else
                {
                    SceneBusiness scene;
                    if (ScenesProxy.TryGetScene(strs[0], out scene))
                    {
                        Dictionary<string, int> dic = new Dictionary<string, int>(1);
                        dic.Add(strs[0], scene.Players.Count);
                        note.Call(GMCommand.OnlineR, dic);
                        return;
                    }
                    note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.Online), strs[0]));
                    //"场景信息不正确:" + strs[0]);
                }
            }
        }

        /// <summary>
        /// 设置玩家状态
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void SetPlayerState(GMNote note, string[] strs)
        {
            if (strs.Length != 2) return;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]));
                return;
            }
            int state;
            Int32.TryParse(strs[1], out state);
            if (state < 0 || state > 4)
            {
                return;
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
            note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.SetPlayerState), player.Name, player.State));
        }

        /// <summary>
        /// 查看玩家信息
        /// </summary>
        /// <param name="note"></param>
        /// <param name="comm"></param>
        internal static void ViewPlayer(GMNote note, string[] strs)
        {
            if (strs.Length != 1) return;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]));
                return;
            }
            note.Call(GMCommand.ViewPlayerR, player.Online, new PlayerDetail(player, 2));
        }

        internal static void SetTalk(GMNote note, string[] strs)
        {
            if (strs.Length != 2) return;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]));
                return;
            }
            int min;
            Int32.TryParse(strs[1], out min);
            if (min > 0)
            {
                player.Danned = DateTime.UtcNow.AddMinutes(min);
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.SetTalk1), strs[0]));
            }
            else
            {
                player.Danned = DateTime.MinValue;
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.SetTalk2), strs[0]));
            }
        }


        #region 公告
        /// <summary>
        /// 公告
        /// </summary>
        /// <param name="note"></param>
        public static void Notice(GMNote note)
        {
            Variant v = note.GetVariant(1);
            if (v == null)
            {
                return;
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
                note.Call(GMCommand.GMR, TipManager.GetMessage(GMReturn.Notice1));
            }
            else
            {
                note.Call(GMCommand.GMR, "");
            }
        }


        /// <summary>
        /// 取得公告列表
        /// </summary>
        /// <param name="note"></param>
        public static void NoticeList(GMNote note)
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
            note.Call(GMCommand.NoticeListR, tmp);
        }

        /// <summary>
        /// 公告更新
        /// </summary>
        /// <param name="note"></param>
        public static void UpdateNotice(GMNote note)
        {
            Variant v = note.GetVariant(1);
            if (v == null)
            {
                return;
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
                    note.Call(GMCommand.UpdateNoticeR, true);
                    return;
                }
            }
            note.Call(GMCommand.UpdateNoticeR, false);
        }
        #endregion

        /// <summary>
        /// 开启多倍经验活动
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void DoubleExp(GMNote note, string[] strs)
        {
            if (strs.Length != 4) return;
            DateTime start;
            if (DateTime.TryParse(strs[0], out start))
            {
                start = start.ToUniversalTime();
            }
            else
            {
                return;
            }
            DateTime end;
            if (DateTime.TryParse(strs[1], out end))
            {
                end = end.ToUniversalTime();
            }
            else
            {
                return;
            }
            double x;
            if (!double.TryParse(strs[2], out x))
            {
                return;
            }
            ExperienceControl.Instance.SetExp(start, end, x, strs[3]);
            note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.DoubleExp), ExperienceControl.ExpCoe));
            //("设置成功,当前经验:" + ExperienceControl.ExpCoe));
        }

        public static void OpenPart(GMNote note, string[] strs)
        {
            if (strs.Length != 4) return;
            DateTime start;
            if (!DateTime.TryParse(strs[0], out start))
            {
                return;
            }

            DateTime end;
            if (!DateTime.TryParse(strs[1], out end))
            {
                return;
            }
            if (end < DateTime.Now)
            {
                return;
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
                return;
            }

            Period p = new Period(start, end);
            part.Periods.Add(p);
            //("活动开启成功:" + name)
            note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.OpenPart), name));
        }
    }
}
