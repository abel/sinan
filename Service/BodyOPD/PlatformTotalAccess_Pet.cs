using System;
using System.Linq;
using System.Collections.Generic;
using Sinan.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Sinan.BabyOPD
{
    partial class PlatformTotalAccess : PlatformBase
    {
        /// <summary>
        /// 统计
        /// </summary>
        /// <param name="id">唯一标识</param>
        /// <returns></returns>
        public bool SaveTotal(DateTime endTime)
        {
            string id = endTime.ToString("yyyyMMdd");
            Dictionary<string, BsonValue> data = new Dictionary<string, BsonValue>();
            data.Add("UserTotal", BsonValue.Create(UserTotal(endTime)));
            data.Add("PlayerTotal", BsonValue.Create(PlayerTotal(endTime)));

            data.Add("PetTotal", BsonValue.Create(PetTotal(endTime)));
            data.Add("PetRankTotal", BsonValue.Create(PetRankTotal(endTime)));
            data.Add("PetZiZhi", BsonValue.Create(PetZiZhiTotal(endTime)));
            return Save(id, data);
        }

        /// <summary>
        /// 得到玩家账号账数
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public int UserTotal(DateTime endTime)
        {
            MongoCollection<BsonDocument> userLog = m_playerDb.GetCollection("UserLog");
            var query = Query.LT("Created", endTime);
            return userLog.Count(query);
        }

        /// <summary>
        /// 玩家各等级统计
        /// </summary>
        /// <returns></returns>
        public BsonDocument PlayerTotal(DateTime endTime)
        {
            var query = Query.LT("Created", endTime);
            MongoCollection<BsonDocument> player = m_playerDb.GetCollection("Player");
            var up = "function(obj,prev){ prev.Count+=1;}";
            var ui = new BsonDocument("Count", 0);
            var ug = player.Group(query, "Level", ui, up, null);
            return LevelDoc(ug);
        }

        /// <summary>
        /// 宠物各等级数量的统计
        /// </summary>
        /// <returns></returns>
        public BsonDocument PetTotal(DateTime endTime)
        {
            var query = Query.LT("Created", endTime);
            MongoCollection<BsonDocument> pets = m_playerDb.GetCollection("Pet");
            var pr = "function(obj,prev){ prev.Count+=1;}";
            var pi = new BsonDocument("Count", 0);
            var pg = pets.Group(query, "Value.PetsLevel", pi, pr, null);
            return LevelDoc(pg);
        }

        /// <summary>
        /// 宠物各阶级数量的统计
        /// </summary>
        /// <returns></returns>
        public BsonDocument PetRankTotal(DateTime endTime)
        {
            var query = Query.LT("Created", endTime);
            MongoCollection<BsonDocument> pets = m_playerDb.GetCollection("Pet");
            var pr = "function(obj,prev){ prev.Count+=1;}";
            var pi = new BsonDocument("Count", 0);
            var pg = pets.Group(query, "Value.PetsRank", pi, pr, null);
            return LevelDoc(pg);
        }


        /// <summary>
        /// 宠物不同资质的统计
        /// </summary>
        /// <returns></returns>
        public BsonDocument PetZiZhiTotal(DateTime endTime)
        {
            var query = Query.LT("Created", endTime);
            MongoCollection<BsonDocument> pets = m_playerDb.GetCollection("Pet");
            var pr = "function(obj,prev){ prev.Count+=1;}";
            var pi = new BsonDocument("Count", 0);
            var pg = pets.Group(query, "Value.ZiZhi", pi, pr, null);
            return LevelDoc(pg);
        }

        /// <summary>
        /// 得到指定字段的数量统计
        /// </summary>
        /// <param name="bsonDoc"></param>
        /// <returns></returns>
        BsonDocument LevelDoc(IEnumerable<BsonDocument> bsonDoc)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            foreach (BsonDocument doc in bsonDoc)
            {
                int level = 0;
                int count = 0;
                foreach (BsonElement k in doc)
                {
                    switch (k.Name)
                    {
                        case "Value.PetsLevel"://宠物等级
                        case "Value.PetsRank": //宠物等级
                        case "Value.ZiZhi":    //宠物资质
                        case "Level":          //角色等级

                            level = Convert.ToInt32(k.Value);
                            break;
                        case "Count":
                            count = Convert.ToInt32(k.Value);
                            break;
                    }
                }
                dic.SetOrInc(level.ToString(), count);
            }
            return new BsonDocument(dic);
        }
    }
}
