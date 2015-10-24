using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sinan.Data;
using Sinan.Observer;
using MongoDB.Driver;

namespace Sinan.Core
{
    public class Application
    {
        /// <summary>
        /// 应用程序开始启动
        /// </summary>
        public const string APPSTART = "$appstart";

        /// <summary>
        /// 应用程序停止通知
        /// </summary>
        public const string APPSTOP = "$appsstop";

        protected static Application m_instance;

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static Application Instance
        {
            get { return m_instance; }
        }

        //DBWriteProxy m_writeProxy = new DBWriteProxy();

        protected Application()
        {
        }

        ///// <summary>
        ///// 将待写数据放入写队列..
        ///// </summary>
        ///// <param name="data"></param>
        //public bool QueueWriteItem(IPersistable data)
        //{
        //    if (data != null && data.Changed)
        //    {
        //        m_writeProxy.Write(data);
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// 及时写数据..
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //public bool WriteItem(ISmartEntity data)
        //{
        //    return data.Write();
        //}

        public virtual void Start()
        {
            //Task.Factory.StartNew(m_writeProxy.StartWork);
            Notifier.Instance.Publish(new Notification(APPSTART, new object[] { this }), false);
        }

        public virtual void Stop()
        {
            Notifier.Instance.Publish(new Notification(APPSTOP, new object[] { this }), false);
        }
    }
}
