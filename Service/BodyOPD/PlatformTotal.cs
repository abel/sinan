using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Sinan.AMF3;
using Sinan.Util;

namespace Sinan.BabyOPD
{
    /// <summary>
    /// 信息相关信息统计
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class PlatformTotal
    {
        public PlatformTotal()
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

        /// <summary>
        /// 新增账号数
        /// </summary>
        public int NewUser
        { get; set; }

        /// <summary>
        /// 新增角色数
        /// </summary>
        public int NewPlayer
        { get; set; }

        /// <summary>
        /// 新增角色2次登录数
        /// </summary>
        public int NewPlayerSecondLogin
        { get; set; }

        /// <summary>
        /// 角色总数
        /// </summary>
        public int PlayerTotal
        { get; set; }

        /// <summary>
        /// 账号总数
        /// </summary>
        public int AccountTotal
        { get; set; }

        /// <summary>
        /// 有效角色数(角色等级达到20级)
        /// </summary>
        public int ValidPlayer
        { get; set; }

        /// <summary>
        /// 活跃角色数(当日在线时长超过2小时的角色总数)
        /// </summary>
        public int ActivePlayer
        { get; set; }

        /// <summary>
        /// 平均在线数(每5分钟为一个检测点)
        /// </summary>
        public int AverageOnline
        { get; set; }

        /// <summary>
        /// 最高在线数
        /// </summary>
        public int MaxOnline
        { get; set; }

        /// <summary>
        /// 宠物总数
        /// </summary>
        public int PetTotal { get; set; }

        /// <summary>
        /// 宠物平均等级
        /// </summary>
        public int PetAveLevel { get; set; }
        /// <summary>
        /// 各等级角色数量统计
        /// </summary>
        public List<int> LevelTotal { get; set; }

        /// <summary>
        /// 各阶级宠物数量统计
        /// </summary>
        public List<int> PetLevelTotal { get; set; }

    }
}
