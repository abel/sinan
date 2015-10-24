using System;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// 家族日志
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class Family : SmartVariantEntity
    {
        private Family() { }

        /// <summary>
        /// 家族资金
        /// </summary>
        public int Score
        {
            get;
            set;
        }

        /// <summary>
        /// 家族威望
        /// </summary>
        public int WeiWang
        {
            get;
            set;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Created
        {
            get;
            set;
        }

        public override bool Save()
        {
            this.IncrementVer();
            this.Changed = false;
            return FamilyAccess.Instance.Save(this);
        }

        public static Family Create(Variant v)
        {
            Family f = new Family();
            f.m_value = v;
            return f;
        }
    }
}
