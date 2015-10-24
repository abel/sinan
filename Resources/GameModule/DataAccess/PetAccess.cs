using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.FastConfig;
using Sinan.Util;

namespace Sinan.GameModule
{
    public class PetAccess : VariantBuilder<Pet>, IConfigManager
    {
        readonly static PetAccess m_instance = new PetAccess();

        Dictionary<int, IList> m_keys;

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static PetAccess Instance
        {
            get { return m_instance; }
        }

        PetAccess()
            : base("Pet")
        {
        }
        #region 宠物计算

        public bool SaveLife(Pet p)
        {
            if (p != null && p.ID != null)
            {
                int shengMing = p.Value.GetVariantOrDefault("ShengMing").GetIntOrDefault("V");
                int moFa = p.Value.GetVariantOrDefault("MoFa").GetIntOrDefault("V");
                int exp = p.Value.GetVariantOrDefault("Experience").GetIntOrDefault("V");
                var query = Query.EQ("_id", p.ID);
                var update = Update.Set("Value.ShengMing.V", shengMing).Set("Value.MoFa.V", moFa).Set("Value.Experience.V", exp);
                var v = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);                
                return v == null ? false : v.DocumentsAffected > 0;
            }
            return false;
        }

        /// <summary>
        /// 保存当前疲劳值
        /// </summary>
        /// <param name="pid">宠物</param>
        /// <param name="fatigue">值</param>
        /// <returns></returns>
        public bool SaveFatigue(string pid, int fatigue)
        {
            if (!string.IsNullOrEmpty(pid))
            {
                var query = Query.EQ("_id", pid);
                var update = Update.Set("Value.Fatigue", fatigue);
                var t = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
                return t == null ? false : t.DocumentsAffected > 0;
            }
            return false;
        }

