using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using Sinan.Extensions;
using Sinan.FastJson;
using Sinan.Util;
using System.Windows.Forms;

namespace WindowsConfig
{
    public static class ConfigFile
    {
        static readonly Encoding noBomUTF8 = new System.Text.UTF8Encoding(false);

        /// <summary>
        /// 将数据库配置转换为本地文件
        /// </summary>
        /// <param name="connect">连接字符串</param>
        /// <param name="path"></param>
        public static int ConfigToFile(string connect, string path)
        {
            var m_collection = MongoDatabase.Create(connect).GetCollection("GameConfig");
            StringBuilder sb = new StringBuilder(1024 * 5000);
            List<Variant> list = m_collection.FindAllAs<Variant>().ToList();
            int count = 0;
            //WriteAllKey(path, list);
            foreach (Variant v in list)
            {
                string author = v.GetStringOrEmpty("Author");
                string modified = v.GetStringOrEmpty("Modified");
                v.Remove("Author");
                v.Remove("Modified");
                SortedDictionary<string, object> sorted = ConvertSortedDic(v);

                string x = JsonConvert.SerializeObject(sorted);
                string mainType = v.GetStringOrDefault("MainType");
                string subType = v.GetStringOrDefault("SubType");
                string id = v.GetStringOrDefault("_id");
                string name = v.GetStringOrDefault("Name");

                bool chekc = v.CheckKey();
                if (!chekc)
                {
                    Console.WriteLine(id + "  " + name);
                }

                string dName = path;
                if (mainType != null)
                {
                    dName = Path.Combine(path, mainType);
                    if (!System.IO.Directory.Exists(dName))
                    {
                        System.IO.Directory.CreateDirectory(dName);
                    }
                }
                if (subType != null)
                {
                    dName = Path.Combine(dName, subType);
                    if (!System.IO.Directory.Exists(dName))
                    {
                        System.IO.Directory.CreateDirectory(dName);
                    }
                }

                try
                {
                    dName = Path.Combine(dName, id + ".txt");
                    using (FileStream fs = File.Open(dName, FileMode.OpenOrCreate))
                    using (StreamWriter sw = new StreamWriter(fs, noBomUTF8))
                    {
                        sw.WriteLine(x);
                    }

                    int ver = v.GetIntOrDefault("Ver");
                    sb.Append(id);
                    sb.Append("	");
                    sb.Append(ver.ToString());
                    sb.Append("	");
                    sb.Append(author);
                    sb.Append("	");
                    sb.Append(modified);
                    sb.Append("	");
                    sb.AppendLine(name);

                    count++;
                }
                catch (Exception err)
                {
                    MessageBox.Show("配置文件名错误:" + dName + "  " + err.TargetSite);
                }
            }

            string logName = Path.Combine(path, "LoadLog.txt");
            using (FileStream fs = File.Open(logName, FileMode.OpenOrCreate))
            using (StreamWriter sw = new StreamWriter(fs, noBomUTF8))
            {
                sw.Write(sb.ToString());
            }

            return count;
        }

        /// <summary>
        /// 转换为排序字典,方便序列化后进行比较
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        static SortedDictionary<string, object> ConvertSortedDic(IDictionary<string, object> dic)
        {
            SortedDictionary<string, object> sortedDic = new SortedDictionary<string, object>();
            foreach (var item in dic)
            {
                IDictionary<string, object> value = item.Value as IDictionary<string, object>;
                if (value != null)
                {
                    SortedDictionary<string, object> obj = ConvertSortedDic(value);
                    sortedDic.Add(item.Key, obj);
                }
                else
                {
                    sortedDic.Add(item.Key, item.Value);
                }
            }
            return sortedDic;
        }


    }
}
