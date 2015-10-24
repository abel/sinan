using System;
using System.Collections.Generic;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.AMF3;
using Sinan.Command;
using Sinan.Core;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 玩家业务
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    sealed public partial class PlayerBusiness : Player
    {
        private UserSession m_session;
        private TeamJob m_teamJob;
        private PlayerTeam m_team;
        private SceneBusiness m_scene;
        private Pet m_pet;
        private Mounts m_mounts;
        private List<Variant> m_slippets;
        readonly private HashSet<string> m_taskApc;
        readonly private HashSet<string> m_taskgoods;

        /// <summary>
        /// 在队伍中的职位
        /// </summary>
        [BsonIgnore]
        public TeamJob TeamJob
        {
            get { return m_teamJob; }
        }

        /// <summary>
        /// 队伍
        /// </summary>
        [BsonIgnore]
        public PlayerTeam Team
        {
            get { return m_team; }
        }

        /// <summary>
        /// 玩家所在的场景
        /// </summary>
        [BsonIgnore]
        public SceneBusiness Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        /// 携带的宠物
        /// </summary>
        [BsonIgnore]
        public Pet Pet
        {
            get { return m_pet; }
        }

        /// <summary>
        /// 坐骑
        /// </summary>
        [BsonIgnore]
        public Mounts Mounts
        {
            get { return m_mounts; }
        }

        /// <summary>
        /// 溜宠信息
        /// </summary>
        [BsonIgnore]
        public List<Variant> SlipPets
        {
            get { return m_slippets; }
        }

        /// <summary>
        /// 关联有任务的APC.
        /// </summary>
        [BsonIgnore]
        public HashSet<string> TaskAPC
        {
            get { return m_taskApc; }
        }

        /// <summary>
        /// 关联任务的Good
        /// </summary>
        [BsonIgnore]
        public HashSet<string> TaskGoods
        {
            get { return m_taskgoods; }
        }

        /// <summary>
        /// 战斗属性
        /// </summary>
        [BsonIgnore]
        public PlayerProperty Life
        {
            get { return m_life; }
        }

        /// <summary>
        /// 战斗场景
        /// </summary>
        [BsonIgnore]
        public FightBase Fight
        {
            get;
            set;
        }

        /// <summary>
        /// 最后战斗时间
        /// </summary>
        [BsonIgnore]
        public DateTime FightTime
        {
            get;
            set;
        }


        /// <summary>
        /// PK保护到期时间
        /// </summary>
        [BsonIgnore]
        public DateTime LastPK
        {
            get;
            set;
        }

        ///// <summary>
        ///// 最后活动时间
        ///// </summary>
        //[BsonIgnore]
        //public DateTime LastTime
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// 冥想时间
        /// </summary>
        [BsonIgnore]
        public DateTime MeditationTime
        {
            get;
            set;
        }

        /// <summary>
        /// 连接的Session
        /// </summary>
        [BsonIgnore]
        public UserSession Session
        {
            get { return m_session; }
        }

        /// <summary>
        /// 用户登录时间
        /// </summary>
        [BsonIgnore]
        public DateTime LoginTime
        {
            get;
            set;
        }

        /// <summary>
        /// 非战斗时间
        /// </summary>
        [BsonIgnore]
        public int NoFight
        {
            get;
            set;
        }

        /// <summary>
        /// 连续PK胜利次数
        /// </summary>
        [BsonIgnore]
        public int MoreFight
        {
            get;
            set;
        }

        /// <summary>
        /// 显示ID
        /// </summary>
        [BsonIgnore]
        public long ShowID
        {
            get;
            set;
        }

        /// <summary>
        /// 用户是否为黄钻:0表示非黄钻  
        /// 个位为0非年费，1表示年费
        /// 十位表示黄钻等级
        /// </summary>
        [BsonIgnore]
        public int Yellow
        {
            get;
            set;
        }

        /// <summary>
        /// 时装
        /// </summary>
        //[BsonIgnore]
        //public string ShiZhuangSkin
        //{
        //    get;
        //    set;
        //}

        public PlayerBusiness()
        {
            m_taskApc = new HashSet<string>();
            m_taskgoods = new HashSet<string>();
        }

        /// <summary>
        /// 改变队伍状态
        /// </summary>
        /// <param name="team"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool SetTeam(PlayerTeam team, TeamJob state)
        {
            if (m_team == null || team == null)
            {
                m_team = team;
            }
            else if (m_team != team)
            {
                return false;
            }
            m_teamJob = state;
            return true;
        }

        /// <summary>
        /// 设置活动状态
        /// </summary>
        public bool SetActionState(ActionState state)
        {
            if (AState != state)
            {
                AState = state;
                SceneBusiness scene = Scene;
                if (scene != null)
                {
                    Variant v = new Variant(2);
                    v["ActionState"] = (int)AState;
                    v["ID"] = this.ID;
                    this.CallAll(ClientCommand.UpdateActorR, v);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置称号
        /// </summary>
        /// <param name="nikename"></param>
        /// <returns></returns>
        public bool SetNicename(string nikename)
        {
            if (this.Nicename != nikename)
            {
                this.Nicename = nikename;
                SceneBusiness scene = Scene;
                if (scene != null)
                {
                    Variant v = new Variant(2);
                    v["Nicename"] = nikename;
                    v["ID"] = this.ID;
                    this.CallAll(ClientCommand.UpdateActorR, v);
                }
                return PlayerAccess.Instance.SaveValue(_id, "Nicename", nikename);
            }
            return true;
        }

        /// <summary>
        /// 设置家族名称
        /// </summary>
        /// <param name="familyName">家族名称</param>
        /// <param name="familyJob">家族职位</param>
        /// <returns></returns>
        public bool SetFamilyName(string familyName, string familyJob)
        {
            if (this.FamilyName != familyName || this.FamilyJob != familyJob)
            {
                this.FamilyName = familyName;
                this.FamilyJob = familyJob;

                SceneBusiness scene = Scene;
                if (scene != null)
                {
                    Variant v = new Variant(3);
                    v["FamilyName"] = familyName;
                    v["FamilyJob"] = familyJob;
                    v["ID"] = this.ID;
                    this.CallAll(ClientCommand.UpdateActorR, v);
                }
                return PlayerAccess.Instance.SaveValue(_id,
                    new Tuple<string, BsonValue>("FamilyName", FamilyName),
                    new Tuple<string, BsonValue>("FamilyJob", FamilyJob));
            }
            return true;
        }

        /// <summary>
        /// 保存用户的当前数据...
        /// </summary>
        /// <param name="sceneID"></param>
        public bool Save()
        {
            PlayerAccess.Instance.Save(this);
            SaveEx();
            return true;
        }

        /// <summary>
        /// 重新连接
        /// </summary>
        /// <param name="session">新的连接</param>
        /// <returns>原来连接</returns>
        public UserSession Reconnection(UserSession session)
        {
            lock (m_locker)
            {
                Online = (session == null ? false : true);
                UserSession old = m_session;
                m_session = session;
                return old;
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public bool DisConnection(UserSession session)
        {
            return Interlocked.CompareExchange(ref m_session, null, session) == session;
        }

        #region  //自定义AMF3序列化,用于写在线列表

        /// <summary>
        /// 战宠/溜宠/坐骑
        /// </summary>
        /// <param name="writer"></param>
        public void WritePet(IExternalWriter writer)
        {
            // 写宠物信息
            writer.WriteKey("Pet");
            var pet = m_pet;
            if (pet == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(new PetSimple(pet, 3));
            }

            // 溜宠信息
            writer.WriteKey("SlipPets");
            writer.WriteValue(m_slippets);

            // 写坐骑信息,
            writer.WriteKey("Mounts");
            var mount = m_mounts;
            if (mount == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(new MountsSimple(mount));
            }
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            WriteBase(writer);
            WriteScene(writer);
            WriteShape(writer);
            WriteOther(writer);
            WritePet(writer);

            writer.WriteKey("ActionState");
            writer.WriteInt((int)AState);

            //writer.WriteKey("ShiZhuangSkin");
            //writer.WriteUTF(ShiZhuangSkin);
            //writer.WriteKey("SZSkin");
            //writer.WriteUTF("SZSkin");
            //writer.WriteKey("PID");
            //writer.WriteInt(this.PID);

            if (m_teamJob != TeamJob.NoTeam)
            {
                if (m_team != null)
                {
                    writer.WriteKey("TeamJob");
                    writer.WriteInt((int)m_teamJob);
                    writer.WriteKey("TeamID");
                    writer.WriteUTF(m_team.TeamID);
                }
            }
            if (m_yellowTime > DateTime.UtcNow)
            {
                writer.WriteKey("Yellow");
                writer.WriteDateTime(m_yellowTime);
            }
            if (m_redTime > DateTime.UtcNow)
            {
                writer.WriteKey("Red");
                writer.WriteDateTime(m_redTime);
            }
        }
        #endregion
    }
}
