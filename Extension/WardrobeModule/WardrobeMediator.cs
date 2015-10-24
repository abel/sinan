using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Observer;
using Sinan.Core;
using Sinan.FrontServer;
using Sinan.Entity;
using Sinan.Command;
using Sinan.WardrobeModule.Bussiness;

namespace Sinan.WardrobeModule
{
    public class WardrobeMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[] 
            {
                WardrobeCommand.MallDressing,
                WardrobeCommand.Dressing,
                WardrobeCommand.NoDressing,
                WardrobeCommand.FashionExchange
            };
        }

        public override void Execute(INotification notification)
        {
            if (notification.Name == Application.APPSTART)
            {
                //加载任务
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
                case WardrobeCommand.MallDressing:
                    WardrobeBusiness.MallDressing(note);
                    break;
                case WardrobeCommand.Dressing:
                    WardrobeBusiness.Dressing(note);
                    break;
                case WardrobeCommand.NoDressing:
                    WardrobeBusiness.NoDressing(note);
                    break;
                case WardrobeCommand.FashionExchange:
                    //WardrobeBusiness.FashionExchange(note);
                    break;
            }
        }
    }
}
