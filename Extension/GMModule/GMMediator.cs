using System.Collections;
using System.Collections.Generic;
using log4net;
using Sinan.Command;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.GMModule.Business;
using Sinan.Observer;

namespace Sinan.GMModule
{
    sealed public class GMMediator : AysnSubscriber
    {
        private static ILog logger = LogManager.GetLogger("GMLog");

        public override IList<string> Topics()
        {
            return new string[] { 
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
                 GMCommand.Bond,
                 GMCommand.GMStart,
            };
        }

        public override void Execute(INotification notification)
        {
            GMNote note = notification as GMNote;
            if (note == null)
            {
                if (notification.Name == GMCommand.GMStart)
                {
                    StartGM();
                }
                return;
            }

            IList s = null;
            if (note.Body != null && note.Body.Count >= 2)
            {
                s = note[1] as IList;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder(128);
            sb.Append("IP:");
            sb.Append(note.Session.IP);
            sb.Append(" UID:");
            sb.Append(note.Session.UserID);
            sb.Append(" CMD:");
            sb.Append(note.Name);

            string[] comm;
            if (s == null)
            {
                comm = new string[0];
            }
            else
            {
                comm = new string[s.Count];
                for (int i = 0; i < s.Count; i++)
                {
                    sb.Append(",");
                    comm[i] = s[i].ToString();
                    sb.Append(comm[i]);
                }
            }

            //写日志
            if (note.Name != GMCommand.Online)
            {
                logger.Info(sb.ToString());
            }
            GMInstruction(note, comm);
        }

        /// <summary>
        /// GM基本指令
        /// </summary>
        /// <param name="note"></param>
        private void GMInstruction(GMNote note, string[] comm)
        {
            switch (note.Name)
            {
                case GMCommand.KickUser:
                    GMBusiness.KickUser(note, comm);
                    break;

                case GMCommand.Online:
                    GMBusiness.Online(note, comm);
                    break;

                case GMCommand.SetPlayerState:
                    GMBusiness.SetPlayerState(note, comm);
                    break;

                case GMCommand.ViewPlayer:
                    GMBusiness.ViewPlayer(note, comm);
                    break;

                case GMCommand.SetTalk:
                    GMBusiness.SetTalk(note, comm);
                    break;
                case GMCommand.DoubleExp:
                    GMBusiness.DoubleExp(note, comm);
                    break;

                case GMCommand.Part:
                    GMBusiness.OpenPart(note, comm);
                    break;

                case GMCommand.Bond:
                    GMBusiness.AddBond(note, comm);
                    break;

                case GMCommand.Notice:
                    GMBusiness.Notice(note);
                    break;
                case GMCommand.NoticeList:
                    GMBusiness.NoticeList(note);
                    break;
                case GMCommand.UpdateNotice:
                    GMBusiness.UpdateNotice(note);
                    break;
            }
        }

        private static void StartGM()
        {
            //GMClient client = new GMClient();
            //client.Start(ConfigLoader.Config.EpGMM);
        }
    }
}
