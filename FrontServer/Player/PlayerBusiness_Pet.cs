using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Util;
using Sinan.Log;

namespace Sinan.FrontServer
{
    partial class PlayerBusiness
    {
        static int maxLevel = ConfigLoader.Config.MaxLevel;

        public void ResetPet(Pet value, bool iscall = true)
        {
            m_pet = value;
            if (m_pet != null)
            {
                m_pet.Init();
            }
            if (iscall)
            {
                Variant v = new Variant(2);
                v["ID"] = this.ID;
                v["Pet"] = value;
                this.CallAll(ClientCommand.UpdateActorR, v);
            }
        }

        /// <summary>
        /// 得到溜宠信息
        /// </summary>
        /// <param name="value"></param>
        public void GetSlipPets(List<Variant> value)
        {
            m_slippets = value;
            Variant v = new Variant(2);
            v["ID"] = this.ID;
            v["SlipPets"] = value;
            this.CallAll(ClientCommand.UpdateActorR, v);
        }

        /// <summary>
        /// 换宠
        /// </summary>
        /// <param name="petid"></param>
        /// <returns></returns>
        public bool ChangePet(string petid,Mounts mounts)
        {
            Pet p = PetAccess.Instance.GetPetByID(petid, this.ID);
            if (p == null)
            {
                Call(PetsCommand.GuidePetsInfoR, false, TipManager.GetMessage(PetsReturn.ParaNameError));
                return false;
            }
            int petslevel = p.Value.GetIntOrDefault("PetsLevel");
            if (m_pet == null || p.ID != m_pet.ID)
            {
                if ((petslevel - m_level) > 5)
                {
                    Call(PetsCommand.GuidePetsInfoR, false, TipManager.GetMessage(PetsReturn.NoLevel));
                    return false;
                }
            }
            IList c = m_b3.Value.GetValue<IList>("C");
            Variant v0 = null;//是否存在出征宠
            Variant v1 = null;//要求出征的宠

            foreach (Variant v in c)
            {
                if (v.GetStringOrEmpty("E") == string.Empty)
                    continue;

                if (m_pet != null && v.GetStringOrDefault("E") == m_pet.ID)
                    v0 = v;
                if (v.GetStringOrDefault("E") == p.ID)
                    v1 = v;
                //如果两者不为空的时候退出
                if (v0 != null && v1 != null)
                    break;
            }
            if (v1 == null)
            {
                return false;
            }
            if (v0 != null && v0.GetStringOrDefault("E") == v1.GetStringOrDefault("E"))
            {
                v0["I"] = v0.GetIntOrDefault("I") == 1 ? 0 : 1;
                if (v0.GetIntOrDefault("I") == 0)
                {
                    PetAccess.PetReset(m_pet, null, false, null);
                    m_pet.Save();
                    Call(PetsCommand.UpdatePetR, true, m_pet);
                    Call(PetsCommand.GuidePetsInfoR, true, m_pet, 0);
                    ResetPet(null);
                }
            }
            else
            {
                if (v0 != null)
                {
                    PetAccess.PetReset(m_pet, null, false, null);
                    m_pet.Save();
                    Call(PetsCommand.UpdatePetR, true, m_pet);
                    v0["I"] = 0;
                }

                //坐骑加成
                PetAccess.PetReset(p, m_skill, false, mounts);
                v1["I"] = 1;
                ResetPet(p);
                Call(PetsCommand.GuidePetsInfoR, true, p, 1);
            }
            m_b3.Save();
            Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_b3));
            List<Variant> ps = PlayerExAccess.Instance.SlipPets(m_b3);

