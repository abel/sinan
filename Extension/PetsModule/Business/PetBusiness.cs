using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.FrontServer;
using Sinan.Entity;
using Sinan.Util;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Data;
using Sinan.Observer;
using Sinan.Schedule;
using Sinan.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;
using Sinan.Log;

namespace Sinan.PetsModule.Business
{
    public partial class PetBusiness
    {
        /// <summary>
        /// 提取宠物技能
        /// </summary>
        public static void DrawPetSkill(UserNote note)
        {
            string id = note.GetString(0);
            int place = note.GetInt32(1);
            int number = note.GetInt32(2);

            //提升成功率的道具
            string goodsid = TipManager.GetMessage(PetsReturn.DrawGoods);
            Pet p = PetAccess.Instance.FindOneById(id);
            if (p == null)
            {
                note.Call(PetsCommand.DrawPetSkillR, false, TipManager.GetMessage(PetsReturn.NoPets), id);
                return;
            }
            string petsid = p.Value.GetStringOrDefault("PetsID");

            int isBinding = p.Value.GetIntOrDefault("IsBinding");//绑定状态    
            int level = p.Value.GetIntOrDefault("PetsLevel");

            IList skillGroove = p.Value.GetValue<IList>("SkillGroove");
            Variant t = null;
            foreach (Variant d in skillGroove)
            {
                if (d.GetIntOrDefault("P") == place)
                {
                    t = d;
                    break;
                }
            }
            if (t == null)
            {
                note.Call(PetsCommand.DrawPetSkillR, false, TipManager.GetMessage(PetsReturn.NoPlace), id);
                return;
            }

            //是否允许提取
            if (t.GetIntOrDefault("Born") != 0)
            {
                note.Call(PetsCommand.DrawPetSkillR, false, TipManager.GetMessage(PetsReturn.NoDraw), id);
                return;
            }
            //技能等级
            int skillLevel = t.GetIntOrDefault("Level");
            //需要石币数量
            int score = skillLevel * 500;
            if (note.Player.Score < score)
            {
                note.Call(PetsCommand.DrawPetSkillR, false, TipManager.GetMessage(PetsReturn.NoScore), id);
                return;
            }
            GameConfig gc = GameConfigAccess.Instance.FindOneById(t.GetStringOrDefault("SkillID"));
            IList cl = gc.UI.GetValue<IList>("ControlLev");
            PlayerEx b0 = note.Player.B0;
            IList c = b0.Value.GetValue<IList>("C");
            //提升技能提取成功率的道具的数量
            int n = 0;
            if (number > 0)
            {
                foreach (Variant k in c)
                {
                    if (k.GetStringOrDefault("G") == goodsid)
                    {
                        n += k.GetIntOrDefault("A");
                    }
                }
                if (n < number)
                {
                    note.Call(PetsCommand.DrawPetSkillR, false, TipManager.GetMessage(PetsReturn.DrawSuccessGoods), id);
                    return;
                }
            }

            IList cp = gc.UI.GetValue<IList>("CheckProb");
            IList GoodsID = gc.UI.GetValue<IList>("GoodsID");
            //目标道具ID
            string goal = GoodsID[skillLevel - 1].ToString();




            PlayerEx b3 = note.Player.B3;
            IList c3 = b3.Value.GetValue<IList>("C");
            Variant t3 = null;
            foreach (Variant k3 in c3)
            {
                if (k3.GetStringOrDefault("E") == id)
                {
                    t3 = k3;//表示背包中的宠物
                    break;
                }
            }


            Variant pk = null;
            PlayerEx home = null;
            if (t3 == null)
            {
                home = note.Player.Home;
                Variant vt = home.Value.GetValueOrDefault<Variant>("PetKeep");
                if (vt.GetStringOrDefault("ID") == id)
                {
                    pk = vt;
                }
            }

            if (t3 == null && pk == null)
            {
                note.Call(PetsCommand.DrawPetSkillR, false, TipManager.GetMessage(PetsReturn.NoExists), id);
                return;
            }

            if (!note.Player.AddScore(-score, FinanceType.DrawPetSkill))
            {
                note.Call(PetsCommand.DrawPetSkillR, false, TipManager.GetMessage(PetsReturn.NoScore), id);
                return;
            }

            Dictionary<string, Variant> goods = new Dictionary<string, Variant>();
            Variant v = new Variant();
            v.Add("Number" + isBinding, 1);
            goods.Add(goal, v);
            if (BurdenManager.IsFullBurden(c, goods))
            {
                note.Call(PetsCommand.DrawPetSkillR, false, TipManager.GetMessage(BurdenReturn.BurdenFull), id);
                return;
            }

            //提取是否成功
            double a = number > 0 ? (GoodsAccess.Instance.GetSuccess(number) + Convert.ToDouble(cp[skillLevel - 1])) : Convert.ToDouble(cp[skillLevel - 1]);
                       
            Variant mv = MemberAccess.MemberInfo(note.Player.MLevel);
            if (mv != null)
            {
                a *= (1 + mv.GetDoubleOrDefault("PetSkillLv"));
            }
            

            bool isSuccess = NumberRandom.RandomHit(a);

            //删除物品
            Variant us = new Variant();
            if (number > 0)
            {
                if (note.Player.RemoveGoods(goodsid, number, GoodsSource.DrawCard)) 
                {
                    us[goodsid] = number;
                }                                
            }

            //移除宠物
            if (t3 != null)
            {
                BurdenManager.BurdenClear(t3);
            }

            //移除家园中宠物
            if (pk != null)
            {
                pk["ID"] = string.Empty;
                pk["PetID"] = string.Empty;
                pk["StartTime"] = string.Empty;
                pk["EndTime"] = string.Empty;
                pk["PetsWild"] = 0;
                pk["PetName"] = string.Empty;
                pk["PetsRank"] = 0;
                home.Save();
            }

            b0.Save();
            b3.Save();

            PetAccess.Instance.RemoveOneById(p.ID, SafeMode.False);

            //得到物品
            Variant gs = new Variant();
            if (isSuccess)
            {
                gs[goal] = 1;
                note.Player.AddGoods(goods, GoodsSource.DrawCard);
               
                note.Call(PetsCommand.DrawPetSkillR, true, goal, id);
            }
            else
            {
                note.Call(PetsCommand.DrawPetSkillR, false, TipManager.GetMessage(PetsReturn.DrawFail), id);
            }
            if (t3 != null)
            {
                note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(b3));
                if (t3.GetIntOrDefault("P") > 3)
                {
                    List<Variant> ps = PlayerExAccess.Instance.SlipPets(note.Player.B3);
                    note.Player.GetSlipPets(ps);
                }
            }

