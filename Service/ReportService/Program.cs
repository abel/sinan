using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Sinan.Log;

namespace Sinan.ReportService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
#if !mono
            if (args.Length > 0)
            {
                InstallHelper.Description = "47ReportService";
                InstallHelper.Install(args);
                //Console.WriteLine("按任意键退出......");
                //Console.Read();
                return;
            }
#endif

            string curDir = System.AppDomain.CurrentDomain.BaseDirectory;
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(Path.Combine(curDir, "log4net.config")));

            DCService s = new DCService();
            //s.Exec();

            ServiceBase[] ServicesToRun = new ServiceBase[] { s };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
