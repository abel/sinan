using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Log;
using Sinan.Util;

#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.TaskModule.Business
{
    class TaskBusiness
    {
        static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 任务触发
        /// </summary>
        /// <param name="OnlineBusiness"></param>
        /// <param name="gc"></param>
        public static void TaskStart(UserNote note, GameConfig gc)
        {

            if (note.Name == UserPlayerCommand.CreatePlayerSuccess)
            {
                Player model = note[0] as Player;
                if (!TaskAccess.Instance.TaskLimit(model.ID, model.Level, gc.ID, gc.Value))
                    return;
                //激活创建任务
                TaskAccess.Instance.TaskBase(model.ID, model.RoleID, gc, 0, model.Level);
            }
            else
            {
                TaskStartBusiness(note.Player, gc);
            }
        }
        /// <summary>
        /// 任务触发
        /// </summary>
        /// <param name="note"></param>
        /// <param name="gc"></param>
        public static void TaskStartBusiness(PlayerBusiness pb, GameConfig gc, bool IsCall = true)
        {
            if (!TaskAccess.Instance.TaskLimit(pb.ID, pb.Level, gc.ID, gc.Value))
            {
                return;
            }
            int loopcount = 0;
            if (gc.Value.GetIntOrDefault("TaskType") == 8) 
            {
                loopcount = pb.WeekTotal(8) + 1;
            }

            Task task = TaskAccess.Instance.TaskBase(pb.ID, pb.RoleID, gc, loopcount, pb.Level);

            if (IsCall && task != null)
            {
                pb.Call(TaskCommand.TaskActivationR, TaskAccess.Instance.GetTaskInfo(task));
            }
        }

        /// <summary>
        /// 任务进行中关联的APC
        /// </summary>
        /// <param name="note"></param>
        public static List<Task> TaskAPC(UserNote note)
        {
            note.Player.TaskAPC.Clear();
            note.Player.TaskGoods.Clear();
            List<Task> gs = TaskAccess.Instance.GetTaskList(note.PlayerID);
            for (int i = 0; i < gs.Count; i++)
            {
                Task g = gs[i];
                if (g == null)
                    continue;
                int status = g.Value.GetIntOrDefault("Status");

                if (status > 2 || status == 0)
                    continue;

                IList finish = g.Value.GetValue<IList>("Finish");
                for (int j = 0; j < finish.Count; j++)
                {
                    Variant f = finish[j] as Variant;
                    if (f == null)
                    {
                        continue;
                    }

                    string type = f.GetStringOrDefault("Type");
                    if (type == "10004" || type == "10003")
                    {
                        string goodsid = f.GetStringOrDefault("GoodsID");
                        note.Player.TaskGoods.Add(goodsid);
                    }

                    if (f.GetIntOrDefault("Cur") >= f.GetIntOrDefault("Total"))
                    {
                        continue;
                    }

                    //打怪，打怪得道具
                    if (type == "10001" || type == "10003")
                    {
                        string npc = f.GetStringOrDefault("NPCTypeID");
                        note.Player.TaskAPC.Add(npc);
                    }
                }
            }
            return gs;
        }


        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="list"></param>
        public static void DeleteTask(PlayerBusiness pb, IList list, bool isremove = true)
        {
            List<Task> tasks = TaskAccess.Instance.FindConfigList(pb.ID, list);
            for (int i = 0; i < tasks.Count; i++)
            {
                Task task = tasks[i];
                if (task != null)
                {
                    FinishBusiness.TaskGiveup(pb, task, isremove);
                }
            }
            //删除之后,如果已经奖励道具要求回收
            TaskAccess.Instance.Remove(pb.ID, list);
        }

        #region 日常任务重置

        /// <summary>
        /// 重置日常任务
        /// </summary>
        /// <param name="pb">当前在线</param>
        /// <param name="iscall"></param>
        public static void DayTask(PlayerBusiness pb, bool iscall = true)
        {
            string[] strs = TipManager.GetMessage(TaskReturn.DayTaskConfig).Split('|');

            if (strs.Length < 2)
                return;


            List<string> list = TaskAccess.Instance.Remove_1(pb.ID, 2);
            if (iscall)
            {
                foreach (string id in list)
                {
                    pb.Call(TaskCommand.GiveupR, true, id);
                }
            }

            int total = Convert.ToInt32(strs[0]);
            int max = Convert.ToInt32(strs[1]);

            PlayerEx ex;
            Variant v = null;

            if (pb.Value.ContainsKey("DayTask"))
            {
                ex = pb.Value["DayTask"] as PlayerEx;
                v = ex.Value;
                //表示时间到,更新日常任务的当前值
                if (v.GetLocalTimeOrDefault("NowData").Date != DateTime.Now.Date)
                {
                    v["Cur"] = 0;
                    v["Total"] = total;//
                    v["Max"] = max;//最大日常任务数量
                    v["NowData"] = DateTime.UtcNow;
                }                
            }
            else
            {
                ex = new PlayerEx(pb.ID, "DayTask");
                v = new Variant();
                v.Add("Cur", 0);//当前完成数量
                v.Add("Total", total);//默认可以执行次数
                v.Add("Max", max);//每天最多允许执行次数
                v.Add("NowData", DateTime.UtcNow);//谋一天
                ex.Value = v;
                ex.Save();
                pb.Value.Add("DayTask", ex);
            }
            //更新一次，通知客户端
            if (v != null && v.GetIntOrDefault("Cur") < v.GetIntOrDefault("Total"))
            {
                if (!TaskAccess.Instance.IsDayTask(pb.ID,2))
                {
                    //表示当天日常任务已经完成
                    GameConfig gc = GameConfigAccess.Instance.GetDayTaskOne(pb.Level, 2);
                    if (gc != null)
                    {
                        Task t = TaskAccess.Instance.TaskBase(pb.ID, pb.RoleID, gc,0,0);
                        if (t != null)
                        {
                            v["Cur"] = v.GetIntOrDefault("Cur") + 1;
                            if (iscall)
                            {
                                pb.Call(TaskCommand.TaskActivationR, TaskAccess.Instance.GetTaskInfo(t));
                            }
                        }
                    }
                }
                else
                {
                    v["Cur"] = v.GetIntOrDefault("Cur") == 0 ? 1 : v.GetIntOrDefault("Cur");
                }
            }
            //如果成在变化测发送
            ex.Save();
            pb.Call(ClientCommand.UpdateActorR, new PlayerExDetail(ex));
        }

        #endregion

        #region 家族贡献任务
        /// <summary>
        /// 家族贡献任务
        /// </summary>
        /// <param name="pb"></param>
        public static void FamilyTask(PlayerBusiness pb, bool iscall)
        {
            string soleid = pb.ID + "FamilyTask";
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                //不存在家族
                if (!TaskBusinessBase.IsFamily(pb))
                    return;
                int num = pb.TaskTotal(7);
                if (num >= 20)
                    return;
                if (num == 0)
                {
                    List<string> list = TaskAccess.Instance.Remove_1(pb.ID, 7);
                    if (iscall)
                    {
                        foreach (string id in list)
                        {
                            pb.Call(TaskCommand.GiveupR, true, id);
                        }
                    }
                }

                if (TaskAccess.Instance.IsDayTask(pb.ID, 7))
                    return;
                //随机取得一个家族贡献任务
                GameConfig gc = GameConfigAccess.Instance.GetDayTaskOne(pb.Level, 7);
                if (gc != null)
                {
                    Task t = TaskAccess.Instance.TaskBase(pb.ID, pb.RoleID, gc, 0, 0);
                    if (t != null)
                    {
                        if (iscall)
                        {
                            pb.Call(TaskCommand.TaskActivationR, TaskAccess.Instance.GetTaskInfo(t));
                        }
                    }
                }
            }
            finally 
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }
        #endregion

        #region 循式任务重置
        /// <summary>
        /// 循式任务
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="iscall"></param>
        public static void LoopTask(PlayerBusiness pb, bool iscall,List<int> loop)
        {
            //4环式任务,6家族循环任务
            //List<int> loop = new List<int>() { 4, 6 };
            //List<int> loop = new List<int>() { 6 };
            for (int i = 0; i < loop.Count; i++)
            {
                int taskType = loop[i];
                if (taskType == 6) 
                {
                    if (!TaskBusinessBase.IsFamily(pb))
                        continue;
                }
                //取得循环任务列表
                List<GameConfig> loopList = GameConfigAccess.Instance.FindTask(taskType);

                //起始任务所关联的所有任务
                Dictionary<string, List<GameConfig>> startList = new Dictionary<string, List<GameConfig>>();
                //所有的起始任务,如果起始任务过期，则删除所有相关联的任务，重置起始任务
                Dictionary<string, GameConfig> startTask = new Dictionary<string, GameConfig>();
                //  StartLoop开始任务,EndLoop结束任务,LoopDate任务周期
                List<string> list = new List<string>();
                foreach (GameConfig gc in loopList)
                {
                    string StartLoop = gc.Value.GetStringOrDefault("StartLoop");
                    if (!startList.ContainsKey(StartLoop))
                    {
                        startList.Add(StartLoop, new List<GameConfig>() { gc });
                    }
                    else
                    {
                        List<GameConfig> tasks = startList[StartLoop];
                        tasks.Add(gc);
                    }

                    if (gc.ID == StartLoop)
                    {
                        if (!startTask.ContainsKey(StartLoop))
                        {
                            startTask.Add(StartLoop, gc);
                        }
                        if (!list.Contains(StartLoop))
                        {
                            list.Add(StartLoop);
                        }
                    }
                }
                LoopTask(pb, startList, startTask, list, iscall);
            }
        }

        /// <summary>
        /// 循式任务重置
        /// </summary>
        /// <param name="pb">在线用户</param>
        /// <param name="startList">循环任务列表</param>
        /// <param name="startTask">起始任务</param>
        /// <param name="list">起始任务</param>
        public static void LoopTask(PlayerBusiness pb, Dictionary<string, List<GameConfig>> startList, Dictionary<string, GameConfig> startTask, List<string> list, bool iscall)
        {
            //得到所有被激活的起始环式任务
            List<Task> taskList = TaskAccess.Instance.FindConfigList(pb.ID, list);
            //可以删除的循环任务
            List<string> removeTask = new List<string>();
            //当前不能触发的任务
            List<string> noTask = new List<string>();
            DateTime dt = DateTime.UtcNow;

            for (int i = 0; i < taskList.Count; i++)
            {
                Task task = taskList[i];
                if (task == null)
                    continue;
                string taskid = task.Value.GetStringOrDefault("TaskID");
                GameConfig gc;
                if (startTask.TryGetValue(taskid, out gc))
                {
                    int loopDate = gc.Value.GetIntOrDefault("LoopDate");
                    TimeSpan ts = dt.ToLocalTime().Date - task.Created.ToLocalTime().Date;
                    if (ts.TotalDays >= loopDate)
                    {
                        if (!startList.ContainsKey(taskid))
                            continue;
                        foreach (GameConfig k in startList[taskid])
                        {
                            removeTask.Add(k.ID);
                        }
                    }
                    else
                    {
                        //移除不能重置的任务
                        noTask.Add(taskid);
                    }
                }
            }

            //删除过期任务
            if (removeTask.Count > 0)
            {
                DeleteTask(pb, removeTask, iscall);
            }
            foreach (GameConfig gc in startTask.Values)
            {
                if (noTask.Contains(gc.ID))
                    continue;
                TaskStartBusiness(pb, gc, iscall);
            }
        }
        #endregion


        /// <summary>
        /// 周式任务
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="npcid"></param>
        /// <param name="isremove"></param>
        public static void WeekTask(PlayerBusiness pb, string npcid, bool isremove)
        {
            string soleid = pb.ID + "WeekTask";
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                int cur = pb.WeekTotal(8);
                int max = Convert.ToInt32(TipManager.GetMessage(TaskReturn.LoopMax));
                if (cur >= max)
                    return;
                
                //表示一个环式任务都没有做的情况
                GameConfig gc = null;//要触发的任务
                Task t = TaskAccess.Instance.GetWeekTask(pb.ID);
                if (t == null)
                {
                    //触发新的任务
                    if (cur == 0)
                    {
                       gc = GameConfigAccess.Instance.GetFristWeek(pb.Level, 8);     
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(npcid))
                        {
                            gc = GameConfigAccess.Instance.GetWeekTask(npcid, 8, pb.Level);
                        }
                    }
                }
                else 
                {
                    DateTime dt = DateTime.Now;
                    
                    string taskid = t.Value.GetStringOrDefault("TaskID");
                    //任务触发时间
                    DateTime created = t.Created.ToLocalTime();
                    if (!ActivityManager.LocalWeek(created, dt)) 
                    {
                        //移除任务
                        DeleteTask(pb, new List<string>() { taskid }, isremove);
                        //触发第一个任务
                        gc = GameConfigAccess.Instance.GetFristWeek(pb.Level, 8);
                    }     
                }

                if (gc == null)
                    return;

                //表示没有任务
                Task task = TaskAccess.Instance.TaskBase(pb.ID, pb.RoleID, gc, (cur + 1), pb.Level);
                if (task == null)
                    return;

                if (isremove)
                {
                    pb.Call(TaskCommand.TaskActivationR, TaskAccess.Instance.GetTaskInfo(task));
                }
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }


        /// <summary>
        /// 主线任务
        /// </summary>
        /// <param name="obj"></param>
        public static void MainTask(string playerid, string id, int taskType)
        {
            try
            {
                if (!string.IsNullOrEmpty(playerid))
                {
                    List<Task> tasks = TaskAccess.Instance.GetTaskList(playerid);
                    for (int i = 0; i < tasks.Count; i++)
                    {
                        Task task = tasks[i];
                        if ((taskType == 0 || taskType == 8) &&id == task.ID)
                        {
                            continue;
                        }
                        Variant v = task.Value;
                        if ((v.GetIntOrDefault("TaskType") == taskType) && v.GetIntOrDefault("Status") >= 3)
                        {
                            //任务所有主线任务
                            TaskAccess.Instance.RemoveTask(playerid, task.ID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWrapper.Warn("MainTask:" + ex);
            }
        }


        /// <summary>
        /// 任务NPC检查
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="comm"></param>
        /// <param name="npcid"></param>
        /// <returns></returns>
        public static bool TaskNpcCheck(PlayerBusiness pb, string comm, string npcid)
        {
            Npc npc = NpcManager.Instance.FindOne(npcid);
            if (npc == null)
            {
                //pb.Call(comm, false, null, TipManager.GetMessage(TaskReturn.Npc1));
                return false;
            }
            //场景检查
            if (npc.SceneID != pb.SceneID)
            {
                //pb.Call(comm, false,null, TipManager.GetMessage(TaskReturn.Npc2));
                return false;
            }

            //距离检查
            //int x = npc.X - pb.X;
            //int y = npc.Y - pb.Y;
            //if (x * x + y * y > 100)
            //{
            //    pb.Call(comm, false, null, TipManager.GetMessage(TaskReturn.Npc3));
            //    return false;
            //}
            //功能检查:
            return true;
        }
    }
}
