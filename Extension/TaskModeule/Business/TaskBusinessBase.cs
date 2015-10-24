using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.FrontServer;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Command;
using System.Collections;
using Sinan.Entity;
using Sinan.Util;
using Sinan.Log;

#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.TaskModule.Business
{
    public class TaskBusinessBase
    {
        static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 任务触发
        /// </summary>
        /// <param name="note"></param>
        public static void TaskActivation(UserNote note)
        {
            List<GameConfig> tasklist = GameConfigAccess.Instance.FindChufaType(note.Name);
            if (tasklist == null || tasklist.Count == 0)
                return;

            foreach (GameConfig gc in tasklist)
            {
                int taskType = gc.Value.GetIntOrDefault("TaskType");
                //日常任务,家族贡献任务不通过这种方式chu
                if (taskType == 2 || taskType == 7 || taskType == 8)
                    continue;
                if (taskType == 6)
                {
                    if (!IsFamily(note.Player))
                        continue;
                }
                TaskBusiness.TaskStart(note, gc);
            }

            if (note.Name != UserPlayerCommand.CreatePlayerSuccess)
            {
                TaskBusiness.WeekTask(note.Player, "", true);

                TaskBusiness.DayTask(note.Player);
                ActFamilyTask(note);
                TaskBusiness.TaskAPC(note);
            }
        }

        /// <summary>
        /// 链式任务的触发
        /// </summary>
        /// <param name="list">关联任务列表</param>
        public static void TaskFinishActivation(UserNote note, IList list)
        {
            List<GameConfig> config = GameConfigAccess.Instance.FindTaskConfig(list);
            //string playerid = note.PlayerID;
            //int level = note.Player.Level;
            foreach (GameConfig gc in config)
            {
                int taskType = gc.Value.GetIntOrDefault("TaskType");
                if (taskType == 6 || taskType == 7)
                {
                    if (!IsFamily(note.Player))
                        continue;
                }
                TaskBusiness.TaskStart(note, gc);
            }
            TaskBusiness.TaskAPC(note);

        }

        /// <summary>
        /// 登录成功得到任务列表
        /// </summary>
        /// <param name="note"></param>
        public static void TaskList(UserNote note)
        {
            //新加载任务列表
            TaskAccess.Instance.GetTaskList(note.PlayerID);
            //日常任务
            TaskBusiness.DayTask(note.Player, false);
            //环式任务
            TaskBusiness.LoopTask(note.Player, false, new List<int>() { 4, 6 });
            //家族任务
            TaskBusiness.FamilyTask(note.Player, false);
            //登录时不能发送给客户端
            TaskBusiness.WeekTask(note.Player, "", false);

            List<Task> gs = TaskBusiness.TaskAPC(note);
            //得到角色任务列表
            foreach (Task g in gs)
            {
                Variant v = g.Value;
                GameConfig gc = GameConfigAccess.Instance.FindTaskId(v.GetStringOrDefault("TaskID"));
                if (gc == null)
                    continue;
                Variant d = TaskAccess.Instance.GetTaskInfo(g);
                if (d.GetIntOrDefault("Status") == 3)
                    continue;
                note.Call(TaskCommand.PlayerTaskListR, d);
            }
            note.Call(TaskCommand.PlayerTaskListR, true);
        }

        /// <summary>
        /// 更新任务
        /// </summary>
        /// <param name="note"></param>
        /// <param name="obj">要求更新的任务</param>
        public static void UpdateTask_1(UserNote note)
        {
            string id = note.GetString(0);

            Task task = TaskAccess.Instance.GetTaskInfo(id, note.PlayerID);
            if (task == null)
            {
                note.Call(TaskCommand.UpdateTaskR, false, TipManager.GetMessage(TaskReturn.TaskError));
                return;
            }

            GameConfig gc = GameConfigAccess.Instance.FindTaskId(task.Value.GetStringOrDefault("TaskID"));
            if (gc == null)
            {
                note.Call(TaskCommand.UpdateTaskR, false, TipManager.GetMessage(TaskReturn.TaskError));
                return;
            }
            FinishBusiness.UpdateFinishTask(note, gc, task);
        }

        /// <summary>
        /// 角色等级升级，更新任务相关信息
        /// </summary>
        /// <param name="note"></param>
        public static void UpdateTask(UserNote note)
        {
            List<Task> taskList = TaskAccess.Instance.GetTaskList(note.PlayerID, 1);
            for (int i = 0; i < taskList.Count; i++)
            {
                Task task = taskList[i] as Task;
                if (task == null) continue;
                GameConfig gc = GameConfigAccess.Instance.FindTaskId(task.Value.GetStringOrDefault("TaskID"));

                if (gc != null)
                {
                    FinishBusiness.UpdateFinishTask(note, gc, task);
                }
            }
        }

        /// <summary>
        /// 战斗结束更新相关任务
        /// </summary>
        /// <param name="note"></param>
        public static void FightingTask(UserNote note)
        {
            ////得到进行中的任务
            List<Task> tasks = TaskAccess.Instance.GetTaskList(note.PlayerID, 1);

            Dictionary<string, int> apcs = note[0] as Dictionary<string, int>;
            if (apcs.Count > 0)
            {
                Dictionary<string, int> n_apc = new Dictionary<string, int>();
                int num = 0;
                foreach (string k in apcs.Keys)
                {
                    num += apcs[k];
                    if (tasks.Count > 0)
                    {
                        if (note.Player.TaskAPC.Contains(k))
                        {
                            if (n_apc.ContainsKey(k))
                            {
                                n_apc[k] += apcs[k];
                            }
                            else
                            {
                                n_apc.Add(k, apcs[k]);
                            }
                        }
                    }
                }
                if (n_apc.Count > 0)
                {
                    FinishBusiness.TaskFinish10001(note, tasks, n_apc);
                }
                if (num > 0)
                {
                    //Console.WriteLine("怪的数量:"+num);
                    note.Player.AddAcivity(ActivityType.APCCount, num);
                }
            }
        }

        /// <summary>
        /// 道具关联任务
        /// </summary>
        /// <param name="note"></param>
        public static void GoodsTask(UserNote note)
        {
            List<Task> tasks = TaskAccess.Instance.GetTasks(note.PlayerID);
            //发生变化的道具            
            string goodsid = note.GetString(0);
            //得到指定道具的数量
            int num = BurdenManager.GoodsCount(note.Player.B0, goodsid);
            for (int j = 0; j < tasks.Count; j++)
            {
                Task t = tasks[j] as Task;
                if (t == null)
                    continue;
                IList finish = t.Value["Finish"] as IList;
                bool isChange = false;
                for (int i = 0; i < finish.Count; i++)
                {
                    Variant f = finish[i] as Variant;
                    int type = f.GetIntOrDefault("Type");
                    if (type == 10004 && f.GetStringOrDefault("GoodsID") == goodsid)
                    {
                        if (num >= f.GetIntOrDefault("Total"))
                        {
                            f["Cur"] = f.GetIntOrDefault("Total");
                        }
                        else
                        {
                            f["Cur"] = num;
                        }
                        isChange = true;
                    }

                    if (type == 10003 && f.GetStringOrDefault("GoodsID") == goodsid)
                    {
                        if (num >= f.GetIntOrDefault("Total"))
                        {
                            f["Cur"] = f.GetIntOrDefault("Total");
                        }
                        else
                        {
                            f["Cur"] = num;
                        }
                        isChange = true;
                    }
                }

                //判断是否已经完成,true表示已经完成                
                if (isChange)
                {
                    bool isfinish = true;//判断任务是否已经完成
                    foreach (Variant f in finish)
                    {
                        if (f.GetIntOrDefault("Total") != f.GetIntOrDefault("Cur"))
                        {
                            isfinish = false;
                            break;
                        }
                    }

                    t.Value["Status"] = isfinish ? 2 : 1;
                    t.Save();
                    note.Call(TaskCommand.UpdateTaskR, true, TaskAccess.Instance.GetTaskInfo(t));
                    TaskBusiness.TaskAPC(note);
                }
            }
        }

        /// <summary>
        /// 任务奖励
        /// </summary>
        /// <param name="note"></param>
        /// <param name="obj">任务奖励</param>
        public static void Award(UserNote note)
        {
            string soleid = note.PlayerID + "Award";
            if (!m_dic.TryAdd(soleid, soleid))
            {
                return;
            }

            try
            {

                string id = note.GetString(0);
                IList list = note[1] as IList;

                #region 奖励判断

                //得到指定的任务
                Task task = TaskAccess.Instance.GetTaskInfo(id, note.PlayerID);
                if (task == null)
                {
                    note.Call(TaskCommand.AwardR, false, null, TipManager.GetMessage(TaskReturn.TaskError));
                    return;
                }



                //得到基本配置表
                GameConfig gc = GameConfigAccess.Instance.FindTaskId(task.Value.GetStringOrDefault("TaskID"));
                if (gc == null)
                {
                    note.Call(TaskCommand.AwardR, false, null, TipManager.GetMessage(TaskReturn.TaskError));
                    return;
                }

                IList npcInfo = gc.Value.GetValue<IList>("NPCBaseInfo");
                if (npcInfo == null)
                    return;
                Variant npc = null;
                foreach (Variant model in npcInfo)
                {
                    if (model.GetIntOrDefault("Type") == 1)
                    {
                        npc = model;
                        break;
                    }
                }

                if (npc == null)
                {
                    return;
                }

                string npcid = npc.GetStringOrDefault("NPCID");

                if (!TaskBusiness.TaskNpcCheck(note.Player, TaskCommand.AwardR, npcid))
                {
                    note.Call(TaskCommand.AwardR, false, null, TipManager.GetMessage(TaskReturn.Npc3));
                    return;
                }

                //表示任务完成条件已经满足,可以领奖
                if (task.Value.GetIntOrDefault("Status") != 2)
                {
                    note.Call(TaskCommand.AwardR, false, null, TipManager.GetMessage(TaskReturn.TaskAwardFail));
                    return;
                }
                ///得到包袱列表
                PlayerEx burden = note.Player.B0;


                ///物品绑定状态
                Dictionary<string, int> isbinding = new Dictionary<string, int>();

                IList finish = task.Value.GetValue<IList>("Finish");
                bool isback = false;
                foreach (Variant f in finish)
                {
                    if (f.GetStringOrDefault("Type") == "10004")
                    {
                        isback = true;
                        break;
                    }
                }
                //bool IsGoods = false;
                ///得到奖励条件
                IList award = task.Value.GetValue<IList>("Award");

                ///得到可选的奖励


                List<Variant> fixAward = new List<Variant>();


                List<string> select = new List<string>();//可选的奖励

                int m = 0;
                int n = 0;
                foreach (Variant w in award)
                {
                    int selectType = w.GetIntOrDefault("SelectType");
                    string awardID = w.GetStringOrDefault("AwardID");
                    if (selectType > 0)
                    {
                        //可选择奖励的数量
                        if (m == 0) m = selectType;
                        select.Add(awardID);
                        if (list.Contains(awardID))
                        {
                            fixAward.Add(w);
                            n++;
                        }
                    }
                    else
                    {
                        fixAward.Add(w);
                    }
                }

                if (m != n)
                {
                    note.Call(TaskCommand.AwardR, false, null, TipManager.GetMessage(TaskReturn.SelectAwardError));
                    return;
                }



                bool noGoods = false;//非道具的情况
                ///宠物
                Dictionary<string, int> pets = new Dictionary<string, int>();

                Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
                foreach (Variant fix in fixAward)
                {
                    if (fix.GetStringOrDefault("Type") != "20002")
                    {
                        noGoods = true;
                        continue;
                    }

                    string goodsid = fix.GetStringOrDefault("GoodsID");
                    int count = fix.GetIntOrDefault("Count");

                    if (fix.GetStringOrDefault("GoodsType") == "112009")
                    {
                        if (!pets.ContainsKey(goodsid))
                        {
                            pets.Add(goodsid, count);
                        }
                        else
                        {
                            pets[goodsid] += count;
                        }
                        continue;
                    }

                    Variant tmp;
                    if (dic.TryGetValue(goodsid, out tmp))
                    {
                        tmp.SetOrInc("Number1", count);
                    }
                    else
                    {
                        tmp = new Variant();
                        tmp.Add("Number1", count);
                        GameConfig gc1 = GameConfigAccess.Instance.FindOneById(goodsid);
                        if (gc1 == null)
                            continue;
                        GoodsAccess.Instance.TimeLines(gc1, tmp);
                        dic.Add(goodsid, tmp);
                    }
                }


                if (dic.Count > 0)
                {
                    if (BurdenManager.IsFullBurden(burden, dic))
                    {
                        note.Call(TaskCommand.AwardR, false, null, TipManager.GetMessage(TaskReturn.BurdenFull));
                        return;
                    }
                }



                #endregion

                #region 任务奖励处理
                task.Value["Status"] = 3;
                if (!task.Save())
                    return;

                //领奖成功
                note.Call(TaskCommand.AwardR, true, null, task.ID);
                int tasktype = task.Value.GetIntOrDefault("TaskType");
                string taskid = task.Value.GetStringOrDefault("TaskID");

                TaskType(note.Player, tasktype.ToString());

                Variant gs = new Variant();
                //得到物品奖励
                if (dic.Count > 0)
                {
                    Dictionary<string, int> info = note.Player.AddGoods(dic, GoodsSource.TaskAward);
                    foreach (var item in info) 
                    {
                        gs.SetOrInc(item.Key, item.Value);
                    }
                }


                //得到宠物奖励
                if (pets.Count > 0)
                {
                    AwardBusiness.TaskAward112009(note, pets);
                }

                Variant us = new Variant();
                //物品回收处理
                if (isback)
                {
                    us = FinishBusiness.TaskFinish10004(note, finish);
                }
                Variant os = new Variant();
                //非道具的奖励
                if (noGoods)
                {
                    if (task.Value.GetIntOrDefault("TaskType") == 2)
                    {
                        os=AwardBusiness.TaskAward20001(note, award, true);
                    }
                    else
                    {
                        os=AwardBusiness.TaskAward20001(note, award);
                    }
                }
                os["ID"] = task.ID;
                os["TaskID"] = taskid;
                os["TaskType"] = tasktype;

                string des = gc.Value.GetStringOrDefault("Description");
                string[] msg = des.Split('|');
                if (msg.Length > 3) AwardBusiness.TaskEmail(note, msg);


                #endregion

                #region 触发新的任务

                switch (tasktype)
                {
                    case 2:
                        note.Player.AddAcivity(ActivityType.DayTask, 1);
                        break;
                    case 4:
                        note.Player.AddAcivity(ActivityType.LoopTask, 1);
                        
                        break;

                }

                if (tasktype == 2)
                {
                    TaskBusiness.DayTask(note.Player, true);
                }
                else if (tasktype == 7)
                {
                    TaskBusiness.FamilyTask(note.Player, true);
                }
                else if (tasktype == 8)
                {
                    TaskBusiness.WeekTask(note.Player, npcid, true);
                }
                else
                {
                    List<string> reatTask = new List<string>();
                    string[] strs = gc.Value.GetStringOrDefault("RearTask").Split(',');
                    for (int i = 0; i < strs.Length; i++)
                    {
                        if (string.IsNullOrEmpty(strs[i]))
                            continue;
                        reatTask.Add(strs[i]);
                    }
                    if (reatTask.Count > 0)
                    {
                        TaskFinishActivation(note, reatTask);
                    }
                }
                #endregion

                #region 任务成就

                note.Player.FinishNote(FinishCommand.TotalTask, taskid, tasktype);

                if (tasktype == 0 || tasktype == 1 || tasktype == 2 || tasktype == 8)
                {
                    //移除主线任务,支线任务
                    TaskBusiness.MainTask(note.PlayerID, task.ID, tasktype);
                }
                #endregion

                note.Player.AddLogVariant(Actiontype.TaskAward, us, gs, os);
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 放弃任务
        /// </summary>
        /// <param name="note"></param>
        /// <param name="playerTaskID">放弃任务ID</param>
        public static void Giveup(UserNote note, string playerTaskID)
        {
            //得到指定的任务
            Task task = TaskAccess.Instance.GetTaskInfo(playerTaskID, note.PlayerID);
            if (task == null)
            {
                note.Call(TaskCommand.GiveupR, false, TipManager.GetMessage(TaskReturn.TaskError));
                return;
            }

            Variant v = task.Value;

            GameConfig gc = GameConfigAccess.Instance.FindTaskId(task.Value["TaskID"].ToString());
            if (gc == null)
            {
                note.Call(TaskCommand.GiveupR, false, TipManager.GetMessage(TaskReturn.TaskError));
                return;
            }
            Variant vc = gc.Value;
            int taskType = vc.GetIntOrDefault("TaskType");
            if (taskType == 0 || taskType == 8)
            {
                note.Call(TaskCommand.GiveupR, false, TipManager.GetMessage(TaskReturn.TaskMainGiveup));
                return;
            }

            if (v.GetIntOrDefault("Status") == 3)
            {
                note.Call(TaskCommand.GiveupR, false, TipManager.GetMessage(TaskReturn.TaskNoGiveup));
                return;
            }
            if (FinishBusiness.TaskGiveup(note.Player, task))
                return;
            TaskAccess.Instance.RemoveTask(note.PlayerID, task.ID);
            if (taskType != 2)
            {
                TaskFinishActivation(note, new List<string> { gc.ID });
                TaskBusiness.TaskAPC(note);
            }
            else
            {
                //如果为日常任务,是否还可以接新的日常任务
                TaskBusiness.DayTask(note.Player, true);
            }
        }

        /// <summary>
        /// 是否显示
        /// </summary>
        /// <param name="note"></param>
        /// <param name="playerTaskID">任务ID</param>
        public static void IsShow(UserNote note, string playerTaskID)
        {
            Task task = TaskAccess.Instance.GetTaskInfo(playerTaskID, note.PlayerID);
            if (task == null)
            {
                note.Call(TaskCommand.IsShowR, false, TipManager.GetMessage(TaskReturn.TaskError));
                return;
            }

            Variant v = task.Value;
            v["IsShow"] = v.GetIntOrDefault("IsShow") == 0 ? 1 : 0;
            note.Call(TaskCommand.IsShowR, true, task.ID, v.GetIntOrDefault("IsShow"));
            task.Save();
        }

        /// <summary>
        /// 采集任务
        /// </summary>
        /// <param name="note"></param>
        public static void TaskCollect(UserNote note)
        {
            string taskid = note.GetString(0);
            string npcid = note.GetString(1);

            Npc npcinfo = NpcManager.Instance.FindOne(npcid);
            if (npcinfo == null) return;

            string goodsid = npcinfo.Value.GetStringOrDefault("GoodsID");
            Task task = TaskAccess.Instance.GetTaskInfo(taskid, note.PlayerID);
            if (task == null)
                return;

            GameConfig gc = GameConfigAccess.Instance.FindTaskId(task.Value.GetStringOrDefault("TaskID"));
            if (gc == null)
                return;

            if (task.Value.GetIntOrDefault("Status") == 1)
            {
                IList finish = task.Value.GetValue<IList>("Finish");
                //表示要已经接收
                for (int i = 0; i < finish.Count; i++)
                {
                    Variant v = finish[i] as Variant;
                    if (v == null)
                        continue;
                    if (v.GetIntOrDefault("Type") != 10009)
                        continue;

                    if (FinishBusiness.TaskFinish10009(note, v, goodsid))
                        return;
                }

                for (int i = 0; i < finish.Count; i++)
                {
                    Variant v = finish[i] as Variant;
                    if (v == null)
                        continue;
                    if (v.GetIntOrDefault("Type") == 10004)
                    {
                        FinishBusiness.Task10004(note, v);
                    }
                }

                bool ishave = true;

                foreach (Variant v in finish)
                {
                    if (v.GetIntOrDefault("Total") != v.GetIntOrDefault("Cur"))
                    {
                        ishave = false;
                        break;
                    }
                }

                if (ishave)
                {
                    task.Value["Status"] = 2;
                }

                task.Save();
                note.Call(TaskCommand.UpdateTaskR, true, TaskAccess.Instance.GetTaskInfo(task));
                TaskBusiness.TaskAPC(note);
            }
        }

        /// <summary>
        /// 触发家族任务
        /// </summary>
        /// <param name="note"></param>
        public static void ActFamilyTask(UserNote note) 
        {
            TaskBusiness.LoopTask(note.Player, false, new List<int>() { 6 });
            //家族任务
            TaskBusiness.FamilyTask(note.Player, true);
        }
        /// <summary>
        /// 记录不同任务类型每天完成情况
        /// </summary>
        /// <param name="pb">任务id</param>
        /// <param name="tasktype">任务类型</param>
        private static void TaskType(PlayerBusiness pb, string tasktype)
        {
            PlayerEx fx = pb.TaskDay;
            Variant v = fx.Value;
            if (v == null)
            {
                fx.Value = new Variant();
                v = fx.Value;
            }

            DateTime dt = DateTime.Now;
            //保留7天记录
            int delKey = Convert.ToInt32(dt.AddDays(-10).Date.ToString("yyyyMMdd"));
            //需要移除的Key
            List<string> keys = new List<string>();
            foreach (var item in v)
            {
                if (Convert.ToInt32(item.Key) < delKey)
                {
                    keys.Add(item.Key);
                }
            }

            if (keys.Count > 0)
            {
                foreach (string k in keys)
                {
                    v.Remove(k);
                }
            }

            string day = dt.Date.ToString("yyyyMMdd");
            Variant info;
            if (v.TryGetValueT(day, out info))
            {
                info.SetOrInc(tasktype, 1);
            }
            else
            {
                info = new Variant();
                info.SetOrInc(tasktype, 1);
                v.Add(day, info);
            }
            fx.Save();
            pb.Call(ClientCommand.UpdateActorR, new PlayerExDetail(fx));
        }

        /// <summary>
        /// 判断角色是否存在家族
        /// </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        public static bool IsFamily(PlayerBusiness pb)
        {
            PlayerEx fx = pb.Family;
            if (fx == null)
                return false;
            Variant v = fx.Value;
            if (v == null)
                return false;
            if (string.IsNullOrEmpty(v.GetStringOrDefault("FamilyID")))
                return false;
            return true;
        }
    }
}
