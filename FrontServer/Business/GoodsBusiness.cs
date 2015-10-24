using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Util;
using Sinan.Entity;

namespace Sinan.FrontServer
{
    sealed public class GoodsBusiness
    {
        static readonly GameConfigAccess m_access;
        static readonly Dictionary<string, GameConfig> cache;

        static GoodsBusiness()
        {
            m_access = GameConfigAccess.Instance;
            cache = new Dictionary<string, GameConfig>();
        }
        /// <summary>
        /// 获取单个物品
        /// </summary>
        /// <param name="goodsID"></param>
        /// <returns></returns>
        static public GameConfig GetGoods(string goodsID)
        {
            GameConfig v;
            if (!cache.TryGetValue(goodsID, out v))
            {
                v = m_access.FindOneById(goodsID);
                if (v != null)
                {
                    cache[goodsID] = v;
                }
            }
            return v;
        }

        /// <summary>
        /// 获取所有物品信息
        /// </summary>
        /// <param name="goodsSort"></param>
        /// <returns></returns>
        static public List<GameConfig> GetAllGoods(string goodsSort)
        {
            // TODO: 需添加缓存机制.
            return m_access.Find(MainType.Goods);
        }

        /// <summary>
        /// 得到物品类型
        /// </summary>
        /// <param name="goodsID"></param>
        /// <returns></returns>
        static public int GetGoodsInfo(string goodsID)
        {
            GameConfig model = GetGoods(goodsID);
            if (model != null)
            {
                Variant stackcount = model.Value["StackCount"] as Variant;

                //表示普通包袱与战斗包袱可同时存放
                if ((int)stackcount["B0"] > 0 && (int)stackcount["B2"] > 0)
                {
                    return 2;
                }

                //表示只能存放普通包袱
                if ((int)stackcount["B0"] > 0)
                {
                    return 0;
                }

                //表示只能存放宠物包袱
                if ((int)stackcount["B1"] > 1)
                {
                    return 1;
                }
                //if ((int)stackcount["B2"] > 1)
                //{
                //    return 2;
                //}

                //表示只能存放入任务包袱
                if ((int)stackcount["B3"] > 1)
                {
                    return 3;
                }
            }
            return 0;
        }

        /// <summary>
        /// 得到商品
        /// </summary>
        /// <param name="goodsID">商品ID</param>
        /// <returns></returns>
        static public Dictionary<string, int> GetStackCount(string goodsID)
        {
            GameConfig model = GetGoods(goodsID);
            if (model != null)
            {
                Variant sc = model.Value["StackCount"] as Variant;
                Dictionary<string, int> dic = new Dictionary<string, int>();
                for (int i = 0; i < 7; i++)
                {
                    dic.Add("B" + i, (int)sc["B" + i]);
                }
                return dic;
            }
            return new Dictionary<string, int>();
        }
    }
}
