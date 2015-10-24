using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.AMF3;
using Sinan.Data;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// 玩家扩展信息
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    sealed public class PlayerEx : VariantExternalizable, IPersistable
    {
        int m_id;
        string m_pid;
        private PlayerEx() { }

        public PlayerEx(int id, string name)
        {
            ID = id;
            Name = name;
        }

        public PlayerEx(string playerID, string name)
        {
            m_id = int.Parse(playerID, NumberStyles.HexNumber, null);
            m_pid = playerID;
            Name = name;
        }
        #region 属性

        /// <summary>
        /// 唯一ID
        /// </summary>
        [BsonId]
        public int ID
        {
            get { return m_id; }
            set
            {
                m_id = value;
                m_pid = m_id.ToHexString();
            }

        }

        public string PlayerID
        {
            get { return m_pid; }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get;
            set;
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

        public bool Save()
        {
            m_lastWriteVer = m_currentVer; //this.Changed = false;
            return PlayerExAccess.Instance.Save(this);
        }
        #endregion


    }
}

