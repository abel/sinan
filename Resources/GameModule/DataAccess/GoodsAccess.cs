using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Data;
using Sinan.Extensions;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.GameModule
{
    /// <summary>
    /// 访问物品表
    /// </summary>
    public class GoodsAccess : VariantBuilder<Goods>
    {
        readonly static GoodsAccess m_instance = new GoodsAccess();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static GoodsAccess Instance
        {
            get { return m_instance; }
        }

        GoodsAccess()
            : base("Goods")
        {
        }

        /// <summary>
        /// 物品保存
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public override bool Save(Goods ex)
        {
            ex.Modified = DateTime.UtcNow;
            m_collection.Save(ex);
            var query = Query.EQ("_id", ex.ID);
            var update = MongoDB.Driver.Builders.Update.Inc("Ver", 1);
            var v = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
            return true;
        }

        /// <summary>
        /// 得到[Task:任务,Pet:宠物,Equip装备]列表
        /// </summary>
        /// <param name="playerid">角色ID</param>
        /// <param name="mainType">类型[Task:任务,Pet:宠物,Equip装备]</param>
        /// <returns></returns>
        public List<Goods> GetGoodsList(string playerid, string mainType)
        {
            var query = Query.And(Query.EQ("PlayerID", playerid), Query.EQ("MainType", mainType));
            var result = m_collection.FindAs<Goods>(query).ToList();
            return result;
        }

        /// <summary>
        /// 取理进行中任务列表
        /// </summary>
        /// <param name="playerid">角色ID</param>
        /// <param name="mainType">类型</param>
        /// <param name="Status">当前状态</param>
        /// <returns></returns>
        public List<Goods> GetTaskList(string playerid, string mainType, int Status)
        {
            var query = Query.And(Query.EQ("PlayerID", playerid), Query.EQ("MainType", mainType), Query.EQ("Value.Status", Status));
            var result = m_collection.FindAs<Goods>(query).ToList();
            return result;
        }

        /// <summary>
        /// 得到指定用户的及指定类型的信息
        /// </summary>
        /// <param name="playerid"></param>
        /// <param name="mainType"></param>
        /// <returns></returns>
        public Variant GetList(string playerid, string mainType)
        {
            List<Goods> list = GetGoodsList(playerid, mainType);
            Variant v = new Variant();
            foreach (Goods g in list)
            {
                v.Add(g.ID, g);
            }
            return v;
        }

        /// <summary>
        /// 删除指定信息
        /// </summary>
        /// <param name="id">唯一id</param>
        /// <param name="playerid">角色名</param>
        /// <returns></returns>
        public bool Remove(string id, string playerid)
        {
            var query = Query.And(Query.EQ("_id", id), Query.EQ("PlayerID", playerid));
            var r = m_collection.Remove(query, SafeMode.False);
            return true;
        }
        /// <summary>
        /// 删除指定信息
        /// </summary>
        /// <param name="id">唯一id</param>
        /// <returns></returns>
        public bool Remove(string id)
        {
            var query = Query.And(Query.EQ("_id", id));
            var r = m_collection.Remove(query, SafeMode.False);
            return true;
        }

        /// <summary>
        /// 获取物品信息
        /// </summary>
        /// <param name="goodsID"></param>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public Goods GetGoodsByID(string goodsID, string playerID)
        {
            var query = Query.And(Query.EQ("_id", goodsID), Query.EQ("PlayerID", playerID));
            return m_collection.FindOneAs<Goods>(query);
        }

        /// <summary>
        /// 获取物品信息
        /// </summary>
        /// <param name="goodsID"></param>
        /// <returns></returns>
        public Goods GetGoodsByID(string goodsID)
        {
            return m_collection.FindOneAs<Goods>(Query.EQ("_id", goodsID));
        }

        /// <summary>
        /// 更新装备持久度
        /// </summary>
        /// <param name="goodsid">装备ID</param>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool UpdateStamina(string goodsid, int n)
        {
            IMongoQuery query;
            if (n < 0)
            {
                query = Query.And(Query.EQ("_id", goodsid), Query.GTE("Value.Stamina.V", -n));
            }
            else
            {
                query = Query.EQ("_id", goodsid);
            }
            var update = MongoDB.Driver.Builders.Update.Inc("Value.Stamina.V", n);
            var v = m_collection.Update(query, update, UpdateFlags.None, SafeMode.True);
            return v.DocumentsAffected > 0;
        }

        /// <summary>
        /// 聊天信息通知信息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public Variant LiaoTianList(string msg)
        {
            Variant v = new Variant();
            v.Add("msg", "<f size=\"12\" color=\"ff3300\"><Send/><msg>" + msg + "</msg></f>");
            v.Add("level", 0);
            v.Add("showType", 0);
            v.Add("date", DateTime.UtcNow);
            return v;
        }

        /// <summary>
        /// 提高成功率
        /// </summary>
        /// <param name="number">数量</param>
        /// <returns></returns>
        public double GetSuccess(int number)
        {
            return (Math.Pow(1.02, number) - 0.98);
        }

        /// <summary>
        /// 道具时间限制
        /// </summary>
        /// <param name="gc">道具</param>
        /// <param name="v"></param>
        public void TimeLines(GameConfig gc, Variant v)
        {
            if (gc.Value.ContainsKey("TimeLines"))
            {
                Variant TimeLines = gc.Value.GetValueOrDefault<Variant>("TimeLines");
                int hour = 0;
                if (TimeLines != null)
                {
                    if (TimeLines.TryGetValueT("Hour", out hour) && hour > 0)
                    {
                        //限时问题
                        v.Add("EndTime", DateTime.UtcNow.AddHours(hour));
                    }
                    else if (TimeLines.ContainsKey("SetDate"))
                    {
                        //定时设置
                        DateTime t;
                        if (DateTime.TryParse(TimeLines.GetStringOrDefault("SetDate"), out t))
                        {
                            v.Add("EndTime", TimeLines.GetDateTimeOrDefault("SetDate"));
                        }
                    }
                }
            }
        }
    }
}
