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
    /// <summary>
    /// 边城浪  QQ：19201576
    /// 最后更新：14:57 2012/4/4
    /// TCP连接客户端,可连接多个服务器端口
    /// </summary>
    public class AsyncTcpClient : AsyncTcpBase
    {
        /// <summary>
        /// 客户端连接成功事件
        /// </summary>
        public event Action<object, SocketAsyncEventArgs> ConnecteSuccess;

        /// <summary>
        /// 客户端连接失败事件
        /// </summary>
        public event Action<object, SocketAsyncEventArgs> ConnecteFail;

        /// <summary>
        /// 开始连接
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        public override void Start(IPEndPoint remoteEndPoint)
        {
            if (remoteEndPoint == null)
            {
                return;
            }
            Socket client = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);
            e.RemoteEndPoint = remoteEndPoint;
            e.SetBuffer(EmptyData, 0, 0);

            bool willRaiseEvent = client.ConnectAsync(e);
            if (!willRaiseEvent)
            {
                ProcessConnect(client, e);
            }
        }

        /// <summary>
        /// 连接事件
        /// </summary>
        /// <param name="e"></param>
        protected override void ProcessConnect(Socket client, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                lock (m_sockets)
                {
                    m_sockets.Add(client);
                }
                try
                {
                    var hander = ConnecteSuccess;
                    if (hander != null)
                    {
                        hander(this, e);
                    }
                    OnConnected(client);
                }
                catch
                {
                    lock (m_sockets)
                    {
                        m_sockets.Remove(client);
                    }
                    client.SafeClose();
                }
            }
            else
            {
                var hander = ConnecteFail;
                if (hander != null)
                {
                    hander(this, e);
                }
            }
        }

    }
}

