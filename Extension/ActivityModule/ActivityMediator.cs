using System;
using System.Collections.Generic;
using Sinan.ActivityModule.Business;
using Sinan.Command;
using Sinan.Core;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Observer;
using System.IO;

namespace Sinan.ActivityModule
{
    sealed public class ActivityMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[] 
            {
               LoginCommand.PlayerLoginSuccess,
               ClientCommand.UserDisconnected,

               ActivityCommand.ActivitySign,
               ActivityCommand.ActivityAward,
               ActivityCommand.CheckActivity,
               ActivityCommand.SignAward,
               ActivityCommand.LoginAward,
               ActivityCommand.VIPDayAward,
               ActivityCommand.VipExchange,
               ActivityCommand.OnlineAward,
               Application.APPSTART
            };
        }

        public override void Execute(INotification notification)
        {
            if (notification.Name == Application.APPSTART)
            {
                string path = Path.Combine(ConfigLoader.Config.DirBase, "Activity.xml");
                ActivityManager.Load(path);
                return;
            }
            UserNote note = notification as UserNote;
            if (note != null && note.Player != null)
            {
                try
                {
                    this.Execute(note);
                }
                catch (Exception ex)
                {
                    LogWrapper.Error(notification.Name, ex);
                }
            }
        }

        void Execute(UserNote note)
        {
            switch (note.Name)
            {
                case LoginCommand.PlayerLoginSuccess:
                //case ActivityCommand.CheckActivity:
                    ActivityBusiness.Activity(note);
                    break;
                case ClientCommand.UserDisconnected:
                    ActivityBusiness.UserDisconnected(note);
                    break;
                case ActivityCommand.ActivitySign:
                    ActivityBusiness.Sign(note);
                    break;
                case ActivityCommand.ActivityAward:
                    ActivityBusiness.ActivityAward(note);
                    break;
                case ActivityCommand.LoginAward:
                    ActivityBusiness.LoginAward(note);
                    break;
                case ActivityCommand.SignAward:
                    ActivityBusiness.SignAward(note);
                    break;
                case ActivityCommand.VIPDayAward:
                    ActivityBusiness.VIPDayAward(note);
                    break;
                case ActivityCommand.VipExchange:
                    ActivityBusiness.VipExchange(note);
                    break;
                case ActivityCommand.OnlineAward:
                    ActivityBusiness.OnlineAward(note);
                    break;
            }
        }
    }
}
