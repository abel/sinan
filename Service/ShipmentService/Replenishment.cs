using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Schedule;
using Sinan.GameModule;
using Sinan.Log;
using System.Configuration;

namespace Sinan.ShipmentService
{
    /// <summary>
    /// 重发发货通知
    /// </summary>
    class Replenishment : SchedulerBase
    {
        ShipmentRouter m_router;
        int lostTime;
        public Replenishment(ShipmentRouter router)
            : base(60 * 1000, 60 * 1000)
        {
            m_router = router;
            if (!int.TryParse(ConfigurationManager.AppSettings["LostTime"], out lostTime))
            {
                lostTime = 60;
            }
        }

        protected override void Exec()
        {
            List<Order> orders = OrderAccess.Instance.GetLost(DateTime.UtcNow.AddSeconds(-lostTime), 2);
            if (orders.Count > 0)
            {
                foreach (Order o in orders)
                {
                    LogWrapper.Warn("补货:" + o.token);
                    m_router.Route(o);
                }
            }
        }
    }
}