        /// <summary>
        /// 加载宠物基本配置
        /// </summary>
        public void Load(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            {
                Dictionary<int, IList> keys = new Dictionary<int, IList>();
                string key = sr.ReadLine();
                while (key != null)
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        key = sr.ReadLine();
                        continue;
                    }
                    string[] strs = key.Split(',');
                    keys[Convert.ToInt32(strs[0])] = new List<object>(strs);
                    key = sr.ReadLine();
                }
                m_keys = keys;
            }
        }

        /// <summary>
        /// 得到不同等级相关信息
        /// </summary>
        /// <param name="l">等级</param>
        /// <param name="v">0表示等级，1-5资质,6经验</param>
        /// <returns></returns>
        public int GetPetLevelMsg(int l, int v)
        {
            if (l > m_keys.Count)
            {
                l = m_keys.Count;
            }
            return Convert.ToInt32(m_keys[l][v]);
        }

        /// <summary>
        /// 得到宠物最大成长度
        /// </summary>
        /// <param name="zizhi">资质</param>
        /// <param name="level">需要等级</param>
        /// <returns></returns>
        public int PetZiZhi(int zizhi, out int level)
        {
            GameConfig gc = GameConfigAccess.Instance.FindOneById("Z_0000");
            if (gc == null)
            {
                level = 0;

                return 0;
            }
            Variant v = gc.Value;
            if (v == null)
            {
                level = 0;

                return 0;
            }
            Variant info = v.GetVariantOrDefault(zizhi.ToString());
            if (info == null)
            {
                level = 0;
                return 0;
            }
            level = info.GetIntOrDefault("Level");
            return info.GetIntOrDefault("Max");
        }

        /// <summary>
        /// 可提升的最大值
        /// </summary>
        /// <returns></returns>
        public int PetMaxZiZhi() 
        {
            GameConfig gc = GameConfigAccess.Instance.FindOneById("Z_0000");
            if (gc == null)
            {
                
                return 0;
            }
            Variant v = gc.Value;
            if (v == null)
            {                
                return 0;
            }
            int max = 0;
            foreach (var item in v) 
            {
                int m = Convert.ToInt32(item.Key);
                if (max < m) 
                {
                    max = m;
                }
            }
            return max;
        }
        

        /// <summary>
        /// 得到带领的宠物基本信息
        /// </summary>
        /// <param name="playerID">角色ID</param>
        /// <returns></returns>
        public Pet GetPetIsGrit(string playerID)
        {
            var query = Query.And(Query.EQ("PlayerID", playerID), Query.EQ("Value.IsGirt", 1));
            return m_collection.FindOneAs<Pet>(query);
        }

        /// <summary>
        /// 获取物品信息
        /// </summary>
        /// <param name="petid">宠物唯一ID</param>
        /// <param name="playerID">所属Player</param>
        /// <returns></returns>
        public Pet GetPetByID(string petid, string playerID)
        {
            var query = Query.And(Query.EQ("_id", petid), Query.EQ("PlayerID", playerID));
            return m_collection.FindOneAs<Pet>(query);
        }

        /// <summary>
        /// 更新宠物基本信息
        /// </summary>
        /// <param name="playerPetsID"></param>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public bool UpdateIsGrit(string playerPetsID, string playerID)
        {
            var query = Query.And(Query.EQ("PlayerID", playerID));
            var update = Update.Set("Value.IsGirt", 0);
            m_collection.Update(query, update, UpdateFlags.Multi, SafeMode.False);

            var query1 = Query.And(Query.EQ("_id", playerPetsID), Query.EQ("PlayerID", playerID));
            var update1 = Update.Set("Value.IsGirt", 1);
            m_collection.Update(query1, update1, UpdateFlags.None, SafeMode.False);
            return true;
        }

        /// <summary>
        /// 宠物统计
        /// </summary>
        /// <param name="petsRank">阶数</param>
        /// <returns></returns>
        public IList GetPets()
        {
            return m_collection.FindAllAs<Pet>().ToList();
        }

        /// <summary>
        /// 不同阶级宠物数量
        /// </summary>
        /// <param name="rank">阶级[0-4]</param>
        /// <returns></returns>
        public List<int> PetRankCount()
        {
            List<int> list = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                int count = m_collection.Count(Query.EQ("Value.PetsRank", i));
                list.Add(count);
            }
            return list;
        }


        public List<Pet> GetPetPaiHang(int pageSize, int index, string roleID)
        {
            SortByBuilder sb = SortBy.Descending("Value.PetsLevel");
            if (string.IsNullOrEmpty(roleID))
            {
                var n = m_collection.FindAllAs<Pet>().SetSortOrder(sb).SetSkip(index * pageSize).SetLimit(pageSize);
                return n.ToList();
            }
            else
            {
                var query = Query.And(Query.EQ("Value.PetsType", roleID));
                var n = m_collection.FindAs<Pet>(query).SetSortOrder(sb).SetSkip(index * pageSize).SetLimit(pageSize);
                return n.ToList();
            }
        }

        /// <summary>
        /// 当前成长总值
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public List<Pet> GetPetPaiHang(int pageSize, int index)
        {
            SortByBuilder sb = SortBy.Descending("Value.ChengChangDu.V");
            var n = m_collection.FindAllAs<Pet>().SetSortOrder(sb).SetSkip(index * pageSize).SetLimit(pageSize);
            return n.ToList();
        }

        /// <summary>
        /// 得到宠物
        /// </summary>
        /// <param name="pv">宠物槽</param>
        /// <param name="petid">宠物ID</param>
        /// <param name="revmode">获得途径:0驯化,1野性</param>
        /// <param name="isbinding">0非绑定,1绑定</param>
        /// <returns></returns>
        public int CreatePet(PlayerEx pv, string petid, int revmode, int isbinding)
        {
            if (pv == null)
            {
                return -3;
            }
            IList c = pv.Value.GetValue<IList>("C");
            Variant tmp = null;
            foreach (Variant k in c)
            {
                if (k.GetStringOrDefault("E") == string.Empty)
                {
                    tmp = k;
                    break;
                }
            }
            if (tmp == null)
                return -1;
            // 得到宠物基本信息
            GameConfig gc = GameConfigAccess.Instance.FindOneById(petid);
            if (gc == null || gc.Value == null)
                return -2;
            Variant model = gc.Value;
            string petType = model.GetStringOrDefault("PetsType");
            int petLevel = model.GetIntOrDefault("PetsLevel");

            Variant petInfo = CreateBase(gc, revmode, isbinding);

            Pet p = new Pet();
            p.ID = ObjectId.GenerateNewId().ToString();
            p.Name = gc.Name;
            p.Value = petInfo;
            p.Modified = DateTime.UtcNow;
            p.PlayerID = pv.PlayerID;
            p.Created = DateTime.UtcNow;
            p.Save();

            tmp["E"] = p.ID;
            tmp["G"] = petid;
            tmp["I"] = 0;
            tmp["A"] = 1;
            tmp["D"] = 0;
            tmp["H"] = isbinding;
            tmp["S"] = p.Value.GetIntOrDefault("Sort");
            tmp["R"] = p.Value.GetIntOrDefault("PetsRank");
            pv.Save();
            if (tmp.GetIntOrDefault("P") > 3)
                return 1;
            return 0;
        }

        /// <summary>
        /// 创建新的
        /// </summary>
        /// <param name="gc"></param>
        /// <param name="RevMode"></param>
        /// <param name="IsBinding"></param>
        /// <param name="playerName"></param>
        /// <param name="v">成长度</param>
        /// <returns></returns>
        public static Variant CreateBase(GameConfig gc, int RevMode, int IsBinding)
        {
            Variant model = gc.Value;
            Variant home = model.GetValueOrDefault<Variant>("Home");
            Variant skill = model.GetValueOrDefault<Variant>("Skill");

            PetProperty life = new PetProperty();


            int level = model.GetIntOrDefault("PetsLevel");
            life.ZiZhi = RandomZiZhi(model.GetStringOrDefault("ZiZhi"));

            //int max = PetAccess.Instance.GetPetLevelMsg(level, life.ZiZhi);

            int l = 0;            
            int max = PetAccess.Instance.PetZiZhi(life.ZiZhi, out l);

            Variant ChengChangDu = new Variant();
            ChengChangDu.Add("M", max);
            if (model.ContainsKey("ZiZhiR"))
            {
                int t = RandomCZD(model.GetStringOrDefault("ZiZhiR"));
                if (t > max)
                {
                    t = max;
                }
                life.ChengChangDu = t;// RandomCZD(model.GetStringOrDefault("ZiZhiR"));
            }
            else
            {
                life.ChengChangDu = NumberRandom.Next(1, max + 1);
            }
            ChengChangDu.Add("V", life.ChengChangDu);


            string roleID = model.GetStringOrDefault("PetsType");
            InitProperty(roleID, level, life);

            Variant pet = life.ToVariant(level);
            pet.Add("ChengChangDu", ChengChangDu);
            pet.Add("PetsID", gc.ID);
            pet.Add("PetsType", roleID);
            pet.Add("RevMode", RevMode);
            pet.Add("IsBinding", IsBinding);
            pet.Add("PetsLevel", level);
            pet.Add("PetsRank", model.GetIntOrDefault("PetsRank"));
            pet.Add("Fatigue", 0);//Fatigue疲劳值达到最大值,宠物将不能做其它操作
            pet.Add("ZiZhi", life.ZiZhi);
            //0表示没有使用在普通包袱中,1表示已经带领的宠物,
            //2表示正在驯化,3表示正在炼金
            //4表示正在书房，5表示正在加工房,
            //6表示正在木工房,7表示正在采集,8拍卖行
            pet.Add("GoodsType", model.GetStringOrDefault("GoodsType"));
            pet.Add("IsGirt", 0);
            pet.Add("Sort", model.GetIntOrDefault("Sort"));
            pet.Add("Skin", gc.UI.GetStringOrDefault("Skin"));
            //pet.Add("PlayerName", playerName);
            pet.Add("IsBack", gc.Value.GetIntOrDefault("IsBack"));
            pet.Add("BackScore", gc.Value.GetIntOrDefault("BackScoreB"));//回收价格
            pet.Add("StallScore", gc.Value.GetIntOrDefault("StallScore"));//拍买价格

            //创建技能槽
            pet.Add("SkillGroove", CreatePetSkillGroove());
            pet.Add("Skill", CreatePetSkill(skill));

            //将技能放入宠物槽
            SkillGroove(pet.GetValue<IList>("SkillGroove"), pet.GetValueOrDefault<Variant>("Skill"));

            //家政属性
            pet.Add("YaoJi", HomeRandom(home.GetStringOrDefault("YaoJi")));
            pet.Add("JuanZhou", HomeRandom(home.GetStringOrDefault("JuanZhou")));
            pet.Add("SheJi", HomeRandom(home.GetStringOrDefault("SheJi")));
            pet.Add("JiaGong", HomeRandom(home.GetStringOrDefault("JiaGong")));
            pet.Add("CaiJi", HomeRandom(home.GetStringOrDefault("CaiJi")));
            return pet;
        }

        /// <summary>
        ///  刷新宠物属性
        /// </summary>
        /// <param name="v">玩家的宠物属性(Goods中的Value)</param>
        /// <param name="life">添加了被动技能附加的属性</param>
        /// <param name="exp">当前宠物的经验</param>
        /// <param name="isup">血和魔是否满</param>
        /// <returns></returns>
        public static void RefreshPetProperty(Variant v, PetProperty life, int exp, bool isup)
        {
            int level = v.GetIntOrDefault("PetsLevel");
            string roleID = v.GetStringOrDefault("PetsType");

            //成长度总值
            Variant ccd = v.GetValueOrDefault<Variant>("ChengChangDu");

            //ccd["M"] = PetAccess.Instance.GetPetLevelMsg(level, v.GetIntOrDefault("ZiZhi"));
            int l = 0;
            ccd["M"] = PetAccess.Instance.PetZiZhi(v.GetIntOrDefault("ZiZhi"), out l);

            InitProperty(roleID, level, life);
            // 同步值
            life.Experience = exp;

            int ShengMing = v.GetVariantOrDefault("ShengMing").GetIntOrDefault("V");
            int MoFa = v.GetVariantOrDefault("MoFa").GetIntOrDefault("V");
            Variant newV = isup ? life.ToVariant(level) : life.ToVariant(level, ShengMing, MoFa);
            foreach (var item in newV)
            {
                v[item.Key] = item.Value;
            }
        }

        /// <summary>
        /// 宠物技能
        /// </summary>
        /// <param name="pet"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static PetProperty GetSkillPro(Pet pet, int level)
        {
            PetProperty life = new PetProperty();
            Variant skills = pet.Value.GetVariantOrDefault("Skill");
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    GameConfig gc = GameConfigAccess.Instance.FindOneById(skill.Key);
                    if (gc == null || gc.Value == null)
                    {
                        continue;
                    }
                    Variant addV = gc.Value.GetVariantOrDefault(skill.Value.ToString());
                    if (addV == null && addV.Count == 0)
                    {
                        continue;
                    }

                    if (gc.SubType == SkillSub.PetAddition)
                    {
                        life.AddPet(addV, level);
                    }
                    else if (gc.SubType == SkillSub.Addition)
                    {
                        life.Add(addV);
                    }
                    else if (gc.SubType == SkillSub.AdditionJ)
                    {
                        life.Add(addV);
                    }
                }
            }
            return life;
        }

        /// <summary>
        /// 宠物属性
        /// </summary>
        /// <param name="roleID">角色</param>
        /// <param name="lev">等级</param>
        /// <param name="lift">宠物加成属性</param>
        static void InitProperty(string roleID, int lev, PetProperty lift)
        {
            lift.BaoJiShangHai += 1.5;
            lift.KangShangHai += 0;
            lift.FanJiLv += 0.05;
            lift.HeJiLv += 0.5;
            lift.HeJiShangHai += 1.2;
            lift.TaoPaoLv += 0;
            InitLevel1(roleID, lev, lift);
        }

        static void InitLevel1(string roleID, int lev, PetProperty lift)
        {
            if (roleID == "1")
            {
                lift.LiLiang += 2 + 3 * lev;
                lift.TiZhi += 2 + 2 * lev;
                lift.ZhiLi += 2;
                lift.JingShen += 2 + lev;

                lift.SuDu += 5 + 5 * lev;
                lift.BaoJi += 45 + 5 * lev;
                lift.MingZhong += 47 + 3 * lev;
                lift.ShanBi += 48 + 2 * lev;
            }
            else if (roleID == "2")
            {
                lift.LiLiang += 2;
                lift.TiZhi += 2 + lev;
                lift.ZhiLi += 2 + 3 * lev;
                lift.JingShen += 2 + 2 * lev;

                lift.SuDu += 7 + 3 * lev;
                lift.BaoJi += 46 + 4 * lev;
                lift.MingZhong += 47 + 3 * lev;
                lift.ShanBi += 45 + 5 * lev;
            }
            else if (roleID == "3")
            {
                lift.LiLiang += 2 + lev;
                lift.TiZhi += 2 + 2 * lev;
                lift.ZhiLi += 2 + 2 * lev;
                lift.JingShen += 2 + lev;

                lift.SuDu += 6 + 4 * lev;
                lift.BaoJi += 47 + 3 * lev;
                lift.MingZhong += 46 + 4 * lev;
                lift.ShanBi += 46 + 4 * lev;
            }
            InitLevel2(roleID, lev, lift);
        }

        static void InitLevel2(string roleID, int lev, PetProperty lift)
        {
            lift.BaoJiLv += lift.BaoJi * 0.0001;
            lift.MingZhongLv += lift.MingZhongLv * 0.0001;
            lift.ShanBiLv += lift.ShanBi * 0.0001;

            if (roleID == "1")
            {
                //狂战士
                lift.ShengMing += 35 + 120 * lev + 3 * lift.LiLiang + 8 * lift.TiZhi + (int)(lift.ChengChangDu * 18.4);
                lift.MoFa += 3 * lift.ZhiLi + 4 * lift.JingShen + (int)(lift.ChengChangDu * 2.4);
                lift.GongJi += 15 + lift.LiLiang + (int)(lift.ChengChangDu * 2);
                lift.MoFaGongJi += lift.ZhiLi;// +(int)(lift.ChengChangDu * 0);
                lift.FangYu += lift.TiZhi + (int)(lift.ChengChangDu * 1.2);
                lift.MoFaFangYu += lift.JingShen + (int)(lift.ChengChangDu * 0.6);
            }
            else if (roleID == "2")
            {
                //魔弓手
                lift.ShengMing += 35 + 85 * lev + 2 * lift.LiLiang + 6 * lift.TiZhi + (int)(lift.ChengChangDu * 8.2);
                lift.MoFa += 3 * lift.ZhiLi + 7 * lift.JingShen + (int)(lift.ChengChangDu * 10.2);
                lift.GongJi += 15 + lift.LiLiang;// +(int)(lift.ChengChangDu * 0);
                lift.MoFaGongJi += lift.ZhiLi + (int)(lift.ChengChangDu * 1.8);
                lift.FangYu += lift.TiZhi + (int)(lift.ChengChangDu * 0.6);
                lift.MoFaFangYu += lift.JingShen + (int)(lift.ChengChangDu * 1.2);
            }
            else if (roleID == "3")
            {
                //神祭师
                lift.ShengMing += 35 + 100 * lev + 2 * lift.LiLiang + 8 * lift.TiZhi + (int)(lift.ChengChangDu * 14.8);
                lift.MoFa += 3 * lift.ZhiLi + 5 * lift.JingShen + (int)(lift.ChengChangDu * 6);
                lift.GongJi += 15 + lift.LiLiang + (int)(lift.ChengChangDu * 0.8);
                lift.MoFaGongJi += lift.ZhiLi + (int)(lift.ChengChangDu * 1.2);
                lift.FangYu += lift.TiZhi + (int)(lift.ChengChangDu * 1.2);
                lift.MoFaFangYu += lift.JingShen + (int)(lift.ChengChangDu * 0.6);
            }
            InitLevel3(roleID, lev, lift);
        }

        static void InitLevel3(string roleId, int lv, PetProperty lift)
        {
            lift.WuLiXiShou += 1.0 * lift.FangYu / (3000 + lift.FangYu);
            lift.MoFaXiShou += 1.0 * lift.MoFaFangYu / (3000 + lift.MoFaFangYu);
        }

        /// <summary>
        /// 生成随机资质
        /// </summary>
        /// <param name="ziZhi">资质范围如(1-5)</param>
        /// <returns></returns>
        private static int RandomZiZhi(string ziZhi)
        {

            int m = 1;
            string[] strs = ziZhi.Split('-');
            int min = Convert.ToInt32(strs[0]);
            int max = Convert.ToInt32(strs[1]);
            if (min <= 0)
                return 1;
            if (min == max)
                return min;

            if (min > max)
            {
                m = min;
                min = max;
                max = min;
            }

            GameConfig gc = GameConfigAccess.Instance.FindOneById("Z_0000");
            if (gc == null)
                return min;
            Variant v = gc.Value;
            if (v == null)
                return min;

            Dictionary<int, int> dic = new Dictionary<int, int>();
            int total = 0;
            for (int i = min; i <= max; i++)
            {
                Variant info = v.GetVariantOrDefault(i.ToString());
                if (info == null)
                    continue;
                int random = info.GetIntOrDefault("Random");
                total += random;
                dic.Add(i, random);
            }

            int n = NumberRandom.Next(0, total);

            //Console.WriteLine("N:" + n + ",Total:" + total);
            int cur = 0;
            foreach (var k in dic)
            {
                int t = cur + Convert.ToInt32(k.Value);
                if (n >= cur && n < t)
                {
                    return Convert.ToInt32(k.Key);
                }
                cur = t;
            }
            return min;
        }

        /// <summary>
        /// 成长度
        /// </summary>
        /// <param name="czd"></param>
        /// <returns></returns>
        private static int RandomCZD(string czd)
        {
            string[] strs = czd.Split('-');
            if (strs.Length != 2)
                return 0;
            int m = Convert.ToInt32(strs[0]);
            int n = Convert.ToInt32(strs[1]) + 1;
            return NumberRandom.Next(m, n);
        }

        /// <summary>
        /// 产生家政信息
        /// </summary>
        /// <param name="home"></param>
        /// <returns></returns>
        private static int HomeRandom(string home)
        {
            string[] strs = home.Split('-');
            int n = Convert.ToInt32(strs[0]);
            int m = Convert.ToInt32(strs[1]);
            return NumberRandom.Next(n, m);
        }
        /// <summary>
        /// 创建宠物技能
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private static Variant CreatePetSkill(Variant d)
        {
            if (d == null) return new Variant();

            int max = d.GetIntOrDefault("Max");
            IList list = d.GetValue<IList>("SkillList");
            //固定技能
            //List<Variant> dic = new List<Variant>();
            //随机技能
            List<Variant> sk = new List<Variant>();
            Variant skill = new Variant();
            foreach (Variant s in list)
            {
                if (skill.Count == max)
                {
                    return skill;
                }
                string skillID = s.GetStringOrDefault("SkillID");
                double rate = s.GetIntOrDefault("Rate");
                if (skill.ContainsKey(skillID))
                {
                    continue;
                }
                if (rate >= 100)
                {
                    //固定技能
                    skill.Add(skillID, s.GetIntOrDefault("Level"));
                    if (skill.Count >= max)
                    {
                        return skill;
                    }
                    continue;
                }

                //随机技能
                Variant tmp = new Variant();
                foreach (var item in s)
                {
                    tmp.Add(item.Key, item.Value);
                }
                sk.Add(tmp);
            }

            //交换技能所处的位置
            for (int i = 0; i < 500; i++)
            {
                int m = NumberRandom.Next(sk.Count);
                int n = NumberRandom.Next(sk.Count);
                if (m != n)
                {
                    Variant tmp = sk[m];
                    sk[m] = sk[n];
                    sk[n] = tmp;
                }
            }

            foreach (Variant cn in sk)
            {
                string skillID = cn.GetStringOrDefault("SkillID");
                if (skill.ContainsKey(skillID))
                    continue;

                double rate = cn.GetDoubleOrDefault("Rate");
                if (CheckLv(rate))
                {
                    skill.Add(skillID, cn.GetIntOrDefault("Level"));
                    if (skill.Count >= max)
                    {
                        return skill;
                    }
                }
            }
            return skill;
        }

        /// <summary>
        /// 创建宠技能槽
        /// </summary>
        private static List<Variant> CreatePetSkillGroove()
        {
            List<Variant> groove = new List<Variant>();
            for (int i = 0; i < 12; i++)
            {
                Variant d = new Variant();
                d.Add("P", i);//技能槽具体位置
                d.Add("SkillID", i > 9 ? "0" : "-1");//-1表示没有激活,0表示激活,但没有装备技能
                d.Add("Born", i > 9 ? 2 : 0);//0表示天生  
                d.Add("MaxUse", 0);//0表示被动
                d.Add("Key", string.Empty);//快捷键
                d.Add("Level", 0);//等级
                d.Add("SkillName", string.Empty);//技能名称

                groove.Add(d);
            }
            return groove;
        }

        /// <summary>
        /// 填入宠物技能
        /// </summary>
        /// <param name="list"></param>
        /// <param name="skill"></param>
        private static void SkillGroove(IList list, Variant skill)
        {
            foreach (string key in skill.Keys)
            {
                GameConfig gc = GameConfigAccess.Instance.FindOneById(key);
                if (gc != null)
                {
                    foreach (Variant d in list)
                    {
                        if (d.GetIntOrDefault("Born") == 2)
                            continue;
                        if (d["SkillID"].ToString() == "-1")
                        {
                            d["SkillID"] = key;
                            d["Born"] = 0;
                            d["MaxUse"] = gc.UI.GetIntOrDefault("MaxUse");
                            d["Key"] = string.Empty;
                            d["Level"] = skill[key];
                            d["SkillName"] = gc.Name;
                            break;
                        }
                    }
                }
            }
            int a = 0;
            foreach (Variant d in list)
            {
                if (d["SkillID"].ToString() == "-1")
                {
                    d["SkillID"] = 0;
                    a++;
                }
                if (a == 1)
                    break;
            }
        }

        /// <summary>
        /// 判断资质提升功率
        /// </summary>
        /// <param name="i">要求提升的资质</param>
        /// <returns>true成功，false失败</returns>
        public static bool UpZiZhiLv(int i)
        {
            int r = NumberRandom.Next(100);
            switch (i)
            {
                case 2:
                    return r > 49 ? false : true;
                case 3:
                    return r > 34 ? false : true;
                case 4:
                    return r > 19 ? false : true;
                case 5:
                    return r > 5 ? false : true;
            }
            return false;
        }

        /// <summary>
        /// 喂养消耗道具数量
        /// </summary>
        /// <param name="i"></param>
        /// <returns>0表示配置不正确</returns>
        public static int KeepNeedGoods(int i)
        {
            if (i <= 400) return 1;
            else if (i <= 720) return 2;
            else if (i <= 1220) return 4;
            else if (i <= 1720) return 8;
            else return 16;
        }

        /// <summary>
        /// 宠物相关属性重新计算
        /// </summary>
        /// <param name="p">宠物技能</param>
        /// <param name="ex">玩家所有技能</param>
        /// <param name="up">是否升级操作</param>
        public static void PetReset(Pet p, PlayerEx ex, bool up, Mounts m)
        {
            if (p == null)
                return;
            List<GameConfig> skillList = GameConfigAccess.Instance.Find(MainType.Skill, SkillSub.AdditionJ);
            Variant v = p.Value;

            //宠物所有技能
            Variant petSkill = v.GetVariantOrDefault("Skill");

            //将宠物所有家族技能移除
            foreach (GameConfig gc in skillList)
            {
                if (petSkill == null)
                    break;
                if (petSkill.ContainsKey(gc.ID))
                {
                    petSkill.Remove(gc.ID);
                }
            }

            int level = v.GetIntOrDefault("PetsLevel");
            int ccd = v.GetVariantOrDefault("ChengChangDu").GetIntOrDefault("V");
            int exp = v.GetVariantOrDefault("Experience").GetIntOrDefault("V");

            if (ex != null && ex.Value != null)
            {
                foreach (GameConfig gk in skillList)
                {
                    if (ex.Value.ContainsKey(gk.ID))
                    {
                        if (petSkill == null)
                        {
                            v["Skill"] = new Variant();
                            petSkill = v.GetValueOrDefault<Variant>("Skill");
                        }
                        if (petSkill.ContainsKey(gk.ID))
                        {
                            petSkill[gk.ID] = ex.Value[gk.ID];
                        }
                        else
                        {
                            petSkill.Add(gk.ID, ex.Value[gk.ID]);
                        }
                    }
                }
            }


            PetProperty life = GetSkillPro(p, level);
            life.ChengChangDu = ccd;
            //得到坐骑加成
            Variant info = MountsAccess.Instance.MountsPetLife(m, p.Value.GetStringOrDefault("PetsType"));
            if (info != null)
            {
                life.Add(info);
            }            
            RefreshPetProperty(v, life, exp, up);
            p.Save();
        }

        /// <summary>
        /// 得到家园宠物列表
        /// </summary>
        public Variant GetPetList(PlayerEx b2)
        {
            //表示存在溜宠物变化
            IList c2 = b2.Value.GetValue<IList>("C");
            Variant m = new Variant();
            m.Add("ID", b2.PlayerID);
            m.Add("Cur", b2.Value.GetIntOrDefault("Cur"));//当前允许放养数量
            List<Variant> lv = new List<Variant>();
            foreach (Variant v in c2)
            {
                Variant n = GetPetModel(v);
                if (n != null)
                {
                    lv.Add(n);
                }
            }
            m.Add("PetsList", lv);
            return m;
        }

        /// <summary>
        /// 得到单个宠物基本信息
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Variant GetPetModel(Variant v)
        {
            string id = v.GetStringOrDefault("E");
            if (!string.IsNullOrEmpty(id))
            {
                Pet p = PetAccess.Instance.FindOneById(id);
                if (p == null) 
                    return null;
                Variant t = v.GetValueOrDefault<Variant>("T");
                return GetPetModel(p, t);
            }
            return null;
        }

        /// <summary>
        /// 得到单个宠物基本信息
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Variant GetPetModel(Pet p, Variant v)
        {
            if (p == null) 
                return null;
            Variant pv = p.Value;
            if (pv == null)
                return null;
            Variant n = new Variant();
            n.Add("ID", p.ID);
            n.Add("Skill", pv.GetValueOrDefault<Variant>("Skill"));
            n.Add("PetsID", pv.GetStringOrDefault("PetsID"));
            n.Add("PetsRank", pv.GetIntOrDefault("PetsRank"));
            n.Add("Skin", pv.GetStringOrDefault("Skin"));
            n.Add("Name", p.Name);
            n.Add("PetsLevel", pv.GetIntOrDefault("PetsLevel"));
            n.Add("ShengMing", pv.GetValueOrDefault<Variant>("ShengMing"));
            n.Add("MoFa", pv.GetValueOrDefault<Variant>("MoFa"));

            int ccd = pv.GetVariantOrDefault("ChengChangDu").GetIntOrDefault("V");
            n.Add("ChengChangDu", ccd);                

            n.Add("Awards", v);
            n.Add("ZiZhi", pv.GetIntOrDefault("ZiZhi"));
            return n;
        }


        public void Unload(string fullPath) { }

        /// <summary>
        ///是否成功
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static bool CheckLv(double rate)
        {
            int m = NumberRandom.Next(1, 100001);
            double n = rate * 1000;
            if (n > m) return true;
            return false;
        }

        #endregion



        #region 疲劳值处理

        /// <summary>
        /// 疲劳增加值
        /// </summary>
        /// <param name="p">宠物</param>
        /// <param name="addFatigue">增加值</param>
        public bool FatigueAdd(Pet p, int addFatigue)
        {
            Variant t = p.Value;
            int level = t.GetIntOrDefault("PetsLevel");
            int maxFatigue = MaxFatigue(level);
            if (t.GetIntOrDefault("Fatigue") + addFatigue >= maxFatigue)
            {
                t["Fatigue"] = maxFatigue;
            }
            else
            {
                t.SetOrInc("Fatigue", addFatigue);
            }
            return p.Save();
        }

        /// <summary>
        /// 疲劳恢复值
        /// </summary>
        /// <param name="p">宠物</param>
        /// <param name="backFatigue">恢复点</param>
        /// <returns></returns>
        public bool FatigueBack(Pet p, int backFatigue)
        {
            Variant v = p.Value;
            //照顾恢复疲劳值
            if (backFatigue >= v.GetIntOrDefault("Fatigue"))
            {
                v["Fatigue"] = 0;
            }
            else
            {
                v["Fatigue"] = v.GetIntOrDefault("Fatigue") - backFatigue;
            }
            return p.Save();
        }


        /// <summary>
        /// 判断宠物是否已经过度疲劳
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsFatigue(Pet p)
        {
            Variant v = p.Value;
            int level = v.GetIntOrDefault("PetsLevel");
            if (v.GetIntOrDefault("Fatigue") >= MaxFatigue(level))
                return true;
            return false;
        }

        /// <summary>
        /// 通过等级得到宠物最大疲劳值
        /// </summary>
        /// <param name="level">宠物等级</param>
        /// <returns>得到最大疲劳值</returns>
        public int MaxFatigue(int level)
        {
            int max = (int)Math.Ceiling(50 + (level - 1) * 9.5);
            return max;
        }

        /// <summary>
        /// 加工产生的疲劳值
        /// </summary>
        /// <param name="p">宠物</param>
        /// <param name="a">需要材料个数</param>
        /// <param name="b">家政值总和</param>
        /// <param name="c">投放家政值总和</param>
        public bool HomeFatigue(Pet p, int a, int b, int c)
        {
            int addFatigue = HomeFatigue(a, b, c);
            return FatigueAdd(p, addFatigue);
        }

        /// <summary>
        /// 照顾疲劳回复值
        /// </summary>
        /// <param name="p">宠物</param>
        public void FatigueBack(Pet p)
        {
            Variant v = p.Value;
            int level = v.GetIntOrDefault("PetsLevel");
            //照顾恢复疲劳值
            int fb = FatigueBack(level);
            FatigueBack(p, fb);
        }


        /// <summary>
        /// 加工产生的疲劳值
        /// </summary>
        /// <param name="a">需要材料个数</param>
        /// <param name="b">家政值总和</param>
        /// <param name="c">投放家政值总和</param>
        /// <returns></returns>
        public int HomeFatigue(int a, int b, int c)
        {
            //double m = (double)a / (a + 100);
            //double n = (double)b / c;
            //return Convert.ToInt32(Math.Ceiling(20 + (double)(8000 * a * b) / ((a + 8000) * c)));
            //return (int)Math.Ceiling(0.01 * Math.Pow(a, 2) * b / c);

            long a3 = ((long)a) * a * a;
            double a1 = a3 / (a3 + 1800000.0);
            double a2 = Math.Sqrt(b) / Math.Sqrt(c);
            return (int)(Math.Ceiling(5 + 200 * a1 * a2));
        }





        /// <summary>
        /// 家园采集，挖掘,养殖产生的疲劳值
        /// </summary>
        /// <param name="m">对应家政值</param>
        /// <returns></returns>
        public int HomeFatigue(int m)
        {
            decimal n = 20 * 200 / m;
            return (int)Math.Ceiling(n);
        }

        /// <summary>
        /// 照顾疲劳回复值
        /// </summary>
        /// <param name="level">宠物等级</param>
        /// <returns></returns>
        private int FatigueBack(int level)
        {
            return (30 + level * 5) * 2;
        }

        /// <summary>
        /// 吸星疲劳回复值
        /// </summary>
        /// <param name="level">宠物等级</param>
        /// <returns></returns>
        public int AbsorbBack(int level)
        {
            return 5 * level;
        }

        /// <summary>
        /// 竞技场增加疲劳值
        /// </summary>
        /// <param name="level">宠物等级</param>
        /// <returns></returns>
        public int ArenaFatigue(int level)
        {
            return 10 + level;
        }
        #endregion

        #region 放养奖励

        /// <summary>
        /// 放养宠物奖励
        /// </summary>
        /// <param name="level">角色等级</param>
        /// <param name="v"></param>
        /// <param name="petID">放养的宠物</param>
        /// <param name="playerID">角色ID</param>
        /// <param name="px">出战宠</param>
        public Variant CreateAward(int level, string petID, string playerID, Pet px)
        {
            Variant v = new Variant();
            //宠物id
            Pet p = PetAccess.Instance.GetPetByID(petID, playerID);
            if (p == null) 
                return null;
            //放养开始时间
            GameConfig gc = GameConfigAccess.Instance.FindOneById(p.Value.GetStringOrDefault("PetsID"));
            if (gc == null)
                return null;
            Variant cv = gc.Value;
            if (cv == null)
                return null;

            int careTime = cv.GetIntOrDefault("CareTime");
            //领奖时间
            v.Add("EndTime", DateTime.UtcNow.AddSeconds(careTime));

            //晶币//石币
            Variant score = cv.GetValueOrDefault<Variant>("Score");
            if (score != null)
            {
                if (NumberRandom.RandomHit(score.GetDoubleOrDefault("P")))
                {
                    int min = score.GetIntOrDefault("Min");
                    int max = score.GetIntOrDefault("Max") + 1;
                    int sc = NumberRandom.Next(min, max);
                    if (sc > 0)
                    {
                        if (sc > 0) v.Add("Score", sc);
                    }
                }
            }

            //出征宠的相关信息            
            int p1 = cv.GetIntOrDefault("P1exp") * level;
            int p2 = cv.GetIntOrDefault("P2exp") * (px == null ? 0 : px.Value.GetIntOrDefault("PetsLevel"));
            int p3 = cv.GetIntOrDefault("P3exp") * p.Value.GetIntOrDefault("PetsLevel");
            if (p1 > 0) v.Add("P1exp", p1);
            if (p2 > 0) v.Add("P2exp", p2);
            if (p3 > 0) v.Add("P3exp", p3);
            return v;
        }
        
        #endregion
    }
}
