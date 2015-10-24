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

                //要生产的道具数量
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

                Variant hv = home.Value;

                //得到需要的道具数量，计算出当前可以生产道具的数量
                Variant produces = hv.GetValueOrDefault<Variant>("Produces");
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
                    yaoJi += d.GetIntOrDefault("YaoJi");
                    juanZhou += d.GetIntOrDefault("JuanZhou");
                    sheJi += d.GetIntOrDefault("SheJi");
                    jiaGong += d.GetIntOrDefault("JiaGong");
                    caiJi += d.GetIntOrDefault("CaiJi");
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
                int produceTime = gc.Value.GetIntOrDefault("ProduceTime");
                //所需家政信息
                Variant homeInfo = gc.Value.GetValueOrDefault<Variant>("HomeValue");


                //所需家政信息
                DateTime dt = DateTime.UtcNow;
                DateTime endTime = dt.AddSeconds(number * produceTime);


                Variant mv = MemberAccess.MemberInfo(note.Player.MLevel);

                //生产队列数
                int qc = 3 + mv.GetIntOrDefault("Produce");

                IList list;
                if (v.TryGetValueT("QueueList", out list))
                {
                    //生产对列
                    for (int i = 0; i < list.Count; i++)
                    {
                        
                    }
                }
                else
                {
                    //表示可以生产
                    v.Add("QueueList", new List<DateTime> { endTime });
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
            //LianJinShi炼金房,ShuFang书房,JiaGongFang加工房,MuGongFang木工房,GuoYuan果园
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
            int PetCount = v.GetIntOrDefault("PetCount");
            IList PetList = v.GetValue<IList>("PetList");
            if (PetList.Count >= PetCount)
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


            Variant vn = new Variant();
            vn.Add("ID", id);
            vn.Add("PetID", p.Value.GetStringOrDefault("PetsID"));
            vn.Add("PetName", p.Name);
            vn.Add("YaoJi", p.Value.GetIntOrDefault("YaoJi"));
            vn.Add("JuanZhou", p.Value.GetIntOrDefault("JuanZhou"));
            vn.Add("SheJi", p.Value.GetIntOrDefault("SheJi"));
            vn.Add("JiaGong", p.Value.GetIntOrDefault("JiaGong"));
            vn.Add("CaiJi", p.Value.GetIntOrDefault("CaiJi"));
            vn.Add("DateTime", DateTime.UtcNow);
            vn.Add("PetsRank", p.Value.GetIntOrDefault("PetsRank"));
            PetList.Add(vn);

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
        }

        /// <summary>
        /// 家园采集
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
                    gc = GameConfigAccess.Instance.FindOneById("Home_CJ00002");
                if (name == "ShuFang")
                    gc = GameConfigAccess.Instance.FindOneById("Home_CJ00001");
                if (name == "JiaGongFang")
                    gc = GameConfigAccess.Instance.FindOneById("Home_CJ00003");
                if (gc == null)
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

                string GoodsID = gc.Value.GetStringOrDefault("GoodsID");
                int ProduceTime = gc.Value.GetIntOrDefault("ProduceTime");
                int Score = gc.Value.GetIntOrDefault("Score");
                //采集需要石器币
                int ScoreT = ((minute * 60) / ProduceTime) * Score;

                PlayerEx home = note.Player.Home;
                if (home == null)
                {
                    note.Call(HomeCommand.HomePluckR, false, TipManager.GetMessage(HomeReturn.NoHome));
                    return;
                }
                Variant Produces = home.Value.GetValueOrDefault<Variant>("Produces");
                Variant v = Produces.GetValueOrDefault<Variant>(gc.SubType);
                IList PetList = v.GetValue<IList>("PetList");
                if (PetList.Count == 0)
                {
                    note.Call(HomeCommand.HomePluckR, false, TipManager.GetMessage(HomeReturn.InPet));
                    return;
                }
                Dictionary<string, Pet> pluckPet = new Dictionary<string, Pet>();
                foreach (Variant k in PetList)
                {
                    string pid = k.GetStringOrDefault("ID");
                    if (!string.IsNullOrEmpty(pid))
                    {
                        Pet p = PetAccess.Instance.FindOneById(pid);
                        if (p != null)
                        {
                            if (PetAccess.Instance.IsFatigue(p))
                            {
                                note.Call(HomeCommand.HomePluckR, false, TipManager.GetMessage(HomeReturn.HomeProduce1));
                                return;
                            }

                            if (!pluckPet.ContainsKey(p.ID))
                            {
                                pluckPet.Add(p.ID, p);
                            }
                        }
                    }
                }

                if (note.Player.Score < ScoreT || (!note.Player.AddScore(-ScoreT, FinanceType.HomePluck)))
                {
                    note.Call(HomeCommand.HomePluckR, false, TipManager.GetMessage(HomeReturn.HomePluckNoScore));
                    return;
                }

                foreach (Pet p in pluckPet.Values)
                {
                    int m = 0;
                    if (gc.SubType == "GuoYuan")
                    {
                        m = p.Value.GetIntOrDefault("CaiJi");
                    }
                    else if (gc.SubType == "ShuFang")
                    {
                        m = p.Value.GetIntOrDefault("JuanZhou");
                    }
                    else if (gc.SubType == "JiaGongFang")
                    {
                        m = p.Value.GetIntOrDefault("JiaGong");
                    }

                    if (m > 0)
                    {
                        //增加疲劳点
                        int fatigue = PetAccess.Instance.HomeFatigue(m);
                        PetAccess.Instance.FatigueAdd(p, fatigue);
                    }
                }

                int n = 0;
                foreach (Variant dy in PetList)
                {
                    if (gc.SubType == "GuoYuan")
                    {
                        n = n + dy.GetIntOrDefault("CaiJi");//采集
                    }
                    else if (gc.SubType == "ShuFang")
                    {
                        n = n + dy.GetIntOrDefault("JuanZhou");//卷轴,养殖
                    }
                    else if (gc.SubType == "JiaGongFang")
                    {
                        n = n + dy.GetIntOrDefault("JiaGong");//加工,挖掘
                    }
                }
                int Number = HomeNumber(n); //可生产道具数量

                if (Number == 0) return;
                DateTime dt = DateTime.UtcNow;
                DateTime endtime = dt.AddMinutes(minute);
                v["GoodsID"] = GoodsID;
                v["Number"] = Number;
                v["StartTime"] = dt;
                v["EndTime"] = endtime;
                v["IsBinding"] = 1;

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
        /// 采集中止
        /// </summary>
        /// <param name="note"></param>
        public static void HomeStopPluck(UserNote note)
        {
            string id = note.GetString(0);
            string npcid = note.GetString(1);//NPC
            if (!note.Player.EffectActive(npcid, ""))
                return;

            PlayerEx home = note.Player.Home;
            if (home == null)
            {
                note.Call(HomeCommand.HomeStopPluckR, false, TipManager.GetMessage(HomeReturn.NoHome));
                return;
            }
            Variant Produces = home.Value.GetValueOrDefault<Variant>("Produces");
            Variant v = Produces[id] as Variant;
            if (v.GetStringOrDefault("GoodsID") == string.Empty)
            {
                note.Call(HomeCommand.HomeStopPluckR, false, TipManager.GetMessage(HomeReturn.NoPluck));
                return;
            }
            if (v.GetStringOrDefault("EndTime") == string.Empty)
            {
                note.Call(HomeCommand.HomeStopPluckR, false, TipManager.GetMessage(HomeReturn.NoPluck));
                return;
            }

            DateTime EndTime;
            if (!DateTime.TryParse(v.GetStringOrDefault("EndTime"), out EndTime))
            {
                note.Call(HomeCommand.HomeStopPluckR, false, TipManager.GetMessage(HomeReturn.NoPluck));
                return;
            }

            if (EndTime < DateTime.UtcNow)
            {
                note.Call(HomeCommand.HomeStopPluckR, false, TipManager.GetMessage(HomeReturn.FinishPluck));
                return;
            }

            v["GoodsID"] = string.Empty;
            v["Number"] = string.Empty;
            v["StartTime"] = string.Empty;
            v["EndTime"] = string.Empty;
            v["IsBinding"] = 0;
            v["PetsRank"] = 0;
            home.Save();
            note.Call(HomeCommand.HomeStopPluckR, true, TipManager.GetMessage(HomeReturn.StopPluckSuccess));
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
                Variant Produces = home.Value.GetValueOrDefault<Variant>("Produces");
                Variant v = Produces[name] as Variant;
                if (v.GetStringOrDefault("GoodsID") == string.Empty)
                {
                    note.Call(HomeCommand.CollectionR, false, TipManager.GetMessage(HomeReturn.NoProduce));
                    return;
                }
                if (v.ContainsKey("EndTime"))
                {
                    if (v.GetDateTimeOrDefault("EndTime") > DateTime.UtcNow)
                    {
                        note.Call(HomeCommand.CollectionR, false, TipManager.GetMessage(HomeReturn.ProduceNoFinish));
                        return;
                    }
                }

                string goodsid = v.GetStringOrDefault("GoodsID");
                int number = v.GetIntOrDefault("Number");
                int isbinding = v.GetIntOrDefault("IsBinding");

                PlayerEx burden = note.Player.B0;
                Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
                Variant tmp = new Variant();
                tmp.Add("Number" + isbinding, number);
                dic.Add(goodsid, tmp);

                if (BurdenManager.IsFullBurden(burden, dic))
                {
                    note.Call(HomeCommand.CollectionR, false, TipManager.GetMessage(HomeReturn.BurdenFull));
                    return;
                }
                note.Player.AddGoods(dic, GoodsSource.Collection);

                v["GoodsID"] = string.Empty;
                v["EndTime"] = string.Empty;
                v["Number"] = 0;
                v["IsBinding"] = 0;
                v["PesRank"] = 0;
                v["StartTime"] = string.Empty;
                v["EndTime"] = string.Empty;
                home.Save();
                note.Call(HomeCommand.CollectionR, true, TipManager.GetMessage(HomeReturn.CollectionSuccess));
                note.Player.AddAcivity(ActivityType.ShengChan, 1);

                //note.Player.FinishNote(FinishCommand.ProductGoods, goodsid);
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 得到可生产道具列表
        /// </summary>
        /// <param name="note"></param>
        public static void HomeProduceList(UserNote note)
        {
            string subtype = note.GetString(0);
            string npcid = note.GetString(1);//NPC
            if (!note.Player.EffectActive(npcid, ""))
                return;

            List<Variant> list = new List<Variant>();
            //得到可生产的道具列表
            List<GameConfig> homeList = GameConfigAccess.Instance.Find("Home", subtype);
            if (homeList != null && homeList.Count != 0)
            {
                PlayerEx burden = note.Player.B0;
                foreach (GameConfig gc in homeList)
                {
                    Variant v = new Variant();
                    //得到需要的道具
                    IList need = gc.Value.GetValue<IList>("NeedGoods");

                    List<Variant> NeedList = new List<Variant>();
                    int t = 0;//可以合成的总数量
                    foreach (Variant d in need)
                    {
                        string goodsid = d.GetStringOrDefault("GoodsID");
                        int m = BurdenManager.GoodsCount(burden, goodsid);
                        int n = d.GetIntOrDefault("Number");
                        Variant tmp = new Variant();
                        tmp.Add("GoodsID", goodsid);
                        tmp.Add("Number", n);
                        tmp.Add("CurNumber", m);
                        NeedList.Add(tmp);

                        int y = 0;
                        int s = Math.DivRem(m, n, out y);
                        if (t == 0) t = s;
                        t = t > s ? s : t;
                    }

                    v.Add("Total", t);//可以合成数量
                    v.Add("NeedGoods", NeedList);
                    v.Add("GoodsID", gc.Value.GetStringOrDefault("GoodsID"));
                    v.Add("ID", gc.ID);
                    v.Add("Name", gc.Name);
                    v.Add("UI", gc.UI);
                    v.Add("HomeValue", gc.Value.GetVariantOrDefault("HomeValue"));
                    v.Add("ProduceTime", gc.Value.GetIntOrDefault("ProduceTime"));
                    //表示生产数量
                    v.Add("Number", gc.Value.GetIntOrDefault("Number"));
                    v.Add("PetsRank", gc.Value.GetIntOrDefault("PetsRank"));
                    list.Add(v);
                }
            }
            note.Call(HomeCommand.HomeProduceListR, list);
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
                            if (!string.IsNullOrEmpty(p.GetStringOrDefault("GoodsID")))
                            {
                                note.Call(HomeCommand.PetRetrieveR, false, goodsType, TipManager.GetMessage(HomeReturn.Produceing));
                                return;
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
                string str;
                m_dic.TryRemove(soleid, out str);
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
                //note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(pg));
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
