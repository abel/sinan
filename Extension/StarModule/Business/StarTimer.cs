using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Schedule;
using Sinan.FrontServer;

namespace Sinan.StarModule.Business
{
    public sealed class StarTimer : SchedulerBase
    {
         //每天0点执行
        StarTimer()
            : base(300 * 1000, 300 * 1000)
        {
        }

        protected override void Exec()
        {
            //当前在线的角色
            PlayerBusiness[] pbs = PlayersProxy.Players;
            if (pbs.Length > 0)
            {                
                foreach (PlayerBusiness pb in pbs)
                {
                    StarBusiness.OnlineTroops(pb);
                }
            }
        }
    }
}
