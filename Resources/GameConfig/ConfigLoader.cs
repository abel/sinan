using System;
using System.IO;
using System.Text;
using Sinan.FastJson;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.GameModule
{
    /// <summary>
    /// 管理数据库连接
    /// </summary>
    public static class ConfigLoader
    {
        public static BaseConfig Config;

        public static void Init(BaseConfig config)
        {
            Config = config;
            InitDataBase(Config.DirDB);
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static void InitDataBase(string path)
        {
            //创建表:??

            //创建索引
            string indexPath = Path.Combine(path, "index");
            if (Directory.Exists(indexPath))
            {
                string[] files = Directory.GetFiles(indexPath, "*.index", SearchOption.TopDirectoryOnly);
                IndexCreater indexCreater = new IndexCreater(Config.DbPlayer);
                foreach (var v in files)
                {
                    indexCreater.LoadIndex(v);
                }
            }
            InitDataBase();
        }

        /// <summary>
        /// 初始化数据库连接
        /// </summary>
        /// <param name="config"></param>
        static void InitDataBase()
        {
            string player = Config.DbPlayer;
            string gameBase = Config.DbBase;
            string gameLog = Config.DbLog;

            //gameBase
            WordAccess.Instance.Connect(gameBase);
            GameConfigAccess.Instance.Connect(gameBase);

            //player
            AuctionAccess.Instance.Connect(player);
            BoardAccess.Instance.Connect(player);
            EmailAccess.Instance.Connect(player);
            FamilyAccess.Instance.Connect(player);
            GoodsAccess.Instance.Connect(player);
            PetAccess.Instance.Connect(player);
            PlayerAccess.Instance.Connect(player);
            PlayerExAccess.Instance.Connect(player);
            TaskAccess.Instance.Connect(player);
            TopAccess.Instance.Connect(player);
            UserLogAccess.Instance.Connect(player);
            OrderAccess.Instance.Connect(player);
            PartAccess.Instance.Connect(player);
            NoticeAccess.Instance.Connect(player);
            LevelLogAccess.Instance.Connect(player);
            FamilyBossAccess.Instance.Connect(player);
            MountsAccess.Instance.Connect(player);
            PlayerSortAccess.Instance.Connect(player, Config.ZoneEpoch);

            //Log
            LogAccess.Instance.Connect(gameLog, "Log");
            UserLogAccess.Instance.Verification = new LoginVerification(Config.LoginKey, Config.DesKey);

            //创建第一个角色ID..
            PlayerAccess.Instance.InitPlayerID(Config.Zoneid);
        }

        //public static Variant LoadVariant(string file)
        //{
        //    //Console.WriteLine("加载文件:" + file);
        //    using (FileStream fs = File.OpenRead(file))
        //    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
        //    {
        //        StringBuilder sb = new StringBuilder(2048);
        //        string item = sr.ReadLine();
        //        while (item != null)
        //        {
        //            item = item.Trim();
        //            if (item.Length > 0)
        //            {
        //                if (item[0] != '/' && item[0] != '*')
        //                {
        //                    sb.Append(item);
        //                }
        //            }
        //            item = sr.ReadLine();
        //        }
        //        item = sb.ToString();
        //        return JsonConvert.DeserializeObject<Variant>(item);
        //    }
        //}

    }
}
