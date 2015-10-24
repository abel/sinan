using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.TaskModule.Business
{
    /// <summary>
    /// 任务完成任务
    /// </summary>
    class FinishBusiness
    {
        /// <summary>
        /// 任务更新
        /// </summary>
        /// <param name="note"></param>
        /// <param name="gc"></param>
        /// <param name="task"></param>
        public static void UpdateFinishTask(UserNote note, GameConfig gc, Task task)
        {
            IList npcs = gc.Value.GetValue<IList>("NPCBaseInfo");
            Variant npcA = npcs[0] as Variant;//接任务NPC
            Variant npcB = npcs[1] as Variant;//领奖NPC
            IList finish = task.Value.GetValue<IList>("Finish");
            int status = task.Value.GetIntOrDefault("Status");
            if (status == 0)
            {
                //接任务的NPC
                string npcid = npcA.GetStringOrDefault("NPCID");
                //接任务
                if (!TaskBusiness.TaskNpcCheck(note.Player, TaskCommand.UpdateTaskR, npcid))
                {
                    note.Call(TaskCommand.UpdateTaskR, false, TipManager.GetMessage(TaskReturn.Npc3));
                    return;
                }

                for (int i = 0; i < finish.Count; i++)
                {
                    Variant v = finish[i] as Variant;
                    string type = v.GetStringOrDefault("Type");
                    if (note.Player.SceneID == npcA.GetStringOrDefault("NPCSceneID"))
                    {
                        if (type == "30001")
                        {
                            if (!TaskFinish30001(note, v))
                                return;
                        }
                    }

                    if (type == "10010")
                    {
                        if (!TaskFinish10010(note, v))
                        {
                            note.Call(TaskCommand.UpdateTaskR, false, TipManager.GetMessage(BurdenReturn.BurdenFull));
                            return;
                        }
                    }

                    if (type == "10004")
                    {
                        Task10004(note, v);
                    }
                    else if (type == "10007")
                    {
                        TaskFinish10007(note, v);
                    }
                    else if (type == "10008")
                    {
                        Task10008(note, v);
                    }
                    else if (type == "10003")
                    {
                        TaskFinish10003(note, v);
                    }
                }


                bool m = true;
                foreach (Variant s in finish)
                {
                    if (s.GetIntOrDefault("Total") != s.GetIntOrDefault("Cur"))
                    {
                        m = false;
                        break;
                    }
                }
                if (m)
                {
                    task.Value["Status"] = 2;
                }
                else
                {
                    //接任务时候的时间
                    task.Value["Status"] = 1;
                }
                task.Save();
                TaskBusiness.TaskAPC(note);
                note.Call(TaskCommand.UpdateTaskR, true, TaskAccess.Instance.GetTaskInfo(task));
                return;
            }

            if (status == 1)
            {
                //判断完成条件                
                foreach (Variant s in finish)
                {
                    string str = s.GetStringOrDefault("Type");
                    switch (str)
                    {
                        case "10002":
                            TaskFinish10002(note, s);
                            break;
                        case "10004":
                            Task10004(note, s);
                            break;
                        case "10005":
                            TaskFinish10005(note, s);
                            break;
                        case "10006":
                            TaskFinish10006(note, s);
                            break;
                        case "10007":
                            TaskFinish10007(note, s);
                            break;
                        case "10003":
                            TaskFinish10003(note, s);
                            break;
                    }
                }
            }

            bool isfinish = true;
            foreach (Variant s in finish)
            {
                if (s.GetIntOrDefault("Total") != s.GetIntOrDefault("Cur"))
                {
                    isfinish = false;
                    break;
                }
            }

            if (isfinish)
            {
                task.Value["Status"] = 2;
            }
            task.Save();
            note.Call(TaskCommand.UpdateTaskR, true, TaskAccess.Instance.GetTaskInfo(task));
            TaskBusiness.TaskAPC(note);
        }

        #region 完成不同类型
        /// <summary>
        /// 接收任务时玩家等级判断
        /// </summary>
        /// <param name="note"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool TaskFinish30001(UserNote note, Variant s)
        {
            //接收任务时候的等级限制
            if (s.GetStringOrDefault("Type") == "30001")
            {
                if (note.Player.Level < s.GetIntOrDefault("Total"))
                {
                    note.Call(TaskCommand.UpdateTaskR, false, TipManager.GetMessage(TaskReturn.TaskRevNoLevel));
                    return false;
                }
                s["Cur"] = s.GetIntOrDefault("Total");
            }
            return true;
        }

        /// <summary>
        /// 要求打几只怪
        /// </summary>
        /// <param name="note">角色对象</param>
        /// <param name="tasks"></param>
        public static void TaskFinish10001(UserNote note, List<Task> tasks, Dictionary<string, int> apcs)
        {
            if (apcs == null || apcs.Count == 0)
            {
                return;
            }

            foreach (Task g in tasks)
            {
                GameConfig gc = GameConfigAccess.Instance.FindTaskId(g.Value.GetStringOrDefault("TaskID"));
                if (gc == null)
                {
                    continue;
                }

                bool isChange = false;
                IList finish = g.Value.GetValue<IList>("Finish");
                foreach (var item in apcs)
                {
                    foreach (Variant d in finish)
                    {
                        if (item.Key != d.GetStringOrDefault("NPCTypeID"))
                        {
                            continue;
                        }
                        int tp = d.GetIntOrDefault("Type");
                        int cur = d.GetIntOrDefault("Cur");
                        int total = d.GetIntOrDefault("Total");
                        if (tp == 10001)
                        {
                            if (cur == total)
                            {
                                continue;
                            }
                            d["Cur"] = Math.Min(total, cur + item.Value);
                            isChange = true;
                        }
                        else if (tp == 10003)
                        {
                            bool t = TaskFinish10003(note, d, item.Value);
                            if (!isChange && t)
                            {
                                isChange = true;
                            }
                        }
                    }
                }

                foreach (Variant v in finish)
                {
                    if (v.GetIntOrDefault("Type") == 10004)
                    {
                        bool t = Task10004(note, v);
                        if (!isChange && t)
                        {
                            isChange = true;
                        }
                    }
                }

                bool IsFinish = true;
                foreach (Variant d in finish)
                {
                    if (d.GetIntOrDefault("Cur") != d.GetIntOrDefault("Total"))
                    {
                        IsFinish = false;
                        break;
                    }
                }

                if (IsFinish)
                {
                    g.Value["Status"] = 2;
                    TaskBusiness.TaskAPC(note);
                }
                if (isChange)
                {
                    //任务发生变化，更新改任务
                    g.Save();
                    note.Call(TaskCommand.UpdateTaskR, true, TaskAccess.Instance.GetTaskInfo(g));
                }
            }
        }

        /// <summary>
        /// 对话操作类
        /// </summary>
        /// <param name="note">角色对象</param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static void TaskFinish10002(UserNote note, Variant s)
        {
            //string sid = s.GetStringOrDefault("SceneID");
            //if (string.IsNullOrEmpty(sid) || sid.Length < 2 || sid == note.Player.SceneID)

            //if (s.GetStringOrDefault("SceneID") == note.Player.SceneID)
            {
                s["Cur"] = s.GetIntOrDefault("Total");
            }
        }

        /// <summary>
        /// 打指定的怪得到指定的物品给指定的NPC
        /// </summary>
        /// <param name="note">角色对象</param>
        /// <param name="s"></param>
        public static bool TaskFinish10003(UserNote note, Variant s, int ApcCount)
        {
            int m = GameConfigAccess.Instance.GetStactCount(s.GetStringOrDefault("GoodsID"));
            if (m <= 0) return false;
            int Lv = s.GetIntOrDefault("GetGoodsLv");
            PlayerEx burden = note.Player.B0;
            IList c = burden.Value.GetValue<IList>("C");

            bool ischange = false;
            for (int i = 0; i < ApcCount; i++)
            {
                int total = s.GetIntOrDefault("Total");
                int cur = s.GetIntOrDefault("Cur");
                if (total <= cur)
                    continue;

                if (NumberRandom.Next(1, 101) > Lv)
                    continue;

                Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
                Variant v = new Variant();
                v.Add("Number1", 1);
                string goodsid = s.GetStringOrDefault("GoodsID");
                dic.Add(goodsid, v);
                if (BurdenManager.IsFullBurden(burden, dic))
                {
                    Variant t = GoodsAccess.Instance.LiaoTianList(TipManager.GetMessage(BurdenReturn.BurdenFull));
                    note.Call(ClientCommand.SendMsgToAllPlayerR, string.Empty, t);
                    break;
                }
                note.Player.AddGoods(dic, GoodsSource.TaskFinish10003);

                if (dic.Count > 0)
                {
                    note.Player.AwardGoods.SetOrInc(goodsid, 1);
                }
                int number = BurdenManager.BurdenGoodsCount(c, goodsid);

                s["Cur"] = number >= total ? total : number;
                burden.Save();
                ischange = true;
            }
            return ischange;
        }

        /// <summary>
        /// 检查物品是否存在
        /// </summary>
        /// <param name="note"></param>
        /// <param name="s"></param>
        public static void TaskFinish10003(UserNote note, Variant s)
        {
            PlayerEx b0 = note.Player.B0;
            IList c = b0.Value.GetValue<IList>("C");
            int num = BurdenManager.BurdenGoodsCount(c, s.GetStringOrDefault("GoodsID"));
            s["Cur"] = Math.Min(s.GetIntOrDefault("Total"), num);
        }

        /// <summary>
        /// 判断是否存在要求回收的物品
        /// </summary>
        /// <param name="note"></param>
        /// <param name="s"></param>
        /// <returns>true表示发生变化</returns>
        public static bool Task10004(UserNote note, Variant s)
        {
            if (s.GetIntOrDefault("Cur") == s.GetIntOrDefault("Total"))
                return false;
            if (!s.ContainsKey("GoodsID"))
                return false;

            if (s.GetIntOrDefault("Type") != 10004)
                return false;

            int m = GameConfigAccess.Instance.GetStactCount(s.GetStringOrDefault("GoodsID"));

            if (m <= 0) return false;

            PlayerEx burden = note.Player.B0;

            IList c = burden.Value.GetValue<IList>("C");
            int number = 0;
            foreach (Variant d in c)
            {
                if (d.GetStringOrDefault("G") == s.GetStringOrDefault("GoodsID"))
                {
                    number += d.GetIntOrDefault("A");
                }
            }

            if (number >= s.GetIntOrDefault("Total"))
                s["Cur"] = s.GetIntOrDefault("Total");
            else
                s["Cur"] = number;
            return true;
        }

        /// <summary>
        /// 回收一定数量的道具,表示在领奖的时候回
        /// </summary>
        /// <param name="note">角色对象</param>
        /// <param name="s"></param>
        public static Variant TaskFinish10004(UserNote note, IList s)
        {
            Variant us = new Variant();
            List<string> list = new List<string>();
            bool ischange = false;
            foreach (Variant d in s)
            {
                //表示回收物品
                if (d.GetIntOrDefault("Type") == 10004)
                {
                    PlayerEx burden = note.Player.B0;
                    IList c = burden.Value.GetValue<IList>("C");
                    //回收数量
                    int num = d.GetIntOrDefault("Total");

                    if (num == d.GetIntOrDefault("Cur"))
                    {
                        string goodsid = d.GetStringOrDefault("GoodsID");
                        note.Player.RemoveGoods(goodsid, num, GoodsSource.TaskFinish10004);
                        us.SetOrInc(goodsid, num);
                        ischange = true;
                    }
                    burden.Save();
                }
            }
            if (ischange) 
            {
                note.Player.UpdateBurden();
            }
            return us;
        }

        /// <summary>
        /// 表示客户端完成一个指定任务的操作
        /// </summary>
        /// <param name="note"></param>
        /// <param name="s"></param>
        public static void TaskFinish10005(UserNote note, Variant s)
        {
            s["Cur"] = s.GetIntOrDefault("Total");
        }

        /// <summary>
        /// 对话过程中回收道具，
        /// 回收成功表示任务完成条件达成
        /// </summary>
        /// <param name="note"></param>
        /// <param name="s"></param>
        public static void TaskFinish10006(UserNote note, Variant s)
        {
            string sceneid = s.GetStringOrDefault("SceneID");
            string goodsid = s.GetStringOrDefault("GoodsID");
            if (sceneid == null || goodsid == null)
                return;

            if (sceneid != note.Player.SceneID)
                return;

            int total = s.GetIntOrDefault("Total");
            if (note.Player.RemoveGoods(goodsid, total, GoodsSource.TaskFinish10006))
            {
                s["Cur"] = s.GetIntOrDefault("Total");
                note.Player.UpdateBurden();
            }
        }

        /// <summary>
        /// 角色等级达到某一等级完成一个任务
        /// </summary>
        /// <param name="note"></param>
        public static void TaskFinish10007(UserNote note, Variant s)
        {
            if (s.GetIntOrDefault("Total") != s.GetIntOrDefault("Cur") && note.Player.Level >= s.GetIntOrDefault("Total"))
            {
                s["Cur"] = s.GetIntOrDefault("Total");
            }
        }

        /// <summary>
        /// 接收任务就完成
        /// </summary>
        /// <param name="note"></param>
        /// <param name="s"></param>
        public static void Task10008(UserNote note, Variant s)
        {
            s["Cur"] = s.GetIntOrDefault("Total");
        }

        /// <summary>
        /// 采集物品
        /// </summary>
        /// <param name="note"></param>
        /// <param name="s"></param>
        public static bool TaskFinish10009(UserNote note, Variant s, string goodsid)
        {
            if (note.Player.SceneID != s.GetStringOrDefault("SceneID"))
            {
                note.Call(TaskCommand.TaskCollectR, false, TipManager.GetMessage(TaskReturn.CollectSceneError));
                return true;
            }
            PlayerEx burden = note.Player.B0;
            string GoodsID = s.GetStringOrDefault("GoodsID");
            if (goodsid != GoodsID)
                return false;
            int Total = s.GetIntOrDefault("Total");
            int Cur = s.GetIntOrDefault("Cur");
            if (Total == Cur)
                return false;
            //产生道具数量
            int GetGoodsLv = s.GetIntOrDefault("GetGoodsLv");
            GetGoodsLv = GetGoodsLv > (Total - Cur) ? (Total - Cur) : GetGoodsLv;
            Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
            Variant v = new Variant();
            v.Add("Number1", GetGoodsLv);
            dic.Add(GoodsID, v);
            if (BurdenManager.IsFullBurden(burden, dic))
            {
                note.Call(TaskCommand.TaskCollectR, false, TipManager.GetMessage(TaskReturn.BurdenFull));
                return true;
            }

            s["Cur"] = Cur + GetGoodsLv;
            note.Player.AddGoods(dic, GoodsSource.TaskFinish10009);
            return false;
        }

        /// <summary>
        /// 接收任务时发方道具
        /// </summary>
        /// <param name="note"></param>
        /// <param name="s"></param>
        public static bool TaskFinish10010(UserNote note, Variant s)
        {
            if (s.GetIntOrDefault("Cur") >= s.GetIntOrDefault("Total"))
                return true;

            Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
            Variant v = new Variant();
            v.Add("Number1", s.GetIntOrDefault("Total"));
            dic.Add(s.GetStringOrDefault("GoodsID"), v);
            if (BurdenManager.IsFullBurden(note.Player.B0, dic))
                return false;
            note.Player.AddGoods(dic, GoodsSource.TaskFinish10010);
            s["Cur"] = s.GetIntOrDefault("Total");
            return true;
        }

        /// <summary>
        /// 更新包袱信息
        /// </summary>
        /// <param name="note"></param>
        public static void UpdateBurden(UserNote note, IList list)
        {
            //更新包袱
            Variant tmp = new Variant();
            foreach (string n in list)
            {
                if (string.IsNullOrEmpty(n))
                    continue;
                tmp.Add(n, note.Player.Value[n]);
            }
            note.Call(BurdenCommand.BurdenListR, tmp);
        }

        /// <summary>
        /// 邀请QQ好友
        /// </summary>
        /// <param name="note"></param>
        public static void TaskFriends(UserNote note)
        {
            //得到已经接的任务
            List<Task> list = TaskAccess.Instance.GetTaskList(note.PlayerID, 1);
            foreach (Task task in list)
            {
                Variant v = task.Value;
                IList finish = v.GetValue<IList>("Finish");
                bool ischange = false;
                foreach (Variant k in finish)
                {
                    if (k.GetStringOrDefault("Type") != "10011")
                        continue;
                    int total = k.GetIntOrDefault("Total");
                    int cur = k.GetIntOrDefault("Cur");
                    if (total > cur)
                    {
                        k.SetOrInc("Cur", 1);
                        //表示发生变化
                        ischange = true;
                    }
                }

                //发生变化再更新
                if (!ischange) continue;

                foreach (Variant k in finish)
                {
                    if (k.GetIntOrDefault("Total") != k.GetIntOrDefault("Cur"))
                    {
                        ischange = false;//表示没有完成
                        break;
                    }
                }

                //表示任务已经完成
                if (ischange)
                {
                    v["Status"] = 2;
                }
                task.Save();
                note.Call(TaskCommand.UpdateTaskR, true, TaskAccess.Instance.GetTaskInfo(task));
                TaskBusiness.TaskAPC(note);
            }
        }

        /// <summary>
        /// 任务放异
        /// </summary>
        /// <param name="note">角色信息</param>
        /// <param name="task">任务</param>
        /// <returns></returns>
        public static bool TaskGiveup(PlayerBusiness pb, Task task, bool isremove = true)
        {

            PlayerEx burden = pb.B0;
            Variant v = task.Value;
            //完成条件
            IList finish = v.GetValue<IList>("Finish");
            for (int i = 0; i < finish.Count; i++)
            {
                Variant k = finish[i] as Variant;
                if (k == null) continue;
                string goodsid = k.GetStringOrDefault("GoodsID");
                int Cur = k.GetIntOrDefault("Cur");
                switch (k.GetIntOrDefault("Type"))
                {
                    case 10006:
                        Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
                        Variant vn = new Variant();
                        vn.Add("Number1", Cur);
                        dic.Add(goodsid, vn);
                        if (BurdenManager.IsFullBurden(burden, dic))
                        {
                            pb.Call(TaskCommand.GiveupR, false, TipManager.GetMessage(BurdenReturn.BurdenFull));
                            return true;
                        }
                        pb.AddGoods(dic, GoodsSource.TaskGiveup);
                        break;
                    case 10003:
                    case 10009:
                    case 10010:
                        //BurdenAccess.Remove(burden, goodsid, Cur);
                        pb.RemoveGoods(goodsid, Cur, GoodsSource.TaskGiveup);
                        break;
                }
                k["Cur"] = 0;
            }
            v["Status"] = 0;
            if (isremove)
            {
                task.Save();
            }
            pb.Call(TaskCommand.GiveupR, true, task.ID);
            return false;
        }
        #endregion
    }
}
