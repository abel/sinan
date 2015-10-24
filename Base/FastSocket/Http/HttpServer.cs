using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Sinan.Util;

namespace Sinan.FastSocket
{
    //簡陋但堪用的HTTP Server
    public abstract class HttpServer
    {
        private Thread serverThread;
        private TcpListener listener;

        public void Start(string address, int port)
        {
            if (listener != null) return;
            IPAddress ipAddr = IPAddress.Parse(address);
            Start(new IPEndPoint(ipAddr, port));
        }

        public void Start(IPEndPoint ep)
        {
            listener = new TcpListener(ep);
            //另建Thread執行
            serverThread = new Thread(() =>
            {
                listener.Start();
                try
                {
                    while (true)
                    {
                        Socket s = listener.AcceptSocket();
                        ThreadPool.UnsafeQueueUserWorkItem(AcceptSocket, s);
                    }
                }
                catch { }
            });
            serverThread.Start();
        }

        private void AcceptSocket(object socket)
        {
            Socket client = socket as Socket;
            if (client == null) return;
            try
            {
                client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                using (NetworkStream ns = new NetworkStream(client))
                using (StreamReader sr = new StreamReader(ns, Encoding.UTF8, false))
                {
                    CompactRequest request = new CompactRequest(sr);
                    //呼叫自訂的處理邏輯，得到要回傳的Response
                    CompactResponse response = Process(client, request);
                    if (response != null)
                    {
                        if (client.Connected)
                        {
                            //傳回Response
                            byte[] data = Encoding.UTF8.GetBytes(response.GetDate());
                            //寫入資料本體
                            client.Send(new ArraySegment<byte>[]{
                                    new ArraySegment<byte>(CompactResponse.HttpOK),
                                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(data.Length + "\r\n\r\n")),
                                    new ArraySegment<byte>(data)}
                                   );
                            client.Shutdown(SocketShutdown.Both);
                        }
                    }
                }
            }
            catch { }
            finally
            {
                client.SafeClose();//結束連線
            }
        }


        public abstract CompactResponse Process(Socket client, CompactRequest request);


        public void Stop()
        {
            try
            {
                listener.Stop();
                listener = null;
                serverThread.Abort();
            }
            catch { }
        }
    }
}