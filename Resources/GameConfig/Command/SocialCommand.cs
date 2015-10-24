using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (36XX)
    /// </summary>
    public class SocialCommand
    {
        /// <summary>
        /// 得到社交关系列表
        /// </summary>
        public const string SocialList = "socialList";
        public const string SocialListR = "d.socialListR";

        /// <summary>
        /// 添加好友
        /// </summary>
        public const string AddFriends = "addFriends";
        public const string AddFriendsR = "d.addFriendsR";

        /// <summary>
        /// 删除好友
        /// </summary>
        public const string DelFriends = "delFriends";
        public const string DelFriendsR = "d.delFriendsR";

        /// <summary>
        /// 拜师申请
        /// </summary>
        public const string MasterApply = "masterApply";
        public const string MasterApplyR = "d.masterApplyR";

        /// <summary>
        /// 拜师回复
        /// </summary>
        public const string MasterBack = "masterBack";
        public const string MasterBackR = "d.masterBackR";

        /// <summary>
        /// 收徒申请
        /// </summary>
        public const string ApprenticeApply = "apprenticeApply";
        public const string ApprenticeApplyR = "d.apprenticeApplyR";

        /// <summary>
        /// 收徒回复
        /// </summary>
        public const string ApprenticeBack = "apprenticeBack";
        public const string ApprenticeBackR = "d.apprenticeBackR";

        /// <summary>
        /// 解除师徒关系
        /// </summary>
        public const string DelMaster = "delMaster";
        public const string DelMasterR = "d.delMasterR";

        /// <summary>
        /// 升级解除师徒关系
        /// </summary>
        public const string UpDelMentorR = "d.upDelMentorR";
        /// <summary>
        /// 添加仇人
        /// </summary>
        public const string AddEnemy = "addEnemy";
        public const string AddEnemyR = "d.addEnemyR";

        /// <summary>
        /// 删除仇人
        /// </summary>
        public const string DelEnemy = "delEnemy";
        public const string DelEnemyR = "d.delEnemyR";

        /// <summary>
        /// 出师
        /// </summary>
        public const string OutMaster = "outMaster";
        public const string OutMasterR = "d.outMasterR";


        /// <summary>
        /// 好友申请
        /// </summary>
        public const string FriendsApply = "friendsApply";
        public const string FriendsApplyR = "d.friendsApplyR";

        /// <summary>
        /// 申请回复
        /// </summary>
        public const string FriendsBack = "friendsBack";
        public const string FriendsBackR = "d.friendsBackR";

        /// <summary>
        /// 晶币消费
        /// </summary>
        public const string ConsumeCoin = "consumeCoin";

        /// <summary>
        /// 招唤师傅
        /// </summary>
        public const string SummonMaster = "summonMaster";
        public const string SummonMasterR = "d.summonMasterR";

        /// <summary>
        /// 回复招唤
        /// </summary>
        public const string ReplySummon = "replySummon";
        public const string ReplySummonR = "d.replySummonR";

        /// <summary>
        /// 好友祝福
        /// </summary>
        public const string FriendsBless = "friendsBless";
        public const string FriendsBlessR = "d.friendsBlessR";

        /// <summary>
        /// 邀请QQ好友
        /// </summary>
        public const string InvitedFriends = "invitedFriends";
        public const string InvitedFriendsR = "w.invitedFriendsR";

        /// <summary>
        /// 好友分享
        /// </summary>
        public const string FriendShare = "friendShare";
        public const string FriendShareR = "d.friendShareR";
    }
    public class SocialReturn
    {
        /// <summary>
        /// 删除成功
        /// </summary>
        public const int DelSuccuess = 22700;
        /// <summary>
        /// 用户不存在
        /// </summary>
        public const int UserInfoError = 22701;
        /// <summary>
        /// 已经存在不能加
        /// </summary>
        public const int IsLet = 22702;
        /// <summary>
        /// 已经拜师
        /// </summary>
        public const int Master = 22703;
        /// <summary>
        /// 徒弟必须小于40级
        /// </summary>
        public const int MasterNoLevel = 22704;
        /// <summary>
        /// 师傅等级必须大于39级
        /// </summary>
        public const int MasterLevelGap = 22705;
        /// <summary>
        /// 已经是好友
        /// </summary>
        public const int Friends = 22706;
        /// <summary>        
        /// 玩家处于拜师与收徒的冷冻期
        /// </summary>
        public const int NoFreezeDate = 22707;
        /// <summary>
        /// 玩家不在线
        /// </summary>
        public const int NoOnLine = 22708;
        /// <summary>
        /// 拜师申请成功
        /// </summary>
        public const int MasterApply = 22709;

        /// <summary>
        /// 拜师回复编号不正确
        /// </summary>
        public const int MasterBackError = 22710;
        /// <summary>
        /// 拒绝拜师申请
        /// </summary>
        public const int MasterDeny = 22711;
        /// <summary>
        /// 仇人
        /// </summary>
        public const int Enemy = 22712;
        /// <summary>
        /// 徒弟
        /// </summary>
        public const int Apprentice = 22713;

        /// <summary>
        /// 收徒申请成功
        /// </summary>
        public const int ApprenticeApply = 22714;
        /// <summary>
        /// 收徒编号不正确
        /// </summary>
        public const int ApprenticeBackError = 22715;
        /// <summary>
        /// 删除好友成功
        /// </summary>
        public const int DelFriends = 22716;
        /// <summary>
        /// 解除师徒关系成功
        /// </summary>
        public const int DelMentor = 22717;

        /// <summary>
        /// 对同一角色1天只能申请一次收徒请求
        /// </summary>
        public const int IsApprentice = 22718;
        /// <summary>
        /// 对同一角色1天只能申请一次拜师请求
        /// </summary>
        public const int IsMaster = 22719;
        /// <summary>
        /// 不能对自己请求
        /// </summary>
        public const int NoSelf = 22720;
        /// <summary>
        /// 申请已经达到上限，请过段时间再申请
        /// </summary>
        public const int AppNumber = 22721;



        /// <summary>
        /// 你已经收【{0}】为徒！
        /// </summary>
        public const int MasterBack1 = 22722;
        /// <summary>
        /// 你已经拜【{0}】为师！
        /// </summary>
        public const int MasterBack2 = 22723;
        /// <summary>
        /// 你已经收【{0}】为徒！
        /// </summary>
        public const int MasterBack3 = 22724;
        /// <summary>
        /// 你已经拜【{0}】为师！
        /// </summary>
        public const int MasterBack4 = 22725;

        /// <summary>
        /// 你与【{0}】成功解除师徒关系！
        /// </summary>
        public const int DelMaster1 = 22726;
        /// <summary>
        /// 师徒【{0}】终止与你的师徒关系!
        /// </summary>
        public const int DelMaster2 = 22727;
        /// <summary>
        /// 你与【{0}】成功解除师徒关系！
        /// </summary>
        public const int DelMaster3 = 22728;
        /// <summary>
        /// 师傅【{0}】终止与你的师徒关系!
        /// </summary>
        public const int DelMaster4 = 22729;

        /// <summary>
        /// 因你等级上升与【{0}】成功解除师徒关系！
        /// </summary>
        public const int UpDelMentor1 = 22730;
        /// <summary>
        /// 因徒弟【{0}】等级上升,终止与你的师徒关系!
        /// </summary>
        public const int UpDelMentor2 = 22731;
        /// <summary>
        /// 因你等级上升与【{0}】成功解除师徒关系！
        /// </summary>
        public const int UpDelMentor3 = 22732;
        /// <summary>
        /// 因师傅【{0}】等级上升,终止与你的师徒关系!
        /// </summary>
        public const int UpDelMentor4 = 22733;


        /// <summary>
        /// <f  color='ff3300'>【{0}】申请拜你为师，是否同意？</f>
        /// </summary>
        public const int MasterApply1 = 22734;
        /// <summary>
        /// <f  color='ff3300'>【{0}】想收你为徒,是否同意？</f>
        /// </summary>
        public const int MasterApply2 = 22735;

        /// <summary>
        /// 徒弟达到上限
        /// </summary>
        public const int MasterCount = 22736;
        /// <summary>
        /// 徒弟达到上限
        /// </summary>
        public const int ApprenticeCount = 22737;


        /// <summary>
        /// 出师等能不足
        /// </summary>
        public const int OutMasterLevel = 22738;
        /// <summary>
        /// 师傅不存在
        /// </summary>
        public const int NoMaster = 22739;
        /// <summary>
        /// 出师奖励
        /// </summary>
        public const int OutMasterAward = 22740;
        /// <summary>
        /// 包袱满，请先清理包袱再进行该操作
        /// </summary>
        public const int BurdenFull = 22741;



        /// <summary>
        /// 师傅正处在冻结期,72小时内无法收徒
        /// </summary>
        public const int MasterApply3 = 22742;
        /// <summary>
        /// 徒弟正处在冻结期,72小时内无法拜师
        /// </summary>
        public const int MasterApply4 = 22743;



        public const int MasterBack5 = 22744;


        /// <summary>
        /// 【{0}】解除与你的好友关系
        /// </summary>
        public const int DelFriends1 = 22745;
        /// <summary>
        /// 你解除与【{0}】的好友关系
        /// </summary>
        public const int DelFriends2 = 22746;

        /// <summary>
        /// 等待【{0}】回复
        /// </summary>
        public const int FriendsApply = 22747;

        /// <summary>
        /// 【{0}】拒绝加你为好友
        /// </summary>
        public const int FriendsBack1 = 22748;
        /// <summary>
        /// 【{0}】与你成为好友,愿你们友谊在石器宝贝中长存
        /// </summary>
        public const int FriendsBack2 = 22749;
        /// <summary>
        /// 【{0}】与你成为好友,愿你们友谊在石器宝贝中长存
        /// </summary>
        public const int FriendsBack3 = 22750;

        /// <summary>
        /// 配置不正确
        /// </summary>
        public const int FriendsBless1 = 22751;
        /// <summary>
        /// 你的包袱已满，不能祝福好友
        /// </summary>
        public const int FriendsBless2 = 22752;
        /// <summary>
        /// 今日祝福次已经用完,每日最多能祝福好友10次
        /// </summary>
        public const int FriendsBless3 = 22753;
        /// <summary>
        /// 每日同一好友只能祝福一次
        /// </summary>
        public const int FriendsBless4 = 22754;
        /// <summary>
        /// 你没有鲜花，无法祝福
        /// </summary>
        public const int FriendsBless5 = 22755;
        /// <summary>
        /// 因为你好友【{0}】对你的祝福，你获得了【{1}】，为了你们的友谊，你也快祝福祝福他吧！
        /// </summary>
        public const int FriendsBless6 = 22756;
        /// <summary>
        /// 好友祝福
        /// </summary>
        public const int FriendsBless7 = 22757;
        /// <summary>
        /// 系统邮件
        /// </summary>
        public const int FriendsBless8 = 22758;
        /// <summary>
        /// 祝福好友成功
        /// </summary>
        public const int FriendsBless9 = 22759;

        /// <summary>
        /// 不能将自己加为好友
        /// </summary>
        public const int CheckFriends1 = 22760;
        /// <summary>
        /// 【{0}】不在线,不能加为好友
        /// </summary>
        public const int CheckFriends2 = 22761;
        /// <summary>
        /// 请先解除与【{0}】的仇人关系再添加为好友
        /// </summary>
        public const int CheckFriends3 = 22762;
        /// <summary>
        /// 【{0}】与你已经是好友不能再添加
        /// </summary>
        public const int CheckFriends4 = 22763;
        /// <summary>
        /// 你的好友已满，其他玩家添加你好友失败
        /// </summary>
        public const int CheckFriends5 = 22764;
        /// <summary>
        /// 对方好友已满，无法添加
        /// </summary>
        public const int CheckFriends6 = 22765;
        /// <summary>
        /// 【{0}】已经将你加为仇人不能加为好代友
        /// </summary>
        public const int CheckFriends7 = 22766;
        /// <summary>
        /// 【{0}】与你已经是好友不能再添加
        /// </summary>
        public const int CheckFriends8 = 22767;
        /// <summary>
        /// 对方好友已满，无法添加
        /// </summary>
        public const int CheckFriends9 = 22768;
        /// <summary>
        /// 你的好友已满，其他玩家添加你好友失败
        /// </summary>
        public const int CheckFriends10 = 22769;


        /// <summary>
        /// 你的徒弟【{0}】消费晶币，为感谢师傅的栽培，为师傅送上了【{1}】点劵
        /// </summary>
        public const int ConsumeCoin1 = 22770;
        /// <summary>
        /// 徒弟消费奖励点劵
        /// </summary>
        public const int ConsumeCoin2 = 22771;
        /// <summary>
        /// 你获得了【{0}】点感恩值
        /// </summary>
        public const int ConsumeCoin3 = 22772;
        /// <summary>
        /// 徒弟消费奖励感恩值
        /// </summary>
        public const int ConsumeCoin4 = 22773;

        /// <summary>
        /// 请先拜师
        /// </summary>
        public const int SummonMaster1 = 22774;
        /// <summary>
        /// 你没有【召唤羽毛】，无法召唤你的师傅
        /// </summary>
        public const int SummonMaster2 = 22775;
        /// <summary>
        /// 当前拜师没上线
        /// </summary>
        public const int SummonMaster3 = 22776;


        /// <summary>
        /// 你师傅看见你如此努力的成长，成为了高徒，开心的将其积攒多年的宝藏赠送于你，并教导不忘师训继续努力！
        /// </summary>
        public const int UpMaster1 = 22777;
        /// <summary>
        /// 师傅送上的一份厚礼
        /// </summary>
        public const int UpMaster2 = 22778;
        /// <summary>
        /// 系统
        /// </summary>
        public const int UpMaster3 = 22779;
        /// <summary>
        /// 您老人家还好吗？经过这么多年的奋斗，我终于达到了40级，也成为了高徒，这一天的到来离不开您的关怀和照顾，感谢您！
        /// </summary>
        public const int UpMaster4 = 22780;
        /// <summary>
        /// 徒弟送上的一份厚礼
        /// </summary>
        public const int UpMaster5= 22781;
        /// <summary>
        /// 您的徒弟【{0}】已成为高徒，快去祝贺他吧！
        /// </summary>
        public const int UpMaster6 = 22782;
        /// <summary>
        /// 你已成功成为高徒，你的师傅为你送上了一份贺礼！
        /// </summary>
        public const int UpMaster7 = 22783;
    }
}
