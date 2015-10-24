using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// 明雷
    /// </summary>
    public class VisibleApc
    {
        /// <summary>
        /// 编号
        /// </summary>
        public string ID
        {
            get;
            protected set;
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get;
            protected set;
        }

        /// <summary>
        /// 皮肤
        /// </summary>
        public string Skin
        {
            get;
            protected set;
        }

        /// <summary>
        /// 单人最大遇怪数
        /// </summary>
        public double MaxCount
        {
            get;
            protected set;
        }

        /// <summary>
        /// 单人最小遇怪数
        /// </summary>
        public double MinCount
        {
            get;
            protected set;
        }

        /// <summary>
        /// 生成规则(随机站位方式)
        /// </summary>
        protected List<Rule> Rules
        {
            get;
            set;
        }

        /// <summary>
        /// 固定站位方式
        /// </summary>
        protected List<string> Fix
        {
            get;
            set;
        }

        public VisibleApc(Variant config)
        {
            ID = config.GetStringOrDefault("_id");
            Name = config.GetStringOrDefault("Name");
            Variant v = config.GetVariantOrDefault("Value");
            if (v != null)
            {
                this.Skin = v.GetStringOrDefault("Skin");
                this.MinCount = v.GetDoubleOrDefault("MinCount", 1);
                this.MaxCount = v.GetDoubleOrDefault("MaxCount", 2);

                this.Fix = new List<string>();
                IList fix = v["Fix"] as IList;
                if (fix != null && fix.Count > 0)
                {
                    foreach (string d in fix)
                    {
                        Fix.Add(d);
                    }
                }

                this.Rules = new List<Rule>();
                IList rules = v["Rules"] as IList;
                if (rules != null && rules.Count > 0)
                {
                    foreach (Variant d in v["Rules"] as IList)
                    {
                        int max = d.GetIntOrDefault("Max", 1);
                        double p = d.GetDoubleOrDefault("P");
                        string apc = d.GetStringOrDefault("APC");
                        if (max >= 1 && p > 0 && (!string.IsNullOrEmpty(apc)))
                        {
                            Rules.Add(new Rule(apc, p, Math.Min(10, max)));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 创建APC列表
        /// </summary>
        /// <param name="playerCount">玩家个数</param>
        /// <returns></returns>
        public Dictionary<int, string> CreateApc(int count)
        {
            if (!reset)
            {
                ResetBB();
            }
            Dictionary<int, string> npcs = new Dictionary<int, string>(count);

            if (Fix != null)
            {
                for (int i = 0; i < Fix.Count; i++)
                {
                    npcs.Add(i, Fix[i]);
                    if (npcs.Count >= count)
                    {
                        return npcs;
                    }
                }
            }

            List<Rule> rules = new List<Rule>(this.Rules);
            List<int> maxCounts = new List<int>(rules.Count);
            List<int> numbers = new List<int>(rules.Count);
            int total = 0;
            for (int i = 0; i < rules.Count; i++)
            {
                maxCounts.Add(rules[i].MaxCount);
                total += (int)(rules[i].Probability * 1000000);
                numbers.Add(total);
            }

            while (npcs.Count < count && rules.Count > 0)
            {
                int num = NumberRandom.Next(numbers[numbers.Count - 1]);
                int i = numbers.FindIndex(x => x > num);
                npcs.Add(npcs.Count, rules[i].ApcID);
                if (npcs.Count >= count)
                {
                    return npcs;
                }

                //控制数量
                maxCounts[i] = maxCounts[i] - 1;
                if (maxCounts[i] == 0)
                {
                    bool bb = rules[i].BB;
                    rules.RemoveAt(i);
                    maxCounts.RemoveAt(i);
                    if (bb) //删除BB
                    {
                        for (i = 0; i < rules.Count; i++)
                        {
                            if (rules[i].BB)
                            {
                                rules.RemoveAt(i);
                                maxCounts.RemoveAt(i--);
                            }
                        }
                    }
                    total = 0;
                    numbers = rules.Select(x => { total += (int)(x.Probability * 1000000); return total; }).ToList();
                }
            }
            return npcs;
        }

        private bool reset = false;
        private void ResetBB()
        {
            foreach (var rule in Rules)
            {
                Apc apc = ApcManager.Instance.FindOne(rule.ApcID);
                if (apc != null && apc.ApcType == FighterType.BB)
                {
                    rule.BB = true;
                    rule.MaxCount = 1;
                }
            }
            reset = true;
        }

        /// <summary>
        /// APC生成规则
        /// </summary>
        protected class Rule
        {
            public Rule(string apcID, double p, int max)
            {
                this.ApcID = apcID;
                this.Probability = p;
                this.MaxCount = max;
            }

            /// <summary>
            /// 是否是宝宝
            /// </summary>
            public bool BB;

            /// <summary>
            /// APCID
            /// </summary>
            public readonly string ApcID;

            /// <summary>
            /// 产生的概率
            /// </summary>
            public readonly double Probability;

            /// <summary>
            /// 同一场战斗中生成的最大生数量
            /// </summary>
            public int MaxCount;
        }
    }
}
