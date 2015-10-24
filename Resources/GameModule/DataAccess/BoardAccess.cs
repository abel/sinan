using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
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
    /// <summary>
    /// 玩家公告板
    /// </summary>
    sealed public class BoardAccess : VariantBuilder
    {
        readonly static BoardAccess m_instance = new BoardAccess();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static BoardAccess Instance
        {
            get { return m_instance; }
        }

        BoardAccess()
            : base("Board")
        {
        }

        /// <summary>
        /// 添加公告
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddNote(int pid, string value)
        {
            var query = Query.EQ("_id", pid);
            var update = Update.Push("Values", value);
            var v = m_collection.Update(query, update, UpdateFlags.Upsert, SafeMode.False);
            return true;
        }

        /// <summary>
        /// 删除公告
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool RemoveNote(int pid, string value)
        {
            var query = Query.EQ("_id", pid);
            var update = Update.Pull("Values", value);
            var v = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
            return true;
        }

        /// <summary>
        /// 清空公告板
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public bool ClearNotes(int pid)
        {
            var query = Query.EQ("_id", pid);
            var update = Update.Set("Values", new BsonArray());
            var v = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
            return true;
        }


        /// <summary>
        /// 获取指定玩家的所有公告
        /// </summary
        /// <param name="pid"></param>
        /// <returns></returns>
        public Board FindHomeNote(int pid)
        {
            var query = Query.EQ("_id", pid);
            Board board = m_collection.FindOneAs<Board>(query);
            if (board != null && board.Values != null)
            {
                while (board.Values.Count > 60)
                {
                    var update = Update.PopFirst("Values");
                    m_collection.Update(query, update, UpdateFlags.None, SafeMode.False);
                    board.Values.RemoveAt(0);
                }
            }
            return board;
        }
    }
}
