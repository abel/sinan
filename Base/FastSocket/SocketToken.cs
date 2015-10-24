using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Net;
using Sinan.Util;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FastSocket
{
    /// <summary>
    /// 连接标识
    /// </summary>
    public class SocketToken : ISession
    {
        /// <summary>
        /// 用于保证同一时间只有1个线程使用SocketAsyncEventArgs发送;
        /// 0:可以发送.1:发送中, 2:已关闭,不能使用. 10
        /// </summary>
        protected int m_socketInUse = 0;
        protected string m_userID;
        protected IPAddress m_address;
        protected Socket m_socket;
        readonly protected int m_connectID;
        readonly protected object m_locker = new object();
        protected Sinan.Collections.CircularQueue<Sinan.Collections.BytesSegment> m_sendPool;

        Sinan.Collections.BytesSegment m_curBuff;
        readonly protected int m_sendOffset;
        readonly protected int m_receiveOffset;
        readonly protected int m_sendMax;
        readonly protected int m_receiveMax;
        readonly SocketAsyncEventArgs m_sender;
        readonly SocketAsyncEventArgs m_receiver;

        /// <summary>
        /// 已写入的等待发送的数据
        /// </summary>
        int m_written = 0;

        /// <summary>
        /// 读取后的剩余数量
        /// </summary>
        int m_remaining = 0;

        /// <summary>
        /// 读取后的剩余数据的偏移位置
        /// </summary>
        int m_remainOffset = 0;

        /// <summary>
        /// 连接的Socket
        /// </summary>
        public Socket Socket
        {
            get { return m_socket; }
        }

        /// <summary>
        /// 连接ID
        /// </summary>
        public int ConnectID
        {
            get { return m_connectID; }
        }

        /// <summary>
        /// IP地址
        /// </summary>
        public IPAddress IP
        {
            get { return m_address; }
        }

        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserID
        {
            get { return m_userID; }
            set { m_userID = value; }
        }

        readonly IBufferProcessor m_processor;

        public SocketToken(int id,
            Sinan.Collections.BytesSegment receiveBuffer,
            Sinan.Collections.BytesSegment sendBuffer,
            IBufferProcessor processor,
            Collections.CircularQueue<Collections.BytesSegment> sendPool = null)
        {
            m_connectID = id;
            m_sendPool = sendPool ?? new Collections.CircularQueue<Collections.BytesSegment>(64);
            m_processor = processor;
            SocketAsyncEventArgs receiver = new SocketAsyncEventArgs();
            receiver.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveCompleted);
            receiver.SetBuffer(receiveBuffer.Array, receiveBuffer.Offset, receiveBuffer.Count);

            SocketAsyncEventArgs sender = new SocketAsyncEventArgs();
            sender.Completed += new EventHandler<SocketAsyncEventArgs>(SendCompleted);
            sender.SetBuffer(sendBuffer.Array, sendBuffer.Offset, sendBuffer.Count);

            m_receiver = receiver;
            m_receiveOffset = receiver.Offset;
            m_receiveMax = receiver.Count;

            m_sender = sender;
            m_sendOffset = sender.Offset;
            m_sendMax = sender.Count;
        }

        #region Completed 事件
        protected void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                ProcessReceive();
            }
            catch { }
        }

        protected void SendCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
#if TraceCall
            TestPrintMethod(m_userID);
