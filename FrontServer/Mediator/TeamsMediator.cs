using System;
using System.Collections.Generic;
using System.Linq;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Observer;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FrontServer
{
    sealed public partial class TeamsMediator : AysnSubscriber
    {
        /// <summary>
        /// 所有队伍(Key:队伍ID,Value: PlayerTeam) 
        /// </summary>
        readonly ConcurrentDictionary<string, PlayerTeam> m_teams = new ConcurrentDictionary<string, PlayerTeam>();

        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                TeamCommand.IntoTeam,
                TeamCommand.QuickApply,
                TeamCommand.ReplyApply,
                TeamCommand.QuickInvite,
                TeamCommand.Invite,
                TeamCommand.ReplyInvite,
                TeamCommand.CreateTeam,
                TeamCommand.GetTeamList,
                TeamCommand.FreeTeam,
                TeamCommand.UpdateTeam,
                TeamCommand.KillMember,
                TeamCommand.OutTeam,
                TeamCommand.AwayTeam,
                TeamCommand.RejoinTeam,
                TeamCommand.ChangeManager,
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
                case TeamCommand.CreateTeam:
                    CreateTeam(note);
                    break;
                case TeamCommand.IntoTeam:
                    IntoTeam(note);
                    break;
                case TeamCommand.GetTeamList:
                    GetTeamList(note);
                    break;
                case TeamCommand.QuickApply:
                    QuickApply(note);
                    break;
                case TeamCommand.Invite:
                    Invite(note);
                    break;
                case TeamCommand.ReplyInvite:
                    ReplyInvite(note);
                    break;
                default:
                    break;
            }

            PlayerTeam team = note.Player.Team;
            if (team == null) return;
            switch (note.Name)
            {
                case TeamCommand.OutTeam:
                    OutTeam(note, team);
                    break;
                case TeamCommand.AwayTeam:
                    AwayTeam(note, team);
                    break;

                case TeamCommand.RejoinTeam:
                    RejoinTeam(note, team);
                    break;
                case ClientCommand.UserDisconnected:
                    UserDisconnected(note, team);
                    return;
                default:
                    break;
            }

            //队长的操作
            if (team.Captain != note.Player) return;
            switch (note.Name)
            {
                case TeamCommand.ReplyApply:
                    ReplyApply(note, team);
                    break;
                case TeamCommand.QuickInvite:
                    QuickInvite(note, team);
                    break;
                case TeamCommand.FreeTeam:
                    FreeTeam(note, team);
                    break;
                case TeamCommand.UpdateTeam:
                    UpdateTeam(note, team);
                    break;
                case TeamCommand.KillMember:
                    KillMember(note, team);
                    break;
                case TeamCommand.ChangeManager:
                    ChangeManager(note, team);
                    break;
                default:
                    break;
            }
        }

        #endregion

        /// <summary>
        /// 创建队伍
        /// </summary>
        /// <param name="note"></param>
        private bool CreateTeam(UserNote note)
        {
            PlayerBusiness player = note.Player;
            if (player.Team != null)
            {
                //已有队伍
                player.Call(TeamCommand.CreateTeamR, false, TipManager.GetMessage(ClientReturn.TeamsMediator1));
                return false;
            }
            PlayerTeam team = new PlayerTeam(player);
            if (team.Captain == player)
            {
                team.Icon = note.GetString(0);
                team.Explain = note.GetString(1);
                if (m_teams.TryAdd(team.TeamID, team))
                {
                    player.CallAll(TeamCommand.CreateTeamR, true, team);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 邀请入队
        /// </summary>
        /// <param name="note"></param>
        /// <param name="team"></param>
        private void Invite(UserNote note)
        {
            PlayerTeam team = note.Player.Team;
            if (team == null)
            {
                //创建?
                return;
            }
            string memberID = note.GetString(0);
            PlayerBusiness member;
            if (note.Player.Scene.TryGetPlayer(memberID, out member))
            {
                if (member.Team == null)
                {
                    member.Call(TeamCommand.InviteR, team);
                }
            }
        }

        /// <summary>
        /// 请求加入队伍
        /// </summary>
        /// <param name="note"></param>
        private void IntoTeam(UserNote note)
        {
            string playerID = note.PlayerID;
            string teamID = note.GetString(0);
            PlayerTeam team;
            if (m_teams.TryGetValue(teamID, out team))
            {
                if (!team.Available)
                {
                    //队伍已满
                    note.Call(TeamCommand.IntoTeamR, false, TipManager.GetMessage(ClientReturn.TeamsMediator2));
                    return;
                }
                if (team.AutoInto)
                {
                    PlayerBusiness member = note.Player;
                    if (team.TryAddMember(member))
                    {
                        var members = team.AllPlayerDetail;
                        note.Call(TeamCommand.IntoTeamR, true, new object[] { team, members });
                        member.CallAllExcludeOne(member, TeamCommand.NewMemberR, teamID, new PlayerDetail(member));
                        return;
                    }
                }
                else
                {
                    //通知队长
                    team.Captain.Call(TeamCommand.ApplyR, playerID, note.Player.GetHashCode());
                }
            }
            //等待队长回复
            note.Call(TeamCommand.IntoTeamR, false, TipManager.GetMessage(ClientReturn.TeamsMediator3));
        }

        /// <summary>
        /// 快速入队请求
        /// </summary>
        /// <param name="note"></param>
        private void QuickApply(UserNote note)
        {
            string playerID = note.PlayerID;
            foreach (var p in note.Player.Scene.Players)
            {
                PlayerTeam team = p.Value.Team;
                if (team != null && team.AutoInto && (!team.ShieldAutoApply))
                {
                    PlayerBusiness player = note.Player;
                    if (team.TryAddMember(player))
                    {
                        var members = team.AllPlayerDetail;
                        note.Call(TeamCommand.IntoTeamR, true, new object[] { team, members });
                        player.CallAllExcludeOne(player, TeamCommand.NewMemberR, team.TeamID, new PlayerDetail(player));
                        return;
                    }
                }
            }
            //发送请求
            int sendCount = 0;
            foreach (var p in note.Player.Scene.Players)
            {
                PlayerTeam team = p.Value.Team;
                if (team != null && team.AutoInto == false && team.Captain == p.Value && (!team.ShieldAutoApply))
                {
                    sendCount++;
                    p.Value.Call(TeamCommand.ApplyR, playerID, note.Player.GetHashCode());
                    if (sendCount > 10) return;
                }
            }
        }

        /// <summary>
        /// 玩家回复是否同意
        /// </summary>
        /// <param name="note"></param>
        private void ReplyInvite(UserNote note)
        {
            string teamID = note.GetString(0);
            bool a = note.GetBoolean(1);
            if (!a) return;
            PlayerTeam team;
            if (m_teams.TryGetValue(teamID, out team))
            {
                if (!team.Available)
                {
                    //队伍已满
                    note.Call(TeamCommand.IntoTeamR, false, TipManager.GetMessage(ClientReturn.TeamsMediator2));
                    return;
                }
                PlayerBusiness member = note.Player;
                if (team.TryAddMember(member))
                {
                    var members = team.AllPlayerDetail;
                    note.Call(TeamCommand.IntoTeamR, true, new object[] { team, members });
                    member.CallAllExcludeOne(member, TeamCommand.NewMemberR, teamID, new PlayerDetail(member));
                }
            }
        }

        private void GetTeamList(UserNote note)
        {
            var lists = m_teams.Values.ToArray();
            List<PlayerTeam> teams = new List<PlayerTeam>();
            foreach (var team in lists)
            {
                PlayerTeam t;
                PlayerBusiness caption = team.Captain;
                if (caption == null)
                {
                    m_teams.TryRemove(team.TeamID, out t);
                }
                else if (caption.SceneID == note.Player.SceneID)
                {
                    teams.Add(team);
                }
            }
            note.Call(TeamCommand.GetTeamListR, teams);
        }
    }
}