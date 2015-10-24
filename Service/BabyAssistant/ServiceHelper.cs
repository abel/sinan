using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using Sinan.BabyAssistant.Command;

namespace Sinan.BabyAssistant
{
    public class ServiceHelper
    {
        /// <summary>
        /// 默认等待时间.30秒
        /// </summary>
        static readonly TimeSpan defaultWait = new TimeSpan(0, 0, 30);

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="name">服务名称</param>
        public static bool Start(string name)
        {
            try
            {
                ServiceController sc = new ServiceController(name);
                sc.Refresh();
                int count = 0;
                while (count++ < 60
                    && (sc.Status == ServiceControllerStatus.StopPending
                    || sc.Status == ServiceControllerStatus.ContinuePending
                    || sc.Status == ServiceControllerStatus.PausePending))
                {
                    System.Threading.Thread.Sleep(1000);
                    sc.Refresh();
                }

                if (sc.Status == ServiceControllerStatus.Stopped
                    || sc.Status == ServiceControllerStatus.Paused)
                {
                    sc.Start();
                    for (int i = 0; i < 10; i++)//等待10个30秒(5分钟)
                    {
                        sc.WaitForStatus(ServiceControllerStatus.Running, defaultWait);
                        sc.Refresh();
                        if (sc.Status == ServiceControllerStatus.Running)
                        {
                            return true;
                        }
                        else
                        {
                            Log.LogInfo(name + "正在启动...:" + i);
                        }
                    }
                }
                return sc.Status == ServiceControllerStatus.Running;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="name">服务名称</param>
        public static bool Stop(string name)
        {
            try
            {
                ServiceController sc = new ServiceController(name);
                if (sc.Status == ServiceControllerStatus.Running
                    || sc.Status == ServiceControllerStatus.Paused)
                {
                    sc.Stop();
                    for (int i = 0; i < 10; i++)//等待10个30秒(5分钟)
                    {
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, defaultWait);
                        sc.Refresh();
                        if (sc.Status == ServiceControllerStatus.Stopped)
                        {
                            return true;
                        }
                        else
                        {
                            Log.LogInfo(name + "正在停止...:" + i);
                        }
                    }
                }
                return sc.Status == ServiceControllerStatus.Stopped;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 得到服务可执文件的路径
        /// </summary>
        /// <param name="name">服务名称</param>
        /// <returns>路径</returns>
        public static string GetServicePath(string name)
        {
            string query = string.Format("Select Name,PathName from Win32_Service where Name='{0}'", name);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject m in searcher.Get())
            {
                return m["PathName"].ToString();
            }
            return string.Empty;
        }


        /// <summary>
        /// 判断进程是否存在
        /// </summary>
        /// <param name="path">服务可执行文件路径</param>
        /// <returns>true表示进程存在</returns>
        public static bool ProcessExists(string path)
        {
            Process[] ps = Process.GetProcesses();
            foreach (var t in ps)
            {
                try
                {
                    //排除系统进程(PID小于256)
                    if (t.Id >= 256 && t.ProcessName != "audiodg")
                    {
                        string fileName = t.MainModule.FileName;
                        //Log.LogInfo(fileName);
                        if (fileName == path)
                        {
                            return true;
                        }
                    }
                }
                catch (Exception err)
                {
                    Log.LogInfo(t.ProcessName + ":" + err.Message);
                }
            }
            return false;
        }

    }
}
