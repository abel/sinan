using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// 44xx
    /// </summary>
    public class MountsCommand
    {
        /// <summary>
        /// 骑乘或召回坐骑
        /// </summary>
        public const string InOutMounts = "inOutMounts";
        public const string InOutMountsR = "j.inOutMountsR";

        /// <summary>
        /// 坐骑技能更换
        /// </summary>
        public const string MountsSkillChange = "mountsSkillChange";
        public const string MountsSkillChangeR = "j.mountsSkillChangeR";

        /// <summary>
        /// 坐骑进化
        /// </summary>
        public const string MountsUp = "mountsUp";
        public const string MountsUpR = "j.mountsUpR";

        /// <summary>
        /// 坐骑技能升级
        /// </summary>
        public const string MountsSkillUp = "mountsSkillUp";
        public const string MountsSkillUpR = "j.mountsSkillUpR";

        /// <summary>
        /// 坐骑提高经验
        /// </summary>
        public const string MountsUpdate = "mountsUpdate";
        public const string MountsUpdateR = "j.mountsUpdateR";
    }

    public class MountsReturn
    {
        /// <summary>
        /// 操作失败
        /// </summary>
        public const int Mounts1 = 42301;
        /// <summary>
        /// 没有坐骑
        /// </summary>
        public const int Mounts2 = 42302;
        /// <summary>
        /// 被更换的技能不存在
        /// </summary>
        public const int Mounts3 = 42303;
        /// <summary>
        /// 你想更换的技能已经存在
        /// </summary>
        public const int Mounts4 = 42304;
        /// <summary>
        /// 坐骑配置问题
        /// </summary>
        public const int Mounts5 = 42305;
        /// <summary>
        /// 技能不存在
        /// </summary>
        public const int Mounts6 = 42306;
        /// <summary>
        /// 技能配置有问题
        /// </summary>
        public const int Mounts7 = 42307;
        /// <summary>
        /// 坐骑技能等级配置有问题
        /// </summary>
        public const int Mounts8 = 42308;
        /// <summary>
        /// 坐骑类型不正确
        /// </summary>
        public const int Mounts9 = 42309;
        /// <summary>
        /// 石币不足
        /// </summary>
        public const int Mounts10 = 42310;
        /// <summary>
        /// 技能已经达到最高级
        /// </summary>
        public const int Mounts11 = 42311;
        /// <summary>
        /// 技能熟练度不足
        /// </summary>
        public const int Mounts12 = 42312;
        /// <summary>
        /// 坐骑已经进化到最高级
        /// </summary>
        public const int Mounts13 = 42313;
        /// <summary>
        /// 坐骑进化需要的道具数量不足
        /// </summary>
        public const int Mounts14 = 42314;
        /// <summary>
        /// 更换技能需要道具不足
        /// </summary>
        public const int Mounts15 = 42315;
        /// <summary>
        /// 运气不佳,进化失败
        /// </summary>
        public const int Mounts16 = 42316;
        /// <summary>
        /// 
        /// </summary>
        public const int Mounts17 = 42317;
        /// <summary>
        /// 
        /// </summary>
        public const int Mounts18= 42318;
        /// <summary>
        /// 
        /// </summary>
        public const int Mounts19 = 42319;
        /// <summary>
        /// 
        /// </summary>
        public const int Mounts20 = 42320;
        /// <summary>
        /// 
        /// </summary>
        public const int Mounts21 = 42321;
        /// <summary>
        /// 
        /// </summary>
        public const int Mounts22 = 42322;
        /// <summary>
        /// 
        /// </summary>
        public const int Mounts23 = 42323;
        /// <summary>
        /// 
        /// </summary>
        public const int Mounts24 = 42324;
        /// <summary>
        /// 
        /// </summary>
        public const int Mounts25 = 42325;
        /// <summary>
        /// 
        /// </summary>
        public const int Mounts26 = 42326;
        /// <summary>
        /// 
        /// </summary>
        public const int Mounts27 = 42327;
        /// <summary>
        /// 
        /// </summary>
        public const int Mounts28 = 42328;
        /// <summary>
        /// 
        /// </summary>
        public const int Mounts29 = 42329;
        /// <summary>
        /// 
        /// </summary>
        public const int Mounts30 = 42330;
    }
}
