using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Observer;
using Sinan.Command;

namespace Sinan.GMServer
{
    public class CallBackMediator : AysnSubscriber
    {
        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                ClientCommand.OpenBox,
            };
        }

        public override void Execute(INotification notification)
        {
            UserNote<UserSession> note = notification as UserNote<UserSession>;
            if (note != null)
            {
                this.ExecuteNote(note);
            }
        }

        void ExecuteNote(UserNote<UserSession> note)
        {
            if (note.Body == null) 
                return;
            //取用户ID,将命令传回给用户
            string sid = note.Body[0].ToString();

            //查找用户:
            UserSession gmClient;
            if (SessionManager<UserSession>.TryGetValue(sid, out gmClient))
            {
                note.Body[0] = note.Session.UserID;
                gmClient.Call(note.Name, note.Body);
            }

        }
        #endregion
    }
}