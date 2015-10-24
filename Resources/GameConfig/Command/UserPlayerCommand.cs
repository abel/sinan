using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (40XX)
    /// </summary>
    public class UserPlayerCommand
    {
        /// <summary>
        /// 用户创建新的玩家
        /// </summary>
        public const string CreatePlayer = "createPlayer";
        public const string CreatePlayerR = "a.createPlayerR";

        /// <summary>
        /// 角色创建成功..
        /// </summary>
        public const string CreatePlayerSuccess = "CreatePlayerSuccess";

        /// <summary>
        /// 自动生成玩家名.
        /// </summary>
        public const string CreatePlayerName = "createPlayerName";
        public const string CreatePlayerNameR = "a.createPlayerNameR";

        /// <summary>
        /// 用户删除自已创建的玩家
        /// </summary>
        public const string DeletePlayer = "delPlayer";
        public const string DeletePlayerR = "a.delPlayerR";

        /// <summary>
        /// 用户恢复删除中的玩家角色
        /// </summary>
        public const string RecoveryPlayer = "recoverPlayer";
        public const string RecoveryPlayerR = "a.recoverPlayerR";

        /// <summary>
        /// 角色删除成功
        /// </summary>
        public const string DeletePlayerSuccess = "DeletePlayerSuccess";

    }

    /// <summary>
    /// 400XX
    /// </summary>
    public class UserPlayerReturn
    {
        /// <summary>
        /// 用户名应为1-16个字符
        /// </summary>
        public const int NameLimit1 = 40001;

        /// <summary>
        /// "不能使用字符:"
        /// </summary>
        public const int NameLimit2 = 40002;

        /// <summary>
        /// "不能包含:"
        /// </summary>
        public const int NameLimit3 = 40003;

        /// <summary>
        /// "名字已存在或系统保留"
        /// </summary>
        public const int NameLimit4 = 40004;

        /// <summary>
        /// "超过创建限制数:"
        /// </summary>
        public const int CreateLimit1 = 40005;

        /// <summary>
        /// "当前区已关闭注册,请选择其它游戏区"
        /// </summary>
        public const int CreateLimit2 = 40006;

    }
}
