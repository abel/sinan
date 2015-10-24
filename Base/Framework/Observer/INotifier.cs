using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Observer
{
    /// <summary>
    /// 邮差.用于订阅和发布消息..
    /// </summary>
    public interface INotifier
    {
        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        bool Subscribe(ISubscriber subscriber);
        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        bool Subscribe(string topic, ISubscriber subscriber);
        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        bool UnSubscribe(ISubscriber subscriber);
        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        bool UnSubscribe(string topic, ISubscriber subscriber);

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="async">是否异步执行</param>
        /// <returns></returns>
        bool Publish(INotification notification, bool async);
    }
}
