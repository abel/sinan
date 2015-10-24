using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Log;
using MongoDB.Bson.Serialization.Attributes;

namespace Sinan.Log
{
    /// <summary>
    /// 实时在线(游戏状态类操作)
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public sealed class OnlineLog : LogBase
    {
        OnlineLog() { }

        public OnlineLog(int woldid) :
            base(woldid, Optype.Other, Actiontype.Online)
        { }

        /// <summary>
        /// 用户在线数量
        /// </summary>
        public int user_num
        {
            get;
            set;
        }

        public override string ToString(StringBuilder sb)
        {
            AppendHead(sb);

            sb.Append("&user_num=");
            sb.Append(user_num);
            sb.Append("&opuid=&opopenid=&key=&onlinetime=&modifyfee=&touid=&toopenid=&source=&level=&itemid=&itemtype=&itemcnt=&modifyexp=&totalexp=&modifycoin=&totalcoin=&totalfee=&keycheckret=&safebuf=&remark=");
            return sb.ToString();
        }
    }
}
