using System;
using System.Collections.Generic;
using Sinan.Extensions;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;
using Sinan.AMF3;

namespace Sinan.FrontServer
{
    //战斗外Buffer
    partial class PlayerBusiness
    {
        /// <summary>
        /// 自动补充的东东
        /// </summary>
        static readonly string[] autoKeys = new string[] { "RHP", "PHP" };

        /// <summary>
        /// 外理退出不计时的Buffer
        /// "RExp":双倍经验,
        /// "AF":战斗计时器,
        /// "AH":"额外增加的遇怪数"
        /// </summary>
        static readonly string[] exitStop = new string[] { "RExp", "AF", "AH" };

        /// <summary>
        /// 有BUFF时自动战斗最大回合数
        /// </summary>
        const int MaxAutoFight = 999;

        /// <summary>
        /// 默认自动战斗最大回合数
        /// </summary>
        static readonly int DefaultAutoFight;

        int m_autoFight;

        static PlayerBusiness()
        {
            int autoFight = RoleManager.Instance.GetValue<int>("AutoFight");
            if (autoFight <= 0)
            {
                autoFight = 90;
            }
            DefaultAutoFight = autoFight;
        }

        /// <summary>
        /// 自动战斗次数
        /// </summary>
        public int AutoFight
        {
            get { return m_autoFight; }
        }

