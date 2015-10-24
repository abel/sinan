using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Sinan.Log;
using Sinan.Observer;

namespace Sinan.Observer
{
    /// <summary>
    /// 异步订阅者基类
    /// </summary>
    public abstract class AysnSubscriber : ISubscriber
    {
        static long f = Stopwatch.Frequency / 1000;

        /// <summary>
        /// 慢执行计时毫秒数
        /// </summary>
        public static long slowms = 200;

        /// <summary>
        /// 记录慢命令
        /// </summary>
        public static bool profile = false;

        static Stopwatch watch = Stopwatch.StartNew();

        /// <summary>
        /// 接收到消息时执行的方法
        /// </summary>
        /// <param name="note"></param>
        void ISubscriber.SafeExecute(object notification)
        {
            INotification note = notification as INotification;
            try
            {
                if (profile)
                {
                    long start = watch.ElapsedTicks;
                    this.Execute(note);
                    long ms = (watch.ElapsedTicks - start) / f;
                    if (ms > slowms)
                    {
                        LogWrapper.Warn(note.Name + ":" + ms.ToString());
                    }
                }
                else
                {
                    this.Execute(note);
                }
            }
            catch (System.Exception ex)
            {
                try
                {
                    LogWrapper.Error(note.Name, ex);
                }
                catch { }
            }
        }

        public abstract IList<string> Topics();
        public abstract void Execute(INotification note);
    }
}
