using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 动作类型
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// 技能攻击
        /// </summary>
        JiNeng = 0,
        /// <summary>
        /// 物理攻击
        /// </summary>
        WuLi = 1,
        /// <summary>
        /// 抓捕
        /// </summary>
        ZhuaPu = 2,
        /// <summary>
        /// 防御
        /// </summary>
        FangYu = 3,
        /// <summary>
        /// 保护
        /// </summary>
        Protect = 4,
        /// <summary>
        /// 逃跑
        /// </summary>
        TaoPao = 5,
        /// <summary>
        /// 道具
        /// </summary>
        DaoJu = 6,
        /// <summary>
        /// 换宠
        /// </summary>
        ChangePet = 7,
    }
}
