using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Schedule
{
    public interface ISchedule
    {
        void Start();
        void Close();
    }
}
