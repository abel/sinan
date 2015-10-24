using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Observer;
using Sinan.Log;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 活动命令
    /// </summary>
    sealed public class PartMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[] 
            {
                LoginCommand.PlayerLoginSuccess,
                PartCommand.Aura, 
                PartCommand.TurnAura, 
            };
        }

        public override void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;
            if (note != null && note.Player != null)
            {
                this.ExecuteNote(note);
            }
        }

        void ExecuteNote(UserNote note)
        {
            switch (note.Name)
            {
                case PartCommand.Aura:
                    Aura(note);
                    return;
                case PartCommand.TurnAura:
                    TurnAura(note);
                    return;
                case LoginCommand.PlayerLoginSuccess:
                    PlayerLoginSuccess(note);
                    return;
                default:
                    break;
            }
        }

        private void PlayerLoginSuccess(UserNote note)
        {
            note.Call(PartCommand.PartsR, PartProxy.GetActiveParts());
        }

        /// <summary>
        /// 合成光环
        /// </summary>
        /// <param name="note"></param>
        private void Aura(UserNote note)
        {
            RobBusiness part = PartProxy.TryGetPart(Part.Rob) as RobBusiness;
            if (part != null)
            {
                if (part.CombinAura(note))
                {
                    //成功合成元素的玩家得奖励
                    Int32 exp = (int)(part.TotalExp / 100);
                    PlayerBusiness player = note.Player;
                    player.AddExperience(exp, FinanceType.Part);
                    player.AddPetExp(player.Pet, exp, true, (int)FinanceType.Part);
                    var team = player.Team;
                    if (team == null)
                    {
                        player.AddExperience(exp, FinanceType.Part);
                        player.AddPetExp(player.Pet, exp, true, (int)FinanceType.Part);
                    }
                    else
                    {
                        TeamAddExp(team, exp);
                    }
                }
            }
        }

        /// <summary>
        /// 上缴光环
        /// </summary>
        /// <param name="note"></param>
        private void TurnAura(UserNote note)
        {
            PlayerBusiness player = note.Player;
            RobBusiness part = PartProxy.TryGetPart(Part.Rob) as RobBusiness;
            if (part != null)
            {
                int exp = part.TotalExp;

                string msg = string.Format(TipManager.GetMessage((int)PartMsgType.TurnAura), player.Name);
                if (part.ChangeAuraOwner(player.ID, "system", "system", msg))
                {
                    PlayerTeam team = player.Team;
                    if (team == null)
                    {
                        player.AddExperience(exp, FinanceType.Part);
                        player.AddPetExp(player.Pet, exp, true, (int)FinanceType.Part);
                    }
                    else
                    {
                        exp = part.TotalExp / team.Count;
                        TeamAddExp(team, exp);
                    }
                    //成功
                    part.End();

                    //发送家族的奖励
                    UserNote note2 = new UserNote(player, PartCommand.RobFamilyAward, new object[] { part.FamilyAward });
                    Notifier.Instance.Publish(note2);

                    //var f = player.GetMyFamily();
                    //if (f != null)
                    //{
                    //    //保存家族奖励
                    //    AwardPart award = new AwardPart();
                    //    award.ID = ObjectId.GenerateNewId().ToString();
                    //    award.Name = Part.Rob;
                    //    award.FID = f.ID;
                    //    award.TotalExp = part.TotalExp;
                    //    award.OverTime = DateTime.UtcNow.AddHours(1);
                    //    AwardPartAccess.Instance.Save(award);
                    //}
                    return;
                }
            }
            part.Call(PartCommand.TurnAuraR, new object[] { false });
        }

        private void TeamAddExp(PlayerTeam team, Int32 exp)
        {
            for (int i = 0; i < team.Members.Length; i++)
            {
                PlayerBusiness member = team.Members[i];
                if (member != null && (member.TeamJob == TeamJob.Member || member.TeamJob == TeamJob.Captain))
                {
                    member.AddExperience(exp, FinanceType.Part);
                    member.AddPetExp(member.Pet, exp, true, (int)FinanceType.Part);
                }
            }
        }
    }
}
