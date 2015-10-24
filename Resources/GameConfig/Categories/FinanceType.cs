using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Entity
{
    public enum FinanceType
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 1000,

        /// <summary>
        /// 开箱
        /// </summary>
        OpenBox = 1001,

        /// <summary>
        /// 进入场景
        /// </summary>
        IntoScene = 1002,

        /// <summary>
        /// 打怪
        /// </summary>
        KillApc = 1003,
        /// <summary>
        /// 任务
        /// </summary>
        Task = 1004,
        /// <summary>
        /// GM
        /// </summary>
        GM = 1005,
        /// <summary>
        /// 商城购买
        /// </summary>
        MallBuy = 1006,
        /// <summary>
        /// 宠物质资
        /// </summary>
        PetZiZhi = 1007,
        /// <summary>
        /// 宠物仓库扩展
        /// </summary>
        ShockPet = 1008,
        /// <summary>
        /// 移炼
        /// </summary>
        ExchangeBao = 1009,
        /// <summary>
        /// 包袱扩展
        /// </summary>
        BurdenExtend = 1010,
        /// <summary>
        /// 镶嵌
        /// </summary>
        Mosaic = 1011,
        /// <summary>
        /// NPC购卖
        /// </summary>
        BuyGoods = 1012,
        /// <summary>
        /// 合成
        /// </summary>
        Fuse = 1013,
        /// <summary>
        /// 打孔
        /// </summary>
        Punch = 1014,
        /// <summary>
        /// 维修
        /// </summary>
        EquipRepair = 1015,
        /// <summary>
        /// 家族创建
        /// </summary>
        FamilyCreate = 1016,
        /// <summary>
        /// 邮件提取
        /// </summary>
        ExtractGoods = 1017,
        /// <summary>
        /// 邮费
        /// </summary>
        EmailFee = 1018,
        /// <summary>
        /// 技能学习
        /// </summary>
        SkillStudy = 1019,
        /// <summary>
        /// 交易
        /// </summary>
        EnterDeal = 1020,
        /// <summary>
        /// 出售
        /// </summary>
        AuctionSell = 1021,
        /// <summary>
        /// 采集
        /// </summary>
        HomePluck = 1022,
        /// <summary>
        /// 连续登录所得奖励
        /// </summary>
        LoginDaysAward = 1023,
        /// <summary>
        /// 礼包
        /// </summary>
        LiBao = 1024,
        /// <summary>
        /// 购买一口价
        /// </summary>
        AuctionBuy = 1025,
        /// <summary>
        /// 卖商品得到游戏币(道具与装备)
        /// </summary>
        SellGoods = 1026,
        /// <summary>
        /// 竞价
        /// </summary>
        AuctionBid = 1027,
        /// <summary>
        /// 每日奖励
        /// </summary>
        SystemAward = 1028,
        /// <summary>
        /// 宠物驯化
        /// </summary>
        HomePetKeep = 1029,
        /// <summary>
        /// 宠物回收
        /// </summary>
        PetBack = 1030,
        /// <summary>
        /// 宠物技能槽
        /// </summary>
        ShockSkill = 1031,
        /// <summary>
        /// 吸收
        /// </summary>
        StockingAward0 = 1032,
        /// <summary>
        /// 照顾
        /// </summary>
        StockingAward1 = 1033,
        /// <summary>
        /// 遗忘技能
        /// </summary>
        OblivionSkill = 1034,

        /// <summary>
        /// 参加活动
        /// </summary>
        Part = 1035,
        /// <summary>
        /// 宠物技能提取
        /// </summary>
        DrawPetSkill = 1036,
        /// <summary>
        /// 一键照顾
        /// </summary>
        StockingAward2 = 1037,

        /// <summary>
        /// 夺宝奇兵PK
        /// </summary>
        RobPK = 1038,

        /// <summary>
        /// 使用道具
        /// </summary>
        UseGoods = 1039,

        /// <summary>
        /// 获得成就
        /// </summary>
        Effort = 1040,
        /// <summary>
        /// 守护战争
        /// </summary>
        Pro = 1041,
        /// <summary>
        /// 每日答题
        /// </summary>
        AnswerAward = 1042,
        /// <summary>
        /// 创建竞技场
        /// </summary>
        CreateArena = 1043,

        /// <summary>
        /// PK失败
        /// </summary>
        PKLost = 1045,
        /// <summary>
        /// 徒弟达成成就
        /// </summary>
        AppFinish = 1046,
        /// <summary>
        /// 消费晶币
        /// </summary>
        ConsumeCoin = 1047,
        /// <summary>
        /// 装备洗点
        /// </summary>
        GoodsWashing = 1048,
        /// <summary>
        /// 冥想
        /// </summary>
        PlayerMeditation=1049,
        /// <summary>
        /// 激活星座
        /// </summary>
        StartStar=1050,
        /// <summary>
        /// 星阵领取
        /// </summary>
        GetStarTroops=1051,
        /// <summary>
        /// 暴星分享
        /// </summary>
        StartStarShared=1052,
        /// <summary>
        /// 分享
        /// </summary>
        FriendShare =1053,
        /// <summary>
        /// 放生
        /// </summary>
        PetRelease = 1054,
        /// <summary>
        /// 星阵离线收益
        /// </summary>
        OfflineTroops=1055,
        /// <summary>
        /// 星阵在线收益
        /// </summary>
        OnlineTroops=1056,

        /// <summary>
        /// Q币或Q点购买晶币
        /// </summary>
        BuyCoin = 2000,
        /// <summary>
        /// 坐骑技能更换
        /// </summary>
        MountsSkillChange=2001
    }
}
