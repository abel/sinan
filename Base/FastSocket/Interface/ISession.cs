using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Sinan.FastSocket
{
    /// <summary>
    /// 客户端连接
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// 连接唯一ID
        /// </summary>
        int ConnectID { get; }

        /// <summary>
        /// 用户唯一标识
        /// </summary>
        string UserID { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        IPAddress IP { get; }

        SocketError SendAsync(byte[] bin);
        SocketError SendAsync(byte[] bin, int offset, int count);
        SocketError SendAsync(Sinan.Collections.BytesSegment buffer);

        bool Decode(byte[] buffer, int offset, int count);
        void ReceiveAsync(byte[] buffer, int offset, int count);
        void Close();
    }

    /// <summary>
    /// 客户端连接状态
    /// </summary>
    public enum ConnectionState : byte
    {
        None = 0,
        Connect = 1,
        Handshake = 2,
        Connected = 3,
        Error = 4,
        Disconnected = 5,
    }

}
