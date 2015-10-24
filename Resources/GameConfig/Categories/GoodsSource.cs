using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Entity
{
    public enum GoodsSource : byte
    {
        None = 0,

        /// <summary>
        /// 杀怪
        /// </summary>
        KillApc = 1,

        /// <summary>
        /// 捕捉
        /// </summary>
        Clap = 2,

        /// <summary>
        /// 开箱
        /// </summary>
        OpenBox = 3,

        /// <summary>
        /// 提取技能卡
        /// </summary>
        DrawCard = 4,

        /// <summary>
        /// 战斗
        /// </summary>
        FightEnd = 5,
        /// <summary>
        /// 进入守护战争
        /// </summary>
        Pro = 6,
        /// <summary>
        /// 邮件
        /// </summary>
        ExtractGoods = 7,
        /// <summary>
        /// 采集
        /// </summary>
        Collection = 8,
        /// <summary>
        /// 每日奖励
        /// </summary>
        SystemAward = 9,
        /// <summary>
        /// 任务奖励
        /// </summary>
        TaskAward = 10,
        /// <summary>
        /// 接收任务时发方道具
        /// </summary>
        TaskFinish10010 = 11,
        /// <summary>
        /// 1点券购买
        /// </summary>
        BondBuy = 12,
        /// <summary>
        /// NPC买得商品(道具与装备)
        /// </summary>
        BuyGoods = 13,
        /// <summary>
        /// 合成
        /// </summary>
        Fuse = 14,
        /// <summary>
        /// 任务集采
        /// </summary>
        TaskFinish10009 = 15,
        /// <summary>
        /// 商城购买
        /// </summary>
        BuyMallGoods = 16,
        /// <summary>
        /// 任务放弃
        /// </summary>
        TaskGiveup = 17,
        /// <summary>
        /// 出师
        /// </summary>
        OutMaster = 18,
        /// <summary>
        /// 活跃领奖
        /// </summary>
        ActivityAward = 19,
        /// <summary>
        /// 签到奖励
        /// </summary>
        SignAward = 20,
        /// <summary>
        /// 礼包
        /// </summary>
        LiBao = 21,
        /// <summary>
        /// GM增送
        /// </summary>
        GMGet = 22,
        /// <summary>
        /// 连续登录奖励
        /// </summary>
        LoginAward = 23,
        /// <summary>
        /// VIP每日登录奖励
        /// </summary>
        VIPDayAward = 24,
        /// <summary>
        /// Vip兑换
        /// </summary>
        VipExchange = 25,
        /// <summary>
        /// 好友祝福
        /// </summary>
        FriendsBless = 26,
        /// <summary>
        /// 充值领奖[不能重复]
        /// </summary>
        CoinAchieve = 27,
        /// <summary>
        /// 打指定的怪得到指定的物品给指定的NPC
        /// </summary>
        TaskFinish10003 = 28,
        /// <summary>
        /// 星力瓶
        /// </summary>
        Bottles = 29,
        /// <summary>
        /// 充值奖励[可重复领取]
        /// </summary>
        CoinSupp = 30,
        /// <summary>
        /// 等级达到
        /// </summary>
        LevelAchieve = 31,
        /// <summary>
        /// 老服用户
        /// </summary>
        LevelSupp = 32,
        /// <summary>
        /// 等级排行(冠军赛)   
        /// </summary>
        LevelRank = 33,
        /// <summary>
        /// 物品兑换
        /// </summary>
        Exchange = 34,
        /// <summary>
        /// 黄钻
        /// </summary>
        Yellow = 35,
        /// <summary>
        /// 抽取奖励
        /// </summary>
        LotteryAward = 36,
        /// <summary>
        /// 晶币商城
        /// </summary>
        MallCoin = 37,
        /// <summary>
        /// 骨币商城
        /// </summary>
        MallBond = 38,
        /// <summary>
        /// 战绩商城
        /// </summary>
        MallFightValue = 39,




        /// <summary>
        /// 喂养
        /// </summary>
        FeedPets = 40,
        /// <summary>
        /// 质资提升
        /// </summary>
        ZiZhiPets = 41,
        /// <summary>
        /// 激活兽栏
        /// </summary>
        ShockPetGroove = 42,
        /// <summary>
        /// 宠物仓库扩展
        /// </summary>
        PetExtend = 43,
        /// <summary>
        /// 激活空位槽
        /// </summary>
        ShockSkill = 44,
        /// <summary>
        /// 诱宠
        /// </summary>
        StealPet = 45,
        /// <summary>
        /// 宠物保护
        /// </summary>
        PetProtection = 46,
        /// <summary>
        /// 宠物赠送
        /// </summary>
        PetPresent = 47,
        /// <summary>
        /// 一键喂养
        /// </summary>
        FeedPetsAll = 48,
        /// <summary>
        /// 打孔
        /// </summary>
        Punch = 49,
        /// <summary>
        /// 镶嵌
        /// </summary>
        Mosaic = 50,
        /// <summary>
        /// 装被洗点
        /// </summary>
        GoodsWashing = 51,
        /// <summary>
        /// 道具及装备的销毁
        /// </summary>
        Ruin = 52,

        /// <summary>
        /// 检查坐骑和时装是否过期
        /// </summary>
        RemoveEquips = 53,
        /// <summary>
        /// 双击使用
        /// </summary>
        DoubleUse = 54,
        /// <summary>
        /// 出售
        /// </summary>
        SellGoods = 55,
        /// <summary>
        /// 答题
        /// </summary>
        AnswerAward = 56,
        /// <summary>
        /// 召唤师傅
        /// </summary>
        SummonMaster = 57,
        /// <summary>
        /// 任务回收
        /// </summary>
        TaskFinish10004 = 58,
        /// <summary>
        /// 对话过程中回收道具,
        /// 回收成功表示任务完成条件达成
        /// </summary>
        TaskFinish10006 = 59,
        /// <summary>
        /// 包袱或仓库扩展[还要求判断是否要求晶币消费]
        /// </summary>
        BurdenExtend = 60,
        /// <summary>
        /// 星座激活
        /// </summary>
        StartStar = 61,
        /// <summary>
        /// 复活
        /// </summary>
        DaoJuRevive = 62,
        /// <summary>
        /// 加血
        /// </summary>
        DaoJuSupply = 63,
        /// <summary>
        /// 竞技场物品使用
        /// </summary>
        ArenaGoodsPet = 64,
        /// <summary>
        /// 守护战斗中断开，移除所有道具
        /// </summary>
        ProExit = 65,
        /// <summary>
        /// 守护PK
        /// </summary>
        ProPK = 66,
        /// <summary>
        /// 合成光环
        /// </summary>
        CombinAura = 67,

        /// <summary>
        /// 宠物放生
        /// </summary>
        PetRelease = 68,
        /// <summary>
        /// 宠物回收(出售)
        /// </summary>
        PetBack = 69,
        /// <summary>
        /// GM移除
        /// </summary>
        GM = 70,
        /// <summary>
        /// 充值
        /// </summary>
        Recharge = 71,
        /// <summary>
        /// 登录天数
        /// </summary>
        LoginCZD = 72,
        /// <summary>
        /// 宠物进化技能升级
        /// </summary>
        EvoSkillUp = 73,
        /// <summary>
        /// 宠物进化技能变更
        /// </summary>
        EvoSkillChange = 74,
        /// <summary>
        /// 家园生产
        /// </summary>
        HomeProduce = 75,

        /// <summary>
        /// 家族Boss领奖
        /// </summary>
        FamilyBoss = 76,
        /// <summary>
        /// 坐骑进化
        /// </summary>
        MountsUp = 77,
        /// <summary>
        /// 邮件发送
        /// </summary>
        SendEmail = 78,
        /// <summary>
        /// 坐骑技能更换
        /// </summary>
        MountsSkillChange = 79,
        /// <summary>
        /// 拍卖
        /// </summary>
        AuctionSell = 80,
        /// <summary>
        /// 夺宝骑兵PK
        /// </summary>
        RobPK = 81,
        /// <summary>
        /// 在线奖励
        /// </summary>
        OnlineAward = 82,

        /// <summary>
        /// 地图任意传送
        /// </summary>
        MapTrans = 83,
        /// <summary>
        /// 一价购买
        /// </summary>
        AuctionBuy=84,
        /// <summary>
        /// 竞价取得物品
        /// </summary>
        AuctionBid=85
    }
}
