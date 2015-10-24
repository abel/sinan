using System;
using System.Net;
using System.Net.Sockets;
using Sinan.FastSocket;

#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.GMServer
{
    class SessionFactory : ISessionFactory, ISocketFilter
    {
        //缓冲区大小
        readonly int m_receiveBufferSize;
        readonly int m_sendBufferSize;

        IBufferProcessor m_process;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="receiveBufferSize">每个连接接收数据缓存的大小</param>
        /// <param name="sendBufferSize">每个连发送收数据缓存的大小</param>
        /// <param name="maxWaitSend">等待发送数据的最大业务包</param>
        public SessionFactory(int receiveBufferSize, int sendBufferSize, int maxWaitSend, IBufferProcessor commandReader)
        {
            m_receiveBufferSize = receiveBufferSize;
            m_sendBufferSize = sendBufferSize;
            m_process = commandReader;
        }

        static int m_id = 0;
        #region ISessionFactory的实现
        public ISession CreateSession(Socket socket)
        {
            var receivebuffer = new Sinan.Collections.BytesSegment(new byte[m_receiveBufferSize]);
            var sendbuffer = new Sinan.Collections.BytesSegment(new byte[m_sendBufferSize]);

            // 初始化session数据
            int id = System.Threading.Interlocked.Increment(ref m_id);
            UserSession session = new UserSession(id, receivebuffer, sendbuffer, socket, m_process);
            return session;
        }

        #endregion

        #region  ISocketFilter
        public bool AllowConnect(Socket client)
        {
            return true;
        }

        /// <summary>
        /// 连接后等待接收数据的最大空闲时间(毫秒)
        /// </summary>
        public int IdleMilliseconds
        {
            get { return 0; }
        }
        #endregion
    }

}