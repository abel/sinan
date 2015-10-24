using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.AMF3;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 战斗对象
    /// </summary>
    public abstract class FightObject : ExternalizableBase
    {
        protected int m_hp;
        protected int m_mp;
        protected bool m_online;
        protected FightObject[] m_team;
        readonly protected BufferResult m_bufferResult;
        readonly protected List<SkillBuffer> m_buffers;
        protected Dictionary<string, Tuple<int, GameConfig>> m_fixBuffer;

        protected PlayerBusiness m_player;

        public FightObject[] Team
        {
            get { return m_team; }
            set { m_team = value; }
        }


        /// <summary>
        /// 玩家信息(NPC为空)
        /// </summary>
        public PlayerBusiness Player
        {
            get { return m_player; }
        }

        public BufferResult BufferResult
        {
            get { return m_bufferResult; }
        }

        /// <summary>
        /// 类型.1:APC 2:玩家 3:庞物
        /// </summary>
        abstract public FighterType FType
        {
            get;
        }

        /// <summary>
        /// 战斗位置
        /// </summary>
        public int P
        {
            get;
            set;
        }

        /// <summary>
        /// 等级
        /// </summary>
        public int Level
        {
            get;
            set;
        }

        /// <summary>
        /// ID
        /// </summary>
        public string ID
        {
            get;
            set;
        }

        /// <summary>
        /// 名字
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 皮肤
        /// </summary>
        public string Skin
        {
            get;
            set;
        }

        /// <summary>
        /// 战斗属性
        /// </summary>
        public PlayerProperty Life
        {
            get;
            set;
        }

        public int HP
        {
            get { return m_hp; }
            set { m_hp = value; }
        }

        public int MP
        {
            get { return m_mp; }
            set { m_mp = value; }
        }

        protected int m_sudu;
        protected FightAction m_action;
        /// <summary>
        /// 当前速度
        /// </summary>
        public int Sudu
        {
            get { return m_sudu; }
            set { m_sudu = value; }
        }

        /// <summary>
        /// 说话
        /// </summary>
        public int Talk
        {
            get;
            set;
        }


        /// <summary>
        /// 攻击行为
        /// </summary>
        public FightAction Action
        {
            get { return m_action; }
            set { m_action = value; }
        }

        /// <summary>
        /// 活动的(生命大于0并而在线)
        /// </summary>
        public bool IsLive
        {
            get { return m_hp > 0 && m_online; }
        }

        /// <summary>
        /// 已结束(生命小于0或都不在线)
        /// </summary>
        public bool Over
        {
            get { return m_hp <= 0 || (!m_online); }
        }

        /// <summary>
        /// 是否受别人保护
        /// </summary>
        public bool Protect
        {
            get;
            set;
        }

        /// <summary>
        /// 是否保护别人
        /// </summary>
        public bool ProtectOther
        {
            get;
            set;
        }

        /// <summary>
        /// 可叠加的Buffer
        /// </summary>
        public List<SkillBuffer> Buffers
        {
            get { return m_buffers; }
        }

        /// <summary>
        /// 强力
        /// </summary>
        abstract public int WG
        {
            get;
        }

        /// <summary>
        /// 魔能
        /// </summary>
        abstract public int MG
        {
            get;
        }

        /// <summary>
        /// 坚韧(增强物理防御)
        /// </summary>
        abstract public int WF
        {
            get;
        }

        /// <summary>
        /// 坚韧(增强魔法防御)
        /// </summary>
        abstract public int MF
        {
            get;
        }

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool Online
        {
            get { return m_online; }
        }

        public Dictionary<string, Tuple<int, GameConfig>> FixBuffer
        {
            get { return m_fixBuffer; }
        }

        Dictionary<string, int> m_skillUsed;
        /// <summary>
        /// 记录有冻结的技能
        /// </summary>
        public Dictionary<string, int> SkillUsed
        {
            get
            {
                if (m_skillUsed == null)
                {
                    m_skillUsed = new Dictionary<string, int>(2);
                }
                return m_skillUsed;
            }
        }

        public bool CanActive
        {
            get
            {
                for (int i = 0; i < m_buffers.Count; i++)
                {
                    var buffer = m_buffers[i];
                    if (buffer.RemainingNumber > 0)
                    {
                        // 混乱/石化/冰冻
                        if (buffer.ID == BufferType.HunLuan || buffer.ID == BufferType.ShiHua || buffer.ID == BufferType.BingDong)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }


        protected FightObject()
        {
            m_online = true;
            m_buffers = new List<SkillBuffer>();
            m_bufferResult = new BufferResult(this);
        }

        /// <summary>
        /// 检查是否存在指字的Buffer
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ExistsBuffer(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return m_buffers.Exists(x => x.ID == name);
        }

        /// <summary>
        /// 检查技能是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract bool ExistsSkill(string name);

        /// <summary>
        /// 查找指定的Buffer
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SkillBuffer FindBuffer(string name)
        {
            return m_buffers.Find(x => x.ID == name);
        }

        /// <summary>
        /// 查找指定的Buffer所在位置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>-
        public int FindIndexBuffer(string name)
        {
            return m_buffers.FindIndex(x => x.ID == name);
        }

        /// <summary>
        /// 查找特定用户发出的Buffer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="senderID"></param>
        /// <returns>Buffer所在位置</returns>
        public int FindIndexBuffer(string name, string senderID)
        {
            return m_buffers.FindIndex(x => x.ID == name && x.SenderID == senderID);
        }

        /// <summary>
        /// 添加不可叠加的Buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool AddBuffer(SkillBuffer buffer)
        {
            int index = FindIndexBuffer(buffer.ID);
            if (index < 0)
            {
                m_buffers.Add(buffer);
                return true;
            }
            else
            {
                if (buffer > m_buffers[index])
                {
                    m_buffers[index] = buffer;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 添加Buffer(不同的发送者可叠加)
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public bool AddBuffer(SkillBuffer buffer, string sender)
        {
            int index = FindIndexBuffer(buffer.ID, sender);
            if (index < 0)
            {
                m_buffers.Add(buffer);
                return true;
            }
            else
            {
                if (buffer > m_buffers[index])
                {
                    m_buffers[index] = buffer;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int TryHP(double dhp)
        {
            //0到1的小数,百分比方式.
            return dhp <= 1 ? (int)(dhp * this.Life.ShengMing) : (int)(dhp);
        }

        public int TryMP(double dmp)
        {
            //0到1的小数,百分比方式.
            return dmp <= 1 ? (int)(dmp * this.Life.MoFa) : (int)(dmp);
        }

        public abstract bool CheckJingNeng(int current, out GameConfig gs);


        /// <summary>
        /// 退出战斗
        /// </summary>
        /// <param name="changeLife">是否改变生命和魔法(切磋时不改变)</param>
        public virtual void ExitFight(bool changeLife)
        {
            m_online = false;
        }

        public virtual void CreateAction(FightObject[] targetTeam, int fightCount)
        {
            if (m_action == null || m_action.FightCount != fightCount)
            {
                FightAction action = new FightAction(ActionType.WuLi, ID, fightCount);
                action.Target = targetTeam[Sinan.Extensions.NumberRandom.Next(targetTeam.Length)].ID;
                this.m_action = action;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hp"></param>
        /// <param name="mp"></param>
        /// <param name="minHP">保留的生命值</param>
        public virtual void AddHPAndMP(int hp, int mp, int minHP = 0)
        {
            if (hp != 0)
            {
                this.HP += hp;
                if (this.HP > this.Life.ShengMing)
                {
                    this.HP = this.Life.ShengMing;
                }
                else if (this.HP < 0)
                {
                    this.HP = minHP;
                }
            }
            if (mp != 0)
            {
                this.MP += mp;
                if (this.MP > this.Life.MoFa)
                {
                    this.MP = this.Life.MoFa;
                }
                else if (this.MP < 0)
                {
                    this.MP = minHP;
                }
            }
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey("FType");
            writer.WriteInt((int)FType);

            writer.WriteKey("P");
            writer.WriteInt(P);

            writer.WriteKey("ID");
            writer.WriteUTF(ID);

            writer.WriteKey("Name");
            writer.WriteUTF(Name);

            writer.WriteKey("ShengMing");
            MVPair.WritePair(writer, Life.ShengMing, m_hp);
            writer.WriteKey("MoFa");
            MVPair.WritePair(writer, Life.MoFa, m_mp);

            writer.WriteKey("Skin");
            writer.WriteUTF(Skin);

            writer.WriteKey("Level");
            writer.WriteInt(this.Level);

            if (Talk > 0)
            {
                writer.WriteKey("Say");
                writer.WriteInt(Talk);
            }
        }
    }

}
