using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Sinan.Data
{
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class ConfigEntity
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        [BsonId]
        public string ID
        {
            get;
            set;
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 主类型
        /// </summary>
        public string MainType
        {
            get;
            set;
        }

        /// <summary>
        /// 子类型
        /// </summary>
        public string SubType
        {
            get;
            set;
        }

        /// <summary>
        /// 用户界面(JSON对象的字符串)
        /// </summary>
        public string UI
        {
            get;
            set;
        }

        /// <summary>
        /// 值(JSON对象的字符串)
        /// </summary>
        public string Value
        {
            get;
            set;
        }

        /// <summary>
        /// 版本号
        /// </summary>
        public int Ver
        {
            get;
            set;
        }
    }
}