using System;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using Sinan.Data;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.FrontServer
{
    partial class PlayerBusiness
    {
        Dictionary<string, Tuple<int, GameConfig>> m_fixBuffer;

        /// <summary>
        /// 非加成的被动技能
        /// 森严/打磨/劈石/趁热/附骨/凝固
        /// 地脉/崩裂/感染/凝霜/玄机/淡定/蔓延
        /// </summary>
        public Dictionary<string, Tuple<int, GameConfig>> FixBuffer
        {
            get { return m_fixBuffer; }
        }

        /// <summary>
        /// 计算称号加成
        /// </summary>
        private void TitleAddition(PlayerProperty p)
        {
            if (m_title != null && m_title.Value != null)
            {
                //称号前缀
                string prefix = m_title.Value.GetStringOrDefault("Prefix");
                if (!string.IsNullOrEmpty(prefix))
                {
                    GameConfig gs = GameConfigAccess.Instance.FindOneById(prefix);
                    if (gs != null && gs.MainType == MainType.Title)
                    {
                        p.Add(gs.Value);
                    }
                }
                //称号后缀
                string suffix = m_title.Value.GetStringOrDefault("Suffix"); ;
                if (!string.IsNullOrEmpty(suffix))
                {
                    GameConfig gs = GameConfigAccess.Instance.FindOneById(suffix);
                    if (gs != null && gs.MainType == MainType.Title)
                    {
                        p.Add(gs.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 初始化(被动技能和称号)加成
        /// 训练/强健/沉稳/睿智/石盾/天人等(前缀称号/后缀称号)
        /// </summary>
        private void ResetSkillsAdd(PlayerProperty p)
        {
            if (m_skill != null && m_skill.Value != null)
            {
                foreach (var s in m_skill.Value)
                {
                    GameConfig gs = GameConfigAccess.Instance.FindOneById(s.Key);
                    if (gs == null) continue;
                    if (gs.SubType == SkillSub.Addition || gs.SubType == SkillSub.AdditionJ)
                    {
                        string level = s.Value.ToString();
                        p.Add(gs.Value[level] as Variant);
                    }
                }
            }
            TitleAddition(p);
            InitFixBuffer();
            AddStar(p);
            AddDressing(p);
            m_sPro.Value = p.GetChange();
            m_sPro.Save();
        }

        /// <summary>
        /// 初始化固定Buffer
        /// </summary>
        private void InitFixBuffer()
        {
            if (m_skill != null && m_skill.Value != null)
            {
                m_fixBuffer = SkillHelper.InitFixBuffer(m_skill.Value);
            }
        }

        /// <summary>
        /// 初始化装备的加成..
        /// </summary>
        /// <returns></returns>
        private void ResetEquipsAdd(PlayerProperty p)
        {
            AddDressing(p);
            if (m_equips != null && m_equips.Value != null)
            {
                Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();

                for (int i = 0; i <= 9; i++)
                {
                    var v = m_equips.Value.GetVariantOrDefault("P" + i);
                    if (v == null)
                        continue;
                    string equiD = v.GetStringOrDefault("E");
                    if (string.IsNullOrEmpty(equiD))
                        continue;

                    Goods gs = GoodsAccess.Instance.FindOneById(equiD);
                    if (gs == null)
                        continue;

                    Variant info = gs.Value;
                    if (info == null)
                        continue;
                    //持久度
                    var st = info.GetVariantOrDefault("Stamina");
                    if (st != null && st.GetIntOrDefault("V") <= 0)
                        continue;

                    object life;
                    if (info.TryGetValueT("Life", out life))
                    {
                        p.Add(life as Variant);
                        //宝石加成..
                        BaoShiAddition(p, gs);
                    }

                    //装备附加属性
                    object affix;
                    if (info.TryGetValueT("Affix", out affix))
                    {
                        p.Add(affix as Variant);
                    }

                    //判断是否是套装
                    if (!info.ContainsKey("EntireID"))
                        continue;

                    string entrieid = info.GetStringOrDefault("EntireID");
                    if (string.IsNullOrEmpty(entrieid))
                        continue;
                    List<string> goods;
                    string goodsid = v.GetStringOrDefault("G");
                    if (dic.TryGetValue(entrieid, out goods))
                    {
                        //if (!goods.Contains(goodsid))
                        //{
                        //    goods.Add(goodsid);
                        //}

                        //趣游要求相同的ID也要计算套装数量
                        goods.Add(goodsid);
                    }
                    else
                    {
                        goods = new List<string>() { goodsid };
                        dic.Add(entrieid, goods);
                    }
                }
                //套装属性
                Entire(p, dic);
                m_ePro.Value = p.GetChange();
                m_ePro.Save();
            }

        }

        /// <summary>
        /// 套装属性
        /// </summary>
        /// <param name="p"></param>
        /// <param name="dic"></param>
        private void Entire(PlayerProperty p, Dictionary<string, List<string>> dic)
        {
            foreach (var item in dic)
            {
                GameConfig gc = GameConfigAccess.Instance.FindOneById(item.Key);
                if (gc == null)
                    continue;

                Variant v = gc.Value;
                if (v == null)
                    continue;

                Variant life = v.GetVariantOrDefault("Life");
                if (life == null)
                    continue;

                //套装数量
                List<string> num = item.Value as List<string>;

                foreach (var o in life)
                {
                    int m = 0;
                    if (int.TryParse(o.Key, out m))
                    {
                        if (m > num.Count)
                            continue;
                        p.Add(o.Value as Variant);
                    }
                }

                //object o;
                //if (life.TryGetValueT(strs.Count.ToString(), out o))
                //{
                //    p.Add(o as Variant);
                //}
            }
        }

        /// <summary>
        /// 星座加成
        /// </summary>
        private void AddStar(PlayerProperty p)
        {
            if (Star == null || Star.Value == null)
                return;

            GameConfig gc = GameConfigAccess.Instance.FindOneById("ST_00001");
            if (gc == null)
                return;

            Variant life = new Variant();
            for (int j = 1; j < 13; j++)
            {
                IList o = Star.Value.GetValue<IList>(j.ToString());
                if (o == null)
                    continue;
                //得到某个星座配置
                Variant v = gc.Value.GetVariantOrDefault(j.ToString());
                if (v == null)
                    continue;

                //得到要加的属性
                string name = v.GetStringOrDefault("Name");
                double num = 0;
                Variant c = v.GetVariantOrDefault("B");
                if (c != null)
                {
                    foreach (int i in o)
                    {
                        //num += PartAccess.Instance.StarForm(c.GetDoubleOrDefault("A"), i + 1, c.GetIntOrDefault("B"), c.GetDoubleOrDefault("C"));
                        //num * Math.Pow(v.GetDoubleOrDefault("A"), n * (n - 1) / 2)
                        //星阵属性加成
                        double b = c.GetDoubleOrDefault("B");
                        double a = c.GetDoubleOrDefault("A");
                        double n = (i + 1) * i / 2;
                        num += Convert.ToInt32(b * Math.Pow(a, n));
                    }
                }

                if (o.Count == 14)
                {
                    //激活所有的守护星额外加成
                    Variant other = v.GetVariantOrDefault("Life");
                    if (other != null)
                    {
                        foreach (var k in other)
                        {
                            life.SetOrInc(k.Key, Convert.ToDouble(k.Value));
                        }
                    }
                }
                if (name == "BaoJiShangHai")
                {
                    num = num / 100;
                }
                life.SetOrInc(name, num);
            }
            p.Add(life);
        }


        /// <summary>
        /// 时装属性加成
        /// </summary>
        /// <param name="p"></param>
        private void AddDressing(PlayerProperty p)
        {
            Variant v = Wardrobe.Value;
            if (v == null)
                return;

            string sz = v.GetStringOrDefault("ShiZhuang");
            if (string.IsNullOrEmpty(sz))
                return;

            GameConfig gc = GameConfigAccess.Instance.FindOneById(sz);
            if (gc == null)
                return;

            Variant life = gc.Value.GetVariantOrDefault("Life");
            if (life == null)
                return;

            p.Add(life);
        }

        /// <summary>
        /// 计算宝石加成
        /// </summary>
        /// <param name="p"></param>
        /// <param name="gs"></param>
        private static void BaoShiAddition(PlayerProperty p, Goods gs)
        {
            object baoShiInfo;
            if (gs.Value.TryGetValueT("BaoShiInfo", out baoShiInfo)
                && baoShiInfo is IDictionary<string, object>)
            {
                foreach (var x in baoShiInfo as IDictionary<string, object>)
                {
                    if (x.Value != null)
                    {
                        string bID = x.Value.ToString();
                        //"-1":无孔,"0"或"":无宝石
                        if (bID == string.Empty || bID == "0" || bID == "-1")
                        {
                            continue;
                        }
                        var bs = GameConfigAccess.Instance.FindOneById(bID);
                        p.Add(bs.Value.GetVariantOrDefault("Life"));
                    }
                }
            }
        }

        /// <summary>
        /// 获取技能等级
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public int GetSkillLev(string skillID)
        {
            return m_skill.Value.GetIntOrDefault(skillID);
        }

        /// <summary>
        /// 刷新技能(称号)
        /// </summary>
        public void RefeshSkill()
        {
            RefreshPlayer(string.Empty, string.Empty, ResetSkillsAdd);
        }


        /// <summary>
        /// 学习宠物技能(御宠图鉴)
        /// </summary>
        /// <param name="variant"></param>
        public bool AddPetBook(Variant c)
        {
            string skillID = c.GetStringOrDefault("SkillID");
            if (string.IsNullOrEmpty(skillID)) return false;

            int level = c.GetIntOrDefault("SkillLev");
            var skill = m_petBook.Value.GetValueOrDefault<List<object>>(skillID);
            if (skill == null)
            {
                skill = new List<object>();
                m_petBook.Value[skillID] = skill;
            }
            if (skill.Contains(level))
            {
                return false;
            }
            int mustslevel = level - 1;
            if (mustslevel > 0 && (!skill.Contains(mustslevel)))
            {
                return false;
            }
            skill.Add(level);
            m_petBook.Save();
            this.Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_petBook));
            return true;
        }


        /// <summary>
        /// 时装信息
        /// </summary>
        public void ShiZhuangInfo()
        {

            Variant wv = m_wardrobe.Value;
            if (wv == null)
                return;

            string sz = wv.GetStringOrDefault("ShiZhuang");
            Variant v = m_equips.Value;
            if (v == null)
                return;
            Variant ev = v.GetVariantOrDefault("P11");
            if (ev == null)
                return;
            if (string.IsNullOrEmpty(sz))
            {
                Variant shengTi = RoleManager.Instance.GetAllRoleConfig(RoleID);
                Coat = shengTi.GetStringOrDefault("Coat");
                BurdenManager.BurdenClear(ev);
            }
            else
            {
                GameConfig gc = GameConfigAccess.Instance.FindOneById(sz);
                if (gc == null)
                    return;
                Coat = gc.UI.GetStringOrDefault("Skin");
                ev["E"] = sz;
                ev["G"] = sz;
                ev["A"] = 1;
            }
            if (m_equips.Save())
            {
                SaveClothing();
                RefreshPlayer("Coat", Coat);
                Call(GoodsCommand.GetEquipPanelR, true, v);
            }
        }

        /// <summary>
        /// 坐骑信息
        /// </summary>
        public void MountsInfo()
        {
            Variant v = m_equips.Value;
            if (v == null)
                return;
            Variant ev = v.GetVariantOrDefault("P10");
            if (ev == null)
                return;
            if (m_mounts == null || m_mounts.Status == 0)
            {
                Variant shengTi = RoleManager.Instance.GetAllRoleConfig(RoleID);
                Mount = shengTi.GetStringOrDefault("Mount");
                BurdenManager.BurdenClear(ev);
            }
            else
            {
                GameConfig gc = GameConfigAccess.Instance.FindOneById(m_mounts.MountsID);
                if (gc == null)
                    return;

                Variant mv = gc.Value;
                if (mv == null)
                    return;
                Variant jh = mv.GetVariantOrDefault("JinHua");
                if (jh == null)
                    return;
                Variant info = jh.GetVariantOrDefault(m_mounts.Rank.ToString());
                if (info == null)
                    return;
                Mount = info.GetStringOrDefault("Skin");
                ev["E"] = m_mounts.ID;
                ev["G"] = m_mounts.MountsID;
                ev["A"] = m_mounts.Rank;
            }
            if (m_equips.Save())
            {
                SaveClothing();
                RefreshPlayer("Mount", Mount);
                Call(GoodsCommand.GetEquipPanelR, true, v);
            }
        }
    }
}