using System;
using Sinan.ArenaModule.Business;
using Sinan.ArenaModule.Detail;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.Util;

namespace Sinan.ArenaModule.Fight
{
    public class FightBase
    {
        private string m_id;
        private string m_name;
        private string m_playerid;
        private string m_curskill;
        private int m_persons;
        private int m_mpcost;
        private int m_hpcost;
        private int m_injuryType;
        private int m_a;
        private int m_b;
        private string m_rangepet;        
        private FightType m_fb;
        private Variant m_shingming;
        private Variant m_mofa;
        private ArenaBase m_model;
        

        /// <summary>
        /// 宠物ID
        /// </summary>
        public string ID 
        {
            get { return m_id; }
            set { m_id = value; }
        }

        /// <summary>
        /// 宠物名字
        /// </summary>
        public string Name 
        {
            get { return m_name; }
            set { m_name = value; }
        }
        /// <summary>
        /// 宠物所属角色
        /// </summary>
        public string PlayerID 
        {
            get { return m_playerid; }
            set { m_playerid = value; }
        }

        /// <summary>
        /// 使用或中的技能
        /// </summary>
        public string CurSkill 
        {
            get { return m_curskill; }
            set { m_curskill = value; }
        }

        /// <summary>
        /// 攻击人数
        /// </summary>
        public int Persons
        {
            get { return m_persons; }
            set { m_persons = value; }
        }

        /// <summary>
        /// 魔法消耗值
        /// </summary>
        public int MPCost
        {
            get { return m_mpcost; }
            set { m_mpcost = value; }
        }

        /// <summary>
        /// 生命受害值
        /// </summary>
        public int HPcost
        {
            get { return m_hpcost; }
            set { m_hpcost = value; }
        }

        /// <summary>
        /// 战斗类型
        /// </summary>
        public int InjuryType
        {
            get { return m_injuryType; }
            set { m_injuryType = value; }
        }

        /// <summary>
        /// 攻击目标
        /// </summary>
        public string RangePet 
        {
            get { return m_rangepet; }
            set { m_rangepet = value; }
        }

        /// <summary>
        /// 攻击类型
        /// </summary>
        public FightType FB 
        {
            get { return m_fb; }
            set { m_fb = value; }
        }

        /// <summary>
        /// 技能最小受害
        /// </summary>
        public int A
        {
            get { return m_a; }
            set { m_a = value; }
        }

        /// <summary>
        /// 技能最大受害
        /// </summary>
        public int B
        {
            get { return m_b; }
            set { m_b = value; }
        }


        /// <summary>
        /// 当前宠物的生命
        /// </summary>
        public Variant ShengMing 
        {
            get { return m_shingming; }
            set { m_shingming = value; }
        }

        /// <summary>
        /// 当前魔法
        /// </summary>
        public Variant MoFa 
        {
            get { return m_mofa; }
            set { m_mofa = value; }
        }

        private Pet m_root;//攻击对象
        private Pet m_target;//目标对象
        
        public FightBase() 
        {
        }
        public FightBase(ArenaBase model)
        {
            m_model = model;
        }

        /// <summary>
        /// 得到攻击受害值
        /// </summary>
        /// <param name="root">攻击者</param>
        /// <param name="target">被攻击者</param>
        /// <returns></returns>
        public PetFightDetail FightObject(Pet root, Pet target) 
        {
            m_root = root;
            m_target = target;

            FightBase fightBase = new FightBase();
            fightBase.ID = target.ID;
            fightBase.Name = target.Name;
            fightBase.PlayerID = target.PlayerID;
            fightBase.CurSkill = m_curskill;
            //判断是否命中
            if (m_injuryType == 0)
            {
                if (CheckMingZhong(m_root.Value, m_target.Value))
                {
                    if (CheckBaoJi(m_root.Value, m_target.Value))
                    {
                        //暴击
                        fightBase.FB = FightType.BaoJi;
                    }
                    else
                    {
                        //普通攻击
                        fightBase.FB = FightType.PuTong;
                    }
                    fightBase.HPcost = FightValue(fightBase.FB);
                    //最小1点受害
                    fightBase.HPcost = fightBase.HPcost >= 1 ? fightBase.HPcost : 1;
                }
                else
                {
                    //闪避成功
                    fightBase.FB = FightType.ShangBi;
                    fightBase.HPcost = 0;
                }
            }
            else 
            {
                if (CheckBaoJi(m_root.Value, m_target.Value))
                {
                    //暴击
                    fightBase.FB = FightType.BaoJi;
                }
                else
                {
                    //普通攻击
                    fightBase.FB = FightType.PuTong;
                }
                fightBase.HPcost = FightValue(fightBase.FB);
                //最小1点受害
                fightBase.HPcost = fightBase.HPcost >= 1 ? fightBase.HPcost : 1;
            }

            Variant sm = m_target.Value.GetValueOrDefault<Variant>("ShengMing");
            int m = sm.GetIntOrDefault("V");
            
            if (fightBase.HPcost >= m)
            { 
                sm["V"] = 0;

                //计算相关战绩值
                GetFightValue();
                //宠物被打挂
                Variant tmp = new Variant();
                tmp.Add("ID", target.ID);
                tmp.Add("Level", target.Value.GetIntOrDefault("PetsLevel"));
                root.FightDeath.Add(tmp);

                PetDetail model = new PetDetail(root, "", 2);
                m_model.CallAll(ArenaCommand.ArenaPetOverR, model);
            }
            else 
            {
                sm["V"] = m - fightBase.HPcost;
            }
            fightBase.ShengMing = sm;
            fightBase.MoFa = m_target.Value.GetValueOrDefault<Variant>("MoFa");
            return new PetFightDetail(fightBase);
        }

