using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Sinan.AMF3;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// Player:ʵ����
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
        /// ������ɫ1��սʿ,2ħ���֣�3���ʦ
        /// </summary>
        public string RoleID
        { get; set; }

        /// <summary>
        /// ������ID
        /// </summary>
        public string UserID
        {
            get { return m_userID; }
            set { m_userID = value; }
        }

        /// <summary>
        /// �ȼ�
        /// </summary>
        public int Level
        {
            get { return m_level; }
            set { m_level = value; }
        }

        /// <summary>
        /// �����ֵ
        /// </summary>
        [BsonIgnore]
        public int MaxExp
        {
            get { return m_maxExp; }
        }

        /// <summary>
        /// ��ǰ����
        /// </summary>
        public int Experience
        {
            get { return m_exp; }
            set { m_exp = value; }
        }

        /// <summary>
        /// ��ǰ����ֵ
        /// </summary>
        public int HP
        {
            get;
            set;
        }

        /// <summary>
        /// ��ǰħ��ֵ.
        /// </summary>
        public int MP
        {
            get;
            set;
        }

        /// <summary>
        /// �Ա�
        /// </summary>
        public int Sex
        {
            get;
            set;
        }

        /// <summary>
        /// ���״̬ 
        /// 0:����;1: �, 2:ɾ����,3:���Ϊ��ɾ��
        /// </summary>
        public int State
        {
            get;
            set;
        }

        /// <summary>
        /// �������Ϸ�еĶ���
        /// (������/��/����/ս��/վ����....)
        /// ��Sinan.Entity.ActionState
        /// </summary>
        [BsonIgnore]
        public ActionState AState
        {
            get;
            protected set;
        }

        /// <summary>
        /// �Ƿ�����
        /// </summary>
        public bool Online
        {
            get;
            set;
        }

        /// <summary>
        /// �ɾ͵�
        /// </summary>
        public int Dian
        {
            get;
            set;
        }

        /// <summary>
        /// �Ƿ���VIP
        /// </summary>
        public int VIP
        {
            get;
            set;
        }

        /// <summary>
        /// ����ʱ���
        /// (���ڵȼ�����)
        /// </summary>
        public int S
        {
            get;
            set;
        }

        /// <summary>
        /// ��ʾ���
        /// </summary>
        public int ShowInfo
        {
            get;
            set;
        }

        /// <summary>
        /// ��Ա�ȼ�
        /// </summary>
        public int MLevel
        {
            get;
            set;
        }

        /// <summary>
        /// ��Ա��ǰ�ɳ���
        /// </summary>
        public int CZD
        {
            get;
            set;
        }

        

        #endregion Entity

        /// <summary>
        /// ֻд�û�����������Ϣ..
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
        /// ������Ϣ
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
        /// �ڳ����е���Ϣ:
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
        /// ս������.
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
        /// �����װ����
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

