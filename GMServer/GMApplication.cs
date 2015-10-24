using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MongoDB.Bson.Serialization;
using Sinan.AMF3;
using Sinan.Core;
using Sinan.Data;
using Sinan.FastSocket;
using Sinan.FrontServer;
using Sinan.Log;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.GMServer
{
    class GMApplication : Application
    {
        /// <summary>
        /// 默认的配置文件目录
        /// </summary>
        public const string DefaultConfig = "Config";
        public static string BaseDirectory;


        /// <summary>
        /// 初始化服务
        /// </summary>
        public static void InitServer()
        {
            BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(Path.Combine(BaseDirectory, "log4net.config")));
            BsonSerializer.RegisterSerializationProvider(new DynamicSerializationProvider());
        }

        static GMApplication()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            m_instance = new GMApplication();
        }

        /// <summary>
        /// 唯一实例
        /// </summary>
        static public new GMApplication Instance
        {
            get { return m_instance as GMApplication; }
        }

        GMService server;

        private GMApplication() { }

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
        ///  开始服务
        /// </summary>
        public override void Start()
        {
            string plugins = Path.Combine(BaseDirectory, "Plugins");
            var mediators = LoadMediators(plugins);

            string config = Path.Combine(BaseDirectory, DefaultConfig);

            base.Start();
            this.InitNetFacade(config);

            LogWrapper.Warn("启动成功");
        }

        /// <summary>
        /// 初始化网络监听
        /// </summary>
        /// <param name="path"></param>
        private void InitNetFacade(string path)
        {
            string policy = null;
            try
            {
                using (FileStream fs = File.OpenRead(Path.Combine(path, "crossdomain.txt")))
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
                {
                    policy = sr.ReadToEnd();
                }
            }
            catch { }

            GMManager.Instance.Load(Path.Combine(path, "GMList.txt"));
            FrontManager.Instance.Load(Path.Combine(path, "FrontIP.txt"));

            AmfStringZip zip = new CommandMap();
            zip.Load(Path.Combine(path, "Command.txt"));

            string ip = "0.0.0.0";
            string ports = "8005";
            int maxClient = 1000;
            int maxWaitSend = 64;

            const int receiveSize = 8 * 1024;
            const int sendSize = 64 * 1024;

            AmfCodec.Init(maxClient, zip);
            IHandshake hander = new Handshaker(policy);

            CommandProcessor gameProcessor = new CommandProcessor(receiveSize, zip);
            SessionFactory gameFactory = new SessionFactory(receiveSize, sendSize, maxWaitSend, gameProcessor);
            server = new GMService(gameFactory, gameFactory, hander);
            server.Start(ip, ports.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
        }

        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception err = e.ExceptionObject as Exception;
            LogWrapper.Fatal(sender == null ? string.Empty : sender.ToString(), err);
        }

        public override void Stop()
        {
            //发送停止通知
            base.Stop();
            if (server != null)
            {
                server.Close();
            }
        }

    }
}