        /// <summary>
        /// 得到战斗受害值
        /// </summary>
        /// <param name="fb">1为普通,2暴击,3闪避</param>
        /// <returns></returns>
        protected int FightValue(FightType fb) 
        {
            int m = 0;
            if (m_a <= m_b)
            {
                m = NumberRandom.Next(m_a, m_b);
            }
            

            if (m_injuryType == 0)
            {
                //物理攻击
                Variant GongJi = m_root.Value.GetValueOrDefault<Variant>("GongJi");
                Variant FangYu = m_target.Value.GetValueOrDefault<Variant>("FangYu");
                if (fb == FightType.PuTong)
                {
                   m+= GongJi.GetIntOrDefault("V") - FangYu.GetIntOrDefault("V");
                }
                else if (fb == FightType.BaoJi)
                {
                    m += GongJi.GetIntOrDefault("V") * 2 - FangYu.GetIntOrDefault("V");
                }
            }
            else 
            {
                //魔法攻击
                Variant MoFaGongJi = m_root.Value.GetValueOrDefault<Variant>("MoFaGongJi");
                Variant MoFaFangYu = m_target.Value.GetValueOrDefault<Variant>("MoFaFangYu");
                if (fb == FightType.PuTong)
                {
                    m += MoFaGongJi.GetIntOrDefault("V") - MoFaFangYu.GetIntOrDefault("V");
                }
                else if (fb == FightType.BaoJi)
                {
                    m += MoFaFangYu.GetIntOrDefault("V") * 2 - MoFaFangYu.GetIntOrDefault("V");
                }
            }
            return m;
        }

        /// <summary>
        /// 检查暴击,2倍受害
        /// </summary>
        /// <param name="p">攻击者</param>
        /// <param name="target">接受攻击者</param>
        /// <returns></returns>
        protected bool CheckBaoJi(Variant p, Variant target)
        {
            return RandomHit(p.GetDoubleOrDefault("BaoJiLv") + 0.02 * (p.GetIntOrDefault("PetsLevel") - target.GetIntOrDefault("PetsLevel")));
        }

        /// <summary>
        /// 检查是否命中
        /// </summary>
        /// <param name="p">攻击者</param>
        /// <param name="target">接受攻击者</param>
        /// <returns></returns>
        protected bool CheckMingZhong(Variant p, Variant target)
        {
            return RandomHit(0.9 + (p.GetDoubleOrDefault("MingZhongLv") - target.GetDoubleOrDefault("ShanBiLv")) +
                             0.01 * (p.GetIntOrDefault("PetsLevel") - target.GetIntOrDefault("PetsLevel")));
        }

        /// <summary>
        /// 随机命中
        /// </summary>
        /// <param name="m">命中率</param>
        /// <returns></returns>
        protected bool RandomHit(double m)
        {
            return NumberRandom.NextDouble() < m;
        }

