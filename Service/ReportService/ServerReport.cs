using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Sinan.Log;

namespace Sinan.ReportService
{
    /// <summary>
    /// 服务报告d
    /// 向网站提交服务状态
    /// </summary>
    public class ServerReport
    {
        //本地
        //const string host = "192.168.100.40";
        //const string page = "report.ashx?";

        //QQ
        //const string host = "tencentlog.com";
        //const string page ="stat/report.php?"; 

        StringBuilder sb = new StringBuilder(1024 * 4);

        /// <summary>
        /// 报告地址
        /// </summary>
        string hostStr;
        IPEndPoint m_host;

        int headLen;
        Byte[] sendEnds;
        Byte[] sendBuffer = new Byte[1024 * 16];
        Byte[] recvBuffer = new Byte[1024 * 16];

        public ServerReport(string host, string page)
        {
            hostStr = host;
            try
            {
                m_host = CreateEndPoint(hostStr);
            }
            catch (Exception err)
            {
                LogWrapper.Error(err);
                m_host = null;
            }
            string head = "GET /" + page;
            headLen = Encoding.UTF8.GetBytes(head, 0, head.Length, sendBuffer, 0);
            sendEnds = Encoding.UTF8.GetBytes(" HTTP/1.1\r\nHost: " + host + "\r\nConnection: Close\r\n\r\n");
            //("GET /" + page) + query + (" HTTP/1.1\r\nHost: " + host + "\r\nConnection: Close\r\n\r\n");
        }


        /// <summary>
        /// 提交到服务器.
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public bool Request(LogBase log)
        {
            sb.Clear();
            try
            {
                string query = log.ToString(sb);
                int offset = Encoding.UTF8.GetBytes(query, 0, query.Length, sendBuffer, headLen) + headLen;
                using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    s.Connect(m_host);
                    if (!s.Connected)
                    {
                        try
                        {
                            m_host = CreateEndPoint(hostStr);
                        }
                        catch (Exception err)
                        {
                            LogWrapper.Error(err);
                            m_host = null;
                        }
                        return false;
                    }
                    Buffer.BlockCopy(sendEnds, 0, sendBuffer, offset, sendEnds.Length);
                    s.Send(sendBuffer, 0, offset + sendEnds.Length, SocketFlags.None);
                    int len = 0;
                    while (true)
                    {
                        int bytes = s.Receive(recvBuffer, len, recvBuffer.Length - len, SocketFlags.None);
                        if (bytes == 0)
                        {
                            break;
                        }
                        len += bytes;
                    }
                    string result = (Encoding.UTF8.GetString(recvBuffer, 0, len));
                    // TODO:检查是否写入成功...
                    return result.Contains("\"ret\":0");
                }
            }
            catch
            {
                return false;
            }
        }


        public static IPEndPoint CreateEndPoint(string address, int port = 80)
        {
            IPAddress hostAddress;
            if (!IPAddress.TryParse(address, out hostAddress))
            {
                hostAddress = Dns.Resolve(address).AddressList[0];
            }
            return new IPEndPoint(hostAddress, port);
        }
    }

}
