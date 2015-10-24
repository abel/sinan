using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.Data;
using Sinan.Util;

namespace Sinan.Log
{
    /// <summary>
    /// 动态日志
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class LogVariant : LogBase
    {
        protected Variant m_value;

        [BsonIgnoreIfDefaultAttribute]
        public Variant Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionid">(其它,100以上)</param>
        public LogVariant(int woldid, Actiontype actionid)
            : base(woldid, Optype.Other, actionid)
        {
            m_value = new Variant();
        }

        /// <summary>
        /// 如果没有变化，则填 0
        /// 上报单位为Q分（ 100Q 分 = 10Q 点 = 1Q 币）
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public int modifyfee
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

        /// <summary>
        /// 操作用户的等级
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public int? level
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

        /// <summary>
        /// 用户操作物品ID
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public string itemid
        {
            get;
            set;
        }

        /// <summary>
        /// 用户操作物品ID 的分类
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public string itemtype
        {
            get;
            set;
        }

        /// <summary>
        /// 用户操作物品数量
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public int? itemcnt
        {
            get;
            set;
        }

        /// <summary>
        /// 经验值变化值
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public int? modifyexp
        {
            get;
            set;
        }

        /// <summary>
        /// 经验值总量
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public long? totalexp
        {
            get;
            set;
        }

        /// <summary>
        /// 虚拟晶币变化值
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public int? modifycoin
        {
            get;
            set;
        }

        /// <summary>
        /// 虚拟晶币总量
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public long? totalcoin
        {
            get;
            set;
        }

        /// <summary>
        /// 游戏币总量
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public long? totalfee
        {
            get;
            set;
        }

        /// <summary>
        /// 附加
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public string remark
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
            sb.Append("&modifyfee=");
            sb.Append(modifyfee);

            sb.Append("&touid=");
            sb.Append(touid);
            sb.Append("&toopenid=");
            sb.Append(toopenid);
            sb.Append("&level=");
            sb.Append(level);
            sb.Append("&source=");
            sb.Append(source);
            sb.Append("&itemid=");
            sb.Append(itemid);
            sb.Append("&itemtype=");
            sb.Append(itemtype);
            sb.Append("&itemcnt=");
            sb.Append(itemcnt);
            sb.Append("&modifyexp=");
            sb.Append(modifyexp);
            sb.Append("&totalexp=");
            sb.Append(totalexp);
            sb.Append("&modifycoin=");
            sb.Append(modifycoin);
            sb.Append("&totalcoin=");
            sb.Append(totalcoin);
            sb.Append("&totalfee=");
            sb.Append(totalfee);
            sb.Append("&remark=");
            sb.Append(remark);

            sb.Append("&onlinetime=&keycheckret=&safebuf=&user_num=");

            AppendReserve(sb);
            return sb.ToString();
        }

        private void AppendReserve(StringBuilder sb)
        {
            if (m_value != null)
            {
                foreach (var item in m_value)
                {
                    sb.Append("&");
                    sb.Append(item.Key);
                    sb.Append("=");
                    sb.Append(item.Value);
                }
            }
        }
    }
}

