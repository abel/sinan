using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Extensions
{
    public static partial class TimeSpanExtention
    {
        /// <summary>
        /// Represents TimeSpan in words
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <returns>String representation of the timespan</returns>
        public static string ToWords(this TimeSpan val)
        {
            return TimeSpanArticulator.Articulate(val, TemporalGroupType.day
                | TemporalGroupType.hour
                | TemporalGroupType.minute
                | TemporalGroupType.month
                | TemporalGroupType.second
                | TemporalGroupType.week
                | TemporalGroupType.year);
        }
        /// <summary>
        /// Converts Datetime value at midnight
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <returns>DateTime value with time set to Midnight</returns>
        public static DateTime MidNight(this DateTime val)
        {
            return new DateTime(val.Year, val.Month, val.Day, 0, 0, 0, DateTimeKind.Utc);
        }
        /// <summary>
        /// Converts Datetime value at noon
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <returns>DateTime value with time set to Noon</returns>
        public static DateTime Noon(this DateTime val)
        {
            return new DateTime(val.Year, val.Month, val.Day, 12, 0, 0, DateTimeKind.Utc);
        }
        /// <summary>
        /// Checks if the Datetime lies within a given range
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="floor">The floor value of the range</param>
        /// <param name="ceiling">The ceiling value of the range</param>
        /// <param name="includeBase">True to include the floor and ceiling values for comparison</param>
        /// <returns>Returns true if the value lies within the range</returns>
        public static bool IsWithinRange(this DateTime val, DateTime floor, DateTime ceiling, bool includeBase)
        {
            if (floor > ceiling)
                throw new InvalidOperationException("floor value cannot be greater than ceiling value");
            if (floor == ceiling)
                throw new InvalidOperationException("floor value cannot be equal to ceiling value");

            if (includeBase)
                return (val >= floor && val <= ceiling);
            else
                return (val > floor && val < ceiling);
        }
        /// <summary>
        /// Calculates the TimeSpan between the current Datetime and the provided Datetime
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <returns>TimeSpan between the current DateTime & the provided DateTime</returns>
        public static TimeSpan GetTimeSpan(this DateTime val)
        {
            TimeSpan dateDiff;
            if (val < DateTime.UtcNow)
                dateDiff = DateTime.UtcNow.Subtract(val);
            else if (val > DateTime.UtcNow)
                dateDiff = val.Subtract(DateTime.UtcNow);
            else
                throw new InvalidOperationException("value cannot be equal to DateTime.UtcNow");
            return dateDiff;
        }
    }
}
