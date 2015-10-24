using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;

namespace Sinan.GameModule
{
    /// <summary>
    /// 名字服务
    /// </summary>
    public class WordAccess : VariantBuilder<Word>
    {
        static WordAccess m_instance = new WordAccess();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static WordAccess Instance
        {
            get { return m_instance; }
        }

        WordAccess()
            : base("Word")
        {
        }

        SortByBuilder desState = SortBy.Descending("State");
        /// <summary>
        /// 自动命名
        /// </summary>
        /// <returns></returns>
        public string AutomaticName()
        {
            try
            {
                int index = NumberRandom.Next(1, Word.AutoCount);
                var query = Query.And(Query.GTE("State", index), Query.LT("State", Word.AutoCount));
                Word word = m_collection.FindAs<Word>(query).SetLimit(1).FirstOrDefault();
                if (word == null)
                {
                    var query2 = Query.And(Query.GT("State", 0), Query.LT("State", index));
                    word = m_collection.FindAs<Word>(query2).SetSortOrder(desState).SetLimit(1).FirstOrDefault();
                }
                return word == null ? string.Empty : word.Key;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 设置为已使用
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public void SetUsed(string key)
        {
            try
            {
                var query = Query.And(Query.EQ("_id", key), Query.LT("State", Word.AutoCount));
                var update = Update.Set("State", Word.Used);
                m_collection.Update(query, update, UpdateFlags.Upsert, SafeMode.False);
            }
            catch { }
        }

        /// <summary>
        /// 将名字设置为已删除状态
        /// </summary>
        /// <param name="key"></param>
        public void SetDeleted(string key)
        {
            var query = Query.And(Query.EQ("_id", key), Query.EQ("State", Word.Used));
            var update = Update.Set("State", Word.Deleted);
            m_collection.Update(query, update, UpdateFlags.Upsert, SafeMode.False);
        }

        /// <summary>
        /// 获取状态..
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetState(string name)
        {
            Word word = m_collection.FindOneByIdAs<Word>(name);
            return word == null ? 0 : word.State;
        }

        public Word[] AllWord(int state)
        {
            var query = Query.EQ("State", state);
            return m_collection.FindAs<Word>(query).ToArray();
        }
    }
}
