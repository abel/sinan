using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (23XX)
    /// </summary>
    public class GMCommand
    {
        /// <summary>
        /// 客户端异常日志
        /// </summary>
        public const string ClientError = "clientError";

        /// <summary>
        /// 客户报告问题
        /// </summary>
        public const string ClinetReport = "clientReport";

        /// <summary>
        /// GM返回
        /// </summary>
        public const string GMR = "a.gmR";

        /// <summary>
        /// 踢出指定用户
        /// </summary>
        public const string KickUser = "kickUser";

        /// <summary>
        /// 当前在线人数
        /// </summary>
        public const string Online = "online";
        public const string OnlineR = "a.onlineR";

        /// <summary>
        /// 设置角色状态
        /// </summary>
        public const string SetPlayerState = "setPlayerState";

        /// <summary>
        /// 查看角色信息
        /// </summary>
        public const string ViewPlayer = "viewPlayer";
        public const string ViewPlayerR = "a.viewPlayerR";

        /// <summary>
        /// 设置禁言
        /// </summary>
        public const string SetTalk = "setTalk";

        /// <summary>
        /// 推送公告
        /// </summary>
        public const string Notice = "notice";

        /// <summary>
        /// 开启多倍经验
        /// </summary>
        public const string DoubleExp = "doubleexp";

        /// <summary>
        /// 开启活动
        /// </summary>
        public const string Part = "part";

        /// <summary>
        /// 充金币
        /// </summary>
        public const string Coin = "coin";

        /// <summary>
        /// 充石器币
        /// </summary>
        public const string Score = "score";

        /// <summary>
        /// 充点券
        /// </summary>
        public const string Bond = "bond";

        /// <summary>
        /// 添加角色经验
        /// </summary>
        public const string Exp = "exp";

        /// <summary>
        /// 添加星力
        /// </summary>
        public const string Power = "power";

        /// <summary>
        /// 删除某个任务
        /// </summary>
        public const string TaskRemove = "taskremove";

        /// <summary>
        /// 重新触发某个任务
        /// </summary>
        public const string TaskAct = "taskact";

        /// <summary>
        /// 任务重置
        /// </summary>
        public const string TaskReset = "taskReset";

        /// <summary>
        /// 得到任务ID
        /// </summary>
        public const string TaskId = "taskid";

        /// <summary>
        /// 根据道具名取具ID
        /// </summary>
        public const string Goodsid = "goodsid";

        /// <summary>
        /// 道具赠送
        /// </summary>
        public const string Getgood = "getgood";

        /// <summary>
        /// 道具移除
        /// </summary>
        public const string GoodRemove = "goodremove";

        /// <summary>
        /// 添加宠物经验
        /// </summary>
        public const string PetExp = "petexp";

        /// <summary>
        /// 添加角色技能
        /// </summary>
        public const string Skill = "skill";

        /// <summary>
        /// 添加宠物技能
        /// </summary>
        public const string Pskill = "pskill";

        /// <summary>
        /// 家族升级
        /// </summary>
        public const string FamilyUp = "familyup";

        /// <summary>
        /// 退出所有玩家
        /// </summary>
        public const string Exitall = "exitall";

        /// <summary>
        /// 重启服务器(暂时屏蔽)
        /// </summary>
        public const string Restart = "restart";

        /// <summary>
        /// 邮件查询
        /// </summary>
        public const string SelectEmail = "selectemail";
        public const string SelectEmailR = "a.selectemailR";

        /// <summary>
        /// GM删除别人邮件
        /// </summary>
        public const string GMDelEmail = "gMDelEmail";

        /// <summary>
        /// GM拍卖行出售列表
        /// </summary>
        public const string GMAuctionList = "gMAuctionList";
        public const string GMAuctionListR = "a.gMAuctionListR";

        /// <summary>
        /// GM删除拍卖行数据
        /// </summary>
        public const string GMAuctionDel = "gMAuctionDel";
        public const string GMAuctionDelR = "a.gMAuctionDelR";

        /// <summary>
        /// GM清理包袱，仓库,家园,兽栏
        /// </summary>
        public const string GMBurdenClear = "gMBurdenClear";
        public const string GMBurdenClearR = "a.gMBurdenClearR";

        /// <summary>
        /// GM操作商诚信息
        /// </summary>
        public const string GMMallInfo = "gMMallInfo";
        public const string GMMallInfoR = "a.gMMallInfoR";


        /// <summary>
        /// 得到公告列表
        /// </summary>
        public const string NoticeList = "noticeList";
        public const string NoticeListR = "a.noticeListR";

        /// <summary>
        /// 公告更新
        /// </summary>
        public const string UpdateNotice = "updateNotice";
        public const string UpdateNoticeR = "a.updateNoticeR";

        /// <summary>
        /// 开始GM服务(内部用)
        /// </summary>
        public const string GMStart = "GMStart";

        /// <summary>
        /// 活动更新
        /// </summary>
        public const string UpdatePart = "updatePart";

        /// <summary>
        /// GM邮件发送
        /// </summary>
        public const string EmailSend = "emailSend";
        /// <summary>
        /// 家族设置
        /// </summary>
        public const string FamilySite = "familySite";
    }

    public class GMReturn
    {
        /// <summary>
        /// 角色【{0}】不存在
        /// </summary>
        public const int NoName = 23601;

        /// <summary>
        /// 【{0}】成功被踢出
        /// </summary>
        public const int Exit = 23602;

        /// <summary>
        /// 添加经验成功
        /// </summary>
        public const int AddExp = 23603;

        /// <summary>
        /// 公告通知
        /// </summary>
        public const int Notice = 23604;

        /// <summary>
        /// 为【{0}】充入点券:{1}
        /// </summary>
        public const int AddBond = 23605;

        /// <summary>
        /// 解除成功【{0}】
        /// </summary>
        public const int KickUser = 23606;

        /// <summary>
        /// 场景信息不正确【{0}】
        /// </summary>
        public const int Online = 23607;

        /// <summary>
        /// 玩家【{0}】,状态:【{1}】
        /// </summary>
        public const int SetPlayerState = 23608;

        /// <summary>
        /// 禁言成功【{0}】
        /// </summary>
        public const int SetTalk1 = 23609;

        /// <summary>
        /// 解除禁言【{0}】
        /// </summary>
        public const int SetTalk2 = 23610;

        /// <summary>
        /// 公告成功
        /// </summary>
        public const int Notice1 = 23611;

        /// <summary>
        /// 设置成功,当前经验:【{0}】
        /// </summary>
        public const int DoubleExp = 23612;

        /// <summary>
        /// 活动开启成功:【{0}】
        /// </summary>
        public const int OpenPart = 23613;

        /// <summary>
        /// 活动更新成功
        /// </summary>
        public const int UpdatePart = 23614;
    }
}
