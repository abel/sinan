using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.FastSocket
{
    /// <summary>
    /// 处理解码后的命令
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICommandProcessor<T>
    {
        /// <summary>
        /// 处理命令
        /// </summary>
        /// <param name="session"></param>
        /// <param name="command"></param>
        bool Execute(ISession session, T command);
    }
}
