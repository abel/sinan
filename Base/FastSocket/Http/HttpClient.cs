using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sinan.FastSocket
{
    public class HttpClient
    {
        #region  Socket
        public static byte[] Get(IPEndPoint endpoint, HttpArgs args)
        {
            return InternalSocketHttp(endpoint, args, HttpMethod.GET);
        }

        public static byte[] Post(IPEndPoint endpoint, HttpArgs args)
        {
            return InternalSocketHttp(endpoint, args, HttpMethod.POST);
        }

        static byte[] InternalSocketHttp(IPEndPoint endpoint, HttpArgs args, HttpMethod method)
        {
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    client.Connect(endpoint);
                    if (client.Connected)
                    {
                        using (NetworkStream stream = new NetworkStream(client))
                        {
                            //生成协议包
                            byte[] buff = HttpClient.ParseHttpArgs(method, args);
                            stream.Write(buff, 0, buff.Length);
                            stream.Flush();
                            return ParseResponse(endpoint, stream, args);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return null;
        }

        /// <summary>
        /// 解析  Response
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="stream"></param>
        /// <param name="args"></param>
        /// <param name="certificates"></param>
        /// <returns></returns>
        private static byte[] ParseResponse(IPEndPoint endpoint, Stream stream, HttpArgs args)
        {
            //尝试4秒时间读取协议头
            CancellationTokenSource source = new CancellationTokenSource();
            Task<string> myTask = Task.Factory.StartNew<string>(ReadHeaderProcess, stream, source.Token);
            if (myTask.Wait(4000))
            {
                string header = myTask.Result;
                if (header.StartsWith("HTTP/1.1 302"))
                {
                    int start = header.IndexOf("LOCATION", StringComparison.OrdinalIgnoreCase);
                    if (start > 0)
                    {
                        string temp = header.Substring(start, header.Length - start);
                        string[] sArry = Regex.Split(temp, "\r\n");
                        args.Url = sArry[0].Remove(0, 10);
                        //注意：302协议需要重定向
                        return Get(endpoint, args);
                    }
                }
                //继续读取内容
                else if (header.StartsWith("HTTP/1.1 200"))
                {
                    int start = header.IndexOf("CONTENT-LENGTH", StringComparison.OrdinalIgnoreCase);
                    if (start > 0)
                    {
                        string temp = header.Substring(start, header.Length - start);
                        string[] sArry = temp.Split(new char[] { '\r', '\n', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        int content_length;
                        if (sArry.Length > 1 && Int32.TryParse(sArry[1], out content_length))
                        {
                            byte[] bytes = new byte[content_length];
                            if (stream.Read(bytes, 0, content_length) > 0)
                            {
                                return bytes;
                            }
                        }
                    }
                    else
                    {
                        //不存在Content-Length协议头
                        return ParseResponse(stream);
                    }
                }
                else
                {
                    return Encoding.UTF8.GetBytes(header);
                }
            }
            else
            {
                //超时的话，别忘记取消任务哦
                source.Cancel();
            }
            return null;
        }

        #endregion

        #region  Helper
        /// <summary>
        ///  读取协议头
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static string ReadHeaderProcess(object args)
        {
            Stream stream = (Stream)args;
            StringBuilder bulider = new StringBuilder(100);
            try
            {
                int read = stream.ReadByte();
                while (read != -1)
                {
                    char b = (char)read;
                    bulider.Append(b);
                    int len = bulider.Length;
                    if (b == '\n' && len >= 4)
                    {
                        if (bulider[len - 4] == '\r' && bulider[len - 3] == '\n' && bulider[len - 2] == '\r')
                        {
                            break;
                        }
                    }
                    read = stream.ReadByte();
                }
            }
            catch { }
            return bulider.ToString();
        }

        /// <summary>
        /// 注意：此函数可能产生死循环
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        internal static byte[] ParseResponse(Stream stream)
        {
            //没有指定协议头，尝试读取至</html>
            byte[] array = new byte[10];
            byte[] buff = new byte[1024];
            int total = 0;
            int len = stream.Read(buff, 0, buff.Length);
            while (len > 0)
            {
                if (array.Length < total + len)
                {
                    byte[] old = array;
                    array = new byte[old.Length * 2];
                    Buffer.BlockCopy(old, 0, array, 0, total);
                }
                Buffer.BlockCopy(buff, 0, array, 0, len);
                total += len;
                //string temp = Encoding.UTF8.GetString(array, 0, total);
                //if (temp.ToUpper().Contains("</HTML>"))
                //{
                //    break;
                //}
                len = stream.Read(buff, 0, buff.Length);
            }
            return array;
        }

        internal static byte[] ParseHttpArgs(HttpMethod method, HttpArgs args)
        {
            StringBuilder bulider = new StringBuilder(1024);
            if (method == HttpMethod.POST)
            {
                bulider.AppendLine(string.Format("POST {0} HTTP/1.1", args.Url));
                bulider.AppendLine("Content-Type: application/x-www-form-urlencoded");
            }
            else
            {
                bulider.AppendLine(string.Format("GET {0} HTTP/1.1", args.Url));
                bulider.AppendLine("Content-type:text/html;charset=utf-8");
                //bulider.AppendLine("Content-Type:text/plain;charset=GBK");
            }
            bulider.AppendLine(string.Format("Host: {0}", args.Host));
            bulider.AppendLine("Connection: Close");
            //bulider.AppendLine("User-Agent: Mozilla/5.0 (Windows NT 6.1; IE 9.0)");
            //if (!string.IsNullOrEmpty(args.Referer))
            //{
            //    bulider.AppendLine(string.Format("Referer: {0}", args.Referer));
            //}
            //if (!string.IsNullOrEmpty(args.Accept))
            //{
            //    bulider.AppendLine(string.Format("Accept: {0}", args.Accept));
            //}
            //if (!string.IsNullOrEmpty(args.Cookie))
            //{
            //    bulider.AppendLine(string.Format("Cookie: {0}", args.Cookie));
            //}
            if (method == HttpMethod.POST)
            {
                int len = Encoding.UTF8.GetByteCount(args.Body);
                bulider.AppendLine(string.Format("Content-Length:{0}\r\n", len));
                bulider.Append(args.Body);
            }
            else
            {
                bulider.Append("\r\n");
            }
            string header = bulider.ToString();
            return Encoding.UTF8.GetBytes(header);
        }
        #endregion


        byte[] sendEnds;
        IPEndPoint m_ep;
        string m_host;
        int m_port;

        public HttpClient(string host, int port = 80)
        {
            m_host = host;
            m_port = port;
            sendEnds = Encoding.UTF8.GetBytes(" HTTP/1.1\r\nHost: " + host + "\r\nConnection: Close\r\n\r\n");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string Get(string url, bool returnHead = false)
        {
            try
            {
                IPEndPoint ep = m_ep;
                if (ep == null)
                {
                    ep = IPHelper.CreateEndPoint(m_host, m_port);
                    m_ep = ep;
                }
                using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    s.Connect(ep);
                    if (s.Connected)
                    {
                        string head = "GET " + url;
                        int bufferLen = (head.Length << 2) + sendEnds.Length;
                        byte[] sendBuffer = new byte[bufferLen];
                        int headLen = Encoding.UTF8.GetBytes(head, 0, head.Length, sendBuffer, 0);
                        Buffer.BlockCopy(sendEnds, 0, sendBuffer, headLen, sendEnds.Length);
                        s.Send(sendBuffer, 0, headLen + sendEnds.Length, SocketFlags.None);

                        int count = 0;
                        byte[] receiveBuffer = new byte[1000];
                        while (true)
                        {
                            int size = receiveBuffer.Length - count;
                            if (size < 1000)
                            {
                                //扩展大小
                                byte[] newbuffer = new byte[receiveBuffer.Length + 1000];
                                Buffer.BlockCopy(receiveBuffer, 0, newbuffer, 0, receiveBuffer.Length);
                                receiveBuffer = newbuffer;
                                size += 1000;
                            }
                            int bytes = s.Receive(receiveBuffer, count, size, SocketFlags.None);
                            if (bytes == 0)
                            {
                                break;
                            }
                            count += bytes;
                        }
                        if (!returnHead)
                        {
                            //查找'\r\n\r\n'
                            for (int i = 0; i < count - 3; i++)
                            {
                                if (receiveBuffer[i] == '\r' && receiveBuffer[i + 1] == '\n' && receiveBuffer[i + 2] == '\r' && receiveBuffer[i + 3] == '\n')
                                {
                                    i += 4;
                                    return (Encoding.UTF8.GetString(receiveBuffer, i, count - i));
                                }
                            }
                        }
                        return (Encoding.UTF8.GetString(receiveBuffer, 0, count));
                    }
                }
            }
            catch
            {
                m_ep = null;
            }
            return null;
        }
    }
}
