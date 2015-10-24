using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Observer;
using System.Collections;
using Sinan.FrontServer;
using Sinan.FastSocket;

namespace Sinan.GMServer
{
    /// <summary>
    /// 连接的客户端发出的通知 
    /// </summary>
    sealed public class UserNote<T> : Notification
        where T : class,ISession
    {
        readonly private T m_session;

        #region Constructors
        public UserNote(T session, string name, IList body)
            : base(name, body)
        {
            m_session = session;
        }
        #endregion

        /// <summary>
        /// 连接的Session
        /// </summary>
        public T Session
        {
            get { return m_session; }
        }

        /// <summary>
        /// 发送消息给玩家
        /// </summary>
        /// <param name="command">命令名</param>
        /// <param name="paras">参数</param>
        public void Call(string command, params object[] paras)
        {
            if (m_session != null)
            {
                var buffer = AmfCodec.Encode(command, paras);
                m_session.SendAsync(buffer);
                return;
            }
        }
    }
}