            if (!string.IsNullOrEmpty(petsid))
            {
                PetExp(note.Player, id, petsid, FinanceType.DrawPetSkill, GoodsSource.DrawCard, 0, level);
            }

            Variant os = new Variant();
            os["ID"] = p.ID;
            os["PetsID"] = petsid;
            os["Score"] = -score;
            os["Lv"] = a;
            os["IsSuccess"] = isSuccess;
            note.Player.AddLogVariant(Actiontype.DrawPetSkill, us, gs, os);
        }

        /// <summary>
        /// 解除技能
        /// </summary>
        /// <param name="note"></param>
        public static void RemoveSkill(UserNote note)
        {
            string playerpetid = note.GetString(0);
            string place = note.GetString(1);

            //判断是否是出战宠物
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
                note.Call(PetsCommand.RemoveSkillR, false, TipManager.GetMessage(PetsReturn.NoPets));
                return;
            }

            PlayerEx b3 = note.Player.B3;
            IList c = b3.Value.GetValue<IList>("C");
            Variant t = null;
            foreach (Variant tt in c)
            {
                if (tt.GetStringOrDefault("E") == playerpetid)
                {
                    t = tt;
                    break;
                }
            }
            if (t == null)
            {
                note.Call(PetsCommand.RemoveSkillR, false, TipManager.GetMessage(PetsReturn.NoPets));
                return;
            }

