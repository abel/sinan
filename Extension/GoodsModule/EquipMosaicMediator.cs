using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Util;
using Sinan.Extensions;
using Sinan.Log;

namespace Sinan.GoodsModule
{
    /// <summary>
    /// 镶嵌
    /// </summary>
    sealed public class EquipMosaicMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[]
            {
                MosaicCommand.MosaicNeed,
                MosaicCommand.Mosaic
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
            // 需验证玩家登录的操作.....
            if (note.Player == null) return;
            //表示角色正在战斗中，客户端提交信息不进行处理
            if (note.Player.AState == ActionState.Fight)
                return;
            switch (note.Name)
            {
                case MosaicCommand.Mosaic:
                    Mosaic(note);
                    break;
                case MosaicCommand.MosaicNeed:
                    MosaicNeed(note);
                    break;

            }
        }

        private void MosaicNeed(UserNote note)
        {
            string goodsid = note.GetString(0);
            Goods g = GoodsAccess.Instance.GetGoodsByID(goodsid, note.PlayerID);
            if (g == null)
            {
                note.Call(MosaicCommand.MosaicNeedR, false, TipManager.GetMessage(MosaicReturn.MosaicEquipNo));
                return;
            }

            if (g.Value.GetIntOrDefault("IsChange") == 0)
            {
                note.Call(ExchangeCommand.ExchangNeedR, false, TipManager.GetMessage(MosaicReturn.MosaicNoChange));
                return;
            }

            Variant BaoShiInfo = g.Value.GetVariantOrDefault("BaoShiInfo");

            bool IsLet = true;
            foreach (string d in BaoShiInfo.Keys)
            {
                if (BaoShiInfo[d].ToString() != "-1")
                {
                    IsLet = false;
                    break;
                }
            }

            if (IsLet)
            {
                note.Call(MosaicCommand.MosaicNeedR, false, TipManager.GetMessage(MosaicReturn.MosaicNoLet));
                return;
            }

            List<GameConfig> mosaic = GameConfigAccess.Instance.Find("Mosaic");
            GameConfig gc = null;
            foreach (GameConfig gcg in mosaic)
            {
                if (gcg.Value.GetStringOrDefault("Type") == g.Value.GetStringOrDefault("GoodsType"))
                {
                    gc = gcg;
                    break;
                }
            }

            if (gc == null)
            {
                note.Call(MosaicCommand.MosaicNeedR, false, TipManager.GetMessage(MosaicReturn.MosaicNoLet));
                return;
            }

            string UpID = gc.Value.GetStringOrDefault("UpID");

            int UpCount = 0;

            PlayerEx b = note.Player.B0;
            foreach (Variant con in b.Value.GetValueOrDefault<IList>("C"))
            {
                if (con.GetStringOrDefault("G") == UpID)
                    UpCount += con.GetIntOrDefault("A");
            }
            //可以镶嵌的宝石
            IList MosaicNeed = gc.Value.GetValue<IList>("MosaicNeed");

            List<string> bs = new List<string>();
            foreach (string key in MosaicNeed)
            {
                GameConfig tmp = GameConfigAccess.Instance.FindOneById(key);
                if (tmp == null)
                    continue;
                if (g.Value.GetIntOrDefault("Level") >= tmp.Value.GetIntOrDefault("Level"))
                {
                    bs.Add(tmp.ID);
                }
            }

            if (bs.Count == 0)
            {
                note.Call(MosaicCommand.MosaicNeedR, false, TipManager.GetMessage(MosaicReturn.MosaicNoLet));
                return;
            }
            Variant v = new Variant();
            v.Add("BaoShiInfo", bs);
            v.Add("UpID", UpID);
            v.Add("UpCount", UpCount);
            note.Call(MosaicCommand.MosaicNeedR, true, v);
        }

