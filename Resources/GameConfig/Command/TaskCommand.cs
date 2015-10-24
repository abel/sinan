using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// 任务触发指令(37XX)
    /// </summary>
    public class TaskCommand
    {
        /// <summary>
        /// 玩家属性发生变化触发
        /// </summary>
        public const string PlayerActivation = "PlayerActivation";

        /// <summary>
        /// 使用道具触发
        /// </summary>
        public const string PropsActivation = "PropsActivation";
        /// <summary>
        /// 宠物属性发生变化触发
        /// </summary>
        public const string PetsActivation = "PetsActivation";
        /// <summary>
        /// 任务完成触发新任务
        /// </summary>
        public const string TaskFinish = "TaskFinish";
        /// <summary>
        /// 任务触发成功通知客户端
        /// </summary>
        public const string TaskActivationR = "i.taskNewR";





        /// <summary>
        /// 得到玩家任务列表
        /// </summary>
        public const string PlayerTaskList = "playerTaskList";
        public const string PlayerTaskListR = "i.playerTaskListR";

        /// <summary>
        /// 更新任务
        /// </summary>
        public const string UpdateTask = "updateTask";
        public const string UpdateTaskR = "i.updateTaskR";


        /// <summary>
        /// 战斗结束任务更新
        /// </summary>
        public const string FightingTask = "fightingTask";
        public const string FightingTaskR = "i.fightingTaskR";
        /// <summary>
        /// 任务完成领奖
        /// </summary>
        public const string Award = "award";
        public const string AwardR = "i.awardR";

        /// <summary>
        /// 玩家任务的放弃
        /// </summary>
        public const string Giveup = "giveup";
        public const string GiveupR = "i.giveupR";

        /// <summary>
        /// 是否显示
        /// </summary>
        public const string IsShow = "isShow";
        public const string IsShowR = "i.isShowR";

        /// <summary>
        /// 任务采集
        /// </summary>
        public const string TaskCollect = "taskCollect";
        public const string TaskCollectR = "i.taskCollectR";

        /// <summary>
        /// 任务道具发生变化
        /// </summary>
        public const string TaskGoods = "taskGoods";
    }

    public class TaskReturn 
    {
        /// <summary>
        /// 放弃任务成功
        /// </summary>
        public const int TaskGiveupSuccess = 22000;
        /// <summary>
        /// 错误任务
        /// </summary>
        public const int TaskError = 22001;
        /// <summary>
        /// 接任务失败
        /// </summary>
        public const int TaskRev = 22002;
        /// <summary>
        /// 该任务类型不能放弃
        /// </summary>
        public const int TaskMainGiveup = 22003;
        /// <summary>
        /// 任务已经完成不能放弃
        /// </summary>
        public const int TaskNoGiveup = 22004;
        /// <summary>
        /// 交接NPC任务不正确
        /// </summary>
        public const int TaskNPCError = 22005;
        /// <summary>
        /// 完成任务所在场景不正确
        /// </summary>
        public const int TaskSceneError = 22006;
        /// <summary>
        /// 任务已经达到完成条件,可以领奖不需要继续更新
        /// </summary>
        public const int TaskFinishTerm = 22007;
        /// <summary>
        /// 没有完成任务不能领奖
        /// </summary>
        public const int TaskAwardFail = 22008;
        /// <summary>
        /// 包袱已满，任务不能够完成
        /// </summary>
        public const int BurdenFull = 22009;
        /// <summary>
        /// 选择奖励不正确
        /// </summary>
        public const int SelectAwardError = 22010;
        /// <summary>
        /// 接收任务等级不足
        /// </summary>
        public const int TaskRevNoLevel = 22011;
        /// <summary>
        /// 有异常
        /// </summary>
        public const int TaskExpetion = 22012;
        /// <summary>
        /// 采集场景不正确
        /// </summary>
        public const int CollectSceneError = 22013;
        /// <summary>
        /// 参数错误
        /// </summary>
        public const int TaskPareError = 22014;
        /// <summary>
        /// 得到日常任务的基本配置
        /// </summary>
        public const int DayTaskConfig = 22015;
        /// <summary>
        /// NPC不存在
        /// </summary>
        public const int Npc1 = 22016;
        /// <summary>
        /// 该场景不能操作该任务
        /// </summary>
        public const int Npc2 = 22017;
        /// <summary>
        /// 距离NPC太远不能操作该任务
        /// </summary>
        public const int Npc3 = 22018;
        /// <summary>
        /// 周循环最大环数
        /// </summary>
        public const int LoopMax = 22019;
    }
}
