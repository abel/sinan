using System.Collections.Generic;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.AuctionModule.Business
{
    public class AuctionBase
    {
        public const string Seller = "Sell";
        public const string Bid = "Bid";


        /// <summary>
        /// 得到出售列表
        /// </summary>
        /// <param name="playerid"></param>
        /// <param name="str"></param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="pageIndex">当前页数</param>
        /// <returns></returns>
        public static List<Variant> GetAuctionSellList(string playerid, string str, int pageSize, int pageIndex,out int total,out int curIndex) 
        {
            int m = 0;
            int n = 0;
            List<Auction> list = null;
            if (Seller==str) 
            {
                list = AuctionAccess.Instance.AuctionSellerList(playerid, pageSize, pageIndex, out m, out n);
            }
            if (Bid == str)
            {
                list = AuctionAccess.Instance.AuctionBidderList(playerid, pageSize, pageIndex, out m, out n);
            }
            List<Variant> msg = new List<Variant>();
            if (list != null)
            {
                foreach (Auction model in list)
                {
                    Variant mv = model.Value;
                    if (mv == null) 
                        continue;

                    Variant v = new Variant();
                    foreach (var item in mv) 
                    {
                        v.Add(item.Key, item.Value);
                    }
                    v.Add("ID", model.ID);
                    v.Add("Name", model.Name);
                    msg.Add(v);
                }
            }

            total = m;
            curIndex = n;
            return msg;
        }

    }
}
