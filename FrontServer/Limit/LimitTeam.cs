using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 组队限制
    /// </summary>
    public class LimitTeam : ILimit
    {
        /// <summary>
        /// 在组队状态下不能进入
        /// </summary>
        readonly protected string teamMsg = TipManager.GetMessage(ClientReturn.IntoLimit1);

        /// <summary>
        /// 只允许单人非组队方式进入
        /// </summary>
        protected bool m_onlyOne;

        /// <summary>
        /// 最小成员
        /// </summary>
        protected int m_minMember;

        /// <summary>
        /// 最大成员数
        /// </summary>
        protected int m_maxMember;

        readonly string m_minMemberMsg;

        public LimitTeam(string name, int minMember, int maxMember)
        {
            m_minMember = minMember;
            maxMember = m_maxMember;
            if (minMember <= 1 && maxMember <= 1)
            {
                m_onlyOne = true; //不能组队进入
            }
            else
            {
                m_minMemberMsg = String.Format(TipManager.GetMessage(ClientReturn.EctypeLimitMinMemberMsg), m_minMember);
            }
        }


        public bool Check(PlayerBusiness player, out string msg)
        {
            //组队限制
            var team = player.Team;
            if (m_onlyOne)
            {
                if (team != null)
                {
                    msg = teamMsg;
                    return false;
                }
            }
            else //需组队进入
            {
                int count = 0;
                if (team != null)
                {
                    for (int i = 0; i < team.Members.Length; i++)
                    {
                        PlayerBusiness member = team.Members[i];
                        if (member != null)
                        {
                            if (member.TeamJob == TeamJob.Away)
                            {
                                msg = TipManager.GetMessage(ClientReturn.EctypeLimitTeamMsg);
                                return false;
                            }
                            else
                            {
                                count++;
                            }
                        }
                    }
                }
                if (count < m_minMember || (m_maxMember > m_minMember && count > m_maxMember))
                {
                    msg = m_minMemberMsg;
                    return false;
                }
            }
            msg = null;
            return true;
        }

        public bool Execute(PlayerBusiness player, out string msg)
        {
            msg = null;
            return true;
        }

        public bool Rollback(PlayerBusiness player)
        {
            return true;
        }

        public static LimitTeam Create(string name, Variant v)
        {
            int minMember = v.GetIntOrDefault("MinMember");
            int maxMember = v.GetIntOrDefault("MaxMember");
            if (minMember > 0 || maxMember > 0)
            {
                return new LimitTeam(name, minMember, maxMember);
            }
            return null;
        }
    }
}
