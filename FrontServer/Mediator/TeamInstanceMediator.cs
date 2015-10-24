using System.Collections.Generic;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Observer;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 组队副本
    /// </summary>
    sealed public partial class TeamInstanceMediator : AysnSubscriber
    {
        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                InstanceCommand.NewInstance,
                InstanceCommand.FightEctypeApc,
                ClientCommand.UserDisconnected,
            };
        }
        public override void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;
            if (note != null)
            {
                this.ExecuteNote(note);
            }
        }

        void ExecuteNote(UserNote note)
        {
            if (note.Player == null) return;

            switch (note.Name)
            {
                case InstanceCommand.NewInstance:
                    NewInstance(note);
                    return;

                case InstanceCommand.FightEctypeApc:
                    FightEctypeApc(note);
                    return;

                case ClientCommand.UserDisconnected:
                    UserDisconnected(note);
                    return;

                default:
                    return;
            }
        }

        #endregion

        /// <summary>
        /// 创建新副本
        /// </summary>
        /// <param name="note"></param>
        private void NewInstance(UserNote note)
        {
            PlayerBusiness player = note.Player;
            string id = note.GetString(0);

            GameConfig gc = GameConfigAccess.Instance.FindOneById(id);
            if (gc == null)
            {
                return;
            }
            //副本难度
            string difficulty = note.GetString(1);
            if (string.IsNullOrEmpty(difficulty))
            {
                return;
            }

            TeamInstanceBusiness eb;
            switch (gc.SubType)
            {
                case "Team":
                    eb = new TeamInstanceBusiness(gc, difficulty);
                    break;
                case "Personal":
                    eb = new PersonalFam(gc, difficulty);
                    break;
                default:
                    eb = new FamilyInstanceBusiness(gc, difficulty);
                    break;
            }

            if (eb.TryInto(player))
            {
                eb.NextDrame();
            }
        }

        /// <summary>
        /// 跟副本怪战斗
        /// </summary>
        /// <param name="note"></param>
        private void FightEctypeApc(UserNote note)
        {
            PlayerBusiness player = note.Player;
            TeamInstanceBusiness eb = player.TeamInstance;
            if (eb == null || player.AState == ActionState.Fight || (player.TeamJob == TeamJob.Member))
            {
                return;
            }

            int index = note.GetInt32(0);
            if (!eb.CheckBatch(index))
            {
                //提示
                return;
            }

            const FightType ft = FightType.EctypeApc;
            EctypeApc apc = eb.CurrentApcs[index];
            List<PlayerBusiness> players = FightBase.GetPlayers(note.Player);
            FightObject[] teamA = FightBase.CreateFightPlayers(players);
            FightObject[] teamB = FightBase.CreateApcTeam(players, ft, apc.Apc);

            if (teamB.Length == 0 || eb.Astate != ActionState.Standing)
            {
                foreach (var v in players)
                {
                    v.SetActionState(ActionState.Standing);
                }
                return;
            }
            FightBusinessEctype fb = new FightBusinessEctype(teamA, teamB, players, eb, apc);
            foreach (var item in players)
            {
                item.Fight = fb;
            }
            fb.SendToClinet(FightCommand.StartFight, (int)ft, teamA, teamB);
            fb.Start();
        }

        /// <summary>
        /// 检查必杀怪
        /// </summary>
        /// <param name="player"></param>
        /// <param name="npcID"></param>
        /// <returns></returns>
        bool CheckFight(PlayerBusiness player, string npcID)
        {
            return true;
        }

        private void UserDisconnected(UserNote note)
        {
            PlayerBusiness player = note.Player;
            TeamInstanceBusiness tb = player.TeamInstance;
            if (tb != null)
            {
                if (tb.Exit(player) > 0)
                {
                    player.Ectype.Value["EID"] = tb.ID;
                    player.Ectype.Save();
                }
                player.TeamInstance = null;
            }
        }
    }
}
