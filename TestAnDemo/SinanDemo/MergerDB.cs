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
    class MergerDB
    {

        static public void CoverDB()
        {
            MongoDatabase oldDB = MongoDatabase.Create(ConfigurationManager.AppSettings["oldplayer"]);
            MongoDatabase newDB = MongoDatabase.Create(ConfigurationManager.AppSettings["newplayer"]);

            var collections = oldDB.GetCollectionNames();
            foreach (string key in collections)
            {
                if (key != "system.indexes")
                {
                    MongoCollection oldPlayer = oldDB.GetCollection(key);
                    MongoCollection newPlayer = newDB.GetCollection(key);
                    CoverDB(key, oldPlayer, newPlayer);
                }
            }
        }



        static public void CoverDB(string player, MongoCollection oldPlayer, MongoCollection newPlayer)
        {
            Console.WriteLine("正在合并:" + player);
            int show = 0;
            int count = 0;
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
                }
                show++;
                if (show % 1000 == 0)
                {
                    Console.WriteLine("正在合并:" + show);
                }
            }
            Console.WriteLine(player + ":" + count);
        }
    }
}
