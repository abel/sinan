using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (17XX)
    /// </summary>
    public class EmailCommand
    {
        /// <summary>
        /// 得到邮件列表
        /// </summary>
        public const string EmailList = "emailList";
        public const string EmailListR = "e.emailListR";

        /// <summary>
        /// 邮件的发送
        /// </summary>
        public const string SendEmail = "sendEmail";
        public const string SendEmailR = "e.sendEmailR";

        /// <summary>
        /// 更新邮件
        /// </summary>
        public const string UpdateEmail = "updateEmail";
        public const string UpdateEmailR = "e.updateEmailR";

        /// <summary>
        /// 删除邮件
        /// </summary>
        public const string DelEmail = "delEmail";
        public const string DelEmailR = "e.delEmailR";

        /// <summary>
        /// 新邮件条数
        /// </summary>
        public const string NewEmailTotal = "newEmailTotal";
        public const string NewEmailTotalR = "e.newEmailTotalR";

        /// <summary>
        /// 提取邮件物品
        /// </summary>
        public const string ExtractGoods = "extractGoods";
        public const string ExtractGoodsR = "e.extractGoodsR";

        /// <summary>
        /// 退售
        /// </summary>
        public const string ExitEmail = "exitEmail";
        public const string ExitEmailR = "e.exitEmailR";

        /// <summary>
        /// 表示私人邮件
        /// </summary>
        public const string Personal = "Personal";

        /// <summary>
        /// 表示系统邮件
        /// </summary>
        public const string System = "System";
    }

    public class EmailReturn 
    {
        /// <summary>
        /// 邮件发送成功
        /// </summary>
        public const int SendEmailSuccess = 23000;
        /// <summary>
        /// 得到新邮件
        /// </summary>
        public const int RecNewEmail = 23001;
        /// <summary>
        /// 有异常
        /// </summary>
        public const int SendExption = 23002;
        /// <summary>
        /// 邮件不存在
        /// </summary>
        public const int NoEmail = 23003;
        /// <summary>
        /// 接收人不正确
        /// </summary>
        public const int NoReceiveID = 23004;
        /// <summary>
        /// 不存在
        /// </summary>
        public const int NoGoods = 23005;
        /// <summary>
        /// 宠物包袱已满
        /// </summary>
        public const int PetBurdenFull = 23006;

        /// <summary>
        /// 对不起，包袱空间不足，请在整理包袱后再次操作
        /// </summary>
        public const int BurdenFull = 23007;
        /// <summary>
        /// 不能发送给自己
        /// </summary>
        public const int NoSelf = 23008;
        /// <summary>
        /// 游戏币不足
        /// </summary>
        public const int NoScore = 23009;
        /// <summary>
        /// 包袱类型不正确
        /// </summary>
        public const int NoBurden = 23010;
        /// <summary>
        /// 你发送的物品不存在
        /// </summary>
        public const int DateError = 23011;
        /// <summary>
        /// 晶币不足
        /// </summary>
        public const int NoCoin = 23012;

        /// <summary>
        /// 退信成功
        /// </summary>
        public const int ExitSuccess = 23013;
        /// <summary>
        /// 非附件不能退信
        /// </summary>
        public const int NoRider = 23014;
        /// <summary>
        /// 系统邮件不能退信
        /// </summary>
        public const int IsSystem = 23015;
        /// <summary>
        /// 标题长度不能超过20个字符
        /// </summary>
        public const int EmailTitalLength = 23016;
        /// <summary>
        /// 邮件内容不能超过300个字会符
        /// </summary>
        public const int EmailContent = 23017;

        /// <summary>        
        /// 新手创建邮件
        /// </summary>
        public const int SendNewEmail = 23018;

        /// <summary>
        /// 您想删除的邮件中存在附件，不允许删除
        /// </summary>
        public const int EmailFuJian = 23019;
        /// <summary>
        /// 收件人不存在
        /// </summary>
        public const int NoExists = 23020;

        /// <summary>
        /// 数据出问题
        /// </summary>
        public const int DataError = 23021;

        /// <summary>
        /// 【{0}】的一封信
        /// </summary>
        public const int SendEmail1 = 23022;
        /// <summary>
        /// 邮件出售成功
        /// </summary>
        public const int ExtractGoods1 = 23023;

        /// <summary>
        /// 夺宝奇兵家族奖励
        /// </summary>
        public const int RobFamilyAward1 = 23024;
        /// <summary>
        /// 了答谢你对家族做出的巨大贡献，家族将夺宝奇兵活动获得的战利品分给你，请查收
        /// </summary>
        public const int RobFamilyAward2 = 23025;
        /// <summary>
        /// 系统邮件
        /// </summary>
        public const int RobFamilyAward3 = 23026;


        /// <summary>
        /// 物品已经提取,不能重复领取
        /// </summary>
        public const int GetEmailGoods1 = 23027;
        /// <summary>
        /// 非附件没有物品或石币可提取
        /// </summary>
        public const int GetEmailGoods2 = 23028;
        /// <summary>
        /// 邮件有效天数
        /// </summary>
        public const int HameDay = 23029;
    }
}
