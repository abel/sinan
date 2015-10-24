using System.Collections.Generic;
using Sinan.Command;
using Sinan.FrontServer;
using Sinan.Observer;
using Sinan.SocialModule.Business;

namespace Sinan.SocialModule
{
    sealed public class SocialMediator : AysnSubscriber
    {
        /// <summary>
        /// 更
        /// </summary>
        /// <returns></returns>
        public override IList<string> Topics()
        {
            return new string[] {
                SocialCommand.SocialList,

                SocialCommand.AddFriends,
                SocialCommand.AddEnemy,

                SocialCommand.MasterApply,
                SocialCommand.MasterBack,

                SocialCommand.ApprenticeApply,
                SocialCommand.ApprenticeBack,
                
                SocialCommand.DelFriends,
                SocialCommand.DelMaster,
                SocialCommand.DelEnemy,

                TaskCommand.PlayerActivation,

                SocialCommand.FriendsApply,
                SocialCommand.FriendsBack,
                SocialCommand.ConsumeCoin,
                SocialCommand.SummonMaster,
                SocialCommand.ReplySummon,
                SocialCommand.FriendsBless,
                SocialCommand.InvitedFriends,
                SocialCommand.FriendShare,
                UserPlayerCommand.DeletePlayerSuccess
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
            //if (note.Player.AState == ActionState.Fight && note.Type == UserNote.FromClient)
            //    return;
            switch (note.Name)
            {
                case SocialCommand.SocialList:
                    SocialBusiness.SocialList(note);
                    break;

                case SocialCommand.AddFriends:
                    //FriendsBusiness.AddFriends(note);
                    break;
                case SocialCommand.AddEnemy:
                    EnemyBusiness.AddEnemy(note);
                    break;

                case SocialCommand.MasterApply:
                    MasterBusiness.MasterApply(note);
                    break;
                case SocialCommand.MasterBack:
                    MasterBusiness.MasterBack(note);
                    break;

                case SocialCommand.ApprenticeApply:
                    //ApprenticeBusiness.ApprenticeApply(note);
                    break;
                case SocialCommand.ApprenticeBack:
                    //ApprenticeBusiness.ApprenticeBack(note);
                    break;



                case SocialCommand.DelFriends:
                    FriendsBusiness.DelFriends(note);
                    break;
                case SocialCommand.DelMaster:
                    MasterBusiness.DelMaster(note);
                    break;
                case SocialCommand.DelEnemy:
                    EnemyBusiness.DelEnemy(note);
                    break;
          
                case TaskCommand.PlayerActivation:
                    MasterBusiness.UpMaster(note);
                    break;

                case SocialCommand.FriendsApply:
                    FriendsBusiness.FriendsApply(note);
                    break;
                case SocialCommand.FriendsBack:
                    FriendsBusiness.FriendsBack(note);
                    break;

                case SocialCommand.ConsumeCoin:
                    MasterBusiness.ConsumeCoin(note);
                    break;
                case SocialCommand.SummonMaster:
                    MasterBusiness.SummonMaster(note);
                    break;
                case SocialCommand.ReplySummon:
                    MasterBusiness.ReplySummon(note);
                    break;
                case SocialCommand.FriendsBless:
                    FriendsBusiness.FriendsBless(note);
                    break;
                case SocialCommand.InvitedFriends:
                    FriendsBusiness.InvitedFriends(note);
                    break;
                case SocialCommand.FriendShare:
                    FriendsBusiness.FriendShare(note);
                    break;
                case UserPlayerCommand.DeletePlayerSuccess:
                    SocialBusiness.DeletePlayerSuccess(note);
                    break;
            }
        }
    }
}
