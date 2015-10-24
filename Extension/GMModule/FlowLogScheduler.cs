//#define TraceCall

namespace Sinan.GMModule.Business
{
#if TraceCall
    /// <summary>
    /// 流量日志
    /// </summary>
    sealed public class FlowLogScheduler : SchedulerBase
    {
        private static ILog logger = LogManager.GetLogger("GMLog");

        DateTime timer = DateTime.MinValue;
        public FlowLogScheduler()
            : base(3600 * 1000, 3600 * 1000)
        {
        }

        protected override void Exec()
        {
            try
            {
                int[] inLog, outLog;
                FlowLog.Reset(out inLog, out outLog);
                //写日志
                WriteLog(inLog, "InLog{");
                WriteLog(outLog, "OutLog{");
            }
            finally
            {
                timer = timer.AddSeconds(60 * 60);
            }
        }

        private static void WriteLog(int[] inLog, string head)
        {
            StringBuilder sb = new StringBuilder(1024 * 8);
            sb.Append(head);
            for (int i = 0; i < inLog.Length; i++)
            {
                int count = inLog[i];
                if (count != 0)
                {
                    sb.Append(i.ToString());
                    sb.Append(':');
                    sb.Append(count.ToString());
                    sb.Append(';');
                }
            }
            sb.Append("}");
            logger.Info(sb.ToString());
        }
    }
#endif
}
