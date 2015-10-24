using System.Collections.Generic;
using Sinan.Command;
using Sinan.DealModule.Business;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.DealModule
{
    sealed public class DealMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[] { 
                DealCommand.DealApply,
                DealCommand.DealApplyBack,   
                DealCommand.LockDeal,
                DealCommand.EnterDeal,
                DealCommand.ExitDeal,
                ClientCommand.UserDisconnected
            };
        }

        public override void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;

            if (note == null || note.Player == null)
                return;

            Execute(note);
        }

        void Execute(UserNote note)
        {
            //表示角色正在战斗中，客户端提交信息不进行处理
            if (note.Player.AState == ActionState.Fight)
            {
                Variant msg = GoodsAccess.Instance.LiaoTianList(TipManager.GetMessage(DealReturn.SelfFighting));
                note.Call(ClientCommand.SendMsgToAllPlayerR, string.Empty, msg);
                return;
            }
            switch (note.Name)
            {
                case DealCommand.DealApply:
                    DealBusiness.DealApply(note);
                    break;
                case DealCommand.DealApplyBack:
                    DealBusiness.DealApplyBack(note);
                    break;
                case DealCommand.LockDeal:
                    DealBusiness.LockDeal(note);
                    break;
                case DealCommand.EnterDeal:
                    DealBusiness.EnterDeal(note);
                    break;
                case DealCommand.ExitDeal:
                case ClientCommand.UserDisconnected:
                    DealBusiness.ExitDeal(note);
                    break;
            }
        }
    }
}
