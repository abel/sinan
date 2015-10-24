using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Extensions.FluentDate
{
    public static class NumericDateTime
    {
        /// <summary>
        /// Associate Week with the integer
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static TimeSpanSelector Weeks(this int i)
        {
            return new WeekSelector { ReferenceValue = i };
        }
        /// <summary>
        /// Associate Days with the integer
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static TimeSpanSelector Days(this int i)
        {
            return new DaysSelector { ReferenceValue = i };
        }
        /// <summary>
        /// Associate Years with the integer
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static TimeSpanSelector Years(this int i)
        {
            return new YearsSelector { ReferenceValue = i };
        }
        /// <summary>
        /// Associate Hours with the integer
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static TimeSpanSelector Hours(this int i)
        {
            return new HourSelector { ReferenceValue = i };
        }
        /// <summary>
        /// Associate Minutes with the integer
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static TimeSpanSelector Minutes(this int i)
        {
            return new MinuteSelector { ReferenceValue = i };
        }
        /// <summary>
        /// Associate Seconds with the integer
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static TimeSpanSelector Seconds(this int i)
        {
            return new SecondSelector { ReferenceValue = i };
        }


        /// <summary>
        /// 时:分[:秒] 格式的字符串转化为秒.
        /// 12:58:00 或 12:58()
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static int ParseSeconds(this string time)
        {
            string[] t = time.Split(new char[] { ':','：' }, StringSplitOptions.RemoveEmptyEntries);
            int h = 0; //时
            int m = 0; //分
            int s = 0; //秒
            if (t.Length > 0)
            {
                int.TryParse(t[0], out h);
                if (t.Length > 1)
                {
                    int.TryParse(t[1], out m);
                    if (t.Length > 2)
                    {
                        int.TryParse(t[2], out s);
                    }
                }
            }
            return h * 60 * 60 + m * 60 + s;
        }
    }    

}
