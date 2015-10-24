using System;
using System.Collections.Generic;
using System.Threading;
using log4net;
using Sinan.AMF3;
using Sinan.Command;
using Sinan.Extensions;
using Sinan.Observer;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FrontServer
{

    /// <summary>
    /// 控制命令..
    /// </summary>
    sealed public class ControlMediator : AysnSubscriber
    {
        private static ILog logger = LogManager.GetLogger("ClientError");
        public static byte[] FastErr;
        static ControlMediator()
        {
            try
            {
                var buffer = AmfCodec.Encode(ClientCommand.Err, new object[] { "command fast" });
                FastErr = new byte[buffer.Count];
                Buffer.BlockCopy(buffer.Array, buffer.Offset, FastErr, 0, buffer.Count);
            }
            catch
            {
                FastErr = new byte[0];
            }
        }

        #region  ISubscriber
        public override IList<string> Topics()
        {
            return new string[] 
            {
                ClientCommand.T,
            };
        }

        public override void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;
            if (note == null) return;
            UserSession session = note.Session;
            if (session == null) return;
            switch (note.Name)
            {
                case ClientCommand.T:
                    TResult(session);
                    return;
                default:
                    return;
            }
        }
        #endregion

        const int len = 14;            //编码后的长度
        const int size = 16;           //分隔长度
        const int tR = 104;            //"tR"命令对应的编码号
        const int count = (0x0FFF + 1);//总包数
        const int mask = 0x000FFFF;    //掩码(size*count-1)


        static int m_offset;
        static readonly byte[] m_bin = new byte[size * count];

        static ControlMediator()
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
        }

        public static void TResult(UserSession session)
        {
            int offset = Interlocked.Add(ref m_offset, size) & mask;
            long nowTicks = WriteTime(offset + 6);
            session.SendAsync(m_bin, offset, len);

            TimeControl t = session.Buffer;
            int id = session.ConnectID;

            //每个计时周期表示一百纳秒，即一千万分之一秒。1 毫秒内有 10,000 个计时周期
            //大于间隔,计数清零,连续8次间隔小于指定间隔,则断开客户端
            long tick = nowTicks - t.Ticks;
            if (tick > 5000 * 10000)
            {
                t.SpeenWarn = 0; //重置警告
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
    }
}
