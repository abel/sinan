using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Observer
{
    /// <summary>
    /// 消息订阅者接口..
    /// </summary>
    public interface ISubscriber
    {
        /// <summary>
        /// 所有需要订阅的消息主题
        /// </summary>
        /// <returns></returns>
        IList<string> Topics();

        /// <summary>
        /// 接收到消息时执行的方法
        /// </summary>
        /// <param name="note"></param>
        void Execute(INotification note);

        /// <summary>
        /// 安全执行消息方法
        /// </summary>
        /// <param name="note"></param>
        void SafeExecute(object note);
    }


}
