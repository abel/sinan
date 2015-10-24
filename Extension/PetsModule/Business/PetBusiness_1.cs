using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Sinan.Core;
using Sinan.Data;
using Sinan.FrontServer;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Observer;
using Sinan.Util;
using Sinan.Extensions;

namespace Sinan.PetsModule.Business
{
    partial class PetBusiness
    {
        /// <summary>
        /// 诱宠成功率
        /// </summary>
        /// <param name="note"></param>
        public static void StealPet(UserNote note)
        {
            string pid = note.GetString(0);
            if (pid == note.PlayerID)
            {
                note.Call(PetsCommand.StealPetR, false, TipManager.GetMessage(PetsReturn.StealSlef));
                return;
            }
            int number = note.GetInt32(1);

            if (note.Player.Level < 40)
            {
                note.Call(PetsCommand.StealPetR, false, TipManager.GetMessage(PetsReturn.StealMinLev));
                return;
            }

            string playerid = note.GetString(2);//被偷宠主人ID
            Pet p = PetAccess.Instance.GetPetByID(pid, playerid);
            if (p == null)
            {
                note.Call(PetsCommand.StealPetR, false, TipManager.GetMessage(PetsReturn.NoExists));
                return;
            }

            if (Loyalty(p) >= 100)
            {
                note.Call(PetsCommand.StealPetR, false, TipManager.GetMessage(PetsReturn.StealPet3));
                return;
            }

            int petlev = p.Value.GetIntOrDefault("PetsLevel");
            if (petlev - note.Player.Level > 5)
            {
                note.Call(PetsCommand.StealPetR, false, TipManager.GetMessage(PetsReturn.StealMinLev2));
                return;
            }
            double stealLv = StealLv(p, number);
            if (stealLv < 0.05)
            {
                note.Call(PetsCommand.StealPetR, false, TipManager.GetMessage(PetsReturn.StealPet4));
                return;
            }

            PlayerEx b0 = note.Player.B0;
            IList c0 = b0.Value.GetValue<IList>("C");
            //偷宠道具
            string goodsid = TipManager.GetMessage(PetsReturn.StealGoods);
            //得到数量
            int num = BurdenManager.GoodsCount(b0, goodsid);
            if (number > num)
            {
                note.Call(PetsCommand.StealPetR, false, TipManager.GetMessage(PetsReturn.StealNoGoods));
                return;
            }

            PlayerEx ob2 = note.Player.B3;
            IList oc = ob2.Value.GetValue<IList>("C");
            //得到一个空格子
            Variant ov = BurdenManager.GetBurdenSpace(oc);
            if (ov == null)
            {
                note.Call(PetsCommand.StealPetR, false, TipManager.GetMessage(PetsReturn.StealBurdenFull));
                return;
            }

            PlayerBusiness pb = PlayersProxy.FindPlayerByID(playerid);
            if (pb == null)
            {
                note.Call(PetsCommand.StealPetR, false, TipManager.GetMessage(PetsReturn.NoUser));
                return;
            }

            PlayerEx b2 = pb.B2;
            if (b2 == null)
            {
                note.Call(PetsCommand.StealPetR, false, TipManager.GetMessage(PetsReturn.ParaNameError));
                return;
            }

            IList c = b2.Value.GetValue<IList>("C");
            Variant m = null;
            foreach (Variant n in c)
            {
                if (n.GetStringOrDefault("E") == pid)
                {
                    m = n;
                    break;
                }
            }

            if (m == null)
            {
                note.Call(PetsCommand.StealPetR, false, TipManager.GetMessage(PetsReturn.ParaNameError));
                return;
            }

            Variant t = m.GetVariantOrDefault("T");
            if (t.ContainsKey("ProtectionTime"))
            {
                if (t.GetDateTimeOrDefault("ProtectionTime") > DateTime.UtcNow)
                {
                    note.Call(PetsCommand.StealPetR, false, TipManager.GetMessage(PetsReturn.StealProtectionTime));
                    return;
                }
            }

            if (!note.Player.RemoveGoods(goodsid, number, GoodsSource.StealPet))
            {
                note.Call(PetsCommand.StealPetR, false, TipManager.GetMessage(PetsReturn.StealPet5));
                return;
            }

            Variant ve = new Variant();
            ve.Add("B0", note.Player.B0);
            note.Call(BurdenCommand.BurdenListR, ve);

            bool issu = NumberRandom.RandomHit(stealLv);
            if (issu)
            {
                SkillClear(p);

                ov["E"] = m["E"];
                ov["G"] = m["G"];
                ov["S"] = m["S"];
                ov["H"] = m["H"];
                ov["A"] = m["A"];
                ov["D"] = m["D"];
                ov["R"] = m["R"];
                ov["T"] = null;
                //被偷宠喂养成长果清0
                p.Value["FeedCount"] = 0;
                BurdenManager.BurdenClear(m);
                PetAccess.PetReset(p, note.Player.Skill, false,null);
                p.PlayerID = note.PlayerID;

                b0.Save();
                ob2.Save();
                b2.Save();
                p.Save();
                Variant msg1 = new Variant();
                msg1.Add("ID", pb.ID);
                msg1.Add("PetID", p.ID);
                msg1.Add("Message", TipManager.GetMessage(PetsReturn.StealPet13));
                note.Call(PetsCommand.StealPetR, true, msg1);
            }
            else
            {
                note.Call(PetsCommand.StealPetR, false, TipManager.GetMessage(PetsReturn.StealPet6));
            }

            string msg = string.Format(TipManager.GetMessage(PetsReturn.StealPet7), DateTime.Now, p.Name, note.Player.Name);
            //"你的宠物【" + p.Name + "】被【" + note.Player.Name + "】诱走";
            if (pb.Online && issu)
            {
                //你的宠物【{0}】被【{1}】偷走
                Variant msg2 = new Variant();
                msg2.Add("ID", pb.ID);
                msg2.Add("PetID", p.ID);
                msg2.Add("Message", msg);
                pb.Call(PetsCommand.StealPetR, true, msg2);
            }
            if (issu)
            {
                //DateTime.Now.ToString("M月d日H时m分,") + 
                pb.AddBoard(msg);
            }

            string title = "";
            string content = "";
            if (issu)
            {
                title = string.Format(TipManager.GetMessage(PetsReturn.StealPet8), p.Name, note.Player.Name);
                content = string.Format(TipManager.GetMessage(PetsReturn.StealPet9), p.Name, note.Player.Name);
            }
            else
            {
                title = string.Format(TipManager.GetMessage(PetsReturn.StealPet10), p.Name);
                content = string.Format(TipManager.GetMessage(PetsReturn.StealPet11), p.Name, note.Player.Name);

            }
            //可以得到多少捕兽网
            int mn = Convert.ToInt32(number * 0.6);
            mn = mn >= 160 ? 160 : mn;

            Variant gs = new Variant();
            gs.Add("G", goodsid);
            gs.Add("A", mn);
            gs.Add("E", goodsid);
            gs.Add("H", 1);

            List<Variant> goodsList = new List<Variant>();
            if (mn > 0)
            {
                goodsList.Add(gs);
            }
            else
            {
                goodsList = null;
            }
            int reTime = Convert.ToInt32(TipManager.GetMessage(EmailReturn.HameDay));
            if (EmailAccess.Instance.SendEmail(title, TipManager.GetMessage(PetsReturn.StealPet12), pb.ID, pb.Name, content, string.Empty, goodsList, reTime))
            {
                if (pb.Online)
                {
                    pb.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(pb.ID));
                }
            }            
        }

