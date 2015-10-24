using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (10XX)
    /// </summary>
    public class ActivityCommand
    {
        /// <summary>
        /// 每日签到
        /// </summary>
        public const string ActivitySign = "activitySign";
        public const string ActivitySignR = "v.activitySignR";

        /// <summary>
        /// 领奖
        /// </summary>
        public const string ActivityAward = "activityAward";
        public const string ActivityAwardR = "v.activityAwardR";

        /// <summary>
        /// 更新活跃度
        /// </summary>
        public const string CheckActivity = "checkActivity";

        /// <summary>
        /// 守护战争结束
        /// </summary>
        public const string FightProEndR = "v.fightProEndR";
        /// <summary>
        /// 守护战争非战斗得到经验
        /// </summary>
        public const string ProExpR = "v.proExpR";
        /// <summary>
        /// 守护PK中失败
        /// </summary>
        public const string PKLoseR = "v.pkLoseR";
        /// <summary>
        /// 得到守护凭证通知
        /// </summary>
        public const string GetDefendCardR = "v.getDefendCardR";
        /// <summary>
        /// 开宝箱
        /// </summary>
        public const string OpenChestR = "v.openChestR";

        /// <summary>
        /// 签到奖励领取
        /// </summary>
        public const string SignAward = "signAward";
        public const string SignAwardR = "v.signAwardR";

        /// <summary>
        /// 登录奖励
        /// </summary>
        public const string LoginAward = "loginAward";
        public const string LoginAwardR = "v.loginAwardR";

        /// <summary>
        /// VIP每日奖励
        /// </summary>
        public const string VIPDayAward = "vIPDayAward";
        public const string VIPDayAwardR = "v.vIPDayAwardR";

        /// <summary>
        /// VIP套餐兑换
        /// </summary>
        public const string VipExchange = "vipExchange";
        public const string VipExchangeR = "v.vipExchangeR";

        /// <summary>
        /// 在线奖励
        /// </summary>
        public const string OnlineAward = "onlineAward";
        public const string OnlineAwardR = "v.onlineAwardR";

    }

    public class ActivityReturn
    {
        /// <summary>
        /// 守护活动胜利通告
        /// </summary>
        public const int Pro = 23800;
        /// <summary>
        /// 守护PK失败
        /// </summary>
        public const int ProFail = 23801;
        /// <summary>
        /// 守护战争开始
        /// </summary>
        public const int ProStart = 23802;
        /// <summary>
        /// 守护战争中失败，守护凭证丢失
        /// </summary>
        public const int ProApcFail = 23803;

        /// <summary>
        /// 今日已经签到
        /// </summary>
        public const int SignMsg1 = 23804;
        /// <summary>
        /// 签到成功
        /// </summary>
        public const int SignMsg2 = 23805;



        /// <summary>
        /// 数据不正确
        /// </summary>
        public const int SignAward1 = 23806;
        /// <summary>
        /// 签到次数没有达到条件不能领取该奖励
        /// </summary>
        public const int SignAward2 = 23807;
        /// <summary>
        /// 该奖励已经领取不能再领取
        /// </summary>
        public const int SignAward3 = 23808;
        /// <summary>
        /// 包袱满，请先整理你的包袱再领取
        /// </summary>
        public const int SignAward4 = 23809;
        /// <summary>
        /// 没有达到条件
        /// </summary>
        public const int SignAward5 = 23810;
        /// <summary>
        /// 签到领奖成功
        /// </summary>
        public const int SignAward6 = 23811;


        /// <summary>
        /// 数据不正确
        /// </summary>
        public const int ActivityAward1 = 23812;
        /// <summary>
        /// 上传参数不正确
        /// </summary>
        public const int ActivityAward2 = 23813;
        /// <summary>
        /// 奖励已经领取
        /// </summary>
        public const int ActivityAward3 = 23814;
        /// <summary>
        /// 活跃度不足不能领奖
        /// </summary>
        public const int ActivityAward4 = 23815;
        /// <summary>
        /// 包袱满，请先整理你的包袱
        /// </summary>
        public const int ActivityAward5 = 23816;
        /// <summary>
        /// 不能奖励
        /// </summary>
        public const int ActivityAward6 = 23817;
        /// <summary>
        /// 没有奖励
        /// </summary>
        public const int ActivityAward7 = 23818;
        /// <summary>
        /// 领奖成功
        /// </summary>
        public const int ActivityAward8 = 23819;


        /// <summary>
        /// 数据不正确
        /// </summary>
        public const int LoginAward1 = 23820;
        /// <summary>
        /// 今天不能领奖
        /// </summary>
        public const int LoginAward2 = 23821;
        /// <summary>
        /// 今天已经领奖
        /// </summary>
        public const int LoginAward3 = 23822;
        /// <summary>
        /// 包袱满，请先整理你的包袱
        /// </summary>
        public const int LoginAward4 = 23823;
        /// <summary>
        /// 不能领奖
        /// </summary>
        public const int LoginAward5 = 23824;
        /// <summary>
        /// 连续登录领奖成功
        /// </summary>
        public const int LoginAward6 = 23825;


        /// <summary>
        /// VIP等级不足
        /// </summary>
        public const int VIPDayAward1 = 23826;
        /// <summary>
        /// 数据不正确
        /// </summary>
        public const int VIPDayAward2 = 23827;
        /// <summary>
        /// 包袱满，请先整理你的包袱
        /// </summary>
        public const int VIPDayAward3 = 23828;
        /// <summary>
        /// 今日已经领取
        /// </summary>
        public const int VIPDayAward4 = 23829;
        /// <summary>
        /// 不能奖励
        /// </summary>
        public const int VIPDayAward5 = 23830;
        /// <summary>
        /// VIP每日登录领奖成功
        /// </summary>
        public const int VIPDayAward6 = 23831;

        /// <summary>
        /// 非VIP不能领取该奖励
        /// </summary>
        public const int VipExchange1 = 23832;
        /// <summary>
        /// 数据不正确
        /// </summary>
        public const int VipExchange2 = 23833;
        /// <summary>
        /// 兑换次数已满
        /// </summary>
        public const int VipExchange3 = 23834;
        /// <summary>
        /// 包袱满，请先整理你的包袱
        /// </summary>
        public const int VipExchange4 = 23835;
        /// <summary>
        /// 兑换需要道具不足
        /// </summary>
        public const int VipExchange5 = 23836;
        /// <summary>
        /// 兑换失败
        /// </summary>
        public const int VipExchange6 = 23837;
        /// <summary>
        /// 兑换成功
        /// </summary>
        public const int VipExchange7 = 23838;

        /// <summary>
        /// 参数不正确
        /// </summary>
        public const int OnlineAward1 = 23839;

        /// <summary>
        /// 在线时间不足
        /// </summary>
        public const int OnlineAward2 = 23840;
        /// <summary>
        /// 配置有问题
        /// </summary>
        public const int OnlineAward3 = 23841;
        /// <summary>
        /// 已经领取
        /// </summary>
        public const int OnlineAward4 = 23842;
        /// <summary>
        /// 包袱满,不能进行该操作
        /// </summary>
        public const int OnlineAward5 = 23843;
        /// <summary>
        /// 
        /// </summary>
        public const int OnlineAward6 = 23844;
        /// <summary>
        /// 
        /// </summary>
        public const int OnlineAward7 = 23845;
    }

    public enum ActivityType
    {
        /// <summary>
        /// 每日签到
        /// </summary>
        Sign=0,
        /// <summary>
        /// 在线时间
        /// </summary>
        LoginTime=1,
        /// <summary>
        /// APC数量
        /// </summary>
        APCCount=2,
        /// <summary>
        /// 知识问答
        /// </summary>
        WenDa=3,
        /// <summary>
        /// 照顾
        /// </summary>
        ZhaoGu=4,
        /// <summary>
        /// 家园生产次数
        /// </summary>
        ShengChan=5,
        /// <summary>
        /// 日常任务
        /// </summary>
        DayTask=6,
        /// <summary>
        /// 环式任务完成
        /// </summary>
        LoopTask=7,
        /// <summary>
        /// 秘境Boss
        /// </summary>
        FuBen=8,
        /// <summary>
        /// 守护战争
        /// </summary>
        Pro=9,
        /// <summary>
        /// 勇闯天梯
        /// </summary>
        TianTi=10,
        /// <summary>
        /// 好友祝福
        /// </summary>
        FriendsBless=11,
        /// <summary>
        /// 邀请QQ好友
        /// </summary>
        InvitedFriends=12,
        /// <summary>
        /// 进入副本次数
        /// </summary>
        FuBenCount=13
    }
}
