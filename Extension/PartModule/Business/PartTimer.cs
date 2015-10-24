using Sinan.FrontServer;
using Sinan.Schedule;

namespace Sinan.PartModule.Business
{
    public sealed class PartTimer : SchedulerBase
    {
        PartTimer()
            : base(5 * 1000, 5 * 1000)
        {
        }

        protected override void Exec()
        {
            NoticeBusiness.NoticeCall();
        }
    }
}
