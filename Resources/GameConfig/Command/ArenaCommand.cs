using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (11XX)
    /// </summary>
    public class ArenaCommand
    {
        /// <summary>
        /// 创建竞技场
        /// </summary>
        public const string CreateArena = "createArena";
        public const string CreateArenaR = "n.createArenaR";

        /// <summary>
        /// 得到竞技场列表
        /// </summary>
        public const string GetArenaList = "getArenaList";
        public const string GetArenaListR = "n.getArenaListR";

        /// <summary>
        /// 进入竞技场
        /// </summary>
        public const string ArenaInto = "arenaInto";
        public const string ArenaIntoR = "n.arenaIntoR";

        /// <summary>
        /// 取得到可以参加竞技场的宠物列表
        /// </summary>
        public const string PetListArena = "petListArena";
        public const string PetListArenaR = "n.petListArenaR";

        /// <summary>
        /// 宠物放入竞技场
        /// </summary>
        public const string PetInArena = "petInArena";
        public const string PetInArenaR = "n.petInArenaR";

        /// <summary>
        /// 宠物退出竞技场
        /// </summary>
        public const string PetOutArena = "petOutArena";
        public const string PetOutArenaR = "n.petOutArenaR";

        /// <summary>
        /// 选择宠物技能
        /// </summary>
        public const string SelectSkill = "selectSkill";
        public const string SelectSkillR = "n.selectSkillR";

        /// <summary>
        /// 竞技场物品使用
        /// </summary>
        public const string ArenaGoods = "arenaGoods";
        public const string ArenaGoodsR = "n.arenaGoodsR";

        /// <summary>
        /// 竞技场宠物行走
        /// </summary>
        public const string ArenaWalk = "arenaWalk";
        public const string ArenaWalkR = "n.arenaWalkR";

        /// <summary>
        /// 竞技场开始
        /// </summary>
        public const string ArenaStartR = "n.arenaStartR";

        /// <summary>
        /// 竞技场结束
        /// </summary>
        public const string ArenaEndR = "n.arenaEndR";

        /// <summary>
        /// 得到分组名称列表
        /// </summary>
        public const string ArenaGroupName = "arenaGroupName";
        public const string ArenaGroupNameR = "n.arenaGroupNameR";

        /// <summary>
        /// 角色退出竞技场
        /// </summary>
        public const string PlayerOutArena = "playerOutArena";
        public const string PlayerOutArenaR = "n.playerOutArenaR";

        /// <summary>
        /// 战斗中
        /// </summary>
        public const string ArenaFightR = "n.arenaFightR";

        /// <summary>
        /// 得到场景信息
        /// </summary>
        public const string SceneBase = "sceneBase";
        public const string SceneBaseR = "n.sceneBaseR";

        /// <summary>
        /// 得到竞技场进入人数与参战人数
        /// </summary>
        public const string ArenaUserCount = "arenaUserCount";
        public const string ArenaUserCountR = "n.arenaUserCountR";

        /// <summary>
        /// 宠物战死
        /// </summary>
        public const string ArenaPetOverR = "n.arenaPetOverR";
    }

    public class ArenaReturn 
    {
        /// <summary>
        /// 参数为不能为空
        /// </summary>
        public const int CreateArena1 = 11001;

        /// <summary>
        /// 竞技场基本配置不正确
        /// </summary>
        public const int CreateArena2 = 11002;
        /// <summary>
        /// 请选择宠物最底等级限制
        /// </summary>
        public const int CreateArena3 = 11003;
        /// <summary>
        /// 请选择宠物最高等级限制
        /// </summary>
        public const int CreateArena4 = 11004;
        /// <summary>
        /// 允许中途参战必须可以观战
        /// </summary>
        public const int CreateArena5 = 11005;
        /// <summary>
        /// 场景不存在
        /// </summary>
        public const int CreateArena6 = 11006;
        /// <summary>
        /// 该竞技场已经存在
        /// </summary>
        public const int CreateArena7 = 11007;
        /// <summary>
        /// 你已经创建有竞技场,不能再创建
        /// </summary>
        public const int CreateArena8 = 11008;

        /// <summary>
        /// 竞技场不存在
        /// </summary>
        public const int ArenaInto1 = 11009;
        /// <summary>
        /// 组队状态不能参加
        /// </summary>
        public const int ArenaInto2 = 11010;
        /// <summary>
        /// 进入密码不正确
        /// </summary>
        public const int ArenaInto3 = 11011;
        /// <summary>
        /// 已经进入竞技场,不能重复进入
        /// </summary>
        public const int ArenaInto4 = 11012;
        /// <summary>
        /// 该竞技场不存在
        /// </summary>
        public const int ArenaInto5 = 11013;
        /// <summary>
        /// 当前场景不能进入竞技场
        /// </summary>
        public const int ArenaInto6 = 11014;

        /// <summary>
        /// 请先选择竞技场
        /// </summary>
        public const int ArenaGroupName1 = 11015;

        /// <summary>
        /// 请选择要参战的竞技场
        /// </summary>
        public const int PetInArena1 = 11016;
        /// <summary>
        /// 竞技场已经开始,不能再参战
        /// </summary>
        public const int PetInArena2 = 11017;
        /// <summary>
        /// 请选择分组
        /// </summary>
        public const int PetInArena3 = 11018;
        /// <summary>
        /// 你已经加入【{0}】组,不能再选择其它组
        /// </summary>
        public const int PetInArena4 = 11019;
        /// <summary>
        /// 宠物不存在
        /// </summary>
        public const int PetInArena5 = 11020;
        /// <summary>
        /// 宠物过度疲劳,不能参加竞技场
        /// </summary>
        public const int PetInArena6 = 11021;
        /// <summary>
        /// 战绩差距太大,不能参加
        /// </summary>
        public const int PetInArena7 = 11022;
        /// <summary>
        /// 每人只能【{0}】只宠物参战
        /// </summary>
        public const int PetInArena8 = 11023;
        /// <summary>
        /// 每组只能【{0}】只宠物参战
        /// </summary>
        public const int PetInArena9 = 11024;
        /// <summary>
        /// 宠物已经参战
        /// </summary>
        public const int PetInArena10 = 11025;

        /// <summary>
        /// 角色不在该竞技场上
        /// </summary>
        public const int PetOutArena1 = 11026;
        /// <summary>
        /// 宠物不存在
        /// </summary>
        public const int PetOutArena2 = 11027;

        /// <summary>
        /// 请先选择竞技场
        /// </summary>
        public const int SelectSkill1 = 11028;
        /// <summary>
        /// 宠物不存在该技能
        /// </summary>
        public const int SelectSkill2 = 11029;
        /// <summary>
        /// 技能不存在
        /// </summary>
        public const int SelectSkill3 = 11030;
        /// <summary>
        /// 非主动技能
        /// </summary>
        public const int SelectSkill4 = 11031;


        /// <summary>
        /// 请先选择竞技场
        /// </summary>
        public const int ArenaGoodsPet1 = 11032;
        /// <summary>
        /// 输入参数不对
        /// </summary>
        public const int ArenaGoodsPet2 = 11033;
        /// <summary>
        /// 物品不存在
        /// </summary>
        public const int ArenaGoodsPet3 = 11034;
        /// <summary>
        /// 该物品不能在战斗中使用
        /// </summary>
        public const int ArenaGoodsPet4 = 11035;
        /// <summary>
        /// 不需要补充
        /// </summary>
        public const int ArenaGoodsPet5 = 11036;

        /// <summary>
        /// 请选择战斗时长
        /// </summary>
        public const int ArenaLimit1 = 11037;
        /// <summary>
        /// 请选择分组
        /// </summary>
        public const int ArenaLimit2 = 11038;
        /// <summary>
        /// 请选择战宠等级
        /// </summary>
        public const int ArenaLimit3 = 11039;
        /// <summary>
        /// 请选择各组产战宠物数量
        /// </summary>
        public const int ArenaLimit4 = 11040;
        /// <summary>
        /// 请选择准备时间
        /// </summary>
        public const int ArenaLimit5 = 11041;
        /// <summary>
        /// 请选择竞技场
        /// </summary>
        public const int ArenaLimit6= 11042;
        /// <summary>
        /// 请选择角色参战宠物数量
        /// </summary>
        public const int ArenaLimit7 = 11043;
        /// <summary>
        /// 请选择战绩差
        /// </summary>
        public const int ArenaLimit8 = 11044;
        /// <summary>
        /// 配置有问题
        /// </summary>
        public const int ArenaLimit9 = 11045;
        /// <summary>
        /// 石币不足,不能创建
        /// </summary>
        public const int ArenaLimit10 = 11046;

        /// <summary>
        /// 竞技场不满足开始条件
        /// </summary>
        public const int CheckArenaStart1 = 11047;
        /// <summary>
        /// 竞技场开始
        /// </summary>
        public const int CheckArenaStart2 = 11048;


        /// <summary>
        /// 致光荣的战场逃亡者
        /// </summary>
        public const int UseDis1 = 11049;
        /// <summary>
        /// 你在竞技场的脱逃让人失望，不过凭着卓越的战斗技巧，仍然获得了X点附加战绩奖励。希望勇者不再以战败者的方式离开战场
        /// </summary>
        public const int UseDis2 = 11050;
        /// <summary>
        /// 致可耻的战场逃亡者
        /// </summary>
        public const int UseDis3 = 11051;
        /// <summary>
        /// 你在竞技场的脱逃行为被人唾弃，你在这次战斗中被判定为失败者，扣除了X点附加战绩惩罚。你的行为背离了竞技场精神
        /// </summary>
        public const int UseDis4 = 11052;
        /// <summary>
        /// 系统邮件
        /// </summary>
        public const int UseDis5 = 11053;
    }
}

