using System;
using System.Collections;
using System.Collections.Generic;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 处理单个场景的业务..
    /// </summary>
    partial class SceneBusiness
    {
        #region 发送消息给玩家

        public void CallAll(long showID, string command, IList objs)
        {
            CallAll(showID, AmfCodec.Encode(command, objs));
        }

        public void CallAll(long showID, Sinan.Collections.BytesSegment buffer)
        {
            if (m_showAll)
            {
                foreach (var member in m_players.Values)
                {
                    member.Call(buffer);
                }
            }
            else
            {
                foreach (var member in m_players.Values)
                {
                    if (member.ShowID == showID)
                    {
                        member.Call(buffer);
                    }
                }
            }
        }

        /// <summary>
        /// 除了指定ID的所有玩家
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="command"></param>
        /// <param name="objs"></param>
        public void CallAllExcludeOne(PlayerBusiness exclude, string command, IList objs)
        {
            CallAllExcludeOne(exclude, AmfCodec.Encode(command, objs));
        }

        public void CallAllExcludeOne(PlayerBusiness exclude, Sinan.Collections.BytesSegment buffer)
        {
            if (this.m_showAll)
            {
                foreach (var member in m_players.Values)
                {
                    if (exclude != member)
                    {
                        member.Call(buffer);
                    }
                }
            }
            else
            {
                //副本场景.只发给本场景的组队成员
                long showID = exclude.ShowID;
                foreach (var member in m_players.Values)
                {
                    if (member.ShowID == showID && exclude != member)
                    {
                        member.Call(buffer);
                    }
                }
            }
        }
        #endregion
    }
}
