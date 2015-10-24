using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Sinan.AMF3;
using Sinan.Data;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// 支持自动保存数据的基类.
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public abstract class SmartVariantEntity : VariantExternalizable, IPersistable
    {
        protected SmartVariantEntity()
        {
            this.Modified = DateTime.UtcNow;
        }

        #region 属性
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
        /// 更新时间
        /// </summary>
        public DateTime Modified
        {
            get;
            set;
        }

        int m_Ver;
        /// <summary>
        /// 版本号
        /// </summary>
        public int Ver
        {
            get { return m_Ver; }
            set { m_Ver = value; }
        }

        #endregion

        #region ISmartEntity实现
        int m_currentVer = 1;
        int m_lastWriteVer = 1;

        [MongoDB.Bson.Serialization.Attributes.BsonIgnore]
        public bool Changed
        {
            get { return m_lastWriteVer != m_currentVer; }
            set
            {
                if (value)
                {
                    m_currentVer++;
                }
                else
                {
                    m_lastWriteVer = m_currentVer;
                }
            }
        }
        public abstract bool Save();
        #endregion

        /// <summary>
        /// 版本号增加1
        /// </summary>
        /// <returns></returns>
        public int IncrementVer()
        {
            return System.Threading.Interlocked.Increment(ref m_Ver);
        }

    }
}