            IList SkillGroove = p.Value.GetValue<IList>("SkillGroove");
            Variant tmp = null;
            foreach (Variant d in SkillGroove)
            {
                if (d.GetStringOrDefault("P") == place)
                {
                    tmp = d;
                    break;
                }
            }
            if (tmp == null)
            {
                note.Call(PetsCommand.RemoveSkillR, false, TipManager.GetMessage(PetsReturn.NoPlace));
                return;
            }
            string skillid = tmp.GetStringOrDefault("SkillID");
            int level = tmp.GetIntOrDefault("Level");
            if (skillid == "0" || skillid == "-1")
            {
                note.Call(PetsCommand.RemoveSkillR, false, TipManager.GetMessage(PetsReturn.NoSkill));
                return;
            }
            if (tmp.GetIntOrDefault("Born") == 0)
            {
                note.Call(PetsCommand.RemoveSkillR, false, TipManager.GetMessage(PetsReturn.RemoveSkill));
                return;
            }

            RemoveKey(p, skillid);
            
            tmp["SkillID"] = "0";
            tmp["Born"] = 0;
            tmp["MaxUse"] = 0;
            tmp["Key"] = string.Empty;
            tmp["Level"] = 0;
            tmp["SkillName"] = string.Empty;//技能名称

            Variant v = p.Value.GetValueOrDefault<Variant>("Skill");
            if (v != null)
            {
                if (v.ContainsKey(skillid))
                {
                    v.Remove(skillid);
                }
            }
            p.Save();
            UpdateSkill(note, p);
            note.Call(PetsCommand.UpdatePetR, true, p);
            note.Call(PetsCommand.RemoveSkillR, true, TipManager.GetMessage(PetsReturn.Success));

            if (IsOnline)
            {
                Variant vv = new Variant();
                vv.Add("ID", p.ID);
                vv.Add("Skill", p.Value.GetValueOrDefault<Variant>("Skill"));
                vv.Add("PetsID", p.Value.GetStringOrDefault("PetsID"));
                vv.Add("PetsRank", p.Value.GetIntOrDefault("PetsRank"));
                vv.Add("Skin", p.Value.GetStringOrDefault("Skin"));
                vv.Add("Name", p.Name);

                Variant nn = new Variant();
                nn.Add("Pet", vv);
                nn.Add("ID", note.PlayerID);
                note.Player.CallAll(ClientCommand.UpdateActorR, nn);
            }
            if (t.GetIntOrDefault("P") > 3)
            {
                List<Variant> ps = PlayerExAccess.Instance.SlipPets(note.Player.B3);
                note.Player.GetSlipPets(ps);
            }

