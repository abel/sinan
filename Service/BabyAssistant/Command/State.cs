using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.BabyAssistant.Command
{
    public enum State
    {
        /// <summary>
        /// 请求连接
        /// </summary>
        SYN_SENT = 0,
        /// <summary>
        /// 表示监听
        /// </summary>
        LISTENING = 1,
        /// <summary>
        /// 表示建立连接
        /// </summary>
        ESTABLISHED = 2,
        /// <summary>
        /// 访问结束
        /// </summary>
        TIME_WAIT = 3,
    }
}
