using System.Collections;
using System.Collections.Generic;
using log4net;
using Sinan.Command;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Core;

namespace Sinan.GMServerModule
{
    sealed public class GMMediator : AysnSubscriber
    {
        private static ILog logger = LogManager.GetLogger("GMLog");

        public override IList<string> Topics()
        {
            return new string[] { 
                GMCommand.GMStart,
                Application.APPSTART,
                Application.APPSTOP,
            };
        }

        public override void Execute(INotification notification)
        {
            GMNote note = notification as GMNote;
            if (note == null)
            {
                if (notification.Name == GMCommand.GMStart)
                {
                    StartGM();
                }
                else if (notification.Name == Application.APPSTOP)
                {
                    if (server != null)
                    {
                        server.Stop();
                        server = null;
                    }
                }
                return;
            }
        }

        static GMServer server;
        private static void StartGM()
        {
            if (server != null)
            {
                server.Stop();
                server = null;
            }
            server = new GMServer("/gm", false);
            server.Init();
            server.Start(ConfigLoader.Config.EpGMM);
        }
    }
}
