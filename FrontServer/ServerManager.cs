using System;
using System.Collections.Generic;
using System.Reflection;
using Sinan.Data;
using Sinan.Extensions;
using Sinan.FastConfig;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Schedule;
using Sinan.Util;

namespace Sinan.FrontServer
{

    /// <summary>
    /// 一点券购买物品
    /// </summary>
    public class ServerManager : IConfigManager
    {
        public static string SqlReport;
        public static string SqlReportTable;

        /// <summary>
        /// 报告在线情况的周期(秒)
        /// </summary>
        public static int ReportPeriod = 60;

        /// <summary>
        /// 是否通过GM上下架检查
        /// </summary>
        public static bool IsMall = false;

        /// <summary>
        /// 是否启用高级GM命令
        /// </summary>
        public static bool AdminGM = false;

        /// <summary>
        /// 113.108.20.23
        /// </summary>
        public static string BuyHost;

        /// <summary>
        /// "/v3/pay/buy_goods"
        /// </summary>
        public static string BuyUri;

        /// <summary>
        /// 是否对订单检查签名
        /// </summary>
        public static bool CheckSig;

        /// <summary>
        /// 是否启用快速登录
        /// </summary>
        public static bool FastLogin = false;

        /// <summary>
        /// 新用户是否可以登录
        /// </summary>
        public static bool OpenUser = true;

        /// <summary>
        /// 是否可以创建新角色
        /// </summary>
        public static bool OpenRole = true;


        readonly static ServerManager m_instance = new ServerManager();

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static ServerManager Instance
        {
            get { return m_instance; }
        }

        ServerManager() { }

        public void Load(string path)
        {
            Variant config = VariantWapper.LoadVariant(path);
            if (config != null)
            {
                AysnSubscriber.profile = config.GetBooleanOrDefault("profile", false);
                AysnSubscriber.slowms = config.GetIntOrDefault("slowms", 200);

                SqlReport = config.GetStringOrDefault("SqlReport");
                SqlReportTable = config.GetStringOrDefault("SqlReportTable");
                ReportPeriod = config.GetIntOrDefault("ReportPeriod", 60);
                IsMall = config.GetBooleanOrDefault("IsMall", false);
                AdminGM = config.GetBooleanOrDefault("AdminGM", false);
                BuyHost = config.GetStringOrDefault("BuyHost", "113.108.20.23");
                BuyUri = config.GetStringOrDefault("BuyHost", "/v3/pay/buy_goods");
                CheckSig = config.GetBooleanOrDefault("CheckSig", false);
                FastLogin = config.GetBooleanOrDefault("FastLogin", false);
                OpenUser = config.GetBooleanOrDefault("OpenUser", true);
                OpenRole = config.GetBooleanOrDefault("OpenRole", true);
            }
        }


        public void Unload(string path)
        {
        }

        /// <summary>
        /// 加载所有Mediator
        /// </summary>
        /// <param name="plugins"></param>
        public static Dictionary<Assembly, HashSet<Type>> LoadMediators(string plugins)
        {
            Assembly current = Assembly.GetExecutingAssembly();
            HashSet<Type> set = Notifier.LoadAllSubscribers(current);
            // 反射加载其它Plugins目录下的观察者
            var doc = Notifier.LoadAllSubscribers(plugins);
            if (doc == null)
            {
                doc = new Dictionary<Assembly, HashSet<Type>>();
            }
            doc.Add(current, set);
            return doc;
        }

        /// <summary>
        /// 加载所有定时任务
        /// </summary>
        /// <param name="plugins"></param>
        public static List<ISchedule> LoadSchedules(string plugins)
        {
            // 反射加载其它Plugins目录下的观察者
            Assembly current = Assembly.GetExecutingAssembly();
            List<ISchedule> list = SchedulerBase.LoadAllSchedule(current);
            list.AddRange(SchedulerBase.LoadAllSchedules(plugins));
            foreach (var v in list)
            {
                v.Start();
            }
            return list;
        }
    }
}
