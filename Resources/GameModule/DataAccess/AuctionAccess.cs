using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Common;

namespace Sinan.GameModule
{
    public class AuctionAccess : VariantBuilder<Auction>
    {
        readonly static AuctionAccess m_instance = new AuctionAccess();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static AuctionAccess Instance
        {
            get { return m_instance; }
        }

        AuctionAccess()
            : base("Auction")
        {
        }

        public override bool Save(Auction t)
        {
            return base.Save(t);
        }

        /// <summary>
        /// 得到出售物品列表
        /// </summary>
        /// <param name="playerid">出售者ID</param>
        /// <returns></returns>
        public List<Auction> AuctionSellerList(string playerid, int pageSize, int pageIndex, out int total, out int curIndex)
        {
            var query = Query.And(
                Query.EQ("Value.SellerID", playerid),
                Query.EQ("Value.Status", 0),
                Query.GT("Value.EndTime", DateTime.UtcNow)
                );
            total = m_collection.Count(query);
            if (total <= 0)
            {
                curIndex = 0;
                return new List<Auction>();
            }
            int y = 0;
            int s = Math.DivRem(total, pageSize, out y);
            if (y > 0) s++;
            if (pageIndex + 1 > s) 
            {
                pageIndex = s; 
            }
            SortByBuilder sb = SortBy.Descending("Value.EndTime");
            curIndex = pageIndex;
            var n = m_collection.FindAs<Auction>(query).SetSortOrder(sb).SetSkip(curIndex * pageSize).SetLimit(pageSize);
            return n.ToList();
        }

        /// <summary>
        /// 得到出售记录条数
        /// </summary>
        /// <param name="playerid">角色</param>
        /// <returns></returns>
        public int AuctionSellerCount(string playerid)
        {
            var query = Query.And(Query.EQ("Value.SellerID", playerid),Query.LTE("Value.Status", 0),Query.GT("Value.EndTime", DateTime.UtcNow));
            return m_collection.Count(query);
        }

        /// <summary>
        /// 得到竞拍物品列表
        /// </summary>
        /// <param name="playerid">竞拍者ID</param>
        /// <returns></returns>
        public List<Auction> AuctionBidderList(string playerid,int pageSize, int pageIndex, out int total, out int curIndex)
        {
            var query = Query.And(
                Query.EQ("Value.BidderID", playerid),
                Query.EQ("Value.Status", 0),
                Query.GT("Value.EndTime", DateTime.UtcNow)
                );
            total = m_collection.Count(query);
            if (total <= 0)
            {
                curIndex = 0;
                return new List<Auction>();
            }
            int y = 0;
            int s = Math.DivRem(total, pageSize, out y);
            if (y > 0) s++;
            if (pageIndex + 1 > s)
            {
                pageIndex = s;
            }
            SortByBuilder sb = SortBy.Descending("Value.EndTime");            
            curIndex = pageIndex;
            var n = m_collection.FindAs<Auction>(query).SetSortOrder(sb).SetSkip(curIndex * pageSize).SetLimit(pageSize);
            return n.ToList();
        }

        /// <summary>
        /// 得到竞拍物品列表
        /// </summary>
        /// <returns></returns>
        public List<Auction> AuctionList()
        {
            var query = Query.And(Query.EQ("Value.Status", 0));
            var result = m_collection.FindAs<Auction>(query).ToList();
            return result;
        }

