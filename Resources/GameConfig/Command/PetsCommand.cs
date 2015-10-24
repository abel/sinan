using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (33XX,34XX)
    /// </summary>
    public class PetsCommand
    {
        /// <summary>
        /// 宠物的创建
        /// </summary>
        public const string CreatePets = "createPets";
        //public const string CreatePetsR = "p.createPetR";

        /// <summary>
        /// 得到宠物基本信息
        /// </summary>
        public const string GetPetsInfo = "getPetsInfo";
        public const string GetPetsInfoR = "p.getPetsInfoR";

        /// <summary>
        /// 得到带领的宠物信息
        /// </summary>
        public const string GuidePetsInfo = "guidePetsInfo";
        public const string GuidePetsInfoR = "p.guidePetsInfoR";

        /// <summary>
        /// 更新宠物名称
        /// </summary>
        public const string ChangePetsName = "changePetsName";
        public const string ChangePetsNameR = "p.changePetsNameR";

        /// <summary>
        /// 资质提升
        /// </summary>
        public const string ZiZhiPets = "ziZhiPets";
        public const string ZiZhiPetsR = "p.ziZhiPetsR";

        /// <summary>
        /// 喂养宠物 
        /// </summary>
        public const string FeedPets = "feedPets";
        public const string FeedPetsR = "p.feedPetsR";

        /// <summary>
        /// 宠物进化
        /// </summary>
        public const string UpPetsRank = "upPetsRank";
        public const string UpPetsRankR = "p.upPetsRankR";

        /// <summary>
        /// 宠物放生
        /// </summary>
        public const string PetRelease = "petRelease";
        public const string PetReleaseR = "p.petReleaseR";
        /// <summary>
        /// 激活宠物槽
        /// </summary>
        public const string ShockPetGroove = "shockPetGroove";
        public const string ShockPetGrooveR = "p.shockPetGrooveR";

        /// <summary>
        /// 宠物更新
        /// </summary>
        public const string UpdatePetR = "p.updatePetR";

        /// <summary>
        /// 宠物仓库拖动操作
        /// </summary>
        public const string PetBurdenDrag = "petBurdenDrag";
        public const string PetBurdenDragR = "p.petBurdenDragR";

        /// <summary>
        /// 放养或抓取
        /// </summary>
        public const string Stocking = "stocking";
        public const string StockingR = "p.stockingR";

        /// <summary>
        /// 得到宠物列表
        /// </summary>
        public const string GetPetsList = "getPetsList";
        public const string GetPetsListR = "h.getPetsListR";

        /// <summary>
        /// 家园放养领取奖励
        /// </summary>
        public const string StockingAward = "stockingAward";
        public const string StockingAwardR = "p.stockingAwardR";


        /// <summary>
        /// 宠物家园扩展
        /// </summary>
        public const string PetExtend = "petExtend";
        public const string PetExtendR = "p.petExtendR";

        /// <summary>
        /// 提取宠物技能
        /// </summary>
        public const string DrawPetSkill = "drawPetSkill";
        public const string DrawPetSkillR = "p.drawPetSkillR";

        /// <summary>
        /// 解除技能
        /// </summary>
        public const string RemoveSkill = "removeSkill";
        public const string RemoveSkillR = "p.removeSkillR";

        /// <summary>
        /// 遗忘技能
        /// </summary>
        public const string OblivionSkill = "oblivionSkill";
        public const string OblivionSkillR = "p.oblivionSkillR";

        /// <summary>
        /// 激活空位槽
        /// </summary>
        public const string ShockSkill = "shockSkill";
        public const string ShockSkillR = "p.shockSkillR";

        /// <summary>
        /// 添加宠物技能
        /// </summary>
        public const string AddSkill = "addSkill";
        public const string AddSkillR = "p.addSkillR";

        /// <summary>
        /// 添加宠物快捷键
        /// </summary>
        public const string AddKey = "addKey";
        public const string AddKeyR = "p.addKeyR";


        /// <summary>
        /// 偷取宠物
        /// </summary>
        public const string StealPet = "stealPet";
        public const string StealPetR = "p.stealPetR";

        /// <summary>
        /// 宠物保护
        /// </summary>
        public const string PetProtection = "petProtection";
        public const string PetProtectionR = "p.petProtectionR";

        /// <summary>
        /// 一键照顾
        /// </summary>
        public const string StockingAll = "stockingAll";
        public const string StockingAllR = "p.stockingAllR";

        /// <summary>
        /// 得到宠物基本属性值
        /// </summary>
        public const string PetProperty = "petProperty";
        public const string PetPropertyR = "p.petPropertyR";

        /// <summary>
        /// 吸星魔法
        /// </summary>
        public const string PetAbsorb = "petAbsorb";
        public const string PetAbsorbR = "p.petAbsorbR";

        /// <summary>
        /// 护理宠物
        /// </summary>
        public const string PetNurse = "petNurse";
        public const string PetNurseR = "p.petNurseR";

        /// <summary>
        /// 宠物赠送
        /// </summary>
        public const string PetPresent = "petPresent";
        public const string PetPresentR = "p.petPresentR";

        /// <summary>
        /// 一键喂养
        /// </summary>
        public const string FeedPetsAll = "feedPetsAll";
        public const string FeedPetsAllR = "p.feedPetsAllR";

        /// <summary>
        /// 宠物进化技能变更
        /// </summary>
        public const string EvoSkillChange = "evoSkillChange";
        public const string EvoSkillChangeR = "p.evoSkillChangeR";

        /// <summary>
        /// 宠物进化技能提升等级
        /// </summary>
        public const string EvoSkillUp = "evoSkillUp";
        public const string EvoSkillUpR = "p.evoSkillUpR";
    }

    public class PetsReturn
    {
        /// <summary>
        /// 表示没有带领的宠物
        /// </summary>
        public const int NoPets = 22800;

        /// <summary>
        /// 表示宠物还没有被驯化
        /// </summary>
        public const int PetsWild = 22801;

        /// <summary>
        /// 数据库更新失败
        /// </summary>
        public const int UpdateFail = 22802;

        /// <summary>
        /// 参数名称不正确
        /// </summary>
        public const int ParaNameError = 22803;

        /// <summary>
        /// 表示升级经验不足
        /// </summary>
        public const int UpgradeShort = 22804;

        /// <summary>
        /// 有异常
        /// </summary>
        public const int PetsException = 22805;

        /// <summary>
        /// 宠物等级大于角色等级5级，不能被激
        /// (表示角色等级不够)
        /// </summary>
        public const int NoLevel = 22806;
        /// <summary>
        /// 提升资质的道具不存在
        /// </summary>
        public const int NoGoodsZiZhi = 22807;
        /// <summary>
        /// 进化所需值不足进化失败
        /// </summary>
        public const int UpPetsRank = 22808;
        /// <summary>
        /// 位置不正确
        /// </summary>
        public const int NoPlace = 22809;
        /// <summary>
        /// 没有遗忘要求的道具
        /// </summary>
        public const int NoOblivionGoods = 22810;
        /// <summary>
        /// 没有激活要求的道具
        /// </summary>
        public const int NoShockGoods = 22811;
        /// <summary>
        /// 非天生不能遗忘
        /// </summary>
        public const int NoBorn = 22812;
        /// <summary>
        /// 操作成功
        /// </summary>
        public const int Success = 22813;
        /// <summary>
        /// 宠物技能槽激活完成
        /// </summary>
        public const int GooveFull = 22814;
        /// <summary>
        /// 非天生技能不能提取
        /// </summary>
        public const int NoDraw = 22815;
        /// <summary>
        /// 宠物不存在
        /// </summary>
        public const int NoExists = 22816;
        /// <summary>
        /// 提高成功率的道具不足
        /// </summary>
        public const int DrawSuccessGoods = 22817;
        /// <summary>
        /// 包袱满
        /// </summary>
        public const int BurdenFull = 22818;
        /// <summary>
        /// 提取失败
        /// </summary>
        public const int DrawFail = 22819;

        /// <summary>
        /// 宠物技能配置不正确
        /// </summary>
        public const int SkillConfigError = 22820;
        /// <summary>
        /// 技能类型不实合该宠物
        /// </summary>
        public const int SkillTypeError = 22821;
        /// <summary>
        /// 宠物阶级不足
        /// </summary>
        public const int NoPetsRank = 22822;
        /// <summary>
        /// 还没有学习该技能
        /// </summary>
        public const int NoStudySkill = 22823;
        /// <summary>
        /// 技能槽已经用完或没有开启新的技能槽
        /// </summary>
        public const int SkillGooveFull = 22824;
        /// <summary>
        /// 已经激活
        /// </summary>
        public const int IsShock = 22825;
        /// <summary>
        /// 天生技能不能解除
        /// </summary>
        public const int RemoveSkill = 22826;
        /// <summary>
        /// 没有激活所需要的道具
        /// </summary>
        public const int NoGooveGoods = 22827;

        /// <summary>
        /// 已经进化到最高级
        /// </summary>
        public const int RankMax = 22828;
        /// <summary>
        /// 宠物槽没有激活或宠物槽没有空位置，进化失败
        /// </summary>
        public const int NoSkillList = 22829;
        /// <summary>
        /// 技能配置不正确
        /// </summary>
        public const int NoSkillConfig = 22830;
        /// <summary>
        /// 宠物不能再进化
        /// </summary>
        public const int NoRank = 22831;
        /// <summary>
        /// 质资提升失败
        /// </summary>
        public const int ZhiZiFail = 22832;
        /// <summary>
        /// 喂养所需要的成长果实不足，请先购买成长果实再进行喂养
        /// </summary>
        public const int NoKeepGoods = 22833;
        /// <summary>
        /// 提升量超过可提升的最大量
        /// </summary>
        public const int KeepBig = 22834;
        /// <summary>
        /// 已经装配该技能,请先解除或遗忘再重新装配
        /// </summary>
        public const int IsSkill = 22835;
        /// <summary>
        /// 该项资质目前已达最高等级
        /// </summary>
        public const int ZiZhiMax = 22836;
        /// <summary>
        /// 技能已经移除
        /// </summary>
        public const int NoSkill = 22837;
        /// <summary>
        /// 资质提升晶币不足
        /// </summary>
        public const int NoCoin = 22838;
        /// <summary>
        /// 带领中的宠物不能进行其它操作
        /// </summary>
        public const int NoRelease = 22839;
        /// <summary>
        /// 添加快捷键成功
        /// </summary>
        public const int AddKey = 22840;
        /// <summary>
        /// 所有宠物槽激活完成
        /// </summary>
        public const int ShockFinish = 22841;
        /// <summary>
        /// 宠物名称超长
        /// </summary>
        public const int PetNameLenght = 22842;
        /// <summary>
        /// 存在非法字符请重新填写
        /// </summary>
        public const int PetNameError = 22843;
        /// <summary>
        /// 宠物格子没有被激活拖动无效
        /// </summary>
        public const int NoShock = 22844;
        /// <summary>
        /// 进化时获得技能的机率配置不正确
        /// </summary>
        public const int RateConfigError = 22845;
        /// <summary>
        /// 宠物技能不存在
        /// </summary>
        public const int NoPetSkill = 22846;
        /// <summary>
        /// 已经存在该技能，进化失败
        /// </summary>
        public const int PetSkill = 22847;
        /// <summary>
        /// 激活宠物技能槽晶币不足
        /// </summary>
        public const int ShockNoCoin = 22848;
        /// <summary>
        /// 遗忘技能晶币不足
        /// </summary>
        public const int OblivionNoCoin = 22849;
        /// <summary>
        /// 提取所需道具
        /// </summary>
        public const int DrawGoods = 22850;
        /// <summary>
        /// 石币不足，不能操作
        /// </summary>
        public const int NoScore = 22851;

        /// <summary>
        /// 激活宠物槽需要晶币
        /// </summary>
        public const int ShockPetGrooveCoin = 22852;

        /// <summary>
        /// 不能在【{0}】场景放养宠物或招回宠物"
        /// </summary>
        public const int NoSceneID = 22853;
        /// <summary>
        /// 家园已满,不能再放养!
        /// </summary>
        public const int PetBurdenB2 = 22854;
        /// <summary>
        /// 宠物背包已满,不能再随身带领!
        /// </summary>
        public const int PetBurdenB3 = 22855;
        /// <summary>
        /// 家园已经扩展到最大，不能再扩展
        /// </summary>
        public const int PetExtendMax = 22856;
        /// <summary>
        /// 家园扩展所需晶币不足
        /// </summary>
        public const int NoCoinEx = 22857;
        /// <summary>
        /// 照顾时间不够
        /// </summary>
        public const int NoCareTime = 22858;

        /// <summary>
        /// 正在溜的宠物不能放生
        /// </summary>
        public const int IsLiu = 22859;

        /// <summary>
        /// 不能偷自己的宠
        /// </summary>
        public const int StealSlef = 22860;

        /// <summary>
        /// 用户不存在
        /// </summary>
        public const int NoUser = 22861;
        /// <summary>
        /// 你背包已满,请先整理你的宠物背包,再来进行偷宠操作
        /// </summary>
        public const int StealBurdenFull = 22862;

        /// <summary>
        /// 【{0}】宠被【{1}】诱走！
        /// </summary>
        public const int StealPet1 = 22863;
        /// <summary>
        /// "【{0}】受到诱惑！";
        /// </summary>
        public const int StealPet2 = 22864;
        /// <summary>
        /// 包袱已满，请先整理你的包袱,再来进行该操作
        /// </summary>
        public const int BurdenB0Full = 22865;

        /// <summary>
        /// 诱宠道具G_d000015
        /// </summary>
        public const int StealGoods = 22866;
        /// <summary>
        /// 不存在抓捕网
        /// </summary>
        public const int StealNoGoods = 22867;
        /// <summary>
        /// 宠物已挂，只能主人重新补血才能复活,照顾才能产生效益
        /// </summary>
        public const int StockingNo = 22868;

        /// <summary>
        /// 捕宠失败
        /// </summary>
        public const int StealFail = 22869;

        /// <summary>
        /// 正在保护期的宠物不能偷取
        /// </summary>
        public const int StealProtectionTime = 22870;

        /// <summary>
        /// 偷宠成功邮件标题
        /// </summary>
        public const int StealEmailTitle = 22871;
        /// <summary>
        /// 偷宠成功邮件内容
        /// </summary>
        public const int StealEmailContent = 22872;


        /// <summary>
        /// 玩家等级没有达到最小等级
        /// </summary>
        public const int StealMinLev = 22873;

        /// <summary>
        /// 玩家等级低于宠物等级x级以上
        /// </summary>
        public const int StealMinLev2 = 22874;

        /// <summary>
        /// 获得返还
        /// </summary>
        public const int StealEmainl1 = 22875;


        /// <summary>
        /// 资质提升成功
        /// </summary>
        public const int ZiZhiPets1 = 22876;
        /// <summary>
        /// 没有需要照顾的宠物
        /// </summary>
        public const int StockingAll1 = 22877;


        /// <summary>
        /// 这只宠物只对主人效忠，无法诱惑
        /// </summary>
        public const int StealPet3 = 22878;
        /// <summary>
        /// 投入成长果实数量太少，无法诱惑！
        /// </summary>
        public const int StealPet4 = 22879;
        /// <summary>
        /// 成长果实不足
        /// </summary>
        public const int StealPet5 = 22880;
        /// <summary>
        /// 很遗憾，诱宠失败！祝您下次好运!
        /// </summary>
        public const int StealPet6 = 22881;
        /// <summary>
        /// {0:M月d日H时m分},你的宠物【{1}】被【{2}】诱走
        /// </summary>
        public const int StealPet7 = 22882;
        /// <summary>
        /// 【{0}】宠被【{1}】诱走！
        /// </summary>
        public const int StealPet8 = 22883;
        /// <summary>
        /// 您的宠物【{0}】因为【{1}】的诱惑离开了你！在清理场地的时候，发现了一些【{1}】留下的成长果实，请在附件中查收！;                              
        /// </summary>
        public const int StealPet9 = 22884;
        /// <summary>
        /// 【{0}】宠受到诱惑
        /// </summary>
        public const int StealPet10 = 22885;
        /// <summary>
        /// 【{1}】试图诱惑你的宠物【{0}】！但你的宠物没有离开你， 【{0}】将【{1}】用来诱惑它的成长果实堆在一起，请在附件中查收！
        /// </summary>
        public const int StealPet11 = 22886;
        /// <summary>
        /// 系统邮件
        /// </summary>
        public const int StealPet12 = 22887;
        /// <summary>
        /// 恭喜你诱宠成功
        /// </summary>
        public const int StealPet13 = 22888;

        /// <summary>
        /// 保护道具不存在
        /// </summary>
        public const int PetProtection1 = 22889;

        /// <summary>
        /// 被赠送者不存在
        /// </summary>
        public const int PetPresent1 = 22890;
        /// <summary>
        /// 【{0}】的家园放养已经达到上限，无法接受您的馈赠！
        /// </summary>
        public const int PetPresent2 = 22891;
        /// <summary>
        /// 赠送宠物所需要的道具不存在
        /// </summary>
        public const int PetPresent3 = 22892;
        /// <summary>
        /// {0:M月d日H时m分},【{1}】赠送了您一只宠物【{2}】！
        /// </summary>
        public const int PetPresent4 = 22893;


        /// <summary>
        /// 您没有学会吸星魔法
        /// </summary>
        public const int Absorb1 = 22894;
        /// <summary>
        /// {0:M月d日H时m分},您的宠物【{1}】遭到【{2}】吸星,濒临死亡,请尽快护理
        /// </summary>
        public const int Absorb2 = 22895;
        /// <summary>
        /// 每天可以使用吸星魔法上限为{0}次
        /// </summary>
        public const int Absorb3 = 22896;
        /// <summary>
        /// 吸星失败！无收益。今日还可以使用吸星魔法{0}次
        /// </summary>
        public const int Absorb4 = 22897;

        public const int Absorb5 = 22898;
        public const int Absorb6 = 22899;


        /// <summary>
        /// 进化技能已经最高级不能再升级
        /// </summary>
        public const int EvoSkillUp1 = 42401;
        /// <summary>
        /// 提升宠物进化技能所需道具不存在
        /// </summary>
        public const int EvoSkillUp2 = 42402;
        /// <summary>
        /// 
        /// </summary>
        public const int EvoSkillUp3 = 42403;
        /// <summary>
        /// 
        /// </summary>
        public const int EvoSkillUp4 = 42404;
        /// <summary>
        /// 
        /// </summary>
        public const int EvoSkillUp5 = 42405;

        /// <summary>
        /// 宠物进化技能变更所需道具不存在
        /// </summary>
        public const int EvoSkillChange1 = 42406;
        /// <summary>
        /// 
        /// </summary>
        public const int EvoSkillChange2 = 42407;
    }
}
