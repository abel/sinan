using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// 战斗命令(20XX)
    /// </summary>
    public class FightCommand
    {
        /// <summary>
        /// 攻打任务怪
        /// </summary>
        public const string FightTaskApc = "fightTaskApc";

        /// <summary>
        /// 打明怪
        /// </summary>
        public const string FightSceneApc = "fightSceneApc";

        /// <summary>
        /// 请求打怪错误
        /// </summary>
        public const string FightFalseR = "g.fightFalseR";

        /// <summary>
        /// 玩家退出
        /// </summary>
        public const string PlayerExit = "PlayerExit";

        /// <summary>
        /// 进入战斗场景
        /// </summary>
        public const string IntoBattle = "IntoBattle";

        /// <summary>
        /// 进入PK战斗场景
        /// </summary>
        public const string IntoBattlePK = "IntoBattlePK";

        /// <summary>
        /// 退出战斗场景
        /// </summary>
        public const string Retire = "RetireFight";

        /// <summary>
        /// 开始战斗
        /// </summary>
        public const string StartFight = "g.startFightR";

        /// <summary>
        /// 战斗行为(用户的攻击动作)
        /// </summary>
        public const string FightAction = "fightAction";

        /// <summary>
        /// 客户端准备好战斗场景
        /// </summary>
        public const string ReadyFight = "readyFight";

        /// <summary>
        /// 返回值
        /// </summary>
        public const string FightActionR = "g.fightActionR";

        /// <summary>
        /// 回合结束..
        /// </summary>
        public const string FightTurnEndR = "g.fightTurnEndR";

        /// <summary>
        /// 准备战斗
        /// </summary>
        public const string FightPreparedR = "g.fightPreparedR";

        /// <summary>
        /// 战斗播放完成
        /// </summary>
        public const string FightPlayerOver = "fightPlayOver";

        /// <summary>
        /// 结束战斗.
        /// </summary>
        public const string FightEndR = "g.fightEndR";

        /// <summary>
        /// 强制PK
        /// </summary>
        public const string FightPK = "fightPK";

        /// <summary>
        /// 请求切磋
        /// </summary>
        public const string FightCC = "fightCC";
        public const string FightCCR = "g.fightCCR";

        /// <summary>
        /// 回复请求
        /// </summary>
        public const string FightReplyCC = "fightReplyCC";
        public const string FightReplyCCR = "g.fightReplyCCR";

        /// <summary>
        /// 自动战斗
        /// </summary>
        public const string AutoFight = "autoFight";
        public const string AutoFightR = "g.autoFightR";
    }
}
