using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Schedule;
using Sinan.Util;

namespace Sinan.GameModule
{
    /// <summary>
    /// 获取排行
    /// </summary>
    public class TopAccess : VariantBuilder<Player>
    {
        readonly static TopAccess m_instance = new TopAccess();

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static TopAccess Instance
        {
            get { return m_instance; }
        }

        TopAccess()
            : base("Player")
        {
        }

        MongoDatabase m_db;

        public override void Connect(string connectionString)
        {
            m_db = MongoDatabase.Create(connectionString);
            m_collection = m_db.GetCollection(m_collName);
        }

        /// <summary>
        /// 获取玩家排名
        /// </summary>
        /// <returns></returns>
        public List<Player> GetPlayerPaiHang(int pageSize, int pageIndex, string roleID)
        {
            SortByBuilder sbb = SortBy.Descending("Level").Ascending("S");
            List<Player> players;
            if (string.IsNullOrEmpty(roleID))
            {
                var n = m_collection.FindAllAs<Player>().SetSortOrder(sbb).SetSkip(pageIndex * pageSize).SetLimit(pageSize);
                players = n.ToList();
            }
            else
            {
                var query = Query.EQ("RoleID", roleID);
                var n = m_collection.FindAs<Player>(query).SetSortOrder(sbb).SetSkip(pageIndex * pageSize).SetLimit(pageSize);
                players = n.ToList();
            }
            return players;
        }

        /// <summary>
        /// 获取玩家排名(按成就点)
        /// </summary>
        /// <returns></returns>
        public List<Player> GetPlayerPaiHang(int pageSize, int pageIndex)
        {
            SortByBuilder sbb = SortBy.Descending("Dian").Ascending("_id");
            var n = m_collection.FindAllAs<Player>().SetSortOrder(sbb).SetSkip(pageIndex * pageSize).SetLimit(pageSize);
            return n.ToList();
        }

        /// <summary>
        /// 获取我的排行
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public int GetMyRank(Player player)
        {
            if (player == null)
            {
                return -1;
            }
            int level = player.Level;
            var query = Query.Or(Query.GT("Level", level), Query.And(Query.EQ("Level", level), Query.LT("S", player.S)));
            return m_collection.Count(query) + 1;
        }

        /// <summary>
        /// 获取我的家族排行
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public int GetMyFamilyRank(Player player)
        {
            if (player == null || string.IsNullOrEmpty(player.FamilyName))
            {
                return -1;
            }
            int level = player.Level;
            var query = Query.And(Query.EQ("FamilyName", player.FamilyName),
                Query.Or(Query.GT("Level", level), Query.And(Query.EQ("Level", level), Query.LT("S", player.S)))
                );
            return m_collection.Count(query) + 1;
        }

        /// <summary>
        ///  获取我的排行
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Player GetMyRank(string tableName, int id)
        {
            var coll = m_db.GetCollection(tableName);
            Player p = coll.FindOneAs<Player>(Query.EQ("_id", id));
            return p;
        }

        /// <summary>
        /// 获取所有玩家排行
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public List<Player> GetPlayerPaiHang(string tableName)
        {
            var coll = m_db.GetCollection(tableName);
            return coll.FindAllAs<Player>().ToList();
        }

        /// <summary>
        /// 获取所有宠物排行
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public List<PetRank> GetPetPaiHang(string tableName)
        {
            var coll = m_db.GetCollection(tableName);
            List<PetRank> pets = new List<PetRank>();
            foreach (Pet pet in coll.FindAllAs<Pet>())
            {
                PetRank rank = new PetRank(pet);

                int pid = rank.PlayerID;
                if (pid > 0)
                {
                    rank.PlayerName = PlayerAccess.Instance.GetPlayerName(pid) ?? string.Empty;
                }
                pets.Add(rank);
            }
            return pets;
        }

