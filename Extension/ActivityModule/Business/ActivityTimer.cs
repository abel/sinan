using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Schedule;
using Sinan.FrontServer;
using System.Threading;

namespace Sinan.ActivityModule.Business
{
    public sealed class ActivityTimer : SchedulerBase
    {
        //每天0点执行
        ActivityTimer()
            : base(Timeout.Infinite, 24 * 3600 * 1000)
        {
            DateTime now = DateTime.Now;
            m_dueTime = (24 * 3600 * 1000) - (int)((now - now.Date).TotalMilliseconds);
        }

        protected override void Exec()
        {
            //当前在线的角色
            PlayerBusiness[] pbs = PlayersProxy.Players;
            if (pbs.Length > 0)
            {
                foreach (PlayerBusiness pb in pbs)
                {
                    ActivityBusiness.Activity(pb);
                }
            }
        }
    }
}
