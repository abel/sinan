using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.FrontServer;
using Sinan.FastSocket;

namespace Sinan.GMModule
{
    class GMNote2 : GMNote
    {
        #region Constructors
        public GMNote2(SocketToken session, string name, IList body)
            : base(session, name, body)
        {
        }
        #endregion

        /// <summary>
        /// 发送消息给玩家..
        /// </summary>
        /// <param name="command">命令名</param>
        /// <param name="msgs">参数</param>
        public override void Call(string command, params object[] msgs)
        {
            if (m_session != null)
            {
                List<object> msg2 = new List<object>(msgs.Length + 1);
                msg2.Add(this.Body[0]);
                msg2.AddRange(msgs);
                var buffer = AmfCodec.Encode(command, msg2);
                m_session.SendAsync(buffer);
            }
        }
    }
}
