using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 进入限制
    /// </summary>
    public interface ILimit
    {
        /// <summary>
        /// 检查是否满足条件
        /// </summary>
        /// <param name="player"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool Check(PlayerBusiness player, out string msg);

        /// <summary>
        /// 执行扣除操作
        /// </summary>
        /// <param name="player"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool Execute(PlayerBusiness player, out string msg);

        /// <summary>
        /// 回滚扣除
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        bool Rollback(PlayerBusiness player);
    }
}
