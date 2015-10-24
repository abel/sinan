using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Sinan.Data;
using Sinan.Util;

namespace Sinan.GameModule
{

    public class ActivityManager
    {
        //活动
        static Dictionary<string, Variant> m_dic = new Dictionary<string, Variant>();
        //奖励
        static Dictionary<int, string> n_dic = new Dictionary<int, string>();
        //签到奖励
        static Dictionary<int, string> n_sign = new Dictionary<int, string>();
        //连续登录奖励
        static Dictionary<int, string> n_day = new Dictionary<int, string>();
        //vip套餐
        static Dictionary<string, Variant> n_vip = new Dictionary<string, Variant>();
        //vip每日奖励
        static Dictionary<int, string> n_dayvip = new Dictionary<int, string>();
        /// <summary>
        /// 判断答题是否正确
        /// </summary>
        /// <param name="soleid">题目ID</param>
        /// <param name="answer">选择答案</param>
        /// <returns>0正确,1表示答错,2题目不正确</returns>
        public static void Load(string path)
        {
            if (m_dic.Count > 0)
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNodeList nodes = doc.SelectNodes("*");
            foreach (XmlNode node in nodes)
            {
                foreach (XmlNode n in node.ChildNodes)
                {
                    Variant tmp = new Variant();
                    string id = "";
                    int m = 0;
                    string goodsid = "";
                    switch (n.Name)
                    {
                        case "content":
                            id = n.Attributes["id"].Value;
                            tmp.Add("id", id);
                            tmp.Add("name", n.Attributes["name"].Value);
                            tmp.Add("type", n.Attributes["type"].Value);
                            tmp.Add("finish", Convert.ToInt32(n.Attributes["finish"].Value));
                            tmp.Add("value", Convert.ToInt32(n.Attributes["value"].Value));
                            if (m_dic.ContainsKey(id))
                                continue;
                            m_dic.Add(id, tmp);
                            break;
                        case "award":
                            m = Convert.ToInt32(n.Attributes["value"].Value);
                            goodsid = n.Attributes["goodsid"].Value;
                            if (n_dic.ContainsKey(m))
                                continue;
                            n_dic.Add(m, goodsid);
                            break;
                        case "sign":
                            m = Convert.ToInt32(n.Attributes["value"].Value);
                            goodsid = n.Attributes["goodsid"].Value;
                            if (n_sign.ContainsKey(m))
                                continue;
                            n_sign.Add(m, goodsid);
                            break;
                        case "day":
                            m = Convert.ToInt32(n.Attributes["value"].Value);
                            goodsid = n.Attributes["goodsid"].Value;
                            if (n_day.ContainsKey(m))
                                continue;
                            n_day.Add(m, goodsid);
                            break;
                        case "vip":
                            id = n.Attributes["id"].Value;
                            if (n_vip.ContainsKey(id))
                                continue;
                            tmp.Add("id", id);
                            tmp.Add("goodsid", n.Attributes["goodsid"].Value);
                            tmp.Add("name", n.Attributes["name"].Value);
                            tmp.Add("vipid", n.Attributes["vipid"].Value);
                            tmp.Add("count", Convert.ToInt32(n.Attributes["count"].Value));
                            tmp.Add("number", Convert.ToInt32(n.Attributes["number"].Value));
                            n_vip.Add(id, tmp);
                            break;
                        case "dayvip":
                            m = Convert.ToInt32(n.Attributes["value"].Value);
                            if (n_dayvip.ContainsKey(m))
                                continue;
                            n_dayvip.Add(m, n.Attributes["goodsid"].Value);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 完成进度
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Variant> GetFinish()
        {
            return m_dic;
        }

        /// <summary>
        /// 奖励情况 
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetAward()
        {
            return n_dic;
        }

        /// <summary>
        /// 签到奖励情况
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetSignAward()
        {
            return n_sign;
        }

        /// <summary>
        /// 连续登录奖励
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetDayAward()
        {
            return n_day;
        }

        /// <summary>
        /// Vip套餐服务
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Variant> GetVipAward()
        {
            return n_vip;
        }

        /// <summary>
        /// Vip每日奖励
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetDayVipAward()
        {
            return n_dayvip;
        }

        /// <summary>
        /// 得到相同类型的活动
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<string> ActivityList(int at)
        {
            List<string> list = new List<string>();
            foreach (Variant v in m_dic.Values)
            {
                if (v.GetIntOrDefault("type") == at)
                {
                    list.Add(v.GetStringOrDefault("id"));
                }
            }
            return list;
        }

        /// <summary>
        /// 清理完成条件
        /// </summary>
        /// <returns></returns>
        public static Variant FinishClear()
        {
            Variant v = new Variant();
            foreach (string k in m_dic.Keys)
            {
                v.Add(k, 0);
            }
            return v;
        }

        /// <summary>
        /// 奖励清理
        /// </summary>
        /// <returns></returns>
        public static Variant AwardClear()
        {
            Variant v = new Variant();
            foreach (int k in n_dic.Keys)
            {
                v.Add(k.ToString(), 0);
            }
            return v;
        }

        /// <summary>
        /// 清理VIP兑换
        /// </summary>
        /// <returns></returns>
        public static Variant VipClear()
        {
            Variant v = new Variant();
            foreach (string k in n_vip.Keys)
            {
                v.Add(k, 0);
            }
            return v;
        }

        /// <summary>
        /// 判断提供的时间是否在本周内
        /// </summary>
        /// <param name="dt">比较时间</param>
        /// <returns></returns>
        public static bool LocalWeek(DateTime dt, DateTime end)
        {
            TimeSpan ts = end.Date - dt.Date;
            double td = ts.TotalDays;
            int wk = (int)(end.DayOfWeek);
            if (wk == 0) wk = 7;
            if (td >= 7 || td >= wk)
                return false;
            return true;
        }
    }
}
