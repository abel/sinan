using System;
using System.Diagnostics;
using System.Text;
using log4net;
using Sinan.Data;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Schedule;
using System.Data.SqlClient;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.GMModule
{
    public sealed class ServerReport : SchedulerBase
    {
        /// <summary>
        /// 服务器端的IP地址
        /// </summary>
        static public readonly long sip;

        static int reportPeriod;

        static ServerReport()
        {
            reportPeriod = ServerManager.ReportPeriod;
        }

        #region 唯一实例
        ServerReport()
            : base(reportPeriod * 1000, reportPeriod * 1000)
        {
        }
        #endregion

        #region 定时报告内存情况
        protected override void Exec()
        {
            try
            {
                Process process = Process.GetCurrentProcess();
                long usedMB = process.WorkingSet64 >> 20;

                StringBuilder sb = new StringBuilder(48);
                sb.Append("ID:");
                sb.Append(ServerLogger.zoneid.ToString());
                sb.Append(",Memory:");
                sb.Append(usedMB.ToString());
                sb.Append(",Cache:");
                sb.Append(PlayersProxy.PlayerCount.ToString());
                sb.Append(",User:");
                sb.Append(UsersProxy.UserCount.ToString());

                sb.Append(",Player:");
                int oCount = PlayersProxy.OnlineCount;
                sb.Append(oCount.ToString());
                LogWrapper.Warn(sb.ToString());

                ReportOnline(oCount);
            }
            catch (Exception ex)
            {
                LogWrapper.Error("MemoryReport:", ex);
            }
        }

        private static void ReportOnline(int count)
        {
            try
            {
                string connect = ServerManager.SqlReport;
                string table = ServerManager.SqlReportTable;
                if (!string.IsNullOrEmpty(connect) && !string.IsNullOrEmpty(table))
                {
                    using (SqlConnection conn = new SqlConnection(connect))
                    {
                        string sql = string.Format("INSERT INTO [{0}] (ServerName,RecDate,ChannelCount,UserCount) VALUES ('0',Getdate(),1,{1})", table, count.ToString());
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }
        #endregion

    }
}

