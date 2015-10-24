using System;
using System.Collections.Generic;
using System.Threading;
using Sinan.Data;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Schedule;

namespace Sinan.TaskModule.Business
{
    public sealed class TaskTimer : SchedulerBase
    {
        //每天0点执行
        TaskTimer()
            : base(Timeout.Infinite, 24 * 3600 * 1000)
        {
            DateTime now = DateTime.Now;
            m_dueTime = (24 * 3600 * 1000) - (int)((now - now.Date).TotalMilliseconds);
        }

        protected override void Exec()
        {
            PlayerBusiness[] pbs = PlayersProxy.Players;//当前在线的角色
            if (pbs.Length > 0)
            {
                DateTime dt = DateTime.Now;
                foreach (PlayerBusiness pb in pbs)
                {
                    TaskBusiness.LoopTask(pb, true, new List<int>() { 4, 6 });
                    TaskBusiness.DayTask(pb, true);
                    TaskBusiness.FamilyTask(pb, true);

                    TaskBusiness.WeekTask(pb, "", true);
                }
            }
        }
    }
}
