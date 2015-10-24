using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.Entity;
using Sinan.GameModule;

namespace Sinan.Entity
{
    /// <summary>
    /// 活动
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class PartBase : SmartVariantEntity
    {

        /// <summary>
        /// 主类型
        /// </summary>
        public string SubType { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        public string PlayerID { get; set; }

        /// <summary>
        /// 活动
        /// </summary>
        public string PartID { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Created { get; set; }

        public override bool Save()
        {            
            return PartAccess.Instance.Save(this);
        }
    }
}
