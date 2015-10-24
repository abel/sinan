using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Sinan.Collections;
using Sinan.Command;
using Sinan.FastSocket;
using Sinan.FrontServer;
using Sinan.Observer;

namespace Sinan.GMServer
{
    public class UserSession : SocketToken
    {
        private int m_zoneid;

        #region 属性
        /// <summary>
        /// 创建连接时间
        /// </summary>
        public DateTime Created
        {
            get;
            set;
        }

        /// <summary>
        /// 应用分区分服时大区的ID. 如果分区分服，
        /// 则该 ID 为新建服务器时自动分配的域名中的serverid 。
        /// </summary>
        public int zoneid
        {
            get { return m_zoneid; }
            set { m_zoneid = value; }
        }

        /// <summary>
        /// 用户权限
        /// </summary>
        public HashSet<int> Permissions
        {
            get;
            set;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">唯一ID</param>
        /// <param name="receiveBuffer">接收缓冲区</param>
        /// <param name="sendBuffer">发送缓冲区</param>
        /// <param name="socket">连接的Socket</param>
        /// <param name="process"></param>
        public UserSession(int id, BytesSegment receiveBuffer, BytesSegment sendBuffer, Socket socket, IBufferProcessor process)
            : base(id, receiveBuffer, sendBuffer, process)
        {
            m_socket = socket;
            m_address = ((System.Net.IPEndPoint)(socket.RemoteEndPoint)).Address;
            Created = DateTime.UtcNow;
        }

        /// <summary>
        /// 调用客户端方法.
        /// </summary>
        /// <param name="name">客户端的方法名</param>
        /// <param name="objs">方法参数数组</param>
        /// <returns></returns>
        public SocketError CallArray(string name, IList objs)
        {
            var buffer = AmfCodec.Encode(name, objs);
            return base.SendAsync(buffer);
        }

        /// <summary>
        /// 调用客户端方法.
        /// </summary>
        /// <param name="name">客户端的方法名</param>
        /// <param name="objs">方法参数数组</param>
        /// <returns></returns>
        public SocketError Call(string name, params object[] objs)
        {
            var buffer = AmfCodec.Encode(name, objs);
            return base.SendAsync(buffer);
        }

        public override void Close()
        {
            try
            {
                //已登录用户断开连接
                if (m_userID != null)
                {
                    if (SessionManager<UserSession>.RemoveUser(m_userID, m_connectID))
                    {
                        UserNote<UserSession> note = new UserNote<UserSession>(this, ClientCommand.UserDisconnected, new object[] { this });
                        Notifier.Instance.Publish(note);
                    }
                }
            }
            finally
            {
                base.Close();
            }
        }

        public override string ToString()
        {
            return ToString(string.Empty);
        }

        public string ToString(string head)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(128);
            sb.Append(head);
            sb.Append("{");
            sb.Append("IP:");
            sb.Append(m_address);
            sb.Append(",UID:");
            sb.Append(m_userID);
            sb.AppendLine("}");
            return sb.ToString();
        }

        public override bool Decode(byte[] buffer, int offset, int count)
        {
            int seed = this.m_connectID;
            //前两字节为包头.表示包长度
            seed = (seed << 3) - seed + buffer[offset];
            seed = (seed << 3) - seed + buffer[offset + 1];
            int max = offset + count;
            for (int i = offset + 2; i < max; i++)
            {
                byte b = buffer[i];
                byte newb = (byte)(b - seed);
                buffer[i] = newb;
                seed = (seed << 3) - seed + newb;
            }
            return true;
        }
    }
}
