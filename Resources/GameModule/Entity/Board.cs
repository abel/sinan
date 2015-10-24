using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.AMF3;
using Sinan.Data;
using Sinan.Extensions;
using Sinan.GameModule;

namespace Sinan.Entity
{
    /// <summary>
    /// 玩家公告板
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    sealed public class Board : ExternalizableBase
    {
        int m_id;
        string m_playerID;
        private Board() { }

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
                m_playerID = value.ToHexString();
            }
        }

        [BsonIgnore]
        public string PlayerID
        {
            get { return m_playerID; }
        }

        public List<string> Values
        {
            get;
            set;
        }
        #endregion

        #region ISmartEntity实现
        //int m_currentVer = 1;
        //int m_lastWriteVer = 1;

        //[BsonIgnore]
        //public bool Changed
        //{
        //    get { return m_lastWriteVer != m_currentVer; }
        //    set
        //    {
        //        if (value)
        //        {
        //            m_currentVer++;
        //        }
        //        else
        //        {
        //            m_lastWriteVer = m_currentVer;
        //        }
        //    }
        //}

        #endregion

        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey("Values");
            writer.WriteValue(Values);
        }
    }

}
