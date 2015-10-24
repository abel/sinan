using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Sinan.Extensions
{
    public static partial class StringFormat
    {
        public static string Format(this string format, IDictionary<string, object> args)
        {
            if (format == null || args == null)
            {
                return format;
            }
            StringBuilder result = new StringBuilder(format.Length << 1);
            int start = 0;
            while (start < format.Length)
            {
                char c = format[start++];
                if (c == '{')
                {
                    int end = start;
                    while (end < format.Length)
                    {
                        c = format[end++];
                        if (c == '}')
                        {
                            string key = format.Substring(start, end - start - 1);
                            start = end;
                            object value;
                            if (args.TryGetValue(key, out value))
                            {
                                if (value != null)
                                {
                                    result.Append(value.ToString());
                                }
                            }
                            break;
                        }
                    }
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        public static string Format(this string format, IDictionary<string, string> args)
        {
            if (format == null || args == null)
            {
                return format;
            }
            StringBuilder result = new StringBuilder(format.Length << 1);
            int start = 0;
            while (start < format.Length)
            {
                char c = format[start++];
                if (c == '{')
                {
                    int end = start;
                    while (end < format.Length)
                    {
                        c = format[end++];
                        if (c == '}')
                        {
                            string key = format.Substring(start, end - start - 1);
                            start = end;
                            string value;
                            if (args.TryGetValue(key, out value))
                            {
                                result.Append(value);
                            }
                            break;
                        }
                    }
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        public static string Format(this string format, IDictionary<string, int> args)
        {
            if (format == null || args == null)
            {
                return format;
            }
            StringBuilder result = new StringBuilder(format.Length << 1);
            int start = 0;
            while (start < format.Length)
            {
                char c = format[start++];
                if (c == '{')
                {
                    int end = start;
                    while (end < format.Length)
                    {
                        c = format[end++];
                        if (c == '}')
                        {
                            string key = format.Substring(start, end - start - 1);
                            start = end;
                            int value;
                            if (args.TryGetValue(key, out value))
                            {
                                result.Append(value.ToString());
                            }
                            break;
                        }
                    }
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }


        public static string ToHexString(this int pid)
        {
            return pid.ToString("X");
        }

        public static string ToHexString(this Int64 pid)
        {
            return pid.ToString("X");
        }

        public static int ToHexNumber(this string number)
        {
            int i = 0;
            int.TryParse(number, NumberStyles.HexNumber, null, out i);
            return i;
        }

        public static bool TryHexNumber(this string number, out int i)
        {
            return int.TryParse(number, NumberStyles.HexNumber, null, out i);
        }
    }
}
