using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization;

namespace Sinan.Util
{
    /// <summary>
    /// 实体
    /// </summary>
    [Serializable]
    sealed public class Variant : Dictionary<string, object>
    {
        public new object this[string name]
        {
            get
            {
                object result;
                if (base.TryGetValue(name, out result))
                {
                    return result;
                }
                return result;
            }
            set
            {
                base[name] = value;
            }
        }

        #region 构造函数
        public Variant()
        {
        }

        public Variant(IDictionary<string, object> dictionary)
            : base(dictionary)
        {
        }

        public Variant(int capacity)
            : base(capacity)
        {
        }

        #endregion

        #region GetXXOrDefault
        /// <summary>
        /// 获取值或默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetValueOrDefault<T>(string name) where T : class
        {
            object value;
            if (base.TryGetValue(name, out value))
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
        /// <returns></returns>
        public DateTime GetDateTimeOrDefault(string name)
        {
            object value;
            if (base.TryGetValue(name, out value))
            {
                if (value is DateTime)
                {
                    return (DateTime)value;
                }
                return Convert.ToDateTime(value);
            }
            return default(DateTime);
        }

        static DateTime defaultLocalTime = default(DateTime).ToLocalTime();
        static DateTime defaultUtcTime = default(DateTime).ToUniversalTime();
        public DateTime GetLocalTimeOrDefault(string name)
        {
            object value;
            if (base.TryGetValue(name, out value))
            {
                if (value is DateTime)
                {
                    return ((DateTime)value).ToLocalTime();
                }
                return Convert.ToDateTime(value).ToLocalTime();
            }
            return defaultLocalTime;
        }

        public DateTime GetUtcTimeOrDefault(string name)
        {
            object value;
            if (base.TryGetValue(name, out value))
            {
                if (value is DateTime)
                {
                    return ((DateTime)value).ToUniversalTime();
                }
                return Convert.ToDateTime(value).ToUniversalTime();
            }
            return defaultUtcTime;
        }

        /// <summary>
        /// 获取整数,不存在返回0
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetIntOrDefault(string name)
        {
            object value;
            if (base.TryGetValue(name, out value))
            {
                if (value is Int32)
                {
                    return (Int32)value;
                }
                return Convert.ToInt32(value);
            }
            return 0;
        }

        /// <summary>
        /// 获取整数,不存在返回指定的值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetIntOrDefault(string name, int def)
        {
            object value;
            if (base.TryGetValue(name, out value))
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
        public Double GetDoubleOrDefault(string name)
        {
            object value;
            if (base.TryGetValue(name, out value))
            {
                if (value is Double)
                {
                    return (Double)value;
                }
                return Convert.ToDouble(value);
            }
            return 0;
        }

        /// <summary>
        /// 获取布尔,不存在返回false
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Boolean GetBooleanOrDefault(string name, bool def = false)
        {
            object value;
            if (base.TryGetValue(name, out value))
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
        public Int64 GetInt64OrDefault(string name)
        {
            object value;
            if (base.TryGetValue(name, out value))
            {
                if (value is Int64)
                {
                    return (Int64)value;
                }
                return Convert.ToInt64(value);
            }
            return 0;
        }

        /// <summary>
        /// 获取字符串,不存在返回null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public String GetStringOrDefault(string name)
        {
            object value;
            if (base.TryGetValue(name, out value))
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
        public String GetStringOrEmpty(string name)
        {
            object value;
            if (base.TryGetValue(name, out value))
            {
                return value == null ? string.Empty : value.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取值或默认值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Variant GetVariantOrDefault(string name)
        {
            object value;
            if (base.TryGetValue(name, out value))
            {
                return value as Variant;
            }
            return null;
        }

        /// <summary>
        /// 获取值或默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool TryGetValueT<T>(string name, out T t)
        {
            object value;
            if (base.TryGetValue(name, out value))
            {
                t = (T)value;
                return true;
            }
            t = default(T);
            return false;
        }
        #endregion

        public T GetValue<T>(string name)
        {
            object value;
            if (base.TryGetValue(name, out value))
            {
                return (T)value;
            }
            return default(T);
        }

        public int SetOrInc(string name, int t)
        {
            object value;
            if (base.TryGetValue(name, out value))
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
            base[name] = t;
            return t;
        }

        public double SetOrInc(string name, double t)
        {
            object value;
            if (base.TryGetValue(name, out value))
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
            base[name] = t;
            return t;
        }
    }
}
