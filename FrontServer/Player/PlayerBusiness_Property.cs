using System;
using System.Diagnostics;
using System.Threading;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Observer;
using Sinan.Util;
using MongoDB.Bson;
using Sinan.Log;
using Sinan.AMF3;
using Sinan.Data;

namespace Sinan.FrontServer
{
    partial class PlayerBusiness
    {
        readonly object m_locker = new object();

        /// <summary>
        /// 创建玩家属性
        /// </summary>
        /// <returns></returns>
        public PlayerProperty CreateProperty()
        {
            PlayerProperty p = new PlayerProperty();
            p.Add(m_ePro.Value);
            p.Add(m_sPro.Value);
            PlayerProperty life = PropertyHelper.CreateProperty(RoleID, Level, p);
            return life;
        }

        /// <summary>
        /// 初始化玩家信息
        /// </summary>
        /// <param name="session"></param>
        public void InitPlayer(UserSession session)
        {
            m_session = session;
            if (session != null)
            {
                Online = true;
                FightTime = DateTime.UtcNow;
                this.AState = ActionState.Standing;
                //this.Coin = UserLogAccess.Instance.GetCoin(m_userID);
            }

            if (m_life == null)
            {
                setPlayerEx();
                this.m_maxExp = RoleManager.Instance.GetRoleExp(this.Level);
                m_life = CreateProperty();
                if (this.HP == 0) this.HP = m_life.ShengMing;
                if (this.MP == 0) this.MP = m_life.MoFa;
                InitWalk();
                InitFixBuffer();
            }
        }

        /// <summary>
        /// 换装时更新玩家属性
        /// </summary>
        /// <param name="name">更新的位置</param>
        public void RefreshPlayer(string name, string value)
        {
            RefreshPlayer(name, value, ResetEquipsAdd);
        }


        /// <summary>
        /// 更新玩家属性
        /// </summary>
        private void RefreshPlayer(string name, string value, Action<PlayerProperty> action)
        {
            PlayerProperty old = m_life;
            PlayerProperty p = new PlayerProperty();
            action(p);
            m_life = CreateProperty();
            if (this.HP > m_life.ShengMing)
            {
                this.HP = m_life.ShengMing;
            }
            if (this.MP > m_life.MoFa)
            {
                this.MP = m_life.MoFa;
            }
            Variant v = m_life.GetChange(old);
            if (!string.IsNullOrEmpty(name))
            {
                SceneBusiness scene = m_scene;
                if (scene != null)
                {
                    Variant t = new Variant(2);
                    t[name] = value;
                    t["ID"] = this.ID;
                    this.CallAll(ClientCommand.UpdateActorR, t);
                }
                else
                {
                    v[name] = value;
                }
            }
            if (v.Count > 0)
            {
                if (v.ContainsKey("ShengMing"))
                {
                    v["ShengMing"] = new MVPair(m_life.ShengMing, this.HP);
                }
                if (v.ContainsKey("MoFa"))
                {
                    v["MoFa"] = new MVPair(m_life.MoFa, this.MP);
                }
                UpdataActorR(v);
            }
        }

