using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Extensions;

namespace Sinan.BabyOPD
{
    public partial class PlatformTotalAccess : PlatformBase
    {
        MongoDatabase m_gameLog;
        MongoDatabase m_playerDb;
        public PlatformTotalAccess(string playerDB, string gameLog, string operationString)
            : base(operationString)
        {
            m_playerDb = MongoDatabase.Create(playerDB);
            m_gameLog = MongoDatabase.Create(gameLog);
        }

        /// <summary>
        /// 统计每日新用户数据
        /// (新增账号数;新增角色数; 新增角色2次登录数; 新角色登录总次数)
        /// </summary>
        /// <param name="endTime">开始时间</param>
        /// <returns></returns>
        public Dictionary<string, BsonValue> NewUserInfo(DateTime endTime)
        {
            MongoCollection<BsonDocument> playerColl = m_playerDb.GetCollection("Player");

            var query = Query.And(Query.GTE("Created", endTime.AddDays(-1)), Query.LT("Created", endTime));
            var result = playerColl.Find(query).SetFields("UserID");

            int newPlayer = 0;
            int newUser = 0;
            HashSet<string> players = new HashSet<string>();
            HashSet<string> users = new HashSet<string>();
            foreach (BsonDocument v in result)
            {
                players.Add(v["_id"].ToString());
                if (users.Add(v["UserID"].ToString()))
                {
                    newUser++;
                }
                newPlayer++;
            }
            int newSecondLogin = 0;
            int newLogin = 0;
            MongoCollection<BsonDocument> loginLog = m_gameLog.GetCollection("LoginLog");
            foreach (var playerID in players)
            {
                query = Query.And(Query.GTE("Created", endTime.AddDays(-1)), Query.LT("Created", endTime), Query.EQ("PID", playerID));
                int count = loginLog.Count(query);
                newLogin += count;
                if (count > 1)
                {
                    newSecondLogin++;
                }
            }
            Dictionary<string, BsonValue> dic = new Dictionary<string, BsonValue>();
            dic.Add("NewPlayer", newPlayer);
            dic.Add("NewUser", newUser);
            dic.Add("NewSecondLogin", newSecondLogin);
            dic.Add("NewLogin", newLogin);
            return dic;
        }

        public Dictionary<string, BsonValue> PlayerLogInfo(DateTime endTime)
        {
            MongoCollection<BsonDocument> loginLog = m_gameLog.GetCollection("LoginLog");
            DateTime start = endTime.AddDays(-1);
            var query1 = Query.And(Query.GTE("Created", start), Query.LT("Created", endTime));
            var query2 = Query.And(Query.GTE("OutTime", start), Query.LT("OutTime", endTime));
            var query = Query.Or(query1, query2);

            //登录IP数
            var ipNum = loginLog.Distinct("IP", query).Count();
            //登录角色数
            var playerNum = loginLog.Distinct("PID", query).Count();
            //登录帐号数
            var userNum = loginLog.Distinct("UID", query).Count();
            //帐号总数

            Dictionary<string, BsonValue> dic = new Dictionary<string, BsonValue>();
            dic.Add("LoginIP", ipNum);
            dic.Add("LoginPlayer", playerNum);
            dic.Add("LoginUser", userNum);
            dic.Add("ActiveUser", userNum);
            return dic;
        }

        /// <summary>
        /// 计算双周留存率
        /// 定义：原登录用户占当前登录用户的比率，以14天为单位进行统计。
        /// 双周用户留存率=上周登录游戏且本周也登录游戏的帐号数量/第14天帐号总数
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        public double DoubleWeekRetained(DateTime endTime)
        {
            //取上周登录游戏的帐号
            HashSet<string> user1 = GetLoginUsers(endTime.AddDays(-14), endTime.AddDays(-7));

            //取本周登录游戏的帐号
            HashSet<string> user2 = GetLoginUsers(endTime.AddDays(-7), endTime);

            //做交集
            double count = 0;
            foreach (string uid in user1)
            {
                if (user2.Contains(uid))
                {
                    count++;
                }
            }
            //取帐号总数
            MongoCollection<BsonDocument> userLog = m_playerDb.GetCollection("UserLog");
            var query1 = Query.LT("Created", endTime);
            int userCount = userLog.Count(query1);

            return count / (userCount == 0 ? 1 : userCount);
        }

