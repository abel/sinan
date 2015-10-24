using System;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Driver;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.PetsModule.Business;
using Sinan.Util;
using Sinan.Log;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.PetsModule
{
    sealed public partial class PetMediator : AysnSubscriber
    {
        static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();

        public override IList<string> Topics()
        {
            return new string[]
            {
                UserPlayerCommand.CreatePlayerSuccess,
                LoginCommand.PlayerLoginSuccess,
                PetsCommand.CreatePets,

                PetsCommand.GetPetsInfo,
                PetsCommand.GuidePetsInfo,
                PetsCommand.ZiZhiPets,
                PetsCommand.ChangePetsName,
                PetsCommand.FeedPets,
                PetsCommand.UpPetsRank,
                PetsCommand.PetRelease,
                PetsCommand.ShockPetGroove,
                PetsCommand.PetBurdenDrag,
                PetsCommand.Stocking,
                PetsCommand.GetPetsList,
                PetsCommand.StockingAward,
                PetsCommand.PetExtend,

                PetsCommand.DrawPetSkill,
                PetsCommand.RemoveSkill,
                PetsCommand.OblivionSkill,
                PetsCommand.ShockSkill,
                PetsCommand.AddSkill,
                PetsCommand.AddKey,

                PetsCommand.StealPet,
                PetsCommand.PetProtection,
                PetsCommand.StockingAll,
                PetsCommand.PetProperty,
                PetsCommand.PetAbsorb,
                PetsCommand.PetNurse,
                PetsCommand.PetPresent,
                PetsCommand.FeedPetsAll,

                PetsCommand.EvoSkillChange,
                PetsCommand.EvoSkillUp
            };
        }

        public override void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;
            if (note != null)
            {
                this.ExecuteNote(note);
            }
        }

        void ExecuteNote(UserNote note)
        {
            // 需验证玩家登录的操作.....
            if (note.Name == UserPlayerCommand.CreatePlayerSuccess)
            {
                //创建角色时得到宠物
                CreatePets(note);
                return;
            }
            if (note.Player == null)
                return;
            //表示角色正在战斗中，客户端提交信息不进行处理
            if (note.Player.AState == ActionState.Fight)
                return;

            switch (note.Name)
            {
                case PetsCommand.CreatePets:
                    break;
                case PetsCommand.GuidePetsInfo:
                    GuidePetsInfo(note);
                    break;
                case LoginCommand.PlayerLoginSuccess:
                    GuidePetsInfo(note);
                    break;
                case PetsCommand.ZiZhiPets:
                    ZiZhiPets(note);
                    break;
                case PetsCommand.ChangePetsName:
                    ChangePetsName(note);
                    break;
                case PetsCommand.FeedPets:
                    FeedPets(note);
                    break;
                case PetsCommand.GetPetsInfo:
                    GetPetsInfo(note);
                    break;
                case PetsCommand.UpPetsRank:
                    UpPetsRank(note);
                    break;

                case PetsCommand.DrawPetSkill:
                    PetBusiness.DrawPetSkill(note);
                    break;
                case PetsCommand.RemoveSkill:
                    PetBusiness.RemoveSkill(note);
                    break;
                case PetsCommand.OblivionSkill:
                    PetBusiness.OblivionSkill(note);
                    break;
                case PetsCommand.ShockSkill:
                    PetBusiness.ShockSkill(note);
                    break;
                case PetsCommand.AddSkill:
                    PetBusiness.AddSkill(note);
                    break;
                case PetsCommand.PetRelease:
                    PetRelease(note);
                    break;
                case PetsCommand.AddKey:
                    PetBusiness.AddKey(note);
                    break;
                case PetsCommand.ShockPetGroove:
                    ShockPetGroove(note);
                    break;

                case PetsCommand.PetBurdenDrag:
                    PetBurdenDrag(note);
                    break;

                case PetsCommand.Stocking:
                    Stocking(note);
                    break;
                case PetsCommand.GetPetsList:
                    GetPetsList(note);
                    break;
                case PetsCommand.StockingAward:
                    StockingAward(note);
                    break;
                case PetsCommand.PetExtend:
                    PetExtend(note);
                    break;
                case PetsCommand.StealPet:
                    PetBusiness.StealPet(note);
                    break;
                case PetsCommand.PetProtection:
                    PetBusiness.PetProtection(note);
                    break;
                case PetsCommand.StockingAll:
                    PetBusiness.StockingAll(note);
                    break;
                case PetsCommand.PetProperty:
                    PetBusiness.PetProperty(note);
                    break;
                case PetsCommand.PetAbsorb:
                    Absorb(note);
                    break;
                case PetsCommand.PetNurse:
                    PetNurse(note);
                    break;
                case PetsCommand.PetPresent:
                    PetBusiness.PetPresent(note);
                    break;
                case PetsCommand.FeedPetsAll:
                    PetBusiness.FeedPetsAll(note);
                    break;

                case PetsCommand.EvoSkillChange:
                    PetBusiness.EvoSkillChange(note);
                    break;

                case PetsCommand.EvoSkillUp:
                    PetBusiness.EvoSkillUp(note);
                    break;
            }
        }



        /// <summary>
        /// 创建宠物槽
        /// </summary>
        /// <param name="note"></param>
        private void CreatePetGroove(UserNote note)
        {
            PlayerEx PetGroove = note.Player.B3;
            if (PetGroove == null)
            {
                PlayerEx p = new PlayerEx(note.PlayerID, "B3");
                p.Value = new Variant();
                Variant v = p.Value;
                for (int i = 0; i < 8; i++)
                {
                    if (!v.ContainsKey(i.ToString()))
                    {
                        Variant d = new Variant();
                        if (i < 4)
                            d.Add("E", "0");//表示已经开启
                        else
                            d.Add("E", "-1");//表示没有开启
                        d.Add("G", string.Empty);
                        d.Add("I", 0);//0表示没有参加战斗，1表示参加战斗
                        v.Add(i.ToString(), d);
                    }
                }
                p.Save();
            }
        }

        /// <summary>
        /// 创建宠物
        /// </summary>
        private void CreatePets(UserNote note)
        {
            PlayerBusiness player = note[0] as PlayerBusiness;

            PlayerEx pv = player.B3;
            string petid = RoleManager.Instance.GetRoleConfig(player.RoleID, "newPet");
            if (PetAccess.Instance.CreatePet(pv, petid, 0, 1) == 0)
            {
                IList c = pv.Value.GetValue<IList>("C");
                Variant tmp = null;
                foreach (Variant v in c)
                {
                    if (v.GetStringOrDefault("E") != "-1" && v.GetStringOrDefault("E") != "0")
                    {
                        tmp = v;
                        break;
                    }
                }
                Pet p = PetAccess.Instance.FindOneById(tmp.GetStringOrDefault("E"));
                if (p != null) tmp["I"] = 1;
                pv.Save();
                player.ResetPet(p, false);
            }

        }

        /// <summary>
        /// 得到宠物的基本信息
        /// </summary>
        /// <param name="note"></param>
        private void GetPetsInfo(UserNote note)
        {
            string playerPetsID = note.GetString(0);
            Pet p = PetAccess.Instance.FindOneById(playerPetsID);
            if (p != null)
            {
                note.Call(PetsCommand.GetPetsInfoR, true, p);
                return;
            }

            GameConfig gc = GameConfigAccess.Instance.FindOneById(playerPetsID);
            if (gc != null)
            {
                note.Call(PetsCommand.GetPetsInfoR, true, gc);
                return;
            }
            note.Call(PetsCommand.GetPetsInfoR, false, TipManager.GetMessage(PetsReturn.ParaNameError));
        }

        /// <summary>
        /// 得到带领宠物基本信息
        /// </summary>
        /// <param name="note"></param>
        private void GuidePetsInfo(UserNote note)
        {
            //得到装备面板信息
            PlayerEx gv = note.Player.B3;
            PlayerEx family = note.Player.Family;
            string familyID = family.Value.GetStringOrDefault("FamilyID");
            IList c = gv.Value.GetValue<IList>("C");

            if (note.Name == LoginCommand.PlayerLoginSuccess)
            {
                if (note.Player.Pet != null)
                {
                    note.Player.Call(PetsCommand.GuidePetsInfoR, true, note.Player.Pet, 1);
                }
                return;
            }
            if (note.Name == PetsCommand.GuidePetsInfo)
            {
                //得到宠物ID,要求带领宠物     
                note.Player.ChangePet(note.GetString(0), note.Player.Mounts);
                return;
            }
            note.Call(PetsCommand.GuidePetsInfoR, false, TipManager.GetMessage(PetsReturn.ParaNameError));
        }

        /// <summary>
        /// 喂养宠物
        /// </summary>
        /// <param name="note"></param>
        private void FeedPets(UserNote note)
        {
            string soleid = note.PlayerID + "FeedPets";
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                string playerpetid = note.GetString(0);
                Pet p = null;
                if (note.Player.Pet != null)
                {
                    if (note.Player.Pet.ID == playerpetid)
                    {
                        p = note.Player.Pet;
                    }
                }
                if (p == null)
                {
                    p = PetAccess.Instance.FindOneById(playerpetid);
                }

                if (p == null)
                {
                    note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.NoPets));
                    return;
                }
                Variant v = p.Value;
                Variant ccd = v.GetVariantOrDefault("ChengChangDu");
                int number = PetAccess.KeepNeedGoods(ccd.GetIntOrDefault("V") + 1);
                string goodsid = "G_d000015";//成长果实
                PlayerEx burden = note.Player.B0;
                IList c = burden.Value.GetValue<IList>("C");
                //得到某物品的数量
                int count = BurdenManager.BurdenGoodsCount(c, goodsid);
                if (number > count)
                {
                    note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.NoKeepGoods));
                    return;
                }

                if (ccd.GetIntOrDefault("V") >= ccd.GetIntOrDefault("M"))
                {
                    note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.KeepBig));
                    return;
                }

                if (note.Player.RemoveGoods(goodsid, number, GoodsSource.FeedPets))
                {
                    ccd["V"] = ccd.GetIntOrDefault("V") + 1;
                    //喂养成长果实数量
                    v.SetOrInc("FeedCount", number);

                    PetAccess.PetReset(p, note.Player.Skill, false,note.Player.Mounts);
                    burden.Save();

                    note.Player.UpdateBurden();

                    note.Call(PetsCommand.UpdatePetR, true, p);
                    note.Call(PetsCommand.FeedPetsR, true, 0);
                }
                else
                {
                    note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.NoKeepGoods));
                }
            }
            finally 
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 宠物资质提升
        /// </summary>
        /// <param name="not"></param>
        private void ZiZhiPets(UserNote note)
        {
            string playerpetid = note.GetString(0);
            Pet p = null;
            if (note.Player.Pet != null)
            {
                if (note.Player.Pet.ID == playerpetid)
                {
                    p = note.Player.Pet;
                }
            }
            if (p == null)
            {
                p = PetAccess.Instance.FindOneById(playerpetid);
            }

            if (p == null)
            {
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.NoPets));
                return;
            }
            //提升的资质名称

            Variant pv = p.Value;
            if (pv == null)
            {
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.NoPets));
                return;
            }

            int m = pv.GetIntOrDefault("ZiZhi");
            if (m >= PetAccess.Instance.PetMaxZiZhi())
            {
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.ZiZhiMax));
                return;
            }
            //当前宠物等级
            int pl = pv.GetIntOrDefault("PetsLevel");
            Variant ccd = pv["ChengChangDu"] as Variant;
            //资质提升要求

            int level = 0;
            int max = PetAccess.Instance.PetZiZhi(m, out level);
            if (level > pl)
            {
                note.Call(PetsCommand.UpdatePetR, false, string.Format(TipManager.GetMessage(ExtendReturn.ZiZhi3),level));
                //"当前宠物等级不足必须达到【" + level + "】级才能提升");
                return;
            }

            if (ccd.GetIntOrDefault("V") < max)
            {
                note.Call(PetsCommand.UpdatePetR, false, string.Format(TipManager.GetMessage(ExtendReturn.ZiZhi4), max));
                //"宠物成长度必须达到【" + max + "】才能提升");
                return;
            }


            string goodsid = "";            
            int number = 0;
            double lv = GameConfigAccess.Instance.FindExtend(ExtendType.ZiZhi, m.ToString(), out number, out goodsid);
            Variant mv = MemberAccess.MemberInfo(note.Player.MLevel);
            if (mv != null)
            {
                lv *= (1 + mv.GetDoubleOrDefault("ZiZhiLv"));
            }

            if (string.IsNullOrEmpty(goodsid) || number <= 0)
            {
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(ExtendReturn.ZiZhi2));
                return;
            }
            Variant us = new Variant();
            if (note.Player.RemoveGoods(goodsid, number, GoodsSource.ZiZhiPets))
            {
                us[goodsid] = number;

                note.Player.UpdateBurden();
                                    
                //祝福值增加成功率        
                int zf = 0;
                DateTime dt = DateTime.UtcNow;
                DateTime ft = pv.GetDateTimeOrDefault("FailTime");
                Variant zhufu = ZhuFuAccess.ZhuFuInfo("Pets", m);
                if (ft.ToLocalTime().Date == dt.ToLocalTime().Date &&
                    zhufu != null)
                {                    
                    Variant fc = zhufu.GetVariantOrDefault("FailCount");
                    double addlv = 0;
                    if (fc != null)
                    {
                        foreach (var item in fc)
                        {
                            if (pv.GetIntOrDefault("FailCount") >= Convert.ToInt32(item.Key) && addlv < Convert.ToDouble(item.Value))
                            {
                                addlv = Convert.ToDouble(item.Value);
                            }
                        }
                    }
                    lv += addlv;
                }
                else
                {
                    pv["FailCount"] = 0;
                    pv["ZhuFu"] = 0;
                }
                if (zhufu != null)
                {
                    //计算祝福值
                    string[] strs = zhufu.GetStringOrDefault("Rand").Split('-');
                    int min = Convert.ToInt32(strs[0]);
                    int max1 = Convert.ToInt32(strs[1]) + 1;
                    zf = NumberRandom.Next(min, max1);
                }

                bool isSuccess = NumberRandom.RandomHit(lv);
                if (isSuccess)
                {
                    pv["ZiZhi"] = m + 1;
                    pv["FailCount"] = 0;
                    pv["FailTime"] = dt;
                    pv["ZhuFu"] = 0;
                    //更新最大成长度
                    ccd["M"] = PetAccess.Instance.PetZiZhi(m + 1, out level);
                    p.Save();
                    note.Player.FinishNote(FinishCommand.PetZhiZi, pv.GetIntOrDefault("ZiZhi"));
                    note.Call(PetsCommand.UpdatePetR, true, p);
                    note.Call(PetsCommand.ZiZhiPetsR, true, TipManager.GetMessage(PetsReturn.ZiZhiPets1));
                }
                else
                {
                    pv.SetOrInc("FailCount", 1);
                    pv["FailTime"] = dt;
                    pv["ZhuFu"] = pv.GetIntOrDefault("ZhuFu") + zf;
                    p.Save();

                    note.Call(PetsCommand.UpdatePetR, true, p);
                    note.Call(PetsCommand.ZiZhiPetsR, false, TipManager.GetMessage(PetsReturn.ZhiZiFail));
                }

                Variant os = new Variant();
                os.Add("ID",p.ID);
                os.Add("PetsID", p.Value.GetStringOrDefault("PetsID"));
                os.Add("ZiZhi", m);                
                os.Add("Lv", lv);//成功率
                os.Add("IsSuccess", isSuccess);
                note.Player.AddLogVariant(Actiontype.ZiZhi, us, null, os);
            }
            else
            {
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(ExtendReturn.ZiZhi2));
            }
        }

        /// <summary>
        /// 修改宠物名字
        /// </summary>
        /// <param name="note"></param>
        private void ChangePetsName(UserNote note)
        {
            string playerpetid = note.GetString(0);
            string name = note.GetString(1).Trim();
            if (name.Length > 7)
            {
                note.Call(PetsCommand.ChangePetsNameR, false, TipManager.GetMessage(PetsReturn.PetNameLenght), 0);
                return;
            }
            bool IsOnline = false;
            Pet p = null;
            if (note.Player.Pet != null)
            {
                if (note.Player.Pet.ID == playerpetid)
                {
                    p = note.Player.Pet;
                    IsOnline = true;
                }
            }
            if (p == null)
            {
                p = PetAccess.Instance.FindOneById(playerpetid);
            }

            if (p == null)
            {
                note.Call(PetsCommand.ChangePetsNameR, false, TipManager.GetMessage(PetsReturn.NoPets), 0);
                return;
            }

            string msg = NameManager.Instance.CheckName(name);
            if (!string.IsNullOrEmpty(msg))
            {
                note.Call(PetsCommand.ChangePetsNameR, false, msg, 0);
                return;
            }

            //冷冻结时间
            p.Modified = DateTime.UtcNow;
            if (string.IsNullOrEmpty(name))
            {
                GameConfig gc = GameConfigAccess.Instance.FindOneById(p.Value.GetStringOrDefault("PetsID"));
                if (gc == null)
                {
                    note.Call(PetsCommand.ChangePetsNameR, false, TipManager.GetMessage(PetsReturn.PetNameError), 0);
                    return;
                }
                name = gc.Name;
            }
            //更名
            p.Name = name;
            p.Save();
            note.Call(PetsCommand.ChangePetsNameR, true, name, playerpetid);
            if (IsOnline)
            {
                Variant vv = new Variant();
                vv.Add("ID", p.ID);
                vv.Add("Skill", p.Value.GetValueOrDefault<Variant>("Skill"));
                vv.Add("PetsID", p.Value["PetsID"]);
                vv.Add("PetsRank", p.Value["PetsRank"]);
                vv.Add("Skin", p.Value["Skin"]);
                vv.Add("Name", p.Name);

                Variant tmp = new Variant();
                tmp.Add("Pet", vv);
                tmp.Add("ID", note.PlayerID);
                note.Player.CallAll(ClientCommand.UpdateActorR, tmp);
            }
        }

        /// <summary>
        /// 进化
        /// </summary>
        /// <param name="note"></param>
        private void UpPetsRank(UserNote note)
        {
            string playerpetid = note.GetString(0);
            Pet p = null;
            if (note.Player.Pet != null)
            {
                if (note.Player.Pet.ID == playerpetid)
                {
                    p = note.Player.Pet;
                }
            }
            if (p == null)
            {
                p = PetAccess.Instance.FindOneById(playerpetid);
            }

            if (p == null)
            {
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.NoPets));
                return;
            }
            //宠物类型
            string roleID = p.Value["PetsType"].ToString();
            //当前阶级
            int petsRank = Convert.ToInt32(p.Value["PetsRank"]);



            GameConfig gc = GameConfigAccess.Instance.FindOneById(p.Value["PetsID"].ToString());
            if (gc == null)
            {
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.NoPets));
                return;
            }

            int isRank = Convert.ToInt32(gc.Value["IsRank"]);
            if (isRank == 0 || (isRank == 1 && petsRank > 0) || (isRank == 2 && petsRank > 1))
            {
                //不能进阶
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.NoRank));
                return;
            }

            Variant rank = null;
            if (petsRank == 0)
            {
                rank = gc.Value["R1"] as Variant;
            }
            else if (petsRank == 1)
            {
                rank = gc.Value["R2"] as Variant;
            }
            else
            {
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.RankMax));
                return;
            }

            //宠物技能槽
            IList skillList = p.Value["SkillGroove"] as IList;
            Variant sk = null;
            foreach (Variant k in skillList)
            {
                if (petsRank == 0 && k.GetIntOrDefault("P") == 10)
                {
                    sk = k;
                    break;
                }
                if (petsRank == 1 && k.GetIntOrDefault("P") == 11)
                {
                    sk = k;
                    break;
                }
            }
            if (sk == null)
            {
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.NoSkillList));
                return;
            }



            IList rankSkill = rank.GetValueOrDefault<IList>("RankSkill");//] as IList;
            int m = NumberRandom.Next(1, 101);
            Variant selectSkill = null;
            if (rankSkill != null)
            {
                foreach (Variant sv in rankSkill)
                {
                    if (m >= sv.GetIntOrDefault("RateMix") && m <= sv.GetIntOrDefault("RateMax"))
                    {
                        selectSkill = sv;
                        break;
                    }
                }
            }

            if (selectSkill == null)
            {
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.RateConfigError));
                return;
            }
            Variant v = p.Value;
            //新增技能
            string newSkill = selectSkill.GetStringOrDefault("SkillID");
            //技能等级
            int skillLevel = selectSkill.GetIntOrDefault("Level");
            GameConfig skillConfig = GameConfigAccess.Instance.FindOneById(newSkill);
            if (skillConfig == null)
            {
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.NoPetSkill));
                return;
            }
            Variant skill = v["Skill"] as Variant;

            if (rank.GetIntOrDefault("Level") > p.Value.GetIntOrDefault("PetsLevel"))
            {
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.UpPetsRank));
                return;
            }

            int chengChangDu = p.Value.GetVariantOrDefault("ChengChangDu").GetIntOrDefault("V");


            if (chengChangDu < rank.GetIntOrDefault("Total"))
            {
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.UpPetsRank));
                return;
            }

            if (skill.ContainsKey(newSkill))
            {
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.PetSkill));
                return;
            }

            PlayerEx burden = note.Player.B3;
            IList c = burden.Value.GetValue<IList>("C");
            Variant rv = null;
            foreach (Variant k in c)
            {
                if (k.GetStringOrDefault("E") == p.ID)
                {
                    rv = k;
                    break;
                }
            }

            if (rv == null)
            {
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.NoPets));
                return;
            }


            sk["SkillID"] = newSkill;
            sk["Level"] = skillLevel;
            sk["Born"] = 2;
            sk["MaxUse"] = skillConfig.UI.GetIntOrDefault("MaxUse");

            if (sk.ContainsKey("SkillName"))
            {
                sk["SkillName"] = skillConfig.Name;
            }
            else
            {
                sk.Add("SkillName", skillConfig.Name);
            }
            p.Value["PetsRank"] = petsRank + 1;
            skill.Add(newSkill, skillLevel);

            rv["R"] = petsRank + 1;
            burden.Save();
            // 新添加技能

            PetAccess.PetReset(p, note.Player.Skill, false,note.Player.Mounts);

            note.Player.FinishNote(FinishCommand.PetJieJi, p.Value["PetsRank"]);
            note.Call(PetsCommand.UpdatePetR, true, p);
            Variant list = new Variant();
            list.Add("B3", burden);
            note.Call(BurdenCommand.BurdenListR, list);
            note.Call(PetsCommand.UpPetsRankR, true);
        }

        /// <summary>
        /// 宠物放生
        /// </summary>
        /// <param name="note"></param>
        private void PetRelease(UserNote note)
        {
            string id = note.GetString(0);
            string b = note.GetString(1);
            Pet p = PetAccess.Instance.FindOneById(id);
            if (p == null)
            {
                note.Call(PetsCommand.PetReleaseR, false, TipManager.GetMessage(PetsReturn.NoExists));
                return;
            }
            if (p.PlayerID != note.PlayerID)
            {
                note.Call(PetsCommand.PetReleaseR, false, TipManager.GetMessage(PetsReturn.NoExists));
                return;
            }    
            
            string petsid = p.Value.GetStringOrDefault("PetsID");
            if (b != "B2" && b != "B3")
            {
                if (b != "Home")
                    return;

                PlayerEx home = note.Player.Home;
                Variant pk = home.Value.GetValueOrDefault<Variant>("PetKeep");
                if (pk == null)
                {
                    note.Call(PetsCommand.PetReleaseR, false, TipManager.GetMessage(PetsReturn.NoExists));
                    return;
                }
                string pid = pk.GetStringOrDefault("ID");
                if (id != pid)
                {
                    note.Call(PetsCommand.PetReleaseR, false, TipManager.GetMessage(PetsReturn.NoExists));
                    return;
                }
                pk["ID"] = string.Empty;
                pk["PetID"] = string.Empty;
                pk["StartTime"] = string.Empty;
                pk["EndTime"] = string.Empty;
                pk["PetsWild"] = 0;
                pk["PetName"] = string.Empty;
                pk["PetsRank"] = 0;
                home.Save();
                note.Call(PetsCommand.PetReleaseR, true, b);
                note.Player.FinishNote(FinishCommand.PetOut);                             
            }
            else
            {
                PlayerEx pg = b == "B3" ? note.Player.B3 : note.Player.B2;

                IList c = pg.Value.GetValue<IList>("C");
                Variant v = null;
                foreach (Variant k in c)
                {
                    if (k.GetStringOrDefault("E") == id)
                    {
                        v = k;
                        break;
                    }
                }

                if (v == null)
                {
                    note.Call(PetsCommand.PetReleaseR, false, TipManager.GetMessage(PetsReturn.NoExists));
                    return;
                }



                if (b == "B3" && v.GetIntOrDefault("P") > 3)
                {
                    note.Call(PetsCommand.PetReleaseR, false, TipManager.GetMessage(PetsReturn.IsLiu));
                    return;
                }

                if (v.GetIntOrDefault("I") == 1)
                {
                    note.Call(PetsCommand.PetReleaseR, false, TipManager.GetMessage(PetsReturn.NoRelease));
                    return;
                }

                //应该移除宠物
                BurdenManager.BurdenClear(v);
                pg.Save();
                note.Player.FinishNote(FinishCommand.PetOut);
                //删除宠物            
                note.Call(PetsCommand.PetReleaseR, true, id);
                note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(pg));                
            }
            int level = p.Value.GetIntOrDefault("PetsLevel");
            PetAccess.Instance.RemoveOneById(p.ID, SafeMode.False);
            if (!string.IsNullOrEmpty(petsid))
            {
                int pl = 0;
                switch (b)
                {
                    case "Hame":
                        pl = 1;
                        break;
                    case "B2":
                        pl = 2;
                        break;
                    case "B3":
                        pl = 3;
                        break;
                }
                PetBusiness.PetExp(note.Player, p.ID, petsid, FinanceType.PetRelease, GoodsSource.PetRelease, pl, level);
            }


            //放生日志
            Variant os = new Variant();
            os["P"] = b;
            os["ID"] = p.ID;
            os["PetsID"] = petsid;
            os["PetsLevel"] = p.Value.GetIntOrDefault("PetsLevel");
            os["SceneID"] = note.Player.SceneID;
            os["ZiZhi"] = p.Value.GetIntOrDefault("ZiZhi");
            os["ChengChangDu"] = p.Value.GetVariantOrDefault("ChengChangDu").GetIntOrDefault("V");            
            note.Player.AddLogVariant(Actiontype.PetRelease, null, null, os);
        }

        /// <summary>
        /// 激活兽栏
        /// </summary>
        /// <param name="note"></param>
        private void ShockPetGroove(UserNote note)
        {
            //得到道具数量
            int number = note.GetInt32(0);
            PlayerEx pg = note.Player.B3;
            Variant v = null;
            IList pc = pg.Value.GetValue<IList>("C");
            int n = 0;
            foreach (Variant k in pc)
            {
                if (k.GetStringOrDefault("E") == "-1")
                {
                    n++;
                    if (v == null)
                    {
                        v = k;
                    }
                    else if (v.GetIntOrDefault("P") > k.GetIntOrDefault("P"))
                    {
                        v = k;
                    }
                }
            }

            if (v == null)
            {
                note.Call(PetsCommand.ShockPetGrooveR, false, TipManager.GetMessage(PetsReturn.ShockFinish));
                return;
            }
            int p = v.GetIntOrDefault("P");
            string goodsid = "";
            number=0;
            double lv = GameConfigAccess.Instance.FindExtend(ExtendType.B3, p.ToString(),out number,  out goodsid);
            if (string.IsNullOrEmpty(goodsid) || number <= 0)
            {
                note.Call(PetsCommand.ShockPetGrooveR, false, TipManager.GetMessage(ExtendReturn.B32));
                return;
            }

            if (note.Player.RemoveGoods(goodsid, number, GoodsSource.ShockPetGroove))
            {
                note.Player.UpdateBurden();
                if (NumberRandom.RandomHit(lv))
                {
                    v["E"] = "";
                    if (pg.Save())
                    {
                        note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(pg));
                        note.Call(PetsCommand.ShockPetGrooveR, true, string.Empty);
                        note.Player.FinishNote(FinishCommand.ExtendStables);
                    }
                }
                else
                {
                    note.Call(PetsCommand.ShockPetGrooveR, false, TipManager.GetMessage(ExtendReturn.B33));
                }                
            }
            else 
            {
                note.Call(PetsCommand.ShockPetGrooveR, false, TipManager.GetMessage(ExtendReturn.B32));                
            }
        }



        /// <summary>
        /// 宠物仓库拖动操作
        /// </summary>
        /// <param name="note"></param>
        private void PetBurdenDrag(UserNote note)
        {
            string b0 = note.GetString(0);
            int p0 = note.GetInt32(1);

            string b1 = note.GetString(2);
            int p1 = note.GetInt32(3);

            PlayerEx burden0 = note.Player.Value[b0] as PlayerEx;
            PlayerEx burden1 = note.Player.Value[b1] as PlayerEx;

            IList c0 = burden0.Value.GetValue<IList>("C");
            IList c1 = burden1.Value.GetValue<IList>("C");

            Variant v0 = null;
            Variant v1 = null;
            foreach (Variant k in c0)
            {
                if (k.GetIntOrDefault("P") == p0)
                {
                    v0 = k;
                    break;
                }
            }
            foreach (Variant k in c1)
            {
                if (k.GetIntOrDefault("P") == p1)
                {
                    v1 = k;
                    break;
                }
            }

            if (v0 == null || v0.GetIntOrDefault("I") == 1)
            {
                note.Call(PetsCommand.PetBurdenDragR, false, TipManager.GetMessage(PetsReturn.NoRelease));
                return;
            }
            if (v0.GetStringOrDefault("E") == "-1")
            {
                note.Call(PetsCommand.PetBurdenDragR, false, TipManager.GetMessage(PetsReturn.NoShock));
                return;
            }

            if (v1 == null || v1.GetIntOrDefault("I") == 1)
            {
                note.Call(PetsCommand.PetBurdenDragR, false, TipManager.GetMessage(PetsReturn.NoRelease));
                return;
            }

            if (v1.GetStringOrDefault("E") == "-1")
            {
                note.Call(PetsCommand.PetBurdenDragR, false, TipManager.GetMessage(PetsReturn.NoShock));
                return;
            }

            if (Swap(v0, v1))
            {
                List<Variant> ps = PlayerExAccess.Instance.SlipPets(note.Player.B3);
                note.Player.GetSlipPets(ps);
            }
            burden0.Save();
            burden1.Save();
            Variant list = new Variant();
            list.Add(b0, burden0);
            if (b0 != b1)
            {
                list.Add(b1, burden1);
            }
            note.Call(BurdenCommand.BurdenListR, list);
        }

        /// <summary>
        /// 交换信息
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        private bool Swap(Variant v0, Variant v1)
        {
            bool isin = false;
            Variant v = new Variant();
            if (v0.GetIntOrDefault("P") > 3 || v1.GetIntOrDefault("P") > 3)
                isin = true;
            foreach (string k in v0.Keys)
            {
                v.Add(k, v0[k]);
            }

            v0["E"] = v1["E"];
            v0["G"] = v1["G"];
            v0["S"] = v1["S"];
            v0["H"] = v1["H"];
            v0["A"] = v1["A"];
            v0["D"] = v1["D"];
            v0["R"] = v1["R"];
            v0["T"] = v1["T"];

            v1["E"] = v["E"];
            v1["G"] = v["G"];
            v1["S"] = v["S"];
            v1["H"] = v["H"];
            v1["A"] = v["A"];
            v1["D"] = v["D"];
            v1["R"] = v["R"];
            v1["T"] = v["T"];
            return isin;
        }


        /// <summary>
        /// 放养
        /// </summary>
        /// <param name="note"></param>
        public void Stocking(UserNote note)
        {
            //宠物
            string id = note.GetString(0);
            bool isStocking = note.GetBoolean(1);
            if (note.Player.SceneID != SceneHome.DefaultID)
            {
                //不能在【{0}】场景{1}
                string str = string.Format(TipManager.GetMessage(PetsReturn.NoSceneID), note.Player.Scene.Name);
                note.Call(PetsCommand.StockingR, false, isStocking, str);
                return;
            }

            #region 家园驯化完成的宠物直接放养
            if (note.GetInt32(2) == 1)
            {
                PlayerEx home = note.Player.Home;
                Variant pk = home.Value["PetKeep"] as Variant;
                if (pk.GetIntOrDefault("PetsWild") != -1)
                {
                    note.Call(PetsCommand.StockingR, false, isStocking, TipManager.GetMessage(PetsReturn.NoExists));
                    //还不能放养
                    return;
                }
                if (pk.GetStringOrDefault("ID") != id)
                {
                    note.Call(PetsCommand.StockingR, false, isStocking, TipManager.GetMessage(PetsReturn.NoExists));
                    return;
                }
                Pet px = PetAccess.Instance.FindOneById(id);
                PlayerEx b2 = note.Player.B2;
                IList c2 = b2.Value.GetValue<IList>("C");
                Variant v2 = null;
                foreach (Variant v in c2)
                {
                    if (v.GetStringOrDefault("E") == string.Empty)
                    {
                        v2 = v;
                        break;
                    }
                }
                if (v2 == null)
                {
                    note.Call(PetsCommand.StockingR, false, isStocking, TipManager.GetMessage(PetsReturn.PetBurdenB2));
                    return;
                }

                v2["E"] = px.ID;
                v2["G"] = px.Value.GetStringOrDefault("PetsID");
                v2["S"] = px.Value.GetIntOrDefault("Sort");
                v2["H"] = px.Value.GetIntOrDefault("IsBinding");
                v2["A"] = 1;
                v2["D"] = 0;
                v2["R"] = px.Value.GetIntOrDefault("PetsRank");
                v2["I"] = 0;

                pk["ID"] = string.Empty;
                pk["PetID"] = string.Empty;
                pk["StartTime"] = string.Empty;
                pk["EndTime"] = string.Empty;
                pk["PetsWild"] = 0;
                pk["PetName"] = string.Empty;
                pk["PetsRank"] = 0;
                v2["T"] = PetAccess.Instance.CreateAward(note.Player.Level, id, note.PlayerID, note.Player.Pet);

                home.Save();
                b2.Save();

                note.Call(PetsCommand.StockingR, true, isStocking, PetAccess.Instance.GetPetModel(v2));
                Variant mn = new Variant();
                mn.Add("B2", note.Player.B2);
                note.Call(BurdenCommand.BurdenListR, mn);
                return;
            }
            #endregion

            #region 家园与宠物背包宠物交换
            //宠物所在位置
            PlayerEx burden0 = isStocking ? note.Player.B3 : note.Player.B2;

            //放入位置
            PlayerEx burden1 = isStocking ? note.Player.B2 : note.Player.B3;
            IList c0 = burden0.Value.GetValue<IList>("C");
            Variant v0 = null;
            foreach (Variant v in c0)
            {
                if (v.GetStringOrDefault("E") == id)
                {
                    v0 = v;
                    break;
                }
            }
            if (v0 == null)
            {
                note.Call(PetsCommand.StockingR, false, isStocking, TipManager.GetMessage(PetsReturn.NoExists));
                return;
            }

            IList c1 = burden1.Value.GetValue<IList>("C");

            Variant v1 = BurdenManager.GetBurdenSpace(c1);
            if (v1 == null)
            {
                string str = isStocking ? TipManager.GetMessage(PetsReturn.PetBurdenB2) : TipManager.GetMessage(PetsReturn.PetBurdenB3);
                note.Call(PetsCommand.StockingR, false, isStocking, str);
                return;
            }

            if (burden1.Name == "B2")
            {
                Variant ct = PetAccess.Instance.CreateAward(note.Player.Level, id, note.PlayerID, note.Player.Pet);
                //PetBusiness.CreateAward(note.Player, ct, id, note.PlayerID);
                
                Variant t = v0["T"] as Variant;
                if (t != null)
                {
                    if (t.ContainsKey("ProtectionTime"))
                    {
                        if (ct == null) ct = new Variant();
                        ct.Add("ProtectionTime", t.GetDateTimeOrDefault("ProtectionTime"));
                    }
                }
                v1["T"] = ct;
            }
            else
            {
                Variant t = v0.GetValueOrDefault<Variant>("T");
                Variant ct = new Variant();
                if (t != null)
                {
                    if (t.ContainsKey("ProtectionTime"))
                    {
                        ct.Add("ProtectionTime", t.GetDateTimeOrDefault("ProtectionTime"));
                    }
                }
                v1["T"] = ct;
            }
            v1["E"] = v0["E"];
            v1["G"] = v0["G"];
            v1["S"] = v0["S"];
            v1["H"] = v0["H"];
            v1["A"] = v0["A"];
            v1["D"] = v0["D"];
            v1["R"] = v0["R"];

            if (v0.GetIntOrDefault("I") == 1)
            {
                note.Player.ResetPet(null);
            }

            BurdenManager.BurdenClear(v0);
            burden0.Save();
            burden1.Save();

            //Variant ve = new Variant();
            //ve.Add("B2", note.Player.B2);
            //ve.Add("B3", note.Player.B3);
            //note.Call(BurdenCommand.BurdenListR, ve);
            note.Player.UpdateBurden("B2", "B3");
            if (isStocking)
            {
                note.Call(PetsCommand.StockingR, true, isStocking, PetAccess.Instance.GetPetModel(v1));
            }
            else
            {
                note.Call(PetsCommand.StockingR, true, isStocking, id);
            }

            bool isslip = false;
            if (isStocking)
            {
                if (v0.GetIntOrDefault("P") > 3)
                {
                    isslip = true;
                }
            }
            else
            {
                if (v1.GetIntOrDefault("P") > 3)
                {
                    isslip = true;
                }
            }

            if (isslip)
            {
                List<Variant> ps = PlayerExAccess.Instance.SlipPets(note.Player.B3);
                note.Player.GetSlipPets(ps);
            }
            #endregion
        }

        /// <summary>
        /// 领取照顾奖励
        /// </summary>
        /// <param name="note"></param>
        public void StockingAward(UserNote note)
        {
            string soleid = note.GetString(1);//正在放养的宠物

            Pet p = PetAccess.Instance.FindOneById(soleid);
            if (p == null)
            {
                note.Call(PetsCommand.StockingAwardR, false, string.Empty, TipManager.GetMessage(PetsReturn.NoExists));
                return;
            }
            //得到奖励的用户
            PlayerBusiness ob = note.Player;
            if (ob == null)
            {
                //用户不存在 
                note.Call(PetsCommand.StockingAwardR, false, string.Empty, TipManager.GetMessage(PetsReturn.NoExists));
                return;
            }

            PlayerEx b2 = ob.B2;
            IList c = b2.Value.GetValue<IList>("C");
            Variant m = null;
            foreach (Variant v in c)
            {
                if (v.GetStringOrDefault("E") == soleid)
                {
                    m = v;
                    break;
                }
            }
            if (m == null)
            {
                //宠物已经不在家园中
                note.Call(PetsCommand.StockingAwardR, false, string.Empty, TipManager.GetMessage(PetsReturn.NoExists));
                return;
            }

            //可以得到的奖励
            Variant award = m.GetValueOrDefault<Variant>("T");
            if (award == null)
            {
                //奖励不存在 
                note.Call(PetsCommand.StockingAwardR, false, string.Empty, TipManager.GetMessage(PetsReturn.NoExists));
                return;
            }

            // TODO:是否到领奖时间
            DateTime endTime = award.GetDateTimeOrDefault("EndTime", DateTime.MaxValue);
            if (DateTime.UtcNow < endTime)
            {
                note.Call(PetsCommand.StockingAwardR, false, string.Empty, TipManager.GetMessage(PetsReturn.NoCareTime));
                return;
            }

            Variant psm = p.Value.GetValueOrDefault<Variant>("ShengMing");
            int pss = psm.GetIntOrDefault("V");//被吸宠当前生命值
            if (pss < 1)
            {
                note.Call(PetsCommand.StockingAwardR, false, string.Empty, TipManager.GetMessage(PetsReturn.StockingNo));
                return;
            }
            Variant os = new Variant();
            try
            {
                int score = award.GetIntOrDefault("Score");
                int p1exp = award.GetIntOrDefault("P1exp");
                int p2exp = award.GetIntOrDefault("P2exp");
                int p3exp = award.GetIntOrDefault("P3exp");

                if (score > 0)
                {
                    ob.AddScore(score, FinanceType.StockingAward1);
                }
                if (p1exp > 0) ob.AddExperience(p1exp, FinanceType.StockingAward1);
                if (p2exp > 0) ob.AddPetExp(note.Player.Pet, p2exp, true, (int)FinanceType.StockingAward1);
                if (p3exp > 0) ob.AddPetExp(p, p3exp, false, (int)FinanceType.StockingAward1);

                //照顾一次恢复一定疲劳值
                PetAccess.Instance.FatigueBack(p);

                os["Score"] = score;
                os["P1exp"] = p1exp;
                os["P2exp"] = note.Player.Pet != null ? p2exp : 0;
                os["P3exp"] = p3exp;
                os["PetsID"] = p.Value.GetStringOrDefault("PetsID");
                os["ID"] = p.ID;
                os["IsAll"] = 0;
            }
            catch
            {
            }
            Variant ct = PetAccess.Instance.CreateAward(note.Player.Level, soleid, ob.ID, note.Player.Pet);
            Variant t = m.GetValueOrDefault<Variant>("T");
            if (t.ContainsKey("ProtectionTime"))
            {
                if (ct == null) ct = new Variant();
                ct.Add("ProtectionTime", t.GetDateTimeOrDefault("ProtectionTime"));
            }
            m["T"] = ct;


            b2.Save();
            award.Remove("EndTime");
            ob.Call(PetsCommand.StockingAwardR, true, PetAccess.Instance.GetPetModel(m), award);
            ob.AddAcivity(ActivityType.ZhaoGu, 1);

           note.Player.AddLogVariant(Actiontype.Stocking, null, null,os);
        }

        /// <summary>
        /// 宠物仓库扩展
        /// </summary>
        public void PetExtend(UserNote note)
        {
            PlayerEx b2 = note.Player.B2;
            Variant v = b2.Value;
            //允许最大值
            int max = v.GetIntOrDefault("Max");
            //当前值
            int cur = v.GetIntOrDefault("Cur");

            IList c = v.GetValue<IList>("C");
            if (cur >= max)
            {
                note.Call(PetsCommand.PetExtendR, false, TipManager.GetMessage(PetsReturn.PetExtendMax));
                return;
            }
            int n = 4;

            int number = 0;//note.GetInt32(0);
            
            string goodsid = "";
            double lv = GameConfigAccess.Instance.FindExtend(ExtendType.B2, cur.ToString(),out number, out goodsid);
            if (string.IsNullOrEmpty(goodsid) || number <= 0)
            {
                note.Call(PetsCommand.PetExtendR, false, TipManager.GetMessage(ExtendReturn.B22));
                return;
            }

            if (note.Player.RemoveGoods(goodsid, number, GoodsSource.PetExtend))
            {
                note.Player.UpdateBurden();
                if (NumberRandom.RandomHit(lv))
                {
                    for (int i = cur; i < cur + n; i++)
                    {
                        Variant m = new Variant();
                        m.Add("P", i);
                        m.Add("E", "");
                        m.Add("G", "");//道具ID
                        m.Add("A", 0);
                        m.Add("S", 0);//排序
                        m.Add("H", 0);//0非绑定,1绑定是否绑定
                        m.Add("D", 0);//0不能堆叠,1可以
                        m.Add("T", null);//物品类型 
                        c.Add(m);
                    }
                    v["Cur"] = cur + n;
                    b2.Save();
                    note.Call(PetsCommand.PetExtendR, true, (cur + n));
                }
                else
                {
                    note.Call(PetsCommand.PetExtendR, false, TipManager.GetMessage(ExtendReturn.B23));
                }                
            }

            else
            {
                note.Call(PetsCommand.PetExtendR, false, TipManager.GetMessage(ExtendReturn.B22));
            }
        }

        /// <summary>
        /// 得到宠物列表
        /// </summary>
        /// <param name="note"></param>
        public void GetPetsList(UserNote note)
        {
            //表示没有家园不
            //if (note.Player.SceneID != SceneHome.DefaultID)
            //    return;
            //玩家ID
            string id = note.GetString(0);
            if (note.PlayerID == id)
            {
                note.Call(PetsCommand.GetPetsListR, PetAccess.Instance.GetPetList(note.Player.B2));
                return;
            }

            PlayerBusiness pb = PlayersProxy.FindPlayerByID(id);
            if (pb == null)
                return;
            note.Call(PetsCommand.GetPetsListR, PetAccess.Instance.GetPetList(pb.B2));
        }


        
    }
}
