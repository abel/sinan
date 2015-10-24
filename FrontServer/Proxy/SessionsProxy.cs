using System;
using Sinan.Schedule;
using Sinan.Log;
using System.Diagnostics;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FrontServer
{
    sealed public class SessionsProxy : SchedulerBase
    {
        internal static readonly System.Collections.Concurrent.ConcurrentQueue<UserSession> Sessions
            = new System.Collections.Concurrent.ConcurrentQueue<UserSession>();

        #region ISchedule
        SessionsProxy()
            : base(10 * 1000, 5 * 1000)
        {
        }

        protected override void Exec()
        {
            //int work, comp;
            //System.Threading.ThreadPool.GetAvailableThreads(out work, out comp);
            //LogWrapper.Debug(string.Format("可用线程{0},IO:{1}", work, comp));

            UserSession user;
            while (Sessions.TryPeek(out user))
            {
                if (user.UserID != null) //已登录
                {
                    Sessions.TryDequeue(out user);
                }
                else
                {
                    //未登录,并且创建时间大于16秒
                    if ((DateTime.UtcNow - user.Created).TotalSeconds > 16)
                    {
                        user.Close();
                        Sessions.TryDequeue(out user);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
        #endregion
    }
}
