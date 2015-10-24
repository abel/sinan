using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Observer;
using Sinan.Util;
using Sinan.FastSocket;

namespace Sinan.GMModule
{
    /// <summary>
    /// 连接的客户端发出的通知 
    /// </summary>
    public class GMNote : Notification
    {
        readonly protected SocketToken m_session;

        #region Constructors
        public GMNote(SocketToken session, string name, IList body)
            : base(name, body)
        {
            m_session = session;
        }
        #endregion

        /// <summary>
        /// 连接的Session
        /// </summary>
        public SocketToken Session
        {
            get { return m_session; }
        }

        /// <summary>
        /// 发送消息给玩家..
        /// </summary>
        /// <param name="command">命令名</param>
        /// <param name="msgs">参数</param>
        public virtual void Call(string command, params object[] msgs)
        {
            //if (m_session != null)
            //{
            //    var buffer = AmfCodec.Encode(command, msgs);
            //    m_session.SendAsync(buffer);
            //}
            if (m_session != null)
            {
                List<object> msg2 = new List<object>(msgs.Length + 1);
                //msg2.Add(ServerLogger.zoneid.ToString());
                msg2.AddRange(msgs);
                var buffer = AmfCodec.Encode(command, msg2);
                m_session.SendAsync(buffer);
            }
        }
    }
}
