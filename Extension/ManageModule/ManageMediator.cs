using System.Collections;
using System.Collections.Generic;
using log4net;
using Sinan.Command;
using Sinan.FrontServer;
using Sinan.Observer;
using Sinan.GameModule;

namespace Sinan.ManageModule
{
    sealed public class ManageMediator : AysnSubscriber
    {
        private static ILog logger = LogManager.GetLogger("GMLog");

        public override IList<string> Topics()
        {
            return new string[] { 
                 GMCommand.Exitall,
                 GMCommand.Restart,
                 GMCommand.Coin,
                 GMCommand.Score,
                 //GMCommand.Bond,
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
                 GMCommand.GMMallInfo,
                 GMCommand.TaskReset
                };
        }

        public override void Execute(INotification notification)
        {
            if (!ServerManager.AdminGM)
            {
                return;
            }
            GMNote note = notification as GMNote;
            if (note == null)
            {
                return;
            }

            IList s = null;
            if (note.Body != null && note.Body.Count >= 2)
            {
                s = note[1] as IList;
            }

            string[] comm;
            System.Text.StringBuilder sb = new System.Text.StringBuilder(128);
            sb.Append("IP:");
            sb.Append(note.Session.IP);
            sb.Append(" UID:");
            sb.Append(note.Session.UserID);
            sb.Append(" CMD:");
            sb.Append(note.Name);
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
            //写日志...
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
                case GMCommand.Exitall:
                    ManageBusiness.ExitAll(note);
                    break;
                case GMCommand.Coin:
                    //充值
                    ManageBusiness.AddCoin(note, comm);
                    break;
                case GMCommand.Score:
                    ManageBusiness.AddScore(note, comm);
                    break;
                //case GMCommand.Bond:
                //    ManageBusiness.AddBond(note, comm);
                //    break;
                case GMCommand.Exp:
                    //添加经验
                    ManageBusiness.Exp(note, comm);
                    break;
                case GMCommand.TaskRemove:
                    //删除某个任务
                    ManageBusiness.TaskRemove(note, comm);
                    break;
                case GMCommand.TaskAct:
                    //重新触发某个任务
                    ManageBusiness.TaskAct(note, comm);
                    break;
                case GMCommand.TaskId:
                    //得到任务ID
                    ManageBusiness.GetTaskID(note, comm);
                    break;
                case GMCommand.Goodsid:
                    //道具ID
                    ManageBusiness.GoodID(note, comm);
                    break;
                case GMCommand.Getgood:
                    //道具赠送
                    ManageBusiness.GetGood(note, comm);
                    break;
                case GMCommand.GoodRemove:
                    //道具移除
                    ManageBusiness.GoodRemove(note, comm);
                    break;
                case GMCommand.PetExp:
                    //添加宠物经验
                    ManageBusiness.PetExp(note, comm);
                    break;
                case GMCommand.Skill:
                    ManageBusiness.Skill(note, comm);
                    break;
                case GMCommand.Pskill:
                    ManageBusiness.PSkill(note, comm);
                    break;
                case GMCommand.Restart:
                    ManageBusiness.ReStart(note, comm);
                    break;
                case GMCommand.SelectEmail:
                    ManageBusiness.SelectEmail(note, comm);
                    break;
                case GMCommand.GMDelEmail:
                    ManageBusiness.GMDelEmail(note, comm);
                    break;
                case GMCommand.GMAuctionList:
                    ManageAuction.GMAuctionList(note, comm);
                    break;
                case GMCommand.GMAuctionDel:
                    ManageAuction.GMAuctionDel(note, comm);
                    break;
                case GMCommand.GMBurdenClear:
                    ManageAuction.GMBurdenClear(note, comm);
                    break;
                case GMCommand.GMMallInfo:
                    ManageAuction.GMMallInfo(note);
                    break;
                case GMCommand.TaskReset:
                    ManageBusiness.TaskReset(note);
                    break;
            }
        }
    }
}
