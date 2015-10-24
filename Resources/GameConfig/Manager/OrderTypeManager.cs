using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Sinan.Data;
using Sinan.FastConfig;
using Sinan.FastJson;
using Sinan.Log;
using Sinan.Util;
using Sinan.Security.Cryptography;
using Sinan.Extensions;

namespace Sinan.GameModule
{
    /// <summary>
    /// 趣游用,充值比例
    /// </summary>
    sealed public class OrderTypeManager : IConfigManager
    {
        readonly static OrderTypeManager m_instance = new OrderTypeManager();

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static OrderTypeManager Instance
        {
            get { return m_instance; }
        }
        private OrderTypeManager() { }

        public void Load(string path)
        {
            Variant roleConfig = VariantWapper.LoadVariant(path);
            if (roleConfig != null)
            {
                foreach (var item in roleConfig)
                {
                    int key;
                    if (int.TryParse(item.Key, out key))
                    {
                        Variant v = item.Value as Variant;
                        if (key >= 0 && key < maxoType && v != null)
                        {
                            moneys[key] = v.GetIntOrDefault("M");
                            coins[key] = v.GetIntOrDefault("G");
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 渠道编号	渠道名称	货币	兑换比例（元宝）
        /// 1	Mycard	台币	1:1
        /// 2	paypal	美元	1:30
        /// 3	mol	马币	1:10
        /// 4	offgames	马币	1:10
        /// 5	Gudang Voucher	印尼盾	300:1
        /// 6	mobius	比索	1.4:1
        /// 7	yeepay	RMB	1:5
        /// 8	MOL-Thai	泰株	1:1
        /// </summary>
        static int[] moneys = new int[maxoType];
        static int[] coins = new int[maxoType];
        const int maxoType = 255;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="order"></param>
        /// <param name="time">时间</param>
        /// <returns></returns>
        public static OrderResult CheckOrder(Order order, int time = 900)
        {
            if (order == null)
            {
                return OrderResult.OtherError;
            }
            //检查时间(误差time秒)
            if (Math.Abs(UtcTimeExtention.NowTotalSeconds() - order.ts) > time)
            {
                return OrderResult.InvalidTime;
            }

            if (order.money <= 0)
            {
                return OrderResult.AmtError;
            }
            if (order.Coin <= 0)
            {
                return OrderResult.CoinError;
            }

            if (order.providetype >= 0 && order.providetype < maxoType)
            {
                int a = moneys[order.providetype];
                int b = coins[order.providetype];
                if (a * b == 0)
                {
                    return OrderResult.InvalidOtype;
                }
                if ((int)(Math.Round(b * order.money)) == (a * order.Coin))
                {
                    return OrderResult.Success;
                }
                else
                {
                    return OrderResult.CoinError;
                }
            }
            else
            {
                return OrderResult.InvalidOtype;
            }
        }

        public void Unload(string fullPath)
        {
            Array.Clear(moneys, 0, maxoType);
            Array.Clear(coins, 0, maxoType);
        }
    }
}
