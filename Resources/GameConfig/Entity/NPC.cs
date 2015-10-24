using System;
using System.Dynamic;
using Sinan.AMF3;
using Sinan.Data;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// NPC:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Serializable]
    public class Npc
    {
        int m_killLev;
        Variant m_task;
        Variant m_value;

        /// <summary>
        /// 编号
        /// </summary>
        public string ID
        {
            get;
            private set;
        }

        /// <summary>
        /// NPC类型
        /// 1:功能NPC，2:任务NPC,3:商业NPC
        /// 4:传送阵，8传送门
        /// 6:明雷，7采集物品
        /// </summary>
        public string NpcType
        {
            get;
            private set;
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        public string SceneID
        {
            get;
            private set;
        }

        public int X
        {
            get;
            private set;
        }

        public int Y
        {
            get;
            private set;
        }

        public Variant Value
        {
            get { return m_value; }
        }


        /// <summary>
        /// 秘境中是否必须清除
        /// 0:不限制
        /// 1:必须清除
        /// 2:必须清除,但需要先清除1
        /// 3:必须清除,但需要先清除1,2
        /// </summary>
        public int KillLev
        {
            get { return m_killLev; }
        }

        public Npc(Variant config)
        {
            ID = config.GetStringOrDefault("_id");
            NpcType = config.GetStringOrDefault("SubType");
            Name = config.GetStringOrDefault("Name");
            m_value = config.GetVariantOrDefault("Value");
            if (m_value != null)
            {
                m_task = m_value.GetVariantOrDefault("Task");
                SceneID = m_value.GetStringOrDefault("SceneID");
                X = m_value.GetIntOrDefault("X");
                Y = m_value.GetIntOrDefault("Y");
                object lev;
                if (m_value.TryGetValueT("MustKill", out lev))
                {
                    if (lev is bool)
                    {
                        if ((bool)lev)
                        {
                            m_killLev = 1;
                        }
                    }
                    else if (lev != null)
                    {
                        int.TryParse(lev.ToString(), out m_killLev);
                    }
                }
            }
        }

        public Variant GetTaskOrDefault(string taskID)
        {
            if (m_task == null) return null;
            if (string.IsNullOrEmpty(taskID))
            {
                taskID = "Default";
            }
            return m_task.GetVariantOrDefault(taskID) ?? m_task.GetVariantOrDefault("Default");
        }

        public Variant GetTaskOrDefault(string taskID, string sceneID)
        {
            if (this.SceneID == sceneID)
            {
                return GetTaskOrDefault(taskID);
            }
            return null;
        }
    }
}

