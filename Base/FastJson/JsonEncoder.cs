using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sinan.FastJson
{
    /// <summary>
    /// 字典对象的JSON序列化..
    /// </summary>
    public class JsonEncoder
    {
        StringBuilder m_builder;
        JsonEncoder()
        {
            m_builder = new StringBuilder(1024);
        }

        /// <summary>
        /// 序列化对象...
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string JsonSerialize(object obj)
        {
            if (obj == null)
            {
                return JsonToken.Null;
            }
            JsonEncoder encoder = new JsonEncoder();
            encoder.WriteValue(obj);
            return encoder.m_builder.ToString();
        }

        public static string JsonSerialize(IDictionary<string, object> dic)
        {
            if (dic == null)
            {
                return JsonToken.Null;
            }
            JsonEncoder encoder = new JsonEncoder();
            encoder.WriteMap<object>(dic);
            return encoder.m_builder.ToString();
        }

        private void WriteString(string s)
        {
            m_builder.Append(JsonToken.Quote);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                switch (c)
                {
                    case '\"':
                        m_builder.Append("\\\"");
                        break;
                    case '\\':
                        m_builder.Append("\\\\");
                        break;
                    case '/':
                        m_builder.Append("\\/");
                        break;
                    case '\b':
                        m_builder.Append("\\b");
                        break;
                    case '\f':
                        m_builder.Append("\\f");
                        break;
                    case '\n':
                        m_builder.Append("\\n");
                        break;
                    case '\r':
                        m_builder.Append("\\r");
                        break;
                    case '\t':
                        m_builder.Append("\\t");
                        break;
                    default:
                        m_builder.Append(c);
                        break;
                }
            }
            m_builder.Append(JsonToken.Quote);
        }

        private void WriteMap<T>(IDictionary<string, T> v) //where T : class
        {
            m_builder.Append(JsonToken.ObjectHead);
            bool pendingSeperator = false;
            foreach (var i in v)
            {
                AddSplit(ref pendingSeperator);
                WriteString(i.Key);
                m_builder.Append(JsonToken.KeySplit);
                WriteValue(i.Value);
            }
            m_builder.Append(JsonToken.ObjectEnd);
        }

        private void WriteNumberMap<T>(IDictionary<string, T> v) where T : struct
        {
            m_builder.Append(JsonToken.ObjectHead);
            bool pendingSeperator = false;
            foreach (var i in v)
            {
                AddSplit(ref pendingSeperator);
                WriteString(i.Key);
                m_builder.Append(JsonToken.KeySplit);
                m_builder.Append(Convert.ToString(i.Value, NumberFormatInfo.InvariantInfo));
            }
            m_builder.Append(JsonToken.ObjectEnd);
        }

        private void WriteMap2(IDictionary v)
        {
            m_builder.Append(JsonToken.ObjectHead);
            bool pendingSeperator = false;
            foreach (IDictionaryEnumerator i in v)
            {
                AddSplit(ref pendingSeperator);
                WriteString(i.Key.ToString());
                m_builder.Append(JsonToken.KeySplit);
                WriteValue(i.Value);
            }
            m_builder.Append(JsonToken.ObjectEnd);
        }

        private void AddSplit(ref bool pendingSeperator)
        {
            if (pendingSeperator)
            {
                m_builder.Append(JsonToken.ValueSplit);
            }
            else
            {
                pendingSeperator = true;
            }
        }

        private void WriteStringFast(string key)
        {
            m_builder.Append(JsonToken.Quote);
            m_builder.Append(key);
            m_builder.Append(JsonToken.Quote);
        }

        private void WriteArray(IEnumerable value)
        {
            m_builder.Append(JsonToken.ArrayHead);
            bool pendingSeperator = false;
            foreach (var item in value)
            {
                AddSplit(ref pendingSeperator);
                WriteValue(item);
            }
            m_builder.Append(JsonToken.ArrayEnd);
        }

        private void WriteObject(object obj)
        {
            m_builder.Append(JsonToken.ObjectHead);
            Type t = obj.GetType();
            List<Getters> g = JSON.Instance.GetGetters(t);
            bool pendingSeperator = false;
            foreach (var p in g)
            {
                AddSplit(ref pendingSeperator);
                WriteStringFast(p.Name);
                m_builder.Append(JsonToken.KeySplit);
                WriteValue(p.Getter(obj));
            }
            m_builder.Append(JsonToken.ObjectEnd);
        }

        private void WriteValue(object obj)
        {
            if (obj == null || obj is DBNull)
            {
                m_builder.Append(JsonToken.Null);
                return;
            }
            if (obj is Byte || obj is SByte ||
                obj is Int32 || obj is UInt32 ||
                obj is Int16 || obj is UInt16 ||
                obj is Int64 || obj is UInt64)
            {
                m_builder.Append(Convert.ToString(obj, NumberFormatInfo.InvariantInfo));
                return;
            }

            if (obj is Double || obj is Single || obj is Decimal)
            {
                string number = Convert.ToString(obj, NumberFormatInfo.InvariantInfo);
                m_builder.Append(number);
                if (!number.Contains('.'))
                {
                    m_builder.Append(".0");
                }
                return;
            }

            if (obj is Boolean)
            {
                m_builder.Append((Boolean)obj ? JsonToken.True : JsonToken.False);
                return;
            }

            if (obj is String || obj is Char || obj is Guid)
            {
                WriteString(obj.ToString());
                return;
            }

            if (obj is Enum)
            {
                m_builder.Append(((Enum)obj).ToString("D"));
                return;
            }

            if (obj is DateTime)
            {
                m_builder.Append(@"""/Date(");
                DateTime time = (DateTime)obj;
                if (time.Kind == DateTimeKind.Local)
                {
                    m_builder.Append(time.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    m_builder.Append(((time).ToLocalTime()).ToString("yyyy-MM-dd HH:mm:ss"));
                }
                m_builder.Append(@")/""");
                return;
            }

            if (obj is IDictionary<string, object>)
            {
                WriteMap<object>(obj as IDictionary<string, object>);
                return;
            }

            MethodInfo method = GetWriteMethod(obj.GetType());
            if (method != null)
            {
                method.Invoke(this, new object[] { obj });
                return;
            }

            if (obj is IDictionary)
            {
                WriteMap2(obj as IDictionary);
                return;
            }

            if (obj is IEnumerable)
            {
                WriteArray((IEnumerable)obj);
                return;
            }

            WriteObject(obj);
        }

        static readonly Dictionary<Type, MethodInfo> methods = new Dictionary<Type, MethodInfo>(10);
        public static MethodInfo GetWriteMethod(Type t)
        {
            MethodInfo constructed = null;
            if (methods.TryGetValue(t, out constructed))
            {
                return constructed;
            }
            lock (methods)
            {
                if (methods.TryGetValue(t, out constructed))
                {
                    return constructed;
                }
                Type dic = t.GetInterface("System.Collections.Generic.IDictionary`2");
                if (dic != null)
                {
                    var args = dic.GetGenericArguments();
                    if (args[0] == typeof(string))
                    {
                        //构造泛型方法
                        MethodInfo mdefinition = typeof(JsonEncoder).GetMethod(args[1].IsPrimitive ? "WriteNumberMap" : "WriteMap", BindingFlags.Instance | BindingFlags.NonPublic);
                        constructed = mdefinition.MakeGenericMethod(args[1]);
                    }
                }
                methods.Add(t, constructed);
            }
            return constructed;
        }
    }
}
