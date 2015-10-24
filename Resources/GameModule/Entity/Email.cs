using System;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Sinan.AMF3;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Util;


namespace Sinan.Entity
{
    /// <summary>
    /// 商城日志
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class Email : SmartVariantEntity
    {
        /// <summary>
        /// 主类型
        /// </summary>
        public string MainType { get; set; }

        /// <summary>
        /// 0没有读取,1已经读取,2领取,3删除
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Created { get; set; }

        public override bool Save()
        {
            IncrementVer();
            this.Changed = false;
            return EmailAccess.Instance.Save(this);
        }

    }
}
