using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (14XX)
    /// </summary>
    public class ClientCommand
    {
        /// <summary>
        /// 握手
        /// </summary>
        public const string Hand = "hand";
        public const string HandR = "handR";

        /// <summary>
        /// 行走
        /// </summary>
        public const string WalkTo = "walkTo";
        public const string WalkToR = "g.walkToR";

        /// <summary>
        /// 心跳测试
        /// </summary>
        public const string T = "t";
        public const string TR = "tR";
        public const string Err = "err";

        public const string ClientToServer = "ClientToServer";
        public const string ServerToClient = "ServerToClient";

        /// <summary>
        /// 网络连接被断开.
        /// </summary>
        public const string UserDisconnected = "UserDisconnected";
        public const string UserConnected = "UserConnected";

        /// <summary>
        /// 获取所有物品信息
        /// </summary>
        public const string GetGoods = "getGoods";
        public const string GetGoodsR = "o.getGoodsR";

        /// <summary>
        /// 获取所有职业信息
        /// </summary>
        public const string GetRoles = "getRoles";
        public const string GetRolesR = "a.getRolesR";

        /// <summary>
        /// 获取用户详细信息
        /// </summary>
        public const string GetPlayerDetail = "getPlayerDetail";
        public const string GetPlayerDetailR = "a.getPlayerDetailR";

        /// <summary>
        /// 更新玩家属性
        /// </summary>
        public const string UpdateActorR = "a.updateActorR";


        /// <summary>
        /// 丢弃物品
        /// </summary>
        public const string DiscardGoods = "discardGoods";
        public const string DiscardGoodsR = "o.discardGoodsR";

        /// <summary>
        /// 玩家请求进入场景
        /// </summary>
        public const string IntoScene = "intoScene";
        public const string IntoSceneR = "g.intoSceneR";

        /// <summary>
        /// 成功进入场景
        /// </summary>
        public const string IntoSceneSuccess = "IntoSceneSuccess";
        public const string MorePlayer = "g.morePlayersR";

        /// <summary>
        /// 查找传送阵路径
        /// </summary>
        public const string FindPinPath = "findPinPath";
        public const string FindPinPathR = "g.findPinPathR";

        /// <summary>
        /// 玩家退出场景
        /// </summary>
        public const string ExitScene = "exitScene";
        public const string ExitSceneR = "g.exitSceneR";

        /// <summary>
        /// 通知客户端玩家退出游戏
        /// </summary>
        public const string PlayerExitR = "a.playerExitR";

        /// <summary>
        /// 其它玩家进入场景
        /// </summary>
        public const string OtherIntoSceneR = "g.otherIntoSceneR";

        /// <summary>
        /// 开箱
        /// </summary>
        public const string OpenBox = "openBox";
        public const string OpenBoxR = "g.openBoxR";

        /// <summary>
        /// 更新箱子
        /// </summary>
        public const string RefreshBoxR = "g.refreshBoxR";

        /// <summary>
        /// 更新APC
        /// </summary>
        public const string RefreshApcR = "g.refreshApcR";

        /// <summary>
        /// APC状态更改
        /// </summary>
        public const string KillApcR = "g.killApcR";

        /// <summary>
        /// 使用技能
        /// </summary>
        public const string UseSkill = "useSkill";

        /// <summary>
        /// 更新热键
        /// </summary>
        public const string UpdateHotKeys = "updateHotKeys";
        public const string UpdateHotKeysR = "a.updateHotKeysR";

        /// <summary>
        /// 领悟新技能
        /// </summary>
        public const string GetNewSkillR = "a.getNewSkillR";

        /// <summary>
        /// 发送消息给所有在线的玩家
        /// </summary>
        public const string SendMsgToAllPlayer = "sendMsgToAllPlayer";
        public const string SendMsgToAllPlayerR = "m.sendMsgToAllPlayerR";

        /// <summary>
        /// 发送消息给玩家
        /// </summary>
        public const string SendActivtyR = "m.sendActivtyR";

        /// <summary>
        /// 同场景瞬间移动
        /// </summary>
        public const string FastToR = "g.fastToR";

        /// <summary>
        /// 地图直接传送
        /// </summary>
        public const string MapTrans = "mapTrans";
        public const string MapTransR = "g.mapTransR";


        /// <summary>
        /// 获取用户详细信息(可以指定获取哪部分)
        /// </summary>
        public const string GetPlayerDetail2 = "getPlayerDetail2";
        public const string GetPlayerDetail2R = "a.getPlayerDetail2R";

    }

    /// <summary>
    /// 140XX
    /// </summary>
    public class ClientReturn
    {
        /// <summary>
        /// 在组队状态下不能进入
        /// </summary>
        public const int IntoLimit1 = 14001;
        /// <summary>
        /// 进入或途经【{0}】需要{1}级，您的等级不足,可以通过活动、支线任务，日常任务，每日答题，经验秘境，组队刷怪等方式快速提升等级。
        /// </summary>
        public const int IntoLimit2 = 14002;
        /// <summary>
        /// 进入【{0}】最大{1}级，您的等级超过限制
        /// </summary>
        public const int IntoLimit3 = 14003;
        /// <summary>
        /// 很遗憾您无法进入，【{0}】每天最多可进入【{1}】次
        /// </summary>
        public const int IntoLimit4 = 14004;
        /// <summary>
        /// 缺少道具【{0}】,无法进入
        /// </summary>
        public const int IntoLimit5 = 14005;
        /// <summary>
        /// 很遗憾您无法进入，【{0}】开启时间是每日【{1}】
        /// </summary>
        public const int IntoLimit6 = 14006;
        /// <summary>
        /// 队员【{0}】 的等级不符合要求
        /// </summary>
        public const int IntoLimit7 = 14007;

        /// <summary>
        /// 无此任务,不能打怪
        /// </summary>
        public const int SceneBusiness1 = 14008;

        /// <summary>
        /// 进入【{0}】需要{1}石币或{2}晶币，您的货币不足，无法进入
        /// </summary>
        public const int SceneEctype1 = 14009;
        /// <summary>
        /// 进入【{0}】需要{1}晶币，您的晶币不足，无法进入
        /// </summary>
        public const int SceneEctype2 = 14010;
        /// <summary>
        /// 进入【{0}】需要{1}石币，您的石币不足，无法进入
        /// </summary>
        public const int SceneEctype3 = 14011;
        /// <summary>
        /// 必须杀死{0}
        /// </summary>
        public const int SceneEctype4 = 14012;
        /// <summary>
        /// 不能重复打怪
        /// </summary>
        public const int SceneEctype5 = 14013;

        /// <summary>
        /// 组队状态不能进家园
        /// </summary>
        public const int SceneHome1 = 14014;
        /// <summary>
        /// 战斗状态不能进家园
        /// </summary>
        public const int SceneHome2 = 14015;
        /// <summary>
        /// 只能从城市或野外进入
        /// </summary>
        public const int SceneHome3 = 14016;

        /// <summary>
        /// 你的徒弟【{0}】达成【{1}】成就，贡献了【{2}】点感恩值给你
        /// </summary>
        public const int GetOwe1 = 14017;
        /// <summary>
        /// 徒弟感恩
        /// </summary>
        public const int GetOwe2 = 14018;
        /// <summary>
        /// 系统邮件
        /// </summary>
        public const int GetOwe3 = 14019;



        /// <summary>
        /// 只有加入家族才能进入
        /// </summary>
        public const int RobBusiness1 = 14020;
        /// <summary>
        /// 请先处理包袱中的【守护凭证】后再进入
        /// </summary>
        public const int ProBusiness1 = 14021;
        /// <summary>
        /// 包袱满,请整理你的包袱再进行该操作
        /// </summary>
        public const int ProBusiness2 = 14022;

        /// <summary>
        /// 活动已结束
        /// </summary>
        public const int PartBusiness1 = 14023;
        /// <summary>
        /// 只有活动时间才能进入
        /// </summary>
        public const int PartBusiness2 = 14024;
        /// <summary>
        /// 组队状态不能进入
        /// </summary>
        public const int PartBusiness3 = 14025;
        /// <summary>
        /// 每次活动最多可进入【{0}】次
        /// </summary>
        public const int PartBusiness4 = 14026;



        /// <summary>
        /// 您已被禁言,如有疑问请联系GM
        /// </summary>
        public const int SendMsgToPlayers1 = 14027;
        /// <summary>
        /// 发送失败,消息内容超长
        /// </summary>
        public const int SendMsgToPlayers2 = 14028;


        /// <summary>
        /// 玩家角色不存在
        /// </summary>
        public const int PlayerLogin1 = 14029;
        /// <summary>
        /// 角色已冻结
        /// </summary>
        public const int PlayerLogin2 = 14030;


        /// <summary>
        /// 怪物已被击杀
        /// </summary>
        public const int FightSceneApc1 = 14031;
        /// <summary>
        /// 你已跨场景
        /// </summary>
        public const int FightSceneApc2 = 14032;
        /// <summary>
        /// 怪物已被击杀
        /// </summary>
        public const int FightSceneApc3 = 14033;
        /// <summary>
        /// 怪物在战斗中,不能攻击
        /// </summary>
        public const int FightSceneApc4 = 14034;


        /// <summary>
        /// 只有队长可以执行此操作
        /// </summary>
        public const int IntoScene1 = 14035;


        /// <summary>
        /// 对方拒绝了你的入队请求
        /// </summary>
        public const int ReplyApply1 = 14036;

        /// <summary>
        /// 队长不能暂离
        /// </summary>
        public const int AwayTeam1 = 14037;
        /// <summary>
        /// 你已暂离
        /// </summary>
        public const int AwayTeam2 = 14038;

        /// <summary>
        /// 您需要和队伍在同一场景中才能归队
        /// </summary>
        public const int RejoinTeam1 = 14039;

        /// <summary>
        /// 已有队伍
        /// </summary>
        public const int TeamsMediator1 = 14040;
        /// <summary>
        /// 队伍已满
        /// </summary>
        public const int TeamsMediator2 = 14041;
        /// <summary>
        /// 等待队长回复
        /// </summary>
        public const int TeamsMediator3 = 14042;


        /// <summary>
        /// 不可捕捉
        /// </summary>
        public const int ZhuaPu1 = 14043;
        /// <summary>
        /// 包袱已满
        /// </summary>
        public const int ZhuaPu2 = 14044;
        /// <summary>
        /// 捕捉成功
        /// </summary>
        public const int ZhuaPu3 = 14045;
        /// <summary>
        /// 捕捉失败
        /// </summary>
        public const int ZhuaPu4 = 14046;


        /// <summary>
        /// 你被击败,损失了10%的角色经验
        /// </summary>
        public const int FightBusinessPK1 = 14047;

        /// <summary>
        /// 距离太远
        /// </summary>
        public const int CheckBox1 = 14048;
        /// <summary>
        /// 当天开启数达到限制数
        /// </summary>
        public const int CheckBox2 = 14049;
        /// <summary>
        /// 缺少钥匙
        /// </summary>
        public const int CheckBox3 = 14050;
        /// <summary>
        /// 你已进入战斗中,无法打开宝箱
        /// </summary>
        public const int CheckBox4 = 14051;

        /// <summary>
        /// 已达最大人数,不能开启
        /// </summary>
        public const int CheckBox5 = 14052;

        /// <summary>
        /// 登录成功
        /// </summary>
        public const int PlayerLogin3 = 14053;
        /// <summary>
        /// 活动还未开始
        /// </summary>
        public const int IntoPart1 = 14054;

        /// <summary>
        /// 加入家族才能进入
        /// </summary>
        public const int EctypeLimitFamilyMsg1 = 14055;

        /// <summary>
        /// 当日家族贡献值必须达到[{0}]点才能进入
        /// </summary>
        public const int EctypeLimitFamilyMsg2 = 14056;

        /// <summary>
        /// 所有成员必须为同一家族
        /// </summary>
        public const int EctypeLimitFamilyMsg3 = 14057;

        /// <summary>
        /// 有队员暂离,不能进入
        /// </summary>
        public const int EctypeLimitTeamMsg = 14058;

        /// <summary>
        /// 只有队长才能请求进入组队秘境
        /// </summary>
        public const int EctypeLimitTeamMsg1 = 14059;

        /// <summary>
        /// 人数不足,至少需要[{0}]人.
        /// </summary>
        public const int EctypeLimitMinMemberMsg = 14060;

        /// <summary>
        /// 您已被禁言,如有疑问请联系GM
        /// </summary>
        public const int NoTalk = 14061;

        /// <summary>
        /// 发送失败,消息内容超长
        /// </summary>
        public const int MsgToBig = 14062;

        /// <summary>
        /// 处于保护状态,不能PK
        /// </summary>
        public const int PKProtect = 14063;

        /// <summary>
        /// "只能在城市/野外/战场/家园使用此功能"
        /// </summary>
        public const int MapTrans1 = 14064;

        /// <summary>
        /// "只能传送到城市/野外和战场"
        /// </summary>
        public const int MapTrans2 = 14065;

        /// <summary>
        /// "只有会员才能使用传送功能"
        /// </summary>
        public const int MapTrans3 = 14066;


        /// <summary>
        ///"已达传送使用次数限制"
        /// </summary>
        public const int MapTrans4 = 14067;
    }
}
