using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (19XX)
    /// </summary>
    public class FamilyCommand
    {
        /// <summary>
        /// 得到家族列表
        /// </summary>
        public const string FamilyList = "familyList";
        public const string FamilyListR = "f.familyListR";

        /// <summary>
        /// 得到家族成员列表信息
        /// </summary>
        public const string FamilyMembers = "familyMembers";
        public const string FamilyMembersR = "f.familyMembersR";

        /// <summary>
        /// 家族创建
        /// </summary>
        public const string FamilyCreate = "familyCreate";
        public const string FamilyCreateR = "f.familyCreateR";

        /// <summary>
        /// 申请加入家族
        /// </summary>
        public const string FamilyApply = "familyApply";
        public const string FamilyApplyR = "f.familyApplyR";

        /// <summary>
        /// 申请入族回复
        /// </summary>
        public const string FamilyApplyBack = "familyApplyBack";
        public const string FamilyApplyBackR = "f.familyApplyBackR";

        /// <summary>
        /// 邀请加入家族
        /// </summary>
        public const string FamilyInvite = "familyInvite";
        public const string FamilyInviteR = "f.familyInviteR";

        /// <summary>
        /// 邀请回复
        /// </summary>
        public const string FamilyInviteBack = "familyInviteBack";
        public const string FamilyInviteBackR = "f.familyInviteBackR";

        /// <summary>
        /// 退出家族
        /// </summary>
        public const string ExitFamily = "exitFamily";
        public const string ExitFamilyR = "f.exitFamilyR";

        /// <summary>
        /// 开除成员
        /// </summary>
        public const string FamilyFire = "familyFire";
        public const string FamilyFireR = "f.familyFireR";

        /// <summary>
        /// 任命副族长
        /// </summary>
        public const string AppointedNegative = "appointedNegative";
        public const string AppointedNegativeR = "f.appointedNegativeR";

        /// <summary>
        /// 撤消副族长
        /// </summary>
        public const string FireNegative = "fireNegative";
        public const string FireNegativeR = "f.fireNegativeR";

        /// <summary>
        /// 更新家族公告
        /// </summary>
        public const string UpdateFamilyNotice = "updateFamilyNotice";
        public const string UpdateFamilyNoticeR = "f.updateFamilyNoticeR";

        /// <summary>
        /// 解散家族
        /// </summary>
        public const string DissolveFamily = "dissolveFamily";
        public const string DissolveFamilyR = "f.dissolveFamilyR";

        /// <summary>
        /// 移交族长
        /// </summary>
        public const string TransferBoss = "transferBoss";
        public const string TransferBossR = "f.transferBossR";

        /// <summary>
        /// 得到家族经验
        /// </summary>
        public const string FamilyExperience = "FamilyExperience";
        public const string FamilyExperienceR = "f.familyExperienceR";

        /// <summary>
        /// 得到家族技能
        /// </summary>
        public const string FamilySkill = "familySkill";
        public const string FamilySkillR = "f.familySkillR";

        /// <summary>
        /// 学习家族技能
        /// </summary>
        public const string StudyFamilySkill = "studyFamilySkill";
        public const string StudyFamilySkillR = "f.studyFamilySkillR";

        /// <summary>
        /// 获取Boss列表
        /// </summary>
        public const string BossList = "bossList";
        public const string BossListR = "f.bossListR";

        /// <summary>
        /// 招唤Boss
        /// </summary>
        public const string SummonBoss = "summonBoss";
        public const string SummonBossR = "f.summonBossR";


        /// <summary>
        /// 挑战Boss
        /// </summary>
        public const string FightBoss = "fightBoss";
        public const string FightBossR = "f.fightBossR";

        /// <summary>
        /// 领取Boss奖励
        /// </summary>
        public const string BossAward = "bossAward";
        public const string BossAwardR = "f.bossAwardR";

        /// <summary>
        /// 创建家族
        /// </summary>
        public const string AddFamily = "addFamily";

    }

    public class FamilyReturn
    {
        /// <summary>
        /// 家族创建成功
        /// </summary>
        public const int FamilyCreateSuccess = 23100;
        /// <summary>
        /// 家族名称重名请重新输入
        /// </summary>
        public const int FamilyNameExist = 23101;
        /// <summary>
        /// 存在非法字符请重新填写
        /// </summary>
        public const int FamilyNameError = 23102;
        /// <summary>
        /// 创建家族等级不足
        /// </summary>
        public const int NoLevel = 23103;
        /// <summary>
        /// 家族名称不能超过指定长度
        /// </summary>
        public const int NameOutLength = 23104;
        /// <summary>
        /// 创建家族游戏币不足
        /// </summary>
        public const int NoScore = 23105;
        /// <summary>
        /// 已经加入家族
        /// </summary>
        public const int ExistFamily = 23106;
        /// <summary>
        /// 时间格式不正确
        /// </summary>
        public const int DateError = 23107;
        /// <summary>
        /// 冻结期不能操作
        /// </summary>
        public const int FreezeDate = 23108;
        /// <summary>
        /// 已在家族中，不可申请加入多个家族
        /// </summary>
        public const int FamilyExist = 23109;
        /// <summary>
        /// 家族信息不正确
        /// </summary>
        public const int FamilyError = 23110;

        /// <summary>
        /// 申请家族成功
        /// </summary>
        public const int FamilyApplySuccess = 23111;
        /// <summary>
        /// 已经提出入族申请
        /// </summary>
        public const int IsApply = 23112;
        /// <summary>
        /// 家族不存在
        /// </summary>
        public const int NoFamily = 23113;
        /// <summary>
        /// 族长不在线
        /// </summary>
        public const int FamilyBossLoss = 23114;
        /// <summary>
        /// 回复数据不正确
        /// </summary>
        public const int ApplyBackError = 23115;
        /// <summary>
        /// 没有权限
        /// </summary>
        public const int NoPower = 23116;
        /// <summary>
        /// 入族成功
        /// </summary>
        public const int FamilySuccess = 23117;
        /// <summary>
        /// 没有入族
        /// </summary>
        public const int NoAddFamily = 23118;
        /// <summary>
        /// 更新公告成功
        /// </summary>
        public const int NoticeSuccess = 23119;
        /// <summary>
        /// 族长不能退出
        /// </summary>
        public const int NoExitBoss = 23120;
        /// <summary>
        /// 退出成功
        /// </summary>
        public const int ExitSuccess = 23121;
        /// <summary>
        /// 不能开除族长
        /// </summary>
        public const int NoFireBoss = 23122;
        /// <summary>
        /// 不能开除自己
        /// </summary>
        public const int NoFireSelf = 23123;
        /// <summary>
        /// 不能任命自己
        /// </summary>
        public const int NoAppointedSelf = 23124;
        /// <summary>
        /// 非族员不能任员为副族长
        /// </summary>
        public const int NoAppointedMember = 23125;
        /// <summary>
        /// 副族长已经达到上限，不能再任命
        /// </summary>
        public const int AppointedCount = 23126;
        /// <summary>
        /// 任命副族长成功
        /// </summary>
        public const int AppointedSuccess = 23127;

        /// <summary>
        /// 不能撤消自己
        /// </summary>
        public const int FireNegativeSelf = 23128;
        /// <summary>
        /// 非副族长撤消无效
        /// </summary>
        public const int NoFireNegative = 23129;
        /// <summary>
        /// 撤消成功
        /// </summary>
        public const int FireNegativeSuccess = 23130;
        /// <summary>
        /// 不能移交给自己
        /// </summary>
        public const int TransferBossSelf = 23131;
        /// <summary>
        /// 移交成员不正确
        /// </summary>
        public const int NoTransferBoss = 23132;
        /// <summary>
        /// 解散成功
        /// </summary>
        public const int DissolveSuccess = 23133;

        /// <summary>
        /// 入族申请已经达到上限
        /// </summary>
        public const int InApplyLimit = 23134;
        /// <summary>
        /// 家族人数达到上限
        /// </summary>
        public const int PersonsLimit = 23135;

        /// <summary>
        /// 邀请入族成功
        /// </summary>
        public const int InviteInFamily = 23136;
        /// <summary>
        /// 邀请信息不正确
        /// </summary>
        public const int InviteError = 23137;
        /// <summary>
        /// 已经邀请了
        /// </summary>
        public const int IsInvite = 23138;

        /// <summary>
        /// 已经达到技能上限不能再升级
        /// </summary>
        public const int SkillMax = 23139;
        /// <summary>
        /// 当前贡献值不足
        /// </summary>
        public const int NoDevote = 23140;
        /// <summary>
        /// 家族等级不足不能升级该技能
        /// </summary>
        public const int NoFamilyLevel = 23141;
        /// <summary>
        /// 学习家族技能成功
        /// </summary>
        public const int StudySuccess = 23142;
        /// <summary>
        /// 申请入族被拒绝
        /// </summary>
        public const int ApplyRefuse = 23143;

        /// <summary>
        /// 创建家族成功
        /// </summary>
        public const int CreateFamilySuccess = 23144;
        /// <summary>
        /// 公告长度不能超过100
        /// </summary>
        public const int NoticeLength = 23145;

        /// <summary>
        /// 家族创建成功邮件23146
        /// </summary>
        public const int CreateFamilyEmail = 23146;
        /// <summary>
        /// 申请加入家族通知23147
        /// </summary>
        public const int ApplyFamilySuccess = 23147;
        /// <summary>
        /// 申请回复成功后的通知23148
        /// </summary>
        public const int ApplyBackSuccess = 23148;
        /// <summary>
        /// 邀请进入家族通知23149
        /// </summary>
        public const int InviteFamilySuccess = 23149;
        /// <summary>
        /// 邀请回复通知23150
        /// </summary>
        public static int InviteBackSuccess = 23150;
        /// <summary>
        /// 退出成功通知23151
        /// </summary>
        public static int ExitFamilySuccess = 23151;
        /// <summary>
        /// 开除成员通知23152
        /// </summary>
        public static int FireFamilySuccess = 23152;
        /// <summary>
        /// 任命副族长通知23153
        /// </summary>
        public static int NegativeSuccess = 23153;
        /// <summary>
        /// 撤消副族长通知23154
        /// </summary>
        public static int FireNegativeClientSuccess = 23154;
        /// <summary>
        /// 解散家族通知23155
        /// </summary>
        public static int DissolveClientSuccess = 23155;
        /// <summary>
        /// 移交族长通知23156
        /// </summary>
        public static int TransferBossSuccess = 23156;
        /// <summary>
        /// 你被开除出【{0}】家族
        /// </summary>
        public static int FireFamily = 23157;


        /// <summary>
        /// "【{0}】邀请加入【{1}】"
        /// </summary>
        public static int LoginSuccess1 = 23158;

        /// <summary>
        /// "【{0}】申请加入【{1}】"
        /// </summary>
        public static int LoginSuccess2 = 23159;

        /// <summary>
        /// 家族成员职称  (族长|副族长|族员)
        /// </summary>
        public static int FamilyRole = 23160;

        /// <summary>
        /// 只有族长或副族长才能召唤BOSS！
        /// </summary>
        public static int NoPowerSummonBoss = 23161;

        /// <summary>
        /// 只有族长（副族长）才可带队挑战！
        /// </summary>
        public static int NoPowerFightBoss = 23162;

        /// <summary>
        /// 当日家族贡献值不足，无法召唤BOSS
        /// </summary>
        public static int DayDevNotEnough = 23163;

        /// <summary>
        /// 家族任务完成数量不足，无法进入单人秘境
        /// </summary>
        public static int FamilyTaskNotEnough = 23164;

        /// <summary>
        /// 已领取
        /// </summary>
        public static int CannotBossAward = 23165;

        /// <summary>
        /// 击杀家族Boss
        /// </summary>
        public static int KillFamilyBoss = 23166;

    }
}
