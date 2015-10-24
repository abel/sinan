using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.GameModule
{
    public class EmailAccess : VariantBuilder<Email>
    {
        readonly static EmailAccess m_instance = new EmailAccess();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static EmailAccess Instance
        {
            get { return m_instance; }
        }

        EmailAccess()
            : base("Email")
        {
        }

        /// <summary>
        /// 邮件分页,只查询附件
        /// </summary>
        /// <param name="playerid">用户ID</param>
        /// <param name="type">收发类型</param>
        /// <param name="pageSize">每一页的条数</param>
        /// <param name="pageIndex">页数</param>
        /// <param name="status">
        /// 0所有,
        /// 1表示没有领奖,
        /// 2表示已经领奖,
        /// 3表示领奖删除,
        /// 4表示没有领奖就删除</param>
        /// <param name="total">总条数</param>
        /// <returns></returns>
        public List<Email> GMEmailList(string playerid, int emailType, int pageSize, int pageIndex, int status, out int total)
        {
            string key = "";
            if (emailType == 0)
            {
                key = "ReceiveID";
            }
            else
            {
                key = "SendID";
            }

            SortByBuilder sb = SortBy.Descending("Created");
            IMongoQuery qc;
            if (status == 0)
            {
                qc = Query.And(Query.EQ("Value." + key, playerid), Query.EQ("Value.IsHave", 1));
            }
            else if (status == 1)
            {
                qc = Query.And(Query.EQ("Value." + key, playerid), Query.EQ("Value.IsHave", 1), Query.LTE("Status", 2));
            }
            else if (status == 2)
            {
                //已经领取没有的删除邮件
                qc = Query.And(Query.EQ("Value." + key, playerid), Query.EQ("Value.IsHave", 1), Query.EQ("Status", 2));
            }
            else if (status == 3)
            {
                //已经领取没有的删除邮件
                qc = Query.And(Query.EQ("Value." + key, playerid), Query.EQ("Value.IsHave", 1), Query.EQ("Status", 3));
            }
            else
            {
                //表示直接删除的邮件
                qc = Query.And(Query.EQ("Value." + key, playerid), Query.EQ("Value.IsHave", 1), Query.EQ("Status", 4));
            }

            total = m_collection.Count(qc);
            if (total == 0 || pageIndex < 0)
            {
                return new List<Email>();
            }

            int y = 0;
            int s = Math.DivRem(total, pageSize, out y);
            if (y > 0) s++;

            if (pageIndex + 1 > s)
            {
                pageIndex = s - 1;
            }

            var n = m_collection.FindAs<Email>(qc).SetSortOrder(sb).SetSkip(pageIndex * pageSize).SetLimit(pageSize);
            return n.ToList();
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="playerid">角色ID</param>
        /// <param name="pageSize">每一页的条数</param>
        /// <param name="pageIndex">页数</param>
        /// <param name="num">0按时间排序,1未读,2附件,3到期时间</param>
        /// <param name="totalCount">总条数</param>
        /// <returns></returns>
        public List<Email> EmailPage(string playerid, int pageSize, int pageIndex, int num, out int totalCount)
        {
            SortByBuilder sb = SortBy.Descending("Created");
            IMongoQuery qc;
            if (num == 0)
            {
                qc = Query.And(Query.EQ("Value.ReceiveID", playerid), Query.LTE("Status", 2));
            }
            else if (num == 1)
            {
                //未读取的邮件
                qc = Query.And(Query.EQ("Value.ReceiveID", playerid), Query.EQ("Status", 0));
            }
            else if (num == 2)
            {
                //得到附件
                qc = Query.And(Query.EQ("Value.ReceiveID", playerid), Query.LTE("Status", 2), Query.LTE("Value.IsHave", 1));
            }
            else
            {
                sb = SortBy.Ascending("Value.EndDate");
                qc = Query.And(Query.EQ("Value.ReceiveID", playerid), Query.LTE("Status", 2));//小于等于2
            }
            totalCount = m_collection.Count(qc);
            if (totalCount == 0 || pageIndex < 0)
                return new List<Email>();

            int y = 0;
            int s = Math.DivRem(totalCount, pageSize, out y);
            if (y > 0) s++;

            if (pageIndex + 1 > s)
            {
                pageIndex = s - 1;
            }

            var n = m_collection.FindAs<Email>(qc).SetSortOrder(sb).SetSkip(pageIndex * pageSize).SetLimit(pageSize);
            return n.ToList();
        }

        /// <summary>
        /// 新邮件条数
        /// </summary>
        /// <param name="playerid"></param>
        /// <returns></returns>
        public int NewTotal(string playerid)
        {
            var query = Query.And(Query.EQ("Value.ReceiveID", playerid), Query.EQ("Status", 0));
            return m_collection.Count(query);
        }

        /// <summary>
        /// 删除邮件,表示GM删除
        /// </summary>
        /// <param name="list"></param>
        /// <param name="playerid"></param>
        /// <returns></returns>
        public bool GMRemoveEmail(string[] strs, string playerid)
        {
            BsonArray bson = new BsonArray();
            foreach (string id in strs)
            {
                if (string.IsNullOrEmpty(id))
                    continue;
                bson.Add(id);
            }
            if (bson.Count <= 0)
                return true;

            long docff = 0;
            //表示领取后删除邮件
            var query = Query.And(Query.In("_id", bson), Query.EQ("Value.ReceiveID", playerid), Query.EQ("Status", 2));
            var update = Update.Set("Status", 5).Set("Modified", DateTime.UtcNow).Inc("Ver", 1);
            var v = m_collection.Update(query, update, UpdateFlags.Multi, SafeMode.True);
            if (v != null)
            {
                docff = v.DocumentsAffected;
            }

            //表示没有领取就删除
            var query1 = Query.And(Query.In("_id", bson), Query.EQ("Value.ReceiveID", playerid), Query.LT("Status", 2));
            var update1 = Update.Set("Status", 6).Set("Modified", DateTime.UtcNow).Inc("Ver", 1);
            var v1 = m_collection.Update(query1, update1, UpdateFlags.Multi, SafeMode.True);
            if (v1 != null)
            {
                docff += v1.DocumentsAffected;
            }

            return docff > 0;
        }

        /// <summary>
        /// 删除邮件
        /// </summary>
        /// <param name="list"></param>
        /// <param name="playerid"></param>
        /// <returns></returns>
        public bool RemoveEmail(IList list, string playerid)
        {
            BsonArray bson = new BsonArray();
            foreach (string id in list)
            {
                if (string.IsNullOrEmpty(id))
                    continue;
                bson.Add(id);
            }

            long docff = 0;
            //表示领取后删除邮件
            var query = Query.And(Query.In("_id", bson), Query.EQ("Value.ReceiveID", playerid), Query.EQ("Status", 2));
            var update = Update.Set("Status", 3).Set("Modified", DateTime.UtcNow).Inc("Ver", 1);
            var v = m_collection.Update(query, update, UpdateFlags.Multi, SafeMode.True);
            if (v != null)
            {
                docff = v.DocumentsAffected;
            }

            //表示没有领取就删除
            var query1 = Query.And(Query.In("_id", bson), Query.EQ("Value.ReceiveID", playerid), Query.LT("Status", 2));
            var update1 = Update.Set("Status", 4).Set("Modified", DateTime.UtcNow).Inc("Ver", 1);
            var v1 = m_collection.Update(query1, update1, UpdateFlags.Multi, SafeMode.True);
            if (v1 != null)
            {
                docff += v1.DocumentsAffected;
            }

            return docff > 0;
        }

        /// <summary>
        /// 邮件更新
        /// </summary>
        /// <param name="id">邮件id</param>
        /// <returns></returns>
        public bool UpdateEmail(string id) 
        {
            if (!string.IsNullOrEmpty(id))
            {
                var query = Query.And(Query.EQ("_id", id), Query.EQ("Status", 0));
                var update = Update.Set("Status", 1).Set("Modified", DateTime.UtcNow).Inc("Ver", 1);
                var v = m_collection.Update(query, update, UpdateFlags.None, SafeMode.False);
                return v == null ? false : v.DocumentsAffected > 0;
            }
            return false;
        }

        /// <summary>
        /// 得到过期邮件列表
        /// </summary>
        /// <returns></returns>
        public List<Email> EmailList(DateTime dt)
        {
            var query = Query.LTE("Value.EndDate", dt);
            //return m_collection.FindAllAs<Email>().ToList();
            return m_collection.FindAs<Email>(query).ToList();
        }

        /// <summary>
        /// 移除过期邮件
        /// </summary>
        /// <returns></returns>
        public void DeleteEmail()
        {
            DateTime dt = DateTime.UtcNow.AddDays(-7);
            //用户主动删除一周的邮件,从数据库中移除
            //var query = Query.And(Query.EQ("Status", 3), Query.LTE("Modified", dt));            
            var query = Query.And(Query.GTE("Status", 3), Query.LTE("Modified", dt)); 
            //过期邮件，主动从数据库中移除
            var query1 = Query.LTE("Value.EndDate", DateTime.UtcNow);
            //
            m_collection.Remove(Query.Or(query, query1), SafeMode.False);

            //var v = m_collection.Remove(Query.Or(query, query1), SafeMode.False);
            //return v == null ? false : v.DocumentsAffected > 0;
        }



        /// <summary>
        /// 邮件创建
        /// </summary>
        /// <param name="playerid">发送者</param>
        /// <param name="name">发送者名称</param>
        /// <param name="receiveid">接收者</param>
        /// <param name="receivename">接收者名称</param>
        /// <param name="d"></param>
        /// <returns></returns>
        public Variant CreateEmailValue(string playerid, string name, string receiveid, string receivename, Variant d)
        {
            Variant v = new Variant();
            v.Add("SendID", playerid);
            v.Add("SendName", name);
            v.Add("ReceiveID", receiveid);
            v.Add("ReceiveName", receivename);
            v.Add("Content", d["mailMess"]);
            DateTime dt = DateTime.UtcNow;
            v.Add("UpdateDate", dt);
            v.Add("EndDate", dt.AddDays(Convert.ToInt32(d["reTime"])));

            int IsHave = 0;
            if (d.ContainsKey("moneyGoods"))
            {
                Variant money = d["moneyGoods"] as Variant;
                if (money.ContainsKey("Coin"))
                {
                    v.Add("Coin", Convert.ToInt32(money["Coin"]));
                    IsHave = Convert.ToInt32(money["Coin"]) > 0 ? 1 : 0;
                }
                else
                {
                    v.Add("Coin", 0);
                }
                if (money.ContainsKey("Score"))
                {
                    v.Add("Score", Convert.ToInt32(money["Score"]));
                    IsHave = Convert.ToInt32(money["Score"]) > 0 ? 1 : 0;
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
                IList goodsList = d["goodsList"] as IList;
                List<Variant> list = new List<Variant>();
                foreach (Variant msg in goodsList)
                {
                    GameConfig gc = GameConfigAccess.Instance.FindOneById(msg["E"].ToString());
                    string goodsType = string.Empty;
                    int sort = 0;
                    if (gc != null)
                    {
                        goodsType = gc.Value["GoodsType"].ToString();
                        sort = Convert.ToInt32(gc.Value["Sort"]);
                    }
                    else
                    {
                        if (gc == null)
                        {
                            Goods g = GoodsAccess.Instance.FindOneById(msg["E"].ToString());
                            if (g != null)
                            {
                                goodsType = g.Value["GoodsType"].ToString();
                                sort = Convert.ToInt32(g.Value["Sort"]);
                            }
                            else
                            {
                                if (g == null)
                                {
                                    //查询是否是宠物
                                    Pet pet = PetAccess.Instance.FindOneById(msg["E"].ToString());
                                    if (pet != null)
                                    {
                                        goodsType = "Pet";
                                        sort = Convert.ToInt32(pet.Value["Sort"]);
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
                    gs.Add("H", msg.GetIntOrDefault("H"));

                    if (msg.ContainsKey("Coin"))
                    {
                        gs.Add("Coin", Convert.ToInt32(msg["Coin"]));
                    }
                    else
                    {
                        gs.Add("Coin", 0);
                    }

                    if (msg.ContainsKey("Score"))
                    {
                        gs.Add("Score", Convert.ToInt32(msg["Score"]));
                    }
                    else
                    {
                        gs.Add("Score", 0);
                    }
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

        /// <summary>
        /// 发送系统邮件
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="sendName">发件人名称</param>
        /// <param name="playerid">接收人ID</param>
        /// <param name="name">接收人名称</param>
        /// <param name="content">内容</param>
        /// <param name="dic">内容替换信息</param>
        /// <param name="tmp">道具对象</param>
        /// <returns>true表示成功</returns>
        public bool SendEmail(string title, string sendName, string playerid, string name, string content, string playerName, IList tmp,int reTime)
        {

            Email email = new Email();
            email.ID = ObjectId.GenerateNewId().ToString();
            email.Name = title;
            email.Status = 0;
            email.Ver = 1;
            email.MainType = "System";
            email.Created = DateTime.UtcNow;

            string con = string.Format(content, playerName);

            Variant v = new Variant();
            v.Add("mailMess", con);
            v.Add("reTime", reTime);
            //v.Add("moneyGoods",new Variant());
            if (tmp != null)
            {
                v.Add("goodsList", tmp);
            }
            //v.Add("Content",d);
            email.Value = EmailAccess.Instance.CreateEmailValue("System", sendName, playerid, name, v);
            email.Save();
            return true;
        }      
    }
}
