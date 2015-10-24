using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Entity;
using MongoDB.Driver;
using Sinan.GameModule;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Sinan.Util;

namespace Sinan.GameModule
{
    public class PlayerSortAccess
    {
        static readonly PlayerSortAccess m_instance = new PlayerSortAccess();

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static PlayerSortAccess Instance
        {
            get { return m_instance; }
        }

        private MongoDatabase m_db;
        private string m_connectionString;

        /// <summary>
        /// 开服时间
        /// </summary>
        private DateTime m_zoneEpoch;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="zoneEpoch">开服时间</param>
        public void Connect(string connectionString, DateTime zoneEpoch)
        {
            m_zoneEpoch = zoneEpoch;
            m_db = MongoDatabase.Create(connectionString);
            m_connectionString = connectionString;
        }

        /// <summary>
        /// 得到排名
        /// </summary>
        /// <param name="player">角色</param>
        /// <param name="tableName">表名</param>
        /// <returns>排名</returns>
        public int GetMyRank(Player player, string tableName)
        {
            if (!m_db.CollectionExists(tableName))
            {
                return -1;
            }

            MongoCollection<BsonDocument> mc = m_db.GetCollection(tableName);
            if (player == null)
            {
                return -1;
            }

            //表示用户不存在
            var q = Query.EQ("_id", player.PID);
            //表示没有上排行
            if (mc.Count(q) <= 0)
            {
                return -2;
            }

            var result = mc.FindOne(q);

            BsonValue r;
            if (result.TryGetValue("Rank", out r))
            {
                return Convert.ToInt32(r);
            }


            int level = 0;
            BsonValue o;
            if (result.TryGetValue("Level", out o))
            {
                level = Convert.ToInt32(o);
            }


            int sn = 0;
            BsonValue s;
            if (result.TryGetValue("S", out s))
            {
                sn = Convert.ToInt32(s);
            }

            var query = Query.Or(Query.GT("Level", level), Query.And(Query.EQ("Level", level), Query.LT("S", sn)));
            return mc.Count(query) + 1;
        }

