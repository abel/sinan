using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Log;
using Sinan.Extensions;

namespace Sinan.GameModule
{
    /// <summary>
    /// 购买物品订单
    /// </summary>
    public partial class OrderAccess
    {
        const string m_collName = "Order";
        readonly static OrderAccess m_instance = new OrderAccess();

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static OrderAccess Instance
        {
            get { return m_instance; }
        }

        protected MongoCollection m_collection;
        OrderAccess() { }

        /// <summary>
        /// 数据操作基类
        /// </summary>
        /// <param name="connectionString">服务器连接字符串</param>
        /// <param name="collectionName">数据集名</param>
        public void Connect(string connectionString)
        {
            m_collection = MongoDatabase.Create(connectionString).GetCollection(m_collName);
        }

        /// <summary>
        /// 插入新订单
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public bool Insert(Order log)
        {
            try
            {
                var v = m_collection.Insert(log, SafeMode.True);
                return v == null ? false : v.Ok;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 保存订单
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public bool Save(Order log)
        {
            var query = Query.And(Query.EQ("_id", log.token), Query.EQ("state", log.state));
            try
            {
                var update = Update.Set("amt", log.amt)
                    .Set("billno", log.billno)
                    .Set("Coin", log.Coin)
                    .Set("Created", log.Created)
                    .Set("openid", log.openid)
                    .Set("payitem", log.payitem)
                    .Set("pid", log.pid)
                    .Set("ppc", log.ppc)
                    .Set("providetype", log.providetype)
                    .Set("sig", log.sig)
                    .Set("ts", log.ts)
                    .Set("zoneid", log.zoneid)
                    .Set("Ver", log.Ver)
                    .Set("money", log.money)
                    .Set("Url", log.Url);
                var v = m_collection.Update(query, update, UpdateFlags.Upsert, SafeMode.True);
                return v == null ? false : v.DocumentsAffected > 0;
            }
            catch
            {
                return m_collection.Count(query) == 1;
            }
        }

        public bool Exists(string token)
        {
            return m_collection.Count(Query.EQ("_id", token)) > 0;
        }

        int GetPlayerID(string tokenID)
        {
            var query = Query.EQ("_id", tokenID);
            var update = Update.Inc("Ver", 1);
            FieldsBuilder fb = new FieldsBuilder().Include("pid");
            var pf = m_collection.FindAndModify(query, null, update, fb, true, false);
            if (pf != null && pf.Ok && pf.ModifiedDocument != null)
            {
                BsonValue vaule;
                if (pf.ModifiedDocument.TryGetValue("pid", out vaule))
                {
                    return (int)vaule;
                }
            }
            return 0;
        }

        /// <summary>
        /// 补全Order信息
        /// (状态为0时才可以补全)
        /// </summary>
        /// <param name="log"></param>
        /// <param name="newState">补全后状态设置新值,默认为11</param>
        /// <returns>角色ID</returns>
        public int Replenish(Order log, int newState)
        {
            var query = Query.And(Query.EQ("_id", log.token), Query.EQ("state", 0));
            var update = Update.Set("amt", log.amt).Set("billno", log.billno)
                  .Set("openid", log.openid).Set("payitem", log.payitem).Set("ppc", log.ppc)
                  .Set("providetype", log.providetype).Set("sig", log.sig).Set("ts", log.ts)
                  .Set("zoneid", log.zoneid).Inc("state", newState);

            FieldsBuilder fb = new FieldsBuilder().Include("pid");
            var pf = m_collection.FindAndModify(query, null, update, fb, true, false);
            if (pf != null && pf.Ok && pf.ModifiedDocument != null)
            {
                BsonValue vaule;
                if (pf.ModifiedDocument.TryGetValue("pid", out vaule))
                {
                    return (Int32)vaule;
                }
            }
            //记录已补全.直接获取
            return GetPlayerID(log.token);
        }

        /// <summary>
        /// 更改Order状态
        /// </summary>
        /// <param name="tokenID"></param>
        /// <param name="oldState"></param>
        /// <returns></returns>
        public bool IncOrderState(string tokenID, int oldState, int step)
        {
            var query = Query.And(Query.EQ("_id", tokenID), Query.EQ("state", oldState));
            string key = "M" + oldState.ToString() + "T" + (oldState + step).ToString();
            var update = Update.Inc("state", step).Set(key, DateTime.UtcNow).Inc("Ver", 1);
            var v = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
            return v == null ? false : v.DocumentsAffected > 0;
        }

        /// <summary>
        /// 更改Order状态
        /// </summary>
        /// <param name="tokenID"></param>
        /// <param name="oldState"></param>
        /// <returns></returns>
        public bool IncOrderState(string tokenID, int oldState, int step, int coin)
        {
            var query = Query.And(Query.EQ("_id", tokenID), Query.EQ("state", oldState));
            string key = "M" + oldState.ToString() + "T" + (oldState + step).ToString();
            var update = Update.Inc("state", step).Set(key, DateTime.UtcNow).Inc("Ver", 1).Set("Coin", coin);
            var v = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
            return v == null ? false : v.DocumentsAffected > 0;
        }

        /// <summary>
        /// 取没有发送或发送失败的数据
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        internal List<Order> GetLost(DateTime time, int state)
        {
            var query = Query.And(Query.LT("Created", time), Query.EQ("state", state));
            return m_collection.FindAs<Order>(query).ToList();
        }

        static MongoInsertOptions DefaultInsertOptions;
        static OrderAccess()
        {
            DefaultInsertOptions = new MongoInsertOptions();
            DefaultInsertOptions.Flags = InsertFlags.None;
            DefaultInsertOptions.SafeMode = SafeMode.True;
        }

        public bool NewOrder(Order order)
        {
            try
            {
                var result = m_collection.Insert<Order>(order, DefaultInsertOptions);
                return result != null && result.Ok;
            }
            catch
            {
                return false;
            }
        }
    }
}
