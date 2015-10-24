using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Sinan.FastSocket
{
    /// <summary>
    /// Session工厂,用于控制Session的创建和回收
    /// (可在此添加IP过滤和单IP最大连接数限制)
    /// </summary>
    public interface ISessionFactory
    {
        /// <summary>
        /// 创建Session
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        ISession CreateSession(Socket token);
    }
}
