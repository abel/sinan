using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (29XX)
    /// </summary>
    public class MessageCommand
    {
        /// <summary>
        /// 通过连接ID调用客户端的方法
        /// </summary>
        public const string InvokeClientByConnectID = "InvokeClientByConnectID";

        /// <summary>
        /// 根据用户ID调用客户端的方法
        /// </summary>
        public const string InvokeClientByUserID = "InvokeClientByUserID";

        /// <summary>
        /// 根据玩家ID调用客户端的方法
        /// </summary>
        public const string InvokeClientByPlayerID = "InvokeClientByPlayerID";

        /// <summary>
        /// 根据场景ID调用客户端的方法
        /// </summary>
        public const string InvokeClientBySceneID = "InvokeClientBySceneID";

    }
}
