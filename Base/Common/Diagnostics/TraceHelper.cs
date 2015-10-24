using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Sinan.Diagnostics
{
    public class TraceHelper : IDisposable
    {
        static long f = Stopwatch.Frequency / 1000;
        static Stopwatch watch;

        string m_msg;
        long m_start;
        public TraceHelper(string msg = null)
        {
            m_msg = msg;
            m_start = watch.ElapsedTicks;
        }

        static TraceHelper()
        {
            watch = Stopwatch.StartNew();
        }
        public void Dispose()
        {
            long ms = (watch.ElapsedTicks - m_start) / f;
            Console.WriteLine(m_msg + ms);
        }

        public static void TraceInfo(object message)
        {
            if (!string.IsNullOrEmpty(Convert.ToString(message)))
            {
                System.Diagnostics.Trace.WriteLine(message);
            }
        }
    }

}