#endif
                    if (m_sendPool.IsEmpty)
                    {
                        // 尝试1变0,释放Sender;
                        Interlocked.CompareExchange(ref m_socketInUse, 0, 1);
                    }
                    else if (m_socketInUse == 1)
                    {
                        //继续发送
                        SendData();
                    }
                }
            }
            catch
            {
                if (m_socketInUse < 2)
                {
                    Thread.VolatileWrite(ref m_socketInUse, 2);
                }
            }
        }
        #endregion

        #region Receive
        /// <summary>
        /// 接收到数据的处理函数
        /// </summary>
        protected void ProcessReceive()
        {
            if (m_receiver.BytesTransferred > 0 && m_receiver.SocketError == SocketError.Success)
            {
                try
                {
                    int len = m_receiver.BytesTransferred + m_remaining;
                    ReceiveAsync(len);
                }
                catch
                {
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }

        private void ReceiveAsync(int len)
        {
            Sinan.Collections.BytesSegment data = new Sinan.Collections.BytesSegment(m_receiver.Buffer, m_receiveOffset, len);
            //读取命令
            int remaining = m_processor.Execute(this, data);
            if (m_socket == null || remaining < 0)
            {
                return;
            }
            if (remaining > 0 && remaining < m_receiveMax)
            {
                //根据剩余量计算新的偏移
                m_remainOffset = m_receiveOffset + len - remaining;
                m_remaining = remaining;
                if (m_remainOffset > m_receiveOffset)
                {
                    System.Buffer.BlockCopy(m_receiver.Buffer, m_remainOffset, m_receiver.Buffer, m_receiveOffset, m_remaining);
                }
            }
            else
            {
                m_remaining = 0;
                m_remainOffset = m_receiveOffset;
            }

            //因为缓存大小不变,只要偏移量相同,则数量肯定相同,偏移不同,则数量不同
            int offset = m_remaining + m_receiveOffset;
            if (offset != m_receiver.Offset)
            {
                m_receiver.SetBuffer(offset, m_receiveMax - m_remaining);
            }
            if (m_socket != null)
            {
                if (!m_socket.ReceiveAsync(m_receiver))
                {
                    ProcessReceive();
                }
            }
        }

        /// <summary>
        /// 第1次接收数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public void ReceiveAsync(byte[] buffer, int offset, int count)
        {
            if (buffer != null && count > 0)
            {
                //复制数据..
                System.Buffer.BlockCopy(buffer, offset, m_receiver.Buffer, m_receiveOffset, count);
                ReceiveAsync(count);
            }
            else
            {
                if (m_socket != null)
                {
                    if (!m_socket.ReceiveAsync(m_receiver))
                    {
                        ProcessReceive();
                    }
                }
            }
        }
        #endregion

        #region Send
        /// <summary>
        /// 此方法线程安全.
        /// 同时只会有一个线程调用
        /// </summary>
        private void SendData()
        {
            try
            {
                if (m_curBuff != null)
                {
                    if (m_written + m_curBuff.Count > m_sendMax)
                    {
                        return;
                    }
                    else
                    {
                        Buffer.BlockCopy(m_curBuff.Array, m_curBuff.Offset, m_sender.Buffer, m_written + m_sendOffset, m_curBuff.Count);
                        m_written += m_curBuff.Count;
                    }
                }
                while (m_sendPool.TryDequeue(out m_curBuff))
                {
                    if (m_written + m_curBuff.Count > m_sendMax)
                    {
                        break;
                    }
                    else
                    {
                        Buffer.BlockCopy(m_curBuff.Array, m_curBuff.Offset, m_sender.Buffer, m_written + m_sendOffset, m_curBuff.Count);
                        m_written += m_curBuff.Count;
                    }
                }
                SendAsync();
            }
            catch
            {
                if (m_socketInUse < 2)
                {
                    Thread.VolatileWrite(ref m_socketInUse, 2);
                }
            }
        }

        /// <summary>
        /// 向客户端发送数据(异步方式)
        /// </summary>
        /// <param name="bin"></param>
        /// <returns></returns>
        public SocketError SendAsync(byte[] bin)
        {
            if (bin == null)
            {
                return SocketError.NoData;
            }
            return this.SendAsync(new Sinan.Collections.BytesSegment(bin));
        }

        /// <summary>
        /// 向客户端发送数据(异步方式)
        /// </summary>
        /// <param name="bin"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public SocketError SendAsync(byte[] bin, int offset, int count)
        {
            if (bin == null)
            {
                return SocketError.NoData;
            }
            return this.SendAsync(new Sinan.Collections.BytesSegment(bin, offset, count));
        }

        /// <summary>
        /// 向客户端发送数据(异步方式.)
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public SocketError SendAsync(Sinan.Collections.BytesSegment buffer)
        {
            try
            {
                if (buffer == null)
                {
                    return SocketError.NoData;
                }
                // 尝试0变1,锁定Sender;
                int state = Interlocked.CompareExchange(ref m_socketInUse, 1, 0);
                if (state == 0)
                {
                    if (m_curBuff == null && m_sendPool.IsEmpty)
                    {
                        m_curBuff = buffer;
                    }
                    else
                    {
                        m_sendPool.Enqueue(buffer);
                    }
                    SendData();
                }
                else if (state == 1)
                {
                    m_sendPool.Enqueue(buffer);
                }
                else
                {
                    return SocketError.NotConnected;
                }
                //记日志
#if FlowLog
                FlowLog.AddOut(buffer.Array, buffer.Offset);
#endif
                return SocketError.Success;
            }
            catch
            {
                if (m_socketInUse < 2)
                {
                    Thread.VolatileWrite(ref m_socketInUse, 2);
                }
                return SocketError.NotConnected;
            }
        }
        #endregion

        public virtual void Close()
        {
            if (m_socketInUse < 2)
            {
                Thread.VolatileWrite(ref m_socketInUse, 2);
            }
            Socket client = null;
            lock (m_locker)
            {
                if (m_socket != null)
                {
                    client = m_socket;
                    m_socket = null;
                    m_sendPool = null;
                    m_curBuff = null;
                }
            }
            if (client != null)
            {
                m_written = 0;
                client.SafeClose();
            }
        }

        #region Test  //[Conditional("DEBUG")]
        /// <summary>
        /// 测试用,打印发送给客户的方法名
        /// </summary>
        internal void TestPrintMethod(string userID)
        {
            if (m_sender != null)
            {
                int count = 0;
                int offset = m_sendOffset;
                int maxOffset = m_sender.Offset + m_sender.Count;
                while (offset < maxOffset)
                {
                    int total = m_sender.Buffer[offset] + (m_sender.Buffer[offset + 1] << 8);
                    int command = m_sender.Buffer[offset + 2] + (m_sender.Buffer[offset + 3] << 8);
                    //安全沙箱SandBox
                    if (total == 60 + 99 * 256 && command == 114 + 111 * 256)
                    {
                        return;
                    }
                    if (command > 1000)//不打印tR/walk
                    {
                        Console.WriteLine(string.Format("{0}--{1}-{2} ", userID, command.ToString(), total));
                    }
                    offset += total;
                    count++;
                }
            }
        }
        #endregion

        #region SendAsync
#if mono
        /// <summary>
        /// 异步发送已写入的数据
        /// </summary>
        /// <returns></returns>
        void SendAsync()
        {
            m_sender.SetBuffer(m_sendOffset, m_written);
            m_written = 0;
            if (m_socketInUse == 1)
            {
                //mono始终返回true
                m_socket.SendAsync(m_sender);
            }
        }
        #region monoOld
        ///// <summary>
        ///// 异步发送已写入的数据
        ///// </summary>
        //void SendAsync()
        //{
        //    m_sender.SetBuffer(m_sendOffset, m_written);
        //    m_written = 0;
        //    queue.Enqueue(this);
        //}

        //static System.Threading.Thread thread;
        //static SocketToken()
        //{
        //    thread = new Thread(SendAsyncMono);
        //    thread.Start();
        //}

        //static System.Collections.Concurrent.ConcurrentQueue<SocketToken> queue = new System.Collections.Concurrent.ConcurrentQueue<SocketToken>();
        //static void SendAsyncMono()
        //{
        //    System.Threading.Thread.Sleep(1000);
        //    while (true)
        //    {
        //        SocketToken token;
        //        while (queue.TryDequeue(out token))
        //        {
        //            try
        //            {
        //                Socket socket = token.m_socket;
        //                if (socket != null && socket.Connected)
        //                {
        //                    //mono始终返回true;
        //                    socket.SendAsync(token.m_sender);
        //                    //if (!socket.SendAsync(token.m_sender))
        //                    //{
        //                    //    token.SendCompleted(socket, token.m_sender);
        //                    //}
        //                }
        //            }
        //            catch { }
        //        }
        //        System.Threading.Thread.Sleep(1);
        //    }
        //}
        #endregion
#else
        /// <summary>
        /// 异步发送已写入的数据
        /// </summary>
        /// <returns></returns>
        void SendAsync()
        {
            m_sender.SetBuffer(m_sendOffset, m_written);
            m_written = 0;
            if (m_socketInUse == 1)
            {
                if (!m_socket.SendAsync(m_sender))
                {
                    SendCompleted(m_socket, m_sender);
                }
            }
        }
#endif
        #endregion



        public virtual bool Decode(byte[] buffer, int offset, int count)
        {
            return true;
        } 

    }
}
