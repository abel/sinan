using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.Extensions;

namespace Sinan.Log
{
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public abstract class LogBase
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public const string version = "1";

        public static int WoldID;

        ObjectId m_id;
        protected LogBase()
        {
            m_domain = 1;
        }

        protected int m_domain;
        protected int m_woldid;
        protected int m_optype;
        protected int m_actionid;
        protected DateTime m_time;
        protected int m_s;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="woldid">应用分区分服时大区的ID</param>
        /// <param name="optype"></param>
        /// <param name="actionid"></param>
        protected LogBase(int woldid, Optype optype, Actiontype actionid)
        {
            m_woldid = woldid;
            m_optype = (int)optype;
            m_actionid = (int)actionid;
            m_time = DateTime.UtcNow;
            m_id = ObjectId.GenerateNewId();
        }

        [BsonId]
        public ObjectId ID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        /// <summary>
        /// 用户IP地址
        /// </summary>
        public long userip
        {
            get;
            set;
        }

        /// <summary>
        /// CGI 或者 ServerIP
        /// 当前处理用户请求的机器IP
        /// </summary>
        public long svrip
        {
            get;
            set;
        }

        /// <summary>
        /// 当前用户的操作时间，
        /// 精确到秒，填入unix时间戳
        /// </summary>
        public DateTime time
        {
            get { return m_time; }
            set { m_time = value; }
        }

        /// <summary>
        /// 用于区分从哪个业务平台进入应用 ：
        /// Qzone 为 1 ；
        /// 腾讯朋友为 2 ；
        /// 腾讯微博为 3 ；
        /// Q+ 平台为 4 ；
        /// 财付通开放平台为 5 ；
        /// QQGame 为 10 ；
        /// 由客户端提上来
        /// </summary>
        public int domain
        {
            get { return m_domain; }
            set { m_domain = value; }
        }

        /// <summary>
        /// 应用分区分服时大区的ID. 如果分区分服，
        /// 则该 ID 为新建服务器时自动分配的域名中的serverid 。
        /// </summary>
        public int worldid
        {
            get { return m_woldid; }
            set { m_woldid = value; }
        }

        /// <summary>
        /// 操作类型
        /// 支付类操作为 1 ；
        /// 留言发表类为 2 ；
        /// 写操作类为 3 ；
        /// 读操作类为 4 ；
        /// 其它为 5
        /// </summary>
        public int optype
        {
            get { return m_optype; }
            set { m_optype = value; }
        }

        /// <summary>
        /// 操作ID
        /// 1~ 1 00 为保留字段，其中 ：
        /// 用户登录为 1 ；
        /// 主动注册为 2 ；
        /// 接受邀请注册为 3 ；
        /// 邀请他人注册是 4 ；
        /// 支付消费 为 5 。
        /// 留言为 6 ；
        /// 留言回复为 7 ；
        /// 如有其它类型的留言发表类操作，请填8 。
        /// 用户登出为 9 ；
        /// 角色登录为 11 ；
        /// 创建角色为 12 ；
        /// 角色退出为 13 ；
        /// 角色实时在线为 14 。
        /// 支付 充值 为 15 。
        /// </summary>
        public int actionid
        {
            get { return m_actionid; }
            set { m_actionid = value; }
        }

        /// <summary>
        /// 操作用户 UID
        /// UID 为应用自身的帐号体系中用户的ID
        /// </summary>
        public int opuid
        {
            get;
            set;
        }

        /// <summary>
        /// 操作用户OpenID
        /// </summary>
        public string opopenid
        {
            get;
            set;
        }

        /// <summary>
        /// 用户登录的session key 
        /// (即openkey,由客户端提上来)
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public string key
        {
            get;
            set;
        }

        /// <summary>
        /// 日志状态,更新用
        /// (指示是否已发送到TM服务器)
        /// </summary>
        public int S
        {
            get { return m_s; }
            set { m_s = value; }
        }

        public abstract string ToString(StringBuilder sb);

        protected virtual void AppendHead(StringBuilder sb)
        {
            sb.Append("version=" + version + "&appid=" + Tencent.OpenSns.AppSign.appid);
            sb.Append("&userip=");
            sb.Append(userip);
            sb.Append("&svrip=");
            sb.Append(svrip);
            sb.Append("&time=");
            sb.Append((long)Sinan.Extensions.UtcTimeExtention.UnixTotalSeconds(time));
            sb.Append("&domain=");
            sb.Append(domain);
            sb.Append("&worldid=");
            sb.Append(worldid);
            sb.Append("&optype=");
            sb.Append(optype);
            sb.Append("&actionid=");
            sb.Append(actionid);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(2000);
            return this.ToString(sb);
        }
    }
}
