using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.AMF3;
using Sinan.Extensions;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// 战斗奖励(或惩罚)
    /// </summary>
    public class Award : ExternalizableBase
    {
        protected readonly Dictionary<string, int> m_goods;

        /// <summary>
        /// 玩家经验
        /// </summary>
        public int RoleExp
        {
            get;
            set;
        }

        /// <summary>
        /// 宠物经验
        /// </summary>
        public int PetExp
        {
            get;
            set;
        }

        /// <summary>
        /// 点券
        /// </summary>
        public int Bond
        {
            get;
            set;
        }

        /// <summary>
        /// 石币
        /// </summary>
        public int Score
        {
            get;
            set;
        }

        /// <summary>
        /// 物品
        /// </summary>
        public Dictionary<string, int> Goods
        {
            get { return m_goods; }
        }


        public Award()
        {
            m_goods = new Dictionary<string, int>();
        }

        public void Reset()
        {
            RoleExp = 0;
            PetExp = 0;
            Bond = 0;
            Score = 0;
            Goods.Clear();
        }

        /// <summary>
        /// 写战斗奖励
        /// </summary>
        /// <param name="writer"></param>
        protected override void WriteAmf3(IExternalWriter writer)
        {
            if (RoleExp > 0)
            {
                writer.WriteKey("PExp");
                writer.WriteInt(RoleExp);
            }
            if (PetExp > 0)
            {
                writer.WriteKey("RExp");
                writer.WriteInt(PetExp);
            }
            if (Bond > 0)
            {
                writer.WriteKey("Bond");
                writer.WriteInt(Bond);
            }
            if (Score > 0)
            {
                writer.WriteKey("Score");
                writer.WriteInt(Score);
            }
            if (m_goods != null && m_goods.Count > 0)
            {
                writer.WriteKey("Goods");
                writer.WriteIDictionary(m_goods);
            }
        }

        #region 计算方法
        /// <summary>
        /// 获取奖励(多个包)
        /// </summary>
        /// <param name="v">
        /// 多个包{P1:{Packet},P2:{Packet},...}
        /// Packet格式:
        /// {P:0.8,TMax:6,G_*1:{P:0.2,Min:1,Max:2},G_*2:{P:0.3, Min:1,Max:5},...}
        /// </param>
        /// <returns></returns>
        public static void GetPackets(Variant v, Dictionary<string, int> awards)
        {
            foreach (var item in v)
            {
                Variant packate = item.Value as Variant;
                if (packate != null)
                {
                    GetPacketAwards(packate, awards);
                }
            }
        }

        /// <summary>
        /// 计算单个奖励包裹
        /// P:  double      //获得的概率..
        /// TMax :6          //最大获得数量
        /// G_*1: P:double, Min:1,Max:2   //物品g1获得的概率为0.8...可获得1-2个..
        /// G_*2: P:double, Min:1,Max:5   //...............
        /// </summary>
        /// <param name="packate">格式:{P:0.8,TMax:6,G_*1:{P:0.2,Min:1,Max:2},G_*2:{P:0.3, Min:1,Max:5},...}</param>
        /// <param name="awards">奖励结果(物品ID:数量)</param>
        public static void GetPacketAwards(Variant packate, Dictionary<string, int> awards)
        {
            //P:  double 获得的概率.
            double p = packate.GetDoubleOrDefault("P");
            if (!NumberRandom.RandomHit(p)) return;

            // TMax 最大获得数量
            int maxCount = packate.GetIntOrDefault("TMax");
            if (maxCount <= 0)
            {
                maxCount = 5;
            }
            maxCount = NumberRandom.Next(maxCount) + 1;


            var awardP = GetPacketAwards(maxCount, packate);
            foreach (var v in awardP)
            {
                awards.SetOrInc(v.Key, v.Value);
            }
            //// 已获得的物品数
            //int currentCount = 0;
            //foreach (var gg in packate)
            //{
            //    if (maxCount <= currentCount)
            //    {
            //        return;
            //    }
            //    if (gg.Value is Variant)
            //    {
            //        int count = GetAwardCount(gg.Value as Variant);
            //        if (count > 0)
            //        {
            //            if (count + currentCount > maxCount)
            //            {
            //                count = maxCount - currentCount;
            //            }
            //            int rc;
            //            awards.TryGetValue(gg.Key, out rc);
            //            awards[gg.Key] = rc + count;
            //            currentCount += count;
            //        }
            //    }
            //}
        }


        static Dictionary<string, int> GetPacketAwards(int maxCount, Variant packate)
        {
            Dictionary<string, int> awards = new Dictionary<string, int>(maxCount);
            // 已获得的物品数
            int currentCount = 0;
            List<string> goods = new List<string>(packate.Count);
            List<int> pList = new List<int>(packate.Count);
            List<int> maxS = new List<int>(packate.Count);
            int totalP = 0;
            foreach (var gg in packate)
            {
                if (gg.Value is Variant)
                {
                    Variant v = gg.Value as Variant;
                    //int count = v.GetIntOrDefault("Min");
                    //if (count > 0)
                    //{
                    //    awards.SetOrInc(gg.Key, count);
                    //    currentCount += count;
                    //    if (maxCount <= currentCount)
                    //    {
                    //        return awards;
                    //    }
                    //}
                    int p = (int)(v.GetDoubleOrDefault("P") * 10000);
                    if (p <= 0) p = 1;
                    totalP += p;
                    pList.Add(totalP);
                    goods.Add(gg.Key);
                    maxS.Add(v.GetIntOrDefault("Max"));
                }
            }
            while (totalP > 0 && maxCount > currentCount)
            {
                int hit = NumberRandom.Next(totalP);
                for (int i = 0; i < pList.Count; i++)
                {
                    int curP = pList[i];
                    if (hit < pList[i]) //命中
                    {
                        int count = awards.SetOrInc(goods[i], 1);
                        if (count >= maxS[i])
                        {
                            if (count > maxS[i])
                            {
                                currentCount--;
                                awards.SetOrInc(goods[i], -1);
                            }
                            //删除...
                            totalP -= curP;
                            goods.RemoveAt(i);
                            maxS.RemoveAt(i);
                            ReSetPList(ref pList, i);
                            if (pList.Count == 0)
                            {
                                return awards;
                            }
                        }
                        currentCount++;
                        break;
                    }
                }
            }
            return awards;
        }

        static void ReSetPList(ref List<int> p, int i)
        {
            int v = i > 0 ? p[i] - p[i - 1] : p[i];
            int t = i;
            for (; t < p.Count - 1; t++)
            {
                p[i] = p[i + 1] - v;
            }
            p.RemoveAt(t);
        }


        /// <summary>
        /// 获取奖励数量
        /// P:    double //几率
        /// Min:  int    //最小值
        /// Max:  int    //最大值
        /// </summary>
        /// <param name="v">格式:{P:0.8,Min:2,Max:10}</param>
        /// <returns></returns>
        public static int GetAwardCount(Variant v)
        {
            if (v != null)
            {
                double p = v.GetDoubleOrDefault("P");
                if (NumberRandom.RandomHit(p))
                {
                    int min = v.GetIntOrDefault("Min");
                    int max = v.GetIntOrDefault("Max");
                    if (min >= max) return Math.Max(0, min);
                    return NumberRandom.Next(min, max + 1);
                }
            }
            return 0;
        }
        #endregion
    }
}