        /// <summary>
        /// 镶嵌,主要是将宝石镶嵌在装备上
        /// </summary>
        /// <param name="note"></param>
        private void Mosaic(UserNote note)
        {
            //宝石等级不能超过装备等级,必须能够改造
            string goodsid = note.GetString(0);
            //提升成功率的道具
            int upcount = note.GetInt32(1);
            //镶在什么位置
            string p = note.GetString(2);
            string baoshiid = note.GetString(3);

            int baoship = note.GetInt32(4);//宝石在包袱中的位置

            string npcid = note.GetString(5);
            if (!note.Player.EffectActive(npcid, ""))
                return;

            Goods g = GoodsAccess.Instance.GetGoodsByID(goodsid, note.PlayerID);
            if (g == null)
            {
                note.Call(MosaicCommand.MosaicR, false, TipManager.GetMessage(MosaicReturn.MosaicEquipNo));
                return;
            }

            if (g.Value.GetIntOrDefault("IsChange") == 0)
            {
                note.Call(MosaicCommand.MosaicR, false, TipManager.GetMessage(MosaicReturn.MosaicNoChange));
                return;
            }

            List<GameConfig> mosaic = GameConfigAccess.Instance.Find("Mosaic");
            GameConfig gc = null;
            foreach (GameConfig gcg in mosaic)
            {
                if (gcg.Value.GetStringOrDefault("Type") == g.Value.GetStringOrDefault("GoodsType"))
                {
                    gc = gcg;
                    break;
                }
            }
            if (gc == null)
            {
                note.Call(MosaicCommand.MosaicR, false, TipManager.GetMessage(MosaicReturn.MosaicNoLet));
                return;
            }
            //允许镶嵌的宝石列表
            IList mosaicNeed = gc.Value.GetValue<IList>("MosaicNeed");


            GameConfig baoshi = GameConfigAccess.Instance.FindOneById(baoshiid);
            if (baoshi == null)
            {
                note.Call(MosaicCommand.MosaicR, false, TipManager.GetMessage(MosaicReturn.MosaicNoBaoShi));
                return;
            }

            int baoShiLevel = baoshi.Value.GetIntOrDefault("Level");

            //需用绑定游戏币数量

            Variant price = g.Value.GetVariantOrDefault("Price");
            Variant buy = price.GetVariantOrDefault("Buy");



            if (!mosaicNeed.Contains(baoshi.ID))
            {
                //不能够镶嵌该宝石
                note.Call(MosaicCommand.MosaicR, false, TipManager.GetMessage(MosaicReturn.MosaicNoLet));
                return;
            }

            //得到物品所在孔的信息
            Variant baoShiInfo = g.Value.GetVariantOrDefault("BaoShiInfo");

            object o;
            if (!baoShiInfo.TryGetValueT(p, out o))
            {
                note.Call(MosaicCommand.MosaicR, false, TipManager.GetMessage(MosaicReturn.MosaicEquipNo));
                return;
            }

            if ((string)o == "-1")
            {
                note.Call(MosaicCommand.MosaicR, false, TipManager.GetMessage(MosaicReturn.MosaicNoPunch));
                return;
            }

            GameConfig goods = null;

            if ((string)o != "0")
            {
                //表示想镶嵌的位置存在宝石
                goods = GameConfigAccess.Instance.FindOneById((string)o);
                //表示该位置已经镶嵌宝石，但新宝石要求必须大于旧宝石
                if (goods.Value.GetIntOrDefault("Level") >= baoShiLevel)
                {
                    //新镶嵌的宝石等级要求大于旧宝石
                    note.Call(MosaicCommand.MosaicR, false, TipManager.GetMessage(MosaicReturn.MosaicNewOrOld));
                    return;
                }
            }

            string upID = gc.Value.GetStringOrDefault("UpID");

            GameConfig upGoods = GameConfigAccess.Instance.FindOneById(upID);
            if (upGoods == null)
            {
                note.Call(MosaicCommand.MosaicR, false, TipManager.GetMessage(MosaicReturn.MosaicEquipNo));
                return;
            }

            //GameConfig gcWin = ConfigAccess.Instance.FindOneById(g.GoodsID);
            //if (gcWin == null) 
            //    return;

            int score = Convert.ToInt32(baoShiLevel * GetPrice(g.GoodsID) * 0.2);

            if (note.Player.Score < score || (!note.Player.AddScore(-score, FinanceType.Mosaic)))
            {
                note.Call(MosaicCommand.MosaicR, false, TipManager.GetMessage(MosaicReturn.MosaicNoScoreB));
                return;
            }

            int upCount = 0;

            PlayerEx b = note.Player.B0;

            bool isBaoShi = false;
            IList c = b.Value.GetValue<IList>("C");
            Variant vr = null;
            Variant zb = null;
            foreach (Variant con in c)
            {
                if (con.GetIntOrDefault("P") == baoship)
                {
                    if (con.GetStringOrDefault("E") == baoshiid)
                    {
                        vr = con;
                        //break;
                    }
                    else
                    {
                        note.Call(MosaicCommand.MosaicR, false, TipManager.GetMessage(MosaicReturn.MosaicNoLet));
                        return;
                    }
                }
                if (con.GetStringOrDefault("G") == upID)
                {
                    upCount += con.GetIntOrDefault("A");
                }

                if (zb == null)
                {
                    if (con.GetStringOrDefault("E") == goodsid)
                    {
                        zb = con;
                    }
                }
            }
            if (vr != null)
            {
                isBaoShi = true;
            }

            if (!isBaoShi)
            {
                note.Call(MosaicCommand.MosaicR, false, TipManager.GetMessage(MosaicReturn.MosaicNoLet));
                return;
            }

            if (upCount < upcount)
            {
                note.Call(MosaicCommand.MosaicR, false, TipManager.GetMessage(MosaicReturn.MosaicNoLet));
                return;
            }



            double lv = GetSuccessLv(baoShiLevel);
            if (upcount > 0)
            {
                lv += GoodsAccess.Instance.GetSuccess(upcount);
            }

            Variant us = new Variant();

            if (upcount > 0)
            {
                //BurdenAccess.Remove(b, UpID, upcount);
                //移除提高成功率的道具
                note.Player.RemoveGoods(upID, upcount, GoodsSource.Mosaic);

                us[upID] = upcount;
            }

            if (!note.Player.RemoveGoods(baoshiid, 1, GoodsSource.Mosaic))
            {
                note.Call(MosaicCommand.MosaicR, false, TipManager.GetMessage(MosaicReturn.MosaicNoBaoShi));
                return;
            }

            //使用的宝石
            us[baoshiid] = 1;
            bool isSuccess = NumberRandom.RandomHit(lv);
            if (isSuccess)
            {
                baoShiInfo[p] = baoshiid;
                b.Save();
                g.Save();                
                note.Player.FinishNote(FinishCommand.XianQian, g.ID);
                note.Player.UpdateBurden();
                note.Call(MosaicCommand.MosaicR, true, goodsid);            
            }
            else
            {
                note.Player.UpdateBurden();
                note.Call(MosaicCommand.MosaicR, false, TipManager.GetMessage(MosaicReturn.MosaicFail));                
            }
            
            
            Variant os=new Variant();
            os["IsSuccess"] = isSuccess;
            //装备唯一标识
            os["ID"] = g.ID;
            os["GoodsID"] = g.GoodsID;
            os["Lv"] = lv;
            os["Score"] = -score;
            note.Player.AddLogVariant(Actiontype.Mosaic, us, null,os);
        }

        /// <summary>
        /// 得到镶嵌成功率
        /// </summary>
        /// <param name="Level">宝石等级</param>
        /// <returns></returns>
        private double GetSuccessLv(int Level)
        {
            switch (Level)
            {
                case 1:
                    return 1.00;
                case 2:
                    return 0.80;
                case 3:
                    return 0.60;
                case 4:
                    return 0.50;
                case 5:
                    return 0.40;
                case 6:
                    return 0.30;
                case 7:
                    return 0.20;
                case 8:
                    return 0.10;
            }
            return 0;
        }


        private int GetPrice(string goodsid)
        {
            int priceT = 0;
            GameConfig gcWin = GameConfigAccess.Instance.FindOneById(goodsid);
            if (gcWin == null)
                return priceT;
            Variant v = gcWin.Value;
            Variant price = v.GetVariantOrDefault("Price");
            if (price != null)
            {
                Variant buy = price.GetVariantOrDefault("Buy");
                if (price != null)
                {
                    priceT = buy.GetIntOrDefault("Score");
                }
            }
            return priceT;
        }
    }
}
