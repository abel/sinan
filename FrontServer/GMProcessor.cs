using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Sinan.AMF3;
using Sinan.FastSocket;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 处理GM命令
    /// </summary>
    sealed class GMProcessor : CommandProcessor, ICommandProcessor<Tuple<int, List<object>>>
    {
        public GMProcessor(int capacity)
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
                    //string.Empty(命令已执行或忽略)
                    if (name != string.Empty)
                    {
                        return Execute(user, name, bin.Item2);
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

        bool Execute(UserSession user, string command, List<object> parm)
        {
            if (!string.IsNullOrEmpty(user.UserID))
            {
                GMNote note = new GMNote(user, command, parm);
                Notifier.Instance.Publish(note);
                return true;
            }

            //GM登录命令
            if (command != LoginCommand.UserLogin || parm == null || parm.Count != 2)
            {
                user.Close();
                return false;
            }

            string gmName = parm[0].ToString();
            string pwd = parm[1].ToString();
            if (GMManager.Instance.Login(gmName, pwd))
            {
                user.UserID = parm[0].ToString();
                //登录成功
                user.Call(LoginCommand.UserLoginR, TipManager.GetMessage(ClientReturn.PlayerLogin3));
            }
            return true;
        }
    }
}
