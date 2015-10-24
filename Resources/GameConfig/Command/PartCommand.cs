using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (32XX)
    /// </summary>
    public class PartCommand
    {
        /// <summary>
        /// 返回所有活动对象
        /// </summary>
        public const string PartsR = "v.partsR";

        /// <summary>
        /// 活动开始
        /// </summary>
        public const string PartStartR = "v.partStartR";

        /// <summary>
        /// 活动结束
        /// </summary>
        public const string PartEndR = "v.partEndR";

        /// <summary>
        /// 夺宝奇兵家庭奖励
        /// </summary>
        public const string RobFamilyAward = "RobFamilyAward";

        /// <summary>
        /// 合成光环
        /// </summary>
        public const string Aura = "aura";

        /// <summary>
        /// 合成光环返回
        /// </summary>
        public const string AuraR = "v.auraR";

        /// <summary>
        /// 光环改变
        /// </summary>
        public const string AuraChangeR = "v.auraChangeR";

        /// <summary>
        /// 上缴光环
        /// </summary>
        public const string TurnAura = "turnAura";

        /// <summary>
        /// 上缴结果
        /// </summary>
        public const string TurnAuraR = "v.turnAuraR";



        /// <summary>
        /// 活动相关物品兑换
        /// </summary>
        public const string PartExchange = "partExchange";
        public const string PartExchangeR = "v.partExchangeR";

        /// <summary>
        /// 充值
        /// </summary>
        public const string Recharge = "recharge";


        /// <summary>
        /// 活动明细
        /// </summary>
        //public const string PartDetails = "partDetails";
        //public const string PartDetailsR = "v.partDetailsR";

        /// <summary>
        /// 得到活动领取明细
        /// </summary>
        public const string PartReceive = "partReceive";
        public const string PartReceiveR = "v.partReceiveR";
    }

    /// <summary>
    /// 活动提示
    /// </summary>
    public class PartReturn 
    {
        /// <summary>
        /// 该活动不存在
        /// </summary>
        public static int Part1 = 32001;
        /// <summary>
        /// 该活动还没有开始不能兑奖
        /// </summary>
        public static int Part2 = 32002;
        /// <summary>
        /// 表示活动领奖时间已经结束,不能再领奖
        /// </summary>
        public static int Part3 = 32003;
        /// <summary>
        /// 活动其间充值金额不足
        /// </summary>
        public static int Part4 = 32004;
        /// <summary>
        /// 该项活动已经领取，不能重复领取
        /// </summary>
        public static int Part5 = 32005;
        /// <summary>
        /// 活动配置数据不正确
        /// </summary>
        public static int Part6 = 32006;
        /// <summary>
        /// 包袱满不能领取
        /// </summary>
        public static int Part7 = 32007;
        /// <summary>
        /// 活动其间充值金额不足
        /// </summary>
        public static int Part8 = 32008;
        /// <summary>
        /// 非老区用户不能领取该奖励
        /// </summary>
        public static int Part9 = 32009;
        /// <summary>
        /// 没有达到领励排行
        /// </summary>
        public static int Part10 = 32010;
        /// <summary>
        /// 没有达到领励条件
        /// </summary>
        public static int Part11 = 32011;
        /// <summary>
        /// 兑换需要的道具数量不足
        /// </summary>
        public static int Part12 = 32012;
        /// <summary>
        /// 没有达到兑换条件
        /// </summary>
        public static int Part13 = 32013;
        /// <summary>
        /// 非黄钻用户不能领取该奖励
        /// </summary>
        public static int Part14 = 32014;
        /// <summary>
        /// 领奖成功
        /// </summary>
        public static int Part15 = 32015;
        /// <summary>
        /// 非年费黄钻不能领取该奖励
        /// </summary>
        public static int Part16 = 32016;
        /// <summary>
        /// 
        /// </summary>
        public static int Part17 = 32017;
    }

    /// <summary>
    /// 活动消息内容
    /// </summary>
    public enum PartMsgType
    {
        /// <summary>
        /// PK战胜场数
        /// </summary>
        PKWin = 30000,

        /// <summary>
        /// 光环PK战败
        /// </summary>
        AuraLose = 30001,

        /// <summary>
        /// 光环被怪杀死
        /// </summary>
        AuraLoseApc = 30002,

        /// <summary>
        /// 上缴光环给阿波罗
        /// </summary>
        TurnAura = 30003,

        /// <summary>
        /// 元素不足时合成光环失败
        /// </summary>
        AuraLack = 30004,

        /// <summary>
        /// 成功合成光环
        /// </summary>
        AuraCompose = 30005,

        /// <summary>
        /// 元素光环已被其它人合成
        /// </summary>
        AuraLate = 30006,

        /// <summary>
        /// 元素使者掉线
        /// </summary>
        AuraLoseDis = 30007,

        /// <summary>
        /// 元素使者进入地图
        /// </summary>
        AuraCross = 30008,

        /// <summary>
        /// 活动人数
        /// </summary>
        PartCount = 30009,
    }
}
