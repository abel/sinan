using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Sinan.FrontServer
{
    partial class PlayerBusiness
    {
        TeamInstanceBusiness m_teamInstance;

        /// <summary>
        /// 玩家所在的副本
        /// </summary>
        [BsonIgnore]
        public TeamInstanceBusiness TeamInstance
        {
            get { return m_teamInstance; }
            set
            {
                m_teamInstance = value;
                //Console.WriteLine(this.Name + (value == null ? 0 : value.ID));
            }
        }

    }
}
