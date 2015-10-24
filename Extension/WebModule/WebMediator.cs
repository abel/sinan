using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Observer;
using Sinan.FrontServer;
using Sinan.Command;
using Sinan.WebModule.Business;
using Sinan.Core;

namespace Sinan.WebModule
{
    public class WebMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[] 
            {
                Application.APPSTART,
                WebCommand.CodeAward,
                WebCommand.GetPartList
            };
        }

        public override void Execute(INotification notification)
        {
            if (notification.Name == Application.APPSTART)
            {
                //取得活动列表
                WebBusiness.PartList();
                return;
            }

            

            UserNote note = notification as UserNote;
            if (note != null)
            {
                ExecuteNote(note);
            }

            Notification nf = notification as Notification;
            if (nf != null) 
            {
                if (nf.Name == WebCommand.GetPartList) 
                {
                    WebBusiness.PartList();                    
                    PlayersProxy.CallAll(WebCommand.UpdatePartListR, new object[] { true });
                }
            }
        }

        void ExecuteNote(UserNote note) 
        {
            switch (note.Name)
            {
                case WebCommand.CodeAward:
                    WebBusiness.CodeAward(note);
                    break;                     
            }
        }
    }
}
