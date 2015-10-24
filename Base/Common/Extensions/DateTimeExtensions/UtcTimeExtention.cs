using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Extensions
{
    /// <summary>
    /// Unix纪元时间
    /// </summary>
    public static class UtcTimeExtention
    {
        const double t_m = 0.0001d;
        const double t_s = 0.0000001d;
        public const long UnixEpochTicks = 621355968000000000;

        /// <summary>
        /// UTC 1970年1月1日(UnixTime起始时间)
        /// </summary>
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime GetNowDate()
        {
            return DateTime.UtcNow.Date;
        }

        public static string GetNowDay()
        {
            return DateTime.UtcNow.Date.ToString("yyyyMMdd");
        }

        public static int GetNowWeek()
        {
            return (int)(((DateTime.Now.Date - UnixEpoch).TotalDays + 3) / 7);
        }

        public static double UnixTotalSeconds(this DateTime date)
        {
            //return (date - UnixEpoch).TotalSeconds;
            return (date.Ticks - UnixEpochTicks) * t_s;
        }

        public static double UnixTotalMilliseconds(this DateTime date)
        {
            //return (date - UnixEpoch).TotalMilliseconds;
            return (date.Ticks - UnixEpochTicks) * t_m;
        }

        public static DateTime MillisecondToUnix(this double milliseconds)
        {
            return UnixEpoch.AddMilliseconds(milliseconds);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static DateTime SecondToUnix(this double seconds)
        {
            return UnixEpoch.AddSeconds(seconds);
        }

        /// <summary>
        /// 从UnixEpoch时间到当前UTC时间的毫秒数
        /// </summary>
        /// <returns></returns>
        public static double NowTotalMilliseconds()
        {
            //return DateTime.UtcNow.Subtract(UnixEpoch).TotalMilliseconds;
            return (DateTime.UtcNow.Ticks - UnixEpochTicks) * t_m;
        }

        /// <summary>
        /// 从UnixEpoch时间到当前UTC时间的秒数
        /// </summary>
        /// <returns></returns>
        public static double NowTotalSeconds()
        {
            //return DateTime.UtcNow.Subtract(UnixEpoch).TotalSeconds;
            return (DateTime.UtcNow.Ticks - UnixEpochTicks) * t_s;
        }

        public static double SubtractSeconds(this DateTime date, DateTime b)
        {
            return (date.Ticks - b.Ticks) * t_s;
        }

        public static double SubtractMilliseconds(this DateTime date, DateTime b)
        {
            return (date.Ticks - b.Ticks) * t_m;
        }
    }
}
