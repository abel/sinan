using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Sinan.Data
{
    abstract public class VariantBuilder
    {
        public static MongoInsertOptions DefaultInsertOptions;
        static VariantBuilder()
        {
            DefaultInsertOptions = new MongoInsertOptions();
            DefaultInsertOptions.Flags = InsertFlags.ContinueOnError;
            DefaultInsertOptions.SafeMode = SafeMode.True;
        }

        protected readonly string m_collName;
        protected MongoCollection m_collection;

        /// <summary>
        /// 数据操作基类
        /// </summary>
        /// <param name="collectionName">数据集名</param>
        protected VariantBuilder(string collectionName)
        {
            m_collName = collectionName;
        }

        public virtual void Connect(string connectionString)
        {
            m_collection = MongoDatabase.Create(connectionString).GetCollection(m_collName);
        }

        /// <summary>
        /// 根据ID查询实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T FindOneById<T>(BsonValue id)
        {
            if (id.IsString && (string)id == null)
            {
                return default(T);
            }
            return m_collection.FindOneAs<T>(Query.EQ("_id", id));
        }

        /// <summary>
        /// 保存实体
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Save<T>(T entity, SafeMode safeMode)
        {
            var v = m_collection.Save<T>(entity, safeMode);
            return v == null ? false : v.DocumentsAffected > 0;
        }

        /// <summary>
        /// 根据ID删除实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RemoveOneById(BsonValue id, SafeMode safeMode)
        {
            if (id.IsString && (string)id == null)
            {
                return false;
            }
            var v = m_collection.Remove(Query.EQ("_id", id), RemoveFlags.Single, safeMode);
            return v == null ? false : v.DocumentsAffected > 0;
        }

        public long GetCount()
        {
            return m_collection.Count();
        }

        public long GetCount(IMongoQuery query)
        {
            return m_collection.Count(query);
        }


        /// <summary>
        /// 创建固定集合
        /// </summary>
        /// <param name="collName"></param>
        /// <param name="db"></param>
        public static MongoCollection CreateFixColl(string collName, MongoDatabase db)
        {
            const long size = 1024L * 1024 * 1024 * 4;
            CollectionOptionsBuilder options = new CollectionOptionsBuilder();
            options.SetCapped(true);
            options.SetMaxSize(size);
            db.CreateCollection(collName, options);
            MongoCollection collection = db.GetCollection(collName);
            // 固定集合创建索引
            collection.CreateIndex("_id");
            //m_collection.CreateIndex("opuid");
            //m_collection.CreateIndex("actionid");
            //m_collection.CreateIndex("time");
            return collection;
        }
    }

    public class VariantBuilder<T> : VariantBuilder
    {
        public VariantBuilder(string collectionName)
            : base(collectionName)
        { }

        /// <summary>
        /// 保存实体
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual bool Save(T entity)
        {
            var v = m_collection.Save<T>(entity, SafeMode.True);
            return v == null ? false : v.DocumentsAffected > 0;
        }

        /// <summary>
        /// 根据ID查询实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual T FindOneById(string id)
        {
            if (id == null)
            {
                return default(T);
            }
            return m_collection.FindOneAs<T>(Query.EQ("_id", id));
        }

        /// <summary>
        /// 根据ID删除实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RemoveOneById(string id, SafeMode safeMode)
        {
            if (id == null)
            {
                return false;
            }
            var v = m_collection.Remove(Query.EQ("_id", id), RemoveFlags.Single, safeMode);
            return v == null ? false : v.DocumentsAffected > 0;
        }


        /// <summary>
        /// 根据ID查询实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual T FindOneById(int id)
        {
            return m_collection.FindOneAs<T>(Query.EQ("_id", id));
        }

        /// <summary>
        /// 根据ID删除实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RemoveOneById(int id, SafeMode safeMode)
        {
            var v = m_collection.Remove(Query.EQ("_id", id), RemoveFlags.Single, safeMode);
            return v == null ? false : v.DocumentsAffected > 0;
        }

        public T[] FindAll()
        {
            return m_collection.FindAllAs<T>().ToArray();
        }

        public T[] FindAll(int count)
        {
            return m_collection.FindAllAs<T>().SetLimit(count).ToArray();
        }
    }

}

