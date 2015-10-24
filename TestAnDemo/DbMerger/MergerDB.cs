using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace DbMerger
{
    /// <summary>
    /// 合并数据库
    /// </summary>
    class MergerDB
    {
        static public int CoverDB()
        {
            string oldDBStr = ConfigurationManager.AppSettings["oldplayer"];
            string newDBStr = ConfigurationManager.AppSettings["newplayer"];
            Console.WriteLine("将把:" + oldDBStr + " 中的数据合并到:" + newDBStr);
            Console.WriteLine("输入 no 将取消执行");

            string msg = Console.ReadLine();
            if (msg == "no")
            {
                return 0;
            }

            MongoDatabase oldDB = MongoDatabase.Create(oldDBStr);
            MongoDatabase newDB = MongoDatabase.Create(newDBStr);
            int count = 0;
            //var collections = new string[] { "Task" };
            var collections = oldDB.GetCollectionNames();

            MongoCollection oldPlayer1 = oldDB.GetCollection("Player");
            MongoCollection newPlayer1 = newDB.GetCollection("Player");
            count += CoverPlayer("Player", oldPlayer1, newPlayer1);

            foreach (string key in collections)
            {
                if (key != "system.indexes" && key != "Player" && key != "UserLog")
                {
                    MongoCollection oldPlayer = oldDB.GetCollection(key);
                    MongoCollection newPlayer = newDB.GetCollection(key);
                    count += CoverDB(key, oldPlayer, newPlayer);
                }
            }
            MongoCollection oldUser = oldDB.GetCollection("UserLog");
            MongoCollection newUser = newDB.GetCollection("UserLog");
            count += CovertUser("UserLog", oldUser, newUser);
            return count;
        }

        static int CoverPlayer(string tableName, MongoCollection oldPlayer, MongoCollection newPlayer)
        {
            string renameSuffix = ConfigurationManager.AppSettings["renameSuffix"] ?? string.Empty;
            string[] r = renameSuffix.Split(',', ';');

            Console.WriteLine("开始合并:" + tableName);
            int show = 0;
            int count = 0;
            int exists = 0;
            int total = oldPlayer.Count(); //49459

            //var dates2 = oldPlayer.FindAs<BsonDocument>(Query.Exists("_id", true));
            //long dc2 = dates2.Count();
            //Console.WriteLine(dc2 == total);
            var dates = oldPlayer.FindAllAs<BsonDocument>();
            foreach (var v in dates)
            {
                BsonValue key;
                if (v.TryGetValue("_id", out key) && (key != 0))
                {
                    //检查_id是否存在
                    if (newPlayer.Count(Query.EQ("_id", key)) <= 0)
                    {
                        count++;
                        BsonValue name;
                        v.TryGetValue("Name", out name);
                        BsonValue newName = name;
                        if (r != null && r.Length > 0)
                        {
                            int i = 0;
                            newName = name + r[i++];
                            //检查名字是否存在
                            while (newPlayer.Count(Query.EQ("Name", newName)) == 1)
                            {
                                newName = newName.ToString() + r[i++];
                            }
                            if (newName != name)
                            {
                                Console.WriteLine("改名: " + name + "  -->" + newName);
                            }
                        }
                        v["Name"] = newName;
                        newPlayer.Save(v);
                    }
                    else
                    {
                        exists++;
                    }
                }
                show++;
                if (show % 1000 == 0)
                {
                    Console.WriteLine(tableName + "合并中.." + show);
                }
            }
            Console.WriteLine("**********" + tableName + ",合并:" + count + ",已存在:" + exists + ",丢失:" + (total - show));
            return count;
        }

        static int CovertUser(string tableName, MongoCollection oldPlayer, MongoCollection newPlayer)
        {
            Console.WriteLine("开始合并:" + tableName);
            int show = 0;
            int count = 0;
            int exists = 0;
            int total = oldPlayer.Count(); //49459

            var dates2 = oldPlayer.FindAs<BsonDocument>(Query.Exists("_id"));
            long dc2 = dates2.Count();
            Console.WriteLine(dc2 == total);
            var dates = oldPlayer.FindAllAs<BsonDocument>();
            foreach (var v in dates)
            {
                BsonValue key;
                if (v.TryGetValue("_id", out key))
                {
                    count++;
                    BsonDocument newDoc = newPlayer.FindOneAs<BsonDocument>(Query.EQ("_id", key));
                    if (newDoc != null)
                    {
                        newDoc = MergerUser(newDoc, v);
                        newPlayer.Save(newDoc);
                    }
                    else
                    {
                        newPlayer.Save(v);
                    }
                }
                show++;
                if (show % 1000 == 0)
                {
                    Console.WriteLine(tableName + "合并中.." + show);
                }
            }
            Console.WriteLine("**********" + tableName + ",合并:" + count + ",已存在:" + exists + ",丢失:" + (total - show));
            return count;
        }

        /// <summary>
        /// 将老数据合并到新表.
        /// </summary>
        /// <param name="newDoc"></param>
        /// <param name="oldDoc"></param>
        /// <returns></returns>
        static BsonDocument MergerUser(BsonDocument newDoc, BsonDocument oldDoc)
        {
            BsonValue oldV;
            if (oldDoc.TryGetValue("Created", out oldV))
            {
                DateTime newV = newDoc["Created"].AsDateTime;
                if (newV > oldV.AsDateTime)
                {
                    newDoc["Created"] = oldV.AsDateTime;
                }
            }
            if (oldDoc.TryGetValue("Modified", out oldV))
            {
                DateTime newV = newDoc["Modified"].AsDateTime;
                if (newV < oldV.AsDateTime)
                {
                    newDoc["Modified"] = oldV.AsDateTime;
                }
            }
            if (oldDoc.TryGetValue("TOT", out oldV))
            {
                newDoc["TOT"] = oldV.AsInt32 + (newDoc.Contains("TOT") ? newDoc["TOT"].AsInt32 : 0);
            }
            if (oldDoc.TryGetValue("Ver", out oldV))
            {
                newDoc["Ver"] = oldV.AsInt32 + newDoc["Ver"].AsInt32;
            }
            if (oldDoc.TryGetValue("Coin", out oldV))
            {
                newDoc["Coin"] = oldV.AsInt64 + (newDoc.Contains("Coin") ? newDoc["Coin"].AsInt64 : 0);
            }
            if (oldDoc.TryGetValue("GCE", out oldV))
            {
                newDoc["GCE"] = oldV.AsInt64 + (newDoc.Contains("GCE") ? newDoc["GCE"].AsInt64 : 0);
            }
            if (oldDoc.TryGetValue("GCI", out oldV))
            {
                newDoc["GCI"] = oldV.AsInt64 + (newDoc.Contains("GCE") ? newDoc["GCE"].AsInt64 : 0);
            }
            return newDoc;
        }


        static int CoverDB(string tableName, MongoCollection oldPlayer, MongoCollection newPlayer)
        {
            Console.WriteLine("开始合并:" + tableName);
            int show = 0;
            int count = 0;
            int exists = 0;
            int total = oldPlayer.Count(); //49459

            var dates2 = oldPlayer.FindAs<BsonDocument>(Query.Exists("_id"));
            long dc2 = dates2.Count();
            Console.WriteLine(dc2 == total);
            var dates = oldPlayer.FindAllAs<BsonDocument>();
            foreach (var v in dates)
            {
                BsonValue key;
                if (v.TryGetValue("_id", out key))
                {
                    if (newPlayer.Count(Query.EQ("_id", key)) <= 0)
                    {
                        count++;
                        newPlayer.Save(v);
                    }
                    else
                    {
                        exists++;
                    }
                }
                show++;
                if (show % 1000 == 0)
                {
                    Console.WriteLine(tableName + "合并中.." + show);
                }
            }
            Console.WriteLine("**********" + tableName + ",合并:" + count + ",已存在:" + exists + ",丢失:" + (total - show));
            //处理任务表
            //if (tableName == "Task")
            //{
            //    TaskCheck(newPlayer);
            //}
            return count;
        }


        static public void TaskCheck(MongoCollection newPlayer)
        {
            MongoCursor<BsonDocument> dates = newPlayer.FindAllAs<BsonDocument>();
            Dictionary<string, List<TaskInfo>> dic = new Dictionary<string, List<TaskInfo>>();
            foreach (var v in dates)
            {
                TaskInfo model = new TaskInfo();
                BsonValue id;
                v.TryGetValue("_id", out id);
                BsonValue created;
                v.TryGetValue("Created", out created);
                BsonValue playerid;
                v.TryGetValue("PlayerID", out playerid);

                BsonValue t;
                if (v.TryGetValue("Value", out t))
                {
                    BsonDocument bd = t as BsonDocument;
                    BsonValue taskType;
                    bd.TryGetValue("TaskType", out taskType);

                    BsonValue status;
                    if (bd.TryGetValue("Status", out status))
                    {
                        if (taskType.ToString() == "0")
                        {
                            if (status < 3)
                            {
                                model.id = id.ToString();
                                model.created = (DateTime)created;
                                List<TaskInfo> ms;
                                if (dic.TryGetValue((string)playerid, out ms))
                                {
                                    ms.Add(model);
                                }
                                else
                                {
                                    dic.Add((string)playerid, new List<TaskInfo>() { model });
                                }
                            }
                        }
                    }
                }
            }

            Dictionary<string, string> remtask = new Dictionary<string, string>();
            foreach (var k in dic)
            {
                List<TaskInfo> list = k.Value as List<TaskInfo>;
                if (list.Count > 1)
                {
                    Console.WriteLine(k.Key + ",List:" + list.Count);
                    TaskInfo tmp = null;
                    foreach (TaskInfo model in list)
                    {
                        if (tmp == null)
                        {
                            tmp = model;
                        }
                        else
                        {
                            if (tmp.created < model.created)
                            {
                                remtask.Add(tmp.id, tmp.id);
                            }
                            else
                            {
                                remtask.Add(model.id, model.id);
                            }
                        }
                    }
                }
            }

            var newP = newPlayer.FindAllAs<BsonDocument>();
            foreach (string str in remtask.Keys)
            {
                var query = Query.EQ("_id", str);
                newPlayer.Remove(query);
            }
        }
    }

    class TaskInfo
    {
        public string id
        {
            get;
            set;
        }
        public DateTime created
        {
            get;
            set;
        }
    }
}
