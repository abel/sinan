using System;
using System.Configuration;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Data;
using Sinan.Extensions;
using Sinan.FastJson;
using Sinan.Log;
using Sinan.Security;
using Sinan.Security.Cryptography;
using Sinan.Util;

namespace Sinan.GameModule
{
    public class UserLogAccess
    {
        public static readonly char[] splitChars = new char[] { ' ', '-', '_', ',', '|', ';' };
        readonly static UserLogAccess m_instance = new UserLogAccess();

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static UserLogAccess Instance
        {
            get { return m_instance; }
        }

        MongoCollection<BsonDocument> m_collection;
        //MongoCollection<BsonDocument> m_playerColl;

        UserLogAccess()
        {
        }

        public void Connect(string connectionString)
        {
            MongoDatabase db = MongoDatabase.Create(connectionString);
            m_collection = db.GetCollection("UserLog");
            //m_playerColl = db.GetCollection("Player");
        }

        public LoginVerification Verification
        {
            get;
            set;
        }

        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="tokenStr"></param>
        /// <param name="fastLogin">是否启用快速登录</param>
        /// <returns></returns>
        public Variant LoginCheck(string tokenStr, bool fastLogin = false)
        {
            Variant token = null;
            string[] tokens = tokenStr.Split(splitChars);
            //趣游登录
            if (tokenStr.StartsWith("auth="))
            {
                int index = tokenStr.IndexOf('&');
                string authBase64 = tokenStr.Substring(5, index - 5);
                string sign = tokenStr.Substring(index + 6);
                token = Verification.DecryptGamewaveToken(authBase64, sign);
            }
            else
            {
                if (tokens.Length >= 3)
                {
                    token = Verification.DecryptTencentToken(tokens);
                }

                #region 快速登录,临时用,正式发布需删除
                else if (tokens.Length == 1)
                {
                    if (fastLogin)
                    {
                        if (tokenStr.Length < 64)
                        {
                            token = new Variant(2);
                            token["uid"] = tokenStr.TrimStart('0');
                            token["time"] = UtcTimeExtention.NowTotalSeconds();
                        }
                        else
                        {
                            //JSON方式解码(47baby登录)
                            token = Verification.DecryptFastToken(tokenStr);
                        }
                    }
                }
                #endregion
            }

            if (token == null)
            {
                return null;
            }

            string userID = token.GetStringOrDefault("uid") ?? token.GetStringOrEmpty("id");
            userID = userID.Trim().TrimStart('0');
            if (string.IsNullOrEmpty(userID))
            {
                return null;
            }
            token["uid"] = userID;
            return token;
        }

        FieldsBuilder fb = new FieldsBuilder().Include("IP", "Modified");
        public Variant WriteLog(string userID, string ip, int sid)
        {
            Variant result = new Variant(3);
            DateTime now = DateTime.UtcNow;
            var query = Query.EQ("_id", userID);
            var update = Update.Set("Modified", now).Inc("Ver", 1).Set("IP", ip);
            var pf = m_collection.FindAndModify(query, null, update, fb, false, true);
            if (pf != null && pf.Ok && pf.ModifiedDocument != null)
            {
                BsonDocument doc = pf.ModifiedDocument;
                if (doc.Contains("_id"))
                {
                    BsonValue bv;
                    if (doc.TryGetValue("IP", out bv))
                    {
                        result["IP"] = bv.ToString();
                    }
                    else
                    {
                        result["IP"] = ip;
                    }
                    if (doc.TryGetValue("Modified", out bv))
                    {
                        result["Modified"] = bv.AsDateTime;
                    }
                }
                else
                {
                    //写创建日期
                    m_collection.Update(query, Update.Set("Created", now).Set("SID", sid), SafeMode.True);
                    result["IP"] = ip;
                    result["Modified"] = now;
                }
            }
            return result;
        }

        /// <summary>
        /// 玩家退出.更新最后访问时间
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="online">在线时长</param>
        public void UserExit(string userID, int online)
        {
            var query = Query.EQ("_id", userID);
            //TotalOnlineTime.累计总在线时长
            var update = Update.Set("Modified", DateTime.UtcNow).Inc("TOT", online);
            m_collection.Update(query, update, UpdateFlags.None, SafeMode.False);
        }

        //static readonly FieldsBuilder fbSC = new FieldsBuilder().Include("Coin", "GCE", "GCI");
        ///// <summary>
        ///// 更新财务信息
        ///// </summary>
        ///// <param name="uid">用户ID</param>
        ///// <param name="coin">增加晶币</param>
        ///// <returns></returns>
        //public BsonDocument UpdateFinance(string uid, int coin)
        //{
        //    var query = Query.EQ("_id", uid);
        //    if (coin == 0)
        //    {
        //        //查找
        //        return m_collection.FindAs<BsonDocument>(query).SetFields(fbSC).FirstOrDefault();
        //    }
        //    var update = Update.Set("Modified", DateTime.UtcNow).Inc("Ver", 1).Inc("Coin", coin);
        //    if (coin < 0)
        //    {
        //        //支出
        //        update.Inc("GCE", (Int64)(-coin));
        //        query = Query.And(query, Query.GTE("Coin", -coin));
        //    }
        //    else
        //    {
        //        //收入
        //        update.Inc("GCI", (Int64)coin);
        //    }
        //    var pf = m_collection.FindAndModify(query, null, update, fbSC, true, false);
        //    if (pf != null && pf.Ok)
        //    {
        //        BsonDocument doc = pf.ModifiedDocument;
        //        if (doc != null)
        //        {
        //            var query2 = Query.EQ("UserID", uid);
        //            var update2 = Update.Set("Coin", doc["Coin"]);
        //            m_playerColl.Update(query2, update2, UpdateFlags.Multi, SafeMode.True);
        //        }
        //    }
        //    return null;
        //}

        //public long GetCoin(string uid)
        //{
        //    var query = Query.EQ("_id", uid);
        //    var doc = m_collection.FindAs<BsonDocument>(query).SetFields("Coin").FirstOrDefault();
        //    if (doc != null)
        //    {
        //        BsonValue v;
        //        if (doc.TryGetValue("Coin", out v))
        //        {
        //            return v.ToInt64();
        //        }
        //    }
        //    return 0;
        //}
    }
}
