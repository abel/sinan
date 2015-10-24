using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.FrontServer
{
    partial class TeamsMediator
    {
        /// <summary>
        /// 请出队伍
        /// </summary>
        /// <param name="note"></param>
        /// <param name="team"></param>
        private void KillMember(UserNote note, PlayerTeam team)
        {
            string memberID = note.GetString(0);
            PlayerBusiness memeber = team.RemoveMember(memberID);
            if (memeber != null)
            {
                var buffer = AmfCodec.Encode(TeamCommand.OutTeamR,
                    new object[] { true, team.TeamID, memberID, memeber.Name, null });

                team.Captain.CallAll(buffer);
                //通知其它场景上的用户
                team.CallAway(buffer, note.Player.SceneID);

                var t = memeber.TeamInstance;
                if (t != null)
                {
                    t.Exit(memeber, true);
                }
            }
        }

        /// <summary>
        /// 队长返回请求结果
        /// </summary>
        /// <param name="note"></param>
        private void ReplyApply(UserNote note, PlayerTeam team)
        {
            bool reply = note.GetBoolean(0);
            string memberID = note.GetString(1);
            int check = note.GetInt32(2);
            PlayerBusiness member;
            if (note.Player.Scene.TryGetPlayer(memberID, out member))
            {
                if (member.GetHashCode() == check)
                {
                    if (reply)
                    {
                        if (team.TryAddMember(member))
                        {
                            var members = team.AllPlayerDetail;
                            member.Call(TeamCommand.IntoTeamR, true, new object[] { team, members });
                            note.Player.CallAllExcludeOne(member, TeamCommand.NewMemberR, team.TeamID, new PlayerDetail(member));
                            return;
                        }
                    }
                    else //不同意
                    {
                        //对方拒绝了你的入队请求
                        member.Call(TeamCommand.IntoTeamR, false, TipManager.GetMessage(ClientReturn.ReplyApply1));
                    }
                }
            }
        }

        /// <summary>
        /// 解散队伍
        /// </summary>
        /// <param name="note"></param>
        private void FreeTeam(UserNote note, PlayerTeam team)
        {
            if (m_teams.TryRemove(team.TeamID, out team))
            {
                var t = note.Player.TeamInstance;
                if (t != null)
                {
                    t.Over();
                }
                team.FreeTeam();
                var buffer = AmfCodec.Encode(TeamCommand.FreeTeamR, new object[] { team.TeamID });
                note.Player.CallAll(buffer);
                team.CallAway(buffer, note.Player.SceneID);
            }
        }

        /// <summary>
        /// 移交队长
        /// </summary>
        /// <param name="note"></param>
        /// <param name="team"></param>
        private void ChangeManager(UserNote note, PlayerTeam team)
        {
            string teamID = team.TeamID;
            string newCaptainID = note.GetString(0);
            PlayerBusiness newCaptain = team.TransferCaptain(newCaptainID);
            if (newCaptain != null)
            {
                //通知
                var buffer = AmfCodec.Encode(TeamCommand.ChangeManagerR, new object[] { teamID, newCaptainID });
                team.Captain.CallAll(buffer);
                team.CallAway(buffer, note.Player.SceneID);
            }
        }

        /// <summary>
        /// 更新队伍信息
        /// </summary>
        /// <param name="note"></param>
        /// <param name="team"></param>
        private void UpdateTeam(UserNote note, PlayerTeam team)
        {
            Variant v = note.GetVariant(0);
            team.AutoInto = v.GetBooleanOrDefault("AutoAgree");
            team.ShieldAutoApply = v.GetBooleanOrDefault("ShieldAutoApply");
            team.Explain = v.GetStringOrDefault("Explain");
            team.Icon = v.GetStringOrDefault("Icon");
            var buffer = AmfCodec.Encode(TeamCommand.UpdateTeamR, new object[] { team });
            note.Player.CallAll(buffer);
            team.CallAway(buffer, note.Player.SceneID);
        }

        /// <summary>
        /// 快速邀请
        /// </summary>
        /// <param name="note"></param>
        /// <param name="team"></param>
        private void QuickInvite(UserNote note, PlayerTeam team)
        {
            int sendCount = 5;
            foreach (var item in note.Player.Scene.Players)
            {
                if (sendCount <= 0) return;
                {
                    if (item.Value.Team == null)
                    {
                        sendCount--;
                        item.Value.Call(TeamCommand.InviteR, team);
                    }
                }
            }
        }
    }
}