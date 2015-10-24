using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Extensions
{
    /// <summary>
    /// Local纪元时间
    /// </summary>
    public static class LocalTimeExtention
    {
        const double t_m = 0.0001d;
        const double t_s = 0.0000001d;
        public const long LocalEpochTicks = 621355968000000000;

        /// <summary>
        /// UTC 1970年1月1日(LocalTime起始时间)
        /// </summary>
        public static readonly DateTime LocalEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);


        public static DateTime GetNowDate()
        {
            return DateTime.Now.Date;
        }

        public static string GetNowDay()
        {
            return DateTime.Now.Date.ToString("yyyyMMdd");
        }

        public static int GetNowWeek()
        {
            return (int)(((DateTime.Now.Date - LocalEpoch).TotalDays + 3) / 7);
        }

        public static double LocalTotalSeconds(this DateTime date)
        {
            //return (date - LocalEpoch).TotalSeconds;
            return (date.Ticks - LocalEpochTicks) * t_s;
        }

        public static double LocalTotalMilliseconds(this DateTime date)
        {
            //return (date - LocalEpoch).TotalMilliseconds;
            return (date.Ticks - LocalEpochTicks) * t_m;
        }

        public static DateTime MillisecondToLocal(this double milliseconds)
        {
            return LocalEpoch.AddMilliseconds(milliseconds);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static DateTime SecondToLocal(this double seconds)
        {
            return LocalEpoch.AddSeconds(seconds);
        }

        /// <summary>
        /// 从LocalEpoch时间到当前UTC时间的毫秒数
        /// </summary>
        /// <returns></returns>
        public static double NowTotalMilliseconds()
        {
            //return DateTime.UtcNow.Subtract(LocalEpoch).TotalMilliseconds;
            return (DateTime.UtcNow.Ticks - LocalEpochTicks) * t_m;
        }

        /// <summary>
        /// 从LocalEpoch时间到当前UTC时间的秒数
        /// </summary>
        /// <returns></returns>
        public static double NowTotalSeconds()
        {
            //return DateTime.UtcNow.Subtract(LocalEpoch).TotalSeconds;
            return (DateTime.UtcNow.Ticks - LocalEpochTicks) * t_s;
        }
    }
}
