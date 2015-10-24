using System;
using MongoDB.Driver;
using Sinan.AMF3;
using Sinan.Util;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.GameModule;
using MongoDB.Bson;


namespace Sinan.Entity
{
    /// <summary>
    /// 商城日志
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class Task : SmartVariantEntity
    {
        /// 主类型
        /// </summary>
        public string MainType { get; set; }
        /// <summary>
        /// 角色ID
        /// </summary>
        public string PlayerID { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Created { get; set; }


        public override bool Save()
        {
            IncrementVer();
            this.Changed = false;
            TaskAccess.Instance.Save(this);
            return true;
        }
    }
}
