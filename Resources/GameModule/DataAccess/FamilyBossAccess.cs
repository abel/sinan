using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
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
    public class FamilyBossAccess : VariantBuilder<FamilyBoss>
    {
        readonly static FamilyBossAccess m_instance = new FamilyBossAccess();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static FamilyBossAccess Instance
        {
            get { return m_instance; }
        }

        FamilyBossAccess()
            : base("FamilyBoss")
        {
        }

        /// <summary>
        /// 查找Boss
        /// </summary>
        /// <param name="family"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public List<FamilyBoss> FindBoss(string family, string pid)
        {
            string f = "AwardLog." + pid;
            FieldsBuilder fbName = Fields.Include("BossID", "Created", "State", f);

            DateTime now = DateTime.UtcNow;
            var query = Query.And(
                Query.EQ("FamilyID", family),
                Query.GTE("Created", now.AddDays(-1)));

            return m_collection.FindAs<FamilyBoss>(query).SetFields(fbName).ToList();
        }

        /// <summary>
        /// 招唤Boss
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool SummonBoss(string fid, string bid)
        {
            string id = fid + bid;
            int day = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
            var query = Query.And(Query.EQ("_id", id), Query.EQ("Day", day));
            if (m_collection.Count(query) > 0)
            {
                return false; //已存在招唤
            }
            else
            {
                query = Query.EQ("_id", id);
                var update = Update
                    .Set("FamilyID", fid)
                    .Set("BossID", bid)
                    .Set("Created", DateTime.UtcNow)
                    .Set("Day", day)
                    .Set("AwardLog", new BsonDocument())
                    .Set("State", 0);

                var result = m_collection.Update(query, update, UpdateFlags.Upsert, SafeMode.True);
                if (result.Ok && result.DocumentsAffected > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 查看奖励
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="pid">角色ID</param>
        /// <returns></returns>
        public string ViewAward(string id, string pid)
        {
            string f = "AwardLog." + pid;
            DateTime now = DateTime.UtcNow;
            var query = Query.And(
                Query.EQ("_id", id),
                Query.GTE("Created", now.AddDays(-1)),
                Query.EQ("State", 2), //2.已胜利
                Query.Exists(f));

            FieldsBuilder fbName = Fields.Include(f);

            var document = m_collection.FindAs<BsonDocument>(query).SetFields(fbName).FirstOrDefault();
            if (document != null)
            {
                BsonValue v;
                if (document.TryGetValue("AwardLog", out v))
                {
                    BsonDocument doc = v as BsonDocument;
                    if (doc != null && doc.TryGetValue(pid, out v))
                    {
                        BsonArray arr = v.AsBsonArray;
                        if (arr != null && arr.Count > 0)
                        {
                            v = arr[0];
                            return v == null ? string.Empty : v.ToString();
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 领取奖励
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pid"></param>
        /// <param name="goodsid"></param>
        /// <returns></returns>
        public bool GetAward(string id, string pid, string goodsid)
        {
            string f = "AwardLog." + pid;

            DateTime now = DateTime.UtcNow;
            var query = Query.And(
                Query.EQ("_id", id),
                Query.GTE("Created", now.AddDays(-1)),
                Query.EQ("State", 2), //2.已胜利
                Query.EQ(f + ".0", goodsid));

            var update = Update.Unset(f);
            var result = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
            if (result.Ok && result.DocumentsAffected > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 改变状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ChangeState(string id, int oldState, int newState)
        {
            DateTime now = DateTime.UtcNow;
            var query = Query.And(
                Query.EQ("_id", id),
                Query.EQ("State", oldState));
            var update = Update.Set("State", newState);
            var result = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
            return result.Ok && result.DocumentsAffected > 0;
        }

        /// <summary>
        /// 检查能否战斗
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool TryFight(string id)
        {
            DateTime now = DateTime.UtcNow;
            var query = Query.And(
                Query.EQ("_id", id)
                , Query.EQ("State", 0)
                //,Query.GT("Created", now.AddMinutes(-30))
                );
            var update = Update.Set("State", 1);
            var result = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
            return result.Ok && result.DocumentsAffected > 0;
        }

        /// <summary>
        /// 将战斗中的重置为非战斗状态
        /// </summary>
        /// <returns></returns>
        public int ResetFight()
        {
            var query = Query.And(
                Query.GT("Created", DateTime.UtcNow.AddMinutes(-30)),
                Query.EQ("State", 1));
            var update = Update.Set("State", 0);
            var result = m_collection.Update(query, update, UpdateFlags.Multi, SafeMode.True);
            return (int)result.DocumentsAffected;
        }

        /// <summary>
        /// 打败Boss
        /// </summary>
        /// <param name="id"></param>
        /// <param name="awardLog">家族成员的奖励</param>
        /// <returns></returns>
        public bool Win(string id, Variant awardLog)
        {
            DateTime now = DateTime.UtcNow;
            var query = Query.And(
                Query.EQ("_id", id),
                Query.EQ("State", 1));
            var update = Update.Set("State", 2)
                .Set("AwardLog", new BsonDocumentWrapper(awardLog))
                .Set("KillTime", DateTime.UtcNow);
            var result = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
            return result.Ok && result.DocumentsAffected > 0;
        }
    }
}