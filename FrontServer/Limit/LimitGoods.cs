using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Util;
using Sinan.Extensions.FluentDate;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 需要物品
    /// </summary>
    public class LimitGoods : ILimit
    {
        readonly protected string m_holdGoodsMsg;

        /// <summary>
        /// 持有物品
        /// </summary>
        readonly protected string m_holdGoods;

        public LimitGoods(string name, string goodsid)
        {
            if (!string.IsNullOrEmpty(goodsid))
            {
                var needG = GameConfigAccess.Instance.FindOneById(goodsid);
                if (needG != null)
                {
                    //缺少道具【{0}】,无法进入
                    m_holdGoods = goodsid;
                    m_holdGoodsMsg = string.Format(TipManager.GetMessage(ClientReturn.IntoLimit5), needG.Name);
                }
            }
        }

        public bool Check(PlayerBusiness player, out string msg)
        {
            //物品限制
            if (!string.IsNullOrEmpty(m_holdGoods))
            {
                if (BurdenManager.GoodsCount(player.B0, m_holdGoods) <= 0)
                {
                    msg = m_holdGoodsMsg;
                    return false;
                }
            }
            msg = null;
            return true;
        }

        public bool Execute(PlayerBusiness player, out string msg)
        {
            //扣除物品
            if (!string.IsNullOrEmpty(m_holdGoods))
            {
                BurdenManager.Remove(player.B0, m_holdGoods, 1);
            }
            msg = null;
            return true;
        }

        public bool Rollback(PlayerBusiness player)
        {
            //退回物品.
            return true;
        }

        public static LimitGoods Create(string name, Variant v)
        {
            string goodsid = v.GetStringOrDefault("GoodsID");
            if (!string.IsNullOrEmpty(goodsid))
            {
                return new LimitGoods(name, goodsid);
            }
            return null;
        }
    }
}
