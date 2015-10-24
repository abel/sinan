using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Sinan.FastSocket;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 握手信息.处理安全沙箱和TGW
    /// </summary>
    class Handshaker : IHandshake
    {
        #region 处理安全沙箱问题
        /// <summary>
        /// 安全沙箱检查(为空是不检查)
        /// </summary>
        FlashSandBox m_sandBox;

        public Handshaker(string crossDomainPolicy = null)
        {
            m_sandBox = new FlashSandBox(crossDomainPolicy);
        }
        #endregion

        public int Execute(System.Net.Sockets.Socket socket, Sinan.Collections.BytesSegment command)
        {
            if (m_sandBox != null)
            {
                int box = FlashSandBox.IsPolicyRequest(command.Array, command.Offset, command.Count);
                if (box > 0)
                {
                    socket.SendTimeout = 1000;
                    socket.Send(m_sandBox.CrossDomainPolicy);
                    socket.Shutdown(SocketShutdown.Both);
                    return -1;
                    //return box;
                }
            }
            int len = TgwCodec.GetTgwLength(command.Array, command.Offset, command.Count);
            return len;
        }
    }
}
