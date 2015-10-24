using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Util;
using Sinan.Data;
using Sinan.Entity;
using System.Collections;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.WardrobeModule.Bussiness
{
    public class WardrobeBusiness
    {
        static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 商城试穿
        /// </summary>
        /// <param name="note"></param>
        public static void MallDressing(UserNote note)
        {
            //商城时装
            string id = note.GetString(0);

            //是否通过GM上下架检查
            if (ServerManager.IsMall)
            {
                //物品没有上架
                if (!MallAccess.HS.Contains(id))
                {
                    note.Call(WardrobeCommand.MallDressingR, false, TipManager.GetMessage(WardrobeReturn.MallDressing1));
                    return;
                }
            }

            GameConfig m_gc = GameConfigAccess.Instance.FindOneById(id);
            if (m_gc == null)
            {
                note.Call(WardrobeCommand.MallDressingR, false, TipManager.GetMessage(WardrobeReturn.MallDressing2));
                return;
            }

            if (m_gc.SubType != "ShiZhuang")
            {
                note.Call(WardrobeCommand.MallDressingR, false, TipManager.GetMessage(WardrobeReturn.MallDressing3));
                return;
            }

            Variant v = m_gc.Value;
            if (v == null)
            {
                note.Call(WardrobeCommand.MallDressingR, false, TipManager.GetMessage(WardrobeReturn.MallDressing2));
                return;
            }
            if (v.GetIntOrDefault("IsDressing") == 0)
            {
                note.Call(WardrobeCommand.MallDressingR, false, TipManager.GetMessage(WardrobeReturn.MallDressing9));
                return;
            }
            if (v.GetIntOrDefault("MLevel") > note.Player.MLevel)
            {
                note.Call(WardrobeCommand.MallDressingR, false, TipManager.GetMessage(WardrobeReturn.MallDressing4));
                return;
            }
            DateTime dt = DateTime.UtcNow;
            string ud = v.GetStringOrDefault("UpDate");
            if (!string.IsNullOrEmpty(ud))
            {
                DateTime update = v.GetDateTimeOrDefault("UpDate").ToUniversalTime();
                if (dt < update)
                {
                    //没有上架
                    note.Call(WardrobeCommand.MallDressingR, false, TipManager.GetMessage(WardrobeReturn.MallDressing5));
                    return;
                }
            }

            string dd = v.GetStringOrDefault("DownDate");
            if (!string.IsNullOrEmpty(dd))
            {
                DateTime endDate = v.GetDateTimeOrDefault("DownDate").ToUniversalTime();
                if (endDate < dt)
                {
                    //下架时间
                    note.Call(WardrobeCommand.MallDressingR, false, TipManager.GetMessage(WardrobeReturn.MallDressing6));
                    return;
                }
            }

            //时装
            string goodsid = v.GetStringOrDefault("GoodsID");



            PlayerEx wx = note.Player.Wardrobe;           
            Variant wv = wx.Value;
            if (wv.GetStringOrDefault("ShiZhuang") == goodsid)
            {
                note.Call(WardrobeCommand.MallDressingR, false, TipManager.GetMessage(WardrobeReturn.MallDressing10));
                return;
            }

            GameConfig gc = GameConfigAccess.Instance.FindOneById(goodsid);
            if (gc == null)
            {
                note.Call(WardrobeCommand.MallDressingR, false, TipManager.GetMessage(WardrobeReturn.MallDressing7));
                return;
            }
            Variant limit = gc.Value.GetVariantOrDefault("UserLimit");
            if (limit != null)
            {
                if (limit.ContainsKey("Sex"))
                {
                    int sex = Convert.ToInt32(limit["Sex"]);
                    if (sex != 2 && sex != note.Player.Sex)
                    {
                        note.Call(WardrobeCommand.MallDressingR, false, TipManager.GetMessage(WardrobeReturn.MallDressing9));
                        return;
                    }
                }
            }

            wv["ShiZhuang"] = goodsid;
            if (!wx.Save())
            {
                note.Call(WardrobeCommand.MallDressingR, false, TipManager.GetMessage(WardrobeReturn.MallDressing7));
                return;
            }

            note.Player.ShiZhuangInfo();


            note.Player.CallAll(ClientCommand.UpdateActorR, new PlayerExDetail(wx));
            note.Call(WardrobeCommand.MallDressingR, true, TipManager.GetMessage(WardrobeReturn.MallDressing8));
        }

        /// <summary>
        /// 穿时装
        /// </summary>
        /// <param name="note"></param>
        public static void Dressing(UserNote note)
        {
            string goodsid = note.GetString(0);
            PlayerEx wx = note.Player.Wardrobe;
            Variant wv = wx.Value;
            IList wl = wv.GetValue<IList>("WardrobeList");
            if (wl == null)
            {
                note.Call(WardrobeCommand.DressingR, false, TipManager.GetMessage(WardrobeReturn.Dressing1));
                return;
            }
            if (!wl.Contains(goodsid))
            {
                note.Call(WardrobeCommand.DressingR, false, TipManager.GetMessage(WardrobeReturn.Dressing2));
                return;
            }

            GameConfig gc = GameConfigAccess.Instance.FindOneById(goodsid);
            if (gc == null)
            {
                note.Call(WardrobeCommand.DressingR, false, TipManager.GetMessage(WardrobeReturn.Dressing2));
                return;
            }
            
            wv["ShiZhuang"] = goodsid;
            if (!wx.Save())
            {
                note.Call(WardrobeCommand.DressingR, false, TipManager.GetMessage(WardrobeReturn.Dressing3));
                return;
            }

            note.Player.ShiZhuangInfo();

            note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(wx));
            note.Call(WardrobeCommand.DressingR, true, TipManager.GetMessage(WardrobeReturn.Dressing4));
        }

        /// <summary>
        /// 取消时装
        /// </summary>
        /// <param name="note"></param>
        public static void NoDressing(UserNote note)
        {
            PlayerEx wx = note.Player.Wardrobe;
            Variant wv = wx.Value;
            if (string.IsNullOrEmpty(wv.GetStringOrDefault("ShiZhuang")))
            {
                note.Call(WardrobeCommand.NoDressingR, false, TipManager.GetMessage(WardrobeReturn.NoDressing1));
                return;
            }
            wv["ShiZhuang"] = "";
            if (!wx.Save())
            {
                note.Call(WardrobeCommand.NoDressingR, false, TipManager.GetMessage(WardrobeReturn.NoDressing2));
                return;
            }
            note.Player.ShiZhuangInfo();      
            note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(wx));
            note.Call(WardrobeCommand.NoDressingR, true, TipManager.GetMessage(WardrobeReturn.NoDressing3));
        }

        /// <summary>
        /// 时装兑换
        /// </summary>
        /// <param name="note"></param>
        public static void FashionExchange(UserNote note)
        {
            string id = note.GetString(0);
            GameConfig gc = GameConfigAccess.Instance.FindOneById(id);
            if (gc == null)
            {
                note.Call(WardrobeCommand.FashionExchangeR, false, TipManager.GetMessage(WardrobeReturn.FashionExchange1));
                return;
            }

            if (gc.MainType != "FashionExchange")
            {
                note.Call(WardrobeCommand.FashionExchangeR, false, TipManager.GetMessage(WardrobeReturn.FashionExchange2));
                return;
            }


            PlayerEx wx = note.Player.Wardrobe;
            Variant wv = wx.Value;
            string sz = wv.GetStringOrDefault("ShiZhuang");

            IList wl = wv.GetValue<IList>("WardrobeList");
            if (wl == null)
            {
                note.Call(WardrobeCommand.FashionExchangeR, false, TipManager.GetMessage(WardrobeReturn.FashionExchange3));
                return;
            }

            Variant v = gc.Value;
            string goodsid = v.GetStringOrDefault("GoodsID");
            Variant ng = v.GetVariantOrDefault("NeedGoods");

            foreach (var item in ng)
            {
                if (!wl.Contains(item.Key))
                {
                    note.Call(WardrobeCommand.FashionExchangeR, false, TipManager.GetMessage(WardrobeReturn.FashionExchange4));
                    return;
                }
            }

            bool isFash = false;
            foreach (var item in ng)
            {
                //移除时装
                if (sz == item.Key)
                {
                    wv["ShiZhuang"] = "";
                    isFash = true;
                }
                wl.Remove(item.Key);
            }
            if (!wx.Save())
            {
                note.Call(WardrobeCommand.FashionExchangeR, false, TipManager.GetMessage(WardrobeReturn.FashionExchange5));
                return;
            }

            if (isFash)
            {
                note.Player.ShiZhuangInfo();
            }
            note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(wx));
            note.Call(WardrobeCommand.FashionExchangeR, true, TipManager.GetMessage(WardrobeReturn.FashionExchange6));
        }

        /// <summary>
        /// 判断时装是否已经下架
        /// </summary>
        /// <param name="pb"></param>
        public static void DownWardrobe(PlayerBusiness pb)
        {
            string soleid = pb.ID + "DownWardrobe";
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                PlayerEx wx = pb.Wardrobe;
                Variant wv = wx.Value;
                string sz = wv.GetStringOrDefault("ShiZhuang");
                
                if (string.IsNullOrEmpty(sz))
                    return;

                GameConfig gc = GameConfigAccess.Instance.FindOneById(sz);
                if (gc == null)
                    return;

                IList wl = wv.GetValue<IList>("WardrobeList");
                if (wl != null) 
                {
                    //如果已经购买则不需要下架
                    if (wl.Contains(sz))
                        return;
                }
                HashSet<string> hs = WardrobeAccess.Wardrobe();
                
                
                if (!hs.Contains(sz)) 
                {
                    wv["ShiZhuang"] = "";
                    if (wx.Save())
                    {
                        pb.ShiZhuangInfo();
                        pb.Call(ClientCommand.UpdateActorR, new PlayerExDetail(wx));
                        pb.Call(ClientCommand.SendActivtyR, new object[] { "T01", string.Format(TipManager.GetMessage(WardrobeReturn.DownWardrobe1), gc.Name) });
                    }
                }
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }
    }
}
