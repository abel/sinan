using System.Collections.Generic;
using Sinan.Command;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.HomeModule.Business;
using Sinan.Observer;

namespace Sinan.HomeModule
{
    /// <summary>
    /// 家园
    /// </summary>
    sealed public class HomeMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[] 
            { 
                HomeCommand.HomeInfo,
                HomeCommand.HomePetKeep,
                HomeCommand.HomeStopKeep,
                HomeCommand.HomeProduce,
                HomeCommand.HomePluck,
                HomeCommand.HomeProduceList,
                HomeCommand.InPet,
                HomeCommand.StopProduce,
                HomeCommand.Collection,
                HomeCommand.PetRetrieve,
                HomeCommand.HomeStopPluck,
                HomeCommand.PetBack,
                HomeCommand.GetBoard,
                HomeCommand.RemoveBoard,
                LoginCommand.PlayerLoginSuccess
            };
        }

        public override void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;
            if (note == null) return;
            //表示角色正在战斗中，客户端提交信息不进行处理
            if (note.Player.AState == ActionState.Fight)
                return;
            switch (note.Name)
            {
                case HomeCommand.HomeInfo:
                    HomeBusiness.HomeInfo(note);
                    break;
                case HomeCommand.HomePetKeep:
                    HomeBusiness.HomePetKeep(note);
                    break;
                case HomeCommand.HomeStopKeep:
                    HomeBusiness.HomeStopKeep(note);
                    break;
                case HomeCommand.HomeProduce:
                    HomeBusiness.HomeProduce(note);
                    break;
                case HomeCommand.HomePluck:
                    HomeBusiness.HomePluck(note);
                    break;
                case HomeCommand.HomeProduceList:
                    //HomeBusiness.HomeProduceList(note);
                    break;
                case HomeCommand.InPet:
                    HomeBusiness.InPet(note);
                    break;
                case HomeCommand.StopProduce:
                    //HomeBusiness.StopProduce(note);
                    break;
                case HomeCommand.Collection:
                    HomeBusiness.Collection(note);
                    break;
                case HomeCommand.PetRetrieve:
                    HomeBusiness.PetRetrieve(note);
                    break;
                //case HomeCommand.HomeStopPluck:
                //    //HomeBusiness.HomeStopPluck(note);
                //    break;
                case HomeCommand.PetBack:
                    HomeBusiness.PetBack(note);
                    break;
                case HomeCommand.GetBoard:
                    HomeBusiness.GetBoard(note);
                    break;
                case HomeCommand.RemoveBoard:
                    HomeBusiness.RemoveBoard(note);
                    break;
                case LoginCommand.PlayerLoginSuccess:
                    HomeBusiness.HomeInfo(note);
                    break;
            }
        }
    }
}
