using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MongoDB.Bson;
using Sinan.AMF3;
using Sinan.Util;
using Sinan.Extensions;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 队伍
    /// </summary>
    public class PlayerTeam : ExternalizableBase
    {
        public static int T = 0;

        /// <summary>
        /// 队伍最多人数.
        /// </summary>
        public const int MaxMembers = 5;

        readonly private string m_id;
        readonly PlayerBusiness[] m_members = new PlayerBusiness[MaxMembers];

        /// <summary>
        /// 创建队伍
        /// </summary>
        /// <param name="player"></param>
        public PlayerTeam(PlayerBusiness player)
        {
            if (player.SetTeam(this, TeamJob.Captain))
            {
                int id = Interlocked.Increment(ref T) & 0x00ffffff;
                this.m_id = Sinan.Extensions.StringFormat.ToHexString(id);
                m_members[0] = player;
            }
        }

        /// <summary>
        /// 允许自动加入
        /// </summary>
        public bool AutoInto
        {
            get;
            set;
        }

        /// <summary>
        /// 屏蔽自动申请
        /// </summary>
        public bool ShieldAutoApply
        {
            get;
            set;
        }

        /// <summary>
        /// 队伍ID
        /// </summary>
        public string TeamID
        {
            get { return m_id; }
        }

        /// <summary>
        /// 未满.可以加入
        /// </summary>
        public bool Available
        {
            get { return m_members.Any(x => x == null); }
        }

        public int Count
        {
            get { return m_members.Count(x => x != null); }
        }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon
        {
            get;
            set;
        }

        /// <summary>
        /// 宣言
        /// </summary>
        public string Explain
        {
            get;
            set;
        }

        /// <summary>
        /// 队长
        /// </summary>
        public PlayerBusiness Captain
        {
            get { return m_members[0]; }
        }

        /// <summary>
        /// 队员
        /// </summary>
        public PlayerBusiness[] Members
        {
            get { return m_members; }
        }

        /// <summary>
        /// 跟随成员
        /// </summary>
        public PlayerBusiness[] Followers
        {
            get { return m_members.Where(x => (x != null && x.TeamJob == TeamJob.Member)).ToArray(); }
        }

        /// <summary>
        /// 队伍的详细信息
        /// </summary>
        public List<PlayerDetail> AllPlayerDetail
        {
            get
            {
                List<PlayerDetail> detail = new List<PlayerDetail>(MaxMembers);
                for (int i = 0; i < m_members.Length; i++)
                {
                    PlayerBusiness member = m_members[i];
                    if (member != null)
                    {
                        detail.Add(new PlayerDetail(member));
                    }
                }
                return detail;
            }
        }

        /// <summary>
        /// 添加成员
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool TryAddMember(PlayerBusiness player)
        {
            if (player.Team != null || player.Fight != null ||
                player.SceneID != Captain.SceneID)
            {
                return false;
            }
            for (int i = 1; i < m_members.Length; i++)
            {
                if (Interlocked.CompareExchange(ref m_members[i], player, null) == null)
                {
                    player.SetTeam(this, TeamJob.Member);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 请离成员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RemoveMember(PlayerBusiness player)
        {
            if (player != null)
            {
                for (int i = 1; i < m_members.Length; i++)
                {
                    if (Interlocked.CompareExchange(ref m_members[i], null, player) == player)
                    {
                        if (player.Team == this)
                        {
                            TeamJob oldJob = player.TeamJob;
                            player.SetTeam(null, TeamJob.NoTeam);
                            if (oldJob == TeamJob.Member)
                            {
                                player.X = Captain.X;
                                player.Y = Captain.Y;
                                player.Point = Captain.Point;
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 请离成员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PlayerBusiness RemoveMember(string memberID)
        {
            for (int i = 1; i < m_members.Length; i++)
            {
                PlayerBusiness player = m_members[i];
                if (player != null && player.ID == memberID)
                {
                    RemoveMember(player);
                    return player;
                }
            }
            return null;
        }

        /// <summary>
        /// 转让队长
        /// </summary>
        /// <param name="newCaptainID">新队长ID(为空是随机选择)</param>
        /// <param name="exit">老队长是否退出</param>
        /// <returns></returns>
        public PlayerBusiness TransferCaptain(string newCaptainID, bool exit = false)
        {
            PlayerBusiness newCaptain;
            PlayerBusiness oldCaptain = Captain;
            for (int i = 1; i < m_members.Length; i++)
            {
                newCaptain = m_members[i];
                if (newCaptain != null && newCaptain.TeamJob == TeamJob.Member
                    && (string.IsNullOrEmpty(newCaptainID) || newCaptain.ID == newCaptainID))
                {
                    if (Interlocked.CompareExchange(ref m_members[i], (exit ? null : oldCaptain), newCaptain) == newCaptain)
                    {
                        newCaptain.X = oldCaptain.X;
                        newCaptain.Y = oldCaptain.Y;
                        newCaptain.Point = oldCaptain.Point;
                        //this.m_id = newCaptain.ID;
                        m_members[0] = newCaptain;
                        newCaptain.SetTeam(this, TeamJob.Captain);
                        if (exit)
                        {
                            oldCaptain.SetTeam(null, TeamJob.NoTeam);
                        }
                        else
                        {
                            m_members[i] = oldCaptain;
                            oldCaptain.SetTeam(this, TeamJob.Member);
                        }
                        return newCaptain;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 解散队伍
        /// </summary>
        /// <returns></returns>
        public List<PlayerBusiness> FreeTeam()
        {
            List<PlayerBusiness> players = new List<PlayerBusiness>(MaxMembers);
            PlayerBusiness captain = Captain;
            for (int i = 0; i < m_members.Length; i++)
            {
                PlayerBusiness member = m_members[i];
                if (member != null)
                {
                    if (member.TeamJob == TeamJob.Member)
                    {
                        member.X = captain.X;
                        member.Y = captain.Y;
                        member.Point = captain.Point;
                    }
                    if (member.SetTeam(null, TeamJob.NoTeam))
                    {
                        players.Add(member);
                    }
                }
            }
            return players;
        }

        /// <summary>
        /// 调用客户端的方法
        /// </summary>
        /// <param name="buffer">已编码的方法调用</param>
        /// <param name="all">暂离队员是否接收</param>
        /// <param name="exclude">排除的队员</param>
        public bool Call(Sinan.Collections.BytesSegment buffer, bool all, PlayerBusiness exclude)
        {
            for (int i = 0; i < m_members.Length; i++)
            {
                PlayerBusiness member = m_members[i];
                if (member != null && member != exclude)
                {
                    if (all || member.TeamJob != TeamJob.Away)
                    {
                        member.Call(buffer);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 发送消息给其它场景上的队员
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool CallAway(Sinan.Collections.BytesSegment buffer, string sceneID)
        {
            for (int i = 0; i < m_members.Length; i++)
            {
                PlayerBusiness member = m_members[i];
                if (member != null && member.SceneID != sceneID)
                {
                    member.Call(buffer);
                }
            }
            return true;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey("ID");
            writer.WriteUTF(m_id);

            var cap = m_members[0];
            writer.WriteKey("Captain");
            writer.WriteUTF(cap.ID);

            writer.WriteKey("Name");
            writer.WriteUTF(cap.Name);

            writer.WriteKey("Level");
            writer.WriteInt(cap.Level);

            writer.WriteKey("Num");
            writer.WriteInt(Count);

            writer.WriteKey("Icon");
            writer.WriteUTF(Icon);

            writer.WriteKey("Explain");
            writer.WriteUTF(Explain);
        }
    }

    /// <summary>
    /// 队伍职位
    /// </summary>
    public enum TeamJob
    {
        /// <summary>
        /// 没有队伍
        /// </summary>
        NoTeam = 0,
        /// <summary>
        /// 暂离队员
        /// </summary>
        Away = 1,
        /// <summary>
        /// 队长
        /// </summary>
        Captain = 2,
        /// <summary>
        /// 在线队员
        /// </summary>
        Member = 3,
    }
}