        /// <summary>
        /// 升级后获取技能
        /// </summary>
        /// <param name="upLev"></param>
        private void NewSkill(int upLev)
        {
            bool addSkill = false;
            for (int i = this.Level - upLev + 1; i <= this.Level; i++)
            {
                string skillID = RoleManager.Instance.GetNewSkill(this.RoleID, i);
                if (!string.IsNullOrEmpty(skillID))
                {
                    //添加技能..
                    if (!this.m_skill.Value.ContainsKey(skillID))
                    {
                        this.m_skill.Value[skillID] = 1;
                        addSkill = true;
                        //通知客户端
                        this.Call(ClientCommand.GetNewSkillR, 0, skillID, i);
                        this.Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_skill));
                    }
                }
            }
            if (addSkill)
            {
                this.Skill.Save();
                this.RefeshSkill();
            }
        }

        /// <summary>
        /// 更新等级
        /// </summary>
        /// <returns></returns>
        private int UpLevel()
        {
            int upLev = 0;
            lock (m_locker)
            {
                while (true)
                {
                    m_maxExp = RoleManager.Instance.GetRoleExp(m_level);
                    if (m_exp < m_maxExp) break;
                    Interlocked.Add(ref m_exp, -m_maxExp);
                    if (m_level < APCProperty.MaxLevel)
                    {
                        m_level++;
                        LevelLogAccess.Instance.Insert(this._id, m_level, UserID);
                        upLev++;
                    }
                }
                if (upLev > 0)
                {
                    this.S = PlayerAccess.Instance.IncrementLevel(_id, upLev);
                    PlayerProperty old = m_life;
                    m_life = CreateProperty();
                    //生命和魔法加满
                    this.HP = m_life.ShengMing;
                    this.MP = m_life.MoFa;
                    UpdataActorR(old);
                }
            }
            return upLev;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="upLev"></param>
        private void UpdataActorR(PlayerProperty old)
        {
            Variant v = m_life.GetChange(old);
            v["ShengMing"] = new MVPair(m_life.ShengMing, this.HP);
            v["MoFa"] = new MVPair(m_life.MoFa, this.MP);
            v["JingYan"] = new MVPair(this.m_maxExp, this.m_exp);
            v["Level"] = m_level;
            UpdataActorR(v);
        }

        /// <summary>
        /// 添加玩家经验
        /// </summary>
        /// <param name="experience">经验值</param>
        /// <param name="eType"></param>
        /// <param name="des"></param>
        /// <returns></returns>
        public int AddExperience(int experience, FinanceType eType, string remark = null)
        {
            if (experience <= 0)
            {
                return 0;
            }
            int upLev = 0;
            int curExp = Interlocked.Add(ref m_exp, experience);
            if (curExp >= m_maxExp)
            {
                upLev = UpLevel();
                if (upLev > 0)
                {
                    UserNote note = new UserNote(this, TaskCommand.PlayerActivation, new object[] { upLev, this.Level });
                    Notifier.Instance.Publish(note);
                    //成就执行
                    this.FinishNote(FinishCommand.RoleUpLev, upLev);
                    //通知场景上的玩家
                    Variant v = new Variant(2);
                    v["ID"] = this.ID;
                    v["Level"] = this.m_level;
                    this.CallAll(ClientCommand.UpdateActorR, v);
                    //获取技能
                    NewSkill(upLev);
                }
            }
            else
            {
                Variant v = new Variant(2);
                v["JingYan"] = new MVPair(this.m_maxExp, this.m_exp);
                UpdataActorR(v);
            }
            PlayerAccess.Instance.Save(this);

            PlayerLog log = new PlayerLog(ServerLogger.zoneid, Actiontype.RoleExp);
            log.level = m_level;
            log.modifyexp = experience;
            log.totalexp = curExp;
            log.reserve_1 = (int)eType;
            log.remark = remark;
            this.WriteLog(log);
            return upLev;
        }

        /// <summary>
        /// 失去经验
        /// </summary>
        /// <param name="b">百分比</param>
        public void LostExperience(double b, FinanceType eType, string remark = null)
        {
            if (b > 0 && b <= 1.0)
            {
                int experience = (int)(m_exp * b);
                if (experience > 0)
                {
                    m_exp -= experience;
                    Variant v = new Variant(2);
                    v["JingYan"] = new MVPair(this.m_maxExp, this.m_exp);
                    UpdataActorR(v);

                    PlayerAccess.Instance.Save(this);

                    PlayerLog log = new PlayerLog(ServerLogger.zoneid, Actiontype.RoleExp);
                    log.level = m_level;
                    log.modifyexp = -experience;
                    log.totalexp = m_exp;
                    log.reserve_1 = (int)eType;
                    log.remark = remark;
                    this.WriteLog(log);
                }
            }
        }

        /// <summary>
        /// 添加晶币
        /// </summary>
        /// <param name="coin"></param>
        /// <param name="ft"></param>
        /// <returns></returns>
        public bool AddCoin(int coin, FinanceType ft, string remark = null)
        {
            BsonDocument modified = PlayerAccess.Instance.UpdateFinance(_id, coin);
            if (modified == null)
            {
                return false;
            }
            Int64 newCoin = Convert.ToInt64(modified["Coin"]);
            this.Coin = newCoin;

            if (coin == 0)
            {
                //更新值
                return true;
            }

            if (coin < 0)
            {
                this.GCE = Convert.ToInt64(modified["GCE"]);
            }
            else
            {
                this.GCI = Convert.ToInt64(modified["GCI"]);
            }
            Variant v = new Variant(2);
            v.Add("Coin", this.Coin);
            UpdataActorR(v);
            //写日志
            PayLog log = new PayLog(ServerLogger.zoneid, Actiontype.Consumption);
            log.modifycoin = coin;
            log.totalcoin = newCoin;
            log.toopenid = UserID;
            log.touid = PID;
            log.reserve_1 = (int)ft;
            log.remark = remark;
            this.WriteLog(log);
            if (coin > 0)
            {
                this.FinishNote(FinishCommand.MaxMoney, coin, 0);
            }
            else
            {
                //发送消费通知
                UserNote note2 = new UserNote(this, SocialCommand.ConsumeCoin, new object[] { coin, ft });
                Sinan.Observer.Notifier.Instance.Publish(note2);
            }
            return true;
        }

        /// <summary>
        /// 添加游戏币
        /// </summary>
        /// <param name="score"></param>
        /// <param name="ft"></param>
        /// <returns></returns>
        public bool AddScore(int score, FinanceType ft, string remark = null)
        {
            if (score != 0)
            {
                BsonDocument modified = PlayerAccess.Instance.SafeUpdate(_id, "Score", score);
                if (modified == null)
                {
                    return false;
                }
                Int64 newScore = Convert.ToInt64(modified["Score"]);
                this.Score = newScore;
                Variant v = new Variant(2);
                v.Add("Score", this.Score);
                this.UpdataActorR(v);
                //写日志
                PayLog log = new PayLog(ServerLogger.zoneid, Actiontype.Score);
                log.modifyfee = score;
                //log.modifycoin = coin;
                //log.totalcoin = this.Coin;
                log.totalfee = newScore;
                log.toopenid = UserID;
                log.touid = PID;
                log.reserve_1 = (int)ft;
                log.remark = remark;
                this.WriteLog(log);

                if (score > 0)
                {
                    this.FinishNote(FinishCommand.MaxMoney, 0, score);
                }
            }
            return true;
        }

        /// <summary>
        /// 添加点券
        /// </summary>
        /// <param name="bond"></param>
        /// <param name="eType"></param>
        /// <param name="des"></param>
        /// <returns></returns>
        public bool AddBond(int bond, FinanceType ft, string remark = null)
        {
            if (bond != 0)
            {
                BsonDocument modified = PlayerAccess.Instance.SafeUpdate(_id, "Bond", bond);
                if (modified == null)
                {
                    return false;
                }
                long newBond = Convert.ToInt64(modified["Bond"]);
                this.Bond = newBond;
                Variant v = new Variant(2);
                v.Add("Bond", this.Bond);

                this.UpdataActorR(v);
                //写日志
                PayLog log = new PayLog(ServerLogger.zoneid, Actiontype.Bond);
                log.modifyfee = bond;
                log.totalfee = newBond;
                log.toopenid = UserID;
                log.touid = PID;
                log.reserve_1 = (int)ft;
                log.remark = remark;
                this.WriteLog(log);
            }
            return true;
        }

        /// <summary>
        /// 添加星力
        /// </summary>
        /// <param name="starPower"></param>
        /// <param name="ft"></param>
        /// <param name="des"></param>
        /// <returns></returns>
        public bool AddStarPower(int starPower, FinanceType ft, string remark = null)
        {
            if (starPower != 0)
            {
                BsonDocument modified = PlayerAccess.Instance.SafeUpdate(_id, "StarPower", starPower);
                if (modified == null)
                {
                    return false;
                }
                this.StarPower = Convert.ToInt32(modified["StarPower"]);
                Variant v = new Variant(2);
                v.Add("StarPower", this.StarPower);
                this.UpdataActorR(v);
            }
            return true;
        }


        /// <summary>
        /// 添加感恩值
        /// </summary>
        /// <param name="owe"></param>
        /// <param name="eType"></param>
        /// <param name="des"></param>
        /// <returns></returns>
        public bool AddOwe(int owe, FinanceType ft, string remark = null)
        {
            BsonDocument modified = PlayerAccess.Instance.SafeUpdate(_id, "Owe", owe);
            if (modified == null)
            {
                return false;
            }
            this.Owe = Convert.ToInt32(modified["Owe"]);
            Variant v = new Variant(2);
            v.Add("Owe", this.Owe);
            this.UpdataActorR(v);
            return true;
        }

        /// <summary>
        /// 战绩
        /// </summary>
        /// <param name="fightvalue">变更量</param>
        /// <param name="fb"></param>
        /// <returns></returns>
        public bool AddFightValue(int fightvalue, bool check, FinanceType fb, string remark = null)
        {
            if (fightvalue != 0)
            {
                BsonDocument modified = PlayerAccess.Instance.SafeUpdate(_id, "FightValue", fightvalue, check);
                if (modified == null)
                {
                    return false;
                }
                this.FightValue = Convert.ToInt32(modified["FightValue"]);
                Variant v = new Variant(2);
                v.Add("FightValue", this.FightValue);
                this.UpdataActorR(v);
                return true;
            }
            return true;
        }

        /// <summary>
        /// 设置称号
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public string SetTitle(string prefix, string suffix)
        {
            string name = string.Empty;
            if (m_title != null)
            {
                if (!string.IsNullOrEmpty(prefix) && m_title.Value.ContainsKey(prefix))
                {
                    m_title.Value["Prefix"] = prefix;
                    name = m_title.Value.GetStringOrDefault(prefix);
                }
                else
                {
                    m_title.Value["Prefix"] = string.Empty;
                }

                if (!string.IsNullOrEmpty(suffix) && m_title.Value.ContainsKey(suffix))
                {
                    m_title.Value["Suffix"] = suffix;
                    name += m_title.Value.GetStringOrDefault(suffix);
                }
                else
                {
                    m_title.Value["Suffix"] = string.Empty;
                }
                m_title.Save();
                this.SetNicename(name);
                RefreshPlayer("Nicename", this.Nicename, ResetSkillsAdd);
            }
            return name;
        }

        /// <summary>
        /// 发送属性更新通知
        /// </summary>
        /// <param name="v"></param>
        public void UpdataActorR(Variant v)
        {
            try
            {
                if (v == null || v.Count == 0 || m_session == null)
                {
                    return;
                }
                v["ID"] = this.ID;
                m_session.Call(ClientCommand.UpdateActorR, v);
            }
            catch { }
        }


        /// <summary>
        /// 添加会员成长度
        /// </summary>
        /// <param name="czd">增加值</param>
        /// <param name="gs">成长度来源</param>
        public void AddCZD(int czd, GoodsSource gs)
        {
            if (czd <= 0)
                return;
            //得到最大等级      
            int max = MemberAccess.MemberMax();
            if (czd > 200000000)
                return;
            CZD += czd;
            if (MLevel >= max)
                return;
            int mlevel = MLevel;            
            while (true)
            {
                Variant mv = MemberAccess.MemberInfo(MLevel);
                if (mv == null)
                    break;
                int a = mv.GetIntOrDefault("A");
                if (CZD < a)
                    break;
                MLevel++;
                //CZD -= a;   
                if (MLevel >= max)
                    break;
            }

            //保存会员等级与会员当前成长度
            if (PlayerAccess.Instance.SaveValue(PID, new Tuple<string, BsonValue>("MLevel", MLevel), new Tuple<string, BsonValue>("CZD", CZD)))
            {
                if (MLevel > mlevel)
                {
                    for (int i = mlevel + 1; i <= MLevel; i++)
                    {
                        UserNote note2 = new UserNote(this, MemberCommand.MemberUp, new object[] { i });
                        Notifier.Instance.Publish(note2);
                    }
                }
            }

            //通知客户端
            Variant v = new Variant(3);
            v.Add("CZD", CZD);
            v.Add("MLevel", MLevel);
            UpdataActorR(v);
            //记录日志
            AddLog(Actiontype.CZD, MLevel.ToString(), czd, gs, mlevel.ToString(), 0);
        }
    }
}