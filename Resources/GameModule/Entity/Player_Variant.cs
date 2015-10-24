using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.AMF3;
using Sinan.Data;
using Sinan.Extensions;
using Sinan.Util;
using MongoDB.Bson;

namespace Sinan.Entity
{
    /// <summary>
    /// 支持自动保存数据的基类.
    /// </summary>
    public partial class Player : VariantExternalizable
    {
        #region 属性
        protected int _id;
        protected string m_id;

        /// <summary>
        /// 唯一ID
        /// </summary>
        [BsonId]
        public int PID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                m_id = _id.ToHexString();
            }
        }


        /// <summary>
        /// 唯一ID
        /// </summary>
        [BsonIgnore]
        public string ID
        {
            get { return m_id; }
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
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Modified
        {
            get;
            set;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Created
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

        /// <summary>
        /// 服务器编码
        /// </summary>
        public int SID
        {
            get;
            set;
        }
        #endregion

        /// <summary>
        /// 设置降生信息
        /// </summary>
        public void SetBirthInfo(Variant d)
        {
            this.Coin = d.GetInt64OrDefault("Coin");
            this.Score = d.GetInt64OrDefault("Score");
            this.Bond = d.GetInt64OrDefault("Bond");
            this.SceneID = d.GetStringOrDefault("SceneID");
            this.X = d.GetIntOrDefault("X");
            this.Y = d.GetIntOrDefault("Y");
            this.Body = d.GetStringOrDefault("Body");
            this.Weapon = d.GetStringOrDefault("Weapon");
            this.Mount = d.GetStringOrDefault("Mount");
            this.Coat = d.GetStringOrDefault("Coat");
            this.IsCoat = d.GetBooleanOrDefault("IsCoat");
        }

    }
}
