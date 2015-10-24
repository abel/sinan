//#define  TraceCall
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.FastSocket;
using Sinan.Log;
using Sinan.Observer;
using Sinan.AMF3;
using Sinan.GameModule;
using log4net;

namespace Sinan.FrontServer
{
    sealed class GameProcessor : CommandProcessor, ICommandProcessor<Tuple<int, List<object>>>
    {
        private static ILog logger = LogManager.GetLogger("ClientWatch");

        public GameProcessor(int capacity)
            : base(capacity)
        {
        }

        public override bool Execute(ISession token, Tuple<int, List<object>> bin)
        {
            UserSession user = token as UserSession;
            try
            {
                int command = bin.Item1;
                string name = DecodeCommand(user, command);
                if (name != null)
                {
                    if (command >= 1000)
                    {
                        user.LastCommand = bin;

                        PlayerBusiness player = user.Player;
                        if (player != null)
                        {
                            if (WatchPlayerManager.Instance.Contains(player.PID))
                            {
                                logger.Warn(string.Format("{0}:{1}_{2}", player.ID, command.ToString(), name));
                            }
                        }
                    }
                    //name==string.Empty(命令已执行或忽略)
                    if (name != string.Empty)
                    {
                        UserNote note = new UserNote(user, name, bin.Item2);
                        Notifier.Instance.Publish(note, false);
                        //TODO: 屏蔽用户调用提示
                        //LogWrapper.Debug(string.Format("{0}--{1}", user.UserID, name));
                    }
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                LogWrapper.Error(user.ToString(), ex);
            }
            return false;
        }

    }
}