        /// <summary>
        /// 在宠物上使用捕捉网的记录
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="petid"></param>
        private static void StealLog(PlayerBusiness pb, string petid)
        {
            Variant sts = pb.Home.Value.GetVariantOrDefault("StealLog");
            if (sts == null)
            {
                sts = new Variant();
                pb.Home.Value["StealLog"] = sts;
            }
            sts.SetOrInc(petid, 1);
            pb.Home.Save();
        }

        /// <summary>
        /// 宠物保护
        /// </summary>
        /// <param name="note"></param>
        public static void PetProtection(UserNote note)
        {
            string petid = note.GetString(0);
            PlayerEx b2 = note.Player.B2;
            IList c = b2.Value.GetValue<IList>("C");
            Variant v = null;
            foreach (Variant k in c)
            {
                if (k.GetStringOrDefault("E") == petid)
                {
                    v = k;
                    break;
                }
            }
            if (v == null)
            {
                note.Call(PetsCommand.PetProtectionR, false, TipManager.GetMessage(PetsReturn.NoExists));
                return;
            }

            string goodsid = "G_d000750";


            GameConfig gc = GameConfigAccess.Instance.FindOneById(goodsid);
            if (gc == null)
            {
                note.Call(PetsCommand.PetProtectionR, false, TipManager.GetMessage(PetsReturn.PetProtection1));
                return;
            }

            if (!note.Player.RemoveGoods(goodsid, GoodsSource.PetProtection))
            {
                note.Call(PetsCommand.PetProtectionR, false, TipManager.GetMessage(PetsReturn.PetProtection1));
                return;
            }
            Variant t = v.GetValueOrDefault<Variant>("T");
            if (t == null)
            {
                Variant pt = new Variant();
                t.Add("ProtectionTime", DateTime.UtcNow);
                v["T"] = pt;
                t = v["T"] as Variant;
            }
            if (t.ContainsKey("ProtectionTime"))
            {
                t["ProtectionTime"] = t.GetDateTimeOrDefault("ProtectionTime").AddDays(7);
            }
            else
            {
                t.Add("ProtectionTime", DateTime.UtcNow.AddDays(7));
            }
            b2.Save();
            note.Player.UpdateBurden("B2");
            note.Call(PetsCommand.PetProtectionR, true, PetAccess.Instance.GetPetModel(v));
        }

