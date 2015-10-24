using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.GameModule
{
    sealed public partial class PlayerAccess : VariantBuilder<Player>
    {
        const int maxPlyaer = 3;
        static readonly PlayerAccess m_instance = new PlayerAccess();

        const string PidInc = "S"; //角色ID自增保存的位置
        static readonly FieldsBuilder fbPid = Fields.Include(PidInc);
        static readonly IMongoQuery pid0 = Query.EQ("_id", 0);
        static readonly FieldsBuilder fbID = Fields.Include("_id");
        static readonly FieldsBuilder fbName = Fields.Include("Name");

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static PlayerAccess Instance
        {
            get { return m_instance; }
        }

        PlayerAccess()
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
        /// 根据平台ID获取玩家列表
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="worldid">服务器ID</param>
        /// <returns></returns>
        public List<Player> GetPlayers(string userID, int worldid)
        {
            IMongoQuery query;
            if (worldid >= 0)
            {
                query = Query.And(Query.EQ("UserID", userID), Query.EQ("SID", worldid));
            }
            else
            {
                query = Query.EQ("UserID", userID);
            }
            return m_collection.FindAs<Player>(query).ToList();
        }

        /// <summary>
        /// 取单个玩家的详细信息
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="pid"></param>
        /// <param name="readEx">是否读取扩展信息</param>
        /// <returns></returns>
        public T GetPlayer<T>(int pid, bool readEx = true)
            where T : Player
        {
            var query = Query.EQ("_id", pid);
            T player = m_collection.FindOneAs<T>(query);
            if (player != null)
            {
                if (readEx)
                {
                    if (player.Value == null)
                    {
                        player.Value = new Variant();
                    }
                    List<PlayerEx> ex = PlayerExAccess.Instance.FindPlayerEx(pid);
                    if (ex != null)
                    {
                        foreach (var v in ex)
                        {
                            player.Value[v.Name] = v;
                        }
                    }
                }
            }
            return player;
        }

        /// <summary>
        /// 保存玩家信息
        /// </summary>
        /// <param name="playe-r"></param>
        /// <returns></returns>
        public override bool Save(Player player)
        {
            if (player != null && player.PID != 0)
            {
                player.Modified = DateTime.UtcNow;
                var query = Query.EQ("_id", player.PID);
                var update = Update.Set("Modified", player.Modified).Inc("Ver", 1).Set("Online", player.Online)
                    .Set("HP", player.HP).Set("MP", player.MP).Set("Experience", player.Experience);
                if (!string.IsNullOrEmpty(player.SceneID))
                {
                    update.Set("SceneID", player.SceneID).Set("X", player.X).Set("Y", player.Y).Set("Line", player.Line);
                }
                var v = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
                return v == null ? false : v.DocumentsAffected > 0;
            }
            return false;
        }

        /// <summary>
        /// 保存玩家服装信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SaveClothing(Player player)
        {
            player.Modified = DateTime.UtcNow;
            var query = Query.EQ("_id", player.PID);
            var update = Update.Set("Modified", player.Modified).Inc("Ver", 1)
                .Set("Mount", player.Mount)
                .Set("Body", player.Body)
                .Set("Weapon", player.Weapon)
                .Set("Coat", player.Coat)
                .Set("IsCoat", player.IsCoat);
            var v = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
            return v == null ? false : v.DocumentsAffected > 0;
        }

        /// <summary>
        /// 保存玩家的部分信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool SaveValue(int id, string key, BsonValue value)
        {
            var query = Query.EQ("_id", id);
            var update = Update.Set("Modified", DateTime.UtcNow).Inc("Ver", 1).Set(key, value);
            var v = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
            return v == null ? false : v.DocumentsAffected > 0;
        }

        /// <summary>
        /// 保存玩家的部分信息
        /// </summary>
        /// <returns></returns>
        public bool SaveValue(int id, params Tuple<string, BsonValue>[] values)
        {
            if (values == null || values.Length == 0) return false;
            var query = Query.EQ("_id", id);
            var update = Update.Set("Modified", DateTime.UtcNow).Inc("Ver", 1);
            foreach (var kv in values)
            {
                update.Set(kv.Item1, kv.Item2);
            }
            var v = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
            return v == null ? false : v.DocumentsAffected > 0;
        }

        /// <summary>
        /// 改变玩家状态.
        /// </summary>
        /// <param name="pid">角色ID</param>
        /// <param name="statue">角色状态. 0:冻结;1: 活动, 2:删除中</param>
        /// <returns></returns>
        public bool ChangeState(int pid, int statue)
        {
            var query = Query.And(Query.EQ("_id", pid), Query.LT("State", 3));
            var update = Update.Set("State", statue).Set("Modified", DateTime.UtcNow).Inc("Ver", 1);
            var v = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
            bool result = (v == null ? false : v.DocumentsAffected > 0);
            return result;
        }

        /// <summary>
        /// 删除玩家
        /// </summary>
        /// <param name="pid">角色ID</param>
        /// <param name="freeName">是否释放角色名称,使其可以重用</param>
        /// <returns></returns>
        public Player DeletePlayer(int pid, bool freeName)
        {
            //不能恢复
            var query = Query.EQ("_id", pid);
            var player = m_collection.FindOneAs<Player>(query);
            if (player != null)
            {
                var oldColl = m_db.GetCollection("Player_delete");
                oldColl.Save(player);
                m_collection.Remove(query, SafeMode.True);
                if (freeName)
                {
                    //重新开放角色名
                    WordAccess.Instance.SetDeleted(player.Name);
                }
            }
            return player;
        }

        /// <summary>
        /// 获取角色等级
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public int GetPlayerLevel(int pid, string userID)
        {
            var query = Query.And(Query.EQ("_id", pid), Query.EQ("UserID", userID));
            BsonDocument doc = m_collection.FindAs<BsonDocument>(query).SetFields("Level").FirstOrDefault();
            if (doc != null)
            {
                BsonValue lev;
                if (doc.TryGetValue("Level", out lev))
                {
                    return lev.AsInt32;
                }
            }
            return -1;
        }

        public int Count(int zoneid, string userID)
        {
            var query = Query.And(Query.EQ("UserID", userID), Query.EQ("SID", zoneid), Query.LT("State", 3));
            return m_collection.Count(query);
        }

        /// <summary>
        /// 检查用户名是否已存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ExistsName(string name)
        {
            var query = Query.EQ("Name", name);
            return m_collection.Count(query) > 0;
        }

        /// <summary>
        /// 创建玩家
        /// </summary>
        /// <param name="zoneid"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool CreatePlayer(int zoneid, Player player)
        {
            player.SID = zoneid;
            DateTime now = DateTime.UtcNow;
            player.Modified = now;
            player.Created = now;

            SafeModeResult result = m_collection.Insert(player, SafeMode.True);

            //插入扩展信息
            PlayerExAccess.Instance.CreatePlayerEx(player.PID, player.RoleID);
            //设置用户名为已使用
            WordAccess.Instance.SetUsed(player.Name);
            return true;
        }

        /// <summary>
        /// 升级
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public int IncrementLevel(int pid, int upLev)
        {
            if (pid == 0) return 0;
            //2010年1月1日的Ticks;
            const long t2010_1_1 = 633979008000000000 / 1000000;
            DateTime now = DateTime.UtcNow;
            int sl = (int)((now.Ticks / 1000000 - t2010_1_1));

            var query = Query.EQ("_id", pid);
            var update = Update.Set("Modified", now).Inc("Ver", 1).Inc("Level", upLev).Set("S", sl);
            var v = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
            if (v == null || v.DocumentsAffected == 0)
            {
                return 0;
            }
            return sl;
        }

        /// <summary>
        /// 根据玩家名查找ID
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetPlayerIdByName(string name)
        {
            var query = Query.EQ("Name", name);
            var document = m_collection.FindAs<BsonDocument>(query).SetFields(fbID).FirstOrDefault();
            if (document != null)
            {
                return document["_id"].AsInt32;
            }
            return 0;
        }

        /// <summary>
        /// 根据服务器ID和用户ID查找玩家角色ID
        /// (只适合单角色的趣游版)
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public int GetPlayerId(int sid, string uid)
        {
            var query = Query.And(Query.EQ("SID", sid), Query.EQ("UserID", uid), Query.LT("State", 3));
            var document = m_collection.FindAs<BsonDocument>(query).SetFields(fbID).FirstOrDefault();
            if (document != null)
            {
                return document["_id"].AsInt32;
            }
            return 0;
        }

        /// <summary>
        /// 根据ID查找玩家名
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetPlayerName(int id)
        {
            var query = Query.And(Query.EQ("_id", id));
            var document = m_collection.FindAs<BsonDocument>(query).SetFields(fbName).FirstOrDefault();
            if (document != null)
            {
                return document["Name"].ToString();
            }
            return null;
        }

        /// <summary>
        /// 战绩清除
        /// </summary>
        /// <returns></returns>
        public bool FightValueClear()
        {
            var update = Update.Set("FightValue", 0);
            IMongoQuery query = Query.NE("FightValue", 0);
            var v = m_collection.Update(query, update, UpdateFlags.Multi, SafeMode.False);
            return true;
        }

        /// <summary>
        /// 重置在线信息
        /// </summary>
        /// <returns></returns>
        public int ResetOnline()
        {
            var query = Query.EQ("Online", true);
            var update = Update.Set("Online", false);
            var v = m_collection.Update(query, update, UpdateFlags.Multi, SafeMode.True);
            return v == null ? 0 : (int)(v.DocumentsAffected);
        }

        public bool Rename(int pid, string newName)
        {
            var query = Query.EQ("_id", pid);
            var update = Update.Set("Name", newName);
            var v = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
            return v == null ? false : v.DocumentsAffected > 0;
        }


        /// <summary>
        /// 家族成员列表
        /// </summary>
        /// <param name="famliyName">家族名</param>
        /// <returns></returns>
        public List<Variant> GetPlayersByFamliy(string famliyName)
        {
            var query = Query.EQ("FamilyName", famliyName);
            var players = m_collection.FindAs<Variant>(query).SetFields("Name", "Level", "Online", "RoleID");
            return players.ToList();
        }

        /// <summary>
        /// 创建玩家ID
        /// </summary>
        /// <returns></returns>
        public int CreatePlayerID(int zoneid)
        {
            var update = MongoDB.Driver.Builders.Update.Inc(PidInc, 256);
            var fr = m_collection.FindAndModify(pid0, null, update, fbPid, true, true);
            if (fr != null && fr.Ok)
            {
                BsonValue vaule;
                if (fr.ModifiedDocument.TryGetValue(PidInc, out vaule))
                {
                    int id = vaule.ToInt32();
                    //后8位表示服务器ID
                    return (id & 0x7FFFFF00) + (zoneid & 0xFF);
                }
            }
            return 0;
        }

        /// <summary>
        /// 初始化角色ID,
        /// 只在初始化Player表时执行一次插入
        /// </summary>
        /// <param name="zoneid">分区</param>
        public void InitPlayerID(int zoneid)
        {
            var query = Query.And(Query.NotExists(PidInc), Query.EQ("_id", 0));
            var update = Update.Inc(PidInc, zoneid & 0xff).Set("Created", DateTime.UtcNow);
            m_collection.Update(query, update, UpdateFlags.Upsert, SafeMode.False);
        }

        /// <summary>
        /// 取得角色列表
        /// </summary>
        /// <param name="ids">角色ID</param>
        /// <returns></returns>
        public List<Player> GetPlayers(List<string> ids)
        {
            BsonValue[] q = new BsonValue[ids.Count];
            for (int i = 0; i < ids.Count; i++)
            {
                q[i] = Convert.ToInt32(ids[i], 16);
            }
            var query = Query.In("_id", q);
            return m_collection.FindAs<Player>(query).ToList();
        }

    }
}