        public void CreatePlayerTopn(string day, int topn, bool checkExists)
        {
            string dumpname = "Topn" + day;
            MongoCollection dumplog = m_db.GetCollection("DumpLog");
            var dumpQ = Query.EQ("_id", dumpname);

            if (checkExists && m_db.CollectionExists(dumpname))
            {
                BsonDocument doc = dumplog.FindOneAs<BsonDocument>(dumpQ);
                if (doc == null || doc.Contains("End"))
                {
                    return;
                }
                m_db.RenameCollection(dumpname, dumpname + DateTime.UtcNow.Ticks);
            }

            var up = Update.Set("Start", DateTime.UtcNow);
            dumplog.Update(dumpQ, up, UpdateFlags.Upsert, SafeMode.True);


            var playerColl = m_db.GetCollection("Player" + day);
            var topnColl = m_db.GetCollection<Player>(dumpname);

            Dictionary<int, Player> players = new Dictionary<int, Player>();

            SortByBuilder sbb = SortBy.Descending("Level").Ascending("S");

            var n = playerColl.FindAllAs<Player>().SetSortOrder(sbb).SetLimit(topn);
            int rank = 1;
            foreach (var player in n)
            {
                player.Value = new Variant();
                player.Value["Lev0"] = rank++;
                players.Add(player.PID, player);
            }

            int count = rank;
            foreach (string roleID in new string[] { "1", "2", "3" })
            {
                n = playerColl.FindAs<Player>(Query.EQ("RoleID", roleID)).SetSortOrder(sbb).SetLimit(topn);
                rank = 1;
                foreach (var player in n)
                {
                    Player p;
                    if (!players.TryGetValue(player.PID, out p))
                    {
                        count++;
                        player.Value = new Variant();
                        players.Add(player.PID, player);
                        p = player;
                    }
                    p.Value["Lev" + roleID] = rank++;
                }
            }

            sbb = SortBy.Descending("Dian").Ascending("_id");
            n = playerColl.FindAllAs<Player>().SetSortOrder(sbb).SetLimit(topn);
            rank = 1;
            foreach (var player in n)
            {
                Player p;
                if (!players.TryGetValue(player.PID, out p))
                {
                    count++;
                    player.Value = new Variant();
                    players.Add(player.PID, player);
                    p = player;
                }
                p.Value["Dian"] = rank++;
            }
            foreach (Player player in players.Values)
            {
                topnColl.Save(player);
            }

            up = Update.Set("End", DateTime.UtcNow).Set("Count", count);
            dumplog.Update(dumpQ, up, UpdateFlags.Upsert, SafeMode.True);
        }

        public void CreatePetTopn(string day, int topn, bool checkExists)
        {
            string dumpname = "TopnPet" + day;
            MongoCollection dumplog = m_db.GetCollection("DumpLog");
            var dumpQ = Query.EQ("_id", dumpname);

            if (checkExists && m_db.CollectionExists(dumpname))
            {
                BsonDocument doc = dumplog.FindOneAs<BsonDocument>(dumpQ);
                if (doc == null || doc.Contains("End"))
                {
                    return;
                }
                m_db.RenameCollection(dumpname, dumpname + DateTime.UtcNow.Ticks);
            }

            var up = Update.Set("Start", DateTime.UtcNow);
            dumplog.Update(dumpQ, up, UpdateFlags.Upsert, SafeMode.True);

            var topnColl = m_db.GetCollection<Pet>(dumpname);
            var petColl = m_db.GetCollection("Pet");

            Dictionary<string, Pet> players = new Dictionary<string, Pet>();

            SortByBuilder sbb = SortBy.Descending("Value.PetsLevel");

            var n = petColl.FindAllAs<Pet>().SetSortOrder(sbb).SetLimit(topn);
            int rank = 1;
            foreach (var pet in n)
            {
                pet.Value["Topn"] = new Variant();
                ((Variant)(pet.Value["Topn"]))["Lev0"] = rank++;
                players.Add(pet.ID, pet);
            }

            int count = rank;
            foreach (string roleID in new string[] { "1", "2", "3" })
            {
                n = petColl.FindAs<Pet>(Query.EQ("Value.PetsType", roleID)).SetSortOrder(sbb).SetLimit(topn);
                rank = 1;
                foreach (var pet in n)
                {
                    Pet p;
                    if (!players.TryGetValue(pet.ID, out p))
                    {
                        count++;
                        pet.Value["Topn"] = new Variant();
                        players.Add(pet.ID, pet);
                        p = pet;
                    }
                    ((Variant)(p.Value["Topn"]))["Lev" + roleID] = rank++;
                }
            }

            sbb = SortBy.Descending("Value.ChengChangDu.V");
            n = petColl.FindAllAs<Pet>().SetSortOrder(sbb).SetLimit(topn);
            rank = 1;
            foreach (var pet in n)
            {
                Pet p;
                if (!players.TryGetValue(pet.ID, out p))
                {
                    count++;
                    pet.Value["Topn"] = new Variant();
                    players.Add(pet.ID, pet);
                    p = pet;
                }
                ((Variant)(p.Value["Topn"]))["CCD"] = rank++;
            }

            foreach (Pet player in players.Values)
            {
                topnColl.Save(player);
            }

            up = Update.Set("End", DateTime.UtcNow).Set("Count", count);
            dumplog.Update(dumpQ, up, UpdateFlags.Upsert, SafeMode.True);
        }
    }
}
