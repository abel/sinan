using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Sinan.Util;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FastSocket
{
    public class AsyncTcpListener : AsyncTcpBase
    {
        class StateObject
        {
            public byte[] data = new byte[100];
            public Socket workSocket;
            public DateTime time;
        }

        protected readonly ISessionFactory m_sessionFactory;
        protected readonly ISocketFilter m_filter;
        protected readonly IHandshake m_handshake;

        readonly System.Collections.Concurrent.ConcurrentQueue<StateObject> m_queue;

        public int ListenCount
        {
            get { return m_sockets.Count; }
        }

        /// <summary>
        ///  初始化各种处理器
        /// </summary>
        /// <param name="factory">Session工厂</param>
        /// <param name="filter">连接过滤器</param>
        /// <param name="handshake">握手处理器</param>
        public AsyncTcpListener(ISessionFactory factory, ISocketFilter filter, IHandshake handshake)
        {
            m_sessionFactory = factory;
            m_filter = filter;
            m_handshake = handshake;
            if (m_filter != null && m_filter.IdleMilliseconds > 0)
            {
                m_queue = new System.Collections.Concurrent.ConcurrentQueue<StateObject>();
            }
        }

        /// <summary>
        /// 开始监听
        /// </summary>
        /// <param name="localEndPoint"></param>
        public override void Start(IPEndPoint localEndPoint)
        {
            if (localEndPoint == null) return;

            Socket listener = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            listener.Bind(localEndPoint);
            // 可挂起连接队列的最大长度
            const Int32 m_backlog = 64;
            listener.Listen(m_backlog);
            //执行AcceptAsync所需的缓冲区必须至少为 2 * (sizeof(SOCKADDR_STORAGE) + 16) 字节。
            int acceptSize = 2 * (localEndPoint.Serialize().Size + 0x10);
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);
            e.SetBuffer(new byte[acceptSize], 0, acceptSize);
            e.UserToken = this;
            lock (m_sockets)
            {
                m_sockets.Add(listener);
            }
            StartAccept(listener, e);
        }

        /// <summary>
        /// 接收连接请求.
        /// </summary>
        private void StartAccept(Socket listener, SocketAsyncEventArgs e)
        {
            try
            {
                e.AcceptSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (!listener.AcceptAsync(e))
                {
                    ProcessConnect(listener, e);
                }
            }
            catch { }
        }

        protected override void ProcessConnect(Socket listener, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    Socket client = e.AcceptSocket;
                    //继续监听
                    StartAccept(listener, e);
                    try
                    {
                        if (client.Connected)
                        {
                            if (m_filter == null || m_filter.AllowConnect(client))
                            {
                                SetClient(client);
                                //base.OnConnected(client);
                                if (m_handshake != null)
                                {
                                    //接收第1个包
                                    StateObject state = new StateObject();
                                    state.workSocket = client;
                                    client.BeginReceive(state.data, 0, state.data.Length, SocketFlags.None, ProcessHandler, state);
                                    if (m_queue != null)
                                    {
                                        CheckOverTime();
                                        state.time = DateTime.UtcNow.AddMilliseconds(m_filter.IdleMilliseconds);
                                        m_queue.Enqueue(state);
                                    }
                                }
                                else
                                {
                                    StartReceive(client, EmptyData, 0, 0);
                                }
                            }
                            else
                            {
                                client.SafeClose();
                            }
                        }
                    }
                    catch
                    {
                        client.SafeClose();
                    }
                }
            }
            catch { }
        }

        void ProcessHandler(IAsyncResult iar)
        {
            try
            {
                StateObject state = iar.AsyncState as StateObject;
                Socket client = state.workSocket;
                try
                {
                    SocketError error;
                    int len = client.EndReceive(iar, out error);
                    if (len > 0 && error == SocketError.Success)
                    {
                        //处理握手
                        Sinan.Collections.BytesSegment bin = new Sinan.Collections.BytesSegment(state.data, 0, len);
                        int offset = m_handshake.Execute(client, bin);
                        if (offset >= 0)
                        {
                            int count = len - offset;
                            if (count == 0)
                            {
                                //继续接收
                                client.BeginReceive(state.data, 0, state.data.Length, SocketFlags.None, ProcessHandler, state);
                                return;
                            }
                            else if (count > 0)
                            {
                                state.workSocket = null;
                                StartReceive(client, state.data, offset, count);
                                return;
                            }
                        }
                    }
                }
                catch { }
                client.SafeClose();
            }
            catch { }
        }

        private void CheckOverTime()
        {
            StateObject state;
            while (m_queue.TryPeek(out state))
            {
                Socket client = state.workSocket;
                if (client != null)
                {
                    if (state.time > DateTime.UtcNow)
                    {
                        return;
                    }
                    else
                    {
                        //超时.
                        m_queue.TryDequeue(out state);
                        client.SafeClose();
                    }
                }
                else
                {
                    m_queue.TryDequeue(out state);
                }
            }
        }

        public override void Close()
        {
            base.Close();
            if (m_queue != null)
            {
                StateObject state;
                while (m_queue.TryDequeue(out state)) ;
            }
        }

        /// <summary>
        /// 第1次接收到数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bin"></param>
        protected virtual void StartReceive(Socket client, byte[] buffer, int offset, int count)
        {
            ISession session = m_sessionFactory.CreateSession(client);
            if (session != null)
            {
                session.ReceiveAsync(buffer, offset, count);
            }
            else
            {
                client.SafeClose();
            }
        }

    }

}
