using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Sinan.FastSocket
{
    public static class SslHttpClient
    {
        #region Ssl Socket
        static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static byte[] Get(IPEndPoint endpoint, HttpArgs args, X509CertificateCollection certificates = null)
        {
            return InternalSslSocketHttp(endpoint, args, HttpMethod.GET, certificates);
        }

        public static byte[] Post(IPEndPoint endpoint, HttpArgs args, X509CertificateCollection certificates = null)
        {
            return InternalSslSocketHttp(endpoint, args, HttpMethod.POST, certificates);
        }

        static byte[] InternalSslSocketHttp(IPEndPoint endpoint, HttpArgs args, HttpMethod method, X509CertificateCollection certificates)
        {
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    client.Connect(endpoint);
                    if (client.Connected)
                    {
                        using (SslStream stream = new SslStream(new NetworkStream(client), false, ValidateServerCertificate, null))
                        {
                            stream.AuthenticateAsClient("ServerName", certificates, SslProtocols.Tls, false);
                            if (stream.IsAuthenticated)
                            {
                                //生成协议包
                                byte[] buff = HttpClient.ParseHttpArgs(method, args);
                                stream.Write(buff, 0, buff.Length);
                                stream.Flush();
                                return ParseSslResponse(endpoint, stream, args, certificates);

                            }
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
        /// 解析 Ssl Response
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="ssl"></param>
        /// <param name="args"></param>
        /// <param name="certificates"></param>
        /// <returns></returns>
        private static byte[] ParseSslResponse(IPEndPoint endpoint, SslStream ssl, HttpArgs args, X509CertificateCollection certificates)
        {
            //尝试4秒时间读取协议头
            CancellationTokenSource source = new CancellationTokenSource();
            Task<string> myTask = Task.Factory.StartNew<string>(HttpClient.ReadHeaderProcess, ssl, source.Token);
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
                        return Get(endpoint, args, certificates);
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
                            if (ssl.Read(bytes, 0, content_length) > 0)
                            {
                                return bytes;
                            }
                        }
                    }
                    else
                    {
                        //不存在Content-Length协议头
                        return HttpClient.ParseResponse(ssl);
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


    }
}
