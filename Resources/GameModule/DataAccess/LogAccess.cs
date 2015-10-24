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
    /// 集合日志
    /// </summary>
    public class LogAccess
    {
        readonly static LogAccess m_instance = new LogAccess();

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static LogAccess Instance
        {
            get { return m_instance; }
        }

        protected MongoCollection m_collection;
        LogAccess() { }

        /// <summary>
        /// 数据操作基类
        /// </summary>
        /// <param name="connectionString">服务器连接字符串</param>
        public void Connect(string connectionString, string collName)
        {
            MongoDatabase db = MongoDatabase.Create(connectionString);
            m_collection = db.GetCollection(collName);
            var x = MongoServer.Create();
        }

        MongoInsertOptions options = new MongoInsertOptions
        {
            SafeMode = SafeMode.True,
            CheckElementNames = false
        };

        public bool Insert(LogBase log)
        {
            m_collection.Insert(log, options);
            return true;
        }

        MongoInsertOptions options2 = new MongoInsertOptions
        {
            SafeMode = SafeMode.True,
            CheckElementNames = false,
            Flags = InsertFlags.ContinueOnError,
        };

        /// <summary>
        /// 批量写入日志
        /// </summary>
        /// <param name="logs"></param>
        /// <returns></returns>
        public bool InsertBatch(IEnumerable<LogBase> logs)
        {
            m_collection.InsertBatch(logs, options2);
            return true;
        }

        public T FindOneById<T>(ObjectId id)
        {
            return m_collection.FindOneByIdAs<T>(id);
        }
    }
}
