using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Schedule;
using Sinan.FrontServer;

namespace Sinan.HomeModule.Business
{
    public class HomeTimer: SchedulerBase
    {        
        HomeTimer()
            : base(60 * 1000, 60 * 1000)
        {
           
        }

        protected override void Exec()
        {
            PlayerBusiness[] pbs = PlayersProxy.Players;
            if (pbs.Length > 0) 
            {
                foreach (PlayerBusiness pb in pbs)
                {
                    HomeBusiness.HomeInfoCall(pb);
                }
            }
        }
    }
}
