using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Sinan.FastSocket
{
    public static class IPHelper
    {
        /// <summary>
        /// 得到tcp_keepalive结构值
        /// 在Windows中，第一次探测是在最后一次数据发送的两个小时，然后每隔1秒探测一次，一共探测5次，
        /// 假如5次都没有收到回应的话，就会断开这个连接。但两个小时对于我们的项目来说显然太长了......
        /// </summary>
        /// <param name="enable">是否启用Keep-Alive,启用为1</param>
        /// <param name="keepAliveTime">多长时间后开始第一次探测（单位：毫秒）</param>
        /// <param name="keepAliveinterval">探测时间间隔（单位：毫秒）</param>
        /// <returns></returns>
        public static byte[] Keepalive(int enable, int keepAliveTime, int keepAliveinterval)
        {
            byte[] inOptionValues = new byte[12];
            BitConverter.GetBytes(enable).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(inOptionValues, 4);
            BitConverter.GetBytes(keepAliveinterval).CopyTo(inOptionValues, 8);
            return inOptionValues;
        }

        /// <summary>
        /// 取得本机的第1个IPV4地址
        /// </summary>
        /// <returns></returns>
        public static IPAddress FirstLocalHostIP()
        {
            //得到本机的主机名
            string strHostName = Dns.GetHostName();
            //取得本机IP
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            foreach (System.Net.IPAddress ip in ipEntry.AddressList)
            {
                //IP V4的地址
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            return null;
        }

        /// <summary>
        /// 使用指定的IP地址和端口创建网络端点
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="ports"></param>
        /// <returns></returns>
        public static List<IPEndPoint> CreateEndPoints(string ip, IList ports)
        {
            List<IPEndPoint> addresss = new List<IPEndPoint>();
            System.Net.IPAddress address;
            if (IPAddress.TryParse(ip, out address))
            {
                foreach (object port in ports)
                {
                    addresss.Add(new IPEndPoint(address, Convert.ToInt32(port)));
                }
            }
            return addresss;
        }

        /// <summary>
        ///  使用指IPAddress.Any和指定的端口创建网络端点
        /// </summary>
        /// <param name="ports"></param>
        /// <returns></returns>
        public static List<IPEndPoint> CreateEndPoints(IList ports)
        {
            List<IPEndPoint> addresss = new List<IPEndPoint>();
            foreach (object port in ports)
            {
                addresss.Add(new IPEndPoint(IPAddress.Any, Convert.ToInt32(port)));
            }
            return addresss;
        }

        public static void SafeClose(this Socket socket)
        {
            try
            {
                if (socket != null && socket.Connected)
                {
                    socket.Close();
                }
            }
            catch { }
        }

        public static void SafeClose(this Socket socket, SocketShutdown down)
        {
            try
            {
                if (socket != null && socket.Connected)
                {
                    socket.Shutdown(down);
                    socket.Close();
                }
            }
            catch { }
        }

        //public static void SafeClose(this Socket socket, int timeout)
        //{
        //    try
        //    {
        //        if (socket != null && socket.Connected)
        //        {
        //            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
        //            socket.Close(timeout);
        //        }
        //    }
        //    catch { }
        //}

        public static IPEndPoint CreateEndPoint(string address, int port)
        {
            IPAddress hostAddress;
            if (!IPAddress.TryParse(address, out hostAddress))
            {
                hostAddress = Dns.Resolve(address).AddressList[0];
            }
            return new IPEndPoint(hostAddress, port);
        }

        //public static IPEndPoint CreateEndPoint(string address)
        //{
        //    if (address != null)
        //    {
        //        string[] ap = address.Split(new char[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
        //        if (ap.Length == 2)
        //        {
        //            int port;
        //            if (int.TryParse(ap[1], out port))
        //            {
        //                return CreateEndPoint(ap[0], port);
        //            }
        //        }
        //    }
        //    return null;
        //}

        //[DllImport("ws2_32.dll", SetLastError = true)]
        //static extern SocketError WSASend([In] IntPtr socketHandle, [In] ref WSABuffer buffer, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] IntPtr overlapped, [In] IntPtr completionRoutine);

        //[StructLayout(LayoutKind.Sequential)]
        //internal struct WSABuffer
        //{
        //    internal int Length;
        //    internal IntPtr Pointer;
        //}
    }

}