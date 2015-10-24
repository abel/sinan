using System;
using System.Net;
using System.Net.Sockets;
using Sinan.Command;
using Sinan.FastSocket;
using Sinan.GameModule;
using Sinan.Observer;
using MongoDB.Bson;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FrontServer
{
    /// <summary>
    /// Session管理者
    /// </summary>
    class SessionFactory : ISessionFactory, ISocketFilter
    {
        //缓冲区大小
        readonly int m_receiveBufferSize;
        readonly int m_sendBufferSize;
        readonly byte[] m_receivebuffer;
        readonly byte[] m_sendbuffer;

        IBufferProcessor m_process;

        //接收连接缓存池
        readonly System.Collections.Concurrent.ConcurrentQueue<UserBuffer> m_bufferPool;

        /// <summary>
        /// Key:连接ID,Value:UserSession
        /// </summary>
        readonly ConcurrentDictionary<int, UserSession> m_sessions = new ConcurrentDictionary<int, UserSession>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxClients">允许的最大连接数</param>
        /// <param name="receiveBufferSize">每个连接接收数据缓存的大小</param>
        /// <param name="sendBufferSize">每个连发送收数据缓存的大小</param>
        /// <param name="sendQueueSize">等待发送数据的最大业务包</param>
        public SessionFactory(int maxClients, int receiveBufferSize, int sendBufferSize, int sendQueueSize, IBufferProcessor commandReader)
        {
            m_receiveBufferSize = receiveBufferSize;
            m_sendBufferSize = sendBufferSize;
            m_bufferPool = new System.Collections.Concurrent.ConcurrentQueue<UserBuffer>();
            m_receivebuffer = new byte[maxClients * receiveBufferSize];
            m_sendbuffer = new byte[maxClients * sendBufferSize];
            m_process = commandReader;
            for (int i = 0; i < maxClients; i++)
            {
                var receivebuffer = new Sinan.Collections.BytesSegment(m_receivebuffer, m_receiveBufferSize * i, m_receiveBufferSize);
                var sendbuffer = new Sinan.Collections.BytesSegment(m_sendbuffer, m_sendBufferSize * i, m_sendBufferSize);
                UserBuffer helper = new UserBuffer(i, this, receivebuffer, sendbuffer, sendQueueSize);
                m_bufferPool.Enqueue(helper);
            }
        }

        static int m_id = 0;
        #region ISessionFactory的实现
        public ISession CreateSession(Socket socket)
        {
            // 初始化session数据
            UserBuffer helper;
            if (m_bufferPool.TryDequeue(out helper))
            {
                int id = System.Threading.Interlocked.Increment(ref m_id);
                UserSession session = new UserSession(id, helper, socket, m_process);
                if (m_sessions.TryAdd(id, session))
                {
                    SessionsProxy.Sessions.Enqueue(session);
                    return session;
                }
                else
                {
                    m_bufferPool.Enqueue(helper);
                }
            }
            return null;
        }

        internal void ReoverySession(UserBuffer helper, ISession token)
        {
            UserSession session;
            if (m_sessions.TryRemove(token.ConnectID, out session))
            {
                m_bufferPool.Enqueue(helper);
            }
        }
        #endregion

        #region  ISocketFilter
        public bool AllowConnect(Socket client)
        {
            try
            {
                //可添加单IP最大连接数限制..
                return !BlackListManager.Instance.Contains(client.RemoteEndPoint.ToString());
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// 连接后等待接收数据的最大空闲时间(毫秒)
        /// </summary>
        public int IdleMilliseconds
        {
            get { return 8000; }
        }
        #endregion
    }
}
