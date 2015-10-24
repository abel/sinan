using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Extensions.FluentDate;
using Sinan.Util;
using Sinan.GameModule;
using Sinan.Command;

namespace Sinan.Entity
{
    /// <summary>
    /// 游戏活动
    /// </summary>
    public class Part
    {
        /// <summary>
        /// 夺宝奇兵
        /// </summary>
        public const string Rob = "PA_Rob";

        /// <summary>
        /// 守护战争
        /// </summary>
        public const string Pro = "PA_Pro";

        /// <summary>
        /// 打怪类活动(怪物来袭/兽王降临)
        /// </summary>
        public const string Hunt = "Hunt";

        /// <summary>
        /// 双倍经验活动
        /// </summary>
        public const string DoubleExp = "DoubleExp";

        /// <summary>
        /// 最早通知
        /// </summary>
        private int m_perpare;

        /// <summary>
        /// 每次活动玩家最多进入次数
        /// </summary>
        private int m_maxInto;

        private Variant m_value;
        private List<string> m_maps;

        /// <summary>
        /// 所需晶币
        /// </summary>
        protected int m_coin;

        /// <summary>
        /// 所需石币
        /// </summary>
        protected int m_score;

        protected string m_coinMsg;

        /// <summary>
        /// 编号
        /// </summary>
        public string ID
        {
            get;
            private set;
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        public string SubType
        {
            get;
            private set;
        }

        public int Coin
        {
            get { return m_coin; }
        }

        public int Score
        {
            get { return m_score; }
        }

        public string CoinMsg
        {
            get { return m_coinMsg; }
        }

        /// <summary>
        /// 活动总经验
        /// </summary>
        Int64 m_totalExp;

        /// <summary>
        /// 活动周期
        /// </summary>
        List<Period> m_periods;

        public List<Period> Periods
        {
            get { return m_periods; }
        }

        public Variant Value
        {
            get { return m_value; }
        }

        /// <summary>
        /// 最早的通知(活动开始前x分钟)
        /// </summary>
        public int Perpare
        {
            get { return m_perpare; }
        }

        /// <summary>
        /// 活动场景ID
        /// </summary>
        public List<string> Maps
        {
            get { return m_maps; }
        }

        /// <summary>
        /// 最大进入次数
        /// </summary>
        public int MaxInto
        {
            get { return m_maxInto; }
        }

        public Part(Variant config)
        {
            m_maps = new List<string>();
            ID = config.GetStringOrDefault("_id");
            SubType = config.GetStringOrDefault("SubType");
            Name = config.GetStringOrDefault("Name");
            m_value = config.GetVariantOrDefault("Value");
            if (m_value != null)
            {
                m_totalExp = m_value.GetInt64OrDefault("TotalExp");
                m_perpare = m_value.GetIntOrDefault("Perpare", 5);
                m_maxInto = m_value.GetIntOrDefault("MaxInto");

                m_coin = m_value.GetIntOrDefault("Coin");
                m_score = m_value.GetIntOrDefault("Score");

                if (m_coin > 0 && m_score > 0)
                {
                    m_coinMsg = string.Format(TipManager.GetMessage(ClientReturn.SceneEctype1), this.Name, m_score, m_coin);
                }
                else
                {
                    if (m_coin > 0)
                    {
                        m_coinMsg = string.Format(TipManager.GetMessage(ClientReturn.SceneEctype2), this.Name, m_coin);
                    }
                    if (m_score > 0)
                    {
                        m_coinMsg = string.Format(TipManager.GetMessage(ClientReturn.SceneEctype3), this.Name, m_score);
                    }
                }


                Variant openTime = m_value.GetVariantOrDefault("OpenTime");
                if (openTime != null)
                {
                    InitPeriods(openTime);
                    InitMap(m_value.GetValueOrDefault<IList>("Map"));
                }
            }
        }

        void InitMap(IList map)
        {
            if (map != null)
            {
                m_maps.Clear();
                foreach (var mapid in map)
                {
                    m_maps.Add(mapid.ToString());
                }
            }
        }

        /// <summary>
        /// 初始化开放时间
        /// </summary>
        /// <param name="openTime"></param>
        void InitPeriods(Variant openTime)
        {
            List<Period> periods = new List<Period>();
            ////TODO: 临时修改添加活动时间.需删除
            //#region
            //DateTime test = DateTime.Now;
            //int se = test.Second + test.Minute * 60 + test.Hour * 60 * 60;
            //periods.Add(new Period(se, se + 60 * 6, 0));
            //#endregion

            for (byte week = 0; week <= 7; week++)
            {
                string time = openTime.GetStringOrDefault(week.ToString());
                if (!string.IsNullOrEmpty(time))
                {
                    // 设置开放时间,格式"12:00-14:00,18:00-20:00"
                    foreach (var s in time.Split(new char[] { ',', ';', '，', '；' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] t = s.Split(new char[] { '-', '~' }, StringSplitOptions.RemoveEmptyEntries);
                        if (t.Length == 2)
                        {
                            int t0 = t[0].ParseSeconds();
                            int t1 = t[1].ParseSeconds();
                            periods.Add(new Period(t0, t1, week));
                        }
                    }
                }
            }
            m_periods = periods;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time">需为本地时间</param>
        /// <returns></returns>
        public Period Contains(DateTime time)
        {
            int index = m_periods.FindIndex(x => x.Contains(time));
            if (index == -1)
            {
                return null;
            }
            Period p = m_periods[index];
            if (p.Once)
            {
                m_periods.RemoveAt(index);
            }
            return p;
        }
    }


    /// <summary>
    /// 周期()
    /// </summary>
    public class Period
    {
        int m_start;
        int m_end;

        /// <summary>
        /// (0,表示每天,1-7 表示星期1到星期天)
        /// (大于100,表示一次性,m_week为2000年到现在的天数)
        /// </summary>
        int m_week;

        DateTime m_startTime;
        DateTime m_endTime;

        public int Start
        {
            get { return m_start; }
        }

        public int End
        {
            get { return m_end; }
        }

        public Period(int start, int end, byte week = 0)
        {
            this.m_start = start;
            this.m_end = end;
            this.m_week = week;
        }

        public Period(DateTime start, DateTime end)
        {
            this.m_week = -1;
            this.m_startTime = start;
            this.m_endTime = end;
        }

        /// <summary>
        /// 表示只执行1次.
        /// </summary>
        public bool Once
        {
            get { return m_week == -1; }
        }

        /// <summary>
        /// 检查时间是否在周期内
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool Contains(DateTime time)
        {
            if (Once)
            {
                return m_startTime <= time && m_endTime > time;
            }
            //先检查星期是否匹配
            if (m_week != 0)
            {
                if ((int)time.DayOfWeek != (m_week % 7)) //星期天
                {
                    return false;
                }
            }
            int se = time.Second + time.Minute * 60 + time.Hour * 60 * 60;
            return m_start <= se && m_end > se;
        }

        /// <summary>
        /// 得到开始时间
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public DateTime GetNearStart(DateTime time)
        {
            if (Once)
            {
                return m_startTime;
            }
            else
            {
                return time.Date.AddSeconds(m_start);
            }
        }

        /// <summary>
        /// 得到结束时间
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public DateTime GetNearEnd(DateTime time)
        {
            if (Once)
            {
                return m_endTime;
            }
            else
            {
                return time.Date.AddSeconds(m_end);
            }
        }
    }
}
