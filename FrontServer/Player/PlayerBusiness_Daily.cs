using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sinan.Data;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.FrontServer
{
    partial class PlayerBusiness
    {
        /// <summary>
        /// 捕兽网使用次数
        /// </summary>
        public int UseClapnet
        {
            get;
            set;
        }

        public const string DailyBox = "Box";
        public const string DailyMap = "Map";
        public const string DailyOther = "Other";

        public int ReadDaily(string key, string id)
        {
            Variant log = m_daily.Value.GetVariantOrDefault(key);
            if (log != null)
            {
                //前面的为次数.后10位为日期
                int old = log.GetIntOrDefault(id);
                //同一天的..
                if (DateTime.Now.DayOfYear == (old & 0X1FF)) //365小于1FF.
                {
                    return old >> 10;
                }
            }
            return 0;
        }

        public int WriteDaily(string key, string id)
        {
            int day = DateTime.Now.DayOfYear;
            ClearDaily(day);
            Variant log = m_daily.Value.GetVariantOrDefault(key);
            //同一天的..
            if (log == null)
            {
                log = new Variant();
                m_daily.Value[key] = log;
            }
            else
            {
                //删除过期的.
                ClearDaily(day);
            }
            //前面的为次数.后10位为日期
            int old = log.GetIntOrDefault(id);
            int count = old >> 10;
            if (day == (old & 0X1FF)) //365小于1FF.
            {
                log[id] = (((count + 1) << 10) | day);
            }
            else
            {
                log[id] = (1 << 10) | day;
            }
            m_daily.Save();
            this.Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_daily));
            return count + 1;
        }

        private bool ClearDaily(int day)
        {
            bool change = false;
            foreach (object item in m_daily.Value.Values)
            {
                Variant log = item as Variant;
                if (log != null)
                {
                    foreach (string key in log.Keys.ToArray())
                    {
                        object o;
                        if (log.TryGetValue(key, out o) && o is int)
                        {
                            if (day != (((int)o) & 0X1FF))
                            {
                                change = true;
                                log.Remove(key);
                            }
                        }
                    }
                }
            }
            return change;
        }
    }
}
