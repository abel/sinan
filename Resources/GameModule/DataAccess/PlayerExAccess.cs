using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.GameModule
{
    sealed public class PlayerExAccess : VariantBuilder
    {
        readonly static PlayerExAccess m_instance = new PlayerExAccess();

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static PlayerExAccess Instance
        {
            get { return m_instance; }
        }

        FieldsBuilder excludeFields;
        PlayerExAccess()
            : base("PlayerEx")
        {
            excludeFields = new FieldsBuilder();
            excludeFields.Exclude("Ver", "Modified");
        }

        /// <summary>
        /// 保存玩家扩展信息
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public bool Save(PlayerEx ex)
        {
            var query = Query.EQ("_id", ex.ID);
            var value = BsonDocumentWrapper.Create(ex.Value);
            var update = Update.Set("Modified", DateTime.UtcNow).Inc("Ver", 1).Set(ex.Name, value);
            //为UpdateFlags.None,因为已存在_id.
            var v = m_collection.Update(query, update, UpdateFlags.None, SafeMode.False);
            return true;
        }

        /// <summary>
        /// 创建用户扩展信息
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public bool CreatePlayerEx(int pid, string roleID)
        {
            var list = GameConfigAccess.Instance.Find("PlayerEx");
            var query = Query.EQ("_id", pid);
            var update = Update.Set("Modified", DateTime.UtcNow).Inc("Ver", 1);
            foreach (var item in list)
            {
                Variant obj = null;
                if (item.Name == "Skill")
                {
                    item.Value.TryGetValueT<Variant>(roleID, out obj);
                }
                var value = BsonDocumentWrapper.Create(obj ?? item.Value);
                update.Set(item.Name, value);
            }
            var v = m_collection.Update(query, update, UpdateFlags.Upsert, SafeMode.True);
            if (v.Ok && v.DocumentsAffected > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取指定玩家的所有扩展信息
        /// </summary
        /// <param name="playerID"></param>
        /// <returns></returns>
        public List<PlayerEx> FindPlayerEx(int pid)
        {
            var query = Query.EQ("_id", pid);
            List<PlayerEx> results = new List<PlayerEx>();
            var extends = m_collection.FindAs<Variant>(query).SetFields(excludeFields);
            foreach (Variant v in extends)
            {
                foreach (var item in v)
                {
                    if (item.Key != "_id")
                    {
                        PlayerEx ex = new PlayerEx(pid, item.Key);
                        ex.Value = item.Value as Variant;
                        results.Add(ex);
                    }
                }
                break;
            }
            return results;
        }

        /// <summary>
        /// 根据ID查询实体
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public PlayerEx FindOneById(string playerID)
        {
            if (playerID != null)
            {
                int pid;
                if (Sinan.Extensions.StringFormat.TryHexNumber(playerID, out pid))
                {
                    return m_collection.FindOneAs<PlayerEx>(Query.EQ("_id", playerID));
                }
            }
            return null;
        }

        /// <summary>
        /// 溜宠信息
        /// </summary>
        public List<Variant> SlipPets(PlayerEx b3)
        {
            //表示存在溜宠物变化
            IList c3 = b3.Value.GetValue<IList>("C");
            List<Variant> lv = new List<Variant>();
            foreach (Variant v in c3)
            {
                if (v.GetIntOrDefault("P") > 3 && v.GetStringOrDefault("E") != string.Empty && v.GetIntOrDefault("I") == 0)
                {
                    Pet p = PetAccess.Instance.FindOneById(v.GetStringOrDefault("E"));
                    if (p == null) 
                        continue;
                    Variant pv = new Variant();
                    pv.Add("ID", p.ID);
                    pv.Add("Skill", p.Value.GetValueOrDefault<Variant>("Skill"));
                    pv.Add("PetsID", p.Value["PetsID"]);
                    pv.Add("PetsRank", p.Value["PetsRank"]);
                    pv.Add("Skin", p.Value["Skin"]);
                    pv.Add("Name", p.Name);
                    pv.Add("ZiZhi", p.Value["ZiZhi"]);
                    lv.Add(pv);
                }
            }
            return lv;
        }

    }
}
