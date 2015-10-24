using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.Util;

namespace Sinan.GameModule
{
    /// <summary>
    /// 任务基本，访问配置文件.
    /// </summary>
    sealed public partial class GameConfigAccess
    {
        #region  任务基本方法
       static Dictionary<string, GameConfig> m_task = new Dictionary<string, GameConfig>();
        static Dictionary<string, List<GameConfig>> m_chufatype = new Dictionary<string, List<GameConfig>>();

        /// <summary>
        /// 得到任务列表
        /// </summary>
        /// <returns></returns>
        public void FindTaskInfo()
        {
            List<GameConfig> list = Find(MainType.Task);
            
            foreach (GameConfig gc in list)
            {
                if (!m_task.ContainsKey(gc.ID))
                {
                    m_task[gc.ID] = gc;
                }

                Variant v = gc.Value;
                string[] chufatype = v.GetStringOrDefault("ChufaType").Split(',');
                for (int i = 0; i < chufatype.Length; i++)
                {
                    if (string.IsNullOrEmpty(chufatype[i]))
                        continue;
                    List<GameConfig> typeList;
                    if (m_chufatype.TryGetValue(chufatype[i], out typeList))
                    {
                        typeList.Add(gc);
                    }
                    else
                    {
                        typeList = new List<GameConfig>();
                        typeList.Add(gc);
                        m_chufatype.Add(chufatype[i], typeList);
                    }
                }

            }
            //return m_task;
        }

        /// <summary>
        /// 得到指定列表
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<GameConfig> FindTaskConfig(IList ids)
        {
            //Dictionary<string, GameConfig> tasks = FindTask();
            List<GameConfig> gcs = new List<GameConfig>();
            foreach (string k in ids)
            {
                if (m_task.ContainsKey(k))
                {
                    gcs.Add(m_task[k]);
                }
            }
            return gcs;
        }

        /// <summary>
        /// 得到指定任务
        /// </summary>
        /// <param name="taskid">任务ID</param>
        /// <returns></returns>
        public GameConfig FindTaskId(string taskid)
        {
            //Dictionary<string, GameConfig> tasks = FindTask();
            GameConfig gc;
            m_task.TryGetValue(taskid, out  gc);
            return gc;
        }

        /// <summary>
        /// 获取所有配置信息
        /// </summary>
        /// <param name="TaskType">任务类型</param>
        /// <returns></returns>
        public List<GameConfig> FindTask(int TaskType)
        {
            //Dictionary<string, GameConfig> tasks = FindTask();
            List<GameConfig> list = new List<GameConfig>();
            if (m_task == null) return list;

            string[] strs = new string[m_task.Count];
            m_task.Keys.CopyTo(strs, 0);

            //foreach (GameConfig gc in tasks.Values)
            //{
            //    if (gc == null || gc.Value == null)
            //        continue;
            //    if (gc.Value.GetIntOrDefault("TaskType") == TaskType)
            //    {
            //        list.Add(gc);
            //    }
            //}

            foreach (string key in strs)
            {
                GameConfig gc;
                if (m_task.TryGetValue(key, out gc))
                {
                    if (gc.Value.GetIntOrDefault("TaskType") == TaskType)
                    {
                        list.Add(gc);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 得到满足条件的任务
        /// </summary>
        /// <param name="level">等级</param>
        /// <returns></returns>
        public List<GameConfig> FindChufaType(string tasktype)
        {
            //FindTask();
            if (m_chufatype.ContainsKey(tasktype))
                return m_chufatype[tasktype];
            return null;
        }

        /// <summary>
        /// 随机产生一个日常任务
        /// </summary>
        /// <param name="level"></param>
        /// <param name="status">2日常,7家族贡献任务</param>
        /// <returns></returns>
        public GameConfig GetDayTaskOne(int level, int status)
        {
            //得到所有日常任务
            List<GameConfig> tasks = FindTask(status);
            //得到满足条件的所有日常任务
            List<GameConfig> list = new List<GameConfig>();


            foreach (GameConfig gc in tasks)
            {
                IList chufa = gc.Value.GetValue<IList>("Chufa");
                Variant v = chufa[0] as Variant;
                if (v != null)
                {
                    //当前最小要求
                    int min = v.GetIntOrDefault("LevelMin");
                    //当前最大要求
                    int max = v.GetIntOrDefault("LevelMax");
                    //要求完成的任务ID
                    //IList t = v["TaskID"] as IList;
                    if (level >= min && level <= max)
                    {
                        list.Add(gc);
                    }
                }
            }

            if (list.Count == 0)
                return null;
            return list[NumberRandom.Next(0, list.Count)];
        }

        /// <summary>
        /// 随机得到循环任务
        /// </summary>
        /// <param name="npcid">得到NPC信息</param>
        ///<param name="tasktype">任务类型</param>
        ///<param name="level">当前角色等级</param>
        /// <returns></returns>
        public GameConfig GetWeekTask(string npcid, int tasktype, int level)
        {
            List<GameConfig> list = new List<GameConfig>();

            List<GameConfig> tasks = FindTask(tasktype);
            if (tasks == null)
            {
                return null;
            }
            foreach (GameConfig gc in tasks)
            {
                Variant v = gc.Value;
                if (v == null)
                    continue;

                IList info = v.GetValue<IList>("NPCBaseInfo");
                if (info == null)
                    continue;

                //发布NPC
                Variant npc = info[0] as Variant;
                if (npc == null)
                    continue;

                if (npc.GetStringOrDefault("NPCID") != npcid)
                    continue;

                IList chufa = v.GetValueOrDefault<IList>("Chufa");
                if (chufa.Count != 0)
                {
                    //任务触发限制
                    Variant limit = chufa[0] as Variant;
                    int levelMin = 0;
                    if (limit.TryGetValueT("LevelMin", out levelMin))
                    {
                        if (levelMin > level)
                            continue;
                    }

                    int levelMax = 0;
                    if (limit.TryGetValueT("LevelMax", out levelMax))
                    {
                        if (levelMax < level)
                            continue;
                    }
                }

                list.Add(gc);
            }

            if (list.Count == 0)
            {
                return null;
            }
            return list[NumberRandom.Next(0, list.Count)];
        }

        /// <summary>
        /// 取得周任务的开始任务
        /// </summary>
        /// <param name="level">当前等级</param>
        /// <param name="tasktype"></param>
        /// <returns></returns>
        public GameConfig GetFristWeek(int level, int tasktype)
        {
            List<GameConfig> list = new List<GameConfig>();
            List<GameConfig> tasks = FindTask(tasktype);
            if (tasks == null)
            {
                return null;
            }
            foreach (GameConfig gc in tasks)
            {
                Variant v = gc.Value;
                if (v == null) 
                    continue;
                if (v.GetIntOrDefault("TaskType") != tasktype)
                    continue;
                if (v.GetStringOrDefault("ChufaType") != "PlayerActivation") 
                    continue;

                IList chufa = v.GetValueOrDefault<IList>("Chufa");
                if (chufa.Count != 0)
                {
                    //任务触发限制
                    Variant limit = chufa[0] as Variant;
                    int levelMin = 0;
                    if (limit.TryGetValueT("LevelMin", out levelMin))
                    {
                        if (levelMin > level)
                            continue;
                    }

                    int levelMax = 0;
                    if (limit.TryGetValueT("LevelMax", out levelMax))
                    {
                        if (levelMax < level)
                            continue;
                    }
                }
                list.Add(gc);
            }

            if (list.Count == 0)
                return null;
            return list[NumberRandom.Next(0, list.Count)];
        }
        #endregion

    }
}