        /// <summary>
        /// 取指定时间段登录过游戏的用户ID
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        HashSet<string> GetLoginUsers(DateTime start, DateTime end)
        {
            var query1 = Query.And(Query.GTE("Created", start), Query.LT("Created", end));
            var query2 = Query.And(Query.GTE("OutTime", start), Query.LT("OutTime", end));
            var query = Query.Or(query1, query2);
            HashSet<string> users = new HashSet<string>();
            MongoCollection<BsonDocument> log = m_gameLog.GetCollection("LoginLog");
            var xx = log.Distinct("UID", query);
            foreach (var v in xx)
            {
                users.Add(v.ToString());
            }
            return users;
        }

        /// <summary>
        /// 活跃用户数量
        /// 取指定时间内登录过游戏的用户数
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        int GetActiveUserCount(DateTime start, DateTime end)
        {
            var query1 = Query.And(Query.GTE("Created", start), Query.LT("Created", end));
            var query2 = Query.And(Query.GTE("OutTime", start), Query.LT("OutTime", end));
            var query = Query.Or(query1, query2);
            HashSet<string> users = new HashSet<string>();
            MongoCollection<BsonDocument> log = m_gameLog.GetCollection("LoginLog");
            int count = log.Distinct("UID", query).Count();
            return count;
        }

        /// <summary>
        /// 周流失用户数量
        /// 上周登录但本周没有登录的用户数量
        /// </summary>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public int LoseUserCount(DateTime endTime)
        {
            //取上周登录游戏的帐号
            HashSet<string> user1 = GetLoginUsers(endTime.AddDays(-14), endTime.AddDays(-7));
            //取本周登录游戏的帐号
            HashSet<string> user2 = GetLoginUsers(endTime.AddDays(-7), endTime);
            //去交集
            int count = 0;
            foreach (string uid in user1)
            {
                if (!user2.Contains(uid))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 流失率(周)
        /// 流失用户/上周活跃数量
        /// </summary>
        /// <returns></returns>
        public double LiuShiLv(DateTime endTime)
        {
            double loser = LoseUserCount(endTime);
            int frontWeek = GetActiveUserCount(endTime.AddDays(-14), endTime.AddDays(-7));
            return loser / (frontWeek == 0 ? 1 : frontWeek);
        }

        public Dictionary<string, BsonValue> WeekInfo(DateTime endTime)
        {
            string id = endTime.ToString("yyyyMMdd");
            //双周留存率
            double dwRetaine = DoubleWeekRetained(endTime);
            //周留失率
            double wLose = LiuShiLv(endTime);

            Dictionary<string, BsonValue> data = new Dictionary<string, BsonValue>();
            data.Add("DoubleWeekRetained", dwRetaine);
            data.Add("LiuShiLv", wLose);
            return data;
        }


        /// <summary>
        /// 流失率
        /// </summary>
        /// <returns>键:天数.值:人数</returns>
        public Dictionary<int, int> LiuShiLVLog(DateTime end)
        {
            //取当天所有登录过服务器的用户..
            HashSet<string> users = GetLoginUsers(end.AddDays(-1), end);

            //取一周之内创建的用户
            var query = Query.And(Query.GTE("Created", end.AddDays(-8)), Query.LT("Created", end));
            MongoCollection<BsonDocument> userLog = m_playerDb.GetCollection("UserLog");
            var xx = userLog.Find(query).SetFields("Created");
            Dictionary<int, int> days = new Dictionary<int, int>();
            foreach (var v in xx)
            {
                if (users.Contains(v["_id"].ToString()))
                {
                    DateTime t = (DateTime)(v["Created"]);
                    int day = (end.Date - t.Date).Days - 1;
                    int count;
                    days.TryGetValue(day, out count);
                    days[day] = count + 1;
                }
            }
            return days;
        }

        public Dictionary<int, int> NewUsers()
        {
            //取每一天的新用户数
            MongoCollection<BsonDocument> pl = m_operation.GetCollection("PlatformTotal");
            var xx = pl.Find(Query.Null).SetFields("NewUser");
            Dictionary<int, int> days = new Dictionary<int, int>();
            foreach (var v in xx)
            {
                if (v.Contains("NewUser"))
                {
                    days[Convert.ToInt32(v["_id"])] = Convert.ToInt32(v["NewUser"]);
                }
                else
                {
                    days[Convert.ToInt32(v["_id"])] = 0;
                }
            }
            return days;
        }

        //public Dictionary<string, DateTime> FillOld(DateTime end)
        //{      
        //    //取一周之内创建的用户
        //    MongoCollection<BsonDocument> player = m_gameLog.GetCollection("Player");
        //    var xx = player.Find(Query.Null).SetFields("UserID","Created");
        //    Dictionary<string, DateTime> user = new Dictionary<string, DateTime>();
        //    foreach (var v in xx)
        //    {
        //        if()
        //    }
        //}
    }
}
