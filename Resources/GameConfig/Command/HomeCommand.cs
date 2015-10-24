using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (25XX)
    /// </summary>
    public class HomeCommand
    {
        /// <summary>
        /// 得到家园信息
        /// </summary>
        public const string HomeInfo = "homeInfo";
        public const string HomeInfoR = "h.homeInfoR";
        /// <summary>
        /// 宠物驯养
        /// </summary>
        public const string HomePetKeep = "homePetKeep";
        public const string HomePetKeepR = "h.homePetKeepR";

        /// <summary>
        /// 终止宠物驯养
        /// </summary>
        public const string HomeStopKeep = "homeStopKeep";
        public const string HomeStopKeepR = "h.homeStopKeepR";
        /// <summary>
        /// 家园生产
        /// </summary>
        public const string HomeProduce = "homeProduce";
        public const string HomeProduceR = "h.homeProduceR";

        /// <summary>
        /// 家园采集
        /// </summary>
        public const string HomePluck = "homePluck";
        public const string HomePluckR = "h.homePluckR";
        /// <summary>
        /// 采集中止
        /// </summary>
        public const string HomeStopPluck = "homeStopPluck";
        public const string HomeStopPluckR = "h.homeStopPluckR";

        /// <summary>
        /// 得到生产列表道具信息
        /// </summary>
        public const string HomeProduceList = "homeProduceList";
        public const string HomeProduceListR = "h.homeProduceListR";

        /// <summary>
        /// 宠物进入工作士
        /// </summary>
        public const string InPet = "inPet";
        public const string InPetR = "h.inPetR";

        /// <summary>
        /// 停止生产
        /// </summary>
        public const string StopProduce = "stopProduce";
        public const string StopProduceR = "h.stopProduceR";

        /// <summary>
        /// 完成后收集物品
        /// </summary>
        public const string Collection = "collection";
        public const string CollectionR = "h.collectionR";

        /// <summary>
        /// 取回宠物 
        /// </summary>
        public const string PetRetrieve = "petRetrieve";
        public const string PetRetrieveR = "h.petRetrieveR";

        /// <summary>
        /// 回收宠物 
        /// </summary>
        public const string PetBack = "petBack";
        public const string PetBackR = "h.petBackR";


        /// <summary>
        /// 获取留言
        /// </summary>
        public const string GetBoard = "getBoard";
        public const string GetBoardR = "h.getBoardR";

        /// <summary>
        /// 添加留言
        /// </summary>
        public const string AddBoard = "h.addBoardR";

        /// <summary>
        /// 删除留言
        /// </summary>
        public const string RemoveBoard = "removeBoard";
    }

    public class HomeReturn 
    {
        /// <summary>
        /// 正在驯化的宠物
        /// </summary>
        public const int HomeKeeping = 23200;
        /// <summary>
        /// 宠物信息有误
        /// </summary>
        public const int PetError = 23201;
        /// <summary>
        /// 宠物正在进行其它工作
        /// </summary>
        public const int PetTasking = 23202;
        /// <summary>
        /// 驯化游戏币不足
        /// </summary>
        public const int KeepNoScoreB = 23203;
        /// <summary>
        /// 驯化已经完成
        /// </summary>
        public const int KeepFinish = 23204;
        /// <summary>
        /// 不存在驯化的宠物
        /// </summary>
        public const int NoKeepPet = 23205;
        /// <summary>
        /// 已经终止驯化
        /// </summary>
        public const int StopKeep = 23206;
        /// <summary>
        /// 兽栏或家园已满
        /// </summary>
        public const int PetBurdenFull = 23207;
        /// <summary>
        /// 采集时游戏币不足
        /// </summary>
        public const int HomePluckNoScore = 23208;
        /// <summary>
        /// 参数不下确
        /// </summary>
        public const int PareError = 23209;
        /// <summary>
        /// 不存在家园
        /// </summary>
        public const int NoHome = 23210;
        /// <summary>
        /// 宠物数量达到上限
        /// </summary>
        public const int PetNumberLimit = 23211;
        /// <summary>
        /// 表示宠物没有被驯化
        /// </summary>
        public const int NoPetKeep = 23212;
        /// <summary>
        /// 进入成功
        /// </summary>
        public const int InSuccess = 23213;
        /// <summary>
        /// 请先放入宠物 
        /// </summary>
        public const int InPet = 23214;
        /// <summary>
        /// 正在采集
        /// </summary>
        public const int Plucking = 23215;
        /// <summary>
        /// 停止生产成功
        /// </summary>
        public const int StopProduceSuccess = 23216;
        /// <summary>
        /// 道具数量不足
        /// </summary>
        public const int GoodsNumberNo = 23217;
        /// <summary>
        /// 配置有问题
        /// </summary>
        public const int ConfigError = 23218;
        /// <summary>
        /// 宠物家政信息不满足
        /// </summary>
        public const int PetHomeNo = 23219;
        /// <summary>
        /// 开始生产
        /// </summary>
        public const int ProduceSuccess = 23220;
        /// <summary>
        /// 生产还没有完成
        /// </summary>
        public const int ProduceNoFinish = 23221;
        /// <summary>
        /// 驯化停止成功
        /// </summary>
        public const int KeepStopSuccess = 23222;
        /// <summary>
        /// 没有生产
        /// </summary>
        public const int NoProduce = 23223;
        /// <summary>
        /// 普通包袱满
        /// </summary>
        public const int BurdenFull = 23224;
        /// <summary>
        /// 收集成功
        /// </summary>
        public const int CollectionSuccess = 23225;
        /// <summary>
        /// 已经存在宠物
        /// </summary>
        public const int IsPet = 23226;
        /// <summary>
        /// 取出宠物成功
        /// </summary>
        public const int OutPet = 23227;
        /// <summary>
        /// 正在生产中不能移除宠物
        /// </summary>
        public const int Produceing = 23228;
        /// <summary>
        /// 中止成功
        /// </summary>
        public const int StopPluckSuccess = 23229;
        /// <summary>
        /// 采集已经完成不能中止
        /// </summary>
        public const int FinishPluck = 23230;
        /// <summary>
        /// 没有采集中止无效
        /// </summary>
        public const int NoPluck = 23231;
        /// <summary>
        /// 正在生产其它物品
        /// </summary>
        public const int PrcodeceOther = 23232;
        /// <summary>
        /// 宠物正在战斗中
        /// </summary>
        public const int PetFighting = 23233;



        /// <summary>
        /// 炼金室
        /// </summary>
        public const int LianJinShi = 23234;
        /// <summary>
        /// 书房
        /// </summary>
        public const int ShuFang = 23235;
        /// <summary>
        /// 加工房
        /// </summary>
        public const int JiaGongFang = 23236;
        /// <summary>
        /// 木工房
        /// </summary>
        public const int MuGongFang = 23237;
        /// <summary>
        ///果园
        /// </summary>
        public const int GuoYuan = 23238;

        /// <summary>
        /// 有宠物已经过度疲劳,不能再生产
        /// </summary>
        public const int HomeProduce1 = 23239;


        /// <summary>
        /// 家园自动采集
        /// </summary>
        public const int HomeInfoCall1 = 23240;
        /// <summary>
        /// 家园自动养殖
        /// </summary>
        public const int HomeInfoCall2 = 23241;
        /// <summary>
        /// 家园自动挖掘
        /// </summary>
        public const int HomeInfoCall3 = 23242;
        /// <summary>
        /// 亲爱的会员玩家【{0}】，您在家园中生产的物品【{1}】已到达自动生产的上限时间【{2}】小时，现将您生产出的【{1}】总共【{3}】个邮寄给您，请您及时查收.同时生产又自动开始
        /// </summary>
        public const int HomeInfoCall4 = 23243;
        /// <summary>
        /// 生产对列已满，不能再生产
        /// </summary>
        public const int HomeProduce2 = 23244;

        /// <summary>
        /// 生产功能变更
        /// </summary>
        public const int HomeProduce3 = 23245;
        /// <summary>
        /// 因为生产功能变更，影响了您之前正在生产的物品;特通过邮件将之前您正在生产的【{0}】总供【{1}】个发送给您,因邮件存在过期时间,请即时通过邮件领取;因功能变化对您的影响，给您带来的不便，请凉解！
        /// </summary>
        public const int HomeProduce4 = 23246;

        /// <summary>
        /// 一条生产线最大可以生产【{0}】个该物品
        /// </summary>
        public const int HomeProduce5 = 23247;
    }
}
