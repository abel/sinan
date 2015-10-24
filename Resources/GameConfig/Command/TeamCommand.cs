using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// 团队命令(38XX)
    /// </summary>
    public class TeamCommand
    {
        /// <summary>
        /// 创建队伍
        /// </summary>
        public const string CreateTeam = "createTeam";
        public const string CreateTeamR = "g.createTeamR";

        //updateTeam(o:Object) 队长更新组队信息，包括：AutoAgree（自动同意） ShieldAutoApply（屏弊） Icon（图标） Explain（宣言）
        public const string UpdateTeam = "updateTeam";
        public const string UpdateTeamR = "g.updateTeamR";

        /// <summary>
        /// 请求入队
        /// </summary>
        public const string IntoTeam = "intoTeam";
        public const string IntoTeamR = "g.intoTeamR";

        /// <summary>
        /// 快速入队请求
        /// </summary>
        public const string QuickApply = "quickApply";

        /// <summary>
        /// 队长返回请求结果
        /// </summary>
        public const string ReplyApply = "replyApply";

        /// <summary>
        /// 快速邀请
        /// </summary>
        public const string QuickInvite = "quickInvite";

        /// <summary>
        /// 邀请
        /// </summary>
        public const string Invite = "invite";

        /// <summary>
        /// 通知被邀请的玩家
        /// </summary>
        public const string InviteR = "g.inviteR";

        /// <summary>
        /// 玩家回复是否同意
        /// </summary>
        public const string ReplyInvite = "replyInvite";

        /// <summary>
        /// 如果队伍不是自动接受玩家申请，通知队长
        /// </summary>
        public const string ApplyR = "g.applyR";

        /// <summary>
        /// 队长同意或自动接受通知所有成员
        /// </summary>
        public const string NewMemberR = "g.newMemberR";

        /// <summary>
        ///移交队长给指定玩家,成功后调用updateTeamR
        /// </summary>
        public const string ChangeManager = "changeManager";
        public const string ChangeManagerR = "g.changeManagerR";

        /// <summary>
        /// 其他人进入队伍
        /// </summary>
        public const string OtherIntoTeamR = "g.otherIntoTeamR";

        /// <summary>
        /// 退出队伍
        /// </summary>
        public const string OutTeam = "outTeam";
        public const string OutTeamR = "g.outTeamR";

        /// <summary>
        /// 暂离队伍
        /// </summary>
        public const string AwayTeam = "awayTeam";
        public const string AwayTeamR = "g.awayTeamR";

        /// <summary>
        /// 归队...
        /// </summary>
        public const string RejoinTeam = "rejoinTeam";
        public const string RejoinTeamR = "g.rejoinTeamR";

        /// <summary>
        /// 将指定的玩家踢出队伍
        /// </summary>
        public const string KillMember = "killMember";

        /// <summary>
        /// 解散队伍
        /// </summary>
        public const string FreeTeam = "freeTeam";
        public const string FreeTeamR = "g.freeTeamR";

        /// <summary>
        /// 取得队伍列表
        /// </summary>
        public const string GetTeamList = "getTeamList";
        public const string GetTeamListR = "g.getTeamListR";

        /// <summary>
        /// 取得自己的队伍信息
        /// </summary>
        public const string GetMyTeam = "getMyTeam";
        public const string GetMyTeamR = "g.getMyTeamR";

    }
}
