using System.Collections.Generic;
using Sinan.Command;
using Sinan.EmailModule.Business;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.Observer;

namespace Sinan.EmailModule
{
    sealed public class EmailMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[] 
            {
               UserPlayerCommand.CreatePlayerSuccess,
               EmailCommand.EmailList,
               EmailCommand.SendEmail,
               EmailCommand.NewEmailTotal,
               EmailCommand.UpdateEmail,
               EmailCommand.DelEmail,
               EmailCommand.ExtractGoods,
               EmailCommand.ExitEmail,
               PartCommand.RobFamilyAward,
            };
        }

        public override void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;
            // 需验证玩家登录的操作.....
            if (note.Name == UserPlayerCommand.CreatePlayerSuccess)
            {
                EmailBusiness.SystemEmail(note);
                return;
            }
            if (note == null || note.Player == null)
                return;
            Execute(note);
        }

        void Execute(UserNote note)
        {
            //表示角色正在战斗中，客户端提交信息不进行处理
            if (note.Player.AState == ActionState.Fight)
                return;

            switch (note.Name)
            {
                case EmailCommand.EmailList:
                    EmailBusiness.EmailList(note);
                    break;
                case EmailCommand.SendEmail:
                    EmailBusiness.SendEmail(note);
                    break;
                case EmailCommand.NewEmailTotal:
                    EmailBusiness.NewEmailTotal(note);
                    break;
                case EmailCommand.UpdateEmail:
                    EmailBusiness.UpdateEmail(note);
                    break;
                case EmailCommand.DelEmail:
                    EmailBusiness.DelEmail(note);
                    break;
                case EmailCommand.ExtractGoods:
                    //EmailBusiness.ExtractGoods(note);
                    EmailBusiness.GetEmailGoods(note);
                    break;
                case EmailCommand.ExitEmail:
                    EmailBusiness.ExitEmail(note);
                    break;
                case PartCommand.RobFamilyAward:
                    EmailBusiness.RobFamilyAward(note);
                    break;
            }
        }
    }
}
