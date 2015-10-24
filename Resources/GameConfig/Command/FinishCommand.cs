using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// 动作完成命令(21XX)
    /// </summary>
    public class FinishCommand
    {
        /// <summary>
        /// 角色升级
        /// </summary>
        public const string RoleUpLev = "RoleUpLev";

        /// <summary>
        /// 宠物升级
        /// </summary>
        public const string PetUpLev = "PetUpLev";

        /// <summary>
        /// 非绑定晶币(石币)达到指定数量
        /// </summary>
        public const string MaxMoney = "MaxMoney";

        /// <summary>
        /// 完成任务(任务ID,类型)
        /// </summary>
        public const string TotalTask = "TotalTask";

        /// <summary>
        /// 镶嵌
        /// </summary>
        public const string XianQian = "XianQian";

        /// <summary>
        /// 合成
        /// </summary>
        public const string HeChen = "HeChen";

        /// <summary>
        /// 宠物资质提升
        /// </summary>
        public const string PetZhiZi = "PetZhiZi";

        /// <summary>
        /// 宠物阶级提升
        /// </summary>
        public const string PetJieJi = "PetJieJi";

        /// <summary>
        /// 杀死APC
        /// </summary>
        public const string KillApc = "KillApc";

        /// <summary>
        /// 答题
        /// </summary>
        public const string AnswerAward = "AnswerAward";
        /// <summary>
        /// 添加好友
        /// </summary>
        public const string AddFriend = "";

        /// <summary>
        /// 师徒关系
        /// </summary>
        public const string ShiTu = "";

        /// <summary>
        /// 家族关系
        /// </summary>
        public const string JiaCu = ""; 

        /// <summary>
        /// 宠物技能提取
        /// </summary>
        public const string PetSkill = "PetSkill"; 

        /// <summary>
        /// 放生宠物数量
        /// </summary>
        public const string PetOut = "PetOut";

        /// <summary>
        /// 孵化宠物数量
        /// </summary>
        public const string PetFuHua = "PetFuHua";

        /// <summary>
        /// 孵化不同职业的宠物数量
        /// </summary>
        public const string PetJobFuHua = "PetJobFuHua";

        /// <summary>
        /// 当前好友数量
        /// </summary>
        public const string Friends = "Friends";

        /// <summary>
        /// 击杀Boss数量
        /// </summary>
        public const string Boss = "Boss";

        /// <summary>
        /// 在PK战斗中获胜
        /// </summary>
        public const string PKWin = "PKWin";

        /// <summary>
        /// 星座成就
        /// </summary>
        public const string Star = "Star";
        /// <summary>
        /// 洗炼次数
        /// </summary>
        public const string Purification = "Purification";
        /// <summary>
        /// 竞技场获胜次数
        /// </summary>
        public const string ArenaWin = "ArenaWin";
        /// <summary>
        /// 扩展仓库次数
        /// </summary>
        public const string ExtendLib = "ExtendLib";
   
        /// <summary>
        /// 使用星力空瓶次数
        /// </summary>
        public const string StarBottle = "StarBottle";
        /// <summary>
        /// 好友祝福次数
        /// </summary>
        public const string WishFriends = "WishFriends";
        /// <summary>
        /// 扩展兽栏次数
        /// </summary>
        public const string ExtendStables = "ExtendStables";

        /// <summary>
        /// 时装购买成就
        /// </summary>
        public const string Wardrobe = "Wardrobe";
    }
}
