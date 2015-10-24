using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (24XX)
    /// </summary>
    public class GoodsCommand
    {
        /// <summary>
        /// 获取单个物品的详细信息.
        /// </summary>
        public const string GetGoodsDetail = "getGoodsDetail";
        public const string GetGoodsDetailR = "o.getGoodsDetailR";

        /// <summary>
        /// 得到商品或道具列表
        /// </summary>
        public const string NPCGoodsList = "nPCGoodsList";
        public const string NPCGoodsListR = "o.nPCGoodsListR";
        /// <summary>
        /// 更新装备的基本信息
        /// </summary>
        public const string UpdateEquip = "updateEquip";
        public const string UpdateEquipR = "o.updateEquipR";
        /// <summary>
        /// 得到装备面板
        /// </summary>
        public const string GetEquipPanel = "getEquipPanel";
        public const string GetEquipPanelR = "o.getEquipPanelR";

        /// <summary>
        /// 捡到物品
        /// </summary>
        public const string MadeGoods = "madeGoods";
        public const string MadeGoodsR = "o.madeGoodsR";

        /// <summary>
        /// 物品使用
        /// </summary>
        public const string UseGoods = "useGoods";
        public const string UseGoodsR = "o.useGoodsR";

        /// <summary>
        /// 换装操作
        /// </summary>
        public const string Dress = "dress";
        public const string DressR = "o.DressR";

        /// <summary>
        /// 卸装操作
        /// </summary>
        public const string Uninstall = "uninstall";
        public const string UninstallR = "a.uninstallR";

        /// <summary>
        /// 道具的销毁
        /// </summary>
        public const string Ruin = "ruin";
        public const string RuinR = "o.ruinR";

        /// <summary>
        /// 买得物品
        /// </summary>
        public const string BuyGoods = "buyGoods";
        public const string BuyGoodsR = "o.buyGoodsR";

        /// <summary>
        /// 卖物品
        /// </summary>
        public const string SellGoods = "sellGoods";
        public const string SellGoodsR = "o.sellGoodsR";

        /// <summary>
        /// 装备维修
        /// </summary>
        public const string EquipRepair = "equipRepair";
        public const string EquipRepairR = "o.equipRepairR";

        /// <summary>
        /// 得到道具
        /// </summary>
        public const string GetFightGoods = "getFightGoods";
        public const string GetFightGoodsR = "o.getFightGoodsR";

        /// <summary>
        /// 得到物品通知客户端
        /// </summary>
        public const string GetGoodsCallR = "o.getGoodsCallR";

        /// <summary>
        /// 系统奖励领取
        /// </summary>
        public const string SystemAward = "systemAward";
        public const string SystemAwardR = "o.systemAwardR";

        /// <summary>
        /// 答题
        /// </summary>
        public const string AnswerAward = "answerAward";
        public const string AnswerAwardR = "v.answerAwardR";

        /// <summary>
        /// 批量使用道具
        /// </summary>
        public const string AllUseGoods = "allUseGoods";
        public const string AllUseGoodsR = "o.allUseGoodsR";

        /// <summary>
        /// 装被洗点
        /// </summary>
        public const string GoodsWashing = "goodsWashing";
        public const string GoodsWashingR = "o.goodsWashingR";

        /// <summary>
        /// 抽奖奖品领取
        /// </summary>
        public const string LotteryAward = "lotteryAward";
        public const string LotteryAwardR = "o.lotteryAwardR";

    }

    public class GoodsReturn
    {
        /// <summary>
        /// 使用装备或道具成功
        /// </summary>
        public const int UseGoodsSuccess = 22900;

        /// <summary>
        /// 表示不存在指定的NPC信息
        /// </summary>
        public const int NoNPC = 22901;
        /// <summary>
        /// NPC基本配置有问题
        /// </summary>
        public const int NPCConfigError = 22902;
        /// <summary>
        /// 对不起，您的包袱剩余空间不足，请在整理包袱后再次操作
        /// </summary>
        public const int BurdenB0Full = 22903;
        /// <summary>
        /// 宠物包袱满
        /// </summary>
        public const int BurdenB1Full = 22904;
        /// <summary>
        /// 任务包袱满
        /// </summary>
        public const int BurdenB3Full = 22905;
        /// <summary>
        /// 不存在该物品
        /// </summary>
        public const int NoGoodsInfo = 22906;
        /// <summary>
        /// 不是装备不能进行换装操作
        /// </summary>
        public const int NoEquipInfo = 22907;
        /// <summary>
        /// 信息配置有问题
        /// </summary>
        public const int EquipError = 22908;
        /// <summary>
        /// 包袱有问题
        /// </summary>
        public const int BurdenError = 22909;

        /// <summary>
        /// 角色等级不足
        /// </summary>
        public const int NoLevel = 22910;
        /// <summary>
        /// 性别不符合穿戴条件
        /// </summary>
        public const int NoSex = 22911;
        /// <summary>
        /// 该角色不能使用
        /// </summary>
        public const int NoRoleID = 22912;
        /// <summary>
        /// 道具不能销毁
        /// </summary>
        public const int NoRuin = 22913;

        /// <summary>
        /// 余额不足
        /// </summary>
        public const int NoRemain = 22914;

        /// <summary>
        /// 销售成功
        /// </summary>
        public const int SellSeccess = 22915;
        /// <summary>
        /// 销售的物品所在格子不正确
        /// </summary>
        public const int NoSellGoodsGrid = 22916;
        /// <summary>
        /// 石币金额不足
        /// </summary>
        public const int NoScore = 22917;
        /// <summary>
        /// 装备完好
        /// </summary>
        public const int NoStamina = 22918;
        /// <summary>
        /// 该装备没有穿不能修理
        /// </summary>
        public const int NoHave = 22919;
        /// <summary>
        /// 销毁成功
        /// </summary>
        public const int RuinSuccess = 22920;
        /// <summary>
        /// 表示已经过期不能使用
        /// </summary>
        public const int TimeLines = 22921;
        /// <summary>
        /// 晶币不足
        /// </summary>
        public const int NoCoin = 22922;
        /// <summary>
        /// 物品不能出售
        /// </summary>
        public const int NoSell = 22923;



        /// <summary>
        /// 奖励不存在
        /// </summary>
        public const int NoAward = 22924;
        /// <summary>
        /// 没有到领奖时间
        /// </summary>
        public const int NoAwardDate = 22925;
        /// <summary>
        /// 角色领奖等级不符合
        /// </summary>
        public const int NoAwardLevel = 22926;
        /// <summary>
        /// 奖励配置不正确
        /// </summary>
        public const int AwardConfigError = 22927;
        /// <summary>
        /// 该奖励已经领取
        /// </summary>
        public const int IsAward = 22928;
        /// <summary>
        /// 该奖励不需要选择
        /// </summary>
        public const int NoSelect = 22929;
        /// <summary>
        /// 选择奖励不存在
        /// </summary>
        public const int SelectNoAward = 22930;
        /// <summary>
        /// 可得到宠物经验,你没有带领的宠物，不能领取该奖励!
        /// </summary>
        public const int NoPetExp = 22931;
        /// <summary>
        /// 领取奖励成功
        /// </summary>
        public const int AwardSuccess = 22932;

        /// <summary>
        /// 不能在NPC处购买
        /// </summary>
        public const int NPCError = 22933;
        /// <summary>
        /// 过期物品不能出售
        /// </summary>
        public const int ExpiredGood = 22934;



        /// <summary>
        /// 【{0}】礼包配置不对
        /// </summary>
        public const int LiBao1 = 22935;

        /// <summary>
        /// 战斗状态不能使用
        /// </summary>
        public const int UseGoods1 = 22936;
        /// <summary>
        /// 缺少物品
        /// </summary>
        public const int UseGoods2 = 22937;
        /// <summary>
        /// 不需要补充
        /// </summary>
        public const int UseGoods3 = 22938;
        /// <summary>
        /// 不能越级学习,或者你已拥有此技能
        /// </summary>
        public const int UseGoods4 = 22939;
        /// <summary>
        /// 守护战争或夺宝奇兵中不能使用回城羽毛
        /// </summary>
        public const int UseGoods5 = 22940;
        /// <summary>
        /// 只有队长可以使用回城羽毛
        /// </summary>
        public const int UseGoods6 = 22941;
        /// <summary>
        /// 本场景不可使用回城羽毛
        /// </summary>
        public const int UseGoods7 = 22942;
        /// <summary>
        /// 今天的日常任务已经扩充到上限最大值，请明天再使用探险者石板！
        /// </summary>
        public const int UseGoods8 = 22943;
        /// <summary>
        /// 不能双击使用
        /// </summary>
        public const int UseGoods9 = 22944;

        /// <summary>
        /// 没有出战的宠物不能使用
        /// </summary>
        public const int UseAddExp = 22945;

        /// <summary>
        /// 该道具不能批量使用
        /// </summary>
        public const int AllUseGoods1 = 22946;

        /// <summary>
        /// 包袱格子位置不存在
        /// </summary>
        public const int DateLimit1 = 22947;
        /// <summary>
        /// 操作的位置上已经不存在物品
        /// </summary>
        public const int DateLimit2 = 22948;
        /// <summary>
        /// 物品在包袱中的位置已经变化,请重新使用
        /// </summary>
        public const int DateLimit3 = 22949;
        /// <summary>
        /// 物品在包袱中的位置已经变化,请重新使用
        /// </summary>
        public const int DateLimit4 = 22950;
        /// <summary>
        /// 物品过期不能使用
        /// </summary>
        public const int DateLimit5 = 22951;

        /// <summary>
        /// 你的坐骑已经过期
        /// </summary>
        public const int GoodsIsExpired1 = 22952;
        /// <summary>
        /// 你的时装已经过期
        /// </summary>
        public const int GoodsIsExpired2 = 22953;

        /// <summary>
        /// 该装备不能洗点
        /// </summary>
        public const int GoodsWashing1 = 22954;
        /// <summary>
        /// 洗点成功
        /// </summary>
        public const int GoodsWashing2 = 22955;
        /// <summary>
        /// 洗点石币不足
        /// </summary>
        public const int GoodsWashing3 = 22956;
        /// <summary>
        /// 洗点时保存某属性须要的道具不存在
        /// </summary>
        public const int GoodsWashing4 = 22957;
        /// <summary>
        /// 上传参数不正确
        /// </summary>
        public const int GoodsWashing5 = 22958;
        /// <summary>
        /// 需要的星力不足
        /// </summary>
        public const int GoodsWashing6 = 22959;
        /// <summary>
        /// 星力将超过上限，不能再使用
        /// </summary>
        public const int StarFull = 22960;
        /// <summary>
        /// 请先领取奖励再抽奖
        /// </summary>
        public const int Lottery1 = 22961;
        /// <summary>
        /// 领奖成功
        /// </summary>
        public const int Lottery2 = 22962;

        /// <summary>
        /// 不能所有属性都保护
        /// </summary>
        public const int GoodsWashing7 = 22963;

        /// <summary>
        /// 会员等级不足,不能使用随身商店
        /// </summary>
        public const int NoMember = 22964;

            




        /// <summary>
        /// 答题的基本配置
        /// </summary>
        public const int Answer = 23700;
        /// <summary>
        /// 答题需要道具
        /// </summary>
        public const int AnswerGood = 23701;
        /// <summary>
        /// 答题等级不足
        /// </summary>
        public const int AnswerNoLevel = 23702;
        /// <summary>
        /// 道具数量不足
        /// </summary>
        public const int AnswerNoGood = 23703;
        /// <summary>
        /// 题目不存在
        /// </summary>
        public const int NoAnswer = 23704;
        /// <summary>
        /// 答题达到当前最大数，可使用道具增加!
        /// </summary>
        public const int MaxAnswer = 23705;

        /// <summary>
        /// 已经领奖
        /// </summary>
        public const int IsLoginAward = 23706;

        /// <summary>
        /// 已达使用限制
        /// </summary>
        public const int UserLimit = 23707;
        /// <summary>
        /// 不存在坐骑
        /// </summary>
        public const int NoMounts = 23708;

        /// <summary>
        /// 已经存在坐骑
        /// </summary>
        public const int IsMounst = 23709;


        /// <summary>
        /// 珍稀物品推送(击败怪物获得)
        /// </summary>
        public const int RareKillApc = 23711;

        /// <summary>
        /// 珍稀物品推送(打开宝箱获得)
        /// </summary>
        public const int RareOpenBox = 23712;

        /// <summary>
        /// 珍稀物品推送(捕获获得)
        /// </summary>
        public const int RareClap = 23713;

        /// <summary>
        /// 珍稀物品推送(提取获得)
        /// </summary>
        public const int RareDrawCard = 23714;
    }
}
