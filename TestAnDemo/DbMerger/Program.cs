using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Sinan.Data;
using Sinan.FastJson;
using Sinan.Util;

namespace DbMerger
{
    class Program
    {

        static bool DecodeParams(int seed, byte[] buffer, int offset, int count)
        {
            int max = offset + 2;
            for (int i = offset; i < max; i++)
            {
                seed = (seed << 3) - seed + buffer[i];
            }
            max = offset + count;
            for (int i = offset + 2; i < max; i++)
            {
                byte b = buffer[i];
                byte newb = (byte)(b - seed);
                buffer[i] = newb;
                seed = (seed << 3) - seed + newb;
            }
            return true;
        }

        static bool EncodeParams(int seed, byte[] buffer, int offset, int count)
        {
            int max = offset + 2;
            for (int i = offset; i < max; i++)
            {
                seed = (seed << 3) - seed + buffer[i];
            }

            max = offset + count;
            for (int i = offset + 2; i < max; i++)
            {
                byte old = buffer[i];
                byte newb = (byte)(old + seed);
                buffer[i] = newb;
                seed = (seed << 3) - seed + old;
            }
            return true;
        }

        static void Main(string[] args)
        {
            System.Random random = new System.Random();
            int count = 10;
            byte[] s = new byte[count];

            byte[] r = new byte[count];
            for (int i = 0; i < 20; i++)
            {
                int h = 23;// random.Next();
                random.NextBytes(s);
                Buffer.BlockCopy(s, 0, r, 0, count);
                for (int t = 0; t < count; t++)
                {
                    s[t] = (byte)t;
                    r[t] = (byte)t;
                }
                //编码
                EncodeParams(h, s, 0, count);

                //解码
                DecodeParams(h, s, 0, count);
                //比较
                bool x = true;
                for (int t = 0; t < count; t++)
                {
                    if (s[t] != r[t])
                    {
                        x = false;
                    }
                }
                Console.WriteLine(x);
            }

            WriteGameConfig();
            return;

            //int count = MergerDB.CoverDB();
            //Console.WriteLine("全部合并完成,共" + count);
            //Console.ReadLine();
        }

        /// <summary>
        /// 恢复配置文件数据
        /// </summary>
        private static void WriteGameConfig()
        {
            BsonSerializer.RegisterSerializationProvider(new DynamicSerializationProvider());

            MongoDatabase newDB = MongoDatabase.Create(@"mongodb://192.168.100.40:27017/game2?safe=true");
            var coll = newDB.GetCollection<Variant>("GameConfig");

            int count = 0;
            string dir = @"E:\Work\47Baby\server\布署\47babyService\Config\game";
            string[] files = System.IO.Directory.GetFiles(dir, "*.txt", System.IO.SearchOption.AllDirectories);
            foreach (var file in files)
            {
                using (FileStream fs = File.OpenRead(file))
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
                {
                    count++;
                    Variant v = JsonConvert.DeserializeObject<Variant>(sr.ReadToEnd());
                    GameConfig config = new GameConfig();
                    config.ID = v.GetStringOrDefault("_id");
                    config.Name = v.GetStringOrDefault("Name");
                    config.MainType = v.GetStringOrDefault("MainType");
                    config.SubType = v.GetStringOrDefault("SubType");
                    config.Ver = v.GetIntOrDefault("ver");
                    config.Modified = v.GetDateTimeOrDefault("Modified");
                    config.Author = v.GetStringOrDefault("Author");
                    config.UI = v.GetVariantOrDefault("UI");
                    config.Value = v.GetVariantOrDefault("Value");
                    coll.Save(config);
                }
            }
            Console.WriteLine("全部合并完成,共" + count);
            Console.ReadLine();
        }
    }
}
