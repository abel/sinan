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
using Sinan.FastJson;
using Sinan.Util;

namespace Sinan.Demo
{
    class CoverDBHelper
    {
        static Dictionary<string, int> dicID = new Dictionary<string, int>(100000);

        static public void CoverDB()
        {
            Dictionary<string, string[]> ct = GetConverMap();

            int end = int.Parse(ConfigurationManager.AppSettings["SID"]);
            end = end % 256;
            int index = 16;
            MongoDatabase oldDB = MongoDatabase.Create(ConfigurationManager.AppSettings["oldplayer"]);
            MongoDatabase newDB = MongoDatabase.Create(ConfigurationManager.AppSettings["newplayer"]);

            MongoCollection oldPlayer = oldDB.GetCollection("Player");
            MongoCollection newPlayer = newDB.GetCollection("Player");

            SortByBuilder levDes = SortBy.Descending("Level");
            var players = oldPlayer.FindAllAs<BsonDocument>().SetSortOrder(levDes);
            foreach (var doc in players)
            {
                BsonValue id;
                doc.TryGetValue("_id", out id);
                System.Threading.Interlocked.Increment(ref index);
                int newid = (index << 8) + end;
                doc["_id"] = newid;
                dicID.Add(id.ToString(), newid);
                newPlayer.Save(doc);
            }
            Console.WriteLine("Player转换完成" + dicID.Count);

            foreach (var v in ct)
            {
                CoverDB(oldDB, newDB, v.Key, v.Value);
            }
        }

        private static Dictionary<string, string[]> GetConverMap()
        {
            Dictionary<string, string[]> ct = new Dictionary<string, string[]>();
            string file = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "ConverMap.txt");
            using (FileStream fs = File.OpenRead(file))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            {
                string text = sr.ReadToEnd();
                Variant config = JsonConvert.DeserializeObject<Variant>(text);
                foreach (var item in config)
                {
                    object a = item.Value;
                    if (a is IList)
                    {
                        string[] fileds = ((IList)a).Cast<string>().ToArray();
                        ct.Add(item.Key, fileds);
                    }
                    else
                    {
                        ct.Add(item.Key, new string[] { a.ToString() });
                    }
                }
            }
            return ct;
        }


        static public void CoverDB(MongoDatabase oldDB, MongoDatabase newDB, string collName, params string[] files)
        {
            MongoCollection oldColl = oldDB.GetCollection(collName);
            MongoCollection newColl = newDB.GetCollection(collName);
            int count = 0;

            var players = oldColl.FindAllAs<BsonDocument>();
            foreach (var doc in players)
            {
                foreach (string filed in files)
                {
                    Change(doc, filed);
                }
                //分离Box中的一点券购买
                if (collName == "PlayerEx")
                {
                    BoxToBuy(doc);
                }
                else if (collName == "Pet")
                {
                    RemovePlayerName(doc);
                }

                newColl.Save(doc);
                count++;
            }
            Console.WriteLine(collName + "转换完成" + count);
        }

        private static void RemovePlayerName(BsonDocument doc)
        {
            BsonValue v;
            if (doc.TryGetValue("Value", out v) && v is BsonDocument)
            {
                BsonDocument value = v as BsonDocument;
                value.Remove("PlayerName");
            }
        }

        /// <summary>
        /// 分离Box中的一点券购买
        /// </summary>
        /// <param name="doc"></param>
        private static void BoxToBuy(BsonDocument doc)
        {
            BsonValue v;
            if (doc.TryGetValue("Activity", out v) && v is BsonDocument)
            {
                BsonDocument activity = v as BsonDocument;
                activity["Sign"] = new BsonArray();
            }
            doc["Box"] = new BsonDocument();
        }

        static void Change(BsonDocument doc, string filed)
        {
            int index = filed.IndexOf('.');
            if (index == -1)
            {
                BsonValue value;
                if (doc.TryGetValue(filed, out value))
                {
                    if (value != null)
                    {
                        int newValue;
                        if (dicID.TryGetValue(value.ToString(), out newValue))
                        {
                            if (filed == "_id")
                            {
                                doc[filed] = newValue;
                            }
                            else
                            {
                                doc[filed] = newValue.ToString("X");
                            }
                        }
                    }
                }
            }
            else
            {
                BsonValue value;
                if (doc.TryGetValue(filed.Substring(0, index), out value))
                {
                    if (value is BsonDocument)
                    {
                        Change(value as BsonDocument, filed.Substring(index + 1));
                    }
                    else if (value is BsonArray)
                    {
                        Change(value as BsonArray, filed.Substring(index + 1));
                    }

                }
            }
        }

        static void Change(BsonArray doc, string filed)
        {
            foreach (var v in doc)
            {
                if (v is BsonDocument)
                {
                    Change(v as BsonDocument, filed);
                }
            }
        }
    }
}
