using System;
using System.Globalization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Sinan.AMF3;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// 形象部分
    /// </summary>
    partial class Player
    {
        protected string m_nicename;
        protected string m_familyName;
        protected string m_familyJob;
        protected string m_marriage;
        protected string m_mount;
        protected string m_body;
        protected string m_weapon;
        protected string m_coat;
        protected bool m_iscoat;

        /// <summary>
        /// 称号
        /// </summary>
        public string Nicename
        {
            get { return m_nicename; }
            protected set { m_nicename = value; }
        }

        /// <summary>
        /// 家族
        /// </summary>
        public string FamilyName
        {
            get { return m_familyName; }
            protected set { m_familyName = value; }
        }

        /// <summary>
        /// 家族职位
        /// </summary>
        public string FamilyJob
        {
            get { return m_familyJob; }
            protected set { m_familyJob = value; }
        }

        /// <summary>
        /// 婚姻
        /// </summary>
        public string Marriage
        {
            get { return m_marriage; }
            set { m_marriage = value; }
        }

        /// <summary>
        /// 坐骑
        /// </summary>
        public string Mount
        {
            get { return m_mount; }
            set { m_mount = value; }
        }

        /// <summary>
        /// 身体
        /// </summary>
        public string Body
        {
            get { return m_body; }
            set { m_body = value; }
        }

        /// <summary>
        /// 武器
        /// </summary>
        public string Weapon
        {
            get { return m_weapon; }
            set { m_weapon = value; }
        }

        /// <summary>
        /// 外装
        /// </summary>
        public string Coat
        {
            get { return m_coat; }
            set { m_coat = value; }
        }

        /// <summary>
        /// 是否使用外装
        /// </summary>
        public bool IsCoat
        {
            get { return m_iscoat; }
            set { m_iscoat = value; }
        }

        /// <summary>
        /// 外形信息:
        /// Mount/Body/Weapon/Coat/IsCoat
        /// </summary>
        /// <param name="writer"></param>
        public void WriteShape(IExternalWriter writer)
        {
            writer.WriteKey("Nicename");
            writer.WriteUTF(Nicename);
            writer.WriteKey("Mount");
            writer.WriteUTF(Mount);
            writer.WriteKey("Body");
            writer.WriteUTF(Body);
            writer.WriteKey("Weapon");
            writer.WriteUTF(Weapon);
            writer.WriteKey("Coat");
            writer.WriteUTF(Coat);
            writer.WriteKey("IsCoat");
            writer.WriteBoolean(IsCoat);
        }

        /// <summary>
        /// FamilyName/FamilyJob/Marriage
        /// </summary>
        /// <param name="writer"></param>
        public void WriteOther(IExternalWriter writer)
        {
            writer.WriteKey("FamilyName");
            writer.WriteUTF(FamilyName);
            writer.WriteKey("FamilyJob");
            writer.WriteUTF(FamilyJob);
            writer.WriteKey("Marriage");
            writer.WriteUTF(Marriage);
        }

    }
}
