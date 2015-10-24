using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Sinan.Command;
using Sinan.Core;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Schedule;
using Sinan.Util;
using Sinan.Log;

#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.HomeModule.Business
{
    public class HomeBusiness
    {
        static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 取得当前家园基本信息
        /// </summary>
        /// <param name="note"></param>
        public static void HomeInfo(UserNote note)
        {
            PlayerEx home = note.Player.Home;
            if (home == null)
                return;
            Variant mv = home.Value;
            if (mv == null)
                return;
            Variant v = new Variant();
            foreach (var item in mv)
            {
                v.Add(item.Key, item.Value);
            }
            note.Call(HomeCommand.HomeInfoR, v);
        }

        /// <summary>
        /// 宠物驯化
        /// </summary>
        /// <param name="note"></param>
        public static void HomePetKeep(UserNote note)
        {
            string id = note.GetString(0);

            string npcid = note.GetString(1);//NPC
            if (!note.Player.EffectActive(npcid, ""))
                return;

            //PetsWild宠物野性值,取得玩家家园信息
            PlayerEx model = note.Player.Home;
            Variant petKeep = model.Value.GetValueOrDefault<Variant>("PetKeep");

            if (petKeep["ID"].ToString() == string.Empty)
            {
                petKeep["ID"] = string.Empty;
                petKeep["PetID"] = string.Empty;
                petKeep["StartTime"] = string.Empty;
                petKeep["EndTime"] = string.Empty;
                petKeep["PetsWild"] = 0;
                petKeep["PetName"] = string.Empty;
                petKeep["PetsRank"] = 0;
            }

            if (petKeep.GetStringOrDefault("EndTime") != string.Empty)
            {
                note.Call(HomeCommand.HomePetKeepR, false, TipManager.GetMessage(HomeReturn.HomeKeeping));
                return;
            }

            //取得宠物包袱列表
            PlayerEx burden = note.Player.B0;
            IList c = burden.Value.GetValue<IList>("C");
            Variant tmp = null;
            foreach (Variant d in c)
            {
                if (d.GetStringOrDefault("E") == id)
                {
                    tmp = d;
                    break;
                }
            }
            if (id != petKeep.GetStringOrDefault("ID"))
            {
                if (tmp == null)
                {
                    note.Call(HomeCommand.HomePetKeepR, false, TipManager.GetMessage(HomeReturn.PetError));
                    return;
                }
            }

            Goods g = GoodsAccess.Instance.FindOneById(id);
            //得到宠物信息
            if (g == null)
            {
                note.Call(HomeCommand.HomePetKeepR, false, TipManager.GetMessage(HomeReturn.PetError));
                return;
            }
            int keepTime = g.Value.GetIntOrDefault("KeepTime");//驯化计算单位时间
            int keepCount = g.Value.GetIntOrDefault("KeepCount");//单位时间更新点数
            int score = g.Value.GetIntOrDefault("KeepScore");//单次驯化需要费用


            int PetsWild = g.Value.GetIntOrDefault("PetsWild");
            if (PetsWild <= 0)
            {
                note.Call(HomeCommand.HomePetKeepR, false, TipManager.GetMessage(HomeReturn.KeepFinish));
                return;
            }
            //判断驯化需要的费用
            if (note.Player.Score < score || (!note.Player.AddScore(-score, FinanceType.HomePetKeep)))
            {
                note.Call(HomeCommand.HomePetKeepR, false, TipManager.GetMessage(HomeReturn.KeepNoScoreB));
                return;
            }
            string goodsid = tmp.GetStringOrDefault("G");
            //清理包袱
            BurdenManager.BurdenClear(tmp);
            burden.Save();

            note.Player.UpdateTaskGoods(goodsid);

            int y = 0;
            int s = Math.DivRem(PetsWild, keepCount, out y);
            if (y > 0) s++;
            DateTime dt = DateTime.UtcNow;
            //得到驯化基本信息
            petKeep["ID"] = id;
            petKeep["PetID"] = g.GoodsID;//道具ID
            petKeep["StartTime"] = dt;
            petKeep["EndTime"] = dt.AddMilliseconds(s * keepTime);
            petKeep["PetsWild"] = PetsWild;
            petKeep["PetName"] = g.Name;
            petKeep["PetsRank"] = g.Value.GetIntOrDefault("PetsRank");
            model.Save();

            note.Player.UpdateBurden();
            note.Call(HomeCommand.HomePetKeepR, true, petKeep);
        }

        /// <summary>
        /// 终止宠物驯化
        /// </summary>
        /// <param name="note"></param>
        public static void HomeStopKeep(UserNote note)
        {
            string npcid = note.GetString(0);//NPC
            if (!note.Player.EffectActive(npcid, ""))
                return;

            PlayerEx model = note.Player.Home;//["Home"] as PlayerEx;
            Variant petKeep = model.Value.GetValueOrDefault<Variant>("PetKeep");
            DateTime dt = DateTime.UtcNow;
            DateTime StartTime;
            DateTime EndTime;

            if (!DateTime.TryParse(petKeep.GetStringOrDefault("StartTime"), out StartTime))
            {
                note.Call(HomeCommand.HomeStopKeepR, false, TipManager.GetMessage(HomeReturn.StopKeep));
                return;
            }

            if (!DateTime.TryParse(petKeep.GetStringOrDefault("EndTime"), out EndTime))
            {
                note.Call(HomeCommand.HomeStopKeepR, false, TipManager.GetMessage(HomeReturn.StopKeep));
                return;
            }

            Goods g = GoodsAccess.Instance.FindOneById(petKeep.GetStringOrDefault("ID"));
            if (g == null || g.Value == null)
            {
                return;
            }
            int keepTime = g.Value.GetIntOrDefault("KeepTime");//驯化计算单位时间
            int keepCount = g.Value.GetIntOrDefault("KeepCount");//单位时间更新点数
            int keepScore = g.Value.GetIntOrDefault("KeepScore");//单次驯化需要费用            
            if (dt >= EndTime)
            {
                g.Value["PetsWild"] = 0;
            }
            else
            {
                TimeSpan ts = dt - StartTime;
                //驯化的毫秒数
                int y = 0;
                int s = Math.DivRem(Convert.ToInt32(ts.TotalMilliseconds), keepTime, out y);
                //当前野性值
                int PetsWild = g.Value.GetIntOrDefault("PetsWild");
                g.Value["PetsWild"] = (s * keepCount >= PetsWild) ? 0 : (PetsWild - s * keepCount);
            }


            if (g.Value.GetIntOrDefault("PetsWild") == 0)
            {
                return;
            }
            else
            {
                PlayerEx burden = note.Player.B0;
                IList c = burden.Value.GetValue<IList>("C");
                Variant v = BurdenManager.GetBurdenSpace(c);
                if (v == null)
                {
                    note.Call(HomeCommand.HomeStopKeepR, false, TipManager.GetMessage(HomeReturn.BurdenFull));
                    return;
                }
                v["E"] = g.ID;
                v["G"] = g.GoodsID;
                v["S"] = g.Value.GetIntOrDefault("Sort");
                v["H"] = g.Value.GetIntOrDefault("IsBinding");
                v["D"] = 0;
                v["A"] = 1;
                Variant tmp = new Variant();
                tmp.Add("Created", g.Created);
                tmp.Add("PetsWild", g.Value.GetIntOrDefault("PetsWild"));
                v["T"] = tmp;
                PetKeepClear(petKeep);
                g.Save();
                burden.Save();
                model.Save();

                Variant bv = new Variant();
                bv.Add("B0", burden);
                note.Call(BurdenCommand.BurdenListR, bv);
                note.Call(HomeCommand.HomeStopKeepR, true, TipManager.GetMessage(HomeReturn.KeepStopSuccess));
            }
        }

        /// <summary>
        /// 清理
        /// </summary>
        /// <param name="v"></param>
        private static void PetKeepClear(Variant v)
        {
            v["ID"] = string.Empty;
            v["PetID"] = string.Empty;
            v["StartTime"] = string.Empty;
            v["EndTime"] = string.Empty;
            v["PetsWild"] = 0;
            v["PetName"] = string.Empty;
            v["PetsRank"] = 0;
        }

        /// <summary>
        /// 家园生产
        /// </summary>
        /// <param name="note"></param>
        public static void HomeProduce(UserNote note)
        {
            string soleid = note.PlayerID + "HomeProduce";
            if (!m_dic.TryAdd(soleid, soleid))
                return;

            try
            {

                string id = note.GetString(0);
                int number = note.GetInt32(1);

                string npcid = note.GetString(2);//NPC
                if (!note.Player.EffectActive(npcid, ""))
                    return;

                if (number <= 0) 
                {
                    note.Call(HomeCommand.HomeProduceR, false, TipManager.GetMessage(HomeReturn.PareError));
                    return;
                }
                //生产数量
                GameConfig gc = GameConfigAccess.Instance.FindOneById(id);
                if (gc == null)
                {
                    note.Call(HomeCommand.HomeProduceR, false, TipManager.GetMessage(HomeReturn.PareError));
                    return;
                }
                string subType = gc.SubType;
                PlayerEx home = note.Player.Home;
                if (home == null)
                {
                    note.Call(HomeCommand.HomeProduceR, false, TipManager.GetMessage(HomeReturn.NoHome));
                    return;
                }
                //得到需要的道具数量，计算出当前可以生产道具的数量

                Variant produces = home.Value.GetValueOrDefault<Variant>("Produces");
                if (produces == null)
                {
                    note.Call(HomeCommand.HomeProduceR, false, TipManager.GetMessage(HomeReturn.PareError));
                    return;
                }
                Variant v = produces.GetValueOrDefault<Variant>(subType);
                if (v == null)
                {
                    note.Call(HomeCommand.HomeProduceR, false, TipManager.GetMessage(HomeReturn.PareError));
                    return;
                }

                IList petList = v.GetValue<IList>("PetList");
                if (petList.Count == 0)
                {
                    note.Call(HomeCommand.HomeProduceR, false, TipManager.GetMessage(HomeReturn.InPet));
                    return;
                }

                int yaoJi = 0;
                int juanZhou = 0;
                int sheJi = 0;
                int jiaGong = 0;
                int caiJi = 0;
                int jiazhengtotal = 0;//宠物家政值总数

                Dictionary<string, Pet> producePet = new Dictionary<string, Pet>();
                foreach (Variant d in petList)
                {
                    yaoJi = yaoJi + d.GetIntOrDefault("YaoJi");
                    juanZhou = juanZhou + d.GetIntOrDefault("JuanZhou");
                    sheJi = sheJi + d.GetIntOrDefault("SheJi");
                    jiaGong = jiaGong + d.GetIntOrDefault("JiaGong");
                    caiJi = caiJi + d.GetIntOrDefault("CaiJi");
                }
                jiazhengtotal = yaoJi + juanZhou + jiaGong + caiJi;

                Dictionary<string, int> dic = new Dictionary<string, int>();
                dic.Add("YaoJi", yaoJi);
                dic.Add("JuanZhou", juanZhou);
                dic.Add("SheJi", sheJi);
                dic.Add("JiaGong", jiaGong);
                dic.Add("CaiJi", caiJi);
                //目标道具
                string goodsid = gc.Value.GetStringOrDefault("GoodsID");
                //生产所需时间,秒钟
                int producetime = gc.Value.GetIntOrDefault("ProduceTime");
                //所需家政信息
                Variant homeInfo = gc.Value.GetValueOrDefault<Variant>("HomeValue");
                //一条生产线可生产最大数量
                int produceMax = gc.Value.GetIntOrDefault("Number");
                if (number > produceMax)
                {
                    note.Call(HomeCommand.HomeProduceR, false, string.Format(TipManager.GetMessage(HomeReturn.HomeProduce5),produceMax));
                    return;
                }

                int need = 0;
                foreach (var item in homeInfo)
                {
                    if (!dic.ContainsKey(item.Key))
                    {
                        note.Call(HomeCommand.HomeProduceR, false, TipManager.GetMessage(HomeReturn.ConfigError));
                        return;
                    }
                    if (dic[item.Key] < (int)item.Value)
                    {
                        note.Call(HomeCommand.HomeProduceR, false, TipManager.GetMessage(HomeReturn.PetHomeNo));
                        return;
                    }
                    need += Convert.ToInt32(item.Value);
                }
                //所需物品
                IList needGoods = gc.Value.GetValue<IList>("NeedGoods");
                PlayerEx b0 = note.Player.B0;
                //需要的物品数量
                Dictionary<string, int> goods = new Dictionary<string, int>();
                int goodsTotal = 0;
                foreach (Variant ng in needGoods)
                {
                    int total = ng.GetIntOrDefault("Number") * number;
                    string gs = ng.GetStringOrDefault("GoodsID");
                    if (BurdenManager.GoodsCount(b0, gs) < total)
                    {
                        note.Call(HomeCommand.HomeProduceR, false, TipManager.GetMessage(HomeReturn.GoodsNumberNo));
                        return;
                    }
                    if (goods.ContainsKey(gs))
                    {
                        goods[gs] += total;
                    }
                    else
                    {
                        goods.Add(gs, total);
                    }
                    
                    if (gs == "G_d000389" || gs == "G_d000390" || gs == "G_d000391")
                    {
                        goodsTotal += total;
                    }
                }

                int isbinding = 0;
                foreach (var item in goods) 
                {
                    isbinding=BurdenManager.IsBinding(b0, item.Key, (int)item.Value);                    
                    if (isbinding == 1)
                        break;
                }

                Dictionary<string, Variant> info = new Dictionary<string, Variant>();
                Variant vg = new Variant();
                vg.Add("Number" + isbinding, number);
                info.Add(goodsid, vg);

                if (BurdenManager.IsFullBurden(b0, info))
                {
                    note.Call(HomeCommand.HomeProduceR, false, TipManager.GetMessage(HomeReturn.BurdenFull));
                    return;
                }
                DateTime dt=DateTime.UtcNow;

                if (v.ContainsKey("ProduceList")) 
                {
                    Variant mv = MemberAccess.MemberInfo(note.Player.MLevel);
                    int produce = mv.GetIntOrDefault("Produce");//生产对列数
                    
                    IList r = v.GetValue<IList>("ProduceList");
                    if (r.Count > 0)
                    {
                        for (int i = r.Count - 1; i >=0; i--)
                        {
                            DateTime t = ((DateTime)r[i]).ToUniversalTime();
                            if (t < dt)
                            {
                                //移除过期对列
                                r.Remove(r[i]);
                            }
                        }
                    }

                    if (r.Count >= produce) 
                    {
                        note.Call(HomeCommand.HomeProduceR, false, TipManager.GetMessage(HomeReturn.HomeProduce2));
                        return;
                    }
                }
                int totaltime = producetime * number;
                if (v.ContainsKey("ProduceList"))
                {
                    IList pl = v.GetValue<IList>("ProduceList");
                    pl.Add(dt.AddSeconds(totaltime));
                }
                else 
                {
                    v.Add("ProduceList", new List<DateTime>() { dt.AddSeconds(totaltime) });
                }
                int b = 0;
                BurdenManager.Remove(b0, goods, out b);

                Variant us = new Variant();
                foreach (var item in goods)
                {
                    us.SetOrInc(item.Key,Convert.ToInt32(item.Value));
                    //生产消耗
                    note.Player.AddLog(Actiontype.GoodsUse, item.Key, Convert.ToInt32(item.Value), GoodsSource.HomeProduce, goodsid, number);
                    note.Player.UpdateTaskGoods(item.Key);
                }

                //判断以前是否正在生产
                //string gid = v.GetStringOrDefault("GoodsID");
                //int num = v.GetIntOrDefault("Number");
                //if (num > 0) 
                //{
                //    GameConfig gcm = GameConfigAccess.Instance.FindOneById(gid);
                //    if (gcm != null)
                //    {
                //        string title = TipManager.GetMessage(HomeReturn.HomeProduce3);
                //        string content = string.Format(TipManager.GetMessage(HomeReturn.HomeProduce4), gcm.Name, num);
                //        Variant gs = new Variant();
                //        gs.Add("G", gcm.ID);
                //        gs.Add("A", num);
                //        gs.Add("E", gcm.ID);
                //        gs.Add("H", v.GetIntOrDefault("IsBinding"));

                //        List<Variant> goodsList = new List<Variant>();
                //        if (number > 0)
                //        {
                //            goodsList.Add(gs);
                //        }
                //        else
                //        {
                //            goodsList = null;
                //        }
                //        int reTime = Convert.ToInt32(TipManager.GetMessage(EmailReturn.HameDay));
                //        if (EmailAccess.Instance.SendEmail(title, TipManager.GetMessage(PetsReturn.StealPet12), note.PlayerID, note.Player.Name, content, string.Empty, goodsList, reTime))
                //        {
                //            note.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(note.PlayerID));
                //        }
                //        v["GoodsID"] = "";
                //        v["Number"] = 0;
                //    }
                //}

                Variant gs1 = new Variant();
                if (home.Save())
                {
                    gs1[goodsid] = number;
                    note.Player.AddGoods(info, GoodsSource.HomeProduce);

                    Variant tmp = new Variant();
                    tmp.Add(subType, v);
                    note.Call(HomeCommand.HomeProduceR, true, tmp);
                    Variant os=new Variant();
                    os["TotalTime"] = totaltime;
                    note.Player.AddLogVariant(Actiontype.HomeProduce, us, gs1, os);
                }
                else 
                {
                    note.Call(HomeCommand.HomeProduceR, false, "");
                }
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 宠物进入工作房
        /// </summary>
        /// <param name="note"></param>
        public static void InPet(UserNote note)
        {
            IList boby = note.Body as IList;
            if (boby == null)
                return;
            //LianJinShi炼金房,MuGongFang木工房,
            //ShuFang养殖,JiaGongFang挖掘,GuoYuan采集
            List<string> list = new List<string>() { "LianJinShi", "ShuFang", "JiaGongFang", "MuGongFang", "GuoYuan" };
            string name = note.GetString(0);
            string id = note.GetString(1);//宠物ID

            string npcid = note.GetString(4);//NPC
            if (!note.Player.EffectActive(npcid, ""))
                return;

            if (!list.Contains(name))
            {
                note.Call(HomeCommand.InPetR, false, TipManager.GetMessage(HomeReturn.PareError));
                return;
            }
            PlayerEx home = note.Player.Home;
            if (home == null)
            {
                note.Call(HomeCommand.InPetR, false, TipManager.GetMessage(HomeReturn.NoHome));
                return;
            }

            Variant produces = home.Value.GetValueOrDefault<Variant>("Produces");

            if (!produces.ContainsKey(name))
            {
                note.Call(HomeCommand.InPetR, false, TipManager.GetMessage(HomeReturn.PareError));
                return;
            }

            Variant v = produces[name] as Variant;
            //允许宠物进入数量
            //int PetCount = v.GetIntOrDefault("PetCount");

            //允许宠物进入数量
            int PetCount = v.GetIntOrDefault("PetCount");
            if (name == "ShuFang" || name == "JiaGongFang" || name == "GuoYuan")
            {
                PetCount = 1;
            }

            IList petList = v.GetValue<IList>("PetList");
            if (petList.Count >= PetCount)
            {
                note.Call(HomeCommand.InPetR, false, TipManager.GetMessage(HomeReturn.PetNumberLimit));
                return;
            }
            
            Pet p = PetAccess.Instance.FindOneById(id);
            if (p == null || p.PlayerID != note.PlayerID)
            {
                note.Call(HomeCommand.InPetR, false, TipManager.GetMessage(HomeReturn.PetError));
                return;
            }
            string bType = "B3";
            if (boby.Count > 2)
            {
                bType = note.GetString(2);
            }
            if (bType != "B2" && bType != "B3")
                return;


            //取得宠物
            PlayerEx pg = bType == "B3" ? note.Player.B3 : note.Player.B2;

            IList c = pg.Value.GetValue<IList>("C");
            Variant tmp = null;
            foreach (Variant d in c)
            {
                if (d.GetStringOrDefault("E") == id)
                {
                    tmp = d;
                    break;
                }
            }
            if (tmp == null)
            {
                note.Call(HomeCommand.InPetR, false, TipManager.GetMessage(HomeReturn.PetError));
                return;
            }
            if (tmp.GetIntOrDefault("I") == 1)
            {
                note.Call(HomeCommand.InPetR, false, TipManager.GetMessage(HomeReturn.PetFighting));
                return;
            }

            BurdenManager.BurdenClear(tmp);
            DateTime dt = DateTime.UtcNow;

            Variant mv = MemberAccess.MemberInfo(note.Player.MLevel);
            if (mv == null)
            {
                note.Call(HomeCommand.InPetR, false, TipManager.GetMessage(HomeReturn.PareError));
                return;
            }

            v["StartTime"] = dt;
            v["EndTime"] = dt.AddHours(mv.GetIntOrDefault("HomeHour"));  
            

            Variant vn = new Variant();
            vn.Add("ID", id);
            vn.Add("PetID", p.Value.GetStringOrDefault("PetsID"));
            vn.Add("PetName", p.Name);
            vn.Add("YaoJi", p.Value.GetIntOrDefault("YaoJi"));
            vn.Add("JuanZhou", p.Value.GetIntOrDefault("JuanZhou"));
            vn.Add("SheJi", p.Value.GetIntOrDefault("SheJi"));
            vn.Add("JiaGong", p.Value.GetIntOrDefault("JiaGong"));
            vn.Add("CaiJi", p.Value.GetIntOrDefault("CaiJi"));
            vn.Add("DateTime", dt);
            vn.Add("PetsRank", p.Value.GetIntOrDefault("PetsRank"));
            petList.Add(vn);

            pg.Save();
            home.Save();
            p.Save();
            note.Player.UpdateBurden(bType);
            if (bType == "B3")
            {
                if (tmp.GetIntOrDefault("P") > 3)
                {
                    List<Variant> ps = PlayerExAccess.Instance.SlipPets(note.Player.B3);
                    note.Player.GetSlipPets(ps);
                }
            }
            note.Call(HomeCommand.InPetR, true, id);
            //Variant os=new Variant();
            //note.Player.AddLogVariant(Actiontype.HomeProduce, null, null, 0, 0, 0, 0, 0, os);
        }

        /// <summary>
        /// ShuFang养殖,JiaGongFang挖掘,GuoYuan采集
        /// </summary>
        /// <param name="note"></param>
        public static void HomePluck(UserNote note)
        {
            string soleid = note.PlayerID + "HomePluck";

            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                //G_d000015成长果实,G_d000016潜力果实  Minute;          
                int minute = note.GetInt32(0);
                string name = note.GetString(1);


                string npcid = note.GetString(2);//NPC
                if (!note.Player.EffectActive(npcid, ""))
                    return;

                if (minute < 1)
                {
                    note.Call(HomeCommand.HomePluckR, false, TipManager.GetMessage(HomeReturn.PareError));
                    return;
                }

                GameConfig gc = null;
                if (name == "GuoYuan")
                {
                    gc = GameConfigAccess.Instance.FindOneById("Home_CJ00002");
                }
                else if (name == "ShuFang")
                {
                    gc = GameConfigAccess.Instance.FindOneById("Home_CJ00001");
                }
                else if (name == "JiaGongFang")
                {
                    gc = GameConfigAccess.Instance.FindOneById("Home_CJ00003");
                }
                else
                {
                    note.Call(HomeCommand.HomePluckR, false, TipManager.GetMessage(HomeReturn.PareError));
                    return;
                }

                List<string> list = new List<string>() { "GuoYuan", "ShuFang", "JiaGongFang" };
                if (!list.Contains(gc.SubType))
                {
                    note.Call(HomeCommand.HomePluckR, false, TipManager.GetMessage(HomeReturn.PareError));
                    return;
                }

                string goodsID = gc.Value.GetStringOrDefault("GoodsID");
                int produceTime = gc.Value.GetIntOrDefault("ProduceTime");
                //int score = gc.Value.GetIntOrDefault("Score");
                //采集需要石币
                //int scoreT = ((minute * 60) / produceTime) * score;

                PlayerEx home = note.Player.Home;
                if (home == null)
                {
                    note.Call(HomeCommand.HomePluckR, false, TipManager.GetMessage(HomeReturn.NoHome));
                    return;
                }
                Variant produces = home.Value.GetValueOrDefault<Variant>("Produces");
                Variant v = produces.GetValueOrDefault<Variant>(gc.SubType);
                IList petList = v.GetValue<IList>("PetList");
                if (petList == null || petList.Count == 0)
                {
                    note.Call(HomeCommand.HomePluckR, false, TipManager.GetMessage(HomeReturn.InPet));
                    return;
                }


                Variant mv = MemberAccess.MemberInfo(note.Player.MLevel);
                if (mv == null)
                {
                    note.Call(HomeCommand.HomePluckR, false, TipManager.GetMessage(HomeReturn.ConfigError));
                    return;
                }
                int homeHour = mv.GetIntOrDefault("HomeHour");
                DateTime dt = DateTime.UtcNow;
                DateTime endtime = dt.AddHours(homeHour);

                //v["GoodsID"] = goodsID;
                //v["Number"] = 0;
                v["StartTime"] = dt;
                v["EndTime"] = endtime;
                //v["IsBinding"] = 1;
                home.Save();
                note.Call(HomeCommand.HomePluckR, true, TipManager.GetMessage(HomeReturn.Plucking));
            }

            finally
            {
                string str;
                m_dic.TryRemove(soleid, out str);
            }
        }


        /// <summary>
        /// 收集完成产品
        /// </summary>
        /// <param name="note"></param>
        public static void Collection(UserNote note)
        {
            string soleid = note.PlayerID + "Collection";

            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                //收集那种生产的物品
                string name = note.GetString(0);

                string npcid = note.GetString(1);//NPC
                if (!note.Player.EffectActive(npcid, ""))
                    return;

                PlayerEx home = note.Player.Home;
                if (home == null)
                {
                    note.Call(HomeCommand.CollectionR, false, TipManager.GetMessage(HomeReturn.NoHome));
                    return;
                }
                Variant produces = home.Value.GetValueOrDefault<Variant>("Produces");
                Variant v = produces[name] as Variant;
                if (v == null)
                {
                    note.Call(HomeCommand.CollectionR, false, TipManager.GetMessage(HomeReturn.NoProduce));
                    return;
                }
                //宠物列表
                IList petList = v.GetValue<IList>("PetList");
                if (petList == null || petList.Count == 0)
                {
                    note.Call(HomeCommand.CollectionR, false, TipManager.GetMessage(HomeReturn.PareError));
                    return;
                }

                string key = "";
                if (name == "GuoYuan")
                {
                    key="CaiJi";//采集
                }
                else if (name == "ShuFang")
                {
                    key = "JuanZhou";//卷轴,养殖
                }
                else if (name == "JiaGongFang")
                {
                    key = "JiaGong";//加工,挖掘
                }
                else
                {
                    note.Call(HomeCommand.CollectionR, false, TipManager.GetMessage(HomeReturn.PareError));
                    return;
                }

                GameConfig gc = null;
                if (name == "GuoYuan")
                {
                    gc = GameConfigAccess.Instance.FindOneById("Home_CJ00002");
                }
                else if (name == "ShuFang")
                {
                    gc = GameConfigAccess.Instance.FindOneById("Home_CJ00001");
                }
                else if (name == "JiaGongFang")
                {
                    gc = GameConfigAccess.Instance.FindOneById("Home_CJ00003");
                }
                else
                {
                    note.Call(HomeCommand.HomePluckR, false, TipManager.GetMessage(HomeReturn.PareError));
                    return;
                }
                //string goodsID = gc.Value.GetStringOrDefault("GoodsID");
                string goodsid = gc.Value.GetStringOrDefault("GoodsID");
                //string goodsid = v.GetStringOrDefault("GoodsID");
                if (string.IsNullOrEmpty(goodsid))
                {
                    note.Call(HomeCommand.CollectionR, false, TipManager.GetMessage(HomeReturn.NoProduce));
                    return;
                }

                //开始时间
                DateTime startTime = v.GetDateTimeOrDefault("StartTime");
                //结束时间
                DateTime endTime = v.GetDateTimeOrDefault("EndTime");
                //当前时间
                DateTime dt = DateTime.UtcNow;

                 Variant pm = petList[0] as Variant;
                int zv = pm.GetIntOrDefault(key);

                //得到总供生产秒数
                int t=0;
                if (dt >= endTime)
                {
                    t = Convert.ToInt32((endTime - startTime).TotalSeconds);
                }
                else
                {
                    t = Convert.ToInt32((dt - startTime).TotalSeconds);
                }
                //表示如果超过2小时只能算2小时
                //t = t > 7200 ? 7200 : t;
                //技算得到道具数量
                int number = Convert.ToInt32(HomeNumber(zv) * t / 7200);
                if (number <= 0)
                {
                    note.Call(HomeCommand.CollectionR, false, TipManager.GetMessage(HomeReturn.ProduceNoFinish));
                    return;
                }
                
                PlayerEx burden = note.Player.B0;
                Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
                Variant tmp = new Variant();
                tmp.Add("Number1", number);
                dic.Add(goodsid, tmp);

                if (BurdenManager.IsFullBurden(burden, dic))
                {
                    note.Call(HomeCommand.CollectionR, false, TipManager.GetMessage(HomeReturn.BurdenFull));
                    return;
                }
                note.Player.AddGoods(dic, GoodsSource.Collection);

                Variant mv = MemberAccess.MemberInfo(note.Player.MLevel);
                if (mv == null)
                {
                    note.Call(HomeCommand.CollectionR, false, TipManager.GetMessage(HomeReturn.PareError));
                    return;
                }
                v["StartTime"] = dt;
                v["EndTime"] = dt.AddHours(mv.GetIntOrDefault("HomeHour"));  
                //v["EndTime"] = dt.AddHours(2);                                              
                home.Save();
                note.Call(HomeCommand.CollectionR, true, TipManager.GetMessage(HomeReturn.CollectionSuccess));
                note.Player.AddAcivity(ActivityType.ShengChan, 1);

                Variant gs1 = new Variant();
                gs1[goodsid] = number;

                Variant os = new Variant();
                os["StartTime"] = startTime;
                os["EndTime"] = endTime;
                os["PetsID"] = pm.GetStringOrDefault("PetID");
                os["ID"] = pm.GetStringOrDefault("ID");
                os["Number"] = zv;
                os["Name"] = key;
                os["IsHand"] = 1;//手动
                note.Player.AddLogVariant(Actiontype.JiaZheng, null, gs1, os);
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }
  
        /// <summary>
        /// 取回宠物
        /// </summary>
        /// <param name="note"></param>
        public static void PetRetrieve(UserNote note)
        {
            string soleid = note.PlayerID + "PetRetrieve";
            if (!m_dic.TryAdd(soleid, soleid))
            {
                return;
            }
            try
            {
                IList boby = note.Body as IList;
                if (boby == null)
                    return;
                string id = note.GetString(0);
                //0表示产生宠物,1表示采集,生产取出宠物,2表示训化完成后取回宠物
                int goodsType = note.GetInt32(1);
                string bType = "B3";
                if (boby.Count > 2)
                {
                    bType = note.GetString(2);
                }

                if (bType != "B2" && bType != "B3")
                    return;

                string npcid = note.GetString(3);//NPC
                if (!note.Player.EffectActive(npcid, ""))
                    return;

                //驯化取回
                PlayerEx home = note.Player.Home;
                if (goodsType == 0)
                {
                    #region 将道具变成宠物的过程
                    Variant pk = home.Value.GetValueOrDefault<Variant>("PetKeep");
                    string goodsid = pk.GetStringOrDefault("ID");
                    Goods g = GoodsAccess.Instance.FindOneById(goodsid);
                    if (g == null)
                    {
                        note.Call(HomeCommand.PetRetrieveR, false, goodsType, TipManager.GetMessage(HomeReturn.ConfigError));
                        return;
                    }
                    if (goodsid == id)
                    {
                        if (pk.GetStringOrDefault("EndTime") != string.Empty)
                        {
                            DateTime endTime;
                            if (!DateTime.TryParse(pk.GetStringOrDefault("EndTime"), out endTime))
                            {
                                note.Call(HomeCommand.PetRetrieveR, false, goodsType, TipManager.GetMessage(HomeReturn.HomeKeeping));
                                return;
                            }
                            if (endTime > DateTime.UtcNow)
                            {
                                note.Call(HomeCommand.PetRetrieveR, false, goodsType, TipManager.GetMessage(HomeReturn.HomeKeeping));
                                return;
                            }
                        }

                        if (pk.GetIntOrDefault("PetsWild") == -1)
                        {
                            //宠物已经产生,不能重复产生
                            return;
                        }

                        string petid = g.Value.GetStringOrDefault("PetID");

                        // 得到宠物基本信息
                        GameConfig gc = GameConfigAccess.Instance.FindOneById(petid);
                        if (gc == null || gc.Value == null)
                        {
                            note.Call(HomeCommand.PetRetrieveR, false, goodsType, TipManager.GetMessage(HomeReturn.PetError));
                            return;
                        }
                        Variant model = gc.Value;
                        string petType = model.GetStringOrDefault("PetsType");
                        int petLevel = model.GetIntOrDefault("PetsLevel");

                        //宠物已经产生
                        Variant petInfo = PetAccess.CreateBase(gc, 0, g.Value.GetIntOrDefault("IsBinding"));
                        Pet p = new Pet();
                        p.ID = ObjectId.GenerateNewId().ToString();
                        p.Name = gc.Name;
                        p.Value = petInfo;
                        p.Modified = DateTime.UtcNow;
                        p.PlayerID = note.PlayerID;
                        p.Created = DateTime.UtcNow;
                        p.Save();

                        pk["ID"] = p.ID;
                        pk["PetID"] = petid;
                        pk["StartTime"] = string.Empty;
                        pk["EndTime"] = string.Empty;
                        pk["PetsWild"] = -1;
                        pk["PetName"] = gc.Name;
                        pk["PetsRank"] = gc.Value.GetIntOrDefault("PetsRank");
                        home.Save();
                        note.Call(HomeCommand.PetRetrieveR, true, goodsType, id);
                        //表示宠物驯化成功
                        note.Player.FinishNote(FinishCommand.PetJobFuHua, gc.Value.GetStringOrDefault("PetsType"));
                        //移除道具
                        GoodsAccess.Instance.Remove(g.ID, note.PlayerID);
                    }
                    return;
                    #endregion
                }
                else if (goodsType == 2)
                {
                    #region 表示取出训化成功的宠物
                    Variant pk = home.Value.GetValueOrDefault<Variant>("PetKeep");
                    if (pk.GetStringOrDefault("ID") != id)
                    {
                        //宠物ID不正确
                        note.Call(HomeCommand.PetRetrieveR, false, goodsType, TipManager.GetMessage(HomeReturn.PetError));
                        return;
                    }

                    Pet px = PetAccess.Instance.FindOneById(id);
                    if (px == null)
                    {
                        note.Call(HomeCommand.PetRetrieveR, false, goodsType, TipManager.GetMessage(HomeReturn.PetError));
                        return;
                    }
                    //宠物槽
                    PlayerEx pgx = bType == "B3" ? note.Player.B3 : note.Player.B2;
                    IList pcx = pgx.Value.GetValue<IList>("C");
                    Variant vx = null;
                    foreach (Variant k in pcx)
                    {
                        if (k.GetStringOrDefault("E") == string.Empty)
                        {
                            vx = k;
                            break;
                        }
                    }
                    if (vx == null)
                    {
                        note.Call(HomeCommand.PetRetrieveR, false, goodsType, TipManager.GetMessage(HomeReturn.PetBurdenFull));
                        return;
                    }

                    vx["E"] = px.ID;
                    vx["G"] = px.Value.GetStringOrDefault("PetsID");
                    vx["S"] = px.Value.GetIntOrDefault("Sort");
                    vx["H"] = px.Value.GetIntOrDefault("IsBinding");
                    vx["A"] = 1;
                    vx["D"] = 0;
                    vx["R"] = px.Value.GetIntOrDefault("PetsRank");
                    vx["I"] = 0;
                    if (bType == "B2")
                    {
                        Variant ct = PetAccess.Instance.CreateAward(note.Player.Level, id, note.PlayerID, note.Player.Pet);
                        vx["T"] = ct;
                    }

                    //宠物背包保存
                    pgx.Save();

                    pk["ID"] = string.Empty;
                    pk["PetID"] = string.Empty;
                    pk["StartTime"] = string.Empty;
                    pk["EndTime"] = string.Empty;
                    pk["PetsWild"] = 0;
                    pk["PetName"] = string.Empty;
                    pk["PetsRank"] = 0;
                    home.Save();
                    //更新宠物背包
                    //note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(note.Player.B3));
                    note.Player.UpdateBurden(bType);
                    note.Call(HomeCommand.PetRetrieveR, true, goodsType, id);
                    if (bType == "B3")
                    {
                        //溜宠格子内，宠物将给随
                        if (vx.GetIntOrDefault("P") > 3)
                        {
                            List<Variant> ps = PlayerExAccess.Instance.SlipPets(note.Player.B3);
                            note.Player.GetSlipPets(ps);
                        }
                    }
                    else
                    {
                        note.Call(PetsCommand.StockingR, true, true, PetAccess.Instance.GetPetModel(vx));
                    }
                    return;
                    #endregion
                }


                #region 取回采集，生产，合成的宠物
                Pet pet = PetAccess.Instance.FindOneById(id);
                if (pet == null)
                {
                    note.Call(HomeCommand.PetRetrieveR, false, goodsType, TipManager.GetMessage(HomeReturn.PetError));
                    return;
                }
                //宠物槽
                PlayerEx pg = bType == "B3" ? note.Player.B3 : note.Player.B2;
                IList pc = pg.Value.GetValue<IList>("C");
                Variant v = null;
                foreach (Variant k in pc)
                {
                    if (string.IsNullOrEmpty(k.GetStringOrDefault("E")))
                    {
                        v = k;
                        break;
                    }
                }
                if (v == null)
                {
                    note.Call(HomeCommand.PetRetrieveR, false, goodsType, TipManager.GetMessage(HomeReturn.PetBurdenFull));
                    return;
                }

                Variant produces = home.Value.GetValueOrDefault<Variant>("Produces");
                foreach (string k in produces.Keys)
                {
                    Variant p = produces[k] as Variant;
                    IList petList = p.GetValue<IList>("PetList");
                    for (int i = 0; i < petList.Count; i++)
                    {
                        Variant m = petList[i] as Variant;
                        if (m.GetStringOrDefault("ID") == id)
                        {
                            string keyType = "";
                            if (k == "GuoYuan")
                            {
                                keyType = "CaiJi";//采集
                            }
                            else if (k == "ShuFang")
                            {
                                keyType = "JuanZhou";//卷轴,养殖
                            }
                            else if (k == "JiaGongFang")
                            {
                                keyType = "JiaGongFang";//加工,挖掘
                            }
                            if (k != "LianJinShi")
                            {
                                if (string.IsNullOrEmpty(keyType))
                                    return;
                                int l = HomeNumber(m.GetIntOrDefault(keyType));
                                string st = p.GetStringOrDefault("StartTime");
                                string ed = p.GetStringOrDefault("EndTime");

                                if ((!string.IsNullOrEmpty(st)) && (!string.IsNullOrEmpty(ed)))
                                {
                                    DateTime startTime = p.GetDateTimeOrDefault("StartTime");
                                    DateTime endTime = p.GetDateTimeOrDefault("EndTime");
                                    DateTime dt = DateTime.UtcNow;

                                    int t = 0;//总时间
                                    if (endTime < dt)
                                    {
                                        t = Convert.ToInt32((endTime - startTime).TotalSeconds);
                                    }
                                    else
                                    {
                                        t = Convert.ToInt32((dt - startTime).TotalSeconds);
                                    }

                                    //技算得到道具数量
                                    int number = Convert.ToInt32(l * t / 7200);
                                    if (number > 0)
                                    {
                                        note.Call(HomeCommand.PetRetrieveR, false, goodsType,TipManager.GetMessage(HomeReturn.Produceing));
                                        return;
                                    }
                                }
                            }

                            v["E"] = pet.ID;
                            v["G"] = pet.Value.GetStringOrDefault("PetsID");
                            v["S"] = pet.Value.GetIntOrDefault("Sort");
                            v["D"] = 0;
                            v["R"] = pet.Value.GetIntOrDefault("PetsRank");
                            v["I"] = 0;
                            v["A"] = 1;
                            v["H"] = pet.Value.GetIntOrDefault("IsBinding");

                            if (bType == "B2")
                            {
                                v["T"] = PetAccess.Instance.CreateAward(note.Player.Level, id, note.PlayerID, note.Player.Pet);
                            }

                            petList.Remove(m);
                            home.Save();
                            pet.Save();
                            pg.Save();
                            if (bType == "B3")
                            {
                                //溜宠格子内，宠物将给随
                                if (v.GetIntOrDefault("P") > 3)
                                {
                                    List<Variant> ps = PlayerExAccess.Instance.SlipPets(note.Player.B3);
                                    note.Player.GetSlipPets(ps);
                                }
                            }
                            else
                            {
                                note.Call(PetsCommand.StockingR, true, true, PetAccess.Instance.GetPetModel(v));
                            }
                            note.Player.UpdateBurden(bType);
                            note.Call(HomeCommand.PetRetrieveR, true, goodsType, id);
                            return;
                        }
                    }
                }
                note.Call(HomeCommand.PetRetrieveR, false, TipManager.GetMessage(HomeReturn.PetError));
                #endregion
            }
            finally
            {                
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 宠物回收
        /// </summary>
        /// <param name="note"></param>
        public static void PetBack(UserNote note)
        {
            string soleid = note.PlayerID + "PetBack";

            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                IList list = note[0] as IList;

                string npcid = note.GetString(1);//NPC
                if (!note.Player.EffectActive(npcid, ""))
                    return;

                PlayerEx pg = note.Player.B3;
                IList c = pg.Value.GetValue<IList>("C");


                //回收的宠物列表
                List<Variant> pets = new List<Variant>();
                foreach (string id in list)
                {
                    if (id == string.Empty)
                    {
                        note.Call(HomeCommand.PetBackR, false, string.Empty);
                        return;
                    }
                    bool isHave = false;
                    foreach (Variant v in c)
                    {
                        if (v.GetStringOrDefault("E") == id)
                        {
                            isHave = true;
                            pets.Add(v);
                            break;
                        }
                    }
                    if (!isHave)
                    {
                        //表示有不存在的宠物
                        note.Call(HomeCommand.PetBackR, false, string.Empty);
                        return;
                    }
                }

                int score = 0;
                foreach (Variant v in pets)
                {
                    string id = v.GetStringOrDefault("E");
                    Pet p = PetAccess.Instance.FindOneById(id);
                    if (p != null)
                    {
                        string petid = p.Value.GetStringOrDefault("PetsID");
                        int level = p.Value.GetIntOrDefault("PetsLevel");
                        GameConfig gc = GameConfigAccess.Instance.FindOneById(petid);
                        if (gc != null)
                        {
                            score += gc.Value.GetIntOrDefault("BackScoreB");
                        }
                        BurdenManager.BurdenClear(v);
                        //移除
                        PetAccess.Instance.RemoveOneById(p.ID, MongoDB.Driver.SafeMode.False);
                        //宠物回收
                        note.Player.AddLog(Log.Actiontype.PetRemove, petid, level, GoodsSource.PetBack, p.ID, 3);
                    }
                }
                pg.Save();
                //得到游戏数量

                note.Player.AddScore(score, FinanceType.PetBack);
                note.Player.UpdateBurden("B3");
                note.Call(HomeCommand.PetBackR, true, string.Empty);  
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 操作家园信息,在角色登录或在线进行操作
        /// </summary>
        /// <param name="note"></param>
        public static void HomeInfoCall(UserNote note) 
        {
            HomeInfoCall(note.Player);
        }

        /// <summary>
        /// 操作家园信息,在角色登录或在线进行操作
        /// </summary>
        /// <param name="pb"></param>
        public static void HomeInfoCall(PlayerBusiness pb)
        {
            
            string soleid = pb.ID + "HomeInfoCall";
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            
            try
            {
                Variant mv = MemberAccess.MemberInfo(pb.MLevel);
                if (mv == null)
                    return;
                //表示没有自动发送邮件的功能
                if (!mv.GetBooleanOrDefault("IsHome"))
                    return;
                PlayerEx home = pb.Home;
                if (home == null)
                    return;
                Variant hv = home.Value;
                if (hv == null)
                    return;
                Variant produces = hv.GetVariantOrDefault("Produces");

                //自动取得道具功能
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("GuoYuan", "CaiJi");
                dic.Add("ShuFang", "JuanZhou");
                dic.Add("JiaGongFang", "JiaGong");

                foreach (var item in dic)
                {

                    GameConfig model = null;
                    if (item.Key == "GuoYuan")
                    {
                        model = GameConfigAccess.Instance.FindOneById("Home_CJ00002");
                    }
                    else if (item.Key == "ShuFang")
                    {
                        model = GameConfigAccess.Instance.FindOneById("Home_CJ00001");
                    }
                    else if (item.Key == "JiaGongFang")
                    {
                        model = GameConfigAccess.Instance.FindOneById("Home_CJ00003");
                    }
                    if(model==null)                                            
                        return;                   
 
                    string goodsid = model.Value.GetStringOrDefault("GoodsID");

                    Variant vp = produces.GetVariantOrDefault(item.Key);
                    if (vp == null)
                        continue;
                    IList petList = vp.GetValue<IList>("PetList");
                    //表示没有放入宠物
                    if (petList == null || petList.Count == 0)
                        continue;
                    Variant p = petList[0] as Variant;
                    if (p == null)
                        continue;

                    //string goodsid = vp.GetStringOrDefault("GoodsID");
                    string st = vp.GetStringOrDefault("StartTime");
                    string et = vp.GetStringOrDefault("EndTime");

                    if (string.IsNullOrEmpty(st) || string.IsNullOrEmpty(et))
                        continue;

                    DateTime startTime = vp.GetDateTimeOrDefault("StartTime");
                    DateTime endTime = vp.GetDateTimeOrDefault("EndTime");
                    DateTime dt = DateTime.UtcNow;
                    int zv = p.GetIntOrDefault(item.Value);
                    //得到总供生产秒数
                    if (endTime > dt)
                        continue;
                    int t = Convert.ToInt32((endTime - startTime).TotalSeconds);
                    //技算得到道具数量
                    int number = Convert.ToInt32(HomeNumber(zv) * t / 7200);
                    if (number <= 0)
                        continue;

                    GameConfig gc = GameConfigAccess.Instance.FindOneById(goodsid);
                    if (gc == null) 
                        continue;

                    vp["StartTime"] = dt;
                    vp["EndTime"] = dt.AddHours(mv.GetIntOrDefault("HomeHour"));
                    if (home.Save())
                    {
                        
                        string title = "";
                        // "亲爱的会员玩家【{0}】，您在家园中生产的物品【{1}】已到达自动生产的上限时间【{2}】小时，现将您生产出的【{1}】总共【{3}】个邮寄给您，请您及时查收.同时生产又自动开始";                        
                        string content = string.Format(TipManager.GetMessage(HomeReturn.HomeInfoCall4), pb.Name, gc.Name, (t / 3600), number);                        
                        switch (item.Key)
                        {
                            case "GuoYuan":
                                // "家园自动采集";
                                title =TipManager.GetMessage(HomeReturn.HomeInfoCall1);
                                break;
                            case "ShuFang":
                                //"家园自动养殖";
                                title = TipManager.GetMessage(HomeReturn.HomeInfoCall2);
                                break;
                            case "JiaGongFang":
                                //"家园自动挖掘";
                                title = TipManager.GetMessage(HomeReturn.HomeInfoCall3);
                                break;
                        }
                        Variant gs = new Variant();
                        gs.Add("G", goodsid);
                        gs.Add("A", number);
                        gs.Add("E", goodsid);
                        gs.Add("H", 1);

                        List<Variant> goodsList = new List<Variant>();
                        if (number > 0)
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

                    Variant gs1 = new Variant();
                    gs1[goodsid] = number;

                    Variant os = new Variant();
                    os["StartTime"] = startTime;
                    os["EndTime"] = endTime;
                    os["PetsID"] = p.GetStringOrDefault("PetID");
                    os["ID"] = p.GetStringOrDefault("ID");
                    os["Number"] = zv;
                    os["Name"] = item.Value;
                    os["IsHand"] = 0;//自动
                    pb.AddLogVariant(Actiontype.JiaZheng, null, gs1,os);
                }                 
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }


        /// <summary>
        /// 通过采集计算出可得商品数量
        /// </summary>
        /// <param name="caiji"></param>
        /// <returns></returns>
        private static int HomePluchInfo(int caiji)
        {
            int m = NumberRandom.Next(100);
            if (caiji > 0 && caiji < 41)
            {
                if (m < 75)
                    return 1;
                return 2;
            }
            else if (caiji > 40 && caiji < 81)
            {
                if (m < 50)
                    return 1;
                else if (m < 85)
                    return 2;
                else
                    return 3;
            }
            else if (caiji > 80 && caiji < 121)
            {
                if (m < 60)
                    return 2;
                else
                    return 3;
            }
            else if (caiji > 120 && caiji < 161)
            {
                if (m < 45)
                    return 2;
                else if (m < 85)
                    return 3;
                else
                    return 2;
            }
            else if (caiji > 160 && caiji < 200)
            {
                if (m < 35)
                    return 2;
                else if (m < 70)
                    return 3;
                else if (m < 95)
                    return 4;
                else
                    return 5;
            }
            else
            {
                //caiji=200
                if (m < 50)
                    return 3;
                else if (m < 85)
                    return 4;
                else
                    return 5;
            }
        }

        /// <summary>
        /// 采集数量
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private static int HomeNumber(int m)
        {
            if (m < 51)
                return 10;
            else if (m < 101)
                return 50;
            else if (m < 151)
                return 100;
            else if (m <= 200)
                return 200;
            else
                return 0;
        }

        internal static void GetBoard(UserNote note)
        {
            var v = BoardAccess.Instance.FindHomeNote(note.Player.PID);
            note.Call(HomeCommand.GetBoardR, v);
        }

        internal static void RemoveBoard(UserNote note)
        {
            note.Player.RemoveBoard(note.GetString(0));
        }
    }
}