        /// <summary>
        /// 判断奖励是否存在
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool IsAward(Player player, string tableName, string msg)
        {
            if (!m_db.CollectionExists(tableName))
                return true;
            MongoCollection<BsonDocument> mc = m_db.GetCollection(tableName);
            if (player == null)
                return true;

            var query = Query.And(Query.EQ("_id", player.PID), Query.Exists("Value." + msg));
            return mc.Count(query) > 0;
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="player"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool UpdateInfo(Player player, string tableName, string msg)
        {
            if (!m_db.CollectionExists(tableName))
                return false;
            MongoCollection<BsonDocument> mc = m_db.GetCollection(tableName);
            if (player == null)
                return false;
            var query = Query.EQ("_id", player.PID);
            var update = Update.Set("Value." + msg, DateTime.UtcNow);
            var v = mc.Update(query, update, UpdateFlags.None, SafeMode.True);
            return v == null ? false : v.DocumentsAffected > 0;
        }

        /// <summary>
        /// 得到老服用户最大等级
        /// </summary>
        /// <param name="userid">用户ID</param>
        /// <returns></returns>
        public int PlayerOld(string userid, string tableName, out int pid)
        {
            if (!m_db.CollectionExists(tableName))
            {
                pid = 0;
                return 0;
            }
            MongoCollection<BsonDocument> mc = m_db.GetCollection(tableName);
            if (mc == null)
            {
                pid = 0;
                return 0;
            }

            var res = Query.And(Query.EQ("UserID", userid), Query.Exists("Value.OldAward"));
            if (mc.Count(res) > 0)
            {
                pid = 0;
                //表示已经领取该奖励
                return -1;
            }

            var query = Query.EQ("UserID", userid);
            SortByBuilder sb = SortBy.Descending("Level");
            var result = mc.Find(query).SetSortOrder(sb).SetLimit(1);
            foreach (var item in result)
            {
                BsonValue o;
                if (item.TryGetValue("_id", out o))
                {
                    pid = Convert.ToInt32(o);
                }
                else
                {
                    pid = 0;
                }

                BsonValue value;
                if (item.TryGetValue("Level", out value))
                {
                    return Convert.ToInt32(value.ToString());
                }
            }
            pid = 0;
            return 0;
        }

        /// <summary>
        /// 更新老服用户信息
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public bool UpdateOld(int pid, string tableName)
        {
            if (!m_db.CollectionExists(tableName))
                return false;
            MongoCollection<BsonDocument> mc = m_db.GetCollection(tableName);

            var query = Query.EQ("_id", pid);
            var update = Update.Set("Value.OldAward", DateTime.UtcNow);
            var v = mc.Update(query, update, UpdateFlags.None, SafeMode.True);
            return v == null ? false : v.DocumentsAffected > 0;
        }

        /// <summary>
        /// 得到月周日排名时间
        /// </summary>
        /// <param name="awardType">奖励类型</param>
        /// <returns></returns>
        public string GetTableName(string awardType)
        {
            DateTime now = DateTime.Now;
            if (awardType == "WeekAward")
            {
                //int week = (int)now.DayOfWeek;             //以周六23:59:59做为截止时间
                ////int week = (((int)now.DayOfWeek + 6) % 7); //以周日23:59:59做为截止时间
                //return now.AddDays(-week).ToString("yyyyMMdd");
                int weekEpoch = (int)m_zoneEpoch.DayOfWeek;
                int weekCur = (int)now.DayOfWeek;
                if (weekEpoch <= weekCur)
                {
                    return now.AddDays(-(weekCur - weekEpoch)).ToString("yyyyMMdd");
                }
                else
                {
                    return now.AddDays(-(weekCur + 7 - weekEpoch)).ToString("yyyyMMdd");
                }
            }

            if (awardType == "MonthAward")
            {
                //return now.ToString("yyyyMM") + "01";
                DateTime t = GetMonthTime(now, m_zoneEpoch.Day);
                return t.ToString("yyyyMMdd");
            }
            //"DayAward"
            return now.ToString("yyyyMMdd");
        }

        static DateTime GetMonthTime(DateTime time, int dayEpoch)
        {
            int dayCur = time.Day;
            if (dayEpoch <= dayCur)
            {
                return new DateTime(time.Year, time.Month, dayEpoch);
            }
            else
            {
                //检查是否是本月最后一天.
                if (time.AddDays(1).Day == 1)
                {
                    return time.Date;
                }
                else
                {
                    //取上个月的最后一天
                    time = time.AddDays(-dayCur);
                    if (time.Day <= dayEpoch)
                    {
                        return time.Date;
                    }
                    return new DateTime(time.Year, time.Month, dayEpoch);
                }
            }
        }

        /// <summary>
        /// 保留的排名
        /// </summary>
        /// <returns></returns>
        private List<string> SaveTable()
        {
            List<string> list = new List<string>();
            list.Add(GetTableName("DayAward"));
            list.Add(GetTableName("WeekAward"));
            list.Add(GetTableName("MonthAward"));
            return list;
        }

        /// <summary>
        /// 取得领奖情况信息
        /// </summary>
        /// <returns></returns>
        public Variant GetAwardMsg(Player player)
        {
            Variant v = new Variant();
            for (int i = 0; i < 3; i++)
            {
                string str = "";
                if (i == 0)
                {
                    str = "MonthAward";
                }
                else if (i == 1)
                {
                    str = "WeekAward";
                }
                else
                {
                    str = "DayAward";
                }

                string tableName = "Player" + GetTableName(str);
                if (IsAward(player, tableName, str))
                {
                    v[str] = -1;//表示已经领取
                }
                else
                {
                    //得到排名
                    v[str] = GetMyRank(player, tableName);
                }
            }
            return v;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="checkExists">为真时,如果表已复制,则直接返回</param>
        public void PlayerDump(string day, bool checkExists)
        {
            string dumpname = "Player" + day;
            MongoCollection dumplog = m_db.GetCollection("DumpLog");
            var dumpQ = Query.EQ("_id", dumpname);
            if (checkExists && m_db.CollectionExists(dumpname))
            {
                BsonDocument doc = dumplog.FindOneAs<BsonDocument>(dumpQ);
                if (doc == null || doc.Contains("End"))
                {
                    TopAccess.Instance.CreatePlayerTopn(day, 100, checkExists);
                    TopAccess.Instance.CreatePetTopn(day, 100, checkExists);
                    return;
                }
                m_db.RenameCollection(dumpname, dumpname + DateTime.UtcNow.Ticks);
            }
            var up = Update.Set("Start", DateTime.UtcNow);
            dumplog.Update(dumpQ, up, UpdateFlags.Upsert, SafeMode.True);

            MongoCollection dumpColl = m_db.GetCollection(dumpname);
            MongoCollection coll = m_db.GetCollection("Player");
            //var query = Query.Or(Query.GTE("Level", 20), Query.GTE("Coin", 0));
            SortByBuilder sbb = SortBy.Descending("Level").Ascending("S");
            //var v = coll.FindAs<BsonDocument>(query).SetSortOrder(sbb);
            var v = coll.FindAllAs<BsonDocument>().SetSortOrder(sbb);
            int count = 0;
            foreach (var item in v)
            {
                BsonValue value;
                if ((!item.TryGetValue("Value", out value))
                    || value.BsonType != BsonType.Document)
                {
                    item["Value"] = new BsonDocument();
                }
                item["Rank"] = ++count;
                dumpColl.Insert(item);
            }
            up = Update.Set("End", DateTime.UtcNow).Set("Count", count);
            dumplog.Update(dumpQ, up, UpdateFlags.Upsert, SafeMode.True);

            TopAccess.Instance.CreatePlayerTopn(day, 100, checkExists);
            TopAccess.Instance.CreatePetTopn(day, 100, checkExists);
            PlayerRemove();
        }

        /// <summary>
        /// 移除不需要的排名
        /// </summary>
        public void PlayerRemove()
        {
            List<string> list = SaveTable();
            DateTime dt = DateTime.UtcNow.ToLocalTime();
            for (int i = 45; i > 35; i--)
            {
                string str = dt.AddDays(-i).ToString("yyyyMMdd");
                if (list.Contains(str))
                    continue;

                string tableName = "Player" + str;
                if (!m_db.CollectionExists(tableName))
                    continue;
                m_db.DropCollection(tableName);
            }
        }

    }
}