        /// <summary>
        /// 得到出售物的列表
        /// </summary>
        /// <param name="playerid">角色ID</param>
        /// <param name="pageSize">每一页的条数</param>
        /// <param name="pageIndex">页数</param>
        /// <param name="currPage">当前页数</param>
        /// <returns></returns>
        public List<Auction> AuctionBuyList(string playerID, string goodsType, bool isName, int pageSize, int pageIndex, out int totalCount, out int currPage)
        {
            int total = 0;
            IMongoQuery query = null;
            DateTime dt = DateTime.UtcNow;
            if (!isName)
            {
                query = Query.And(
                    Query.NE("Value.SellerID", playerID),
                    Query.Matches("Name", new BsonRegularExpression(".*" + goodsType + ".*", "i")),
                    Query.EQ("Value.Status", 0),
                    Query.GT("Value.EndTime", dt)
                    );
            }
            else
            {
                if (goodsType != "all")
                {
                    query = Query.And(
                        Query.NE("Value.SellerID", playerID),
                        Query.EQ("Value.GoodsType", goodsType),
                        Query.EQ("Value.Status", 0),
                        Query.GT("Value.EndTime", dt)
                        );
                }
                else
                {
                    query = Query.And(
                        Query.NE("Value.SellerID", playerID),
                        Query.EQ("Value.Status", 0),
                        Query.GT("Value.EndTime", dt)
                        );
                }
            }

            total = m_collection.Count(query);
            int y = 0;
            int s = Math.DivRem(total, pageSize, out y);
            if (y > 0) s++;
            if (pageIndex + 1 > s)
            {
                pageIndex = s;
            }
            SortByBuilder sb = SortBy.Descending("Value.GoodsID").Ascending("Value.Price");
            currPage = pageIndex;
            var n = m_collection.FindAs<Auction>(query).SetSortOrder(sb).SetSkip(currPage * pageSize).SetLimit(pageSize);
            totalCount = total;
            return n.ToList();
        }

        /// <summary>
        /// 得到拍卖物品相关信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Auction FindOne(string id)
        {
            var query = Query.And(Query.EQ("_id", id), Query.EQ("Value.Status", 0));
            var result = m_collection.FindOneAs<Auction>(query);
            return result;
        }

        /// <summary>
        /// 得到当期拍卖列表
        /// </summary>
        /// <returns></returns>
        public List<Auction> FindExpiredList()
        {
            var query = Query.And(Query.EQ("Value.Status", 0), Query.LT("Value.EndTime", DateTime.UtcNow));
            return m_collection.FindAs<Auction>(query).ToList();
        }


        /// <summary>
        /// 拍卖
        /// </summary>
        /// <param name="id">订单</param>
        /// <param name="price">竞价</param>
        /// <param name="baderid">拍卖者</param>
        /// <param name="bidname">拍卖者名称</param>        
        /// <returns></returns>
        public bool AddAution(string id, int price, string baderid, string bidname)
        {
            var update = Update.Set("Modified", DateTime.UtcNow).Inc("Ver", 1);
            var query = Query.And(
                Query.EQ("_id", id),
                Query.EQ("Value.Status", 0),
                Query.GT("Value.EndTime", DateTime.UtcNow),
                Query.EQ("Value.Price", price)
                );
            update.Set("Value.Status", 1);
            update.Set("Value.BidPrice", price);
            var pf = m_collection.Update(query, update, UpdateFlags.Upsert, SafeMode.True);
            if (pf != null && pf.Ok)
            {
                return pf.DocumentsAffected > 0;
            }
            return false;
        }

        /// <summary>
        /// 移除拍买行过期数据,过期后保留7天
        /// </summary>
        /// <returns></returns>
        public void AutionDel()
        {
            DateTime dt = DateTime.UtcNow.AddDays(-7);
            //过期7天的拍卖行东西将移除
            var query = Query.And(Query.LTE("Value.EndTime", dt));
            m_collection.Remove(query);
            //return v == null ? false : v.DocumentsAffected > 0;            
        }

        /// <summary>
        /// GM删除拍卖道具
        /// </summary>
        /// <param name="playerid"></param>
        public bool GMAuctionDel(string playerid, string[] strs)
        {
            BsonArray bson = new BsonArray();
            foreach (string id in strs)
            {
                if (string.IsNullOrEmpty(id))
                    continue;
                bson.Add(id);
            }
            if (bson.Count <= 0)
                return false;

            //表示领取后删除邮件
            var query = Query.And(
                Query.In("_id", bson),
                Query.EQ("Value.SoleID", playerid),
                Query.EQ("Status", 0),
                Query.EQ("Value.BidderID", "")
                );

            //4表示GM删除 
            var update = Update.Set("Value.Status", 4).Set("Modified", DateTime.UtcNow).Inc("Ver", 1);
            var v = m_collection.Update(query, update, UpdateFlags.Multi, SafeMode.True);
            if (v == null)
            {
                return false;
            }
            return v.DocumentsAffected > 0;
        }
    }
}
