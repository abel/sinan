using System;
using System.Threading;
using Sinan.Command;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Schedule;
using Sinan.Util;

namespace Sinan.ArenaModule.Award
{
    public sealed class FightValueTimer : SchedulerBase
    {
        FightValueTimer()
            : base(Timeout.Infinite, 7 * 24 * 3600 * 1000)
        {
            DateTime time = NextMonday();
            m_dueTime = (int)(time - DateTime.Now).TotalMilliseconds;
        }

        protected override void Exec()
        {
            if (PlayerAccess.Instance.FightValueClear())
            {
                //在线用户列表
                foreach (PlayerBusiness user in PlayersProxy.Players)
                {
                    user.FightValue = 0;

                    Variant v = new Variant();
                    v.Add("ID", user.ID);
                    v.Add("FightValue", user.FightValue);
                    user.Call(ClientCommand.UpdateActorR, v);
                }
            }
        }

        /// <summary>
        /// 得到下一个星期一
        /// </summary>
        public DateTime NextMonday()
        {
            int addDay = 0;
            DateTime dt = DateTime.Now;
            switch ((int)dt.DayOfWeek)
            {
                case 0:
                    addDay = 1;
                    break;
                case 1:
                    addDay = 7;
                    break;
                case 2:
                    addDay = 6;
                    break;
                case 3:
                    addDay = 5;
                    break;
                case 4:
                    addDay = 4;
                    break;
                case 5:
                    addDay = 3;
                    break;
                case 6:
                    addDay = 2;
                    break;
            }
            return dt.Date.AddDays(addDay).AddSeconds(2);
        }
    }
}
