using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Data;
using Sinan.Schedule;

namespace Sinan.BabyOPD
{
    public class TopAccess : PlatformBase
    {
        string m_serverID;

        MongoDatabase m_playerDb;
        public TopAccess(string serverID, string playerDb, string operationString)
            : base(operationString)
        {
            m_serverID = serverID;
            m_playerDb = MongoDatabase.Create(playerDb);
        }

        //public void SetTopN(DateTime time)
        //{
        //    var coll = m_playerDb.GetCollection("Player");
        //    SortByBuilder sb = SortBy.Descending("Level");
        //    //取前100名
        //    var players = coll.FindAll().SetFields("Name", "Level", "RoleID", "Dian", "FamilyName").
        //        SetSortOrder(sb).SetLimit(100);
        //    var topn = m_operation.GetCollection("Topn");

        //    //删除以前的.
        //    var q = Query.EQ("Created", time);
        //    topn.Remove(q);

        //    int index = 0;
        //    List<BsonDocument> ps = new List<BsonDocument>();
        //    foreach (BsonDocument v in players)
        //    {
        //        v["_id"] = ObjectId.GenerateNewId();
        //        v["Rank"] = ++index;
        //        v["Server"] = m_serverID;
        //        v["Created"] = time;
        //        ps.Add(v);
        //    }
        //    topn.InsertBatch(ps);
        //}

        //public void SetTopNPet(DateTime time)
        //{
        //    var coll = m_playerDb.GetCollection("Pet");
        //    SortByBuilder sb = SortBy.Descending("Value.PetsLevel");
        //    //取前100名
        //    var pets = coll.FindAll().SetFields("Name", "Value.PetsLevel", "Value.PetsType", "Value.PetsRank", "Value.ZiZhi", "Value.PlayerName", "Value.ChengChangDu.V").
        //        SetSortOrder(sb).SetLimit(100);

        //    var topn = m_operation.GetCollection("TopnPet");

        //    //删除以前的.
        //    var q = Query.EQ("Created", time);
        //    topn.Remove(q);

        //    int index = 0;
        //    List<BsonDocument> ps = new List<BsonDocument>();
        //    foreach (BsonDocument v in pets)
        //    {
        //        v["_id"] = ObjectId.GenerateNewId();
        //        v["Rank"] = ++index;
        //        v["Server"] = m_serverID;
        //        v["Created"] = time;
        //        ps.Add(v);
        //    }
        //    topn.InsertBatch(ps);
        //}
    }
}
