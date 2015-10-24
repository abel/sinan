using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Sinan.Log
{
    /// <summary>
    /// 留言
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public sealed class Note : LogBase
    {
        Note() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionid">
        /// 留言的actionid为保留id为6；
        /// 留言回复的actionid为7；
        /// 其它留言类操作请填8</param>
        public Note(int woldid, Actiontype actionid)
            : base(woldid, Optype.Msg, actionid)
        {
        }

        /// <summary>
        /// 填入留言编号id
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public string safebuf
        {
            get;
            set;
        }

        /// <summary>
        /// 备注
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public string remark
        {
            get;
            set;
        }

        /// <summary>
        /// 被操作用户 UID
        /// (游戏中的创建的角色ID)
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public int? touid
        {
            get;
            set;
        }

        /// <summary>
        /// 被操作用户OpenID
        /// (QQ号转换出来的ID)
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
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
            sb.Append("&safebuf=");
            sb.Append(safebuf);
            sb.Append("&remark=");
            sb.Append(remark);
            sb.Append("&touid=");
            sb.Append(touid);
            sb.Append("&toopenid=");
            sb.Append(toopenid);
            sb.Append("&key=&modifyfee=&source=&level=&itemid=&itemtype=&itemcnt=&modifyexp=&totalexp=&modifycoin=&totalcoin=&totalfee=&onlinetime=&keycheckret=&user_num=");
            return sb.ToString();
        }
    }
}
