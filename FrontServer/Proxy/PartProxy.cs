using System;
using System.Collections.Generic;
using log4net;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Schedule;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FrontServer
{
    /// <summary>
    /// 活动代理,用于加载活动
    /// 管理所有活动的开始和结束
    /// </summary>
    sealed public class PartProxy : SchedulerBase
    {
        private static ILog logger = LogManager.GetLogger("GMLog");
        /// <summary>
        /// 所有活动(Key:活动ID,Value: BoxBusiness)
        /// </summary>
        static readonly ConcurrentDictionary<string, PartBusiness> m_parts = new ConcurrentDictionary<string, PartBusiness>();

        PartProxy()
            : base((120 - DateTime.UtcNow.Second) * 1000, 60 * 1000)
        {
        }
        protected override void Exec()
        {
            //检查是否有活动需要开始.
            foreach (Part v in PartManager.Instance.Values)
            {
                if (v.ID == Part.Pro || v.ID == Part.Rob || v.SubType == Part.Hunt || v.SubType == Part.DoubleExp)
                {
                    CheckPartStart(v);
                }
            }
            CheckPartEnd();
        }

        /// <summary>
        /// 检查活动开始.
        /// </summary>
        /// <param name="part"></param>
        private static void CheckPartStart(Part part)
        {
            PartBusiness pb;
            if (m_parts.TryGetValue(part.ID, out pb))
            {
                return;
            }
            //必须使用本地时间
            DateTime checkTime = DateTime.Now;
            int early = part.Perpare;
            if (early > 0)
            {
                checkTime = DateTime.Now.AddMinutes(early);
            }
            Period period = part.Contains(checkTime);
            if (period == null)
            {
                return;
            }
            DateTime start = period.GetNearStart(checkTime).ToUniversalTime();
            DateTime end = period.GetNearEnd(checkTime).ToUniversalTime();
            //夺宝奇兵
            if (part.ID == Part.Rob)
            {
                pb = new RobBusiness(part);
            }
            //守护战争
            else if (part.ID == Part.Pro)
            {
                pb = new ProBusiness(part);
            }
            //日常打怪
            else if (part.SubType == Part.Hunt)
            {
                pb = new HuntBusiness(part);
            }
            //双倍经验
            else if (part.SubType == Part.DoubleExp)
            {
                pb = new ManyExpBusiness(part);
            }

            if (pb != null && m_parts.TryAdd(part.ID, pb))
            {
                //活动开始
                try
                {
                    pb.Start(start, end);
                    logger.Info("Part start:" + part.Name);
                }
                catch { }
            }
        }


        private static void CheckPartEnd()
        {
            foreach (var item in m_parts)
            {
                try
                {
                    PartBusiness pb = item.Value;
                    DateTime now = pb.EndTime.Kind == DateTimeKind.Utc ? DateTime.UtcNow : DateTime.Now;
                    if (pb.EndTime < now)  //活动结束
                    {
                        pb.End();
                        if (pb.Over)
                        {
                            m_parts.TryRemove(item.Key, out pb);
                            logger.Info("Part end:" + item.Value.Part.Name);
                        }
                    }
                }
                catch (Exception err)
                {
                    LogWrapper.Error("Part:" + err);
                }
            }
        }

        public static PartBusiness TryGetPart(string id)
        {
            PartBusiness part;
            m_parts.TryGetValue(id, out part);
            return part;
        }

        public static List<PartBusiness> GetActiveParts()
        {
            List<PartBusiness> parts = new List<PartBusiness>(m_parts.Count);
            foreach (var v in m_parts.Values)
            {
                if (!v.Over)
                {
                    parts.Add(v);
                }
            }
            return parts;
        }
    }
}
