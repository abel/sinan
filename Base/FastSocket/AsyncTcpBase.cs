using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Sinan.Util;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FastSocket
{
    /// <summary>
    /// 异步Socket基础类
    /// </summary>
    public abstract class AsyncTcpBase : System.IDisposable
    {
        protected static readonly byte[] EmptyData = new byte[0];

        // 基础socket 
        protected readonly HashSet<Socket> m_sockets = new HashSet<Socket>();

        public int Count
        {
            get { return m_sockets.Count; }
        }

        #region 事件定义
        /// <summary>
        /// 客户端连接事件
        /// </summary>
        public event Action<object, Socket> Connected;
        #endregion

        #region Connect
        /// <summary>
        /// 开始连接,可连接多个IP的多个端口.双线很实用.
        /// </summary>
        /// <param name="endPoints"></param>
        public void Start(IList<IPEndPoint> endPoints)
        {
            if (endPoints == null || endPoints.Count == 0)
            {
                return;
            }
            foreach (var p in endPoints)
            {
                Start(p);
            }
        }

        public void Start(string ip, IList ports)
        {
            Start((IList<IPEndPoint>)IPHelper.CreateEndPoints(ip, ports));
        }

        public void Start(string ip, int port)
        {
            System.Net.IPAddress address;
            if (IPAddress.TryParse(ip, out address))
            {
                Start(new IPEndPoint(address, port));
            }
        }
        #endregion

        /// <summary>
        /// 监听所有网络接口上的指定端口.
        /// IPAddress.Any
        /// </summary>
        /// <param name="ports">需要监听的端口集</param>
        public void Start(IList ports)
        {
            Start((IList<IPEndPoint>)(IPHelper.CreateEndPoints(ports)));
        }

        public abstract void Start(IPEndPoint localEndPoint);

        protected abstract void ProcessConnect(Socket socket, SocketAsyncEventArgs e);

        protected void IOCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                Socket socket = sender as Socket;
                ProcessConnect(socket, e);
            }
            catch { }
        }

        /// <summary>
        /// 处理客户端连接事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnConnected(Socket e)
        {
            Action<object, Socket> hander = Connected;
            if (hander != null)
            {
                hander(this, e);
            }
        }

        void IDisposable.Dispose()
        {
            this.Close();
        }

        public virtual void Close()
        {
            //关闭所有端口
            lock (m_sockets)
            {
                foreach (var x in m_sockets)
                {
                    x.SafeClose();
                }
                m_sockets.Clear();
            }
        }


        // KeepAlive的时间 默认为10秒，检查间隔为2秒
        protected static readonly byte[] KeepValue = new byte[] { 1, 0, 0, 0, 0x10, 0x27, 0, 0, 0xd0, 0x07, 0, 0 };
        public static void SetClient(Socket client)
        {
            //client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            //client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, m_receiveBufferSize);
            //client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, m_sendBufferSize);
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
#if mono
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
#else
            client.IOControl(IOControlCode.KeepAliveValues, KeepValue, null);
#endif
        }
    }
}
