//#define TraceCall
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.FastSocket
{
    /// <summary>
    /// 流量日志
    /// </summary>
    public class FlowLog
    {
        static int[] m_inLog = new int[65536];
        static int[] m_outLog = new int[65536];

#if FlowLog
        /// <summary>
        /// 流入
        /// </summary>
        /// <param name="command">命令代码</param>
        /// <param name="count">接收的字节数</param>
        /// <returns></returns>
        static public int AddIn(int command, int count)
        {
            return System.Threading.Interlocked.Add(ref m_inLog[command], count);
        }

        /// <summary>
        /// 流出
        /// </summary>
        /// <param name="command">命令代码</param>
        /// <param name="count">发送的字节数</param>
        /// <returns></returns>
        static public int AddOut(int command, int count)
        {
            return System.Threading.Interlocked.Add(ref m_outLog[command], count);
        }

        static public int AddIn(byte[] buffer, int offset)
        {
            int count = buffer[offset] + (buffer[offset + 1] << 8);
            int command = buffer[offset + 2] + (buffer[offset + 3] << 8);
            //请求安全沙箱,前四字节: 3C 70 6F 6C 
            //if (count == 0x3C + (0x70 << 8) && command == 0x6F + (0x6C << 8))
            //{
            //    return 0;
            //}
            return System.Threading.Interlocked.Add(ref m_inLog[command], count);
        }

        static public int AddOut(byte[] buffer, int offset)
        {
            int count = buffer[offset] + (buffer[offset + 1] << 8);
            int command = buffer[offset + 2] + (buffer[offset + 3] << 8);
            //发送安全沙箱,前四字节: 3C 63 72 6F
            //if (count == 0x3C + (0x63 << 8) && command == 0x72 + (0x6F << 8))
            //{
            //    return 0;
            //}
            return System.Threading.Interlocked.Add(ref m_outLog[command], count);
        }
#endif

        static public void Reset(out int[] oldInLog, out int[] oldOutLog)
        {
            oldInLog = m_inLog;
            oldOutLog = m_outLog;

            m_inLog = new int[65536];
            m_outLog = new int[65536];
        }
    }
}
