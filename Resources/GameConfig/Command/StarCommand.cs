using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (41XX)
    /// </summary>
    public class StarCommand
    {
        /// <summary>
        /// 冥想
        /// </summary>
        public const string PlayerMeditation = "playerMeditation";
        public const string PlayerMeditationR = "c.playerMeditationR";

        /// <summary>
        /// 激活星座
        /// </summary>
        public const string StartStar = "startStar";
        public const string StartStarR = "c.startStarR";

        /// <summary>
        /// 暴星分享
        /// </summary>
        public const string StartStarShared = "startStarShared";
        public const string StartStarSharedR = "c.startStarSharedR";

        /// <summary>
        /// 宠物放入星阵
        /// </summary>
        public const string InStarTroops = "inStarTroops";
        public const string InStarTroopsR = "c.inStarTroopsR";

        /// <summary>
        /// 取出星阵中宠物
        /// </summary>
        public const string OutStarTroops = "outStarTroops";
        public const string OutStarTroopsR = "c.outStarTroopsR";

        /// <summary>
        /// 取得星阵星力
        /// </summary>
        public const string GetStarTroops = "getStarTroops";
        public const string GetStarTroopsR = "c.getStarTroopsR";

        /// <summary>
        /// 是否激合星阵
        /// </summary>
        public const string IsStartTroops = "isStartTroops";
        public const string IsStartTroopsR = "c.isStartTroopsR";
        
    }

    public class StarReturn 
    {
        /// <summary>
        /// 战斗中不能冥想
        /// </summary>
        public const int PlayerMeditation1 = 41000;
        /// <summary>
        /// 【{0}】场景中不能冥想
        /// </summary>
        public const int PlayerMeditation2 = 41001;



        /// <summary>
        /// 配置不正确
        /// </summary>
        public const int StartStar1 = 41002;
        /// <summary>
        /// 提高激活守护星成功率的道具数量不足
        /// </summary>
        public const int StartStar2 = 41003;
        /// <summary>
        /// 必须按照顺序激活
        /// </summary>
        public const int StartStar3 = 41004;
        /// <summary>
        /// 激活守护星需要的星力值不足
        /// </summary>
        public const int StartStar4 = 41005;
        /// <summary>
        /// 激活守护星需要的石币不足
        /// </summary>
        public const int StartStar5 = 41006;
        /// <summary>
        /// 激活守护星失败
        /// </summary>
        public const int StartStar6 = 41007;
        /// <summary>
        /// 激活守护星成功
        /// </summary>
        public const int StartStar7 = 41008;
        /// <summary>
        /// 该守护星已经激活
        /// </summary>
        public const int StartStar8 = 41009;
        /// <summary>
        /// 该星座官守护星已经激活完成
        /// </summary>
        public const int StartStar9 = 41010;



        /// <summary>
        /// 出战宠不能进入星阵
        /// </summary>
        public const int InStarTroops1 = 41011;
        /// <summary>
        /// 上传参数不正确
        /// </summary>
        public const int InStarTroops2 = 41012;
        /// <summary>
        /// 该宠物不存在
        /// </summary>
        public const int InStarTroops3 = 41013;

        /// <summary>
        /// 该位置已经存在宠物
        /// </summary>
        public const int InStarTroops4 = 41014;
        /// <summary>
        /// 星阵宠物已经放满
        /// </summary>
        public const int InStarTroops5 = 41015;
        /// <summary>
        /// 宠物进入成功
        /// </summary>
        public const int InStarTroops6 = 41016;



        /// <summary>
        /// 家园或兽栏已满
        /// </summary>
        public const int OutStarTroops1 = 41017;
        /// <summary>
        /// 没有宠物
        /// </summary>
        public const int OutStarTroops2 = 41018;
        /// <summary>
        /// 宠物进入成功
        /// </summary>
        public const int OutStarTroops3 = 41019;


        /// <summary>
        /// 请先放入宠物再开始
        /// </summary>
        public const int GetStarTroops1 = 41020;
        /// <summary>
        /// 没有星力值
        /// </summary>
        public const int GetStarTroops2 = 41021;
        /// <summary>
        /// 采集星阵星力成功
        /// </summary>
        public const int GetStarTroops3 = 41022;


        /// <summary>
        /// 提商守护星激活成功率所需道具不足
        /// </summary>
        public const int StartStar10 = 41023;


        /// <summary>
        /// 你已经达到星力爆发分享次数总上限，无法获得经验值！
        /// </summary>
        public const int StartStarShared1 = 41024;
        /// <summary>
        /// 你已经达到当天的星力爆发分享次数上限，无法获得经验!
        /// </summary>
        public const int StartStarShared2 = 41025;

        /// <summary>
        /// 该位置宠物等级必须达到【{0}】级
        /// </summary>
        public const int InStarTroops7 = 41026;

        /// <summary>
        /// 该位置宠物成长度必须达到【{0}】
        /// </summary>
        public const int InStarTroops8 = 41027;

        /// <summary>
        /// 该星阵放入宠物数量不足,不能激合
        /// </summary>
        public const int IsStartTroops1 = 41028;
    
    }
}
