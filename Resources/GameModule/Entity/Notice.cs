using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Entity;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.GameModule;

namespace Sinan.Entity
{
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class Notice : SmartVariantEntity
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int Sort
        {
            get;
            set;
        }
        /// <summary>
        /// 开始时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime StartTime
        {
            get;
            set;
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime EndTime
        {
            get;
            set;
        }

        /// <summary>
        /// 公告内容
        /// </summary>
        public string Content
        {
            get;
            set;
        }

        /// <summary>
        /// 频率
        /// </summary>
        public int Rate
        {
            get;
            set;
        }

        /// <summary>
        /// 次数
        /// </summary>
        public int Count
        {
            get;
            set;
        }

        /// <summary>
        /// 当前公告状态0正常,1停止
        /// </summary>
        public int Status
        {
            get;
            set;
        }

        /// <summary>
        /// 发送次数
        /// </summary>
        public int Cur
        {
            get;
            set;
        }

        /// <summary>
        /// 发送位置
        /// </summary>
        public string Place
        {
            get;
            set;
        }

        public override bool Save()
        {
            return NoticeAccess.Instance.Save(this);
        }
    }
}
