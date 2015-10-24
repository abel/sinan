using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.FastJson;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Util;

namespace Sinan.GameModule
{
    public static class ConfigFile
    {
        static readonly Encoding noBomUTF8 = new System.Text.UTF8Encoding(false);

        /// <summary>
        /// 将数据库配置转换为本地文件
        /// </summary>
        /// <param name="connect">连接字符串</param>
        /// <param name="path"></param>
        public static void ConfigToFile(string connect, string path)
        {
            GameConfigAccess.Instance.Connect(connect);
            StringBuilder sb = new StringBuilder(1024 * 5000);
            List<Variant> list = GameConfigAccess.Instance.FindAllVariant();
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
                }
                catch (Exception err)
                {
                    LogWrapper.Warn("file err:" + dName, err);
                }
            }

            string logName = Path.Combine(path, "LoadLog.txt");
            using (FileStream fs = File.Open(logName, FileMode.OpenOrCreate))
            using (StreamWriter sw = new StreamWriter(fs, noBomUTF8))
            {
                sw.Write(sb.ToString());
            }
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

        private static void WriteAllKey(string path, List<Variant> list)
        {
            foreach (Variant v in list)
            {
                AddStringKeys(v);
            }
            string logName = Path.Combine(path, "AllKeys_temp.txt");
            using (FileStream fs = File.Open(logName, FileMode.OpenOrCreate))
            using (StreamWriter sw = new StreamWriter(fs, noBomUTF8))
            {
                foreach (var v in AllKeys.Keys)
                {
                    string jsonv = JsonConvert.SerializeObject(v);
                    sw.WriteLine(jsonv);
                }
            }
        }

        public static void AddStringKeys(IDictionary<string, object> dic, bool readValue = true)
        {
            foreach (var item in dic)
            {
                IDictionary<string, object> value = item.Value as IDictionary<string, object>;
                if (value != null)
                {
                    AddStringKeys(value, readValue);
                }
                else if (readValue && item.Value is string)
                {
                    string str = item.Value.ToString().TrimEnd();
                    AllKeys.SetOrInc(str, 1);
                }
                AllKeys.SetOrInc(item.Key, 1);
            }
        }

        public static readonly SortedDictionary<string, int> AllKeys = new SortedDictionary<string, int>();

        ///// <summary>
        ///// 转换信息.
        ///// </summary>
        //public static void ReadTable(string path)
        //{
        //    List<string> tables = new List<string> { "Auction", "Email", "Family", "Goods", "Mall", "Pet", "PlatformTotal", "Player", "PlayerEx", "Task", "UserLog" };
        //    string connectionString = ConfigurationManager.ConnectionStrings["player"].ConnectionString;
        //    foreach (string name in tables)
        //    {
        //        Console.WriteLine("正在读取:" + name);
        //        VariantBuilder<Variant> access = new VariantBuilder<Variant>(name);
        //        access.Connect(connectionString);
        //        var all = access.FindAll(5000);
        //        foreach (Variant pet in all)
        //        {
        //            //创建默认的.
        //            AddStringKeys(pet, false);
        //        }
        //    }
        //    string logName = Path.Combine(path, "AllKeys.txt");
        //    using (FileStream fs = File.Open(logName, FileMode.OpenOrCreate))
        //    using (StreamWriter sw = new StreamWriter(fs))
        //    {
        //        foreach (var v in AllKeys.Keys)
        //        {
        //            string jsonv = JsonConvert.SerializeObject(v);
        //            sw.WriteLine(jsonv);
        //        }
        //    }
        //    Console.WriteLine("执行完成");
        //    System.Environment.Exit(0);
        //}
    }
}
