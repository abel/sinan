using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Sinan.FastSocket
{
    /// <summary>
    /// 处理握手信息
    /// 如Flash安全沙箱/腾讯TGW
    /// </summary>
    public interface IHandshake
    {
        /// <summary>
        /// 处理命令
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="command"></param>
        /// <returns>已处理数据的长度</returns>
        int Execute(Socket socket, Sinan.Collections.BytesSegment command);
    }
}
