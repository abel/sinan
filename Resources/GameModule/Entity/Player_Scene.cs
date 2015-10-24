using System;
using System.Drawing;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Sinan.AMF3;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// Player的场景部分
    /// </summary>
    partial class Player
    {
        protected double m_point;

        /// <summary>
        /// 当前坐标X
        /// </summary>
        public int X
        {
            get;
            set;
        }

        /// <summary>
        /// 当前坐标Y
        /// </summary>
        public int Y
        {
            get;
            set;
        }

        /// <summary>
        /// 最后一次行走路线.
        /// </summary>
        [BsonIgnore]
        public double Point
        {
            get { return m_point; }
            set { m_point = value; }
        }

        /// <summary>
        /// 所在场景
        /// </summary>
        public string SceneID
        {
            get;
            set;
        }

        /// <summary>
        /// 所在线路
        /// </summary>
        public int Line
        {
            get;
            set;
        }
    }
}
