
namespace Sinan.PartModule.Business
{
    public class PartType
    {
        /// <summary>
        /// 充值附送
        /// 【达到充值金额,送的物品.可重复领取(领取后会扣除已领奖的部分).多组只能选一种.】
        /// </summary>
       public const string CoinSupp="CoinSupp";
        /// <summary>
        /// 充值达到
        /// 【在指定时间内充值金额达到一定数量,可领取对应的奖品】
        /// </summary>
        public const string CoinAchieve = "CoinAchieve";


        /// <summary>
        /// 等级达到
        /// 【在指定时间内达到一定等级,可领取对应等级的奖品】
        /// </summary>
        public const string LevelAchieve = "LevelAchieve";
        /// <summary>
        /// 等级附加
        /// 【在老服上至少有一个角色等级大于等于指定等级的玩家】
        /// </summary>
        public const string LevelSupp = "LevelSupp";
        /// <summary>
        /// 等级排行(冠军赛)        
        /// </summary>
        public const string LevelRank = "LevelRank";
        /// <summary>
        /// 物品兑换物品
        /// </summary>
        public const string Exchange = "Exchange";
        /// <summary>
        /// 黄钻奖励
        /// </summary>
        public const string Yellow = "Yellow";
        /// <summary>
        /// 每日奖励活动
        /// </summary>
        public const string NowAward = "NowAward";
    }
}
