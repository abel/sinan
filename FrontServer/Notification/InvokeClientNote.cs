using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Observer;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 连接的客户端发出的通知 
    /// </summary>
    sealed public class InvokeClientNote : Notification
    {
        public InvokeClientNote(string name, object[] body = null)
            : base(name, body)
        {
        }

        public IList<string> IDList
        {
            get;
            set;
        }
    }
}
