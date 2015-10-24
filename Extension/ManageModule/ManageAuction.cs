using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.ManageModule
{
    public class ManageAuction
    {
        /// <summary>
        /// GM取得拍卖行数据
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void GMAuctionList(GMNote note, string[] strs)
        {
            if (strs.Length < 3)
                return;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
                return;
            int pageIndex = 0;

            if (!int.TryParse(strs[1].Trim(), out pageIndex))
                return;
            int pageSize = 6;

            int total = 0;
            int curIndex = 0;
            List<Auction> auctionList = AuctionAccess.Instance.AuctionSellerList(player.ID, pageSize, pageIndex, out total, out curIndex);
            List<Variant> list = new List<Variant>();
            foreach (Auction model in auctionList) 
            {
                Variant mv = model.Value;
                Variant v = new Variant();
                foreach (var item in mv) 
                {
                    v.Add(item.Key, item.Value);
                }
                v.Add("ID", model.ID);
                v.Add("Name", model.Name);
                list.Add(v);
            }
            note.Call(GMCommand.GMAuctionListR, list, total, curIndex);
        }

        /// <summary>
        /// GM删除拍卖行记当
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void GMAuctionDel(GMNote note, string[] strs)
        {
            if (strs.Length != 2)
                return;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
                return;
            string[] s = strs[1].Split('|');
            if (AuctionAccess.Instance.GMAuctionDel(player.ID, s))
            {
                note.Call(GMCommand.GMAuctionDelR, true);
            }
        }

        /// <summary>
        /// 清理包袱,仓库,家园,兽栏
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void GMBurdenClear(GMNote note, string[] strs)
        {
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
                return;
            string b = strs[1].Trim();
            List<string> list = new List<string>() 
            { 
                "B0", 
                "B1", 
                "B2", 
                "B3",
                "Home",
                "Star"
            };

            if (!list.Contains(b))
                return;

            PlayerEx burden = player.Value[b] as PlayerEx;
            if (burden == null) 
                return;
            Variant v = burden.Value;
            if (b.IndexOf("B") == 0)
            {
                IList c = v.GetValue<IList>("C");
                foreach (Variant k in c)
                {

                    string soleid = k.GetStringOrDefault("E");

                   
                    if (string.IsNullOrEmpty(soleid))
                        continue;
                    string goodsid = k.GetStringOrDefault("G");
                    GameConfig gc = GameConfigAccess.Instance.FindOneById(goodsid);
                    if (gc == null)
                    {
                        BurdenManager.BurdenClear(k);
                        continue;
                    }
                    //道具类型
                    string goodsType = gc.Value.GetStringOrDefault("GoodsType");
                    if (goodsType == "")
                        continue;
                    BurdenManager.BurdenClear(k);
                    if (b == "B0" || b == "B1")
                    {
                        if (soleid != goodsid)
                        {
                            GoodsAccess.Instance.Remove(soleid, player.ID);
                        }
                    }
                    else
                    {
                        Pet p = PetAccess.Instance.FindOneById(soleid);
                        if (p != null)
                        {
                            int level = p.Value.GetIntOrDefault("PetsLevel");
                            string petsid = p.Value.GetStringOrDefault("PetsID");

                            PetAccess.Instance.RemoveOneById(p.ID, SafeMode.False);
                            int pl = (b == "B2") ? 2 : 3;
                            player.AddLog(Log.Actiontype.PetRemove, p.ID, level, GoodsSource.GM, petsid, pl);
                        }
                    }
                    
                }
            }
            else if (b == "Home")
            {
                Variant pk = v.GetValueOrDefault<Variant>("PetKeep");
                string id = pk.GetStringOrDefault("ID");
                string petid = "";
                if (!string.IsNullOrEmpty(id))
                {
                    Pet p = PetAccess.Instance.FindOneById(id);
                    if (p != null)
                    {
                        petid = p.Value.GetStringOrDefault("PetsID");
                        int level = p.Value.GetIntOrDefault("PetsLevel");
                        PetAccess.Instance.RemoveOneById(id, SafeMode.False);
                        pk["EndTime"] = "";
                        pk["ID"] = "";
                        pk["PetID"] = "";
                        pk["PetName"] = "";
                        pk["PetsWild"] = 0;
                        pk["StartTime"] = "";

                        player.AddLog(Log.Actiontype.PetRemove, petid, level, GoodsSource.GM, p.ID, 1);
                    }
                }

                Variant pd = v.GetValueOrDefault<Variant>("Produces");
                if (pd != null) 
                {
                    foreach (var item in pd) 
                    {
                        Variant info = item.Value as Variant;
                        IList petlist = info.GetValue<IList>("PetList");
                        if (petlist != null && petlist.Count > 0)
                        {
                            foreach (Variant k in petlist)
                            {
                                id = k.GetStringOrDefault("E");
                                Pet p = PetAccess.Instance.FindOneById(id);
                                if (p != null)
                                {
                                    petid = p.Value.GetStringOrDefault("PetsID");
                                    int level = p.Value.GetIntOrDefault("PetsLevel");
                                    PetAccess.Instance.RemoveOneById(p.ID, SafeMode.False);
                                    player.AddLog(Log.Actiontype.PetRemove, petid, level, GoodsSource.GM, p.ID, 1);
                                }
                            }
                            //宠物回收
                            petlist.Clear();
                        }                        
                    }
                }
            }
            else 
            {
                Variant pl = v.GetVariantOrDefault("PetsList");
                if (pl != null) 
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Variant pv = pl.GetVariantOrDefault(i.ToString());
                        if (pv == null) 
                            continue;
                        //移除星阵中宠物
                        string id = pv.GetStringOrDefault("ID");
                        Pet p = PetAccess.Instance.FindOneById(id);
                        if (p != null)
                        {
                            string petid = p.Value.GetStringOrDefault("PetsID");
                            int level = p.Value.GetIntOrDefault("PetsLevel");
                            PetAccess.Instance.RemoveOneById(p.ID, SafeMode.False);
                            //星阵中宠物
                            player.AddLog(Log.Actiontype.PetRemove, p.ID, level, GoodsSource.GM, petid, 4);
                        }
                    }
                }
                //星座清理
                burden.Value.Clear();
            }
            if (burden.Save())
            {
                if (player.Online)
                {
                    player.UpdateBurden(b);
                }
                note.Call("", true);
            }
        }

        /// <summary>
        /// GM取得商城列表
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void GMMallInfo(GMNote note)
        {
            string type = note.GetString(1);
            if (type == "get")
            {
                HashSet<string> hs = MallAccess.HS;
                List<string> list = new List<string>();
                foreach (string k in hs)
                {
                    list.Add(k);
                }
                note.Call(GMCommand.GMMallInfoR, new object[] { type, list });                
            }
            else
            {
                IList ms = note[2] as IList;
                note.Call(GMCommand.GMMallInfoR, new object[] { type, MallAccess.UpdateMell(ms) });
            }
        }
    }
}
