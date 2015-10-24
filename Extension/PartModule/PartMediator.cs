using System.Collections.Generic;
using Sinan.Command;
using Sinan.FrontServer;
using Sinan.Observer;
using Sinan.PartModule.Business;
using Sinan.Core;
using Sinan.GameModule;

namespace Sinan.PartModule
{
    sealed public class PartMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[] 
            { 
                PartCommand.PartExchange,
                PartCommand.Recharge,
                Application.APPSTART,
                //PartCommand.PartDetails,
                PartCommand.PartReceive
            };
        }

        public override void Execute(INotification notification)
        {
            if (notification.Name == Application.APPSTART)
            {
                NoticeAccess.Instance.LoadNotices();
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
            if (note.Player == null)
                return;
            switch (note.Name)
            {
                case PartCommand.PartExchange:
                    PartInfo.PartExchange(note);
                    break;
                case PartCommand.Recharge:
                    PartInfo.Recharge(note);
                    break;
                case PartCommand.PartReceive:
                    PartInfo.PartReceive(note);
                    break;
            }
        }
    }
}
