using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using MongoDB.Bson.Serialization;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Observer;

namespace Sinan.FrontServer
{
    class Program
    {
        [LoaderOptimizationAttribute(LoaderOptimization.MultiDomainHost)]
        static void Main(string[] args)
        {
            //QQLog.CheckLog();
            AppDomain app = AppDomain.CurrentDomain;
            app.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);
            app.ProcessExit += new EventHandler(Exit);
            app.DomainUnload += new EventHandler(Exit);

            if (args.Length == 0)
            {
                string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "47baby_hwj.txt");
                BaseConfig config = InitServer(file, true);
                FrontApplication.Instance.Start(config, true);
                ServerManager.FastLogin = true;

                while (true)
                {
                    string message = Console.ReadLine();
                    if (message == "exit")
                    {
                        return;
                    }
                    ShowCommand(message);
                }
            }
            else
            {
                string file = args[0];
                if (File.Exists(file))
                {
                    BaseConfig config = InitServer(file, false);
                    FrontApplication.Instance.Start(config, false);
#if !mono
                    if (app.Id == 1) //主应用程序域. Linux下为0.Winodws下为1
                    {
                        System.Threading.Thread.Sleep(Timeout.Infinite);
                    }
#endif
                }
            }
        }

        static void Exit(object sender, EventArgs e)
        {
            FrontApplication.Instance.Stop();
        }

        /// <summary>
        /// 初始化服务
        /// </summary>
        /// <param name="file">配置文件</param>
        public static BaseConfig InitServer(string file, bool readConfig)
        {
            BaseConfig config = BaseConfig.Create(file);
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(config.Log4Config));
            BsonSerializer.RegisterSerializationProvider(new DynamicSerializationProvider());
            try
            {
                if (readConfig)
                {
                    ConfigFile.ConfigToFile(config.DbBase, config.DirGame);
                }
            }
            catch (Exception err)
            {
                LogWrapper.Fatal("file err:" + file, err);
                throw;
            }
            return config;
        }


        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //var x = System.Threading.CompressedStack.Capture();
            Exception err = e.ExceptionObject as Exception;
            LogWrapper.Fatal(sender == null ? string.Empty : sender.ToString(), err);
        }

        /// <summary>
        /// 显示命令
        /// </summary>
        /// <param name="message"></param>
        private static void ShowCommand(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            String[] msg = message.Split(' ');
            List<string> x = msg.ToList();
            x.RemoveAt(0);
            var v = AmfCodec.Encode(msg[0], x);
            StringBuilder sb = new StringBuilder(v.Count << 2);
            for (int i = 0; i < v.Count; i++)
            {
                if (i > 0) sb.Append(" ");
                sb.Append(v.Array[v.Offset + i].ToString("X2"));
            }
            Console.WriteLine(sb.ToString());
        }
    }
}
