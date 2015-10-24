using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.FastSocket
{
    /// <summary>
    /// 处理接收到的数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBufferProcessor
    {
        /// <summary>
        /// 处理命令
        /// </summary>
        /// <param name="session"></param>
        /// <param name="data"></param>
        int Execute(ISession session, Sinan.Collections.BytesSegment data);
    }
}
