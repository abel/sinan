using System;
using System.Threading;
using Sinan.FrontServer;
using Sinan.Schedule;

namespace Sinan.GoodsModule.Business
{
    public sealed class AwardTimer : SchedulerBase
    {
        AwardTimer()
            : base(Timeout.Infinite, 24 * 3600 * 1000)
        {
            DateTime now = DateTime.Now;
            m_dueTime = (24 * 3600 * 1000) - (int)((now - now.Date).TotalMilliseconds);
        }

        protected override void Exec()
        {
            foreach (PlayerBusiness pb in PlayersProxy.Players)
            {
                GoodsBusiness.ResetAnswer(pb);
            }
        }
    }
}