            Variant os = new Variant();
            os["ID"] = p.ID;
            os["PetsID"] = p.Value.GetStringOrDefault("PetsID");
            os["PetsLevel"] = p.Value.GetIntOrDefault("PetsLevel");
            os["Status"] = 2;
            os["SkillID"] = skillid;
            os["Level"] = level;
            os["Score"] = 0;
            note.Player.AddLogVariant(Actiontype.PetSkill, null, null, os);
        }

        /// <summary>
        /// 遗忘技能
        /// </summary>
        /// <param name="note"></param>
        public static void OblivionSkill(UserNote note)
        {
            string playerpetid = note.GetString(0);
            int place = note.GetInt32(1);
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

            if (p == null || p.Value == null)
            {
                note.Call(PetsCommand.OblivionSkillR, false, TipManager.GetMessage(PetsReturn.NoPlace));
                return;
            }

            //得到宠物技能槽
            IList sg = p.Value.GetValue<IList>("SkillGroove");
            Variant t = null;
            foreach (Variant d in sg)
            {
                if (d.GetIntOrDefault("P") == place)
                {
                    t = d;
                    break;
                }
            }
            if (t == null)
            {
                note.Call(PetsCommand.OblivionSkillR, false, TipManager.GetMessage(PetsReturn.NoPlace));
                return;
            }

            if (t.GetIntOrDefault("Born") != 0)
            {
                note.Call(PetsCommand.OblivionSkillR, false, TipManager.GetMessage(PetsReturn.NoBorn));
                return;
            }

            //遗忘需要石币
            int level = t.GetIntOrDefault("Level");
            int score = level * 500;
            if (note.Player.Score < score || (!note.Player.AddScore(-score, FinanceType.OblivionSkill)))
            {
                note.Call(PetsCommand.OblivionSkillR, false, TipManager.GetMessage(PetsReturn.NoScore));
                return;
            }

            string skillid = t.GetStringOrDefault("SkillID");

            RemoveKey(p, skillid);

            t["SkillID"] = "0";
            t["Born"] = 0;
            t["MaxUse"] = 0;
            t["Key"] = string.Empty;
            t["Level"] = 0;

            Variant skill = p.Value.GetValueOrDefault<Variant>("Skill");
            if (skill.ContainsKey(skillid))
            {
                skill.Remove(skillid);
            }
            p.Save();
            UpdateSkill(note, p);

            note.Call(PetsCommand.UpdatePetR, true, p);
            List<Variant> ps = PlayerExAccess.Instance.SlipPets(note.Player.B3);
            note.Player.GetSlipPets(ps);


            Variant os = new Variant();
            os["ID"] = p.ID;
            os["PetsID"] = p.Value.GetStringOrDefault("PetsID");
            os["PetsLevel"] = p.Value.GetIntOrDefault("PetsLevel");
            os["Status"] = 0;
            os["SkillID"] = skillid;
            os["Level"] = level;
            os["Score"] = -score;
            note.Player.AddLogVariant(Actiontype.PetSkill, null, null, os);
        }
        /// <summary>
        /// 激活空位槽
        /// </summary>
        /// <param name="note"></param>
        public static void ShockSkill(UserNote note)
        {
            string playerpetid = note.GetString(0);
            //int place = note.GetInt32(1);

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
                note.Call(PetsCommand.ShockSkillR, false, TipManager.GetMessage(PetsReturn.NoPets));
                return;
            }


            //得到宠物技能槽
            IList SkillGroove = p.Value.GetValue<IList>("SkillGroove");
            Variant tmp = null;
            foreach (Variant d in SkillGroove)
            {
                if (d.GetStringOrDefault("SkillID") != "-1")
                    continue;

                if (tmp == null || tmp.GetIntOrDefault("P") > d.GetIntOrDefault("P"))
                {
                    tmp = d;
                }
            }

            if (tmp==null)
            {
                note.Call(PetsCommand.ShockSkillR, false, TipManager.GetMessage(PetsReturn.GooveFull));
                return;
            }

            int number = 0;//note.GetInt32(1);
            string goodsid = "";
            double lv = GameConfigAccess.Instance.FindExtend(ExtendType.ShockSkill, tmp.GetStringOrDefault("P"), out number, out goodsid);
            if (string.IsNullOrEmpty(goodsid) || number <= 0)
            {
                note.Call(PetsCommand.ShockSkillR, false, TipManager.GetMessage(ExtendReturn.ShockSkill2));
                return;
            }
            
            Variant mv = MemberAccess.MemberInfo(note.Player.MLevel);
            if (mv != null)
            {
                lv *= (1 + mv.GetDoubleOrDefault("ShockSkillLv"));
            }

            
            

            if (note.Player.RemoveGoods(goodsid, number, GoodsSource.ShockSkill))
            {
                note.Player.UpdateBurden();
                if (NumberRandom.RandomHit(lv))
                {

                    tmp["SkillID"] = "0";
                    p.Save();
                    //note.Player.UpdateBurden();
                    note.Call(PetsCommand.UpdatePetR, true, p);
                    //note.Call(PetsCommand.ShockSkillR, false, TipManager.GetMessage(ExtendReturn.ShockSkill3));

                }
                else
                {
                    note.Call(PetsCommand.ShockSkillR, false, TipManager.GetMessage(ExtendReturn.ShockSkill3));
                }                
            }
            else 
            {
                note.Call(PetsCommand.ShockSkillR, false, TipManager.GetMessage(ExtendReturn.ShockSkill2));                
            }
        }

        /// <summary>
        /// 添加技能
        /// </summary>
        /// <param name="note"></param>
        public static void AddSkill(UserNote note)
        {
            string playerpetid = note.GetString(0);
            Pet p = null;
            bool IsOnline = false;
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
                note.Call(PetsCommand.AddSkillR, false, TipManager.GetMessage(PetsReturn.NoPets));
                return;
            }
            string skillid = note.GetString(1);
            int level = note.GetInt32(2);
            //得到技能信息
            GameConfig gc = GameConfigAccess.Instance.FindOneById(skillid);
            if (gc == null)
            {
                note.Call(PetsCommand.AddSkillR, false, TipManager.GetMessage(PetsReturn.SkillConfigError));
                return;
            }
            //角色使用限制
            string RoleLimit = gc.UI.GetStringOrDefault("RoleLimit");
            IList ControlLev = gc.UI.GetValue<IList>("ControlLev");
            if (RoleLimit != "0")
            {
                if (p.Value.GetStringOrDefault("PetsType") != RoleLimit)
                {
                    note.Call(PetsCommand.AddSkillR, false, TipManager.GetMessage(PetsReturn.SkillTypeError));
                    return;
                }
            }
            //技能要求宠物阶级
            if (Convert.ToInt32(ControlLev[level - 1]) > p.Value.GetIntOrDefault("PetsRank"))
            {
                note.Call(PetsCommand.AddSkillR, false, TipManager.GetMessage(PetsReturn.NoPetsRank));
                return;
            }

            PlayerEx petbook = note.Player.PetBook;
            if (petbook == null)
            {
                note.Call(PetsCommand.AddSkillR, false, TipManager.GetMessage(PetsReturn.NoStudySkill));
                return;
            }

            IList ls = petbook.Value[skillid] as IList;
            if (ls == null || ls.Count == 0)
            {
                note.Call(PetsCommand.AddSkillR, false, TipManager.GetMessage(PetsReturn.NoStudySkill));
                return;
            }
            //表示还没有学会该等级
            if (!ls.Contains(level))
            {
                note.Call(PetsCommand.AddSkillR, false, TipManager.GetMessage(PetsReturn.NoStudySkill));
                return;
            }
            IList SkillGroove = p.Value.GetValue<IList>("SkillGroove");

            Variant tmp = null;
            foreach (Variant k in SkillGroove)
            {
                if (k.GetStringOrDefault("SkillID") == skillid)
                {
                    note.Call(PetsCommand.AddSkillR, false, TipManager.GetMessage(PetsReturn.IsSkill));
                    return;
                }
                if (tmp == null && k.GetStringOrDefault("SkillID") == "0")
                {
                    tmp = k;
                }
            }
            if (tmp == null)
            {
                note.Call(PetsCommand.AddSkillR, false, TipManager.GetMessage(PetsReturn.SkillGooveFull));
                return;
            }

            Variant v = p.Value.GetValueOrDefault<Variant>("Skill");
            if (v == null)
            {
                v = new Variant();
                v.Add(skillid, level);
                p.Value["Skill"] = v;
            }
            else if (v.ContainsKey(skillid))
            {
                v[skillid] = level;
            }
            else
            {
                v.Add(skillid, level);
            }
            tmp["SkillID"] = gc.ID;
            tmp["Born"] = 1;
            tmp["MaxUse"] = gc.UI.GetIntOrDefault("MaxUse");
            tmp["Key"] = string.Empty;
            tmp["Level"] = level;
            tmp["SkillName"] = gc.Name;

            UpdateSkill(note, p);
            p.Save();
            note.Call(PetsCommand.AddSkillR, true, string.Empty);


            if (IsOnline)
            {
                Variant vv = new Variant();
                vv.Add("ID", p.ID);
                vv.Add("Skill", p.Value.GetValueOrDefault<Variant>("Skill"));
                vv.Add("PetsID", p.Value.GetStringOrDefault("PetsID"));
                vv.Add("PetsRank", p.Value.GetIntOrDefault("PetsRank"));
                vv.Add("Skin", p.Value.GetStringOrDefault("Skin"));
                vv.Add("Name", p.Name);

                Variant nn = new Variant();
                nn.Add("Pet", vv);
                nn.Add("ID", note.PlayerID);

                note.Player.CallAll(ClientCommand.UpdateActorR, nn);
            }
            //发送溜宠信息
            List<Variant> ps = PlayerExAccess.Instance.SlipPets(note.Player.B3);
            note.Player.GetSlipPets(ps);

            Variant os = new Variant();
            os["ID"] = p.ID;
            os["PetsID"] = p.Value.GetStringOrDefault("PetsID");
            os["PetsLevel"] = p.Value.GetIntOrDefault("PetsLevel");
            os["Status"] = 1;
            os["SkillID"] = skillid;
            os["Level"] = level;
            os["Score"] = 0;
            note.Player.AddLogVariant(Actiontype.PetSkill, null, null, os);
        }

        /// <summary>
        /// 重新计算宠物基本属性
        /// </summary>
        /// <param name="note"></param>
        /// <param name="p"></param>
        private static void UpdateSkill(UserNote note, Pet p)
        {
            PlayerEx family = note.Player.Family;
            if (note.Player.Pet != null && note.Player.Pet.ID == p.ID)
            {
                PetAccess.PetReset(p, note.Player.Skill, false,note.Player.Mounts);
            }
            else
            {
                PetAccess.PetReset(p, null, false, null);
            }
            note.Call(PetsCommand.UpdatePetR, true, p);
        }

        /// <summary>
        /// 清除快捷键
        /// </summary>
        /// <param name="p"></param>
        /// <param name="skillid"></param>
        private static void RemoveKey(Pet p, string skillid)
        {
            for (int i = 0; i < 10; i++)
            {
                if (p.Value.ContainsKey(i.ToString()))
                {
                    if (p.Value.GetStringOrDefault(i.ToString()) == skillid)
                    {
                        p.Value[i.ToString()] = string.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// 添加快捷键
        /// </summary>
        /// <param name="note"></param>
        public static void AddKey(UserNote note)
        {
            string playerpetid = note.GetString(0);

            Variant o = note.GetVariant(1);
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
                note.Call(PetsCommand.AddKeyR, false, TipManager.GetMessage(PetsReturn.NoPets));
                return;
            }

            if (!p.Value.ContainsKey("HotKeys"))
            {
                Variant v = new Variant();
                for (int i = 0; i < 10; i++)
                {
                    v.Add(i.ToString(), string.Empty);
                }
                p.Value.Add("HotKeys", v);
            }
            else
            {
                Variant h = p.Value.GetVariantOrDefault("HotKeys");
                string[] strs = new string[h.Count];
                h.Keys.CopyTo(strs, 0);
                for (int i = 0; i < strs.Length; i++)
                {
                    if (h.ContainsKey(strs[i]))
                    {
                        h[strs[i]] = string.Empty;
                    }
                }
            }

            //宠物技能槽
            IList SkillGroove = p.Value.GetValue<IList>("SkillGroove");
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var item in o)
            {
                string value = (string)item.Value;
                bool IsHave = false;
                foreach (Variant v in SkillGroove)
                {
                    if (v.GetStringOrDefault("SkillID") == value)
                    {
                        IsHave = true;
                        break;
                    }
                }
                if (IsHave)
                {
                    if (!dic.ContainsKey(value))
                    {
                        //去除相同技能
                        dic.Add(value, item.Key);
                    }
                }
            }
            Variant HotKeys = p.Value.GetVariantOrDefault("HotKeys");

            foreach (string k in dic.Keys)
            {
                if (HotKeys.ContainsKey(dic[k]))
                {
                    HotKeys[dic[k]] = k;
                }
            }
            p.Save();
            note.Call(PetsCommand.UpdatePetR, true, p);

            note.Call(PetsCommand.AddKeyR, true, TipManager.GetMessage(PetsReturn.AddKey));
        }

        /// <summary>
        /// 宠物进化技能变更
        /// </summary>
        /// <param name="note"></param>
        public static void EvoSkillChange(UserNote note)
        {
            //宠物ID
            string id = note.GetString(0);
            //升级哪个位置的技能
            int p = note.GetInt32(1);
            if (p != 10 && p != 11)
                return;

            Pet pet = PetAccess.Instance.FindOneById(id);
            if (pet == null)
            {
                note.Call(PetsCommand.EvoSkillChangeR, false, TipManager.GetMessage(PetsReturn.NoPets));
                return;
            }
            Variant pv = pet.Value;
            IList sg = pv.GetValue<IList>("SkillGroove");

            Variant t = null;
            foreach (Variant v in sg)
            {
                if (v.GetIntOrDefault("P") == p)
                {
                    t = v;
                    break;
                }
            }

            if (t == null)
            {
                note.Call(PetsCommand.EvoSkillChangeR, false, TipManager.GetMessage(PetsReturn.NoPetSkill));
                return;
            }

            //之前的技能
            string skillid = t.GetStringOrDefault("SkillID");
            if (skillid == "-1")
            {
                //表示还没有进化技能
                note.Call(PetsCommand.EvoSkillChangeR, false, TipManager.GetMessage(PetsReturn.NoPetSkill));
                return;
            }
            string petsid = pv.GetStringOrDefault("PetsID");
            GameConfig gc = GameConfigAccess.Instance.FindOneById(petsid);
            if (gc == null) 
            {
                note.Call(PetsCommand.EvoSkillChangeR, false, TipManager.GetMessage(PetsReturn.NoPets));
                return;
            }

            string r = "R1";
            if (p == 11) r = "R2";
            

            //得到相关进化技能
            Variant vr = gc.Value.GetVariantOrDefault(r);

            if (vr == null)
            {
                note.Call(PetsCommand.EvoSkillChangeR, false, TipManager.GetMessage(PetsReturn.NoPetSkill));
                return;
            }


            

            IList rankSkill = vr.GetValue<IList>("RankSkill");
            if (rankSkill == null)
            {
                note.Call(PetsCommand.EvoSkillChangeR, false, TipManager.GetMessage(PetsReturn.NoPetSkill));
                return;
            }

            int m = NumberRandom.Next(1, 101);
            
            Variant sk = null;//随机产生
            Variant skid = null;//取得一个技能
            foreach (Variant k in rankSkill)
            {
                if (k.GetStringOrDefault("SkillID") == skillid)
                    continue;
                if (m >= k.GetIntOrDefault("RateMix") && m <= k.GetIntOrDefault("RateMax"))
                {
                    sk = k;
                    break;
                }
                if (skid == null)
                {
                    skid = k;
                }
            }

            if (sk == null && skid == null)
            {
                note.Call(PetsCommand.EvoSkillChangeR, false, TipManager.GetMessage(PetsReturn.NoPetSkill));
                return;
            }


            string goodsid = "G_n000010";
            if (!note.Player.RemoveGoods(goodsid, GoodsSource.EvoSkillChange))
            {
                //宠物进化技能变更所需道具不存在
                note.Call(PetsCommand.EvoSkillChangeR, false, TipManager.GetMessage(PetsReturn.EvoSkillChange1));
                return;
            }

            if (sk != null)
            {
                t["SkillID"] = sk.GetStringOrDefault("SkillID");
                t["Level"] = sk.GetIntOrDefault("Level");
            }
            else 
            {   
                t["SkillID"] = skid.GetStringOrDefault("SkillID");
                t["Level"] = skid.GetIntOrDefault("Level");
            }



            //宠物技能
            Variant skill = pv.GetVariantOrDefault("Skill");

            string s = t.GetStringOrDefault("SkillID");

            GameConfig gcx = GameConfigAccess.Instance.FindOneById(s);
            t["SkillName"] = gcx.Name;
            int l = t.GetIntOrDefault("Level");
            if (skill != null)
            {
                //移除旧的技能
                skill.Remove(skillid);
                //添加新技能
                skill[s] = l;
            }
            else 
            {
                skill = new Variant();
                skill[s] = l;
                pv["Skill"] = skill;
            }
            UpdateSkill(note, pet);
            note.Call(PetsCommand.EvoSkillChangeR, true, "");
        }

        /// <summary>
        /// 宠物进化技能升级
        /// </summary>
        /// <param name="note"></param>
        public static void EvoSkillUp(UserNote note)
        {
            //宠物ID
            string id = note.GetString(0);
            //升级哪个位置的技能
            int p = note.GetInt32(1);
            if (p != 10 && p != 11)
                return;

            Pet pet = PetAccess.Instance.FindOneById(id);
            if (pet == null) 
            {
                note.Call(PetsCommand.EvoSkillUpR, false, TipManager.GetMessage(PetsReturn.NoPets));
                return;
            }

            Variant pv = pet.Value;
            IList sg = pv.GetValue<IList>("SkillGroove");

            Variant t = null;
            foreach (Variant v in sg) 
            {
                if (v.GetIntOrDefault("P") == p) 
                {
                    t = v;
                    break;
                }
            }

            if (t == null) 
            {
                note.Call(PetsCommand.EvoSkillUpR, false, TipManager.GetMessage(PetsReturn.NoPetSkill));
                return;
            }

            string skillid = t.GetStringOrDefault("SkillID");
            if (skillid == "-1") 
            {
                //表示还没有进化技能
                note.Call(PetsCommand.EvoSkillUpR, false, TipManager.GetMessage(PetsReturn.NoPetSkill));
                return;
            }

            GameConfig gc = GameConfigAccess.Instance.FindOneById(skillid);
            if (gc == null) 
            {
                note.Call(PetsCommand.EvoSkillUpR, false, TipManager.GetMessage(PetsReturn.NoPetSkill));
                return;
            }
            //技能最大等级数
            int ml = gc.UI.GetIntOrDefault("Level");

            if (!gc.Value.ContainsKey(ml.ToString()))
            {
                //技能配置有问题
                note.Call(PetsCommand.EvoSkillUpR, false, TipManager.GetMessage(PetsReturn.SkillConfigError));
                return;
            }

            //技能当前等级数
            int cl = t.GetIntOrDefault("Level");

            if (cl >= ml) 
            {
                //进化技能已经最高级不能再升级
                note.Call(PetsCommand.EvoSkillUpR, false, TipManager.GetMessage(PetsReturn.EvoSkillUp1));
                return;
            }

            string goodsid = "G_n000011";
            if (!note.Player.RemoveGoods(goodsid, GoodsSource.EvoSkillUp))
            {
                //提升宠物进化技能所需道具不存在
                note.Call(PetsCommand.EvoSkillUpR, false,TipManager.GetMessage(PetsReturn.EvoSkillUp2));
                return;
            }

            t.SetOrInc("Level", 1);
            Variant skill = pv.GetVariantOrDefault("Skill");
            if (skill == null)
            {
                skill = new Variant();
                skill[skillid] = t.GetIntOrDefault("Level");
                pv["Skill"] = skill;
            }
            else
            {
                skill[skillid] = t.GetIntOrDefault("Level");
            }
            UpdateSkill(note, pet);
            note.Call(PetsCommand.EvoSkillUpR, true, "");
        }


        /// <summary>
        /// 提取技能或放生得到经验
        /// </summary>
        /// <param name="player">角色</param>
        /// <param name="id">宠物唯一标识</param>
        /// <param name="petsid">宠物</param>
        /// <param name="ft">操作方式</param>
        /// <param name="pl">宠物所有位置</param>
        /// <param name="gs">操作</param>
        public static void PetExp(PlayerBusiness player, string id, string petsid, FinanceType ft, GoodsSource gs, int pl,int level)
        {
            GameConfig gc = GameConfigAccess.Instance.FindOneById(petsid);
            if (gc == null)
                return;

            Variant v = gc.Value;
            if (v == null)
                return;

            //角色得到经验
            int p1exp = v.GetIntOrDefault("P1exp");
            //宠物取得经验
            int p2exp = v.GetIntOrDefault("P2exp");

            if (p1exp > 0)
            {
                player.AddExperience(p1exp, ft);
            }

            if (p2exp > 0 && player.Pet != null)
            {
                player.AddPetExp(player.Pet, p2exp, true, (int)ft);
            }

            player.AddLog(Actiontype.PetRemove, petsid, level, gs, id, pl);
        }
    }
}
