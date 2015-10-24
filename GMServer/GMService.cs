using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Sinan.Collections;
using Sinan.Extensions;
using Sinan.FastSocket;
using Sinan.FrontServer;
using Sinan.Util;

namespace Sinan.GMServer
{
    public class GMService : Sinan.FastSocket.AsyncTcpListener
    {
        static string serverVer;
        static GMService()
        {
            //得到程序集的版本号.
            Assembly assem = Assembly.GetExecutingAssembly();
            Version v = assem.GetName().Version;
            serverVer = v.ToString();
        }

        /// <summary>
        ///  初始化各种处理器
        /// </summary>
        /// <param name="factory">Session工厂</param>
        /// <param name="filter">连接过滤器</param>
        /// <param name="handshake">握手处理器</param>
        public GMService(ISessionFactory factory, ISocketFilter filter, IHandshake handshake)
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
                        var handR = AmfCodec.Encode("handR", new object[] {
                            serverVer,      //版本号
                            new object[]{session.ConnectID, serverVer} //用于加密数组
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
