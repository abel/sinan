using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.GameModule;

namespace Sinan.Entity
{
    /// <summary>
    /// 坐骑
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class Mounts : SmartVariantEntity    
    {
        /// <summary>
        /// 当前等级
        /// </summary>
        public int Level { get;set;}

        /// <summary>
        /// 当前经验
        /// </summary>
        public int Experience { get; set; }

        /// <summary>
        /// 状态:0召回,1骑乘
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 坐骑价数
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        public string PlayerID { get; set; }

        /// <summary>
        /// 关联坐骑配置ID
        /// </summary>
        public string MountsID { get; set; }   

        /// <summary>
        /// 升级时间
        /// </summary>
        public DateTime Update { get; set; }

        /// <summary>
        /// 连续失败次数
        /// </summary>
        public int FailCount { get; set; }

        /// <summary>
        /// 祝福值
        /// </summary>
        public int ZhuFu { get; set; }

        /// <summary>
        /// 失败时间
        /// </summary>
        public DateTime FailTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Created { get; set; }

        public override bool Save()
        {
            return MountsAccess.Instance.Save(this);
        }
    }
}