        /// <summary>
        /// 宠物赠送
        /// </summary>
        /// <param name="note"></param>
        public static void PetPresent(UserNote note)
        {
            string id = note.GetString(0);
            PlayerBusiness pb = PlayersProxy.FindPlayerByID(id);
            if (pb == null)
            {
                note.Call(PetsCommand.PetPresentR, false, TipManager.GetMessage(PetsReturn.PetPresent1), "");
                return;
            }
            PlayerEx b2 = pb.B2;
            Variant v2 = b2.Value;
            IList c2 = v2.GetValue<IList>("C");
            if (c2 == null)
            {
                note.Call(PetsCommand.PetPresentR, false, TipManager.GetMessage(PetsReturn.PetPresent1), "");
                return;
            }
            Variant t2 = null;
            foreach (Variant k in c2)
            {
                if (string.IsNullOrEmpty(k.GetStringOrDefault("E")))
                {
                    t2 = k;
                    break;
                }
            }
            if (t2 == null)
            {
                note.Call(PetsCommand.PetPresentR, false, string.Format(TipManager.GetMessage(PetsReturn.PetPresent2), pb.Name));                
                return;
            }

            string petid = note.GetString(1);


            PlayerEx b3 = note.Player.B3;
            Variant v3 = b3.Value;
            IList c3 = v3.GetValue<IList>("C");
            Variant t3 = null;
            foreach (Variant k in c3)
            {
                if (k.GetStringOrDefault("E") == petid)
                {
                    t3 = k;
                    break;
                }
            }

            if (t3 == null)
            {
                note.Call(PetsCommand.PetPresentR, false, TipManager.GetMessage(PetsReturn.NoExists), "");
                return;
            }

            Pet p = PetAccess.Instance.GetPetByID(petid, note.PlayerID);
            if (p == null)
            {
                note.Call(PetsCommand.PetPresentR, false, TipManager.GetMessage(PetsReturn.NoExists), "");
                return;
            }
            //宠物盒子
            string goodsid = "G_d001002";
            if (!note.Player.RemoveGoods(goodsid, GoodsSource.PetPresent))
            {
                note.Call(PetsCommand.PetPresentR, false, TipManager.GetMessage(PetsReturn.PetPresent3), "");
                return;
            }

            p.Value["FeedCount"] = 0;
            //宠物技能清理
            SkillClear(p);

            t2["E"] = t3["E"];
            t2["G"] = t3["G"];
            t2["S"] = t3["S"];
            t2["H"] = t3["H"];
            t2["A"] = t3["A"];
            t2["D"] = t3["D"];
            t2["R"] = t3["R"];
            BurdenManager.BurdenClear(t3);
            //放养得到奖励
            Variant ct = PetAccess.Instance.CreateAward(pb.Level, p.ID, note.PlayerID, pb.Pet);                       
            t2["T"] = ct;

            b2.Save();
            b3.Save();
            p.PlayerID = pb.ID;
            p.Save();

            string msg = string.Format(TipManager.GetMessage(PetsReturn.PetPresent4), DateTime.Now, note.Player.Name, p.Name);
            pb.AddBoard(msg);
            note.Call(PetsCommand.PetPresentR, true, note.PlayerID, PetAccess.Instance.GetPetModel(t2));
            if (pb.Online)
            {
                pb.Call(PetsCommand.PetPresentR, true, note.PlayerID, PetAccess.Instance.GetPetModel(t2));
            }            
        }

