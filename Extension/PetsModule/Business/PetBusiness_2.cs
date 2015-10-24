using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Extensions;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Util;
using Sinan.Log;
using MongoDB.Bson;

#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.PetsModule.Business
{
    partial class PetBusiness
    {
        static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 一键照顾
        /// </summary>
        /// <param name="note"></param>
        public static void StockingAll(UserNote note)
        {
            string soleid = note.PlayerID + "StockingAll";
            if (!m_dic.TryAdd(soleid, soleid))
            {
                return;
            }
            try
            {
                PlayerBusiness player = note.Player;
                PlayerEx b2 = player.B2;
                IList list = b2.Value.GetValue<IList>("C");

                int score = 0;//石币
                int p1 = 0;//角色经验
                int p2 = 0;//出战宠经验
                bool isstock = false;//是否可以
                string id = ObjectId.GenerateNewId().ToString();
                Variant info = new Variant();

                foreach (Variant v in list)
                {
                    if (string.IsNullOrEmpty(v.GetStringOrDefault("E")))
                        continue;
                    //可以得到的奖励
                    Variant award = v.GetValueOrDefault<Variant>("T");
                    if (award == null)
                        continue;
                    // TODO:是否到领奖时间

                    DateTime endTime = award.GetDateTimeOrDefault("EndTime", DateTime.MaxValue);
                    if (DateTime.UtcNow < endTime)
                        continue;

                    int Score = award.GetIntOrDefault("Score");
                    int P1exp = award.GetIntOrDefault("P1exp");
                    int P2exp = award.GetIntOrDefault("P2exp");
                    int P3exp = award.GetIntOrDefault("P3exp");

                    score += Score;
                    p1 += P1exp;
                    p2 += P2exp;


                    Pet p = PetAccess.Instance.FindOneById(v.GetStringOrDefault("E"));
                    if (p == null)
                    {
                        BurdenManager.BurdenClear(v);
                        continue;
                    }
                    player.AddPetExp(p, P3exp, false, (int)FinanceType.StockingAward1);

                    Variant ct = PetAccess.Instance.CreateAward(player.Level, p.ID, player.ID, player.Pet);
                    Variant t = v.GetValueOrDefault<Variant>("T");
                    if (t.ContainsKey("ProtectionTime"))
                    {
                        if (ct == null) ct = new Variant();
                        ct.Add("ProtectionTime", t.GetDateTimeOrDefault("ProtectionTime"));
                    }

                    v["T"] = ct;
                    PetAccess.Instance.FatigueBack(p);
                    //p.Save();
                    isstock = true;

                    Variant va = new Variant();
                    va["Score"] = Score;
                    va["P1exp"] = P1exp;
                    va["P2exp"] = player.Pet != null ? P2exp : 0;
                    va["P3exp"] = P3exp;
                    va["PetsID"] = p.Value.GetStringOrDefault("PetsID");
                    va["ID"] = p.ID;
                    va["IsAll"] = id;//一键照顾                    
                    info[p.ID] = award;
                }

                if (isstock)
                {
                    if (score > 0)
                    {
                        player.AddScore(score, FinanceType.StockingAward2);
                    }
                    if (p1 > 0) player.AddExperience(p1, FinanceType.StockingAward2);
                    if (p2 > 0) player.AddPetExp(player.Pet, p2, true, (int)FinanceType.StockingAward2);
                    b2.Save();

                    Variant tmp = new Variant(3);
                    tmp.Add("P1exp", p1);
                    tmp.Add("P2exp", p2);
                    tmp.Add("Score", score);

                    note.Call(PetsCommand.StockingAllR, true, tmp);
                    player.AddAcivity(ActivityType.ZhaoGu, 1);
                }
                else
                {
                    note.Call(PetsCommand.StockingAllR, false, TipManager.GetMessage(PetsReturn.StockingAll1));
                }

                foreach (var item in info)
                {
                    Variant os = item.Value as Variant;
                    player.AddLogVariant(Actiontype.Stocking, null, null, os);
                }
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 得到宠物基本属性值
        /// </summary>
        /// <param name="note"></param>
        public static void PetProperty(UserNote note)
        {
            string id = note.GetString(0);
            Pet p = PetAccess.Instance.FindOneById(id);
            if (p == null)
            {
                note.Call(PetsCommand.PetPropertyR, false, TipManager.GetMessage(PetsReturn.NoExists));
                return;
            }
            IList list = note[1] as IList;
            Variant v = new Variant();
            v.Add("ID", p.ID);
            if (list.Contains("Name"))
            {
                v.Add("Name", p.Name);
            }
            if (list.Contains("PlayerID"))
            {
                v.Add("PlayerID", p.PlayerID);
            }
            foreach (string m in list)
            {
                if (m == "Name" || m == "PlayerID")
                    continue;
                if (p.Value.ContainsKey(m))
                {
                    v[m] = p.Value[m];
                }
            }
            note.Call(PetsCommand.PetPropertyR, true, v);
        }

        /// <summary>
        /// 通过等级得到宠物最大疲劳值
        /// </summary>
        /// <param name="level">宠物等级</param>
        /// <returns></returns>
        public static int MaxFatigue(int level)
        {
            if (level > 0 && level <= 96)
            {
                return 400 + level * 100;
            }
            else if (level > 96)
            {
                return 10000 + (level - 96) * 500;
            }
            return 0;
        }

        /// <summary>
        /// 一键喂养
        /// </summary>
        /// <param name="note"></param>
        public static void FeedPetsAll(UserNote note)
        {
            string soleid = note.PlayerID + "FeedPetsAll";
            if (!m_dic.TryAdd(soleid, soleid))
            {
                return;
            }

            try
            {
                string petid = note.GetString(0);
                Pet p = null;
                if (note.Player.Pet != null)
                {
                    if (note.Player.Pet.ID == petid)
                    {
                        p = note.Player.Pet;
                    }
                }

                if (p == null)
                {
                    p = PetAccess.Instance.FindOneById(petid);
                }

                if (p == null)
                {
                    note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.NoPets));
                    return;
                }

                Variant v = p.Value;
                Variant ccd = v.GetVariantOrDefault("ChengChangDu");
                //当前成长度
                int curMin = ccd.GetIntOrDefault("V");
                //允许最大成长度
                int curMax = ccd.GetIntOrDefault("M");

                if (curMin >= curMax)
                {
                    note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.KeepBig));
                    return;
                }

                string goodsid = "G_d000015";//成长果实
                PlayerEx burden = note.Player.Value["B0"] as PlayerEx;
                IList c = burden.Value.GetValue<IList>("C");
                //得到成长果实的总数量
                int count = BurdenManager.BurdenGoodsCount(c, goodsid);

                int number = 0;//需要成长果实数量
                int addcount = 0;//增加成长度点数
                for (int i = curMin + 1; i <= curMax; i++)
                {
                    int m = PetAccess.KeepNeedGoods(i);
                    if (number + m > count)
                        break;

                    number += m;
                    addcount++;
                }

                if (addcount <= 0 || number <= 0)
                {
                    note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.NoKeepGoods));
                    return;
                }

                if (note.Player.RemoveGoods(goodsid, number, GoodsSource.FeedPetsAll))
                {
                    ccd["V"] = ccd.GetIntOrDefault("V") + addcount;
                    //喂养成长果实数量
                    v.SetOrInc("FeedCount", number);

                    PetAccess.PetReset(p, note.Player.Skill, false,note.Player.Mounts);
                    burden.Save();

                    note.Player.UpdateBurden();
                    note.Call(PetsCommand.UpdatePetR, true, p);
                    note.Call(PetsCommand.FeedPetsR, true, 0);
                    return;
                }
                note.Call(PetsCommand.UpdatePetR, false, TipManager.GetMessage(PetsReturn.NoKeepGoods));
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }
    }
}
