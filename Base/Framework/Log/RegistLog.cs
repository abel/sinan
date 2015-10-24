using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Log;
using MongoDB.Bson.Serialization.Attributes;

namespace Sinan.Log
{
    /// <summary>
    /// 主动注册
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public sealed class RegistLog : LogBase
    {
        RegistLog() { }

        public RegistLog(int woldid = 0) :
            base(woldid, Optype.Write, Actiontype.InviteRegist)
        { }

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
            sb.Append("&source=");
            sb.Append(source);
            sb.Append("&touid=&toopenid=&level=&itemid=&itemtype=&itemcnt=&modifyexp=&totalexp=&modifycoin=&totalcoin=&modifyfee=&totalfee=&onlinetime=&keycheckret=&safebuf=&remark=&user_num=");
            return sb.ToString();
        }
    }
}
