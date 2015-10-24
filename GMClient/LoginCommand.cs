using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.GMModule
{
    /// <summary>
    /// (26XX)
    /// </summary>
    public class LoginCommand
    {
        /// <summary>
        /// 用户登录命令
        /// </summary>
        public const string UserLogin = "login";
        public const string UserLoginR = "a.loginR";

        /// <summary>
        /// 角色登录命令..
        /// </summary>
        public const string PlayerLogin = "playerLogin";
        public const string PlayerLoginR = "a.playerLoginR";

        /// <summary>
        /// 角色登录成功命令.
        /// </summary>
        public const string PlayerLoginSuccess = "PlayerLoginSuccess";

        /// <summary>
        /// 角色登录结果
        /// </summary>
        public const string PlayerLoginFailed = "PlayerLoginFailed";
    }

    public enum LoginResult : byte
    {
        /// <summary>
        /// 未知
        /// </summary>
        NONO = 0,

        /// <summary>
        /// 成功(可以创建色角)
        /// </summary>
        Success = 1,

        /// <summary>
        /// 成功(不能创建色角)
        /// </summary>
        Success2 = 2,

        /// <summary>
        /// 不能创建角色
        /// </summary>
        FreezeCreate = 12,

        /// <summary>
        /// 认证失败,请重新登录
        /// </summary>
        Fail = 13,

        /// <summary>
        /// 认证过期
        /// </summary>
        OverTime = 14,

        /// <summary>
        /// 验证出错,请联系客服
        /// </summary>
        ServerErr = 15,

        /// <summary>
        /// IP黑名单或用户黑名单
        /// </summary>
        BlackList = 16,

        /// <summary>
        /// 登录参数不正确
        /// </summary>
        ParaErr = 17,

        /// <summary>
        /// 在其它地方登录,被迫下线
        /// </summary>
        OtherInto = 18,
    }

}
