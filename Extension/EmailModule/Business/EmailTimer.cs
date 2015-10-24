using Sinan.GameModule;
using Sinan.Schedule;

namespace Sinan.EmailModule.Business
{
    public sealed class EmailTimer : SchedulerBase
    {
        EmailTimer()
            : base(3600 * 1000, 3600 * 1000)
        {
        }

        protected override void Exec()
        {
            EmailAccess.Instance.DeleteEmail();
        }
    }
}
