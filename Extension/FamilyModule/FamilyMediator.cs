using System.Collections.Generic;
using Sinan.Command;
using Sinan.FamilyModule.Business;
using Sinan.FrontServer;
using Sinan.Observer;

namespace Sinan.FamilyModule
{
    sealed public class FamilyMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[] {
                LoginCommand.PlayerLoginSuccess,
                FamilyCommand.FamilyList,
                FamilyCommand.FamilyMembers,
                FamilyCommand.FamilyCreate,
                FamilyCommand.AppointedNegative,
                FamilyCommand.DissolveFamily,
                FamilyCommand.ExitFamily,
                FamilyCommand.FamilyApply,
                FamilyCommand.FamilyFire,
                FamilyCommand.FamilyInvite,
                FamilyCommand.FamilyInviteBack,
                FamilyCommand.UpdateFamilyNotice,
                FamilyCommand.FireNegative,
                FamilyCommand.FamilyApplyBack,
                FamilyCommand.TransferBoss,
                FamilyCommand.FamilyExperience,
                FamilyCommand.FamilySkill,
                FamilyCommand.StudyFamilySkill,
                FamilyCommand.BossList,
                FamilyCommand.SummonBoss,
                FamilyCommand.FightBoss,
                FamilyCommand.BossAward,
                UserPlayerCommand.DeletePlayerSuccess
            };
        }

        public override void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;

            if (note == null || note.Player == null)
            {
                switch (notification.Name)
                {
                    case FamilyCommand.FamilyExperience:
                        FamilyBusiness.FamilyExperience(notification);
                        break;
                }
                return;
            }
            Execute(note);
        }

        void Execute(UserNote note)
        {
            //表示角色正在战斗中，客户端提交信息不进行处理
            //if (note.Player.AState == ActionState.Fight && note.Type == UserNote.FromClient)
            //    return;
            switch (note.Name)
            {

                case LoginCommand.PlayerLoginSuccess:
                    FamilyBusiness.LoginSuccess(note);
                    break;

                case FamilyCommand.FamilyList:
                    FamilyBusiness.FamilyList(note);
                    break;
                case FamilyCommand.FamilyMembers:
                    FamilyBusiness.FamilyMembers(note);
                    break;
                case FamilyCommand.FamilyCreate:
                    FamilyBusiness.FamilyCreate(note);
                    break;
                case FamilyCommand.AppointedNegative:
                    FamilyBusiness.AppointedNegative(note);
                    break;
                case FamilyCommand.DissolveFamily:
                    FamilyBusiness.DissolveFamily(note);
                    break;
                case FamilyCommand.ExitFamily:
                    FamilyBusiness.ExitFamily(note);
                    break;
                case FamilyCommand.FamilyApply:
                    FamilyBusiness.FamilyApply(note);
                    break;
                case FamilyCommand.FamilyFire:
                    FamilyBusiness.FamilyFire(note);
                    break;
                case FamilyCommand.FamilyInvite:
                    FamilyBusiness.FamilyInvite(note);
                    break;
                case FamilyCommand.FamilyInviteBack:
                    FamilyBusiness.FamilyInviteBack(note);
                    break;
                case FamilyCommand.UpdateFamilyNotice:
                    FamilyBusiness.UpdateFamilyNotice(note);
                    break;
                case FamilyCommand.FireNegative:
                    FamilyBusiness.FireNegative(note);
                    break;
                case FamilyCommand.FamilyApplyBack:
                    FamilyBusiness.FamilyApplyBack(note);
                    break;
                case FamilyCommand.TransferBoss:
                    FamilyBusiness.TransferBoss(note);
                    break;
                case FamilyCommand.FamilySkill:
                    FamilyBusiness.FamilySkill(note);
                    break;
                case FamilyCommand.StudyFamilySkill:
                    FamilyBusiness.StudyFamilySkill(note);
                    break;

                case UserPlayerCommand.DeletePlayerSuccess:
                    FamilyBusiness.DeletePlayerSuccess(note);
                    break;

                case FamilyCommand.BossList:
                    FamilyBossBusiness.BossList(note);
                    break;

                case FamilyCommand.SummonBoss:
                    FamilyBossBusiness.SummonBoss(note);
                    break;

                case FamilyCommand.FightBoss:
                    FamilyBossBusiness.FightBoss(note);
                    break;

                case FamilyCommand.BossAward:
                    FamilyBossBusiness.BossAward(note);
                    break;
            }
        }
    }
}
