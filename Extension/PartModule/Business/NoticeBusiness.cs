using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Command;
using Sinan.FrontServer;

namespace Sinan.PartModule.Business
{
    class NoticeBusiness
    {
        public static void NoticeCall()
        {
            List<Notice> list = NoticeAccess.Instance.GetNotices();
            DateTime dt = DateTime.UtcNow;
            foreach (Notice model in list)
            {
                if (model.Status != 0)
                {
                    continue;
                }

                if (model.StartTime > (model.StartTime.Kind == DateTimeKind.Utc ? dt : DateTime.Now))
                {
                    continue;
                }

                if (model.EndTime < (model.EndTime.Kind == DateTimeKind.Utc ? dt : DateTime.Now))
                {
                    continue;
                }

                model.Cur += 5;

                if (model.Cur >= model.Rate)
                {
                    PlayersProxy.CallAll(ClientCommand.SendActivtyR, new object[] { model.Place, model.Content, model.Count });                    
                    model.Cur = 0;
                }
            }
        }
    }
}
