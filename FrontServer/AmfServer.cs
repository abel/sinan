using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Sinan.Command;
using Sinan.Core;
using Sinan.FastSocket;
using Sinan.Util;

namespace Sinan.FrontServer
{
    public class AmfServer : Sinan.FastSocket.AsyncTcpListener
    {
        /// <summary>
        ///  初始化各种处理器
        /// </summary>
        /// <param name="factory">Session工厂</param>
        /// <param name="filter">连接过滤器</param>
        /// <param name="handshake">握手处理器</param>
        public AmfServer(ISessionFactory factory, ISocketFilter filter, IHandshake handshake)
            : base(factory, filter, handshake)
        {
        }

        protected override void StartReceive(Socket client, byte[] buffer, int offset, int count)
        {
            if (count >= 8)
            {
                //检查握手包(8字节)
                int code = buffer[offset + 2] + (buffer[offset + 3] << 8);
                if (code == 99)
                {
                    ISession session = m_sessionFactory.CreateSession(client);
                    if (session != null)
                    {
                        var handR = AmfCodec.Encode(ClientCommand.HandR, new object[] {
                            ServerLogger.serverVer,      //版本号
                            new object[]{session.ConnectID, ServerLogger.serverVer} //用于加密数组
                        });
                        session.SendAsync(handR);
                        session.ReceiveAsync(buffer, offset + 8, count - 8);
                        return;
                    }
                }
            }
            //断开
            client.SafeClose();
        }
    }
}
