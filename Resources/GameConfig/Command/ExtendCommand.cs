using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// 扩展(42xx)
    /// </summary>
    public class ExtendCommand
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public class ExtendType 
    {
        /// <summary>
        /// 资质
        /// </summary>
        public const string ZiZhi = "ZiZhi";
        /// <summary>
        /// 仓库
        /// </summary>
        public const string B1 = "B1";
        /// <summary>
        /// 家园
        /// </summary>
        public const string B2 = "B2";
        /// <summary>
        /// 兽栏
        /// </summary>
        public const string B3 = "B3";
        /// <summary>
        /// 宠物技能槽
        /// </summary>
        public const string ShockSkill = "ShockSkill";
    }
    /// <summary>
    /// 返回信息
    /// </summary>
    public class ExtendReturn
    {
        /// <summary>
        /// 没有达到最小提升机率不能提升资质
        /// </summary>
        public const int ZiZhi1 = 42001;
        /// <summary>
        /// 提升资质的道具数量不足
        /// </summary>
        public const int ZiZhi2 = 42002;
        /// <summary>
        /// 当前宠物等级不足必须达到【{0}】级才能提升
        /// </summary>
        public const int ZiZhi3 = 42003;
        /// <summary>
        /// 宠物成长度必须达到【{0}】才能提升
        /// </summary>
        public const int ZiZhi4 = 42004;
        /// <summary>
        /// 
        /// </summary>
        public const int ZiZhi5 = 42005;

        /// <summary>
        /// 没有达到扩展仓库的最小机率
        /// </summary>
        public const int B11 = 42006;
        /// <summary>
        /// 扩展仓库所需道具数量不足
        /// </summary>
        public const int B12 = 42007;
        /// <summary>
        /// 扩展仓库失败
        /// </summary>
        public const int B13 = 42008;
        /// <summary>
        /// 
        /// </summary>
        public const int B14 = 42009;
        /// <summary>
        /// 
        /// </summary>
        public const int B15 = 42010;



        /// <summary>
        /// 没有达到扩展家园的最小成功率不能扩展
        /// </summary>
        public const int B21 = 42011;

        /// <summary>
        /// 扩展家园所需要的道具不足
        /// </summary>
        public const int B22 = 42012;
        /// <summary>
        /// 家园扩展失败
        /// </summary>
        public const int B23 = 42013;
        /// <summary>
        /// 
        /// </summary>
        public const int B24 = 42014;
        /// <summary>
        /// 
        /// </summary>
        public const int B25 = 42015;



        /// <summary>
        /// 没有达到最小提升机率不能扩展兽栏
        /// </summary>
        public const int B31 = 42016;

        /// <summary>
        /// 扩展兽栏所需道具不足
        /// </summary>
        public const int B32 = 42017;
        /// <summary>
        /// 扩展兽栏失败
        /// </summary>
        public const int B33 = 42018;
        /// <summary>
        /// 
        /// </summary>
        public const int B34 = 42019;
        /// <summary>
        /// 
        /// </summary>
        public const int B35 = 42020;


        /// <summary>
        /// 没有达到扩展宠物技能槽的最底值
        /// </summary>
        public const int ShockSkill1 = 42021;
        /// <summary>
        /// 扩展宠物技能槽所需要的道具不足
        /// </summary>
        public const int ShockSkill2 = 42022;
        /// <summary>
        /// 扩展宠物技能槽失败
        /// </summary>
        public const int ShockSkill3 = 42023;
        /// <summary>
        /// 
        /// </summary>
        public const int ShockSkill4 = 42024;
        /// <summary>
        /// 
        /// </summary>
        public const int ShockSkill5 = 42025;

        /// <summary>
        /// 当前宠物等级不足必须达到【{0}】级才能提升
        /// </summary>
        public const int ZiZhi6 = 42026;
        /// <summary>
        /// 宠物成长度必须达到【{0}】才能提升
        /// </summary>
        public const int ZiZhi7 = 42027;
    }
}
