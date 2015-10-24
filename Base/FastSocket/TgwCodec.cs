
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.FastSocket
{
    /// <summary>
    /// Tencent GateWay 辅助类
    /// </summary>
    public class TgwCodec
    {
        byte[] m_head;

        /// <summary>
        /// TGW头
        /// </summary>
        public byte[] TgwHead
        {
            get { return m_head; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appid">应用ID</param>
        /// <param name="zone">分区ID</param>
        /// <param name="port">端口</param>
        public TgwCodec(int appid, int zone, int port)
        {
            //string tgwHead = String.Format("GET / HTTP/1.1\r\nHost: app{0}-{1}.qzoneapp.com:{2}\r\n\r\n", appid, zone, port);
            string tgwHead = String.Format("get / http/1.1\r\nhost: {1}.app{0}.qzoneapp.com:{2}\r\n\r\n", appid, zone, port);
            m_head = Encoding.ASCII.GetBytes(tgwHead);
        }

        /// <summary>
        /// 检查是否是TGW请求头
        /// </summary>
        public bool IsTgwHead(byte[] bin, int offset, int count)
        {
            if (count < m_head.Length)
            {
                return false;
            }
            for (int i = 0; i < m_head.Length; i++)
            {
                byte b = bin[i + offset];
                if (b >= 'A' && b <= 'Z')
                {
                    b += 0x20; //('a' - 'A');
                }
                if (m_head[i] != b)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 检查是否是TGW请求头
        /// </summary>
        public bool IsTgwHead(byte[] bin)
        {
            return IsTgwHead(bin, 0, bin.Length);
        }


        static byte[] head = Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\n");

        /// <summary>
        /// 返回标识的长度
        /// </summary>
        /// <param name="bin"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int GetTgwLength(byte[] bin, int offset, int count)
        {
            if (count <= head.Length)
            {
                return 0;
            }
            int i = 0;
            for (; i < head.Length; i++)
            {
                byte b = bin[offset + i];
                if (b >= 'a' && b <= 'z')
                {
                    b -= 0x20; //('a' - 'A');
                }
                if (head[i] != b)
                {
                    return 0;
                }
            }
            count -= 3;
            while (i < count)
            {
                if (bin[offset + i] == '\r' && bin[offset + (i + 1)] == '\n'
                    && bin[offset + (i + 2)] == '\r' && bin[offset + (i + 3)] == '\n')
                {
                    return i + 4;
                }
                i++;
            }
            return 0;
        }

        /// <summary>
        /// 返回标识的长度
        /// </summary>
        /// <param name="bin"></param>
        /// <returns></returns>
        public static int GetTgwLength(byte[] bin)
        {
            return GetTgwLength(bin, 0, bin.Length);
        }
    }
}
