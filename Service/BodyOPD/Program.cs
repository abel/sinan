using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Entity;

namespace Sinan.BabyOPD
{
    static class Program
    {
        static string playerDB = ConfigurationManager.ConnectionStrings["player"].ConnectionString;
        static string gameLogString = ConfigurationManager.ConnectionStrings["gameLog"].ConnectionString;
        static string operationString = ConfigurationManager.ConnectionStrings["operation"].ConnectionString;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            //LiShiLv();
            PlatformTotal();

            //Rename();
            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[] 
            //{ 
            //    new BabyOPDService() 
            //};
            //ServiceBase.Run(ServicesToRun);
        }

        //对相同名字的玩家重命名
        private static void Rename()
        {
            int changed = 0;
            string newIndex = "00ACDEFGHIJKLMNOPQRSTUVWXYZ";
            Dictionary<string, int> hash1 = new Dictionary<string, int>();
            Player[] players = PlayerAccess.Instance.FindAll();
            foreach (var player in players)
            {
                int count = hash1.SetOrInc(player.Name, 1);
                if (count > 1)
                {
                    changed++;
                    string newName = player.Name + newIndex[count];
                    PlayerAccess.Instance.Rename(player.PID, newName);
                }
                else
                {
                    //Word word = new Word();
                    //word.Key = player.Name;
                    //word.State = Word.Used;
                    //WordAccess.Instance.Save(word);
                }
            }
            Console.WriteLine("更新:" + changed);
        }

        /// <summary>
        /// 统计平台数据
        /// </summary>
        private static void PlatformTotal()
        {
            PlatformTotalAccess total = new PlatformTotalAccess(playerDB, gameLogString, operationString);
            Dictionary<int, int> newuser = total.NewUsers();
            int day = 15;
            Dictionary<int, int[]> dddd = new Dictionary<int, int[]>();
            DateTime endTime = DateTime.UtcNow.Date.AddDays(-day);
            for (int i = 0; i < day; i++)
            {
                int id = Convert.ToInt32(endTime.Date.ToString("yyyyMMdd"));
                var data = total.NewUserInfo(endTime);
                total.Save(id.ToString(), data);
                var data2 = total.PlayerLogInfo(endTime);
                total.Save(id.ToString(), data2);
                var date3 = total.WeekInfo(endTime);
                total.Save(id.ToString(), date3);

                total.SaveTotal(endTime);
                endTime = endTime.AddDays(1);
            }
        }

        /// <summary>
        /// 计算流失率
        /// </summary>
        private static void LiShiLv()
        {
            int day = 30;
            PlatformTotalAccess total = new PlatformTotalAccess(playerDB, gameLogString, operationString);
            Dictionary<int, int> newuser = total.NewUsers();
            Dictionary<int, int[]> results = new Dictionary<int, int[]>();
            DateTime endTime = DateTime.UtcNow.Date.AddDays(-day);
            for (int i = 0; i < day; i++)
            {
                int id = Convert.ToInt32(endTime.Date.ToString("yyyyMMdd"));
                //当天登录用户数据..
                Dictionary<int, int> currentLogin = total.LiuShiLVLog(endTime);
                for (int xday = 1; xday < 9; xday++)
                {
                    int hh = id - xday; //前几天的日期..
                    int newUserCount; //前x天注册的新用户数
                    newuser.TryGetValue(hh, out newUserCount);

                    int[] list;
                    if (!results.TryGetValue(hh, out list))
                    {
                        list = new int[10];
                        results[hh] = list;
                    }
                    //前x天的新用户在记录天的登录数
                    int count;
                    currentLogin.TryGetValue(xday, out count);
                    list[0] = newUserCount;
                    if (newUserCount == 0)
                    {
                        list[xday] = 100;
                    }
                    else
                    {
                        list[xday] = (count * 100) / newUserCount;
                    }
                }
                endTime = endTime.AddDays(1);
            }
            StringBuilder sb = new StringBuilder();
            Console.WriteLine("数据");
            foreach (var v in results)
            {
                sb.Append(v.Key.ToString() + "	");
                foreach (var count in v.Value)
                {
                    sb.Append(count + "	");
                }
                sb.AppendLine();
            }
            string xxdd = sb.ToString();
            Console.WriteLine("统计完成");
        }

    }
}
