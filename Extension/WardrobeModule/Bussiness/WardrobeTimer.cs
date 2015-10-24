using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Schedule;
using Sinan.FrontServer;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Entity;

namespace Sinan.WardrobeModule.Bussiness
{
    public class WardrobeTimer : SchedulerBase
    {
        //1分钟检查一次
        WardrobeTimer()
            : base(60 * 1000, 60 * 1000)
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
                    WardrobeBusiness.DownWardrobe(pb);
                }
            }
        }
    }
}
