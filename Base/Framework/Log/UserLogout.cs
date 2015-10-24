using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Sinan.Log
{
    /// <summary>
    /// 用户登出(读操作)
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public sealed class UserLogout : LogBaseEx
    {
        UserLogout() { }

        public UserLogout(int woldid = 0) :
            base(woldid, Optype.Read, Actiontype.UserLogout)
        { }

        /// <summary>
        /// 在线时长(毫秒?)
        /// </summary>
        public int onlinetime
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
            sb.Append("&onlinetime=");
            sb.Append(onlinetime);
            sb.Append("&touid=&toopenid=&level=&itemid=&itemtype=&itemcnt=&modifyexp=&totalexp=&modifycoin=&totalcoin=&modifyfee=&totalfee=&source=&keycheckret=&safebuf=&remark=&user_num=");
            return sb.ToString();
        }
    }
}
