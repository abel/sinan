using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Data;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Util;
using Sinan.Command;

namespace Sinan.EmailModule.Business
{
    public class EmailBase
    {
        /// <summary>
        /// 邮件相关信息
        /// </summary>
        /// <param name="playerid">发件人</param>
        /// <param name="name">发件人名称</param>
        /// <param name="receiveid">接收人</param>
        /// <param name="receivename">接收者名称</param>
        /// <param name="d">邮件内容</param>
        /// <returns></returns>
        public static Variant CreateEmailValue(string playerid, string name, string receiveid, string receivename, Variant d)
        {
            Variant v = new Variant();
            v.Add("SendID", playerid);
            v.Add("SendName", name);
            v.Add("ReceiveID", receiveid);
            v.Add("ReceiveName", receivename);
            v.Add("Content", d.GetValue<object>("mailMess"));
            DateTime dt = DateTime.UtcNow;
            v.Add("UpdateDate", dt);
            //邮件有效天数
            int day = Convert.ToInt32(TipManager.GetMessage(EmailReturn.HameDay));
            v.Add("EndDate", dt.AddDays(day));

            int IsHave = 0;
            if (d.ContainsKey("moneyGoods"))
            {
                Variant money = d.GetVariantOrDefault("moneyGoods");
                if (money.ContainsKey("Coin"))
                {
                    v.Add("Coin", money.GetIntOrDefault("Coin"));
                    IsHave = money.GetIntOrDefault("Coin") > 0 ? 1 : 0;
                }
                else
                {
                    v.Add("Coin", 0);
                }
                if (money.ContainsKey("Score"))
                {
                    v.Add("Score", money.GetIntOrDefault("Score"));
                    IsHave = money.GetIntOrDefault("Score") > 0 ? 1 : 0;
                }
                else
                {
                    v.Add("Score", 0);
                }
            }
            else
            {
                v.Add("Coin", 0);
                v.Add("Score", 0);
            }

            if (d.ContainsKey("goodsList"))
            {
                List<Variant> list = new List<Variant>();
                IList goodsList = d.GetValue<IList>("goodsList");
                foreach (Variant msg in goodsList)
                {
                    GameConfig gc = GameConfigAccess.Instance.FindOneById(msg.GetStringOrDefault("E"));
                    string goodsType = string.Empty;
                    int sort = 0;
                    if (gc != null)
                    {
                        goodsType = gc.Value.GetStringOrDefault("GoodsType");
                        sort = gc.Value.GetIntOrDefault("Sort");
                    }
                    else
                    {
                        if (gc == null)
                        {
                            Goods g = GoodsAccess.Instance.FindOneById(msg.GetStringOrDefault("E"));
                            if (g != null)
                            {
                                goodsType = g.Value.GetStringOrDefault("GoodsType");
                                sort = g.Value.GetIntOrDefault("Sort");
                            }
                            else
                            {
                                if (g == null)
                                {
                                    //查询是否是宠物
                                    Pet pet = PetAccess.Instance.FindOneById(msg.GetStringOrDefault("E"));
                                    if (pet != null)
                                    {
                                        goodsType = "Pet";
                                        sort = pet.Value.GetIntOrDefault("Sort");
                                    }
                                }
                            }
                        }
                    }
                    if (goodsType == string.Empty)
                        continue;

                    Variant gs = new Variant();
                    gs.Add("SoleID", msg.GetStringOrDefault("E"));
                    gs.Add("GoodsID", msg.GetStringOrDefault("G"));
                    gs.Add("Number", msg.GetIntOrDefault("A"));
                    gs.Add("GoodsType", goodsType);
                    gs.Add("Sort", sort);

                    gs.Add("Coin", msg.GetIntOrDefault("Coin"));
                    gs.Add("Score", msg.GetIntOrDefault("Score"));

                    gs.Add("T", msg.GetVariantOrDefault("T"));
                    list.Add(gs);
                }
                v.Add("GoodsList", list);
                if (list.Count > 0)
                {
                    IsHave = 1;
                }
            }
            v.Add("IsHave", IsHave);
            return v;
        }
    }
}
