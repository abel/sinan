using System;
using System.IO;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.Extensions;
using Sinan.FastConfig;
using Sinan.FastJson;
using Sinan.Util;

namespace Sinan.GameModule
{
    /// <summary>
    /// 一点券购买物品
    /// </summary>
    public class BondBuyManager : IConfigManager
    {
        readonly static BondBuyManager m_instance = new BondBuyManager();

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static BondBuyManager Instance
        {
            get { return m_instance; }
        }

        BondBuyManager() { }

        ///// <summary>
        ///// 一点券可购买的物品
        ///// </summary>
        //Dictionary<string, int> m_goods;

        int total = 0;     //所有物品的总比值
        int[] m_goodsR;    //物品被抽中的比值
        string[] m_goods;  //对应的物品ID

        public void Load(string path)
        {
            Variant v = VariantWapper.LoadVariant(path);
            if (v == null)
            {
                m_goods = new string[0];
                m_goodsR = new int[0];
                return;
            }
            m_goods = new string[v.Count];
            m_goodsR = new int[v.Count];
            int index = 0;
            foreach (var item in v)
            {
                int p = Convert.ToInt32(item.Value);
                m_goods[index] = item.Key;
                m_goodsR[index] = p;
                total += p;
                index++;
            }
        }

        public void Unload(string path)
        {
        }

        /// <summary>
        /// 获得随机物品.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public string[] RandomGoods(int count = 3)
        {
            if (m_goods.Length <= count)
            {
                return m_goods;
            }
            int index = 0;
            string[] find = new string[count];
            while (index != count)
            {
                int x = NumberRandom.Next(total);
                for (int i = 0; i < m_goodsR.Length; i++)
                {
                    if (m_goodsR[i] > x)
                    {
                        string k = m_goods[i];
                        if (!find.Contains(k))
                        {
                            find[index++] = k;
                        }
                        break;
                    }
                    else
                    {
                        x -= m_goodsR[i];
                    }
                }
            }
            return find;
        }

    }
}
