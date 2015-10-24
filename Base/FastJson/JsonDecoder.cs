using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Sinan.FastJson
{
    public class JsonDecoder<T> where T : class,IDictionary<string, object>, new()
    {
        int m_offset;
        string m_jsonString;
        StringBuilder m_builder;

        private JsonDecoder(string jsonString)
        {
            m_jsonString = jsonString;
            m_builder = new StringBuilder(64);
        }

        private string GetString(int start, int end)
        {
            return m_jsonString.Substring(start, end - start);
        }

        public static object DeserializeObject(string jsonString)
        {
            try
            {
                JsonDecoder<T> decoder = new JsonDecoder<T>(jsonString);
                return decoder.ReadObject();
            }
            catch
            {
                return jsonString;
            }
        }

        public static T DeserializeDictionary(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString) || jsonString.Length < 2)
            {
                return default(T);
            }
            JsonDecoder<T> decoder = new JsonDecoder<T>(jsonString);
            T t = decoder.ReadDictionary();
            return t;
        }

        /// <summary>
        /// 读取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T ReadDictionary()
        {
            char c = readToken();
            if (c != JsonToken.ObjectHead)
            {
                return default(T);
            }
            m_offset++;

            T v = new T();
            while (true)
            {
                c = readToken();
                if (c == JsonToken.ObjectEnd)
                {
                    m_offset++;
                    return v;
                }
                if (c == JsonToken.ValueSplit)
                {
                    m_offset++;
                }
                string key = ReadKey();
                if (key == null)
                {
                    return v;
                }
                c = readToken();
                if (c == JsonToken.KeySplit)
                {
                    m_offset++;
                    v[key] = ReadObject();
                }
                else
                {
                    throw GetJsonException();
                }
            }
        }

        /// <summary>
        /// 读取键
        /// </summary>
        /// <returns></returns>
        private string ReadKey()
        {
            char c = readToken();
            if (c == JsonToken.Quote)
            {
                string key = ReadString();
                return key;
            }
            else
            {
                for (int end = m_offset; end < m_jsonString.Length; end++)
                {
                    c = m_jsonString[end];
                    if (c == JsonToken.KeySplit || char.IsWhiteSpace(c))
                    {
                        if (m_offset == end)
                        {
                            throw GetJsonException();
                        }
                        string key = GetString(m_offset, end);
                        m_offset = end;
                        return key;
                    }
                }
            }
            return null;
        }

        private JsonException GetJsonException()
        {
            int start = Math.Max(0, m_offset - 16);
            int length = Math.Min(32, m_jsonString.Length - start);
            JsonException ex = new JsonException("format error:" + m_jsonString.Substring(start, length));
            return ex;
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <returns></returns>
        private string ReadString()
        {
            //char c = readToken();
            //if (c != JsonToken.Quote)
            //{
            //    return null;
            //}

            m_builder.Clear();
            m_offset++;
            bool escape = false;
            for (int index = m_offset; index < m_jsonString.Length; index++)
            {
                char c = m_jsonString[index];
                if (escape)
                {
                    #region //后面的为转义字符.
                    switch (c)
                    {
                        //case '\"':
                        //    m_builder.Append("\\\"");
                        //    break;
                        //case '\\':
                        //    m_builder.Append("\\\\");
                        //    break;
                        //case '/':
                        //    m_builder.Append("\\/");
                        //    break;

                        case 'b':
                            m_builder.Append("\b");
                            break;
                        case 'f':
                            m_builder.Append("\f");
                            break;
                        case 'n':
                            m_builder.Append("\n");
                            break;
                        case 'r':
                            m_builder.Append("\r");
                            break;
                        case 't':
                            m_builder.Append("\t");
                            break;
                        //case '"':
                        //case '\'':
                        //case '/':
                        //    m_builder.Append(c);
                        //    break;
                        //case 'u':
                        //    break;
                        default:
                            m_builder.Append(c);
                            break;
                    }
                    escape = false;
                    #endregion
                }
                else
                {
                    //转义符...
                    if (c == JsonToken.Escape)
                    {
                        escape = true;
                        continue;
                    }
                    //字符结束..
                    if (c == JsonToken.Quote)
                    {
                        m_offset = index + 1;
                        return m_builder.ToString();
                    }
                    m_builder.Append(c);
                }
            }
            return null;
        }

        static readonly char[] DoubleToken = new char[] { '.', 'e', 'E' };
        private object ReadObject()
        {
            char c = readToken();
            if (c == JsonToken.Quote)
            {
                string text = ReadString();
                if (text.Length > 10 && text.StartsWith("/Date(", StringComparison.Ordinal)
                    && text.EndsWith(")/", StringComparison.Ordinal))
                {
                    //读取日期
                    return ParseDateTime(text);
                }
                return text;
            }

            if (c == JsonToken.ArrayHead)
            {
                return ReadArray();
            }

            if (c == JsonToken.ObjectHead)
            {
                return ReadDictionary();
            }

            if (c == 'n') //读null
            {
                if (m_offset + 4 <= m_jsonString.Length)
                {
                    if (m_jsonString[m_offset + 1] == 'u' && m_jsonString[m_offset + 2] == 'l' && m_jsonString[m_offset + 3] == 'l')
                    {
                        m_offset += 4;
                        return null;
                    }
                }
            }
            else if (c == 't')//读true
            {
                if (m_offset + 4 <= m_jsonString.Length)
                {
                    if (m_jsonString[m_offset + 1] == 'r' && m_jsonString[m_offset + 2] == 'u' && m_jsonString[m_offset + 3] == 'e')
                    {
                        m_offset += 4;
                        return true;
                    }
                }
            }
            else if (c == 'f')//读false;
            {
                if (m_offset + 5 <= m_jsonString.Length)
                {
                    if (m_jsonString[m_offset + 1] == 'a' && m_jsonString[m_offset + 2] == 'l'
                        && m_jsonString[m_offset + 3] == 's' && m_jsonString[m_offset + 4] == 'e')
                    {
                        m_offset += 5;
                        return false;
                    }
                }
            }
            else if ((c >= '0' && c <= '9') || c == '-' || c == '+' || c == '.' || c == 'e' || c == 'E')
            {
                //读数字
                for (int index = m_offset; index < m_jsonString.Length; index++)
                {
                    c = m_jsonString[index];
                    if (c == JsonToken.ValueSplit || c == JsonToken.ObjectEnd || c == JsonToken.ArrayEnd)
                    {
                        string number = GetString(m_offset, index);
                        m_offset = index;
                        double d = Convert.ToDouble(number);
                        if (number.IndexOfAny(DoubleToken) == -1)
                        {
                            if (d <= Int32.MaxValue && d >= Int32.MinValue)
                            {
                                return (Int32)d;
                            }
                            if (d <= Int64.MaxValue && d >= Int64.MinValue)
                            {
                                return (Int64)d;
                            }
                        }
                        return d;
                    }
                }
            }

            if (m_offset - 1 < m_jsonString.Length)
            {
                m_offset++;
                return ReadObject();
            }
            return null;
        }

        /// <summary>
        /// 读取标识(可重复读取)
        /// </summary>
        /// <returns></returns>
        private char readToken()
        {
            char c = m_jsonString[m_offset];
            while (Char.IsWhiteSpace(c))
            {
                c = m_jsonString[++m_offset];
            }
            return c;
        }

        /// <summary>
        /// 读取列表
        /// </summary>
        /// <returns></returns>
        private ArrayList ReadArray()
        {
            if (m_jsonString[m_offset] == JsonToken.ArrayHead)
            {
                m_offset++;
                ArrayList list = new ArrayList();
                while (true)
                {
                    char c = readToken();
                    if (c == JsonToken.ArrayEnd)
                    {
                        m_offset++;
                        return list;
                    }
                    if (c == JsonToken.ValueSplit)
                    {
                        m_offset++;
                    }
                    list.Add(ReadObject());
                }
            }
            throw GetJsonException();
        }

        /// <summary>
        /// 将字符串转化为时间
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        static object ParseDateTime(string text)
        {
            string value = text.Substring(6, text.Length - 8);

            DateTime time;
            if (DateTime.TryParse(value, out time))
            {
                return time;
            }

            DateTimeKind kind = DateTimeKind.Utc;
            int index = value.IndexOf('+', 1);
            if (index == -1)
                index = value.IndexOf('-', 1);

            if (index != -1)
            {
                kind = DateTimeKind.Local;
                value = value.Substring(0, index);
            }

            long javaScriptTicks;
            if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out javaScriptTicks))
            {
                return text;
            }

            const long InitialJavaScriptDateTicks = 621355968000000000;
            time = new DateTime((javaScriptTicks * 10000) + InitialJavaScriptDateTicks, DateTimeKind.Utc);
            switch (kind)
            {
                case DateTimeKind.Utc:
                    return time;
                case DateTimeKind.Local:
                    return time.ToLocalTime();
                default:
                    return DateTime.SpecifyKind(time.ToLocalTime(), DateTimeKind.Unspecified);
            }
        }
    }
}
