using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Sinan.Log
{
    /// <summary>
    /// 接受邀请注册
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public sealed class InviteRegist1 : LogBase
    {
        InviteRegist1() { }

        public InviteRegist1(int woldid = 0) :
            base(woldid, Optype.Write, Actiontype.AInviteReg)
        { }

        /// <summary>
        /// 邀请人的uid
        /// </summary>
        public int? touid
        {
            get;
            set;
        }

        /// <summary>
        /// 邀请人的openid，
        /// </summary>
        public string toopenid
        {
            get;
            set;
        }

        /// <summary>
        /// 操作来源
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public string source
        {
            get;
            set;
        }

        public override string ToString(StringBuilder sb)
        {
            AppendHead(sb);
            sb.Append("&opuid=");
            sb.Append(opuid);
            sb.Append("&opopenid=");
            sb.Append(opopenid);
            sb.Append("&key=");
            sb.Append(key);
            sb.Append("&touid=");
            sb.Append(touid);
            sb.Append("&toopenid=");
            sb.Append(toopenid);
            sb.Append("&source=");
            sb.Append(source);
            sb.Append("&level=&itemid=&itemtype=&itemcnt=&modifyexp=&totalexp=&modifycoin=&totalcoin=&modifyfee=&totalfee=&onlinetime=&keycheckret=&safebuf=&remark=&user_num=");
            return sb.ToString();
        }


    }

    /// <summary>
    /// 邀请他人注册
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public sealed class InviteRegist2 : LogBase
    {
        InviteRegist2() { }

        public InviteRegist2(int woldid = 0) :
            base(woldid, Optype.Read, Actiontype.SInviteReg)
        { }

        /// <summary>
        /// 被邀请人的 uid ，
        /// </summary>
        public int? touid
        {
            get;
            set;
        }

        /// <summary>
        /// 被邀请人的 openid ，
        /// </summary>
        public string toopenid
        {
            get;
            set;
        }

        public override string ToString(StringBuilder sb)
        {
            AppendHead(sb);

            sb.Append("&opuid=");
            sb.Append(opuid);
            sb.Append("&opopenid=");
            sb.Append(opopenid);
            sb.Append("&key=");
            sb.Append(key);
            sb.Append("&touid=");
            sb.Append(touid);
            sb.Append("&toopenid=");
            sb.Append(toopenid);
            sb.Append("&source=&level=&itemid=&itemtype=&itemcnt=&modifyexp=&totalexp=&modifycoin=&totalcoin=&modifyfee=&totalfee=&onlinetime=&keycheckret=&safebuf=&remark=&user_num=");
            return sb.ToString();
        }
    }
}
