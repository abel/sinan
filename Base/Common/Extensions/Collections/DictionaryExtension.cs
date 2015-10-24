using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Extensions
{
    public static class DictionaryExtension
    {
        public static TDictionary CopyFrom<TDictionary, TKey, TValue>(
            this TDictionary source,
            IDictionary<TKey, TValue> copy)
            where TDictionary : IDictionary<TKey, TValue>
        {
            foreach (var pair in copy)
            {
                source.Add(pair.Key, pair.Value);
            }

            return source;
        }

        public static TDictionary CopyFrom<TDictionary, TKey, TValue>(
            this TDictionary source,
            IDictionary<TKey, TValue> copy,
            IEnumerable<TKey> keys)
            where TDictionary : IDictionary<TKey, TValue>
        {
            foreach (var key in keys)
            {
                source.Add(key, copy[key]);
            }
            return source;
        }

        public static TDictionary RemoveKeys<TDictionary, TKey, TValue>(
            this TDictionary source,
            IEnumerable<TKey> keys)
            where TDictionary : IDictionary<TKey, TValue>
        {
            foreach (var key in keys)
            {
                source.Remove(key);
            }
            return source;
        }

        public static IDictionary<TKey, TValue> RemoveKeys<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            IEnumerable<TKey> keys)
        {
            foreach (var key in keys)
            {
                source.Remove(key);
            }
            return source;
        }

        #region GetXXOrDefault
        /// <summary>
        /// 获取值或默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T? GetNullableValue<T>(this IDictionary<string, object> map, string name) where T : struct
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                return (T)value;
            }
            return null;
        }

        /// <summary>
        /// 获取值或默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetValueOrNull<T>(this IDictionary<string, object> map, string name) where T : class
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                return value as T;
            }
            return null;
        }

        /// <summary>
        /// 获取值或默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="func">转换函数</param>
        /// <returns></returns>
        public static T GetValueOrDefault<T>(this IDictionary<string, object> map, string name, Func<object, T> func)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                return func(value);
            }
            return default(T);
        }

        /// <summary>
        /// 获取整数,不存在返回null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int? GetIntOrNull(this IDictionary<string, object> map, string name)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                if (value is Int32)
                {
                    return (Int32)value;
                }
                return Convert.ToInt32(value);
            }
            return null;
        }

        /// <summary>
        /// 获取双精数,不存在返回null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Double? GetDoubleOrNull(this IDictionary<string, object> map, string name)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                if (value is Double)
                {
                    return (Double)value;
                }
                return Convert.ToDouble(value);
            }
            return null;
        }

        /// <summary>
        /// 获取布尔,不存在返回null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Boolean? GetBoolOrNull(this IDictionary<string, object> map, string name)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                if (value is Boolean)
                {
                    return (Boolean)value;
                }
                return Convert.ToBoolean(value);
            }
            return null;
        }

        /// <summary>
        /// 获取Int64,不存在返回null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Int64? GetInt64OrNull(this IDictionary<string, object> map, string name)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                if (value is Int64)
                {
                    return (Int64)value;
                }
                return Convert.ToInt64(value);
            }
            return null;
        }

        /// <summary>
        /// 获取字符串,不存在返回null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static String GetStringOrDefault(this IDictionary<string, string> map, string name)
        {
            string value;
            map.TryGetValue(name, out value);
            return value;
        }

        /// <summary>
        /// 获取字符串,不存在返回null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static String GetStringOrDefault(this IDictionary<string, object> map, string name)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                return value == null ? string.Empty : value.ToString();
            }
            return null;
        }

        /// <summary>
        /// 获取字符串,不存在或者为NULL,
        /// 则返回string.Empty
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static String GetStringOrEmpty(this IDictionary<string, object> map, string name)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                return value == null ? string.Empty : value.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取值或默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DateTime? GetDateTimeOrNull(this IDictionary<string, object> map, string name)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                if (value is DateTime)
                {
                    return (DateTime)value;
                }
                return Convert.ToDateTime(value);
            }
            return null;
        }

        #region 得到本地时间或UTC时间
        public static DateTime? GetLocalTimeOrNull(this IDictionary<string, object> map, string name)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                if (value is DateTime)
                {
                    return ((DateTime)value).ToLocalTime();
                }
                return Convert.ToDateTime(value).ToLocalTime();
            }
            return null;
        }

        public static DateTime? GetUtcTimeOrNull(this IDictionary<string, object> map, string name)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                if (value is DateTime)
                {
                    return ((DateTime)value).ToUniversalTime();
                }
                return Convert.ToDateTime(value).ToUniversalTime();
            }
            return null;
        }
        #endregion

        /// <summary>
        /// 获取值或默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool TryGetValue<T>(this IDictionary<string, object> map, string name, out T t)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                t = (T)value;
                return true;
            }
            t = default(T);
            return false;
        }
        #endregion

        #region GetXXOrDefault(自行设置默认值)
        /// <summary>
        /// 获取值或默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetStructOrDefault<T>(this IDictionary<string, object> map, string name, T def) where T : struct
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                return (T)value;
            }
            return def;
        }


        /// <summary>
        /// 获取值或默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetValueOrDefault<T>(this IDictionary<string, object> map, string name, T def) where T : class
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                return value as T;
            }
            return def;
        }

        /// <summary>
        /// 获取值或默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DateTime GetDateTimeOrDefault(this IDictionary<string, object> map, string name, DateTime def)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                if (value is DateTime)
                {
                    return (DateTime)value;
                }
                return Convert.ToDateTime(value);
            }
            return def;
        }

        /// <summary>
        /// 获取值或默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="func">转换函数</param>
        /// <returns></returns>
        public static T GetValueOrDefault<T>(this IDictionary<string, object> map, string name, Func<object, T> func, T t)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                return func(value);
            }
            return t;
        }

        /// <summary>
        /// 获取整数,不存在返回0
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetIntOrDefault(this IDictionary<string, object> map, string name, int def)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                if (value is Int32)
                {
                    return (Int32)value;
                }
                return Convert.ToInt32(value);
            }
            return def;
        }

        /// <summary>
        /// 获取双精数,不存在返回0
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Double GetDoubleOrDefault(this IDictionary<string, object> map, string name, double def)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                if (value is Double)
                {
                    return (Double)value;
                }
                return Convert.ToDouble(value);
            }
            return def;
        }

        /// <summary>
        /// 获取布尔,不存在返回false
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Boolean GetBooleanOrDefault(this IDictionary<string, object> map, string name, Boolean def)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                if (value is Boolean)
                {
                    return (Boolean)value;
                }
                return Convert.ToBoolean(value);
            }
            return def;
        }

        /// <summary>
        /// 获取Int64,不存在返回0
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Int64 GetInt64OrDefault(this IDictionary<string, object> map, string name, Int64 def)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                if (value is Int64)
                {
                    return (Int64)value;
                }
                return Convert.ToInt64(value);
            }
            return def;
        }

        /// <summary>
        /// 获取字符串,不存在返回null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static String GetStringOrDefault(this IDictionary<string, object> map, string name, String def)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                return value == null ? string.Empty : value.ToString();
            }
            return def;
        }
        #endregion

        #region SetOrInc
        /// <summary>
        /// 设置或增加
        /// </summary>
        /// <param name="map"></param>
        /// <param name="name"></param>
        /// <param name="t"></param>
        /// <returns></returns> 
        public static int SetOrInc(this IDictionary<string, object> map, string name, int t)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                if (value is Int32)
                {
                    t += (Int32)value;
                }
                else
                {
                    t += Convert.ToInt32(value);
                }
            }
            map[name] = t;
            return t;
        }

        public static Int64 SetOrInc(this IDictionary<string, object> map, string name, Int64 t)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                if (value is Int64)
                {
                    t += (Int64)value;
                }
                else
                {
                    t += Convert.ToInt64(value);
                }
            }
            map[name] = t;
            return t;
        }

        public static double SetOrInc(this IDictionary<string, object> map, string name, double t)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                if (value is Double)
                {
                    t += (Double)value;
                }
                else
                {
                    t += Convert.ToDouble(value);
                }
            }
            map[name] = t;
            return t;
        }

        public static float SetOrInc(this IDictionary<string, object> map, string name, float t)
        {
            object value;
            if (map.TryGetValue(name, out value))
            {
                if (value is Single)
                {
                    t += (Single)value;
                }
                else
                {
                    t += Convert.ToSingle(value);
                }
            }
            map[name] = t;
            return t;
        }


        /// <summary>
        /// 设置或增加
        /// </summary>
        /// <param name="map"></param>
        /// <param name="name"></param>
        /// <param name="t"></param>
        /// <returns></returns> 
        public static int SetOrInc(this IDictionary<string, int> map, string name, int t)
        {
            int value;
            if (map.TryGetValue(name, out value))
            {
                t += value;
            }
            map[name] = t;
            return t;
        }

        public static Int64 SetOrInc(this IDictionary<string, Int64> map, string name, Int64 t)
        {
            Int64 value;
            if (map.TryGetValue(name, out value))
            {
                t += value;
            }
            map[name] = t;
            return t;
        }

        public static double SetOrInc(this IDictionary<string, double> map, string name, double t)
        {
            double value;
            if (map.TryGetValue(name, out value))
            {
                t += value;
            }
            map[name] = t;
            return t;
        }

        public static float SetOrInc(this IDictionary<string, float> map, string name, float t)
        {
            float value;
            if (map.TryGetValue(name, out value))
            {
                t += value;
            }
            map[name] = t;
            return t;
        }


        /// <summary>
        /// 设置或增加
        /// </summary>
        /// <param name="map"></param>
        /// <param name="name"></param>
        /// <param name="t"></param>
        /// <returns></returns> 
        public static int SetOrInc(this IDictionary<int, int> map, int name, int t)
        {
            int value;
            if (map.TryGetValue(name, out value))
            {
                t += value;
            }
            map[name] = t;
            return t;
        }
        #endregion



        public static bool CheckKey(this IDictionary<string, object> v)
        {
            foreach (var item in v)
            {
                if (!CheckKey(item.Key))
                {
                    return false;
                }
                if (item.Value is IDictionary<string, object>)
                {
                    if (!CheckKey(item.Value as IDictionary<string, object>))
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        public static bool CheckKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            foreach (var c in key)
            {
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') ||
                    c == '_' || c == '-' || c == '$')
                {
                    continue;
                }
                Console.WriteLine(key);
                return false;
            }
            return true;
        }
    }
}