        /// <summary>
        /// 结算战绩值
        /// </summary>
        protected void GetFightValue() 
        {
            //得到被攻击者的当前成长度
            Variant target = m_target.Value;
            if (target == null) 
                return;

            Variant ccd = target.GetValueOrDefault<Variant>("ChengChangDu");
            if (ccd == null) 
                return;
            //胜利方得到战绩值
            int fightValue = ccd.GetIntOrDefault("V") * 1;//战绩基础值

            int tlevel = target.GetIntOrDefault("PetsLevel");


            Variant root = m_root.Value;
            if (root == null) 
                return;

            int rlevel = root.GetIntOrDefault("PetsLevel");
            int lp = rlevel-tlevel;//等级差
            if(lp>10 && lp<=15)
            {
                fightValue = Convert.ToInt32(fightValue * 0.8);
            }
            else if (lp > 15 && lp <= 20) 
            {
                fightValue = Convert.ToInt32(fightValue * 0.5);
            }
            else if(lp>20 &&lp<=25)
            {
                fightValue = Convert.ToInt32(fightValue * 0.2);
            }
            else if (lp > 25) 
            {
                fightValue = 0;
            }

            //被攻击者
            FrontServer.PlayerBusiness lossUser;
            Int64 tfv = 0;//被攻击者,当前战绩值
            if (m_model.Players.TryGetValue(m_target.PlayerID, out lossUser)) 
            {
                tfv = lossUser.FightValue;
            }

            //攻击者
            FrontServer.PlayerBusiness winUser;
            Int64 rfv = 0;//攻击者,当前战绩值
            if (m_model.Players.TryGetValue(m_root.PlayerID, out winUser))
            {
                rfv = winUser.FightValue;
            }

            Int64 fvp = rfv - tfv;//战绩差
            if (fvp > 500 && fvp <= 1000) 
            {
                fightValue = Convert.ToInt32(Math.Ceiling(fightValue * 0.8));
            }
            else if (fvp > 1000 && fvp <= 1500) 
            {
                fightValue = Convert.ToInt32(Math.Ceiling(fightValue * 0.5));
            }
            else if (fvp > 1500 && fvp <= 2000)
            {
                fightValue = Convert.ToInt32(Math.Ceiling(fightValue * 0.2));
            }
            else if (fvp > 2000) 
            {
                fightValue = 0;
            }
            
            //胜利方得到的战绩值数量
            //if (fightValue > 0) 
            //{
            //    winUser.FightValue+=fightValue;
            //    PlayerAccess.Instance.SaveValue(winUser.ID, new Tuple<string, BsonValue>("FightValue", winUser.FightValue));

            //    Variant v = new Variant(2);
            //    v["FightValue"] = winUser.FightValue;
            //    v["ID"] = winUser.ID;
            //    winUser.Call(ClientCommand.UpdateActorR, v);
            //}


            int fv = fightValue <= 0 ? 1 : fightValue;
            //失败方损失战绩值
            int f = Convert.ToInt32(Math.Ceiling(fv * 0.2));
            //if (f > 0)
            //{
            //    lossUser.FightValue -= f;
            //    ////PlayerAccess.Instance.SaveValue(lossUser.ID, new Tuple<string, BsonValue>("FightValue", lossUser.FightValue));
            //    Variant v = new Variant(2);
            //    v["FightValue"] = lossUser.FightValue;
            //    v["ID"] = lossUser.ID;
            //    winUser.Call(ClientCommand.UpdateActorR, v);
            //}



            //胜利方相关记录
            Settle winSettle;
            if (m_model.SettleInfo.TryGetValue(winUser.ID, out winSettle))
            {
                winSettle.WinFight += fightValue;
                winSettle.TotalWin++;
            }
            else
            {
                winSettle = new Settle();
                winSettle.PlayerID = winUser.ID;
                winSettle.GroupName = m_root.GroupName;
                winSettle.PlayerName = winUser.Name;
                winSettle.WinFight = fightValue;
                winSettle.TotalWin++;
                m_model.SettleInfo.TryAdd(winUser.ID, winSettle);
            }               

            //失败方记录
            Settle lossSettle;
            if (m_model.SettleInfo.TryGetValue(lossUser.ID, out lossSettle))
            {
                lossSettle.LossFight -= f;
                lossSettle.TotalLoss++;
            }
            else
            {
                lossSettle = new Settle();
                lossSettle.PlayerID = lossUser.ID;
                lossSettle.GroupName = m_target.GroupName;
                lossSettle.PlayerName = lossUser.Name;
                lossSettle.LossFight = -f;
                lossSettle.TotalLoss++;
                m_model.SettleInfo.TryAdd(lossUser.ID, lossSettle);
            } 
        } 
    }
}
