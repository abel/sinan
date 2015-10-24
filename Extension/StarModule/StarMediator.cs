using System.Collections.Generic;
using Sinan.Command;
using Sinan.FrontServer;
using Sinan.Observer;
using Sinan.StarModule.Business;

namespace Sinan.StarModule
{
    /// <summary>
    /// 星座系统
    /// </summary>
    sealed public class StarMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[] 
            { 
                StarCommand.PlayerMeditation,
                StarCommand.StartStar,
                StarCommand.StartStarShared,
                StarCommand.InStarTroops,
                StarCommand.OutStarTroops,
                StarCommand.GetStarTroops,
                StarCommand.IsStartTroops,
                LoginCommand.PlayerLoginSuccess
            };
        }

        public override void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;
            if (note == null || note.Player == null)
                return;
            //表示角色正在战斗中，客户端提交信息不进行处理
            Execute(note);
        }

        void Execute(UserNote note)
        {
            switch (note.Name)
            {
                case StarCommand.PlayerMeditation:
                    StarBusiness.PlayerMeditation(note);
                    break;
                case StarCommand.StartStar:
                    StarBusiness.StartStar(note);
                    break;
                case StarCommand.StartStarShared:
                    StarBusiness.StartStarShared(note);
                    break;
                case StarCommand.InStarTroops:
                    StarBusiness.InStarTroops(note);
                    break;
                case StarCommand.OutStarTroops:
                    StarBusiness.OutStarTroops(note);
                    break;
                case StarCommand.GetStarTroops:
                    //StarBusiness.GetStarTroops(note);
                    break;
                case StarCommand.IsStartTroops:
                    StarBusiness.IsStartTroops(note);
                    break;
                case LoginCommand.PlayerLoginSuccess:
                    StarBusiness.OfflineTroops(note);
                    break;
            }
        }
    }
}
