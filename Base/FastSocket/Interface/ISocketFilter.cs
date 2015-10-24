using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Sinan.FastSocket
{
    /// <summary>
    /// 过滤连接
    /// </summary>
    public interface ISocketFilter
    {
        /// <summary>
        /// 是否允许连接
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        bool AllowConnect(Socket socket);

        /// <summary>
        /// 连接后等待接收数据的最大空闲时间(毫秒)
        /// </summary>
        int IdleMilliseconds { get; }
    }
}
