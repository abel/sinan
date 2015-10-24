using System.Collections.Generic;
using Sinan.AuctionModule.Business;
using Sinan.Command;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.Observer;

namespace Sinan.AuctionModule
{
    /// <summary>
    /// 拍卖
    /// </summary>
    sealed public class AuctionMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[] { 
                AuctionCommand.AuctionSell,
                AuctionCommand.AuctionBuy,
                AuctionCommand.AuctionBid,
                AuctionCommand.AuctionSellOrBidList,
                AuctionCommand.AuctionBuyList,
            };
        }

        public override void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;
            if (note == null || note.Player == null)
            {
                return;
            }
            Execute(note);
        }

        void Execute(UserNote note)
        {
            //表示角色正在战斗中，客户端提交信息不进行处理
            if (note.Player.AState == ActionState.Fight)
                return;

            switch (note.Name)
            {
                case AuctionCommand.AuctionSell:
                    AuctionBusiness.AuctionSell(note);
                    break;
                case AuctionCommand.AuctionBuy:
                    AuctionBusiness.AuctionBuy(note);
                    break;
                case AuctionCommand.AuctionBid:
                    AuctionBusiness.AuctionBid(note);
                    break;

                case AuctionCommand.AuctionSellOrBidList:
                    AuctionBusiness.AuctionSellOrBidList(note);
                    break;
                case AuctionCommand.AuctionBuyList:
                    AuctionBusiness.AuctionBuyList(note);
                    break;
            }
        }
    }
}
