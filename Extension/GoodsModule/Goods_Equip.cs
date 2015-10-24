using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Data;
using Sinan.FrontServer;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Observer;
using Sinan.Util;
using Sinan.Log;
using Sinan.FastSocket;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.GoodsModule
{
    partial class GoodsMediator : AysnSubscriber
    {
        private static int p111004 = 0;//护腕
        private static int p111008 = 0;//戒子
        private static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 换装操作
        /// </summary>
        /// <param name="note"></param>
        private void Dress(UserNote note, Goods g)
        {
            //表示角色正在战斗中，客户端提交信息不进行处理
            if (note.Player.AState == ActionState.Fight)
                return;
            string goodsid = note.GetString(0);
            if (g == null)
            {
                note.Call(GoodsCommand.UseGoodsR, false, TipManager.GetMessage(GoodsReturn.NoGoodsInfo));
                return;
            }
            int check = DressLimit(note.Player, g);
            if (check != 0)
            {
                note.Call(GoodsCommand.UseGoodsR, false, TipManager.GetMessage(check));
                return;
            }
            //得到装备面板
            PlayerEx panel = note.Player.Equips;
            if (panel == null)
            {
                panel = note.Player.Value["Equips"] as PlayerEx;
                if (panel == null)
                {
                    //LogWrapper.Error("角色:" + note.Player.Name + "装被面板为空");
                    return;
                }
            }

            Variant pv = panel.Value;

            PlayerEx b = note.Player.B0;
            if (b == null)
            {
                note.Call(GoodsCommand.UseGoodsR, false, TipManager.GetMessage(GoodsReturn.EquipError));
                return;
            }

            IList c = b.Value.GetValue<IList>("C");

            string str = g.Value.GetStringOrDefault("GoodsType");
            //得到装备可以存放的位置
            GameConfig gc = GameConfigAccess.Instance.FindOneById(g.GoodsID);
            if (gc == null)
            {
                note.Call(GoodsCommand.UseGoodsR, false, TipManager.GetMessage(GoodsReturn.EquipError));
                return;
            }

            IDictionary<string, object> k = null;
            string name = string.Empty;

            switch (str)
            {

                case "111001"://武器
                    k = pv["P0"] as IDictionary<string, object>;
                    name = "P0";
                    break;
                case "111002"://头盔
                    k = pv["P5"] as IDictionary<string, object>;
                    name = "P5";
                    break;
                case "111003"://衣服
                    k = pv["P6"] as IDictionary<string, object>;
                    name = "P6";
                    break;
                case "111004"://护腕
                    IDictionary<string, object> p3 = pv["P3"] as IDictionary<string, object>;
                    IDictionary<string, object> p8 = pv["P8"] as IDictionary<string, object>;
                    if (p3.GetStringOrEmpty("E") == string.Empty)
                    {
                        k = p3;
                        name = "P3";
                    }
                    else if (p8.GetStringOrEmpty("E") == string.Empty)
                    {
                        k = p8;
                        name = "P8";
                    }
                    else if (p111004 == 0)
                    {
                        k = p3;
                        name = "P3";
                        p111004 = p111004 == 0 ? 1 : 0;
                    }
                    else
                    {
                        k = p8;
                        name = "P8";
                        p111004 = p111004 == 0 ? 1 : 0;
                    }
                    break;

                case "111005"://腰带
                    k = pv["P2"] as IDictionary<string, object>;
                    name = "P2";
                    break;
                case "111006"://鞋子
                    k = pv["P7"] as IDictionary<string, object>;
                    name = "P7";
                    break;
                case "111007"://项链
                    k = pv["P1"] as IDictionary<string, object>;
                    name = "P1";
                    break;
                case "111008"://戒子
                    IDictionary<string, object> p4 = pv["P4"] as IDictionary<string, object>;
                    IDictionary<string, object> p9 = pv["P9"] as IDictionary<string, object>;
                    if (p4.GetStringOrEmpty("E") == string.Empty)
                    {
                        k = p4;
                        name = "P4";
                    }
                    else if (p9.GetStringOrEmpty("E") == string.Empty)
                    {
                        k = p9;
                        name = "P9";
                    }
                    else if (p111008 == 0)
                    {
                        k = p4;
                        name = "P4";
                        p111008 = p111008 == 0 ? 1 : 0;
                    }
                    else
                    {
                        k = p9;
                        name = "P9";
                        p111008 = p111008 == 0 ? 1 : 0;
                    }

                    break;

                //case "111010"://坐骑
                //    k = pv["P10"] as IDictionary<string, object>;
                //    name = "P10";
                //    break;

                //case "111000"://时装
                //    k = pv["P11"] as IDictionary<string, object>;
                //    name = "P11";
                //    break;
            }

            if (k == null || name == string.Empty)
            {
                note.Call(GoodsCommand.UseGoodsR, false, TipManager.GetMessage(GoodsReturn.EquipError));
                return;
            }

            IDictionary<string, object> info = null;
            foreach (IDictionary<string, object> p in c)
            {
                if (g.ID == p.GetStringOrDefault("E"))
                {
                    //得到装备所在格子的信息
                    info = p;
                    break;
                }
            }

            if (info == null)
            {
                note.Call(GoodsCommand.UseGoodsR, false, TipManager.GetMessage(GoodsReturn.BurdenError));
                return;
            }

            string name2 = null;
            string skin = gc.UI.GetStringOrDefault("Skin");
            switch (str)
            {
                //case "111000":
                //    //时装
                //    name2 = "Coat";
                //    note.Player.Coat = skin;
                //    break;
                case "111001":
                    //武器
                    name2 = "Weapon";
                    note.Player.Weapon = skin;
                    break;
                case "111003":
                    //衣服
                    name2 = "Body";
                    note.Player.Body = skin;
                    break;
                //case "111010":
                //    //坐骑
                //    name2 = "Mount";
                //    note.Player.Mount = skin;
                //    break;
            }

            Variant tmp = new Variant();
            tmp["E"] = k["E"];
            tmp["G"] = k["G"];
            tmp["A"] = k["A"];
            tmp["S"] = k["S"];
            tmp["H"] = k["H"];
            tmp["D"] = k["D"];
            tmp["T"] = k["T"];
            //tmp["R"] = k["R"];
            //换装

            string g1 = info.GetStringOrDefault("G");
            string g2 = string.Empty;
            if (k["E"].ToString() == string.Empty)
            {
                k["E"] = info["E"];
                k["G"] = info["G"];
                k["A"] = info["A"];
                k["S"] = info["S"];
                k["H"] = info["H"];
                k["D"] = info["D"];
                k["T"] = info["T"];
                //k["R"] = info["R"];
                //更新包袱

                BurdenManager.BurdenClear(info);
            }
            else
            {
                k["E"] = info["E"];
                k["G"] = info["G"];
                k["A"] = info["A"];
                k["S"] = info["S"];
                k["H"] = info["H"];
                k["D"] = info["D"];
                k["T"] = info["T"];
                //k["R"] = info["R"];
                //note.Player.UserNote_T(k.GetStringOrDefault("G"));//表示两类道具交换,都要通知任务
                //更新包袱
                info["E"] = tmp["E"];
                info["G"] = tmp["G"];
                info["A"] = tmp["A"];
                info["S"] = tmp["S"];
                info["H"] = tmp["H"];
                info["D"] = tmp["D"];
                info["T"] = tmp["T"];
                g2 = tmp.GetStringOrDefault("G");
                //note.Player.UserNote_T(info.GetStringOrDefault("G"));
                //info["R"] = tmp["R"];
            }

            b.Save();
            note.Player.SaveClothing();
            panel.Save();
            note.Player.RefreshPlayer(name2, skin);
            note.Call(GoodsCommand.UseGoodsR, true, g.ID);
            note.Call(GoodsCommand.GetEquipPanelR, true, pv);

            note.Player.UpdateBurden();
            //通知任务
            if (!string.IsNullOrEmpty(g1))
            {
                note.Player.UpdateTaskGoods(g1);
            }
            if (!string.IsNullOrEmpty(g2))
            {
                note.Player.UpdateTaskGoods(g2);
            }
        }

        /// <summary>
        /// 卸装操作0
        /// </summary>
        /// <param name="note"></param>
        private void Uninstall(UserNote note)
        {
            //表示角色正在战斗中，客户端提交信息不进行处理
            if (note.Player.AState == ActionState.Fight)
                return;
            string goodsid = note.GetString(0);
            //得到装备面板
            PlayerEx ex = note.Player.Equips;
            Variant ev = ex.Value;

            Goods g = GoodsAccess.Instance.GetGoodsByID(goodsid, note.PlayerID);
            if (g == null)
            {
                note.Call(GoodsCommand.UninstallR, false, TipManager.GetMessage(GoodsReturn.EquipError));
                return;
            }
            PlayerEx b = note.Player.B0;
            IList c = b.Value.GetValue<IList>("C");
            Variant t = null;
            foreach (Variant con in c)
            {
                if (string.IsNullOrEmpty(con.GetStringOrDefault("E")))
                {
                    t = con;
                    break;
                }

            }
            if (t == null)
            {
                note.Call(GoodsCommand.UninstallR, false, TipManager.GetMessage(GoodsReturn.BurdenB0Full));
                return;
            }
            Variant v = g.Value;

            GameConfig gc = GameConfigAccess.Instance.FindOneById(g.GoodsID);
            if (gc == null)
            {
                note.Call(GoodsCommand.UninstallR, false, TipManager.GetMessage(GoodsReturn.EquipError));
                return;
            }
            string goodstype = v.GetStringOrDefault("GoodsType");

            Variant ShengTi = RoleManager.Instance.GetAllRoleConfig(note.Player.RoleID);
            string name = string.Empty;
            string value = string.Empty;
            switch (goodstype)
            {
                //case "111000":
                //    //时装
                //    name = "Coat";
                //    value = ShengTi.GetStringOrDefault("Coat");
                //    note.Player.Coat = value;
                //    break;
                case "111001":
                    //武器
                    name = "Weapon";
                    value = ShengTi.GetStringOrDefault("Weapon");
                    note.Player.Weapon = value;
                    break;
                case "111003":
                    //衣服
                    name = "Body";
                    value = ShengTi.GetStringOrDefault("Body");
                    note.Player.Body = value;
                    break;
                //case "111010":
                //    //坐骑
                //    name = "Mount";
                //    value = ShengTi.GetStringOrDefault("Mount");
                //    note.Player.Mount = value;
                //    break;
            }


            bool ischange = false;
            string gid = "";
            foreach (Variant p in ev.Values)
            {
                if (p.GetStringOrDefault("E") == goodsid)
                {
                    gid = p.GetStringOrDefault("G");
                    t["E"] = p["E"];
                    t["G"] = p["G"];
                    t["A"] = p["A"];
                    t["S"] = p["S"];
                    t["H"] = p["H"];
                    t["D"] = p["D"];
                    t["T"] = p["T"];
                    //tmp["R"] = p["R"];
                    //卸装成功
                    //是否存在与任务进行关联

                    BurdenManager.BurdenClear(p);
                    ischange = true;
                    break;
                }
            }

            if (ischange)
            {
                b.Save();
                ex.Save();
                note.Player.SaveClothing();
                note.Player.RefreshPlayer(name, value);

                note.Call(GoodsCommand.UninstallR, true, goodsid);
                note.Call(GoodsCommand.GetEquipPanelR, true, ev);
                note.Player.UpdateBurden();
                note.Player.UpdateTaskGoods(gid);
            }
            else
            {
                note.Call(GoodsCommand.UninstallR, false, TipManager.GetMessage(GoodsReturn.EquipError));
            }
        }

        /// <summary>
        /// 礼包
        /// </summary>
        /// <param name="note"></param>
        /// <param name="g"></param>
        private void LiBao(UserNote note, Goods g)
        {
            int check = DressLimit(note.Player, g);
            if (check != 0)
            {
                note.Call(GoodsCommand.UseGoodsR, false, TipManager.GetMessage(check));
                return;
            }
            Variant package = note.GetVariant(0);
            int p = package.GetIntOrDefault("P");
            int isbinding = g.Value.GetIntOrDefault("IsBinding");
            Variant libao = g.Value.GetVariantOrDefault("LiBao");
            if (libao == null)
            {
                note.Call(GoodsCommand.UseGoodsR, false, string.Format(TipManager.GetMessage(GoodsReturn.LiBao1), g.Name));
                return;
            }
            Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
            int bond = 0;
            int score = 0;
            int coin = 0;
            if (!libao.ContainsKey("Award"))
            {
                #region 礼包固定信息
                foreach (var item in libao)
                {
                    switch (item.Key)
                    {
                        case "Bond":
                            bond = libao.GetIntOrDefault(item.Key);
                            break;
                        case "Score":
                            score = libao.GetIntOrDefault(item.Key);
                            break;
                        case "Coin":
                            coin = libao.GetIntOrDefault(item.Key);
                            break;
                        default:
                            GetLiBaoAward(dic, item.Key, (int)item.Value, isbinding);
                            break;
                    }
                }
                #endregion
            }
            else
            {
                #region 随机奖励礼包

                Variant award = libao.GetValue<Variant>("Award");
                Dictionary<string, int> goods = new Dictionary<string, int>();
                Award.GetPackets(award, goods);
                if (goods.Count > 0)
                {
                    foreach (var item in goods)
                    {
                        GetLiBaoAward(dic, item.Key, item.Value, isbinding);
                    }
                }
                #endregion
            }

            PlayerEx burden = note.Player.B0;
            if (dic.Count > 0)
            {
                if (BurdenManager.IsFullBurden(burden, dic))
                {
                    note.Call(GoodsCommand.UseGoodsR, false, TipManager.GetMessage(BurdenReturn.BurdenFull));
                    return;
                }
                
            }
            //移除一个道具
            if (note.Player.RemoveGoods(p, GoodsSource.LiBao))
            {
                if (dic.Count > 0)
                {
                    //得到新物品
                    note.Player.AddGoods(dic, GoodsSource.LiBao);
                }
                if (bond > 0)
                {
                    //添加
                    note.Player.AddBond(bond, FinanceType.LiBao);
                }
                if (score > 0)
                {
                    //添加
                    note.Player.AddScore(score, FinanceType.LiBao);
                }

                if (ConfigLoader.Config.FreeCoin)
                {
                    if (coin > 0)
                    {                        
                        note.Player.AddCoin(coin, FinanceType.LiBao);
                        UserNote un = new UserNote(note.Player, PartCommand.Recharge, new object[] { coin });
                        Notifier.Instance.Publish(un);
                    }
                }
                note.Player.UpdateBurden();
                note.Call(GoodsCommand.UseGoodsR, true, g.ID);                                
            }
            else 
            {
                note.Call(GoodsCommand.UseGoodsR, false, string.Format(TipManager.GetMessage(GoodsReturn.LiBao1), g.Name));
            }
        }

        /// <summary>
        /// 礼包
        /// </summary>
        /// <param name="note"></param>
        /// <param name="gc"></param>
        private void LiBao(UserNote note, GameConfig gc)
        {
            Variant package = note.GetVariant(0);
            int p = package.GetIntOrDefault("P");

            Variant tmp = BurdenManager.BurdenPlace(note.Player.B0, p);
            if (tmp == null)
            {
                note.Call(GoodsCommand.UseGoodsR, false, string.Format(TipManager.GetMessage(GoodsReturn.LiBao1), gc.Name));
                return;
            }

            //判断物品是否绑定
            int isBinding = tmp.GetIntOrDefault("H");
            Variant libao = gc.Value.GetVariantOrDefault("LiBao");
            if (libao == null)
            {
                note.Call(GoodsCommand.UseGoodsR, false, string.Format(TipManager.GetMessage(GoodsReturn.LiBao1), gc.Name));
                return;
            }
            Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
            int bond = 0;
            int score = 0;
            int coin = 0;
            if (!libao.ContainsKey("Award"))
            {
                #region 固定奖励礼包
                foreach (var item in libao)
                {
                    switch (item.Key)
                    {
                        case "Bond":
                            bond = libao.GetIntOrDefault(item.Key);
                            break;
                        case "Score":
                            score = libao.GetIntOrDefault(item.Key);
                            break;
                        case "Coin":
                            coin = libao.GetIntOrDefault(item.Key);
                            break;
                        default:
                            GetLiBaoAward(dic, item.Key, (int)item.Value, isBinding);
                            break;
                    }
                }
                #endregion
            }
            else
            {
                #region 随机奖励礼包

                Variant award = libao.GetValue<Variant>("Award");
                Dictionary<string, int> goods = new Dictionary<string, int>();
                Award.GetPackets(award, goods);
                if (goods.Count > 0)
                {
                    foreach (var item in goods)
                    {
                        GetLiBaoAward(dic, item.Key, item.Value, isBinding);
                    }
                }
                #endregion
            }

            PlayerEx burden = note.Player.B0;
            if (dic.Count > 0)
            {
                if (BurdenManager.IsFullBurden(burden, dic))
                {
                    note.Call(GoodsCommand.UseGoodsR, false, TipManager.GetMessage(BurdenReturn.BurdenFull));
                    return;
                }
            }

            //移除成功
            if (note.Player.RemoveGoods(p, GoodsSource.LiBao))
            {
                if (dic.Count > 0)
                {
                    //得到新物品
                    note.Player.AddGoods(dic, GoodsSource.LiBao);
                }
                if (bond > 0)
                {
                    //添加
                    note.Player.AddBond(bond, FinanceType.LiBao);
                }

                if (score > 0)
                {
                    //添加
                    note.Player.AddScore(score, FinanceType.LiBao);
                }

                if (ConfigLoader.Config.FreeCoin)
                {
                    if (coin > 0)
                    {                        
                        note.Player.AddCoin(coin, FinanceType.LiBao);
                        UserNote un = new UserNote(note.Player, PartCommand.Recharge, new object[] { coin });
                        Notifier.Instance.Publish(un);
                    } 
                }
                //移除一个道具
                note.Call(GoodsCommand.UseGoodsR, true, gc.ID);                
                
            }
            else
            {
                note.Call(GoodsCommand.UseGoodsR, false, string.Format(TipManager.GetMessage(GoodsReturn.LiBao1), gc.Name));
            }
        }

        /// <summary>
        /// 装备维修
        /// </summary>
        /// <param name="note"></param>
        private void EquipRepair(UserNote note)
        {
            //表示角色正在战斗中，客户端提交信息不进行处理
            if (note.Player.AState == ActionState.Fight)
                return;
            //装备ID
            string goodsid = note.GetString(0);

            string npcid = note.GetString(1);
            if (!note.Player.EffectActive(npcid, ""))
                return;

            PlayerEx equips = note.Player.Equips;
            int score = 0;
            if (goodsid == "all")
            {
                Variant v = equips.Value;
                List<Goods> list = new List<Goods>();
                bool isrepair = true;
                for (int i = 0; i < 11; i++)
                {
                    Variant d = v.GetValueOrDefault<Variant>("P" + i);
                    string gid = d.GetStringOrDefault("E");
                    if (string.IsNullOrEmpty(gid)) 
                        continue;

                    Goods g = GoodsAccess.Instance.FindOneById(d.GetStringOrDefault("E"));
                    if (g == null) continue;

                    Variant stam0 = g.Value.GetValueOrDefault<Variant>("Stamina");// as Variant;
                    if (stam0.GetIntOrDefault("V") != stam0.GetIntOrDefault("M"))
                    {
                        isrepair = false;
                        score += RepairFee(g);
                        list.Add(g);
                    }
                }

                if (isrepair)
                {
                    note.Call(GoodsCommand.EquipRepairR, false, TipManager.GetMessage(GoodsReturn.NoStamina));
                    return;
                }
                if (note.Player.Score < score)
                {
                    note.Call(GoodsCommand.EquipRepairR, false, TipManager.GetMessage(GoodsReturn.NoScore));
                    return;
                }
                if (!note.Player.AddScore(-score, FinanceType.EquipRepair))
                {
                    note.Call(GoodsCommand.EquipRepairR, false, TipManager.GetMessage(GoodsReturn.NoScore));
                    return;
                }
                foreach (Goods gs in list)
                {
                    Variant stam1 = gs.Value.GetValueOrDefault<Variant>("Stamina");
                    stam1["V"] = stam1.GetIntOrDefault("M");
                    //UpdateStamina(equips, gs.ID, stam1.GetIntOrDefault("M"));
                    gs.Save();
                }
                //equips.Save();

                //note.Player.RefreshPlayer(string.Empty, null);
                //note.Call(GoodsCommand.EquipRepairR, true, score,goodsid);
                //note.Call(GoodsCommand.GetEquipPanelR, true, equips);

            }
            else
            {
                Variant v = equips.Value;
                bool ishame = true;
                foreach (Variant cn in v.Values)
                {
                    if (cn.GetStringOrDefault("E") == goodsid)
                    {
                        ishame = false;
                        break;
                    }
                }

                if (ishame)
                {
                    note.Call(GoodsCommand.EquipRepairR, false, TipManager.GetMessage(GoodsReturn.NoHave));
                    return;
                }
                Goods g = GoodsAccess.Instance.FindOneById(goodsid);
                if (g == null)
                {
                    note.Call(GoodsCommand.EquipRepairR, false, TipManager.GetMessage(GoodsReturn.NoStamina));
                    return;
                }
                Variant stam2 = g.Value.GetValueOrDefault<Variant>("Stamina");
                if (stam2.GetIntOrDefault("V") == stam2.GetIntOrDefault("M"))
                {
                    note.Call(GoodsCommand.EquipRepairR, false, TipManager.GetMessage(GoodsReturn.NoStamina));
                    return;
                }
                score = RepairFee(g);
                if (note.Player.Score < score || (!note.Player.AddScore(-score, FinanceType.EquipRepair)))
                {
                    note.Call(GoodsCommand.EquipRepairR, false, TipManager.GetMessage(GoodsReturn.NoScore));
                    return;
                }
                stam2["V"] = stam2.GetIntOrDefault("M");

                //UpdateStamina(equips, g.ID, stam2.GetIntOrDefault("M"));

                g.Save();
                //equips.Save();

                //note.Player.RefreshPlayer(string.Empty, null);
                //note.Player.AddScore(-Score);
                
                //note.Call(GoodsCommand.GetEquipPanelR, true, equips);
            }
            note.Player.RefreshPlayer(string.Empty, null);
            note.Call(GoodsCommand.EquipRepairR, true, score, goodsid);
        }

        /// <summary>
        /// 单件物品维修费用
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        private int RepairFee(Goods g)
        {
            //得到物品持久度
            Variant Stamina = g.Value.GetValueOrDefault<Variant>("Stamina");
            Variant price = g.Value.GetValueOrDefault<Variant>("Price");

            int Score = price.GetValueOrDefault<Variant>("Buy").GetIntOrDefault("Score");
            int Max = Stamina.GetIntOrDefault("M");
            int Mix = Stamina.GetIntOrDefault("V");
            int Price = Convert.ToInt32(Score * 0.1 * (Max - Mix));

            return (Price / Max);
        }

        /// <summary>
        /// 更新表中持久度
        /// </summary>
        /// <param name="equips"></param>
        /// <param name="goodsid"></param>
        /// <param name="max"></param>
        private void UpdateStamina(PlayerEx equips, string goodsid, int max)
        {
            Variant v = equips.Value;
            Variant tmp = null;
            foreach (Variant vas in v.Values)
            {
                if (vas.GetStringOrDefault("E") == goodsid)
                {
                    tmp = vas;
                    break;
                }
            }
            Variant t = tmp.GetVariantOrDefault("T");
            if (t != null)
            {
                t["Stamina"] = max;
            }
        }

        /// <summary>
        /// 得到装背面板明详
        /// </summary>
        /// <param name="note"></param>
        private void PanelDetails(UserNote note)
        {
            PlayerEx panel = note.Player.Equips;
            Variant v = panel.Value;
            Variant tmp = new Variant();
            foreach (string p in v.Keys)
            {
                Variant con = v.GetValueOrDefault<Variant>(p);
                string id = con.GetStringOrDefault("E");
                if (id == string.Empty) continue;
                if (p == "P11")
                {
                    Pet pet = PetAccess.Instance.FindOneById(id);
                }
                else
                {
                    Goods g = GoodsAccess.Instance.FindOneById(id);
                }
            }
        }


        /// <summary>
        /// 每日奖励
        /// </summary>
        /// <param name="note"></param>
        private void SystemAward(UserNote note)
        {
            string soleid = note.PlayerID;
            if (!m_dic.TryAdd(soleid, soleid))
                return;

            try
            {
                string name = "SystemAward";
                PlayerEx SystemAward;
                if (!note.Player.Value.TryGetValueT(name, out SystemAward))
                {
                    SystemAward = new PlayerEx(note.PlayerID, name);
                    note.Player.Value.Add("SystemAward", SystemAward);
                }

                string id = note.GetString(0);
                string key = note.GetString(1);//选择的奖励


                Variant v = AwardManager.Instance.FindOne(id);
                if (v == null)
                {
                    note.Call(GoodsCommand.SystemAwardR, false, TipManager.GetMessage(GoodsReturn.NoAward));
                    return;
                }
                DateTime dt = DateTime.Now;
                string[] UpdateTime = v.GetStringOrDefault("UpdateTime").Split(':');
                int Hour = Convert.ToInt32(UpdateTime[0].Trim());//小时
                int Minute = Convert.ToInt32(UpdateTime[1].Trim());//分钟

                //要求更新时间
                DateTime ut = dt.Date.AddHours(Hour).AddMinutes(Minute);

                if (dt < ut)
                {
                    note.Call(GoodsCommand.SystemAwardR, false, TipManager.GetMessage(GoodsReturn.NoAwardDate));
                    return;
                }

                //等级限制
                string[] Level = v.GetStringOrDefault("Level").Split('-');
                int minLevel = Convert.ToInt32(Level[0]);
                int maxLevel = Convert.ToInt32(Level[1]);

                if (note.Player.Level < minLevel || note.Player.Level > maxLevel)
                {
                    note.Call(GoodsCommand.SystemAwardR, false, TipManager.GetMessage(GoodsReturn.NoAwardLevel));
                    return;
                }

                Variant award = v.GetVariantOrDefault("Award");
                if (award == null)
                {
                    note.Call(GoodsCommand.SystemAwardR, false, TipManager.GetMessage(GoodsReturn.AwardConfigError));
                    return;
                }

                Variant awardList = award.GetVariantOrDefault(note.Player.RoleID + note.Player.Sex);
                if (awardList == null)
                {
                    note.Call(GoodsCommand.SystemAwardR, false, TipManager.GetMessage(GoodsReturn.AwardConfigError));
                    return;
                }

                Variant s = SystemAward.Value;

                if (s != null)
                {
                    //表示已经领奖
                    if (s.ContainsKey(id))
                    {
                        note.Call(GoodsCommand.SystemAwardR, false, TipManager.GetMessage(GoodsReturn.IsAward));
                        return;
                    }

                    //判断当天是否领奖
                    if (s.Count > 0)
                    {
                        note.Call(GoodsCommand.SystemAwardR, false, TipManager.GetMessage(GoodsReturn.IsAward));
                        return;
                    }
                }

                //是否为选择类型
                int selectType = awardList.GetIntOrDefault("SelectType");
                int score = 0, bond = 0, exp = 0, pExp = 0;
                //石币,晶币,经验，宠物经验
                Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
                if (selectType == 0)
                {
                    if (key != string.Empty)
                    {
                        note.Call(GoodsCommand.SystemAwardR, false, TipManager.GetMessage(GoodsReturn.NoSelect));
                        return;
                    }
                    foreach (string k in awardList.Keys)
                    {
                        if (k == "SelectType")
                            continue;
                        int count = awardList.GetIntOrDefault(k);
                        switch (k)
                        {
                            case "Score":
                                score = count;
                                break;
                            case "Coin":
                                bond = count;
                                break;
                            case "Exp":
                                exp = count;
                                break;
                            case "PExp":
                                pExp = count;
                                break;
                            default:
                                Variant vn = new Variant();
                                vn.Add("Number1", awardList.GetIntOrDefault(k));
                                if (dic.ContainsKey(k))
                                {
                                    Variant t = dic[k];
                                    t["Number1"] = t.GetIntOrDefault("Number1") + count;
                                }
                                else
                                {
                                    dic.Add(k, vn);
                                }
                                break;
                        }
                    }

                    if (dic.Count > 0)
                    {
                        #if QY
                        Variant mv = MemberAccess.MemberInfo(note.Player.MLevel);
                        if (mv != null) 
                        {
                            string gid = "";
                            int c = mv.GetIntOrDefault("Award");
                            if (c >= 0)
                            {
                                Variant gv = new Variant();
                                gv.Add("Number1", c);
                                if (dic.ContainsKey(gid))
                                {
                                    Variant t = dic[gid];
                                    t["Number1"] = t.GetIntOrDefault("Number1") + c;
                                }
                                else 
                                {
                                    dic.Add(gid, gv);
                                }
                            }
                        }
                        #endif

                        //包袱满
                        if (BurdenManager.IsFullBurden(note.Player.B0, dic))
                        {
                            note.Call(GoodsCommand.SystemAwardR, false, TipManager.GetMessage(GoodsReturn.BurdenB0Full));
                            return;
                        }
                    }
                }
                else
                {
                    if (!awardList.ContainsKey(key))
                    {
                        note.Call(GoodsCommand.SystemAwardR, false, TipManager.GetMessage(GoodsReturn.SelectNoAward));
                        return;
                    }
                    //奖励数量
                    int count = awardList.GetIntOrDefault(key);
                    if (key == "PExp")
                    {
                        if (note.Player.Pet == null)
                        {
                            note.Call(GoodsCommand.SystemAwardR, false, TipManager.GetMessage(GoodsReturn.NoPetExp));
                            return;
                        }
                        pExp = count;
                    }
                    else if (key == "Score")
                    {
                        score = count;
                    }
                    else if (key == "Bond")
                    {
                        bond = count;
                    }
                    else if (key == "Exp")
                    {
                        exp = count;
                    }
                }
                //领奖时间
                if (s != null)
                {
                    s.Add(id, dt);
                }
                else
                {
                    Variant mn = new Variant();
                    mn.Add(id, dt);
                    SystemAward.Value = mn;
                    s = SystemAward.Value;
                }

                if (!SystemAward.Save())
                {
                    note.Call(GoodsCommand.SystemAwardR, false, TipManager.GetMessage(GoodsReturn.IsAward));
                    return;
                }
                if (bond > 0)
                {
                    note.Player.AddBond(bond, FinanceType.SystemAward);
                }
                if (score > 0)
                {
                    note.Player.AddScore(score, FinanceType.SystemAward);
                }
                if (exp > 0)
                {
                    note.Player.AddExperience(exp, FinanceType.SystemAward);
                }
                if (pExp > 0)
                {
                    note.Player.AddPetExp(note.Player.Pet, pExp, true, (int)FinanceType.SystemAward);
                }
                if (dic.Count > 0)
                {
                    note.Player.AddGoods(dic, GoodsSource.SystemAward);
                }

                //操作成功移除
                //领奖成功
                note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(SystemAward));
                note.Call(GoodsCommand.SystemAwardR, true, TipManager.GetMessage(GoodsReturn.AwardSuccess));
            }
            finally
            {                
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 清理领取奖励
        /// </summary>
        /// <param name="note"></param>
        private void SystemAwardClear(UserNote note)
        {
            string name = "SystemAward";
            PlayerEx p;
            if (!note.Player.Value.TryGetValueT(name, out p))
            {
                p = new PlayerEx(note.Player.ID, name);
            }

            Variant v = p.Value;
            if (v != null)
            {
                if (v.Count > 0)
                {
                    string[] strs = new string[v.Count];
                    v.Keys.CopyTo(strs, 0);

                    for (int i = 0; i < strs.Length; i++)
                    {
                        if (!v.ContainsKey(strs[i]))
                            continue;
                        DateTime dt = v.GetDateTimeOrDefault(strs[i]).ToLocalTime();
                        TimeSpan ts = DateTime.Now.Date - dt.Date;
                        if (ts.TotalDays >= 1)
                        {
                            v.Remove(strs[i]);
                        }
                    }
                }
            }
            else
            {
                p.Value = new Variant();
            }
            p.Save();
            //领奖成功
            note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(p));
        }

        /// <summary>
        /// 答填奖励
        /// </summary>
        /// <param name="note"></param>
        private void AnswerAward(UserNote note)
        {
            string soleid = note.PlayerID + "AnswerAward";
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                string npcid = note.GetString(2);
                if (!note.Player.EffectActive(npcid, ""))
                    return;

                if (note.Player.Level < 20)
                {
                    note.Call(GoodsCommand.AnswerAwardR, false, TipManager.GetMessage(GoodsReturn.AnswerNoLevel));
                    return;
                }

                PlayerEx answer = note.Player.Value["Answer"] as PlayerEx;
                if (answer == null)
                {
                    note.Call(GoodsCommand.AnswerAwardR, false, TipManager.GetMessage(GoodsReturn.AnswerNoLevel));
                    return;
                }

                Variant v = answer.Value;
                if (v.GetIntOrDefault("Cur") < v.GetIntOrDefault("Max"))
                {
                    v["Cur"] = v.GetIntOrDefault("Cur") + 1;
                    int number = v.GetIntOrDefault("Cur");//当前第几题
                    int m = AnswerGoods(number);//需要道具数量
                    //需要道具
                    string goodsid = TipManager.GetMessage(GoodsReturn.AnswerGood);
                    if (m > 0)
                    {
                        if (BurdenManager.GoodsCount(note.Player.B0, goodsid) < m)
                        {
                            note.Call(GoodsCommand.AnswerAwardR, false, TipManager.GetMessage(GoodsReturn.AnswerNoGood));
                            return;
                        }
                    }
                    //移除成功
                    if (m > 0)
                    {
                        if (!note.Player.RemoveGoods(goodsid, m, GoodsSource.AnswerAward))
                        {
                            note.Call(GoodsCommand.AnswerAwardR, false, TipManager.GetMessage(GoodsReturn.AnswerNoGood));
                            return;
                        }                        
                    }

                    int an = AnswerManager.IsAnswer(note.GetString(0), note.GetString(1));
                    int rexp = AnswerExp(number, note.Player.Level);
                    int pexp = 0;
                    if (note.Player.Pet != null)
                    {
                        Pet p = note.Player.Pet;
                        int petsLevel = p.Value.GetIntOrDefault("PetsLevel");
                        if (petsLevel < 20) petsLevel = 20;
                        pexp = AnswerExp(number, petsLevel);
                    }

                    if (an == 2)
                    {
                        note.Call(GoodsCommand.AnswerAwardR, false, TipManager.GetMessage(GoodsReturn.NoAnswer));
                        return;
                    }
                    else if (an == 1)
                    {
                        //题目答错
                        rexp = rexp / 2;
                        pexp = pexp / 2;
                    }

                                        
                    Variant mv = MemberAccess.MemberInfo(note.Player.MLevel);
                    if (mv != null)
                    {
                        double dati=mv.GetDoubleOrDefault("DaTi");
                        rexp = rexp + Convert.ToInt32(rexp * dati);
                        pexp = pexp + Convert.ToInt32(pexp * dati);
                    }
                    

                    answer.Save();
                    note.Player.AddExperience(rexp, FinanceType.AnswerAward);
                    note.Player.AddPetExp(note.Player.Pet, pexp, true, (int)FinanceType.AnswerAward);


                    Variant list = new Variant();
                    list.Add("B0", note.Player.B0);
                    note.Call(BurdenCommand.BurdenListR, list);

                    Variant tmp = new Variant();
                    tmp.Add("RExp", rexp);
                    tmp.Add("PExp", pexp);
                    tmp.Add("Result", an);

                    note.Player.Call(ClientCommand.UpdateActorR, new PlayerExDetail(answer));
                    note.Call(GoodsCommand.AnswerAwardR, true, tmp);
                    note.Player.AddAcivity(ActivityType.WenDa, 1);

                    note.Player.FinishNote(FinishCommand.AnswerAward, 1);
                }
                else
                {
                    note.Call(GoodsCommand.AnswerAwardR, false, TipManager.GetMessage(GoodsReturn.MaxAnswer));
                    return;
                }
            }
            finally 
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 答题经验奖励20级得到的奖励
        /// </summary>
        /// <param name="number">当前答题数量</param>
        /// <param name="level">等级</param>
        /// <param name="ispet">是否是宠物</param>
        /// <returns></returns>
        private int AnswerExp(int number, int level)
        {
            int exp = 0;//上一等级的经验
            if (level >= 20)
            {
                if (number < 6)
                    exp = (5000 + 1200 * (number - 1));
                else
                    exp = (28000 + 2000 * (number - 5));
            }
            for (int i = 21; i < level; i++)
            {
                exp = Convert.ToInt32(2 * (Math.Pow(i, 2) / Math.Pow(i - 1, 3)) * exp + exp);
            }
            if (level > 20)
            {
                exp = Convert.ToInt32(2 * (Math.Pow(level, 2) / Math.Pow(level - 1, 3)) * exp + exp);
            }
            return exp;
        }


        /// <summary>
        /// 需要多少道局
        /// </summary>
        /// <param name="number">答题数量</param>
        /// <returns></returns>
        private int AnswerGoods(int number)
        {
            if (number < 6)
                return 0;
            else if (number < 11)
                return 1;
            else if (number < 16)
                return 2;
            else if (number < 21)
                return 3;
            else if (number < 26)
                return 4;
            else
                return 5;
        }


        /// <summary>
        /// 取得礼包奖励
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="goodsid"></param>
        /// <param name="number"></param>
        /// <param name="isbinding"></param>
        private void GetLiBaoAward(Dictionary<string, Variant> dic, string goodsid, int number, int isbinding)
        {
            GameConfig config = GameConfigAccess.Instance.FindOneById(goodsid);
            if (config == null)
                return;
            Variant v;
            string n = "Number" + isbinding;

            if (dic.TryGetValue(goodsid, out v))
            {
                v[goodsid] = v.GetIntOrDefault(goodsid) + number;
            }
            else
            {
                v = new Variant();
                v.Add(n, number);
                GoodsAccess.Instance.TimeLines(config, v);
                //if (config.Value.ContainsKey("TimeLines"))
                //{
                //    Variant TimeLines = config.Value.GetValueOrDefault<Variant>("TimeLines");
                //    int hour = 0;
                //    if (TimeLines != null)
                //    {
                //        if (TimeLines.TryGetValueT("Hour", out hour) && hour > 0)
                //        {
                //            //限时问题
                //            v.Add("EndTime", DateTime.UtcNow.AddHours(hour));
                //        }
                //        else if (TimeLines.ContainsKey("SetDate"))
                //        {
                //            //定时设置
                //            DateTime t;
                //            if (DateTime.TryParse(TimeLines.GetStringOrDefault("SetDate"), out t))
                //            {
                //                v.Add("EndTime", TimeLines.GetDateTimeOrDefault("SetDate"));
                //            }
                //        }
                //    }
                //}

                dic.Add(goodsid, v);
            }
        }
    }
}