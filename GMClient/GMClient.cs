using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.FastSocket;
using Sinan.Collections;

namespace Sinan.GMModule
{
    class GMClient : AsyncTcpClient
    {
        static byte[] hand = new byte[8] { 8, 0, 99, 0, 0, 0, 0, 0 };

        static GMClient()
        {
        }

        int m_zoneid;
        protected override void OnConnected(System.Net.Sockets.Socket e)
        {
            const int bufferSize = 65535;
            BytesSegment receivebuffer = new BytesSegment(new byte[bufferSize], 0, bufferSize);
            BytesSegment sendbuffer = new BytesSegment(new byte[bufferSize], 0, bufferSize);
            GMProcessor2 processor = new GMProcessor2(bufferSize);
            GMToken session = new GMToken(m_zoneid, receivebuffer, sendbuffer, e, processor);
            //发送握手信息..
            session.SendAsync(hand);
            //开始接收数据
            session.ReceiveAsync(null,0,0);
            //发送登录信息
            session.Call(LoginCommand.UserLogin, ConfigLoader.Config.Zoneid.ToString(), ConfigLoader.Config.ReportSIP);
        }
    }
}
