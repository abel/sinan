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
    public class FamilyAccess : VariantBuilder<Family>
    {
        readonly static FamilyAccess m_instance = new FamilyAccess();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static FamilyAccess Instance
        {
            get { return m_instance; }
        }

        FamilyAccess()
            : base("Family")
        {
        }

        /// <summary>
        /// 家族列表分页
        /// </summary>
        /// <param name="pageSize">角色ID</param>
        /// <param name="pageIndex">每一页的条数</param>
        /// <param name="familyName">家族名字</param>
        /// <param name="totalCount">总页数</param>
        /// <param name="currPage">当前页数</param>
        /// <returns></returns>
        public List<Family> FamilyPage(int pageSize, int pageIndex, string familyName, out int totalCount, out int currPage)
        {
            //得到总条数
            IMongoQuery qc = Query.And(Query.Matches("Name", new BsonRegularExpression(".*" + familyName + ".*", "i")));
            if (familyName != string.Empty)
                totalCount = m_collection.Count(qc);
            else
                totalCount = m_collection.Count();

            int y = 0;
            int s = Math.DivRem(totalCount, pageSize, out y);
            if (y > 0) s++;
            if (pageIndex + 1 > s) pageIndex = s;


            SortByBuilder sb = SortBy.Descending("Value.Level").Ascending("Modified").Descending("Created");
            currPage = pageIndex;

            MongoCursor<Family> mc;
            if (familyName != string.Empty)
                mc = m_collection.FindAs<Family>(qc).SetSortOrder(sb).SetSkip(currPage * pageSize).SetLimit(pageSize);
            else
                mc = m_collection.FindAllAs<Family>().SetSortOrder(sb).SetSkip(currPage * pageSize).SetLimit(pageSize);
            return mc.ToList();
        }

        /// <summary>
        /// 判断家族名称是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool FamilyIsExist(string name)
        {
            var query = Query.And(Query.EQ("Name", name));
            return m_collection.Count(query) > 0;
        }

        /// <summary>
        /// 解散家族
        /// </summary>
        /// <param name="familyid"></param>
        /// <returns></returns>
        public bool DelFamily(string familyid)
        {
            var quert = Query.And(Query.EQ("_id", familyid));
            m_collection.Remove(quert, SafeMode.False);
            return true;
        }

        public Family GetFamilyName(string familyname) 
        {
            //var query = Query.EQ("Name", familyname);
            //return m_collection.FindAs<Family>(query);
            return null;
        }

        /// <summary>
        /// 家族排名
        /// </summary>
        /// <param name="level">家族等级</param>
        /// <param name="modified">升级时间</param>
        /// <param name="created"></param>
        /// <returns></returns>
        public int FamilySort(int level, DateTime modified, DateTime created)
        {
            var query = Query.And(Query.GTE("Value.Level", level), Query.LTE("Modified", modified), Query.LTE("Created", created));
            return m_collection.Count(query);
        }

        /// <summary>
        /// 在家族成员中的排序
        /// </summary>
        /// <param name="playerid"></param>
        /// <returns></returns>
        public void MembersSort(List<Variant> persons)
        {
            int i, j, k;
            Variant tmp;
            for (i = 0; i < persons.Count; ++i)
            {
                j = i;
                for (k = i + 1; k < persons.Count; ++k)
                {
                    if (Convert.ToInt32(persons[i]["Level"]) < Convert.ToInt32(persons[k]["Level"]))
                    {
                        tmp = persons[k];
                        persons[k] = persons[i];
                        persons[i] = tmp;
                    }
                    else if (Convert.ToInt32(persons[i]["Level"]) == Convert.ToInt32(persons[k]["Level"]))
                    {
                        if (Convert.ToInt32(persons[i]["Devote"]) < Convert.ToInt32(persons[k]["Devote"]))
                        {
                            tmp = persons[k];
                            persons[k] = persons[i];
                            persons[i] = tmp;
                        }
                        else if (Convert.ToInt32(persons[i]["Devote"]) == Convert.ToInt32(persons[k]["Devote"]))
                        {
                            if (DateTime.Parse(persons[i]["AddDate"].ToString()) > DateTime.Parse(persons[k]["AddDate"].ToString()))
                            {
                                tmp = persons[k];
                                persons[k] = persons[i];
                                persons[i] = tmp;
                            }
                        }
                    }
                }
            }

            for (int m = 0; m < persons.Count; m++)
            {
                persons[m]["Sort"] = m + 1;
            }
        }

        /// <summary>
        /// 得到家族的技能
        /// </summary>
        /// <param name="id">家族</param>
        /// <returns></returns>
        public Variant GetFamilySkill(string id)
        {
            return FindOneById(id).Value.GetVariantOrDefault("Skill");
        }

        /// <summary>
        /// 得到某家族每日贡献度
        /// </summary>
        /// <param name="id">家族</param>
        /// <returns></returns>
        public int FamilyDev(string id)
        {
            Family model = FindOneById(id);
            if (model == null)
                return 0;
            Variant v = model.Value;
            if (v == null)
                return 0;
            DateTime devTime;
            if (v.TryGetValueT("DevTime", out devTime))
            {
                DateTime dt = DateTime.UtcNow;
                if (dt.Date != devTime.Date)
                    return 0;
                return v.GetIntOrDefault("DayDev");
            }
            return 0;
        }

        /// <summary>
        /// 得到某家族每日贡献度
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int FamilyDev(Family model) 
        {            
            if (model == null)
                return 0;
            Variant v = model.Value;
            if (v == null)
                return 0;
            DateTime devTime;
            if (v.TryGetValueT("DevTime", out devTime))
            {
                DateTime dt = DateTime.UtcNow;
                if (dt.Date != devTime.Date)
                    return 0;
                return v.GetIntOrDefault("DayDev");
            }
            return 0;
        }

        public override bool Save(Family entity)
        {
            if (entity != null && !string.IsNullOrEmpty(entity.ID))
            {
                var query = Query.EQ("_id", entity.ID);
                var update = Update.Set("Modified", entity.Modified)
                    .Set("Score", entity.Score)
                    .Set("Value", BsonDocumentWrapper.Create(entity.Value))
                    .Set("WeiWang", entity.WeiWang)
                    .Inc("Ver", 1);

                var v = m_collection.Update(query, update, UpdateFlags.Upsert, SafeMode.True);
                return v == null ? false : v.DocumentsAffected > 0;
            }
            return false;
        }

        public bool Insert(Family entity)
        {
            var v = m_collection.Save<Family>(entity, SafeMode.True);
            return v == null ? false : v.DocumentsAffected > 0;
        }
    }
}