using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Sinan.AMF3;
using Sinan.Core;
using Sinan.FastConfig;
using Sinan.FastSocket;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Schedule;
using Sinan.Observer;
using Sinan.Command;

#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FrontServer
{
    /// <summary>
    /// ISessionManager的实现部分..
    /// </summary>
    sealed public class FrontApplication : Application
    {
        static FrontApplication()
        {
            m_instance = new FrontApplication();
        }
        /// <summary>
        /// 唯一实例
        /// </summary>
        static public new FrontApplication Instance
        {
            get { return m_instance as FrontApplication; }
        }

        AmfServer server;
        AmfServer gmServer;

        //配置接口
        ConfigFacade m_gameFacade;
        ConfigFacade m_baseFacade;
        ConfigFacade m_scriptFacade;
        List<ISchedule> sechedules;

        private FrontApplication() { }


        /// <summary>
        ///  开始服务
        /// </summary>
        public void Start(BaseConfig config, bool show = false)
        {
            var mediators = ServerManager.LoadMediators(config.DirPlugin);
            ConfigLoader.Init(config);

            this.LoadDBFacade(config.DirDB);
            this.LoadBaseFacade(config.DirBase);
            this.LoadGameFacade(config.DirGame);

            ScenesProxy.LoadScenes(this);

            //重置在线信息
            int count = PlayerAccess.Instance.ResetOnline();
            count = FamilyBossAccess.Instance.ResetFight();
            base.Start();
            this.InitNetFacade(config.Crossdomain);

            sechedules = ServerManager.LoadSchedules(config.DirPlugin);
            if (show)
            {
                ShowMediators(mediators);
                ShowSchedules(sechedules);
            }
            LogWrapper.Warn("Successful startup:" + count);
        }

        /// <summary>
        /// 初始化网络监听
        /// </summary>
        /// <param name="crossdomain"></param>
        private void InitNetFacade(string crossdomain)
        {
            string policy = null;
            try
            {
                if (!string.IsNullOrEmpty(crossdomain))
                {
                    using (FileStream fs = File.OpenRead(crossdomain))
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
                    {
                        policy = sr.ReadToEnd();
                    }
                }
            }
            catch { }

            int maxClient = ConfigLoader.Config.MaxClient;
            int sendQueueSize = ConfigLoader.Config.SendQueueSize;

            const int receiveSize = 8 * 1024;
            const int sendSize = 64 * 1024;

            AmfCodec.Init(maxClient, CommandManager.Instance);
            IHandshake hander = new Handshaker(policy);

            GMProcessor gmProcessor = new GMProcessor(receiveSize);
            SessionFactory gmFactory = new SessionFactory(8, receiveSize, sendSize, 8, gmProcessor);
            gmServer = new AmfServer(gmFactory, gmFactory, hander);
            gmServer.Start(ConfigLoader.Config.EpGM);

            GameProcessor gameProcessor = new GameProcessor(receiveSize);
            SessionFactory gameFactory = new SessionFactory(maxClient, receiveSize, sendSize, sendQueueSize, gameProcessor);
            server = new AmfServer(gameFactory, gameFactory, hander);
            server.Start(ConfigLoader.Config.EpGame);

            Notifier.Instance.Publish(new Notification(GMCommand.GMStart, new object[] { this }), false);

        }

        /// <summary>
        /// 注册监听器,并加载基本配置文件
        /// </summary>
        /// <param name="path"></param>
        void LoadBaseFacade(string path)
        {
            m_baseFacade = new ConfigFacade(path, "*.txt", false);
            m_baseFacade.RegistConfigProcessor(RareGoodsManager.Instance, "RareGoods.txt");
            m_baseFacade.RegistConfigProcessor(BondBuyManager.Instance, "OneBondBuy.txt");
            m_baseFacade.RegistConfigProcessor(PetAccess.Instance, "PetConfig.txt");
            m_baseFacade.RegistConfigProcessor(StringFilter.Instance, "FilterWord.txt");
            m_baseFacade.RegistConfigProcessor(NameManager.Instance, "WhiteChars.txt");
            m_baseFacade.RegistConfigProcessor(AwardManager.Instance, "Award.txt");
            m_baseFacade.RegistConfigProcessor(TipManager.Instance, "Tip.txt");
            m_baseFacade.RegistConfigProcessor(CommandManager.Instance, "Command.txt");
            m_baseFacade.RegistConfigProcessor(RoleManager.Instance, "RoleInfo.txt");
            m_baseFacade.RegistConfigProcessor(GMManager.Instance, "GMList.txt");
            m_baseFacade.RegistConfigProcessor(BlackListManager.Instance, "IPBlackList.txt");
            m_baseFacade.RegistConfigProcessor(WatchPlayerManager.Instance, "WatchPlayer.txt");
            m_baseFacade.RegistConfigProcessor(OrderTypeManager.Instance, "OrderType.txt");
            m_baseFacade.RegistConfigProcessor(ServerManager.Instance, "server" + ConfigLoader.Config.Zoneid + ".txt");
            m_baseFacade.LoadAll(LogWrapper.Warn);
            //开始监听文件变化
            m_baseFacade.Enable = true;

        }

        void LoadDBFacade(string path)
        {
            m_scriptFacade = new ConfigFacade(Path.Combine(path, "script"), "*.sql", false);

            //注册默认处理器
            m_scriptFacade.RegistConfigProcessor(SqlScriptManager.Instance, "*");
            m_scriptFacade.LoadAll(LogWrapper.Warn);

            //开始监听文件变化
            m_scriptFacade.Enable = true;
        }

        /// <summary>
        /// 注册监听器,并加载游戏配置文件
        /// </summary>
        void LoadGameFacade(string path)
        {
            m_gameFacade = new ConfigFacade(path, "*.txt", true);
            m_gameFacade.RegistConfigProcessor(ApcManager.Instance, "APC");
            m_gameFacade.RegistConfigProcessor(NpcManager.Instance, "NPC");
            m_gameFacade.RegistConfigProcessor(ScenePinManager.Instance, "ScenePin");
            m_gameFacade.RegistConfigProcessor(HideApcManager.Instance, "APCFactory", "HideAPC");
            m_gameFacade.RegistConfigProcessor(VisibleAPCManager.Instance, "APCFactory", "VisibleAPC");

            m_gameFacade.RegistConfigProcessor(BoxManager.Instance, "Box");
            m_gameFacade.RegistConfigProcessor(PartManager.Instance, "Part");

            //注册默认处理器
            m_gameFacade.RegistConfigProcessor(GameConfigAccess.Instance, "*");
            m_gameFacade.LoadAll(LogWrapper.Warn);

            //开始监听文件变化
            m_gameFacade.Enable = true;
        }

        public override void Stop()
        {
            //发送停止通知
            base.Stop();
            if (server != null)
            {
                server.Close();
            }
            if (m_gameFacade != null)
            {
                m_gameFacade.Enable = false;
            }
            if (sechedules != null)
            {
                foreach (var sechedule in sechedules)
                {
                    sechedule.Close();
                }
            }
        }

        //重新开始服务
        public bool Restart()
        {
            //if (server != null && server.ListenCount == 0)
            //{
            //    if (m_gameFacade != null)
            //    {
            //        m_gameFacade.Enable = true;
            //    }

            //    server.Start(ConfigLoader.Config.EpGame);
            //    return true;
            //}
            return false;
        }

        private static void ShowSchedules(List<Schedule.ISchedule> sechedules)
        {
            foreach (var t in sechedules)
            {
                Console.WriteLine(t.GetType().FullName);
            }
        }

        private static void ShowMediators(Dictionary<Assembly, HashSet<Type>> mediators)
        {
            Assembly current = Assembly.GetExecutingAssembly();
            foreach (var h in mediators)
            {
                if (h.Key != current)
                {
                    foreach (Type t in h.Value)
                    {
                        Console.WriteLine(string.Format("Notifier.Instance.Subscribe(new {0}());", t.FullName));
                    }
                }
            }
        }
    }
}
