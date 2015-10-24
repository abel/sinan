using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Util;
using Sinan.FastJson;
using Sinan.Log;

#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.StarModule.Business
{
    class StarBusiness
    {
        static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();

        #region 冥想

        /// <summary>
        /// 冥想星力操作
        /// </summary>
        /// <param name="note"></param>
        public static void PlayerMeditation(UserNote note)
        {
            string soleid = note.PlayerID + "PlayerMeditation";
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                if (note.Player.AState == ActionState.Fight)
                {
                    note.Call(StarCommand.PlayerMeditationR, false, TipManager.GetMessage(StarReturn.PlayerMeditation1));
                    return;
                }

                SceneBusiness sb = note.Player.Scene;
                if (sb.SceneType != SceneType.City && sb.SceneType != SceneType.Outdoor && sb.SceneType != SceneType.Home)
                {
                    note.Call(StarCommand.PlayerMeditationR, false, TipManager.GetMessage(StarReturn.PlayerMeditation2));
                    return;
                }

                
                int msg = note.GetInt32(0);
                switch (msg)
                {
                    case 0:
                        note.Player.SetActionState(ActionState.Meditation);
                        note.Player.MeditationTime = DateTime.UtcNow;
                        break;
                    case 1:
                        GetStarPower(note);
                        break;
                    case 2:
                        note.Player.SetActionState(ActionState.Standing);
                        break;
                    case 3:
                        GetStarPower(note);
                        break;
                }
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 星阵激合
        /// </summary>
        /// <param name="note"></param>
        public static void IsStartTroops(UserNote note)
        {
            //操作星阵
            int starid = note.GetInt32(0);
            PlayerEx star = note.Player.Star;
            Variant v = star.Value;
            //宠物星阵列表
            Variant petsList = v.GetVariantOrDefault("PetsList");
            if (petsList == null)
            {
                //该星阵放入宠物数量不足
                note.Call(StarCommand.IsStartTroopsR, false, TipManager.GetMessage(StarReturn.IsStartTroops1));
                return;
            }
            Variant ps = petsList.GetVariantOrDefault(starid.ToString());
            if (ps == null)
            {
                //该星阵放入宠物数量不足
                note.Call(StarCommand.IsStartTroopsR, false, TipManager.GetMessage(StarReturn.IsStartTroops1));
                return;
            }
            int num = 3;
            if (starid == 0)
            {
                num = 3;
            }
            else if (starid == 1)
            {
                num = 5;
            }
            else if (starid == 2)
            {
                num = 7;
            }
            else
            {
                num = 9;
            }
            if (ps.Count != num)
            {
                //该星阵放入宠物数量不足
                note.Call(StarCommand.IsStartTroopsR, false, TipManager.GetMessage(StarReturn.IsStartTroops1));
                return;
            }
            //当前激合星阵
            string curStar = v.GetStringOrDefault("CurStar");
            if (curStar == starid.ToString())
            {
                v["CurStar"] = -1;
                v["StarOnline"] = 0;
            }
            else
            {
                v["CurStar"] = starid;
            }
            star.Save();
            note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(star));
            note.Call(StarCommand.IsStartTroopsR, true, "");
        }

        /// <summary>
        /// 冥想星力结算
        /// </summary>
        /// <param name="note"></param>
        private static void GetStarPower(UserNote note)
        {

            DateTime dt = DateTime.UtcNow;
            GameConfig gc = GameConfigAccess.Instance.FindOneById("ME_00001");
            if (gc == null)
                return;
            if (note.Player.AState != ActionState.Meditation)
                return;

            //int lv = 1;
            int msg = note.GetInt32(0);
            SceneBusiness sb = note.Player.Scene;
            //if (msg == 3 && sb.SceneType == SceneType.Home)
            //{
            //    lv = StarIncome(note); ;
            //}

            TimeSpan ts = dt - note.Player.MeditationTime;
            Variant v = gc.Value;

            if (ts.TotalSeconds < v.GetIntOrDefault("Second"))
                return;

            note.Player.MeditationTime = dt;

            int exp = v.GetIntOrDefault("Exp");
            int sp = StarIncome(note);



            int max = v.GetIntOrDefault("Max");                        
            Variant mv = MemberAccess.MemberInfo(note.Player.MLevel);
            if (mv != null)
            {
                max = mv.GetIntOrDefault("StarMax");
            }
            
            
            
            
            note.Player.AddExperience(exp, FinanceType.PlayerMeditation);

            //星力达到最大值,不能再增加
            if (note.Player.StarPower >= max)
                return;

            //星力总值
            int total = note.Player.StarPower + sp;
            int addVlaue = 0;
            if (total >= max)
            {
                addVlaue = max - note.Player.StarPower;
            }
            else
            {
                addVlaue = sp;
            }
            //得到星力值

            note.Player.AddStarPower(addVlaue, FinanceType.PlayerMeditation);

        }
        
        #endregion

        #region 激活守护星
        /// <summary>
        /// 星座激活
        /// </summary>
        /// <param name="note"></param>
        public static void StartStar(UserNote note)
        {
            string soleid = note.PlayerID + "StartStar";
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                //星座
                string starid = note.GetString(0);
                //激活第几颗星
                int m = note.GetInt32(1);
                //使用第几个提高成功率的道具
                int number = note.GetInt32(2);

                GameConfig gc = GameConfigAccess.Instance.FindOneById("ST_00001");
                if (gc == null)
                {
                    note.Call(StarCommand.StartStarR, false, TipManager.GetMessage(StarReturn.StartStar1));
                    return;
                }

                Variant v = gc.Value;

                string goodsid = v.GetStringOrDefault("GoodsID");

                //得到星座基本信息
                Variant starInfo = v.GetVariantOrDefault(starid);
                //成功率能够提高的成功率
                double max = v.GetDoubleOrDefault("Max");
                // A点亮该星成功率,
                // B每颗星提供的属性值,
                // C星座点亮耗费的星力值,
                // D星座点亮耗费石币
                double a = 0;
                int b = 0, c = 0, d = 0;
                PartAccess.Instance.GetStarValue(starInfo, m + 1, out a, out b, out c, out d);
                if (number > 0)
                {
                    a += PartAccess.Instance.GetStarLv(number);                                           
                }
                //增加星座成功率                    
                Variant mv = MemberAccess.MemberInfo(note.Player.MLevel);
                if (mv != null)
                {
                    a *= (1 + mv.GetDoubleOrDefault("StarLv"));
                }  
                a = a > max ? max : a;

                PlayerEx star = note.Player.Star;

                Variant sv = star.Value;
                //得到激活守护星列表
                IList list = sv.GetValue<IList>(starid);
                if (list == null)
                {
                    if (m != 0)
                    {
                        note.Call(StarCommand.StartStarR, false, TipManager.GetMessage(StarReturn.StartStar3));
                        return;
                    }
                }
                else
                {
                    if (list.Count >= 14) 
                    {
                        //该星座官守护星已经激活完成
                        note.Call(StarCommand.StartStarR, false, TipManager.GetMessage(StarReturn.StartStar9));
                        return;
                    }
                    if (list.Contains(m))
                    {
                        //"该守护星已经激活"
                        note.Call(StarCommand.StartStarR, false, TipManager.GetMessage(StarReturn.StartStar8));
                        return;
                    }

                    if (list.Count != m)
                    {
                        note.Call(StarCommand.StartStarR, false, TipManager.GetMessage(StarReturn.StartStar3));
                        return;
                    }
                }

                if (BurdenManager.GoodsCount(note.Player.B0, goodsid) < number) 
                {
                    note.Call(StarCommand.StartStarR, false, TipManager.GetMessage(StarReturn.StartStar10));
                    return;
                }

                if (note.Player.StarPower < c) 
                {
                    note.Call(StarCommand.StartStarR, false, TipManager.GetMessage(StarReturn.StartStar4));
                    return;
                }
                if (note.Player.Score < d)
                {
                    note.Call(StarCommand.StartStarR, false, TipManager.GetMessage(StarReturn.StartStar5));
                    return;
                }

                Variant us = new Variant();
                if (number > 0)
                {
                    if (!note.Player.RemoveGoods(goodsid, number,GoodsSource.StartStar))
                    {
                        note.Call(StarCommand.StartStarR, false, TipManager.GetMessage(StarReturn.StartStar10));
                        return;
                    }
                    us[goodsid] = number;
                }

                if (!note.Player.AddStarPower(-c, FinanceType.StartStar))
                {
                    //表示星力值不足
                    note.Call(StarCommand.StartStarR, false, TipManager.GetMessage(StarReturn.StartStar4));
                    return;
                }
                if (!note.Player.AddScore(-d, FinanceType.StartStar))
                {
                    //激活星星需要的石币不足
                    note.Call(StarCommand.StartStarR, false, TipManager.GetMessage(StarReturn.StartStar5));
                    return;
                }

                bool IsSuccess = NumberRandom.RandomHit(a);

                
                //表示激活成功
                if (IsSuccess)
                {
                    if (list == null)
                    {
                        star.Value.Add(starid, new List<int> { m });
                    }
                    else
                    {
                        list.Add(m);
                    }
                    star.Save();
                    note.Player.RefeshSkill();
                    //更新扩展
                    note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(star));

                    //星座
                    note.Player.FinishNote(FinishCommand.Star);

                    //暴星成功后记录一次
                    string share = note.PlayerID + "Share";
                    string n;
                    //如果存在先移除
                    m_dic.TryRemove(share, out n);
                    //暴星成功
                    if (m_dic.TryAdd(share, c.ToString())) 
                    {

                    }

                    note.Call(StarCommand.StartStarR, true, c);
                }
                else
                {
                    note.Call(StarCommand.StartStarR, false, TipManager.GetMessage(StarReturn.StartStar6));
                }
                if (number > 0)
                {
                    note.Player.UpdateBurden();
                }
                
                Variant os=new Variant();
                os["IsSuccess"]=IsSuccess;
                os["Star"]=starid;
                os["Place"]=m;
                os["Lv"] = a;
                os["Score"] = -d;
                os["StaPower"] = -c;
                note.Player.AddLogVariant(Actiontype.StartStar, us, null,os);
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 暴星分享
        /// </summary>
        /// <param name="note"></param>
        public static void StartStarShared(UserNote note) 
        {
            string share = note.PlayerID + "Share";
            try
            {
                if (!m_dic.ContainsKey(share))
                    return;

                string m = "";   
                //暴星
                IList o = note.GetValue<IList>(0);
                int power = note.GetInt32(1);
                if (m_dic.TryGetValue(share, out m))
                {
                    int n=0;
                    if (int.TryParse(m, out n)) 
                    {
                        if (power > n)
                        {
                            power = n;
                        }
                    }
                }

                power = power > 30000 ? 30000 : power;
                int exp = 10 * power + 1000;
                                
                List<string> shareList = new List<string>();
                foreach (string id in o)
                {
                    PlayerBusiness pb = PlayersProxy.FindPlayerByID(id);
                    if (pb.SceneID != note.Player.SceneID)
                        continue;
                    if (note.Player.AState == ActionState.Fight)
                        continue;

                    PlayerEx star = pb.Star;
                    Variant v = star.Value;
                    Variant tmp;
                    if (v.TryGetValue("Share", out tmp))
                    {
                        //分享总次数
                        string msg = "";
                        if (tmp.GetIntOrDefault("Total") >= PartAccess.Instance.MedConfig("Total"))
                        {
                            msg = TipManager.GetMessage(StarReturn.StartStarShared1); //"你已经达到星力爆发分享次数总上限，无法获得经验值！";
                            pb.Call(ClientCommand.SendActivtyR, new object[] { "T02", msg });
                            continue;
                        }

                        //如果是同一开
                        if (tmp.GetLocalTimeOrDefault("ShareTime").Date == DateTime.Now.Date)
                        {
                            //每天最多分享10次暴星经验
                            if (tmp.GetIntOrDefault("Count") >= PartAccess.Instance.MedConfig("EveryDay"))
                            {
                                msg = TipManager.GetMessage(StarReturn.StartStarShared2);// "你已经达到当天的星力爆发分享次数上限，无法获得经验";
                                pb.Call(ClientCommand.SendActivtyR, new object[] { "T02", msg });
                                continue;
                            }
                            tmp.SetOrInc("Count", 1);
                        }
                        else
                        {
                            tmp["Count"] = 1;
                        }
                        tmp.SetOrInc("Exp", exp);
                        tmp["ShareTime"] = DateTime.UtcNow;
                        tmp.SetOrInc("Total", 1);
                    }
                    else
                    {
                        tmp = new Variant();
                        tmp.SetOrInc("Exp", exp);//总共
                        tmp.SetOrInc("Count", 1);//
                        tmp["ShareTime"] = DateTime.UtcNow;//上次分享时间
                        tmp.SetOrInc("Total", 1);//当前分享次数
                        v.Add("Share", tmp);
                    }
                    star.Save();
                    pb.AddExperience(exp, FinanceType.StartStarShared);
                    pb.Call(ClientCommand.UpdateActorR, new PlayerExDetail(star));
                    shareList.Add(pb.ID);
                }

                PlayersProxy.CallAll(StarCommand.StartStarSharedR, new object[] { note.PlayerID, shareList });                
            }
            finally 
            {
                m_dic.TryRemove(share, out share);
            }
        }
        #endregion

        #region 星阵
        /// <summary>
        /// 宠物进入星阵
        /// </summary>
        /// <param name="note"></param>
        public static void InStarTroops(UserNote note)
        {           
            //宠物编号
            string petid = note.GetString(0);
            //宠物所在位置
            string bType = note.GetString(1);
            //星阵类型
            string starP = note.GetString(2);
            //放入位置
            string p = note.GetString(3);

            if (note.Player.Pet != null)
            {
                if (petid == note.Player.Pet.ID)
                {
                    note.Call(StarCommand.InStarTroopsR, false, TipManager.GetMessage(StarReturn.InStarTroops1));
                    return;
                }
            }

            GameConfig gc = GameConfigAccess.Instance.FindOneById("SM_00001");
            if (gc == null)
            {
                note.Call(StarCommand.InStarTroopsR, false, TipManager.GetMessage(StarReturn.StartStar1));
                return;
            }


            Variant sv = gc.Value;
            if (sv == null)
            {
                note.Call(StarCommand.InStarTroopsR, false, TipManager.GetMessage(StarReturn.StartStar1));
                return;
            }

            Variant ml = sv.GetVariantOrDefault("MLevel");
            if (ml == null)
            {
                note.Call(StarCommand.InStarTroopsR, false, TipManager.GetMessage(StarReturn.StartStar1));
                return;
            }
            //可操作的星阵编号
            string sl= ml.GetStringOrDefault(note.Player.MLevel.ToString());

            if (string.IsNullOrEmpty(sl))
            {
                note.Call(StarCommand.InStarTroopsR, false, TipManager.GetMessage(StarReturn.StartStar1));
                return;
            }

            if (Convert.ToInt32(starP) > Convert.ToInt32(sl)) 
            {
                //会员等级不足
                return;
            }

            //星阵基本信息
            Variant sz = sv.GetVariantOrDefault(starP);

            if (sz == null) 
            {
                note.Call(StarCommand.InStarTroopsR, false, TipManager.GetMessage(StarReturn.StartStar1));
                return;
            }

            //最大位置
            int maxp = 0;
            foreach (var item in sz) 
            {
                int cur=0;
                if (int.TryParse(item.Key, out cur)) 
                {
                    if (cur > maxp) 
                    {
                        maxp = cur;
                    }
                }
            }

            if (maxp < Convert.ToInt32(p))
            {
                note.Call(StarCommand.InStarTroopsR, false, TipManager.GetMessage(StarReturn.InStarTroops2));
                return;
            }

            //取得对应位置基本信息
            Variant info = sz.GetVariantOrDefault(p);
            if (info == null)
            {
                //不存在该位置
                note.Call(StarCommand.InStarTroopsR, false, TipManager.GetMessage(StarReturn.StartStar1));
                return;
            }

            Pet pet = PetAccess.Instance.FindOneById(petid);
            if (pet == null)
            {
                note.Call(StarCommand.InStarTroopsR, false, TipManager.GetMessage(StarReturn.InStarTroops3));
                return;
            }

            Variant pv = pet.Value;

            //需要宠物等级
            if (pv.GetIntOrDefault("PetsLevel") < info.GetIntOrDefault("Level"))
            {
                note.Call(StarCommand.InStarTroopsR, false, string.Format(TipManager.GetMessage(StarReturn.InStarTroops7),info.GetIntOrDefault("Level")));
                //宠物等级不足
                return;
            }

            Variant czd = pv.GetVariantOrDefault("ChengChangDu");
            //宠物最小成长度
            if (czd.GetIntOrDefault("V") < info.GetIntOrDefault("Min"))
            {
                note.Call(StarCommand.InStarTroopsR, false, string.Format(TipManager.GetMessage(StarReturn.InStarTroops2),info.GetIntOrDefault("Min")));
                //宠物当前成长度不足
                return;
            }

            int n = 0;
            if (!int.TryParse(p, out n))
            {
                note.Call(StarCommand.InStarTroopsR, false, TipManager.GetMessage(StarReturn.InStarTroops2));
                return;
            }
            if (n < 0 || n > 8)
            {
                note.Call(StarCommand.InStarTroopsR, false, TipManager.GetMessage(StarReturn.InStarTroops2));
                return;
            }

            //bType
            List<string> list = new List<string>() { "B2", "B3" };
            if (!list.Contains(bType))
            {
                note.Call(StarCommand.InStarTroopsR, false, TipManager.GetMessage(StarReturn.InStarTroops2));
                return;
            }

            PlayerEx b = bType == "B2" ? note.Player.B2 : note.Player.B3;            
            IList c = b.Value.GetValue<IList>("C");
            Variant tmp = null;
            foreach (Variant k in c)
            {
                if (k.GetStringOrDefault("E") == petid)
                {
                    tmp = k;
                    break;
                }
            }


            if (tmp == null)
            {
                note.Call(StarCommand.InStarTroopsR, false, TipManager.GetMessage(StarReturn.InStarTroops3));
                return;
            }

   

            PlayerEx star = note.Player.Star;
            Variant v = star.Value;
           
            Variant petsList = v.GetVariantOrDefault("PetsList");
            if (petsList == null) 
            {
                v["PetsList"] = new Variant();
                petsList = v.GetVariantOrDefault("PetsList");
            }

            //得到星阵信息
            Variant s = petsList.GetVariantOrDefault(starP);
            if (s == null)
            {
                petsList[starP] = new Variant();
                s = petsList.GetVariantOrDefault(starP);
            }

            if (s.ContainsKey(p)) 
            {
                note.Call(StarCommand.InStarTroopsR, false, TipManager.GetMessage(StarReturn.InStarTroops4));
                return;
            }
            
            s.Add(p, PetBase(pet));

            star.Save();
            BurdenManager.BurdenClear(tmp);
            b.Save();


            if (bType == "B3")
            {
                if (tmp.GetIntOrDefault("P") > 3)
                {
                    //更新溜宠信息                        
                    List<Variant> ps = PlayerExAccess.Instance.SlipPets(note.Player.B3);
                    note.Player.GetSlipPets(ps);
                }
            }

            note.Player.UpdateBurden(bType);
            note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(star));
            note.Call(StarCommand.InStarTroopsR, true, petid);
        }

        /// <summary>
        /// 取出星阵中宠物
        /// </summary>
        /// <param name="note"></param>
        public static void OutStarTroops(UserNote note)
        {
            //取出宠物放入位置B2,B3
            string bType = note.GetString(0);
            //星阵类型
            string starT = note.GetString(1);
            //宠物所在位置
            string p = note.GetString(2);

            List<string> list = new List<string>() { "B2", "B3" };
            if (!list.Contains(bType))
            {
                note.Call(StarCommand.OutStarTroopsR, false, TipManager.GetMessage(StarReturn.InStarTroops2));
                return;
            }

            PlayerEx b = bType == "B2" ? note.Player.B2 : note.Player.B3;
            IList c = b.Value.GetValue<IList>("C");

            Variant t = BurdenManager.GetBurdenSpace(c);
            if (t == null)
            {
                note.Call(StarCommand.OutStarTroopsR, false, TipManager.GetMessage(StarReturn.OutStarTroops1));
                return;
            }

            PlayerEx star = note.Player.Star;
            Variant v = star.Value;

            Variant petsList = v.GetVariantOrDefault("PetsList");
            if (petsList == null)
            {
                note.Call(StarCommand.OutStarTroopsR, false, TipManager.GetMessage(StarReturn.OutStarTroops2));
                return;
            }

            Variant pv = petsList.GetVariantOrDefault(starT);
            if (pv == null)
            {
                note.Call(StarCommand.OutStarTroopsR, false, TipManager.GetMessage(StarReturn.OutStarTroops2));
                return;
            }

            Variant pn = pv.GetVariantOrDefault(p.ToString());
            if (pn == null)
            {
                note.Call(StarCommand.OutStarTroopsR, false, TipManager.GetMessage(StarReturn.OutStarTroops2));
                return;
            }

            string petid = pn.GetStringOrDefault("ID");

            if (!pv.Remove(p))
            {
                note.Call(StarCommand.OutStarTroopsR, false, TipManager.GetMessage(StarReturn.OutStarTroops2));
                return;
            }

            

            Pet pet = PetAccess.Instance.FindOneById(petid);
            if (pet != null)
            {
                Variant pd = pet.Value;
                t["E"] = petid;
                t["G"] = pd.GetStringOrDefault("PetsID");
                t["I"] = 0;
                t["A"] = 1;
                t["D"] = 0;
                t["H"] = pd.GetIntOrDefault("IsBinding");
                t["S"] = pd.GetIntOrDefault("Sort");
                t["R"] = pd.GetIntOrDefault("PetsRank");

                if (bType == "B2")
                {
                    Variant ct = PetAccess.Instance.CreateAward(note.Player.Level, petid, note.PlayerID, note.Player.Pet);
                    Variant info = t["T"] as Variant;
                    if (info != null)
                    {
                        if (info.ContainsKey("ProtectionTime"))
                        {
                            if (ct == null) ct = new Variant();
                            ct.Add("ProtectionTime", t.GetDateTimeOrDefault("ProtectionTime"));
                        }
                    }
                    t["T"] = ct;
                }
                b.Save();

                if (bType == "B3")
                {
                    if (t.GetIntOrDefault("P") > 3)
                    {
                        //更新溜宠信息                        
                        List<Variant> ps = PlayerExAccess.Instance.SlipPets(note.Player.B3);
                        note.Player.GetSlipPets(ps);
                    }
                }
            }
            string curStar = v.GetStringOrDefault("CurStar");
            if (!string.IsNullOrEmpty(curStar) && curStar == starT)
            {
                v["CurStar"] = -1;
                v["StarOnline"] = 0;
            }
            star.Save();
            note.Player.UpdateBurden(bType);
            note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(star));
            note.Call(StarCommand.OutStarTroopsR, true, TipManager.GetMessage(StarReturn.OutStarTroops3));

        }
   
        /// <summary>
        /// 取得星阵星力值
        /// </summary>
        /// <param name="note"></param>
        public static void GetStarTroops(UserNote note) 
        {
            string soleid = note.PlayerID + "GetStarTroops";

            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                PlayerEx star = note.Player.Star;
                Variant v = star.Value;  
                int sp = StarTroopsSettle(star);
                if (sp <= 0)
                {
                    note.Call(StarCommand.GetStarTroopsR, false, TipManager.GetMessage(StarReturn.GetStarTroops2));
                    return;
                }
                int powerMax = PartAccess.Instance.PowerMax();

                int addPower = 0;
                if (note.Player.StarPower + sp > powerMax)
                {
                    addPower = powerMax - note.Player.StarPower;
                }
                else
                {
                    addPower = sp;
                }
                v["StarPower"] = 0;
                //重新计算时间
                v["StarTime"] = DateTime.UtcNow;

                if (addPower > 0)
                {
                    note.Player.AddStarPower(addPower, FinanceType.GetStarTroops);
                }

                star.Save();
                note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(star));
                note.Call(StarCommand.GetStarTroopsR, true, TipManager.GetMessage(StarReturn.GetStarTroops3));
            }
            finally 
            {
                m_dic.TryRemove(soleid, out soleid);                    
            }
        }

        /// <summary>
        /// 星阵星力结算
        /// </summary>
        /// <param name="note"></param>
        public static int StarTroopsSettle(PlayerEx star)
        {
            if (star == null) return 0;

            Variant v = star.Value;
            if (v == null) return 0;

            Variant petsList = v.GetVariantOrDefault("PetsList");

            if (petsList == null || petsList.Count == 0)
            {
                return v.GetIntOrDefault("StarPower");
            }

            List<Pet> pets = new List<Pet>();
            for (int i = 0; i < 9; i++)
            {
                Variant t = petsList.GetVariantOrDefault(i.ToString());
                if (t == null)
                    continue;
                Pet p = PetAccess.Instance.FindOneById(t.GetStringOrDefault("ID"));
                if (p == null)
                    continue;
                pets.Add(p);
            }

            if (string.IsNullOrEmpty(v.GetStringOrDefault("StarTime")))
            {
                return v.GetIntOrDefault("StarPower");
            }


            int power = PartAccess.Instance.StarTroopsPower(pets, v.GetDateTimeOrDefault("StarTime"));
            int curPower = v.GetIntOrDefault("StarPower");
            int troopsMax = PartAccess.Instance.TroopsMax();
            //已经最大,不需要变化
            if (curPower >= troopsMax)
            {
                return troopsMax;
            }

            if (power > 0)
            {
                if ((curPower + power) > troopsMax)
                {
                    v["StarPower"] = troopsMax;
                }
                else
                {
                    v.SetOrInc("StarPower", power);
                }
                //结算一次更新一次时间
                v["StarTime"] = DateTime.UtcNow;
            }            
            return v.GetIntOrDefault("StarPower");
        }

        /// <summary>
        /// 星阵离线收益
        /// </summary>
        /// <param name="note"></param>
        public static void OfflineTroops(UserNote note)
        {
            GameConfig gc = GameConfigAccess.Instance.FindOneById("ME_00001");
            if (gc == null)
                return;
            Variant v = gc.Value;
            int max = v.GetIntOrDefault("Max");
            Variant mv = MemberAccess.MemberInfo(note.Player.MLevel);
            if (mv != null)
            {
                max = mv.GetIntOrDefault("StarMax");
            }

            //星力达到最大值,不能再增加
            if (note.Player.StarPower >= max)
                return;

            GameConfig sm = GameConfigAccess.Instance.FindOneById("SM_00001");
            if (sm == null)
                return;
            Variant sv = sm.Value;
            if (sv == null)
                return;

            PlayerEx star = note.Player.Star;
            Variant cv = star.Value;
            if (sv == null)
                return;

            string curStar = cv.GetStringOrDefault("CurStar");
            if (string.IsNullOrEmpty(curStar))
                return;
            Variant info = sv.GetVariantOrDefault(curStar);
            if (info == null)
                return;

            int income = info.GetIntOrDefault("Income");

            //上一次下线时间
            DateTime modified = note.Player.Modified;
            DateTime dt = DateTime.UtcNow;
            double totalSecond = (dt - modified).TotalSeconds;
            int sp = Convert.ToInt32(totalSecond / 20);
            //星力总值
            int total = note.Player.StarPower + sp * income;
            int addVlaue = 0;
            if (total >= max)
            {
                addVlaue = max - note.Player.StarPower;
            }
            else
            {
                addVlaue = sp;
            }
            note.Player.AddStarPower(addVlaue, FinanceType.OfflineTroops);
        }

        /// <summary>
        /// 星阵在线收益
        /// </summary>
        /// <param name="pb">在线角色</param>
        public static void OnlineTroops(PlayerBusiness pb)
        {
            if (pb.AState == ActionState.Meditation)
                return;

            PlayerEx star = pb.Star;
            Variant v = star.Value;
            string curStar = v.GetStringOrDefault("CurStar");
            if (string.IsNullOrEmpty(curStar) || curStar == "-1")
                return;

            GameConfig mgc = GameConfigAccess.Instance.FindOneById("ME_00001");
            if (mgc == null)
                return;
            Variant mev = mgc.Value;

            int max = mev.GetIntOrDefault("Max");
            Variant mv = MemberAccess.MemberInfo(pb.MLevel);
            if (mv != null)
            {
                max = mv.GetIntOrDefault("StarMax");
            }
            
            //当前在线总秒数
            int second = v.SetOrInc("StarOnline", 300);
            if (pb.StarPower >= max)
                return;

            int income = 0;
            if (second < 1800)
                return;

            GameConfig gc = GameConfigAccess.Instance.FindOneById("SM_00001");
            if (gc == null)
                return;
            Variant info = gc.Value.GetVariantOrDefault(curStar);
            if (info == null)
                return;

            v["StarOnline"] = 0;
            if (!star.Save())
                return;

            income = Convert.ToInt32(second / 20) * info.GetIntOrDefault("Income");
            int m = pb.StarPower + income;
            int n = 0;
            if (m >= max)
            {
                n = max - pb.StarPower;
            }
            else
            {
                n = income;
            }
            pb.AddStarPower(n, FinanceType.OnlineTroops);
            //Console.WriteLine("OnlineTroops:" + n);
        }

        /// <summary>
        /// 得到宠物基本信息
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static Variant PetBase(Pet p)
        {
            Variant n = new Variant();
            Variant v = p.Value;
            if (v != null)
            {
                n.Add("ID", p.ID);   
                int ccd = v.GetVariantOrDefault("ChengChangDu").GetIntOrDefault("V");
                n.Add("ChengChangDu", ccd);                
                n.Add("PetsID", v.GetStringOrDefault("PetsID"));
                n.Add("PetsRank", v.GetIntOrDefault("PetsRank"));
                n.Add("Skin", v.GetStringOrDefault("Skin"));
                n.Add("Name", p.Name);
                n.Add("PetsLevel", v.GetIntOrDefault("PetsLevel"));
                //进入星阵时间
                n.Add("StarTime", DateTime.UtcNow);
            }
            return n;
        }

        /// <summary>
        /// 当前星阵收益
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        private static int StarIncome(UserNote note)
        {
            int income = 2;
            PlayerEx star = note.Player.Star;
            Variant v = star.Value;
    
            GameConfig gc = GameConfigAccess.Instance.FindOneById("SM_00001");
            if (gc == null)
            {
                return income;
            }
            Variant sv = gc.Value;
            if (sv == null)
            {
                return income;
            }
            string curStar = v.GetStringOrDefault("CurStar");
            if (string.IsNullOrEmpty(curStar))
                return income;
            Variant info = sv.GetVariantOrDefault(curStar);
            if (info == null)
                return income;
            income += info.GetIntOrDefault("Income") * 2;
            return income;

            //int income = 2;
            //PlayerEx star = note.Player.Star;
            //Variant v = star.Value;
            //Variant pl = v.GetVariantOrDefault("PetsList");
            //if (pl == null)
            //{
            //    return income;
            //}

            //GameConfig gc = GameConfigAccess.Instance.FindOneById("SM_00001");
            //if (gc == null)
            //{
            //    return income;
            //}
            //Variant sv = gc.Value;
            //if (sv == null)
            //{
            //    return income;
            //}

            //for (int i = 3; i >= 0; i--)
            //{
            //    Variant ps = pl.GetVariantOrDefault(i.ToString());

            //    if (ps == null)                
            //        continue;            
    
            //    //宠物放放数量
            //    int count = ps.Count;
            //    if (count == 0)
            //        continue;

            //    Variant info = sv.GetVariantOrDefault(i.ToString());
            //    if (info == null)
            //        continue;

            //    bool isfull = false;
            //    if ((i == 3 && count == 9) || (i == 2 && count == 7) || (i == 1 && count == 5) || (i == 0 && count == 3)) 
            //    {
            //        isfull = true;
            //    }

            //    if (isfull) 
            //    {
            //        income += info.GetIntOrDefault("Income");
            //        return income;
            //    }
            //}
            //return income;
        }
        #endregion
    }
}
