using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Command;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 连接的客户端发出的通知 
    /// </summary>
    sealed public class UserNote : Notification
    {
        readonly private UserSession m_session;
        readonly private PlayerBusiness m_player;

        #region Constructors
        public UserNote(UserNote note, string name, object[] body = null)
            : base(name, body)
        {
            m_session = note.Session;
            m_player = note.Player;
        }

        public UserNote(PlayerBusiness player, string name, object[] body = null)
            : base(name, body)
        {
            m_player = player;
            m_session = player.Session;
        }

        public UserNote(UserSession session, string name, IList body = null)
            : base(name, body)
        {
            m_session = session;
            m_player = session.Player;
        }
        #endregion


        /// <summary>
        /// 连接的Session
        /// </summary>
        public UserSession Session
        {
            get { return m_session; }
        }

        /// <summary>
        /// 玩家ID
        /// </summary>
        public string PlayerID
        {
            get { return m_player.ID; }
        }

        public int PID
        {
            get { return m_player.PID; }
        }

        /// <summary>
        /// 玩家数据.
        /// </summary>
        public PlayerBusiness Player
        {
            get { return m_player; }
        }

        /// <summary>
        /// 发送消息给玩家..
        /// </summary>
        /// <param name="command">命令名</param>
        /// <param name="msgs">参数</param>
        public void Call(string command, params object[] msgs)
        {
            UserSession session = m_session ?? (m_player == null ? null : m_player.Session);
            if (session != null)
            {
                session.CallArray(command, msgs);
            }
        }
    }
}
