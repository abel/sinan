using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Util;
using Sinan.Extensions.FluentDate;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 开放时间限制
    /// </summary>
    public class LimitOpentime : ILimit
    {
        readonly protected string m_openTimeMsg;

        /// <summary>
        /// 开放时间
        /// </summary>
        protected List<Tuple<int, int>> m_openTime;


        public LimitOpentime(List<Tuple<int, int>> openTime, string openTimeMsg)
        {
            m_openTime = openTime;
            m_openTimeMsg = openTimeMsg;
        }

        public bool Check(PlayerBusiness player, out string msg)
        {
            msg = null;
            //默认全天开放
            if (m_openTime != null && m_openTime.Count > 0)
            {
                DateTime now = DateTime.Now; //此处使用本地时间
                int se = now.Second + now.Minute * 60 + now.Hour * 60 * 60;
                foreach (var v in m_openTime)
                {
                    if (v.Item1 <= se && v.Item2 >= se)
                    {
                        return true;
                    }
                }
                msg = m_openTimeMsg;
                return false;
            }
            return true;
        }

        public bool Execute(PlayerBusiness player, out string msg)
        {
            msg = null;
            return true;
        }

        public bool Rollback(PlayerBusiness player)
        {
            return true;
        }

        /// <summary>
        /// 初始化开放时间
        /// </summary>
        /// <param name="openTime"></param>
        static List<Tuple<int, int>> InitOpenTime(string name, string openTime)
        {
            if (!string.IsNullOrWhiteSpace(openTime))
            {
                // 设置开放时间,格式"12:00-14:00,18:00-20:00"
                string[] ss = openTime.Split(new char[] { ',', ';', '，', '；' }, StringSplitOptions.RemoveEmptyEntries);
                if (ss.Length > 0)
                {
                    var openList = new List<Tuple<int, int>>();
                    foreach (var s in ss)
                    {
                        string[] t = s.Split(new char[] { '-', '~' }, StringSplitOptions.RemoveEmptyEntries);
                        if (t.Length == 2)
                        {
                            int t0 = t[0].ParseSeconds();
                            int t1 = t[1].ParseSeconds();
                            openList.Add(new Tuple<int, int>(t0, t1));
                        }
                    }
                    return openList;
                }
            }
            return null;
        }

        public static LimitOpentime Create(string name, Variant v)
        {
            string openTime = v.GetStringOrDefault("OpenTime");
            var result = InitOpenTime(name, openTime);
            if (result != null && result.Count > 0)
            {
                string openTimeMsg = string.Format(TipManager.GetMessage(ClientReturn.IntoLimit6), name, openTime);
                return new LimitOpentime(result, openTimeMsg);
            }
            return null;
        }

    }
}