        #region AST(AST:A:值.T:到期时间;S:剩余时间(下线不计时)
        /// <summary>
        /// 设置AST类Buffer
        /// (AST:A:值.T:到期时间;S:剩余时间(下线不计时)
        /// </summary>
        /// <param name="name">
        /// RExp:角色双倍经验,
        /// PExp:宠物双倍经验,
        /// AF:自动战斗时间.
        /// AH:额外增加的遇怪数
        /// </param>
        /// <param name="a"></param>
        /// <param name="second">效果持续时间(秒)</param>
        /// <returns></returns>
        private bool SetASTBuffer(string name, double a, int second)
        {
            if (m_assist.Value == null) return false;
            Variant v = m_assist.Value.GetVariantOrDefault(name);
            if (v == null)
            {
                v = new Variant();
                m_assist.Value[name] = v;
                v["A"] = a;
                v["S"] = second;
                v["T"] = DateTime.UtcNow.AddSeconds(second);
            }
            else
            {
                double oldA = v.GetDoubleOrDefault("A");
                if (oldA == a)//时间叠加
                {
                    DateTime t = v.GetDateTimeOrDefault("T");
                    int s = v.GetIntOrDefault("S");
                    if (t < DateTime.UtcNow)
                    {
                        t = DateTime.UtcNow;
                        s = 0;
                    }
                    v["S"] = s + second;
                    v["T"] = t.AddSeconds(second);
                }
                else //替换
                {
                    v["A"] = a;
                    v["S"] = second;
                    v["T"] = DateTime.UtcNow.AddSeconds(second);
                }
            }
            this.Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_assist));
            return m_assist.Save();
        }

        private double GetASTBuffer(string name)
        {
            if (m_assist.Value != null)
            {
                Variant v = m_assist.Value.GetVariantOrDefault(name);
                if (v != null)
                {
                    DateTime t = v.GetDateTimeOrDefault("T");
                    if (t > DateTime.UtcNow)
                    {
                        return v.GetDoubleOrDefault("A");
                    }
                    else
                    {
                        m_assist.Value.Remove(name);
                        this.Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_assist));
                        m_assist.Save();
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// 剩余秒数.转为到期时间
        /// </summary>
        private void SecondToTime()
        {
            if (m_assist.Value == null)
            {
                return;
            }
            foreach (string key in exitStop)
            {
                var v = m_assist.Value.GetVariantOrDefault(key);
                if (v != null)
                {
                    int s = v.GetIntOrDefault("S");
                    if (s > 0)
                    {
                        v["T"] = DateTime.UtcNow.AddSeconds(s);
                    }
                    else
                    {
                        m_assist.Value.Remove(key);
                    }
                    m_assist.Changed = true;
                }
            }
        }

        /// <summary>
        /// 到期时间,转为剩余秒数存储
        /// </summary>
        private void TimeToSecond()
        {
            if (m_assist.Value == null) return;
            bool change = m_assist.Changed;
            foreach (string key in exitStop)
            {
                var v = m_assist.Value.GetVariantOrDefault(key);
                if (v != null)
                {
                    DateTime t = v.GetDateTimeOrDefault("T");
                    double s = (t - DateTime.UtcNow).TotalSeconds;
                    if (s > 0)
                    {
                        v["S"] = (int)s;
                    }
                    else
                    {
                        m_assist.Value.Remove(key);
                    }
                    change = true;
                }
            }
            if (change)
            {
                m_assist.Save();
            }
        }
        #endregion

        #region 双倍经验

        /// <summary>
        /// 取多倍经验系数
        /// </summary>
        /// <returns></returns>
        public double GetDoubleExp()
        {
            double a = GetASTBuffer("RExp");
            if (a == 0)
            {
                return ExperienceControl.ExpCoe;
            }
            else
            {
                return Math.Max(0, a - 1) + ExperienceControl.ExpCoe;
            }
        }

        public bool SetDoubleExp(Variant v, int number = 1)
        {
            double a = v.GetDoubleOrDefault("A");
            int second = v.GetIntOrDefault("S") * number;
            if (a > 0 && second > 0)
            {
                SetASTBuffer("RExp", a, second);
            }
            return true;
        }
        #endregion

        #region 自动加满(泉水和花蜜)
        /// <summary>
        /// 设置自动加满(累加方式)
        /// "RHP":角色血/魔加满
        /// "PHP":宠物血/魔加满
        /// </summary>
        /// <param name="v"></param>
        public bool SetAutoFull(Variant v, int number = 1)
        {
            if (v == null) return false;
            foreach (string name in autoKeys)
            {
                int rhp = v.GetIntOrDefault(name) * number;
                if (rhp > 0)
                {
                    m_assist.Value.SetOrInc(name, rhp);
                    FristAdd(name);
                }
            }
            this.Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_assist));
            m_assist.Save();
            return true;
        }

        /// <summary>
        /// 产生BUFF前补满
        /// </summary>
        /// <param name="name"></param>
        private void FristAdd(string name)
        {
            int add = 0;
            //加满
            if (name == "RHP")
            {
                add = AutoFullRHP() + AutoFullRMP();
            }

            if (add > 0)
            {
                Variant v = new Variant(4);
                m_assist.Save();
                v["ShengMing"] = new MVPair(m_life.ShengMing, this.HP);
                v["MoFa"] = new MVPair(m_life.MoFa, this.MP);
                v["Assist"] = m_assist;
                UpdataActorR(v);
                return;
            }

            if (name == "PHP")
            {
                if (m_pet != null && m_pet.Value != null)
                {
                    add = AutoFullPHP(m_pet) + AutoFullPMP(m_pet);
                }
            }

            if (add > 0)
            {
                Variant v = new Variant(3);
                v["ShengMing"] = m_pet.Value.GetVariantOrDefault("ShengMing");
                v["MoFa"] = m_pet.Value.GetVariantOrDefault("MoFa");
                v["ID"] = m_pet.ID;
                this.Call(PetsCommand.UpdatePetR, true, v);
                this.Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_assist));
            }
        }

        /// <summary>
        /// 自动补满
        /// </summary>
        public bool AutoFull()
        {
            int total = AutoFullRHP();
            total += AutoFullRMP();
            if (total > 0)
            {
                m_assist.Save();
                Variant v = new Variant(6);
                v["ShengMing"] = new MVPair(m_life.ShengMing, this.HP);
                v["MoFa"] = new MVPair(m_life.MoFa, this.MP);
                v["JingYan"] = new MVPair(this.m_maxExp, this.m_exp);
                v["Level"] = m_level;
                v["Assist"] = m_assist;
                UpdataActorR(v);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 自动加满角色的HP
        /// </summary>
        /// <returns></returns>
        private int AutoFullRHP()
        {
            int need = m_life.ShengMing - this.HP;
            if (need > 0)
            {
                int usable;
                if (m_assist.Value.TryGetValueT<int>("RHP", out usable) && usable > 0)
                {
                    if (usable > need)
                    {
                        this.HP += need;
                        m_assist.Value["RHP"] = usable - need;
                        return need;
                    }
                    else
                    {
                        m_assist.Value.Remove("RHP");
                        this.HP += usable;
                        return usable;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// 自动加满角色的MP
        /// </summary>
        /// <returns></returns>
        private int AutoFullRMP()
        {
            int need = m_life.MoFa - this.MP;
            if (need > 0)
            {
                int usable;
                if (m_assist.Value.TryGetValueT<int>("RHP", out usable) && usable > 0)
                {
                    if (usable > need)
                    {
                        this.MP += need;
                        m_assist.Value["RHP"] = usable - need;
                        return need;
                    }
                    else
                    {
                        m_assist.Value.Remove("RHP");
                        this.MP += usable;
                        return usable;
                    }
                }
            }
            return 0;
        }

        public int AutoFullPet()
        {
            if (m_pet == null)
            {
                return 0;
            }
            int hp = AutoFullPHP(m_pet);
            int mp = AutoFullPMP(m_pet);
            if (hp > 0 || mp > 0)
            {
                this.Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_assist));
            }
            return hp + mp;
        }

        /// <summary>
        /// 自动加满宠物的HP
        /// </summary>
        /// <returns></returns>
        private int AutoFullPHP(Pet pet)
        {
            Variant shengMing = pet.Value.GetVariantOrDefault("ShengMing");
            //"ShengMing" : { "M" : 88, "V" : 88 }
            int need = shengMing.GetIntOrDefault("M") - pet.HP;
            if (need > 0)
            {
                int usable; //可用的
                if (m_assist.Value.TryGetValueT<int>("PHP", out usable) && usable > 0)
                {
                    if (usable > need)
                    {
                        pet.HP += need;
                        shengMing["V"] = pet.HP;
                        m_assist.Value["PHP"] = usable - need;
                        return need;
                    }
                    else
                    {
                        m_assist.Value.Remove("PHP");
                        pet.HP += usable;
                        shengMing["V"] = pet.HP;
                        return usable;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// 自动加满宠物的HP
        /// </summary>
        /// <returns></returns>
        private int AutoFullPMP(Pet pet)
        {
            Variant moFa = pet.Value.GetVariantOrDefault("MoFa");
            //"MoFa" : { "M" : 27, "V" : 27 },
            int need = moFa.GetIntOrDefault("M") - pet.MP;
            if (need > 0)
            {
                int usable;
                if (m_assist.Value.TryGetValueT<int>("PHP", out usable) && usable > 0)
                {
                    if (usable > need)
                    {
                        pet.MP += need;
                        moFa["V"] = pet.MP;
                        m_assist.Value["PHP"] = usable - need;
                        return need;
                    }
                    else
                    {
                        m_assist.Value.Remove("PHP");
                        pet.MP += usable;
                        moFa["V"] = pet.MP;
                        return usable;
                    }
                }
            }
            return 0;
        }
        #endregion

        #region 增强战斗力
        public bool SetFightPro(Variant v, int number = 1)
        {
            string name = v.GetStringOrDefault("ProType");
            if (name == "WG" || name == "MG" || name == "MF" || name == "WF" || name == "CP")
            {
                int a = v.GetIntOrDefault("A");
                int second = v.GetIntOrDefault("S") * number;
                if (a > 0 && second > 0)
                {
                    SetFightPro(name, a, second);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 增强战斗力
        /// </summary>
        /// <param name="name">
        /// WG:强力 MG:魔能
        /// MF:坚韧 WF:
        /// CP:增加捕宠概率
        /// </param>
        /// <param name="a"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private bool SetFightPro(string name, int a, int second)
        {
            Variant v = m_assist.Value.GetVariantOrDefault(name);
            if (v == null)
            {
                v = new Variant();
                m_assist.Value[name] = v;
                v["A"] = a;
                v["T"] = DateTime.UtcNow.AddSeconds(second);
            }
            else
            {
                double oldA = v.GetDoubleOrDefault("A");
                if (oldA == a)//时间叠加
                {
                    DateTime t = v.GetDateTimeOrDefault("T");
                    if (t < DateTime.UtcNow)
                    {
                        t = DateTime.UtcNow;
                    }
                    v["T"] = t.AddSeconds(second);
                }
                else //替换
                {
                    v["A"] = a;
                    v["T"] = DateTime.UtcNow.AddSeconds(second);
                }
            }
            this.Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_assist));
            return m_assist.Save();
        }
        #endregion

        #region 自动战斗
        public bool SetAutoFight(Variant v, int number = 1)
        {
            int second = v.GetIntOrDefault("S") * number;
            if (second > 0)
            {
                SetASTBuffer("AF", 1, second);
            }
            return true;
        }

        /// <summary>
        /// 开始自动战斗
        /// </summary>
        /// <returns></returns>
        public int StartAutoFight()
        {
            Variant af = m_assist.Value.GetVariantOrDefault("AF");
            int count = DefaultAutoFight;
            if (af != null)
            {
                DateTime t = af.GetDateTimeOrDefault("T");
                if (t > DateTime.UtcNow)
                {
                    count = MaxAutoFight;
                }
                else
                {
                    af.Remove("AF");
                    this.Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_assist));
                    m_assist.Save();
                }
            }
            return count;
        }

        /// <summary>
        /// 取消自动战斗
        /// </summary>
        public void EndAutoFight()
        {
            this.m_autoFight = 0;
        }

        /// <summary>
        /// 减去自动战斗回合数
        /// </summary>
        /// <returns></returns>
        public int DecrementAutoFight()
        {
            if (m_autoFight > 0 && m_autoFight < MaxAutoFight)
            {
                m_autoFight--;
            }
            return m_autoFight;
        }

        /// <summary>
        /// 检查自动战斗到期时间
        /// </summary>
        /// <returns></returns>
        public void CheckAutoFight()
        {
            double a = GetASTBuffer("AF");
            if (a == 0)
            {
                if (m_autoFight >= MaxAutoFight)
                {
                    m_autoFight = DefaultAutoFight;
                }
            }
        }
        #endregion

        #region 增加遇怪(暗雷)数
        public bool SetAddHideApc(Variant v, int number = 1)
        {
            double a = v.GetDoubleOrDefault("A");
            int second = v.GetIntOrDefault("S") * number;
            if (a > 0 && second > 0)
            {
                SetASTBuffer("AH", a, second);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetAddHideApc()
        {
            return (int)GetASTBuffer("AH");
        }
        #endregion

        #region 补充HP/MP
        /// <summary>
        /// 补充玩家的HP/MP
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool SupplyRole(Variant v)
        {
            double dhp = v.GetDoubleOrDefault("HP");
            double dmp = v.GetDoubleOrDefault("MP");
            int hp, mp;
            if (dhp <= 1)
            {
                hp = (int)(dhp * m_life.ShengMing); //百分比方式
            }
            else
            {
                hp = (int)(dhp);
            }
            if (dmp <= 1)
            {
                mp = (int)(dmp * m_life.MoFa); //百分比方式
            }
            else
            {
                mp = (int)(dmp);
            }

            bool use = false;
            if (hp > 0)
            {
                int need = m_life.ShengMing - this.HP;
                if (need > 0)
                {
                    this.HP += Math.Min(need, hp);
                    use = true;
                }
            }

            if (mp > 0)
            {
                int need = m_life.MoFa - this.MP;
                if (need > 0)
                {
                    this.MP += Math.Min(need, mp);
                    use = true;
                }
            }
            if (use)
            {
                Variant changed = new Variant(3);
                if (hp > 0) changed["ShengMing"] = new MVPair(m_life.ShengMing, this.HP);
                if (mp > 0) changed["MoFa"] = new MVPair(m_life.MoFa, this.MP);
                UpdataActorR(changed);
            }
            return use;
        }

        /// <summary>
        /// 补充宠物的HP/MP
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool SupplyPet(Variant v)
        {
            bool use = false;
            //庞物
            if (m_pet == null) return use;
            Variant pet = m_pet.Value;
            if (pet == null) return use;

            Variant moFa = pet.GetVariantOrDefault("MoFa");
            Variant shengMing = pet.GetVariantOrDefault("ShengMing");

            double dhp = v.GetDoubleOrDefault("HP");
            double dmp = v.GetDoubleOrDefault("MP");
            int hp, mp;
            if (dhp <= 1)
            {
                hp = (int)(dhp * shengMing.GetIntOrDefault("M")); //百分比方式
            }
            else
            {
                hp = (int)(dhp);
            }
            if (dmp <= 1)
            {
                mp = (int)(dmp * moFa.GetIntOrDefault("M")); //百分比方式
            }
            else
            {
                mp = (int)(dmp);
            }

            if (hp > 0)
            {
                int sv = shengMing.GetIntOrDefault("V");
                int need = shengMing.GetIntOrDefault("M") - sv;
                if (need > 0)
                {
                    m_pet.HP = Math.Min(need, hp) + sv;
                    shengMing["V"] = m_pet.HP;
                    use = true;
                }
            }

            if (mp > 0)
            {
                int mv = moFa.GetIntOrDefault("V");
                int need = moFa.GetIntOrDefault("M") - mv;
                if (need > 0)
                {
                    m_pet.MP = Math.Min(need, mp) + mv;
                    moFa["V"] = m_pet.MP;
                    use = true;
                }
            }

            if (hp > 0 || mp > 0)
            {
                this.Call(PetsCommand.UpdatePetR, true, m_pet);
                m_pet.Save();
            }
            return use;
        }
        #endregion

        #region PK惩罚
        DateTime m_yellowTime;
        DateTime m_redTime;

        /// <summary>
        /// 设置红/黄名
        /// </summary>
        private void SetNameTime()
        {
            Variant buf = m_assist.Value.GetVariantOrDefault("X_b100");
            if (buf != null)
            {
                m_yellowTime = buf.GetDateTimeOrDefault("T");
            }

            buf = m_assist.Value.GetVariantOrDefault("X_b101");
            if (buf != null)
            {
                m_redTime = buf.GetDateTimeOrDefault("T");
            }
        }

        /// <summary>
        /// 黄名(和谐的警告)
        /// </summary>
        /// <returns></returns>
        public bool YellowName
        {
            get { return m_yellowTime > DateTime.UtcNow; }
        }

        /// <summary>
        /// 红名(神的惩罚)
        /// </summary>
        /// <returns></returns>
        public bool RedName
        {
            get { return m_redTime > DateTime.UtcNow; }
        }

        public bool SetPKKill(int count)
        {
            Variant buf = m_assist.Value.GetVariantOrDefault("X_b100");
            if (buf == null)
            {
                buf = new Variant();
                m_assist.Value["X_b100"] = buf;
            }
            int a = buf.GetIntOrDefault("A");
            if (m_yellowTime < DateTime.UtcNow)
            {
                if (a >= 5)
                {
                    a = 0;
                }
            }
            a += count;
            buf["A"] = a;
            if (a >= 5)
            {
                //设置和谐的警告
                if (m_yellowTime < DateTime.UtcNow)
                {
                    m_yellowTime = DateTime.UtcNow.AddMinutes((a - 4) * 60);
                }
                else
                {
                    m_yellowTime = m_yellowTime.AddMinutes(count * 60);
                }
                buf["T"] = m_yellowTime;
                Variant v = new Variant(3);
                v.Add("ID", this.ID);
                v.Add("Yellow", m_yellowTime);

                if (a >= 10)
                {
                    //设置神的惩罚
                    if (a - count > 10)
                    {
                        SetPKKill2(count);
                    }
                    else
                    {
                        SetPKKill2(a - 10);
                    }
                    v.Add("Red", m_redTime);
                }
                this.CallAll(ClientCommand.UpdateActorR, v);
                this.Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_assist));
            }
            return m_assist.Save();
        }

        void SetPKKill2(int count)
        {
            Variant buf = m_assist.Value.GetVariantOrDefault("X_b101");
            if (buf == null)
            {
                buf = new Variant();
                m_assist.Value["X_b101"] = buf;
            }
            if (m_redTime < DateTime.UtcNow)
            {
                buf["A"] = 0;
            }
            int t = buf.SetOrInc("A", count);
            //设置神的惩罚
            if (m_redTime < DateTime.UtcNow)
            {
                m_redTime = DateTime.UtcNow.AddMinutes(t * 30);
            }
            else
            {
                m_redTime = m_redTime.AddMinutes(count * 30);
            }
            buf["T"] = m_redTime;
        }
        #endregion

        #region  PK保护
        public bool SetPKProtect(Variant v, int number = 1)
        {
            int second = v.GetIntOrDefault("S") * number;
            if (second > 0)
            {
                DateTime now = DateTime.UtcNow;
                Variant pk = m_assist.Value.GetVariantOrDefault("PK");
                if (pk == null)
                {
                    pk = new Variant();
                    m_assist.Value["PK"] = pk;
                    pk["A"] = 0;
                    pk["T"] = now.AddSeconds(second);
                }
                else
                {
                    DateTime t = pk.GetDateTimeOrDefault("T");
                    if (t < now)
                    {
                        t = now.AddSeconds(second);
                    }
                    else
                    {
                        t = t.AddSeconds(second);
                        //最多累计24小时
                        if ((t - now).TotalHours > 24)
                        {
                            return false;
                        }
                    }

                    pk["T"] = t;// t.AddSeconds(second);
                }
                this.Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_assist));
                m_assist.Save();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取PK保护时间
        /// </summary>
        /// <returns></returns>
        public DateTime GetPKProtect()
        {
            Variant v = m_assist.Value.GetVariantOrDefault("PK");
            if (v != null)
            {
                return v.GetDateTimeOrDefault("T");
            }
            return DateTime.MinValue;
        }
        #endregion
    }
}
