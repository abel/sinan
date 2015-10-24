using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Sinan.AMF3;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;
using Sinan.Data;

#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FrontServer
{
    /// <summary>
    /// 一般场景APC产生规则
    /// </summary>
    public class SceneApc : ExternalizableBase
    {
        protected bool m_visible;
        protected VisibleApc m_apc;
        protected List<Point> m_bornPlace;

        /// <summary>
        /// 能够战斗的APC
        /// </summary>
        ConcurrentDictionary<int, ActiveApc> m_activeApcs;
        ConcurrentDictionary<int, DateTime> m_showTime;

        /// <summary>
        /// 编号
        /// </summary>
        public string ID
        {
            get;
            set;
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 场景
        /// </summary>
        public string SceneID
        {
            get;
            private set;
        }

        /// <summary>
        /// 关联的明雷
        /// </summary>
        public string ApcID
        {
            get;
            set;
        }

        /// <summary>
        /// 矩形范围
        /// </summary>
        public Rectangle Range
        {
            get;
            set;
        }

        /// <summary>
        /// 生成数量
        /// </summary>
        public int Count
        {
            get;
            set;
        }

        /// <summary>
        /// 消失后再次出现的间隔
        /// </summary>
        public int GrowSecond
        {
            get;
            set;
        }

        /// <summary>
        /// 第1次出现的延迟时间(秒)
        /// (即活动开始多少秒后第一次出怪)
        /// </summary>
        public int DelaySecond
        {
            get;
            set;
        }

        /// <summary>
        ///  一次活动中出现的总数量(0,不限制)
        /// </summary>
        public int TotalCount
        {
            get;
            set;
        }

        /// <summary>
        /// 批次(批次大的必须等待批次小的死亡后才出现,0批次除外)
        /// </summary>
        public int Batch
        {
            get;
            set;
        }

        /// <summary>
        /// Normal:一般场景APC产生规则
        /// Part:  活动场景APC产生规则
        /// </summary>
        public string SubType
        {
            get;
            set;
        }

        /// <summary>
        /// 怪物是否可见
        /// </summary>
        public bool Visible
        {
            get { return m_visible; }
            set { m_visible = value; }
        }

        /// <summary>
        /// 进行战斗的怪
        /// </summary>
        public VisibleApc Apc
        {
            get
            {
                if (m_apc == null)
                {
                    m_apc = VisibleAPCManager.Instance.FindOne(ApcID);
                }
                return m_apc;
            }
        }

        public SceneApc(GameConfig config)
        {
            ID = config.ID;
            Name = config.Name;
            SubType = config.SubType;
            Variant v = config.Value;
            if (v != null)
            {
                SceneID = v.GetStringOrDefault("SceneID");
                ApcID = v.GetStringOrDefault("VisibleAPC");
                Count = v.GetIntOrDefault("Count");
                DelaySecond = v.GetIntOrDefault("DelaySecond");
                TotalCount = v.GetIntOrDefault("TotalCount");
                Batch = v.GetIntOrDefault("Batch");
                GrowSecond = v.GetIntOrDefault("GrowSecond");

                Range = RangeHelper.NewRectangle(v.GetVariantOrDefault("Range"), true);
            }

            m_showTime = new ConcurrentDictionary<int, DateTime>();
            m_activeApcs = new ConcurrentDictionary<int, ActiveApc>();
        }

        /// <summary>
        /// 初始化APC和降生点
        /// </summary>
        /// <param name="points"></param>
        public bool InitApc()
        {
            m_apc = VisibleAPCManager.Instance.FindOne(ApcID);
            if (m_apc != null)
            {
                SceneBusiness scene;
                if (ScenesProxy.TryGetScene(SceneID, out scene))
                {
                    m_bornPlace = scene.GetWalkPoints(Range);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 重置APC
        /// </summary>
        public void ResetApc()
        {
            m_showTime.Clear();
            m_activeApcs.Clear();
            DateTime showTime = DateTime.UtcNow.AddSeconds(DelaySecond);
            for (int i = 0; i < Count; i++)
            {
                m_showTime.TryAdd(i, showTime);
            }
        }

        /// <summary>
        /// 产生新的APC
        /// </summary>
        /// <returns></returns>
        public bool TryActiveApc()
        {
            bool refesh = false;
            if (m_activeApcs.Count < Count && m_showTime.Count > 0)
            {
                DateTime now = DateTime.UtcNow;
                foreach (var item in m_showTime)
                {
                    if (item.Value <= now)
                    {
                        ActiveApc apc = new ActiveApc();
                        apc.ID = item.Key;
                        if (m_bornPlace != null && m_bornPlace.Count > 0)
                        {
                            int index = NumberRandom.Next(m_bornPlace.Count);
                            apc.X = m_bornPlace[index].X;
                            apc.Y = m_bornPlace[index].Y;
                        }
                        m_activeApcs.TryAdd(apc.ID, apc);
                        DateTime old;
                        m_showTime.TryRemove(item.Key, out old);
                        refesh = true;
                    }
                }
            }
            return refesh;
        }

        /// <summary>
        /// 尝试进入战斗
        /// </summary>
        /// <param name="subID"></param>
        /// <returns></returns>
        public bool TryFight(int subID)
        {
            ActiveApc p;
            if (m_activeApcs.TryGetValue(subID, out p))
            {
                //状态由0变为1.
                if (Interlocked.CompareExchange(ref  p.State, 1, 0) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 明怪退出战斗
        /// </summary>
        /// <param name="subID">子ID</param>
        /// <param name="killed">是否被消灭</param>
        /// <returns></returns>
        public void ExitFight(int subID, bool killed)
        {
            ActiveApc p;
            if (killed)
            {
                if (m_activeApcs.TryRemove(subID, out p))
                {
                    if (GrowSecond > 0)
                    {
                        m_showTime.TryAdd(subID, DateTime.UtcNow.AddSeconds(GrowSecond));
                    }
                }
            }
            else
            {
                if (m_activeApcs.TryGetValue(subID, out p))
                {    //状态由1(战斗中)变为0(未战斗)
                    Interlocked.CompareExchange(ref  p.State, 0, 1);
                }
            }
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            if (m_activeApcs.Count > 0)
            {
                writer.WriteKey("ID");
                writer.WriteUTF(ID);
                writer.WriteKey("Name");
                writer.WriteUTF(Name);
                if (m_apc != null)
                {
                    writer.WriteKey("Skin");
                    writer.WriteUTF(m_apc.Skin);
                }
                writer.WriteKey("Apcs");
                writer.WriteValue(m_activeApcs.Values);
            }
        }
    }
}
