using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Sinan.Log;
using System.Threading;

namespace Sinan.ShipmentService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
            FileInfo f = new System.IO.FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config"));
            log4net.Config.XmlConfigurator.Configure(f);
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            RouteService route = new RouteService();

            if (args.Length > 0)
            {
                route.Start();
                Console.WriteLine("ShipmentRoute开始");
                while (true)
                {
                    string message = Console.ReadLine();
                    if (message == "exit")
                    {
                        System.Environment.Exit(0);
                    }
                }
            }

            ServiceBase.Run(new ServiceBase[] { route });

            //LogWrapper.Warn("监听中...?");
            //System.Threading.Thread.Sleep(Timeout.Infinite);
            //LogWrapper.Warn("进程结束");
        }


        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception err = e.ExceptionObject as Exception;
            LogWrapper.Fatal(sender == null ? string.Empty : sender.ToString(), err);
        }
    }
}
