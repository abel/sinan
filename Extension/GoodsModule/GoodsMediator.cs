using System;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.GoodsModule
{
    sealed public partial class GoodsMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[]
            {
                LoginCommand.PlayerLoginSuccess,
                
                GoodsCommand.GetEquipPanel,
                GoodsCommand.GetGoodsDetail,
                GoodsCommand.UpdateEquip,
                GoodsCommand.NPCGoodsList,
                GoodsCommand.UseGoods,
                GoodsCommand.Uninstall,

                GoodsCommand.BuyGoods,
                GoodsCommand.SellGoods,
                GoodsCommand.EquipRepair,
                GoodsCommand.SystemAward,
                GoodsCommand.AnswerAward,
                ClientCommand.UserDisconnected,
                GoodsCommand.AllUseGoods
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
            // 需验证玩家登录的操作
            if (note.Player == null)
                return;
            switch (note.Name)
            {
                case LoginCommand.PlayerLoginSuccess:
                    GetEquipPanel(note);
                    SystemAwardClear(note);
                    //PanelDetails(note);
                    if (note != null)
                    {
                        Business.GoodsBusiness.ResetAnswer(note.Player);
                    }
                    break;
                case GoodsCommand.GetGoodsDetail:
                    GoodsEquipDetails(note);
                    break;
                case GoodsCommand.UpdateEquip:
                    break;
                case GoodsCommand.NPCGoodsList:
                    NPCGoodsList(note);
                    break;
                case GoodsCommand.UseGoods:
                    UseGoods(note);
                    break;
                case GoodsCommand.Uninstall:
                    Uninstall(note);
                    break;
                case GoodsCommand.BuyGoods:
                    BuyGoods(note);
                    break;
                case GoodsCommand.SellGoods:
                    SellGoods(note);
                    break;
                case GoodsCommand.EquipRepair:
                    EquipRepair(note);
                    break;
                case GoodsCommand.SystemAward:
                    SystemAward(note);
                    break;
                case GoodsCommand.AnswerAward:
                    AnswerAward(note);
                    break;
                case GoodsCommand.AllUseGoods:
                    AllUseGoods(note);
                    break;
            }
        }

        private void AllUseGoods(UserNote note)
        {
            //商品的使用
            Variant package = BurdenManager.BurdenPlace(note.Player.B0, note.GetInt32(0));
            if (package == null) return;
            string goodsid = package.GetStringOrDefault("E");
            if (goodsid == null) return;
            GameConfig gc = GameConfigAccess.Instance.FindOneById(goodsid);
            if (gc == null) return;
            UseGoods(note, package, gc, true);
        }

        /// <summary>
        /// 得到装备面板列表
        /// </summary>
        /// <param name="note"></param>
        private void GetEquipPanel(UserNote note)
        {
            PlayerEx equip = note.Player.Equips;
            //判断角色装备信息
            if (equip == null)
            {
                PlayerEx p = new PlayerEx(note.PlayerID, "Equips");
                p.Value = new Variant();
                Variant v = p.Value;
                for (int i = 0; i < 12; i++)
                {
                    Variant t = new Variant();
                    t.Add("E", string.Empty);
                    t.Add("G", string.Empty);
                    t.Add("A", 0);
                    t.Add("S", 0);
                    t.Add("H", 0);
                    t.Add("D", 0);
                    t.Add("T", null);
                    v.Add("P" + i, t);
                }
                p.Save();
                //内存创建
                note.Player.Value["Equips"] = p;
                equip = p;
            }

            //note.Call(GoodsCommand.GetEquipPanelR, true, Business.GoodsBusiness.RemoveEquips(note));
            note.Call(GoodsCommand.GetEquipPanelR, true, equip);
        }

        /// <summary>
        /// 得到商品明细
        /// </summary>
        /// <param name="user"></param>
        /// <param name="GoodsCode">商品ID</param>
        private void GoodsEquipDetails(UserNote note)
        {
            string goodsid = note.GetString(0);

            GameConfig gc = GameConfigAccess.Instance.FindOneById(goodsid);
            Variant v = new Variant();
            if (gc != null)
            {
                foreach (var item in gc.Value)
                {
                    v[item.Key] = item.Value;
                }
                v.Add("ID", gc.ID);
                v.Add("UI", gc.UI);
                v.Add("Name", gc.Name);
                v.Add("Value", gc.Value);
                note.Call(GoodsCommand.GetGoodsDetailR, true, v);
                return;
            }

            Goods g = GoodsAccess.Instance.FindOneById(goodsid);
            if (g != null)
            {
                foreach (var item in g.Value)
                {
                    v.Add(item.Key, item.Value);
                }

                if (!v.ContainsKey("ID"))
                {
                    v.Add("ID", g.ID);
                }
                if (!v.ContainsKey("GoodsID"))
                {
                    v.Add("GoodsID", g.GoodsID);
                }
                if (!v.ContainsKey("Name"))
                {
                    v.Add("Name", g.Name);
                }
                note.Call(GoodsCommand.GetGoodsDetailR, true, v);
                return;
            }

            Mounts ms = MountsAccess.Instance.FindOneById(goodsid);
            if (ms != null) 
            {
                v.Add("ID", ms.ID);
                v.Add("Level", ms.Level);
                v.Add("Name", ms.Name);
                v.Add("Experience", ms.Experience);
                v.Add("Status", ms.Status);
                v.Add("Rank", ms.Rank);
                v.Add("PlayerID", ms.PlayerID);
                v.Add("MountsID", ms.MountsID);
                v.Add("MainType", "Mounts");
                //v.Add("Life", MountsAccess.Instance.MountsLife(ms.Level));
                Variant life = MountsAccess.Instance.MountsLife(ms);
                foreach (var info in life)
                {
                    v.Add(info.Key, info.Value);
                }

                if (ms.Value != null) 
                {
                    foreach (var item in ms.Value) 
                    {
                        v.Add(item.Key, item.Value);
                    }
                }
                note.Call(GoodsCommand.GetGoodsDetailR, true, v);
                return;
            }
            note.Call(GoodsCommand.GetGoodsDetailR, false, null);
        }

   
        /// <summary>
        /// 买得商品(道具与装备)
        /// </summary>
        /// <param name="note"></param>
        private void BuyGoods(UserNote note)
        {
            //可以通过晶币或游戏币买得商品装备
            string goodsid = note.GetString(0);
            //卖商品的数量
            int count = note.GetInt32(1);
            //NPCID
            string npcid = note.GetString(2);

            Npc npc = NpcManager.Instance.FindOne(npcid);
            if (npc == null)
            {
                note.Call(GoodsCommand.BuyGoodsR, false, TipManager.GetMessage(GoodsReturn.NoNPC));
                return;
            }
            Variant nv = npc.Value;

            if (nv == null)
            {
                note.Call(GoodsCommand.BuyGoodsR, false, TipManager.GetMessage(GoodsReturn.NoNPC));
                return;
            }

            IList sg = nv.GetValue<IList>("SellGoods");
            if (sg == null)
            {
                note.Call(GoodsCommand.BuyGoodsR, false, TipManager.GetMessage(GoodsReturn.NoGoodsInfo));
                return;
            }
            else 
            {
                if (!sg.Contains(goodsid))
                {
                    note.Call(GoodsCommand.BuyGoodsR, false, TipManager.GetMessage(GoodsReturn.NoGoodsInfo));
                    return;
                }
            }

            if (nv.GetIntOrDefault("ShangDian") == 1) 
            {
                Variant mv = MemberAccess.MemberInfo(note.Player.MLevel);
                if (mv == null)
                {
                    note.Call(GoodsCommand.BuyGoodsR, false, TipManager.GetMessage(GoodsReturn.NoMember));
                    return;
                }

                if (!mv.GetBooleanOrDefault("ShangDian"))
                {
                    note.Call(GoodsCommand.BuyGoodsR, false, TipManager.GetMessage(GoodsReturn.NoMember));
                    return;
                }
            }

            //npc所在场景
            //string sceneid = npc.Value.GetStringOrEmpty("SceneID");
            //if (sceneid != note.Player.SceneID)
            //{
            //    note.Call(GoodsCommand.BuyGoodsR, false, TipManager.GetMessage(GoodsReturn.NPCError));
            //    return;
            //}
            //Auction

            if (!note.Player.EffectActive(npcid, ""))
                return;

            GameConfig gc = GameConfigAccess.Instance.FindOneById(goodsid);
            if (gc == null)
            {
                note.Call(GoodsCommand.BuyGoodsR, false, TipManager.GetMessage(GoodsReturn.NoGoodsInfo));
                return;
            }

            int m = BurdenManager.StactCount(gc);
            string name = "B0";


            if (name == string.Empty || m == 0)
            {
                note.Call(GoodsCommand.BuyGoodsR, false, TipManager.GetMessage(GoodsReturn.EquipError));
                return;
            }

            PlayerEx burden = note.Player.B0;

            Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
            Variant v = new Variant();
            v.Add("Number1", count);
            if (gc.Value.ContainsKey("TimeLines"))
            {
                Variant timeLines = gc.Value.GetValueOrDefault<Variant>("TimeLines");
                int hour = 0;
                if (timeLines.TryGetValueT("Hour", out hour))
                {
                    v.Add("EndTime", DateTime.UtcNow.AddHours(hour));
                }
            }
            dic.Add(goodsid, v);

            string cn = npc.Value.GetStringOrDefault("Money");


            Variant price = gc.Value.GetVariantOrDefault("Price");
            
            string tranType= gc.Value.GetStringOrEmpty("TranType");

            if (tranType.IndexOf("S") < 0)
            {
                LogWrapper.Warn("NPC:" + note.Player.Name + "," + gc.ID + "," + gc.Name);
                return;
            }

            


            int buyPrice = 0;//得到物品单价

            if (cn == "Score")
            {
                buyPrice = price.GetVariantOrDefault("Buy").GetIntOrDefault("Score");
            }
            else
            {
                note.Call(GoodsCommand.BuyGoodsR, false, TipManager.GetMessage(GoodsReturn.NPCError));
                return;
            }

            int total = buyPrice * count;

            if (total < 1)
            {
                note.Call(GoodsCommand.BuyGoodsR, false, TipManager.GetMessage(GoodsReturn.NPCError));
                return;
            }

            //判断包袱是否满
            if (BurdenManager.IsFullBurden(burden, dic))
            {
                note.Call(GoodsCommand.BuyGoodsR, false, TipManager.GetMessage(GoodsReturn.BurdenB0Full));
                return;
            }


            if (note.Player.Score < total || (!note.Player.AddScore(-total, FinanceType.BuyGoods)))
            {
                note.Call(GoodsCommand.BuyGoodsR, false, TipManager.GetMessage(GoodsReturn.NoScore));
                return;
            }

            note.Player.AddGoods(dic, GoodsSource.BuyGoods);
            burden.Save();

            note.Player.UpdateBurden();
            note.Call(GoodsCommand.BuyGoodsR, true, goodsid);
        }

        /// <summary>
        /// 卖商品得到游戏币(道具与装备)
        /// </summary>
        /// <param name="note"></param>
        private void SellGoods(UserNote note)
        {
            //出售列表
            IList list = note[0] as IList;
            PlayerEx b0 = note.Player.B0;
            string npcid = note.GetString(1);
            if (!note.Player.EffectActive(npcid, ""))
                return;
            #region 判断销售物品是否正确
            foreach (Variant d in list)
            {
                Variant con = BurdenManager.BurdenPlace(b0, d.GetIntOrDefault("P"));
                if (con == null)
                {
                    note.Call(GoodsCommand.SellGoodsR, false, TipManager.GetMessage(GoodsReturn.NoSellGoodsGrid));
                    return;
                }

                if (con.GetStringOrDefault("E") != d.GetStringOrDefault("ID"))
                {
                    note.Call(GoodsCommand.SellGoodsR, false, TipManager.GetMessage(GoodsReturn.NoSellGoodsGrid));
                    return;
                }

                if (con.GetIntOrDefault("A") != d.GetIntOrDefault("Count"))
                {
                    note.Call(GoodsCommand.SellGoodsR, false, TipManager.GetMessage(GoodsReturn.NoSellGoodsGrid));
                    return;
                }

                GameConfig gc = GameConfigAccess.Instance.FindOneById(con.GetStringOrDefault("G"));
                if (gc == null)
                    continue;
                if (gc.Value.GetIntOrDefault("IsSell") == 0)
                {
                    note.Call(GoodsCommand.SellGoodsR, false, TipManager.GetMessage(GoodsReturn.NoSell));
                    return;
                }

                Variant t = con.GetVariantOrDefault("T");
                if (t != null)
                {
                    //存在过期时间,过期物品不能出售
                    if (t.ContainsKey("EndTime"))
                    {
                        DateTime endtime = t.GetDateTimeOrDefault("EndTime");
                        if (endtime < DateTime.UtcNow)
                        {
                            note.Call(GoodsCommand.SellGoodsR, false, TipManager.GetMessage(GoodsReturn.ExpiredGood));
                            return;
                        }
                    }
                }
            }
            #endregion

            #region 计算出售的总价格
            //出售物品数量
            //Variant mv = new Variant();
            int score = 0;//得到绑金总数
            foreach (Variant d in list)
            {
                int price = 0;//得到物品单价
                Variant con = BurdenManager.BurdenPlace(b0, d.GetIntOrDefault("P"));
                if (con == null)
                    continue;

                string ge = con.GetStringOrDefault("E");
                string gg = con.GetStringOrDefault("G");
                if (ge != gg)
                {
                    Goods g = GoodsAccess.Instance.FindOneById(ge);
                    if (g == null)
                        continue;

                    Variant v = g.Value;
                    if (v == null)
                        continue;
                    int sp = v.GetVariantOrDefault("Price").GetIntOrDefault("SellScore");
                    if (v.GetStringOrDefault("GoodsType").IndexOf("111") == 0)
                    {
                        Variant stamina = v.GetVariantOrDefault("Stamina");
                        double min = stamina.GetDoubleOrDefault("V");
                        double max = stamina.GetDoubleOrDefault("M");
                        price = (int)(Math.Floor(sp * min / max));
                    }
                    else
                    {
                        price = sp;
                    }
                }
                else
                {
                    GameConfig gc = GameConfigAccess.Instance.FindOneById(ge);
                    if (gc == null)
                        continue;
                    price = gc.Value.GetVariantOrDefault("Price").GetIntOrDefault("SellScore");
                }
                int num = con.GetIntOrDefault("A");
                //BurdenManager.BurdenClear(con);
                if (note.Player.RemoveGoods(con.GetIntOrDefault("P"), GoodsSource.SellGoods, true))
                {
                    score += price * num;
                }

                //mv.SetOrInc(gg, num);
            }
            #endregion

            if (score > 0)
            {
                note.Player.AddScore(score, FinanceType.SellGoods);
            }
            b0.Save();
            note.Player.UpdateBurden();
            note.Call(GoodsCommand.SellGoodsR, true, TipManager.GetMessage(GoodsReturn.SellSeccess));

            //foreach (var item in mv) 
            //{
            //    note.Player.AddLog(Actiontype.GoodsUse, item.Key, (int)item.Value, GoodsSource.SellGoods, "", 0);
            //}
        }

        /// <summary>
        /// 得到商业NPC商品列表
        /// </summary>
        /// <param name="note"></param>
        private void NPCGoodsList(UserNote note)
        {
            string npcid = note.GetString(0);
            GameConfig gc = GameConfigAccess.Instance.FindOneById(npcid);
            if (gc == null)
            {
                note.Call(GoodsCommand.NPCGoodsListR, false, TipManager.GetMessage(GoodsReturn.NoNPC));
                return;
            }
            IList goods = gc.Value.GetValue<IList>("Goods");
            List<GameConfig> list = GameConfigAccess.Instance.FindByIDList(goods);
            note.Call(GoodsCommand.NPCGoodsListR, true, list);
        }

        /// <summary>
        /// 商品的使用
        /// </summary>
        /// <param name="note"></param>
        private void UseGoods(UserNote note)
        {
            //商品的使用

            Variant package = note.GetVariant(0);
            int check = DateLimit(note.Player, package);
            if (check != 0)
            {
                note.Call(GoodsCommand.UseGoodsR, false, TipManager.GetMessage(check));
                return;
            }
            
            string goodsid = package.GetStringOrDefault("E");
            string gid = string.Empty;
            Goods g = GoodsAccess.Instance.GetGoodsByID(goodsid);
            if (g != null)
            {
                string goodsType = g.Value.GetStringOrDefault("GoodsType");
                if (g.PlayerID != note.PlayerID) return;
                //表示装备的使用
                if (goodsType.StartsWith("111"))
                {
                    Dress(note, g);
                    return;
                }
                if (goodsType == "112011")
                {
                    LiBao(note, g);
                    return;
                }
                //112001	药品类
                //112002	材料类
                //112003	宠物道具
                //112004	辅助类
                //112005	技能类
                //112006	任务类
                //112007	家园类
                //112008	特殊类
                //112009    宠物 
                //112010    宠物蛋
                //112011    礼包
                //药品类
            }
            GameConfig gc = GameConfigAccess.Instance.FindOneById(g == null ? goodsid : g.GoodsID);
            if (gc == null)
            {
                return;
            }
            if (gc.Value.GetStringOrDefault("GoodsType") == "112011")
            {
                LiBao(note, gc);
            }
            else
            {
                UseGoods(note, package, gc, false);
            }
        }
    
    }
}
