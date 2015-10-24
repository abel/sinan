using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Log;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 日志部分
    /// </summary>
    partial class PlayerBusiness
    {
        /// <summary>
        /// 玩家被PK的时间
        /// </summary>
        public DateTime PKTime
        {
            get;
            set;
        }       

        public void WriteLog(LogBase log)
        {
            if (log.opuid == 0)
            {
                log.opuid = this.PID;
            }
            if (log.opopenid == null)
            {
                log.opopenid = this.UserID;
            }
            ServerLogger.WriteLog(this.m_session, log);
        }
    }
}
