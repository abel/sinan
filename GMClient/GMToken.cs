using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Sinan.FastSocket;
using Sinan.FrontServer;

namespace Sinan.GMModule
{
    public class GMToken : SocketToken
    {
        /// <summary>
        /// 创建连接时间
        /// </summary>
        public DateTime Created
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">唯一ID</param>
        /// <param name="receiveBuffer">接收缓冲区</param>
        /// <param name="sendBuffer">发送缓冲区</param>
        /// <param name="socket">连接的Socket</param>
        /// <param name="process"></param>
        public GMToken(int id,
            Sinan.Collections.BytesSegment receiveBuffer,
            Sinan.Collections.BytesSegment sendBuffer,
            Socket socket,
            IBufferProcessor process)
            : base(id, receiveBuffer, sendBuffer, process, null)
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
    }
}