        /// <summary>
        /// 清理宠物装备的技能
        /// </summary>
        /// <param name="p"></param>
        public static void SkillClear(Pet p)
        {
            IList skillGroove = p.Value.GetValue<IList>("SkillGroove");
            if (skillGroove == null) 
                return;

            Variant skill = p.Value.GetVariantOrDefault("Skill");
            foreach (Variant sk in skillGroove)
            {
                int born = sk.GetIntOrDefault("Born");
                if (born == 0 || born == 2)
                    continue;
                string skillid = sk.GetStringOrDefault("SkillID");
                if (skillid != "-1")
                {
                    if (skill != null)
                    {
                        //移除所有非天生技能
                        if (skill.ContainsKey(skillid))
                        {
                            skill.Remove(skillid);
                        }
                    }
                    sk["SkillID"] = "0";
                    sk["MaxUse"] = 0;
                    sk["Key"] = string.Empty;
                    sk["Level"] = 0;
                    sk["SkillName"] = string.Empty;
                    sk["Born"] = 0;
                }
            }
        }
        /// <summary>
        /// 初始忠诚度
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static int Loyalty(Pet p)
        {
            Variant v = p.Value;
            //家政总值
            int total = v.GetIntOrDefault("YaoJi") + v.GetIntOrDefault("JuanZhou") + v.GetIntOrDefault("JiaGong") + v.GetIntOrDefault("CaiJi");
            int feed = v.GetIntOrDefault("FeedCount");
            decimal d = total / 6 + feed / 2;
            return Convert.ToInt32(Math.Ceiling(d));
        }

        /// <summary>
        /// 诱宠成功率
        /// </summary>
        /// <param name="p">被诱宠物</param>
        /// <param name="number">投入成长果实个数</param>
        /// <returns></returns>
        private static double StealLv(Pet p, int number)
        {
            Variant v = p.Value;
            //投放果实数            
            int zz = v.GetIntOrDefault("ZiZhi");
            string petsid = v.GetStringOrDefault("PetsID");
            GameConfig gc = GameConfigAccess.Instance.FindOneById(petsid);
            if (gc == null)
                return 0;
            //是否可以进化
            int isRank = gc.Value.GetIntOrDefault("IsRank");
            double a = number * 5 + (100 - Loyalty(p));
            double b = (1000 * (isRank + zz) + 1200);
            double d = Math.Round(a / b, 4);
            return (Math.Round(d, 4) > 0.8) ? 0.8 : Math.Round(d, 4);

            //return NumberRandom.RandomHit(d);
        }
    }
}
