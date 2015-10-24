using System;
using System.Configuration;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Entity;

namespace Sinan.GameModule
{
    sealed partial class PlayerAccess
    {
        static readonly FieldsBuilder fbSC = new FieldsBuilder().Include("Coin", "Score", "GCE", "GCI");

        /// <summary>
        /// 更新财务信息
        /// </summary>
        /// <param name="pid">玩家ID</param>
        /// <param name="coin">增加晶币</param>
        /// <returns></returns>
        public BsonDocument UpdateFinance(int pid, int coin)
        {
            var query = Query.EQ("_id", pid);
            var update = Update.Set("Modified", DateTime.UtcNow).Inc("Ver", 1);
            if (coin != 0)
            {
                update.Inc("Coin", coin);
                if (coin < 0)
                {
                    //支出
                    update.Inc("GCE", (Int64)(-coin));
                    query = Query.And(query, Query.GTE("Coin", -coin));
                }
                else
                {
                    //收入
                    update.Inc("GCI", (Int64)coin);
                }
            }
            var pf = m_collection.FindAndModify(query, null, update, fbSC, true, false);
            if (pf != null && pf.Ok)
            {
                return pf.ModifiedDocument;
            }
            return null;
        }

        /// <summary>
        /// 更新财务信息
        /// </summary>
        /// <param name="pid">玩家ID</param>
        /// <param name="fileds">要更新的字段</param>
        /// <param name="values">要添加的值</param>
        /// <returns></returns>
        public BsonDocument SafeUpdate(int pid, string[] fileds, Int32[] values)
        {
            if (fileds.Length != values.Length)
            {
                return null;
            }
            var query = Query.EQ("_id", pid);
            var update = MongoDB.Driver.Builders.Update.Set("Modified", DateTime.UtcNow).Inc("Ver", 1);
            for (int i = 0; i < fileds.Length; i++)
            {
                int value = values[i];
                if (value != 0)
                {
                    update.Inc(fileds[i], value);
                    if (value < 0)
                    {
                        query = Query.And(query, Query.GTE(fileds[i], -value));
                    }
                }
            }
            var pf = m_collection.FindAndModify(query, null, update, Fields.Include(fileds), true, false);
            if (pf != null && pf.Ok)
            {
                return pf.ModifiedDocument;
            }
            return null;
        }


        /// <summary>
        /// 更新值
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="filed"></param>
        /// <param name="inc"></param>
        /// <param name="check">不能扣负为true,可以扣负为false</param>
        /// <returns></returns>
        public BsonDocument SafeUpdate(int pid, string filed, int inc, bool check = true)
        {
            if (inc != 0)
            {
                var query = Query.EQ("_id", pid);
                var update = MongoDB.Driver.Builders.Update.Set("Modified", DateTime.UtcNow).Inc("Ver", 1);
                update.Inc(filed, inc);
                if (check && inc < 0)
                {
                    query = Query.And(query, Query.GTE(filed, -inc));
                }
                FieldsBuilder fb = new FieldsBuilder().Include(filed);
                var pf = m_collection.FindAndModify(query, null, update, fb, true, false);
                if (pf != null && pf.Ok)
                {
                    return pf.ModifiedDocument;
                }
            }
            return null;
        }
    }
}
