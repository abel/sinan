using System;
using System.Collections.Generic;
using System.Threading;
using log4net;
using Sinan.AMF3;
using Sinan.Command;
using Sinan.Extensions;
using Sinan.FastSocket;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Observer;

namespace Sinan.FrontServer
{
    public abstract class CommandProcessor : IBufferProcessor
    {
        //接收客户端消息的最大容量
        readonly protected int m_capacity;
        protected CommandProcessor(int capacity)
        {
            m_capacity = capacity;
        }

        protected static string DecodeCommand(UserSession user, int command)
        {
            //心跳
            if (command == 103)
            {
                TResult(user);
                return string.Empty;
            }

            UserBuffer buffer = user.Buffer;
            if (buffer == null) return null;

            string name = null;
            //检查命令范围
            if (command > 100 && command <= CommandManager.MaxCommand)
            {
                //检查调用频繁度
                int ct = CommandManager.ControlTicks[command];
                if (ct > 0)
                {
                    long interval = buffer.LastInterval(command);
                    if (interval < ct)
                    {
                        if (string.IsNullOrEmpty(user.UserID))
                        {
                            user.Close();
                            return null;
                        }
                        else if (buffer.CallTicks[0]++ > 20000)
                        {
                            //发送操作太快信息
                            try
                            {
                                user.SendAsync(FastErr);
                                LogWrapper.Warn(user.ToString("Operating too fast|"));
                            }
                            finally
                            {
                                user.Close();
                            }
                            return null;
                        }
                        return string.Empty;
                    }
                }
                name = CommandManager.Instance.ReadString(command);
            }

            if (name == null)
            {
                //命令不存在时.如果用户未登录,或登录用户总计16次出错则断开
                if (string.IsNullOrEmpty(user.UserID))
                {
                    user.Close();
                }
                else if (buffer.CallTicks[1]++ > 16)
                {
                    LogWrapper.Warn(user.ToString("Protocol error|"));
                    user.Close();
                }
            }
            return name;
        }

        const int len = 14;            //编码后的长度
        const int size = 16;           //分隔长度
        const int tR = 104;            //"tR"命令对应的编码号
        const int count = (0x0FFF + 1);//总包数
        const int mask = 0x000FFFF;    //掩码(size*count-1)

        static int m_offset;
        static readonly byte[] m_bin = new byte[size * count];
        private static ILog logger = LogManager.GetLogger("ClientError");
        private static byte[] FastErr;

        static CommandProcessor()
        {
            for (int i = 0; i < count; i++)
            {
                m_bin[size * i + 0] = len;  //总长14
                m_bin[size * i + 1] = 0;
                m_bin[size * i + 2] = (byte)(tR & 0xFF);
                m_bin[size * i + 3] = (byte)(tR >> 8);
                m_bin[size * i + 4] = Amf3Type.DateTime;  //8
                m_bin[size * i + 5] = 1;    //非引用
            }
            try
            {
                var buffer = AmfCodec.Encode(ClientCommand.Err, new object[] { "Operating too fast" });
                FastErr = new byte[buffer.Count];
                Buffer.BlockCopy(buffer.Array, buffer.Offset, FastErr, 0, buffer.Count);
            }
            catch
            {
                FastErr = new byte[0];
            }
        }

        /// <summary>
        /// 处理心跳包
        /// </summary>
        /// <param name="session"></param>
        public static void TResult(UserSession session)
        {
            int offset = Interlocked.Add(ref m_offset, size) & mask;
            long nowTicks = WriteTime(offset + 6);
            session.SendAsync(new Sinan.Collections.BytesSegment(m_bin, offset, len));

            TimeControl t = session.Buffer;
            if (t == null) return;

            //每个计时周期表示一百纳秒，即一千万分之一秒。1 毫秒内有 10,000 个计时周期
            //大于间隔,计数清零,连续8次间隔小于指定间隔,则断开客户端
            long tick = nowTicks - t.Ticks;
            if (tick > 5000 * 10000)
            {
                //重置警告
                t.SpeenWarn = 0;
            }
            //大于20毫秒,小于4900毫秒,则进行警告计数
            else if (tick > 20 * 10000 && tick < 4900 * 10000)
            {
                //连续8次警告,则断开客户端
                if (Interlocked.Increment(ref t.SpeenWarn) == 8)
                {
                    try
                    {
                        session.SendAsync(FastErr);
                        logger.Info(session.ToString("ForceExit|"));
                    }
                    finally
                    {
                        session.Close();
                    }
                    return;
                }
            }
            t.Ticks = nowTicks;
        }

        static unsafe private long WriteTime(int offset)
        {
            long now = DateTime.UtcNow.Ticks;
            double ms = (now - UtcTimeExtention.UnixEpochTicks) * 0.0001d;
            long v = *((long*)&ms);
            for (int i = 7; i >= 0; i--)
            {
                m_bin[offset++] = (byte)(v >> (i << 3));
            }
            return now;
        }

        public int Execute(ISession session, Sinan.Collections.BytesSegment data)
        {
            List<Tuple<int, List<object>>> results = new List<Tuple<int, List<object>>>();
            int offset = data.Offset;
            int maxIndex = offset + data.Count;
            byte[] buffer = data.Array;
            while (maxIndex >= offset + 2)
            {
                // 取包长,前两字节表示
                int packetLen = buffer[offset] + (buffer[offset + 1] << 8);
                if (packetLen < m_capacity)
                {
                    if (maxIndex < offset + packetLen)
                    {
                        break;
                    }
                    try
                    {
                        if (!session.Decode(buffer, offset, packetLen))
                        {
                            session.Close();
                            return -1;
                        }
                        // 取命令,第3/4字节
                        int command = buffer[offset + 2] + (buffer[offset + 3] << 8);
                        var param = AmfCodec.Decode(buffer, offset, packetLen);
                        results.Add(new Tuple<int, List<object>>(command, param));
#if FlowLog
                        // 写流量接收记录
                        FlowLog.AddIn(command, packetLen);
#endif
                    }
                    catch (AmfException ex)
                    {
                        LogWrapper.Warn(session.ToString(), ex);
                        session.Close();
                        return -1;
                    }
                }
                foreach (var v in results)
                {
                    if (!this.Execute(session, v))
                    {
                        break;
                    }
                }
                offset += packetLen;
            }
            return maxIndex - offset;
        }

        public abstract bool Execute(ISession token, Tuple<int, List<object>> bin);
    }
}
