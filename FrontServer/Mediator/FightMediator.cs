using System.Collections.Generic;
using System.Linq;
using Sinan.Command;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Observer;

namespace Sinan.FrontServer
{
    /// <summary>
    ///  处理战斗行为
    /// </summary>
    sealed public class FightMediator : AysnSubscriber
    {
        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                FightCommand.IntoBattle,
                FightCommand.IntoBattlePK,
                FightCommand.ReadyFight,
                FightCommand.FightAction,
                FightCommand.FightPlayerOver,
                FightCommand.AutoFight,
                ClientCommand.UserDisconnected,
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
            if (note.Name == FightCommand.IntoBattle)
            {
                IntoBattle(note);
            }
            else if (note.Name == FightCommand.IntoBattlePK)
            {
                IntoBattlePK(note);
            }

            FightBase fight = note.Player.Fight;
            if (fight == null) return;
            switch (note.Name)
            {
                default:
                    fight.EnqueueUserNotification(note);
                    return;
            }
        }
        #endregion

        /// <summary>
        /// 进入玩家和怪的战斗
        /// </summary>
        /// <param name="note"></param>
        private void IntoBattle(UserNote note)
        {
            List<PlayerBusiness> players = FightBase.GetPlayers(note.Player);
            FightObject[] teamA = FightBase.CreateFightPlayers(players);
            FightType fType = (FightType)(note.GetInt32(0));
            FightObject[] teamB = FightBase.CreateApcTeam(players, fType, note[1]);

            string npcID = note.GetString(2);

            if (players.Count == 0 || teamB.Length == 0)
            {
                foreach (var v in players)
                {
                    v.SetActionState(ActionState.Standing);
                }
                return;
            }
            bool isEctype = (note.Player.Scene is SceneEctype || note.Player.Scene is SceneSubEctype);

            FightBusiness fb;
            if (fType == FightType.SceneAPC)
            {
                //打明怪
                SceneApc sab = note.GetValue<SceneApc>(3);
                int subID = note.GetInt32(4);
                fb = new FightSceneApc(teamA, teamB, players, npcID, isEctype, sab, subID);
            }
            else if (fType == FightType.RobAPC)
            {
                SceneApc sab = note.GetValue<SceneApc>(3);
                int subID = note.GetInt32(4);
                RobBusiness rb = note.GetValue<RobBusiness>(5);
                fb = new FightBusinessRobApc(teamA, teamB, players, npcID, isEctype, sab, subID, rb);
            }
            else if (fType == FightType.ProAPC)
            {
                //守护战争明怪
                SceneApc sab = note.GetValue<SceneApc>(3);
                int subID = note.GetInt32(4);
                PartBusiness pb = note.GetValue<PartBusiness>(5);
                fb = new FightBusinessProApc(teamA, teamB, players, npcID, isEctype, sab, subID, pb);
            }
            else
            {
                fb = new FightBusiness(teamA, teamB, players, npcID, isEctype);
            }
            foreach (var player in players)
            {
                player.Fight = fb;
            }
            fb.SendToClinet(FightCommand.StartFight, (int)fType, teamA, teamB);
            fb.Start();
        }

        /// <summary>
        /// 进入玩家之间的战斗
        /// </summary>
        /// <param name="note"></param>
        private void IntoBattlePK(UserNote note)
        {
            if (note.Player.Fight != null) return;
            PlayerBusiness playerB = note[1] as PlayerBusiness;
            if (playerB.Fight != null) return;

            List<PlayerBusiness> players = FightBase.GetPlayers(note.Player);
            FightObject[] teamA = FightBase.CreateFightPlayers(players);

            List<PlayerBusiness> playersB = FightBase.GetPlayers(playerB);
            FightObject[] teamB = FightBase.CreateFightPlayers(playersB);

            players.AddRange(playersB);
            FightBase fb = null;
            FightType ft = (FightType)(note.GetInt32(0));
            if (ft == FightType.PK)
            {   //PK
                fb = new FightBusinessPK(teamA, teamB, players);
            }
            else if (ft == FightType.RobPK)
            {
                fb = new FightBusinessRob(teamA, teamB, players);
            }
            else if (ft == FightType.ProPK)
            {
                fb = new FightBusinessPro(teamA, teamB, players);
            }
            else if (ft == FightType.CC)
            {   //切磋
                fb = new FightBusinessCC(teamB, teamA, players);
            }
            else
            {
                return;
            }
            foreach (var player in players)
            {
                player.Fight = fb;
            }
            fb.SendToClinet(FightCommand.StartFight, (int)ft, teamA, teamB);
            fb.Start();
        }



    }
}
