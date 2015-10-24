using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.AMF3;
using Sinan.FastSocket;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Observer;
using Sinan.Command;

namespace Sinan.GMModule
{
    public class GMProcessor2 : CommandProcessor, ICommandProcessor<Tuple<int, List<object>>>
    {
        public GMProcessor2(int capacity)
            : base(capacity)
        {
        }

        public override bool Execute(ISession token, Tuple<int, List<object>> bin)
        {
            GMToken user = token as GMToken;
            try
            {
                int command = bin.Item1;
                string name = CommandManager.Instance.ReadString(command);
                if (name != null)
                {
                    //name==string.Empty(命令已执行或忽略)
                    if (name != string.Empty)
                    {
                        if (!string.IsNullOrEmpty(user.UserID))
                        {
                            GMNote2 note = new GMNote2(user, name, bin.Item2);
                            Notifier.Instance.Publish(note);
                            return true;
                        }
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
