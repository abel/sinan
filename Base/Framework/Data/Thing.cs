using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.AMF3;

namespace Sinan.Data
{
    /// <summary>
    /// NotifyPropertyChanged
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public abstract class Thing : ExternalizableBase
    {
        /// <summary>
        /// 某一属性值更改后调用的方法
        /// </summary>
        [BsonIgnore]
        public Action<Thing, string, object> PropertyChanged;

        /// <summary>
        /// 某一属性值已更改的通知
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="newValue"></param>
        protected void NotifyPropertyChanged<T>(string propertyName, T newValue)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, propertyName, newValue);
            }
        }

        ///// <summary>
        ///// 多个属性更改后调用的方法(用于合并)
        ///// </summary>
        //public Action<EntityBase, IDictionary<string, object>> MulitPropertyChanged;
        ///// <summary>
        ///// 多个属性值已更改的通知
        ///// </summary>
        ///// <param name="mulitProperty"></param>
        //protected void NotifyMulitPropertyChanged(IDictionary<string, object> mulitProperty)
        //{
        //    if (MulitPropertyChanged != null)
        //    {
        //        MulitPropertyChanged(this, mulitProperty);
        //    }
        //}

        /// <summary>
        /// 子对象更新
        /// </summary>
        /// <param name="sub"></param>
        /// <param name="propertyName"></param>
        /// <param name="newValue"></param>
        protected virtual void NotifyPropertyChanged(object sub, string propertyName, object newValue)
        {
            if (PropertyChanged != null)
            {
                //TODO: 使用反射获取名字...
                PropertyChanged(this, propertyName, newValue);
            }
        }

        protected BsonObjectId m_id;
        protected string m_name;
        protected DateTime m_modified;
        protected int m_Ver;

        #region 属性
        [BsonId]
        public MongoDB.Bson.BsonObjectId _id
        {
            get { return m_id; }
            private set { m_id = value; }
        }

        /// <summary>
        /// 唯一ID
        /// </summary>
        [BsonIgnore]
        public string ID
        {
            get { return _id.ToString(); }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set
            {
                if (m_name != value)
                {
                    m_name = value;
                    NotifyPropertyChanged("Name", value);
                }
            }
        }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime Modified
        {
            get { return m_modified; }
            set { m_modified = value; }
        }

        /// <summary>
        /// 版本号
        /// </summary>
        public int Ver
        {
            get { return m_Ver; }
            set { m_Ver = value; }
        }
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

