using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Util;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.GameModule
{
    public class TaskAccess : VariantBuilder<Task>
    {
        readonly static TaskAccess m_instance = new TaskAccess();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static TaskAccess Instance
        {
            get { return m_instance; }
        }

        TaskAccess()
            : base("Task")
        {
        }

        //得到角色的任务列表信息
        static ConcurrentDictionary<string, List<Task>> m_taskList = new ConcurrentDictionary<string, List<Task>>();

        public override bool Save(Task task)
        {
            m_collection.Save<Task>(task, SafeMode.True);
            return true;
        }

        /// <summary>
        /// 得到任务列表
        /// </summary>
        /// <param name="playerid"></param>
        /// <returns></returns>
        public List<Task> GetTaskList(string playerid)
        {
            List<Task> list;
            if (m_taskList.TryGetValue(playerid, out list))
            {
                return new List<Task>(list);
            }
            var query = Query.And(Query.EQ("PlayerID", playerid));
            list = m_collection.FindAs<Task>(query).ToList();
            if (m_taskList.TryAdd(playerid, list))
            {
                return new List<Task>(list);
            }
            else
            {
                m_taskList.TryGetValue(playerid, out list);
            }
            return new List<Task>(list);
        }


        /// <summary>
        /// 得到任务列表
        /// </summary>
        /// <param name="playerid"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<Task> GetTaskList(string playerid, int status)
        {
            return GetTaskList(playerid).FindAll(x => x != null && x.Value.GetIntOrDefault("Status") == status);
        }

        /// <summary>
        /// 得到领奖前任务列表
        /// </summary>
        /// <param name="playerid">角色</param>
        /// <returns></returns>
        public List<Task> GetTasks(string playerid)
        {
            return GetTaskList(playerid).FindAll(x => x != null && x.Value.GetIntOrDefault("Status") <= 2);
        }

        /// <summary>
        /// 得到指定任务信息
        /// </summary>
        /// <param name="playertaskid"></param>
        /// <param name="playerid"></param>
        /// <returns></returns>
        public Task GetTaskInfo(string playertaskid, string playerid)
        {
            return GetTaskList(playerid).Find(x => x != null && x.ID == playertaskid);
        }

        /// <summary>
        /// 检查任务是否存在
        /// </summary>
        /// <param name="playerid">用户id</param>
        /// <param name="taskid">任务id</param>
        /// <returns>true表示存在该任务</returns>
        public bool TaskCheck(string playerid, string taskid)
        {
            var tasklist = GetTaskList(playerid);
            return tasklist.Exists(x => x != null && x.Value.GetStringOrDefault("TaskID") == taskid);
        }

        /// <summary>
        /// 移除任务
        /// </summary>
        /// <param name="playerid">角色</param>
        /// <param name="id">任务ID</param>
        /// <returns></returns>
        public bool RemoveTask(string playerid, string id)
        {
            Task task = GetTaskInfo(id, playerid);
            if (task != null)
            {
                var query = Query.And(Query.EQ("PlayerID", playerid), Query.EQ("_id", id));
                var r = m_collection.Remove(query, SafeMode.False);

                List<Task> source;
                if(m_taskList.TryGetValue(playerid, out source))
                {
                    return source.Remove(task);
                }
            }
            return false;
        }

        /// <summary>
        /// 得到指定配置的任务列表
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<Task> FindConfigList(string playerid, IList ids)
        {
            return GetTaskList(playerid).FindAll(x => x != null && ids.Contains(x.Value.GetStringOrDefault("TaskID")));
        }

        /// <summary>
        /// 得到指定任务列表的相关状态
        /// </summary>
        /// <param name="playerid">角色</param>
        /// <param name="taskid">前置任务</param>
        /// <returns>true表示前置任务完成</returns>
        public bool FindConfigList(string playerid, string taskid)
        {
            var tasklist = GetTaskList(playerid);
            for (int i = 0; i < tasklist.Count; i++)
            {
                Task k = tasklist[i];
                if (k == null)
                    continue;
                Variant v = k.Value;
                if (v.GetStringOrDefault("TaskID") != taskid)
                    continue;
                //判断前置任务完成情况
                if (v.GetIntOrDefault("Status") < 3)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 移除往日的日常任务
        /// </summary>
        /// <param name="playerid">角色ID</param>
        /// <param name="status">2,7</param>
        /// <returns></returns>
        public List<string> Remove_1(string playerid,int status)
        {
            var tasklist = GetTaskList(playerid);
            List<Task> source;
            m_taskList.TryGetValue(playerid, out source);
            //要求移除的任务
            List<string> ids = new List<string>();
            for (int i = 0; i < tasklist.Count; i++)
            {
                Task task = tasklist[i];
                if (task == null)
                    continue;
                int tasktype = task.Value.GetIntOrDefault("TaskType");
                if (tasktype != status)
                    continue;
                DateTime Created = task.Created;
                if (Created.ToLocalTime().Date == DateTime.UtcNow.ToLocalTime().Date)
                    continue;
                ids.Add(task.ID);
                source.Remove(task);
            }

            if (ids.Count > 0)
            {
                BsonValue[] q = new BsonValue[ids.Count];
                for (int i = 0; i < ids.Count; i++)
                {
                    q[i] = new BsonString(ids[i].ToString());
                }
                //移除过期任务
                var query = Query.And(Query.EQ("PlayerID", playerid), Query.In("_id", q));
                m_collection.Remove(query, SafeMode.False);
            }
            return ids;
        }

        /// <summary>
        /// 是否存在没有领奖的        
        /// </summary>
        /// <param name="playerid"></param>
        /// <param name="status">2,7</param>
        /// <returns></returns>
        public bool IsDayTask(string playerid,int status)
        {
            var tasklist = GetTaskList(playerid);
            for (int i = 0; i < tasklist.Count; i++)
            {
                Task task = tasklist[i];
                int tasktype = task.Value.GetIntOrDefault("TaskType");
                if (tasktype != status)
                    continue;
                //DateTime Created = task.Created;//任务触发时间
                //判断是否存在没有完成的日常任务
                if (task.Value.GetIntOrDefault("Status") < 3)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 批量移除
        /// </summary>
        /// <param name="playerid"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool Remove(string playerid, IList ids)
        {
            var tasklist = GetTaskList(playerid);
            List<Task> source;
            m_taskList.TryGetValue(playerid, out source);

            for (int i = tasklist.Count - 1; i >= 0; i--)
            {
                Task task = tasklist[i];
                string taskid = task.Value.GetStringOrDefault("TaskID");

                if (ids.Contains(taskid))
                {
                    source.Remove(task);
                }
            }

            if (source.Count == 0)
            {
                m_taskList.TryRemove(playerid, out source);
            }

            if (ids.Count > 0)
            {
                BsonValue[] q = new BsonValue[ids.Count];
                for (int i = 0; i < q.Length; i++)
                {
                    q[i] = new BsonString(ids[i].ToString());
                }
                IMongoQuery qc = Query.And(Query.EQ("PlayerID", playerid), Query.In("Value.TaskID", q));
                m_collection.Remove(qc, SafeMode.False);
            }
            return true;
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="playerid"></param>
        /// <returns></returns>
        public bool Remove(string playerid)
        {
            List<Task> tasks;
            return m_taskList.TryRemove(playerid, out tasks);
        }

        /// <summary>
        /// 任务基本信息
        /// </summary>
        /// <param name="playerid">角色id</param>
        /// <param name="roleid">角色类型</param>
        /// <param name="v"></param>
        /// <param name="taskid"></param>
        /// <param name="taskname"></param>
        /// <returns></returns>
        public Task TaskBase(string playerid, string roleid, GameConfig gc, int loopcount, int level)
        {
            Variant v = new Variant();
            v.Add("TaskID", gc.ID);
            v.Add("Status", gc.Value.GetIntOrDefault("BeginStatus"));
            v.Add("IsShow", 0);
            v.Add("Update", DateTime.UtcNow);

            int taskType = gc.Value.GetIntOrDefault("TaskType");
            IList finish = gc.Value.GetValue<IList>("Finish");
            List<Variant> fn = new List<Variant>();
            foreach (Variant k in finish)
            {
                Variant m = new Variant();
                foreach (var item in k)
                {
                    m.Add(item.Key, item.Value);
                }
                fn.Add(m);
            }
            

            IList award = gc.Value.GetValue<IList>("Award");
            List<Variant> aw = new List<Variant>();
            foreach (Variant k in award)
            {
                string fz = k.GetStringOrDefault("fz");
                if (string.IsNullOrEmpty(fz) || fz == roleid)
                {
                    Variant m = new Variant();
                    foreach (var item in k)
                    {
                        m.Add(item.Key, item.Value);
                    }
                    aw.Add(m);
                }
            }


            if (taskType == 8)
            {
                //环式任务根据不出的环数以及角色不同等级，产生不同的奖励
                List<string> lv = new List<string>() { "Experience", "PetExperience", "Score" };
                foreach (Variant info in aw)
                {
                    string name = info.GetStringOrDefault("Name");
                    if (lv.Contains(name))
                    {
                        int count = info.GetIntOrDefault("Count");
                        info["Count"] = TaskAwardMethod(count, loopcount, level);
                    }
                }
            }
            

            v.Add("Finish", fn);
            v.Add("Award", aw);
            v.Add("TaskType", taskType);//任务类型


            Task task = new Task();
            task.ID = ObjectId.GenerateNewId().ToString();
            task.PlayerID = playerid;
            task.MainType = "Task";
            task.Value = v;
            task.Name = gc.Name;
            task.Created = DateTime.UtcNow;
            //task.Changed = true;
            if (task.Save())
            {
                List<Task> tasklist;
                if (m_taskList.TryGetValue(playerid, out tasklist))
                {
                    tasklist.Add(task);
                }
                else
                {
                    m_taskList.TryAdd(playerid, new List<Task>() { task });
                }
                return task;
            }
            return null;
        }

        /// <summary>
        /// 得到任务基本信息
        /// </summary>
        /// <param name="gc">任务基本配置信息</param>
        /// <param name="task">激活任务基本信息</param>
        /// <returns></returns>
        public Variant GetTaskInfo(Task task)
        {
            Variant v = new Variant();
            v.Add("ID", task.ID);
            v.Add("TaskID", task.Value["TaskID"]);
            v.Add("Status", task.Value["Status"]);
            v.Add("IsShow", task.Value["IsShow"]);
            v.Add("Finish", task.Value["Finish"]);
            v.Add("Award", task.Value["Award"]);
            v.Add("Update", task.Value["Update"]);
            //v.Add("TaskType",task.Value["TaskType"]);

            return v;
        }

        /// <summary>
        /// 任务限制
        /// </summary>
        /// <param name="playerid">角色</param>
        /// <param name="level">角色</param>
        /// <param name="v">相关限制</param>
        /// <returns></returns>
        public bool TaskLimit(string playerid, int level, string taskid, Variant v)
        {
            if (TaskAccess.Instance.TaskCheck(playerid, taskid))
                return false;

            IList chufa = v.GetValueOrDefault<IList>("Chufa");
            if (chufa.Count != 0)
            {

                //任务触发限制
                Variant limit = chufa[0] as Variant;
                int levelmin = 0;
                if (limit.TryGetValueT("LevelMin", out levelmin))
                {
                    if (levelmin > level)
                        return false;
                }
                int levelmax = 0;

                if (limit.TryGetValueT("LevelMax", out levelmax))
                {
                    if (levelmax < level)
                        return false;
                }

                //最底等级限制
                //if (limit.ContainsKey("LevelMin") && limit.GetIntOrDefault("LevelMin") > level)
                //{
                //    return false;
                //}
                //最高等级限制
                //if (limit.ContainsKey("LevelMax") && limit.GetIntOrDefault("LevelMax") < level)
                //{
                //    return false;
                //}

                //表示技线任务，如果当前等级已经大于触发等级则不能触发
                if (v.GetIntOrDefault("TaskType") == 1)
                {
                    //表示角色升级触发
                    if (v.GetStringOrDefault("ChufaType") == "PlayerActivation")
                    {
                        //if (limit.ContainsKey("LevelMin") && level > limit.GetIntOrDefault("LevelMin"))
                        //{
                        //    return false;
                        //}
                        if (levelmin > 0 && level > levelmin)
                        {
                            return false;
                        }
                    }
                }

                if (limit.ContainsKey("TaskID"))
                {
                    IList TaskID = limit.GetValueOrDefault<IList>("TaskID");
                    if (TaskID.Count > 0)
                    {
                        foreach (string t in TaskID)
                        {
                            if (string.IsNullOrEmpty(t))
                                continue;

                            //表示前置任务没有完成
                            if (!TaskAccess.Instance.FindConfigList(playerid, t))
                                return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 判断是否存在改任务
        /// </summary>
        /// <param name="playerid">角色</param>
        /// <param name="taskid">任务ID</param>
        /// <returns></returns>
        public bool IsTask(string playerid, string taskid)
        {
            var taskList = GetTaskList(playerid);
            for (int i = 0; i < taskList.Count; i++)
            {
                Task k = taskList[i];
                if (k == null) continue;
                if (k.Value.GetStringOrDefault("TaskID") == taskid && k.Value.GetIntOrDefault("Status") == 1)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 得到周任务,类型为3
        /// </summary>
        /// <param name="playerid">角色</param>
        /// <returns></returns>
        public Task GetWeekTask(string playerid)
        {
            List<Task> list = GetTasks(playerid);
            foreach (Task t in list)
            {
                Variant v = t.Value;
                if (v == null)
                    continue;
                if (v.GetIntOrDefault("TaskType") == 8 && v.GetIntOrDefault("Status") <= 2)
                {
                    return t;
                }
            }
            return null;
        }


        /// <summary>
        /// 任务
        /// </summary>
        /// <param name="baseCount">基础值</param>
        /// <param name="loopcount">环数</param>
        /// <param name="level">等级</param>
        /// <returns></returns>
        public int TaskAwardMethod(int baseCount, int loopcount, int level) 
        {
            if (level == 0)
            {
                return baseCount;
            }
            int count = Convert.ToInt32(baseCount * (Math.Pow(1.02, (loopcount - 1)) * Math.Pow((double)level / 45, 2.5)));
            if (count <= 0)
                return 10;
            return count;
        }
    }
}
