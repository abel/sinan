using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Sinan.AMF3;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// Player:实体类
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public partial class Player
    {
        protected Player()
        {
        }

        #region Entity

        protected int m_level;
        protected int m_exp;
        protected int m_maxExp;
        protected string m_userID;

        protected PlayerProperty m_life;

        /// <summary>
        /// 所属角色1狂战士,2魔弓手，3神祭师
        /// </summary>
        public string RoleID
        { get; set; }

        /// <summary>
        /// 所有者ID
        /// </summary>
        public string UserID
        {
            get { return m_userID; }
            set { m_userID = value; }
        }

        /// <summary>
        /// 等级
        /// </summary>
        public int Level
        {
            get { return m_level; }
            set { m_level = value; }
        }

        /// <summary>
        /// 最大经验值
        /// </summary>
        [BsonIgnore]
        public int MaxExp
        {
            get { return m_maxExp; }
        }

        /// <summary>
        /// 当前经验
        /// </summary>
        public int Experience
        {
            get { return m_exp; }
            set { m_exp = value; }
        }

        /// <summary>
        /// 当前生命值
        /// </summary>
        public int HP
        {
            get;
            set;
        }

        /// <summary>
        /// 当前魔法值.
        /// </summary>
        public int MP
        {
            get;
            set;
        }

        /// <summary>
        /// 性别
        /// </summary>
        public int Sex
        {
            get;
            set;
        }

        /// <summary>
        /// 玩家状态 
        /// 0:冻结;1: 活动, 2:删除中,3:标记为已删除
        /// </summary>
        public int State
        {
            get;
            set;
        }

        /// <summary>
        /// 玩家在游戏中的动作
        /// (如行走/跑/打座/战斗/站立等....)
        /// 见Sinan.Entity.ActionState
        /// </summary>
        [BsonIgnore]
        public ActionState AState
        {
            get;
            protected set;
        }

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool Online
        {
            get;
            set;
        }

        /// <summary>
        /// 成就点
        /// </summary>
        public int Dian
        {
            get;
            set;
        }

        /// <summary>
        /// 是否是VIP
        /// </summary>
        public int VIP
        {
            get;
            set;
        }

        /// <summary>
        /// 升级时间戳
        /// (用于等级排行)
        /// </summary>
        public int S
        {
            get;
            set;
        }

        /// <summary>
        /// 显示编号
        /// </summary>
        public int ShowInfo
        {
            get;
            set;
        }

        /// <summary>
        /// 会员等级
        /// </summary>
        public int MLevel
        {
            get;
            set;
        }

        /// <summary>
        /// 会员当前成长度
        /// </summary>
        public int CZD
        {
            get;
            set;
        }

        

        #endregion Entity

        /// <summary>
        /// 只写用户的完整的信息..
        /// </summary>
        /// <param name="writer"></param>
        protected override void WriteAmf3(IExternalWriter writer)
        {
            WriteBase(writer);
            WriteScene(writer);
            WriteShape(writer);
            WriteFinance(writer);
            WriteFightProperty(writer);
            WriteOther(writer);
            WritePlayerEx(writer);
        }

        public void WritePlayerEx(IExternalWriter writer)
        {
            foreach (object v in Value.Values)
            {
                PlayerEx ex = v as PlayerEx;
                if (ex != null)
                {
                    writer.WriteKey(ex.Name);
                    writer.WriteValue(v);
                }
            }
        }

        /// <summary>
        /// 基本信息
        /// ID/RoleID/Name/Level/Sex
        /// </summary>
        /// <param name="writer"></param>
        public void WriteBase(IExternalWriter writer)
        {
            writer.WriteKey("ID");
            writer.WriteUTF(ID);
            writer.WriteKey("UID");
            writer.WriteUTF(m_userID);

            writer.WriteKey("Name");
            writer.WriteUTF(Name);
            writer.WriteKey("RoleID");
            writer.WriteUTF(RoleID);
            writer.WriteKey("Sex");
            writer.WriteInt(Sex);
            writer.WriteKey("Level");
            writer.WriteInt(Level);
            writer.WriteKey("VIP");
            writer.WriteInt(VIP);
            writer.WriteKey("FightValue");
            writer.WriteInt(FightValue);

            writer.WriteKey("Owe");
            writer.WriteInt(Owe);

            writer.WriteKey("StarPower");
            writer.WriteInt(StarPower);

            writer.WriteKey("ShowInfo");
            writer.WriteInt(ShowInfo);

            writer.WriteKey("MLevel");
            writer.WriteInt(MLevel);

            writer.WriteKey("CZD");
            writer.WriteInt(CZD); 
        }

        /// <summary>
        /// 在场景中的信息:
        /// SceneID/Walk/X/Y/ActionState
        /// </summary>
        /// <param name="writer"></param>
        public void WriteScene(IExternalWriter writer)
        {
            writer.WriteKey("SceneID");
            writer.WriteUTF(SceneID);
            writer.WriteKey("Walk");
            writer.WriteDouble(Point);
            writer.WriteKey("X");
            writer.WriteInt(X);
            writer.WriteKey("Y");
            writer.WriteInt(Y);
            writer.WriteKey("ActionState");
            writer.WriteInt((int)AState);
        }

        /// <summary>
        /// 战斗属性.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteFightProperty(IExternalWriter writer)
        {
            writer.WriteKey("JingYan");
            MVPair.WritePair(writer, m_maxExp, m_exp);
            if (m_life != null)
            {
                writer.WriteKey("ShengMing");
                MVPair.WritePair(writer, m_life.ShengMing, HP);
                writer.WriteKey("MoFa");
                MVPair.WritePair(writer, m_life.MoFa, MP);
                m_life.WriteProperty(writer);
            }
        }

        /// <summary>
        /// 保存服装数据
        /// </summary>
        /// <returns></returns>
        public bool SaveClothing()
        {
            return PlayerAccess.Instance.SaveClothing(this);
        }

        public static Player Create()
        {
            Player player = new Player();
            player.Level = 1;
            player.State = 1;
            player.m_value = new Variant();
            return player;
        }
    }
}

