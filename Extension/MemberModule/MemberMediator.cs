using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Observer;
using Sinan.Core;
using Sinan.FrontServer;
using Sinan.Command;
using Sinan.MemberModule.Bussiness;

namespace Sinan.MemberModule
{
    public class MemberMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[] 
            {
                LoginCommand.PlayerLoginSuccess,
                PartCommand.Recharge,
                MemberCommand.MemberUp,
                Application.APPSTART
            };
        }
        public override void Execute(INotification notification)
        {
            if (notification.Name == Application.APPSTART)
            {                
                return;
            }

            UserNote note = notification as UserNote;
            if (note != null)
            {
                ExecuteNote(note);
            }
        }

        void ExecuteNote(UserNote note)
        {
            switch (note.Name) 
            {
                case LoginCommand.PlayerLoginSuccess:
                    MemberBussiness.LoginCZD(note);
                    break;
                case PartCommand.Recharge:
                    MemberBussiness.MemberAdd(note);
                    break;
                case MemberCommand.MemberUp:
                    MemberBussiness.MemberUp(note);
                    break;
            }
        }
    }
}
