using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Observer;
using Sinan.Command;
using Sinan.GameModule;

namespace Sinan.FrontServer
{
    partial class TeamsMediator
    {
        /// <summary>
        /// 离开队伍
        /// </summary>
        /// <param name="note"></param>
        /// <param name="team"></param>
        private void OutTeam(UserNote note, PlayerTeam team)
        {
            PlayerBusiness player = note.Player;
            if (team.RemoveMember(player))
            {
                var buffer = AmfCodec.Encode(TeamCommand.OutTeamR,
                    new object[] { true, team.TeamID, player.ID, player.Name, null });
                if (player.SceneID == team.Captain.SceneID)
                {
                    team.Captain.CallAll(buffer);
                    //通知其它场景上的用户
                    team.CallAway(buffer, team.Captain.SceneID);
                }
                else
                {
                    player.Call(buffer);
                    //通知其它场景上的用户
                    team.CallAway(buffer, player.SceneID);
                }
            }
            var t = player.TeamInstance;
            if (t != null)
            {
                t.Exit(player, true);
            }
        }

        /// <summary>
        /// 暂时离开队伍
        /// </summary>
        /// <param name="note"></param>
        /// <param name="team"></param>
        private void AwayTeam(UserNote note, PlayerTeam team)
        {
            PlayerBusiness player = note.Player;
            if (player.TeamJob == TeamJob.Member && player.SetTeam(team, TeamJob.Away))
            {
                var buffer = AmfCodec.Encode(TeamCommand.AwayTeamR,
                  new object[] { true, team.TeamID, player.ID, player.Name, null });
                player.CallAll(buffer);
                team.CallAway(buffer, team.Captain.SceneID);
            }
            else
            {
                player.Call(TeamCommand.AwayTeamR, false, team.TeamID, player.ID, player.Name,
                    //player.TeamJob == TeamJob.Captain ? "队长不能暂离" : "你已暂离");
                    player.TeamJob == TeamJob.Captain ? TipManager.GetMessage(ClientReturn.AwayTeam1) : TipManager.GetMessage(ClientReturn.AwayTeam2));
            }
        }

        /// <summary>
        /// 归队
        /// </summary>
        /// <param name="note"></param>
        /// <param name="team"></param>
        private void RejoinTeam(UserNote note, PlayerTeam team)
        {
            PlayerBusiness player = note.Player;
            if (player.TeamJob == TeamJob.Away)
            {
                if (player.SceneID != team.Captain.SceneID)
                {
                    //player.Call(TeamCommand.RejoinTeamR, false, team.TeamID, player.ID, player.Name,"您需要和队伍在同一场景中才能归队");
                    player.Call(TeamCommand.RejoinTeamR, false, team.TeamID, player.ID, player.Name, TipManager.GetMessage(ClientReturn.RejoinTeam1));
                    return;
                }
                if (player.SetTeam(team, TeamJob.Member))
                {
                    var buffer = AmfCodec.Encode(TeamCommand.RejoinTeamR,
                                                 new object[] { true, team.TeamID, player.ID, player.Name, null });

                    team.Captain.CallAll(buffer);
                    team.CallAway(buffer, team.Captain.SceneID);
                }
            }
        }

        private void UserDisconnected(UserNote note, PlayerTeam team)
        {
            PlayerBusiness player = note.Player;
            if (player != team.Captain)
            {
                if (team.RemoveMember(player))
                {
                    //TODO:通知队员退出
                    var buffer = AmfCodec.Encode(TeamCommand.AwayTeamR,
                    new object[] { true, team.TeamID, player.ID, player.Name, null });
                    team.CallAway(buffer, player.SceneID);
                }
                return;
            }

            //队长退出
            if (team.Count > 1)
            {
                //转让队伍
                PlayerBusiness newCaptain = team.TransferCaptain(string.Empty, true);
                if (newCaptain != null)
                {
                    var buffer = AmfCodec.Encode(TeamCommand.ChangeManagerR, new object[] { team.TeamID, newCaptain.ID });
                    team.Captain.CallAll(buffer);
                    team.CallAway(buffer, newCaptain.SceneID);
                    return;
                }
            }
            FreeTeam(note, team);
        }
    }
}