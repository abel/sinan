using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Sinan.BabyOPD
{
    public class PlatformBase
    {
        protected MongoDatabase m_operation;

        protected PlatformBase(string operationString)
        {
            m_operation = MongoDatabase.Create(operationString);
        }

        /// <summary>
        /// 统计
        /// </summary>
        /// <param name="id">唯一标识</param>
        /// <param name="data">内容</param>
        /// <returns></returns>
        public bool Save(string id, Dictionary<string, BsonValue> data)
        {
            MongoCollection<BsonDocument> playerColl = m_operation.GetCollection("PlatformTotal");
            var query = Query.EQ("_id", id);
            var update = Update.Set("Modified", DateTime.UtcNow).Inc("Ver", 1);
            foreach (var item in data)
            {
                update.Set(item.Key, item.Value);
            }
            var v = playerColl.Update(query, update, UpdateFlags.Upsert, SafeMode.True);
            return v == null ? false : v.DocumentsAffected > 0;
        }

        public bool RemoveByID(string id)
        {
            MongoCollection<BsonDocument> playerColl = m_operation.GetCollection("PlatformTotal");
            var query = Query.EQ("_id", id);
            var v = playerColl.Remove(query, SafeMode.True);
            return v == null ? false : v.DocumentsAffected > 0;
        }
    }
}
