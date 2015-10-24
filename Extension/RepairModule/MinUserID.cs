using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.GameModule;
using Sinan.Log;

namespace Sinan.RepairModule
{
    /// <summary>
    /// 去掉UserID前面的0
    /// </summary>
    class MinUserID
    {

        MongoDatabase database;


        public MinUserID()
        {
        }


        /// <summary>
        /// 清理用户名
        /// </summary>
        /// <param name="note"></param>
        public void StartPlace()
        {
            string connectionString = ConfigLoader.Config.DbPlayer;
            database = MongoDatabase.Create(connectionString);

            MongoCollection player = database.GetCollection("Player");
            int count = NewMethod(player, "UserID");
            Console.WriteLine("Player:" + count);

            MongoCollection user = database.GetCollection("UserLog");
            count = NewMethod2(user);
            Console.WriteLine("UserLog" + count);
        }

        private int NewMethod2(MongoCollection coll)
        {
            var v = coll.FindAs<BsonDocument>(Query.Null);
            Dictionary<string, string> results = new Dictionary<string, string>();
            List<BsonDocument> list = new List<BsonDocument>();
            List<string> oldID = new List<string>();
            foreach (var item in v)
            {
                BsonValue value;
                if (item.TryGetValue("_id", out value))
                {
                    string oldName = value.ToString();
                    string newName = ReplayceName(oldName);
                    if (oldName != newName)
                    {
                        oldID.Add(oldName);
                        item["_id"] = newName;
                        list.Add(item);
                    }
                }
            }
            foreach (var item in list)
            {
                coll.Save(item);
            }

            foreach (var item in oldID)
            {
                coll.Remove(Query.EQ("_id", item));
            }
            return results.Count;
        }

        private int NewMethod(MongoCollection coll, string fbname)
        {
            var v = coll.FindAs<BsonDocument>(Query.Null).SetFields(fbname);
            Dictionary<BsonValue, string> results = new Dictionary<BsonValue, string>();
            foreach (var item in v)
            {
                BsonValue value;
                if (item.TryGetValue(fbname, out value))
                {
                    string oldName = value.ToString();
                    string newName = ReplayceName(oldName);
                    if (oldName != newName)
                    {
                        item.TryGetValue("_id", out value);
                        results.Add(value, newName);
                    }
                }
            }
            foreach (var item in results)
            {
                BsonValue id = item.Key;
                string newName = item.Value;
                try
                {
                    Query.EQ("_id", id);
                    Update.Set(fbname, newName);
                    coll.Update(Query.EQ("_id", id), Update.Set(fbname, newName), SafeMode.False);
                }
                catch (System.Exception ex)
                {
                    LogWrapper.Error("renameErr:" + id + " newName" + newName, ex);
                }
            }
            return results.Count;
        }

        private string ReplayceName(string oldName)
        {
            return oldName.TrimStart('0');
        }
    }
}
