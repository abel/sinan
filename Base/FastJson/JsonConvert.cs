using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.FastJson
{
    /// <summary>
    /// JSON/动态对象转换类.
    /// </summary>
    public static class JsonConvert
    {
        /// <summary>
        /// 对象序列化为JSON字符串
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string SerializeObject(IDictionary<string, object> v)
        {
            return JsonEncoder.JsonSerialize(v);
        }

        /// <summary>
        ///  对象序列化为JSON字符串
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string SerializeObject(object o)
        {
            return JsonEncoder.JsonSerialize(o);
        }

        /// <summary>
        /// JSON 字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string s) where T : class,IDictionary<string, object>, new()
        {
            return JsonDecoder<T>.DeserializeDictionary(s);
        }

        /// <summary>
        /// JSON 字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <returns></returns>
        public static T DeserializeObjectAndInternKey<T>(string s) where T : class,IDictionary<string, object>, new()
        {
            return JsonDecoder<T>.DeserializeDictionary(s);
        }

        /// <summary>
        /// 将"yyyy-MM-dd HH:mm:ss"格式的字符串转换为时间
        /// eg:"2010-01-03 03:22:32"
        /// </summary>
        /// <param name="v"></param>
        public static void CovertDateTime(IDictionary<string, object> v)
        {
            foreach (var key in v.Keys.ToArray())
            {
                string str = v[key] as String;
                if (str != null)
                {
                    //ToString("yyyy-MM-dd HH:mm:ss")
                    if (str.Length == 19 && str[4] == '-' && str[7] == '-'
                       && str[10] == ' ' && str[13] == ':' && str[16] == ':')
                    {
                        DateTime time;
                        if (DateTime.TryParse(str, out time))
                        {
                            v[key] = time;
                        }
                    }
                }
                else
                {
                    IDictionary<string, object> dic = v[key] as IDictionary<string, object>;
                    if (dic != null)
                    {
                        CovertDateTime(dic);
                    }
                }
            }
        }
    }
}