            if ((v0 != null && v0.GetIntOrDefault("P") > 3) || (v1 != null && v1.GetIntOrDefault("P") > 3))
            {
                GetSlipPets(ps);
            }
            return true;
        }

        /// <summary>
        /// 宠物取得经验
        /// </summary>
        /// <param name="pet"></param>
        /// <param name="experience"></param>
        /// <param name="isCall"></param>
        /// <param name="eType">宠物经验来源</param>
        /// <returns></returns>
        public bool AddPetExp(Pet pet, int experience, bool isCall, int eType)
        {
            if (pet == null)
            {
                return false;
            }
            Variant exp = pet.Value.GetVariantOrDefault("Experience");
            if (exp.GetIntOrDefault("V") == int.MaxValue)
            {
                return false;
            }
            int newExp = exp.SetOrInc("V", experience);
            if (newExp < 0)
            {
                newExp = int.MaxValue;
                exp["V"] = newExp;
            }

            int petLev = pet.Value.GetIntOrDefault("PetsLevel");
            if (petLev >= maxLevel)
            {
                pet.Save();
            }
            else
            {
                int max = exp.GetIntOrDefault("M");
                if (newExp >= max)
                {
                    petLev = UpPetLev(pet, newExp, max, petLev);
                    newExp = exp.GetIntOrDefault("V");
                }
                else
                {
                    pet.SaveLife();
                }
            }
            if (isCall)
            {
                Call(PetsCommand.UpdatePetR, true, pet);
            }

            //宠物取得经验
            PlayerLog log = new PlayerLog(ServerLogger.zoneid, Actiontype.PetExp);
            log.level = petLev;
            log.modifyexp = experience;
            log.totalexp = newExp;
            log.reserve_1 = eType;
            log.remark = pet.ID;
            log.touid = PID;
            this.WriteLog(log);
            return true;
        }

        /// <summary>
        /// 坐骑得到经验
        /// </summary>
        /// <param name="exp">得到经验</param>
        /// <param name="gs">经验来源</param>
        /// <returns></returns>
        public bool AddMounts(int exp,GoodsSource gs)
        {
            if (m_mounts == null)
                return false;

            GameConfig gc = GameConfigAccess.Instance.FindOneById(m_mounts.MountsID);
            if (gc == null)
            {
                return false;
            }
            Variant v = gc.Value;
            if (v == null)
            {
                return false;
            }

            Variant ex = v.GetVariantOrDefault("Exp");

            int max = 1;
            foreach (var item in ex) 
            {
                int m = Convert.ToInt32(item.Key);
                if (m > max) 
                {
                    max = m;
                }
            }
            if (m_mounts.Level >= max)
            {
                //已经升到最高级
                return false;
            }
           
            m_mounts.Experience += exp;

            int level = m_mounts.Level;

            bool isup = false;     
            while (true)
            {
                if (m_mounts.Level >= max)
                    break;
                int maxExp = ex.GetIntOrDefault(m_mounts.Level.ToString());
                if (m_mounts.Experience < maxExp)
                    break;
                m_mounts.Experience -= maxExp;
                m_mounts.Level++;
                m_mounts.Update = DateTime.UtcNow;
                isup = true;
            }

            if (m_mounts.Save())
            {
                //表示升级更新出战宠属性
                if (m_mounts.Status == 1 && isup)
                {
                    PetAccess.PetReset(m_pet, Skill, false, m_mounts);
                }
                MountsUpdate(m_mounts, new List<string>() { "Experience", "Level" });
            }
            //坐骑取得经验 日志          
            PlayerLog log = new PlayerLog(ServerLogger.zoneid, Actiontype.MountsExp);
            //升到的等级
            log.level = m_mounts.Level;
            log.modifyexp = exp;//得到坐骑经验
            log.reserve_1 = (int)gs;
            //升之前的等级
            log.reserve_2 = level;
            log.remark = m_mounts.ID;
            log.touid = PID;
            this.WriteLog(log);
            return true;
        }

        /// <summary>
        /// 更新坐骑
        /// </summary>
        /// <param name="update">更新内容</param>
        public void MountsUpdate(Mounts m, List<string> update)
        {
            m_mounts = m;
            MountsUpdate(update);
        }

        /// <summary>
        /// 更新坐骑
        /// </summary>
        /// <param name="update"></param>
        public void MountsUpdate(List<string> update) 
        {
            Variant v = new Variant();
            v.Add("Mounts", MountsAccess.Instance.MountsInfo(m_mounts, update));
            v.Add("ID", ID);
            Call(ClientCommand.UpdateActorR, v);            
        }


        /// <summary>
        /// 宠物升级
        /// </summary>
        /// <param name="pet"></param>
        /// <param name="newExp"></param>
        /// <param name="max"></param>
        /// <param name="petLev"></param>
        /// <returns></returns>
        private int UpPetLev(Pet pet, int newExp, int max, int petLev)
        {
            if (pet == null) 
                return petLev;
            Variant mv = pet.Value;
            if (mv == null) return petLev;
            string petsType = mv.GetStringOrDefault("PetsType");
            bool isPet = false;
            while (true)
            {
                petLev += 1;
                //得到可以领捂的技能
                
                pet.Value["PetsLevel"] = petLev;
                PetProperty life = PetAccess.GetSkillPro(pet, petLev);
                Variant ccd = mv.GetVariantOrDefault("ChengChangDu");
                life.ChengChangDu = ccd.GetIntOrDefault("V");

                int curExp = newExp - max;

                
                //表示出战宠升级
                if (Pet != null && Pet.ID == pet.ID)
                {
                    isPet = true;                    
                }
                
                PetAccess.RefreshPetProperty(pet.Value, life, curExp, true);
                LevelLogAccess.Instance.Insert(pet.ID, petLev, UserID);
                if (petLev < maxLevel)
                {
                    Variant exp = mv.GetVariantOrDefault("Experience");
                    max = exp.GetIntOrDefault("M");
                    if (curExp < max)
                    {
                        exp["V"] = curExp;
                        break;
                    }
                }
                else
                {
                    break;
                }
                
            }

            FinishNote(FinishCommand.PetUpLev, petLev);
            pet.Init();
            if (isPet)
            {
                //表示出战宠
                PetAccess.PetReset(pet, m_skill, false, m_mounts);
                Call(PetsCommand.UpdatePetR, true, pet);
            }
            else 
            {
                pet.Save();
            }
            return petLev;
        }

        /// <summary>
        /// 宠物领捂技能
        /// </summary>
        /// <param name="pet"></param>
        /// <param name="petLev"></param>
        /// <param name="petsType"></param>
        private void AddPetSkill(Pet pet, int petLev, string petsType)
        {
            string skillid = RoleManager.Instance.GetRoleConfig(petsType, "P" + petLev);
            if (string.IsNullOrEmpty(skillid))
                return;

            GameConfig gc = GameConfigAccess.Instance.FindOneById(skillid);
            if (gc == null)
                return;

            string roleLimit = gc.Value.GetStringOrDefault("RoleLimit");
            if (roleLimit == "0" || petsType == roleLimit)
            {
                Variant skill;
                if (pet.Value.TryGetValueT("Skill", out skill))
                {
                    if (skill == null)
                    {
                        skill = new Variant();
                        pet.Value["Skill"] = skill;
                    }
                }

                if (skill.ContainsKey(skillid))
                    return;

                //得到宠物技能槽
                IList skillList = pet.Value.GetValue<IList>("SkillGroove");
                Variant sk = null;
                bool noSkill = true;
                foreach (Variant k in skillList)
                {
                    if (sk == null && k.GetStringOrDefault("SkillID") == "0" && k.GetIntOrDefault("Born") == 0)
                        sk = k;
                    //已经存在改技能
                    if (k.GetStringOrDefault("SkillID") == skillid)
                        noSkill = false;
                }

                if (noSkill && sk != null)
                {
                    sk["SkillID"] = skillid;
                    sk["Level"] = 1;
                    sk["Born"] = 0;
                    sk["MaxUse"] = gc.UI.GetIntOrDefault("MaxUse");
                    sk["SkillName"] = gc.Name;

                    skill.Add(skillid, 1);
                    
                    //1表示宠物，技能ID，技能ID,表示宠物领悟新的技能
                    Call(ClientCommand.GetNewSkillR, 1, skillid, petLev);
                }
            }
        }
    }
}
