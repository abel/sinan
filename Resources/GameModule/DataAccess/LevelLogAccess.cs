using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Log;

namespace Sinan.GameModule
{
    /// <summary>
    /// 等级日志
    /// </summary>
    public class LevelLogAccess
    {
        readonly static LevelLogAccess m_instance = new LevelLogAccess();

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static LevelLogAccess Instance
        {
            get { return m_instance; }
        }

        protected MongoCollection m_collection;
        LevelLogAccess() { }

        /// <summary>
        /// 数据操作基类
        /// </summary>
        /// <param name="connectionString">服务器连接字符串</param>
        public void Connect(string connectionString)
        {
            MongoDatabase db = MongoDatabase.Create(connectionString);
            m_collection = db.GetCollection("LevelLog");
            var x = MongoServer.Create();
        }

        MongoInsertOptions options = new MongoInsertOptions
        {
            SafeMode = SafeMode.True,
            CheckElementNames = false
        };

        public bool Insert(BsonValue id, int lev)
        {
            var query = Query.EQ("_id", id);
            DateTime now = DateTime.UtcNow;
            var update = Update.Set(lev.ToString(), now).Set("Modified", now).Set("Level", lev);
            m_collection.Update(query, update, UpdateFlags.Upsert);
            return true;
        }

        public bool Insert(BsonValue id, int lev, string uid)
        {
            var query = Query.EQ("_id", id);
            DateTime now = DateTime.UtcNow;
            var update = Update.Set(lev.ToString(), now).Set("Modified", now).Set("Level", lev).Set("UserID", uid);
            m_collection.Update(query, update, UpdateFlags.Upsert);
            return true;
        }
    }
}