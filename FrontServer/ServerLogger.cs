using System;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Reflection;
using log4net;
using MongoDB.Driver;
using Sinan.Log;
using Sinan.Schedule;
using Sinan.GameModule;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FrontServer
{
    /// <summary>
    /// 服务报告d
    /// 向网站提交服务状态
    /// </summary>
    public sealed class ServerLogger : SchedulerBase
    {
        private static ILog logger = LogManager.GetLogger("Report");

        /// <summary>
        /// 应用分区分服时大区的ID
        /// </summary>
        static public readonly int zoneid;

        /// <summary>
        /// 服务器端的IP地址
        /// </summary>
        static public readonly long sip;

        /// <summary>
        /// 服务器版本号
        /// </summary>
        static public string serverVer;

        static ServerLogger()
        {
            zoneid = ConfigLoader.Config.Zoneid;
            sip = IPAddress.Parse(ConfigLoader.Config.ReportSIP).Address;
            //得到程序集的版本号.
            Assembly assem = Assembly.GetExecutingAssembly();
            Version v = assem.GetName().Version;
            serverVer = v.ToString();
        }

        static readonly System.Collections.Concurrent.ConcurrentQueue<LogBase> m_logs = new System.Collections.Concurrent.ConcurrentQueue<LogBase>();

        #region 唯一实例
        ServerLogger()
            : base(60 * 1000, 60 * 1000)
        {
        }
        #endregion

        #region 定时报告在线情况
        protected override void Exec()
        {
            OnlineLog log = new OnlineLog(zoneid);
            log.user_num = PlayersProxy.OnlineCount;
            WriteLog(null, log);
        }
        #endregion

        static public void WriteLog(UserSession session, LogBase log)
        {
            if (log == null) return;
            log.worldid = zoneid;
            log.svrip = sip;
            if (session != null)
            {
                log.key = session.key;
                log.userip = session.IP.Address;
                log.domain = (int)(session.domain);
                if (log.opopenid == null)
                {
                    log.opopenid = session.UserID;
                }
                if (log.opuid == 0)
                {
                    PlayerBusiness player = session.Player;
                    if (player != null)
                    {
                        log.opuid = player.PID;
                        log.worldid = session.zoneid;
                    }
                }
            }
            m_logs.Enqueue(log);
        }

        Thread m_logger;
        public override void Start()
        {
            lock (m_locker)
            {
                base.Start();
                m_logger = new Thread(new ThreadStart(StartWrite));
                m_logger.Start();
            }
        }

        static private void StartWrite()
        {
            try
            {
                while (true)
                {
                    LogBase log;
                    const int count = 16;
                    List<LogBase> logs = new List<LogBase>(count);
                    while (m_logs.TryDequeue(out log))
                    {
                        logs.Add(log);
                        if (logs.Count == count)
                        {
                            WriteLogs(logs);
                        }
                    }
                    if (logs.Count > 0)
                    {
                        WriteLogs(logs);
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(200);
                    }
                }
            }
            catch { }
        }

        private static void WriteLogs(List<LogBase> logs)
        {
            try
            {
                LogAccess.Instance.InsertBatch(logs);
            }
            catch (MongoConnectionException)
            {
                //连接失败,等待1分钟后重试.
                System.Threading.Thread.Sleep(60 * 1000);
                //每10秒钟重试一次,共重试360次(共1个小时)
                TryWrite(logs, 360, 10 * 1000);
            }
            catch
            {
                //每秒钟重试一次,重试10次.
                TryWrite(logs, 10, 1000);
            }
            finally
            {
                logs.Clear();
            }
        }

        public override void Close()
        {
            lock (m_locker)
            {
                if (m_logger != null)
                {
                    try
                    {
                        m_logger.Abort();
                    }
                    catch { }
                }
                base.Close();
            }

        }

        static StringBuilder sb = new StringBuilder(2046);
        /// <summary>
        /// 重新写日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="retry">重试指定次数,直到写入成功</param>
        /// <param name="sleep"></param>
        static private void TryWrite(LogBase log, int retry, int sleep)
        {
            // 最多缓存4万条,大概1个小时的数据量
            const int maxCount = 40000;
            for (int i = 0; i < retry; i++)
            {
                if (m_logs.Count > maxCount)
                {
                    break;
                }
                try
                {
                    LogAccess.Instance.Insert(log);
                    return;
                }
                catch { }
                System.Threading.Thread.Sleep(sleep);
            }
            //尝试指定次数失败或缓存过多时,写入本地日志文件
            try
            {
                sb.Clear();
                sb.Append("Logid=");
                sb.Append(log.ID.ToString());
                sb.Append("&");
                logger.Info(log.ToString(sb));
            }
            catch { }
        }

        /// <summary>
        /// 重新写日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="retry">重试指定次数,直到写入成功</param>
        /// <param name="sleep"></param>
        static private void TryWrite(List<LogBase> logs, int retry, int sleep)
        {
            // 最多缓存4万条,大概1个小时的数据量
            const int maxCount = 40000;
            for (int i = 0; i < retry; i++)
            {
                if (m_logs.Count > maxCount)
                {
                    break;
                }
                try
                {
                    LogAccess.Instance.InsertBatch(logs);
                    return;
                }
                catch { }
                System.Threading.Thread.Sleep(sleep);
            }
            //尝试指定次数失败或缓存过多时,写入本地日志文件
            foreach (var log in logs)
            {
                try
                {
                    sb.Clear();
                    sb.Append("Logid=");
                    sb.Append(log.ID.ToString());
                    sb.Append("&");
                    logger.Info(log.ToString(sb));
                }
                catch { }
            }
        }
    }
}
