using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security;
using System.ServiceProcess;
using System.Text;
using SharpSvn;
using Sinan.BabyAssistant.Command;

namespace Sinan.BabyAssistant
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                InstallHelper.Description = "BabyAssistant";
                InstallHelper.Install(args);
                return;
            }
            AssistantService assistan = new AssistantService();
            //assistan.Start(args);
            //Console.ReadLine();
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                assistan
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
