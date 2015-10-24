using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Sinan.Log;

namespace Sinan.BabyOPD
{
    [System.ComponentModel.DesignerCategory("Class")]
    public partial class BabyOPDService : ServiceBase
    {
        static string playerDB = ConfigurationManager.ConnectionStrings["player"].ConnectionString;
        static string gameLogString = ConfigurationManager.ConnectionStrings["gameLog"].ConnectionString;
        static string operationString = ConfigurationManager.ConnectionStrings["operation"].ConnectionString;

        protected System.Threading.Timer m_timer;
        protected PlatformTotalAccess m_total;

        public BabyOPDService()
        {
            this.ServiceName = "BabyOPDService";
            m_total = new PlatformTotalAccess(playerDB, gameLogString, operationString);
        }

        //protected override void OnStart(string[] args)
        //{
        //    m_timer = new System.Threading.Timer(Exec);
        //    DateTime now = DateTime.UtcNow;
        //    int ms = (int)((now.Date.AddDays(1) - now).TotalMilliseconds);
        //    m_timer.Change(ms, 24 * 3600 * 1000);
        //}

        //protected override void OnStop()
        //{
        //    if (m_timer != null)
        //    {
        //        m_timer.Change(Timeout.Infinite, Timeout.Infinite);
        //        m_timer.Dispose();
        //        m_timer = null;
        //    }
        //}

        protected virtual void Exec(object b)
        {
            try
            {
                DateTime endTime = DateTime.UtcNow.AddHours(1).Date;

                string id = endTime.Date.ToString("yyyyMMdd");
                var data = m_total.NewUserInfo(endTime);
                m_total.Save(id, data);
                var data2 = m_total.PlayerLogInfo(endTime);
                m_total.Save(id, data2);
                var date3 = m_total.WeekInfo(endTime);
                m_total.Save(id, date3);

                m_total.SaveTotal(endTime);
                LogWrapper.Warn("统计执行完成");
            }
            catch (System.Exception ex)
            {
                LogWrapper.Warn("统计执行错误:" + ex);
            }
        }
    }
}
