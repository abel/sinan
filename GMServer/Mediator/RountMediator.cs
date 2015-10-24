using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Observer;
using Sinan.Command;
using System.Collections;

namespace Sinan.GMServer
{
    public class RountMediator : AysnSubscriber
    {
        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                ClientCommand.OpenBox,
                 GMCommand.KickUser,
                 GMCommand.Online,
                 GMCommand.SetPlayerState,
                 GMCommand.ViewPlayer,
                 GMCommand.SetTalk,
                 GMCommand.Notice,
                 GMCommand.NoticeList,
                 GMCommand.UpdateNotice,
                 GMCommand.DoubleExp,
                 GMCommand.Part,
                 
                 GMCommand.GMStart,
                 GMCommand.Exitall,
                 GMCommand.Restart,
                 GMCommand.Coin,
                 GMCommand.Score,
                 GMCommand.Bond,
                 GMCommand.Exp,
                 GMCommand.TaskRemove,
                 GMCommand.TaskAct,
                 GMCommand.TaskId,
                 GMCommand.Goodsid,
                 GMCommand.Getgood,
                 GMCommand.GoodRemove,
                 GMCommand.PetExp,
                 GMCommand.Skill,
                 GMCommand.Pskill,
                 GMCommand.SelectEmail,
                 GMCommand.GMDelEmail,
                 GMCommand.GMAuctionList,
                 GMCommand.GMAuctionDel,
                 GMCommand.GMBurdenClear,
                 GMCommand.GMMallInfo
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

            //取服务器ID,将命令发到游戏服务器
            IList sid = note.Body[0] as IList;
            note.Body[0] = note.Session.UserID;
            foreach (string key in sid)
            {
                //查找服务器:
                //Console.WriteLine("Name:" + note.Name + ",sid:" + key);
                UserSession server;
                if (SessionManager<UserSession>.TryGetValue(key, out server))
                {
                    server.Call(note.Name, note.Body);
                }
                else 
                {
                    Console.WriteLine("没有启动:" + key);
                }
            }
        }
        #endregion
    }
}