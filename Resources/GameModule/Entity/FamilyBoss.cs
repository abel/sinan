using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Util;
using Sinan.AMF3;
using MongoDB.Bson.Serialization.Attributes;

namespace Sinan.Entity
{
    /// <summary>
    /// 家族Boss战
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class FamilyBoss //: ExternalizableBase
    {
        /// <summary>
        /// 唯一ID(家族ID+BossID)
        /// </summary>
        [BsonId]
        public string ID
        {
            get;
            set;
        }

        /// <summary>
        /// 家族Boss
        /// </summary>
        public string FamilyID
        {
            get;
            set;
        }

        /// <summary>
        /// BossID
        /// </summary>
        public string BossID
        {
            get;
            set;
        }

        /// <summary>
        /// Boss创建时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Created
        {
            get;
            set;
        }

        /// <summary>
        /// Boss击杀时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime KillTime
        {
            get;
            set;
        }

        /// <summary>
        /// 状态
        /// </summary>
        public int State
        {
            get;
            set;
        }

        /// <summary>
        /// 领奖记录
        /// </summary>
        public Variant AwardLog
        {
            get;
            set;
        }

        //protected override void WriteAmf3(IExternalWriter writer)
        //{
        //    writer.WriteKey("BossID");
        //    writer.WriteUTF(this.BossID);
        //}
    }
}
