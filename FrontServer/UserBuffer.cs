using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.FastSocket;
using Sinan.GameModule;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 时间控制
    /// </summary>
    public class TimeControl
    {
        /// <summary>
        /// 调用时间
        /// (位置0-100)系统保留
        /// 0:用于统计非法调用次数
        /// 1:用于统计解码失败次数
        /// 其它位置保存命令调用时间的Ticks
        /// </summary>
        readonly public long[] CallTicks;

        public TimeControl()
        {
            CallTicks = new long[CommandManager.MaxCommand];
        }

        /// <summary>
        /// 计时周期
        /// </summary>
        public long Ticks;

        /// <summary>
        /// 加速器警告的次数
        /// </summary>
        public int SpeenWarn;

        /// <summary>
        /// 取上次调用到现在的间隔
        /// 并用当前时间替换旧间隔
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public long LastInterval(int command)
        {
            long old = CallTicks[command];
            long now = DateTime.UtcNow.Ticks;
            CallTicks[command] = now;
            return now - old;
        }
    }

    public class UserBuffer : TimeControl
    {
        readonly SessionFactory factory;
        readonly public int BufferID;
        readonly public Sinan.Collections.BytesSegment ReceiveBuffer;
        readonly public Sinan.Collections.BytesSegment SendBuffer;
        /// <summary>
        /// 发送池,保证等待发送的数据
        /// </summary>
        readonly public Sinan.Collections.CircularQueue<Sinan.Collections.BytesSegment> SendPool;

        internal UserBuffer(int id,
            SessionFactory f,
            Sinan.Collections.BytesSegment r,
            Sinan.Collections.BytesSegment s,
            int sendQueueSize = 64)
        {
            this.factory = f;
            this.BufferID = id;
            this.ReceiveBuffer = r;
            this.SendBuffer = s;
            this.SendPool = new Sinan.Collections.CircularQueue<Sinan.Collections.BytesSegment>(sendQueueSize);
        }

        public void Free(ISession token)
        {
            SendPool.Clear();
            SpeenWarn = 0;
            Ticks = 0;
            Array.Clear(CallTicks, 0, CallTicks.Length);
            factory.ReoverySession(this, token);
        }
    }

}
