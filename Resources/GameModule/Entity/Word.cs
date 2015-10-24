using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Sinan.Entity
{
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class Word
    {
        /// <summary>
        /// 已使用,被删除的,可以重用.
        /// </summary>
        public const int Deleted = 0;

        /// <summary>
        /// 自动命名的数量(99万)
        /// </summary>
        public const int AutoCount = 999999;

        /// <summary>
        /// 表示已使用
        /// </summary>
        public const int Used = 11000000;

        /// <summary>
        /// 系统保留
        /// </summary>
        public const int Keep = 12000000;

        /// <summary>
        /// 敏感字/受屏蔽的
        /// </summary>
        public const int Filter = 13000000;

        [BsonId]
        public string Key
        {
            get;
            set;
        }

        /// <summary>
        /// 状态(小于100000推荐使用)
        /// 1-100000推荐使用名字(可存10万).
        /// 11000000:已使用
        /// 12000000:系统保留
        /// 13000000:敏感字/受屏蔽的
        /// </summary>
        public int State
        {
            get;
            set;
        }

        ///// <summary>
        ///// 修改时间
        ///// </summary>
        //public DateTime Modified
        //{
        //    get;
        //    set;
        //}
    }
}
