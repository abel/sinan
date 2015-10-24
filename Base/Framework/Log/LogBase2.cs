using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Sinan.Log
{
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public abstract class LogBaseEx : LogBase
    {
        protected LogBaseEx() { }
        protected LogBaseEx(int woldid, Optype optype, Actiontype actionid) :
            base(woldid, optype, actionid)
        {
        }

        [BsonIgnoreIfDefaultAttribute]
        public int? reserve_1
        {
            get;
            set;
        }

        [BsonIgnoreIfDefaultAttribute]
        public int? reserve_2
        {
            get;
            set;
        }

        [BsonIgnoreIfDefaultAttribute]
        public int? reserve_3
        {
            get;
            set;
        }

        [BsonIgnoreIfDefaultAttribute]
        public int? reserve_4
        {
            get;
            set;
        }

        [BsonIgnoreIfDefaultAttribute]
        public string reserve_5
        {
            get;
            set;
        }

        [BsonIgnoreIfDefaultAttribute]
        public string reserve_6
        {
            get;
            set;
        }

        protected void AppendReserve(StringBuilder sb)
        {
            if (reserve_1.HasValue)
            {
                sb.Append("&reserve_1=");
                sb.Append(reserve_1);
            }
            if (reserve_2.HasValue)
            {
                sb.Append("&reserve_2=");
                sb.Append(reserve_2);
            }
            if (reserve_3.HasValue)
            {
                sb.Append("&reserve_3=");
                sb.Append(reserve_3);
            }
            if (reserve_4.HasValue)
            {
                sb.Append("&reserve_4=");
                sb.Append(reserve_4);
            }
            if (reserve_5 != null)
            {
                sb.Append("&reserve_5=");
                sb.Append(reserve_5);
            }
            if (reserve_6 != null)
            {
                sb.Append("&reserve_6=");
                sb.Append(reserve_6);
            }
        }
    }
}
