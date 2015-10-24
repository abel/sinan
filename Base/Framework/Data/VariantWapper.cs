using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.FastJson;
using Sinan.Util;

namespace Sinan.Data
{
    /// <summary>
    /// 包装动态对象.可继承的
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class VariantWapper
    {
        protected Variant m_value;

        [BsonElement("Value")]
        public Variant Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        /// <summary>
        /// 从文件加载
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Variant LoadVariant(string file)
        {
            //Console.WriteLine("加载文件:" + file);
            using (FileStream fs = File.OpenRead(file))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            {
                StringBuilder sb = new StringBuilder(2048);
                string item = sr.ReadLine();
                while (item != null)
                {
                    item = item.Trim();
                    if (item.Length > 0)
                    {
                        if (item[0] != '/' && item[0] != '*')
                        {
                            sb.Append(item);
                        }
                    }
                    item = sr.ReadLine();
                }
                item = sb.ToString();
                return JsonConvert.DeserializeObject<Variant>(item);
            }
        }
    }
}
