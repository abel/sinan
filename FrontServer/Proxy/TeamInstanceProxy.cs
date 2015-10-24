using System;
using Sinan.Schedule;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FrontServer
{
    sealed public class TeamInstanceProxy : SchedulerBase
    {
        /// <summary>
        /// 所有副本(Key:玩家ID,Value: PlayerBusiness) 
        /// </summary>
        static readonly ConcurrentDictionary<long, TeamInstanceBusiness> m_online =
            new ConcurrentDictionary<long, TeamInstanceBusiness>();

        static public bool TryGetTeamInstance(long id, out TeamInstanceBusiness instance)
        {
            return m_online.TryGetValue(id, out instance);
        }

        static public bool TryAddInstance(TeamInstanceBusiness instance)
        {
            return m_online.TryAdd(instance.ID, instance);
        }

        static public bool TryRemove(long id)
        {
            TeamInstanceBusiness instance;
            return m_online.TryRemove(id, out instance);
        }

        TeamInstanceProxy()
            : base(60 * 1000, 10 * 1000)
        {
        }

        protected override void Exec()
        {
            long ticks = DateTime.UtcNow.Ticks;
            foreach (var p in m_online.Values)
            {
                try
                {
                    if (p.OverTime.HasValue && p.OverTime < DateTime.UtcNow)
                    {
                        p.TryOver(false);
                    }
                }
                catch { }
            }
        }
    }
}
