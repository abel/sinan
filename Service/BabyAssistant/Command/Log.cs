using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sinan.BabyAssistant.Command
{
    class Log
    {
        /// <summary>
        /// 服务启动
        /// </summary>
        public static string Run = "服务启动...";
        /// <summary>
        /// 服务停止
        /// </summary>
        public static string Stop = "服务停止...";
        /// <summary>
        /// 开始更新
        /// </summary>
        public static string StartUpdate = "更新中...";
        /// <summary>
        /// 正在停止服务
        /// </summary>
        public static string StopService = "正在停止服务【{0}】";
        /// <summary>
        /// 正在更新文件
        /// </summary>
        public static string UpdateFile = "正在更新文件...";
        /// <summary>
        /// 启动服务
        /// </summary>
        public static string StartService = "正在启动服务【{0}】";
        /// <summary>
        /// 更新完成
        /// </summary>
        public static string UpdateFinish = "更新完成";

        /// <summary>
        /// 停止服务成功
        /// </summary>
        public static string StopSucess = "停止服务【{0}】成功";

        /// <summary>
        /// 停止服务失败
        /// </summary>
        public static string StopFail = "停止服务【{0}】失败";

        /// <summary>
        /// 启动服务成功
        /// </summary>
        public static string StartSucess = "启动服务【{0}】成功";

        /// <summary>
        /// 启动服务失败
        /// </summary>
        public static string StartFail = "启动服务【{0}】失败";

        static public void LogInfo(Exception ex)
        {
            if (ex != null)
            {
                LogInfo(ex.Message + ex.TargetSite);
            }
        }

        /// <summary>
        /// 日志记录
        /// </summary>
        /// <param name="msg">日志内容</param>
        static public void LogInfo(string msg)
        {
            try
            {
                string log = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
                if (!Directory.Exists(log))
                {
                    Directory.CreateDirectory(log);
                }
                string xlog = Path.Combine(log, DateTime.Now.ToString("yyyyMMdd") + ".txt");
                if (!File.Exists(xlog))
                {
                    FileStream fs = File.Create(xlog);
                    fs.Close();
                }
                using (FileStream fs = new FileStream(xlog, FileMode.Append, FileAccess.Write))
                {
                    StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                    sw.WriteLine("操作时间:" + DateTime.Now + "," + msg);
                    sw.Flush();
                }
            }
            catch { }
        }
    }
}
