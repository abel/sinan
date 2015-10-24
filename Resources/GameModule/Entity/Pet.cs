using System;
using System.Collections.Generic;
using System.Drawing;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Sinan.AMF3;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.Entity
{
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class Pet : SmartVariantEntity
    {
        #region 基本属性

        PlayerProperty m_life;
        Dictionary<string, Tuple<int, GameConfig>> m_fixBuffer;

        /// <summary>
        /// 非加成的被动技能
        /// 森严/打磨/劈石/趁热/附骨/凝固
        /// 地脉/崩裂/感染/凝霜/玄机/淡定/蔓延
        /// </summary>
        [BsonIgnore]
        public Dictionary<string, Tuple<int, GameConfig>> FixBuffer
        {
            get { return m_fixBuffer; }
        }

        /// <summary>
        /// 所有者ID
        /// </summary>
        public string PlayerID
        {
            get;
            set;
        }

        /// <summary>
        /// 当前生命值
        /// </summary>
        [BsonIgnore]
        public int HP
        { get; set; }

        /// <summary>
        /// 当前魔法值.
        /// </summary>
        [BsonIgnore]
        public int MP
        { get; set; }


        [BsonIgnore]
        public PlayerProperty Life
        {
            get { return m_life; }
        }

        public DateTime Created { get; set; }

        public override bool Save()
        {
            this.Changed = false;
            return PetAccess.Instance.Save(this);
        }

        public void Init()
        {
            //if (m_life == null)
            Variant v = this.Value;
            if (v != null)
            {
                m_life = PetProperty.CreatePlayerProperty(v);
                this.HP = v.GetVariantOrDefault("ShengMing").GetIntOrDefault("V");
                this.MP = v.GetVariantOrDefault("MoFa").GetIntOrDefault("V");
                m_fixBuffer = SkillHelper.InitFixBuffer(this.Value.GetVariantOrDefault("Skill"));
            }
        }

        public void SetHPAndHP(int hp, int mp)
        {
            if (this.HP != hp)
            {
                this.HP = hp;
                Value.GetVariantOrDefault("ShengMing")["V"] = hp;
            }
            if (this.MP != mp)
            {
                this.MP = mp;
                Value.GetVariantOrDefault("MoFa")["V"] = mp;
            }
        }

        /// <summary>
        /// 更新宠物生命值与魔法值
        /// </summary>
        /// <returns></returns>
        public bool SaveLife()
        {
            return PetAccess.Instance.SaveLife(this);
        }
        /// <summary>
        /// 更新宠物疲劳值
        /// </summary>
        /// <returns></returns>
        public bool SaveFatigue() 
        {
            return PetAccess.Instance.SaveFatigue(ID, Value.GetIntOrDefault("Fatigue"));
        }
        protected override void WriteAmf3(IExternalWriter writer)
        {
            base.WriteAmf3(writer);
            writer.WriteKey("ID");
            writer.WriteUTF(ID);
            writer.WriteKey("Name");
            writer.WriteUTF(Name);
            writer.WriteKey("PlayerID");
            writer.WriteUTF(PlayerID);
        }
        #endregion

        #region 竞技场使用
        /// <summary>
        /// 宠物分组
        /// </summary>
        [BsonIgnore]
        public string GroupName
        {
            get;
            set;
        }

        /// <summary>
        /// 当前宠物坐标
        /// </summary>
        [BsonIgnore]
        public Point CurPoint
        {
            get;
            set;
        }

        /// <summary>
        /// 降生点X
        /// </summary>
        [BsonIgnore]
        public int X 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// 降生点Y
        /// </summary>
        [BsonIgnore]
        public int Y 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// 开始坐标
        /// </summary>
        [BsonIgnore]
        public Point BeginPoint
        {
            get;
            set;
        }

        /// <summary>
        /// 结束坐标
        /// </summary>
        [BsonIgnore]
        public Point EndPoint
        {
            get;
            set;
        }

        /// <summary>
        /// 移动时间
        /// </summary>
        [BsonIgnore]
        public DateTime StartTime
        {
            get;
            set;
        }

        /// <summary>
        /// 当前选择的技能
        /// </summary>
        [BsonIgnore]
        public string CurSkill
        {
            get;
            set;
        }

        /// <summary>
        /// 当前技能等级
        /// </summary>
        [BsonIgnore]
        public int CurLevel
        {
            get;
            set;
        }

        /// <summary>
        /// 宠物是否已经参战
        /// </summary>
        [BsonIgnore]
        public bool IsWar
        {
            get;
            set;
        }

        private int m_coolingtime;
        /// <summary>
        /// 冷却时间
        /// </summary>
        [BsonIgnore]
        public int CoolingTime
        {
            get { return m_coolingtime; }
            set { m_coolingtime = value; }
        }

        private int m_speed = 192;
        /// <summary>
        /// 宠物移动速度
        /// </summary>
        [BsonIgnore]
        public int Speed
        {
            get { return m_speed; }
            set { m_speed = value; }
        }

        private int m_range = 100;
        /// <summary>
        /// 宠物攻击范围
        /// </summary>
        [BsonIgnore]
        public int Range
        {
            get { return m_range; }
            set { m_range = value; }
        }

        private string m_rangepet;
        /// <summary>
        /// 攻击目标
        /// </summary>
        [BsonIgnore]
        public string RangePet
        {
            get { return m_rangepet; }
            set { m_rangepet = value; }
        }

        /// <summary>
        /// 宠物当前状态0表示站立,1表示行走
        /// </summary>
        [BsonIgnore]
        public int PetStatus
        {
            get;
            set;
        }

        private List<Variant> m_fightdeath = new List<Variant>();
        /// <summary>
        /// 战死宠物
        /// </summary>
        [BsonIgnore]
        public List<Variant> FightDeath
        {
            get{return m_fightdeath;}
            set{m_fightdeath=value;}
        }
        #endregion
    }
}
