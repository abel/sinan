using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Command;
using Sinan.Entity;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 多倍经验
    /// </summary>
    public sealed class ManyExpBusiness : PartBusiness
    {
        public ManyExpBusiness(Part part)
            : base(part)
        {
        }

        public override void Start(DateTime startTime, DateTime endTime)
        {
            m_start = startTime;
            m_end = endTime;
            PlayersProxy.CallAll(PartCommand.PartStartR, new object[] { this });
            ExperienceControl.SetExpCoePart(2.0);
        }

        /// <summary>
        /// 结束活动
        /// </summary>
        /// <returns></returns>
        public override void End()
        {
            base.End();
            ExperienceControl.SetExpCoePart(1.0);
        }
    }
}