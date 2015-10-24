using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Log
{
    /// <summary>
    /// 用于区分从哪个业务平台进入应用
    /// Qzone 为 1
    /// 腾讯朋友为 2
    /// 腾讯微博为 3
    /// Q+ 平台为 4
    /// 财付通开放平台为 5
    /// QQGame 为 10
    /// 由客户端提上来
    /// </summary>
    public enum Domain
    {
        /// <summary>
        ///  为 Qzone空间1
        /// </summary>
        Qzone = 1,

        /// <summary>
        /// 腾讯朋友为 2
        /// </summary>
        Pengyou = 2,

        /// <summary>
        /// 腾讯微博为 3
        /// </summary>
        Weibo = 3,

        /// <summary>
        ///  Q+ 平台为 4
        /// </summary>
        Qpuls = 4,

        /// <summary>
        /// 财付通开放平台为 5
        /// </summary>
        Qpay = 5,

        /// <summary>
        /// QQGame 为 10
        /// </summary>
        QQGame = 10,
    }

    /// <summary>
    /// 操作类型
    /// 支付类操作为 1
    /// 留言发表类为 2
    /// 写操作类为 3
    /// 读操作类为 4
    /// 其它为 5
    /// </summary>
    public enum Optype
    {
        /// <summary>
        /// 支付类操作为 1 
        /// </summary>
        Pay = 1,

        /// <summary>
        /// 留言发表类为 2 
        /// </summary>
        Msg = 2,

        /// <summary>
        /// 写操作类为 3
        /// </summary>
        Write = 3,

        /// <summary>
        /// 读操作类为 4
        /// </summary>
        Read = 4,

        /// <summary>
        /// 其它为 5
        /// </summary>
        Other = 5,
    }

    /// <summary>
    /// 操作ID
    /// 1~ 1 00 为 保留字段，其中 ：
    /// 用户登录为 1
    /// 主动注册为 2
    /// 接受邀请注册为 3
    /// 邀请他人注册是 4
    /// 支付消费 为 5
    /// 留言为 6
    /// 留言回复为 7
    /// 如有其它类型的留言发表类操作 8
    /// 用户登出为 9
    /// 角色登录为 11
    /// 创建角色为 12
    /// 角色退出为 13
    /// 角色实时在线为 14
    /// 支付充值为 15
    /// </summary>
    public enum Actiontype
    {
        /// <summary>
        /// 用户登录为 1
        /// </summary>
        UserLogin = 1,

        /// <summary>
        /// 主动注册为 2 
        /// </summary>
        InviteRegist = 2,

        /// <summary>
        /// 接受邀请注册为 3 
        /// </summary>
        AInviteReg = 3,

        /// <summary>
        /// 邀请他人注册为 4 
        /// </summary>
        SInviteReg = 4,

        /// <summary>
        /// 支付消费为 5 
        /// </summary>
        Consumption = 5,

        /// <summary>
        /// 留言为 6
        /// </summary>
        Message = 6,

        /// <summary>
        /// 留言回复为 7
        /// </summary>
        Reply = 7,

        /// <summary>
        /// 如有其它类型的留言发表类操作，请填8
        /// </summary>
        OtherMsg = 8,

        /// <summary>
        /// 用户登出为 9
        /// </summary>
        UserLogout = 9,

        /// <summary>
        /// 角色登录为 11
        /// </summary>
        RoleLogin = 11,

        /// <summary>
        /// 创建角色为 12
        /// </summary>
        NewRole = 12,

        /// <summary>
        /// 角色退出为 13
        /// </summary>
        RoleLogout = 13,

        /// <summary>
        /// 角色实时在线为 14 
        /// </summary>
        Online = 14,

        /// <summary>
        /// 支付充值为 15 
        /// </summary>
        PayAndRecharge = 15,

        /// <summary>
        /// 添加物品
        /// </summary>
        AddGoods = 1000,

        /// <summary>
        /// 角色经验1001
        /// </summary>
        RoleExp = 1001,

        /// <summary>
        /// 宠物经验
        /// </summary>
        PetExp = 1002,

        /// <summary>
        /// 点券记录
        /// </summary>
        Bond = 1003,

        /// <summary>
        /// 物品交易
        /// </summary>
        //EnterDeal = 1004,

        /// <summary>
        /// 物品使用
        /// </summary>
        GoodsUse = 1005,

        /// <summary>
        /// 宠物删除
        /// </summary>
        PetRemove = 1006,

        /// <summary>
        /// 石币记录
        /// </summary>
        Score = 1007,

        /// <summary>
        /// 会员成长度增加
        /// </summary>
        CZD = 1008,

        /// <summary>
        /// 获取时装
        /// </summary>
        MallShiZhuang = 1009,

        /// <summary>
        /// 坐骑经验
        /// </summary>
        MountsExp = 1010,

        /// <summary>
        /// 进入副本
        /// </summary>
        EctypeIn = 1011,

        /// <summary>
        /// 退出副本
        /// </summary>
        EctypeOut = 1012,

        /// <summary>
        /// 拍买
        /// </summary>
        Auction = 1013,
        /// <summary>
        /// 暴星
        /// </summary>
        StartStar = 1014,




        /// <summary>
        /// 合成1015
        /// </summary>
        Fuse=1015,
        /// <summary>
        /// 镶嵌1016
        /// </summary>
        Mosaic=1016,
        /// <summary>
        /// 提取宠物技能1017
        /// </summary>
        DrawPetSkill=1017,
        /// <summary>
        /// 生产1018
        /// </summary>
        HomeProduce=1018,
        /// <summary>
        /// 照顾日志1019
        /// </summary>
        Stocking=1019,
        /// <summary>
        /// 家政日志1020
        /// </summary>
        JiaZheng=1020,
        /// <summary>
        /// 宠物放生1021
        /// </summary>
        PetRelease=1021,
        /// <summary>
        /// 宠物技能日志1022
        /// </summary>
        PetSkill=1022,
        /// <summary>
        /// 资质提升日志
        /// </summary>
        ZiZhi=1023,
        /// <summary>
        /// 任务完成日志
        /// </summary>
        TaskAward=1024,
        /// <summary>
        /// 家族日志
        /// </summary>
        FamilyLog=1025,




        /// <summary>
        /// PK失败
        /// </summary>
        PKLoss = 2000,

        /// <summary>
        /// 打怪失败
        /// </summary>
        APCLoss = 2001,

        /// <summary>
        /// 使用吸星
        /// </summary>
        Absorb = 2002,

        /// <summary>
        /// 击杀家族BOSS
        /// </summary>
        KillFamilyBoss = 2003,
    }
}
