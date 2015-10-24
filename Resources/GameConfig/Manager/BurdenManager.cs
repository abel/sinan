using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.Extensions;
using Sinan.Entity;
using Sinan.Observer;
using Sinan.Util;
using Sinan.Log;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Sinan.GameModule
{
    public class BurdenManager
    {
        /// <summary>
        /// 清除格子信息
        /// </summary>
        /// <param name="v"></param>
        public static void BurdenClear(IDictionary<string, object> v)
        {
            if (v == null)
                return;
            v["E"] = string.Empty;//道具唯一
            v["G"] = string.Empty;//道具ID
            v["S"] = 0;//排序
            v["H"] = 0;//是否绑定
            v["A"] = 0;//数量
            v["D"] = 0;//是否可堆叠
            v["T"] = null;//道具可变相关信息
            if (v.ContainsKey("I"))
            {
                v["I"] = 0;
            }
            if (v.ContainsKey("R"))
            {
                v["R"] = 0;
            }
        }

        /// <summary>
        /// 判断包袱是否装满
        /// </summary>
        /// <param name="Content">包袱信息</param>
        /// <param name="goodsList">key为道具ID,Value为Variant,说明Number道具数量,IsBinding绑定状态:0非绑定1为绑定,StactCount道具堆叠数</param>
        /// <returns>true包袱空格子数不足</returns>
        public static bool IsFullBurden(PlayerEx burden, Dictionary<string, Variant> goodsList)
        {
            IList Content = burden.Value.GetValue<IList>("C");
            return IsFullBurden(Content, goodsList);
        }

        /// <summary>
        /// 判断包袱是否装满
        /// </summary>
        /// <param name="Content">包袱信息</param>
        /// <param name="goodsList">key为道具ID,Value为Variant,说明Number道具数量,IsBinding绑定状态:0非绑定1为绑定,StactCount道具堆叠数</param>
        /// <returns>true包袱空格子数不足</returns>
        public static bool IsFullBurden(IList Content, Dictionary<string, Variant> goodsList)
        {
            int m = BurdenSpace(Content);//空格子数量
            int n = 0;//需要空格子数量
            foreach (string k in goodsList.Keys)
            {
                Variant v = goodsList[k] as Variant;
                int z = GameConfigAccess.Instance.GetStactCount(k);
                if (z <= 0) continue;
                n += NeedSpace(Content, k, v, z);
            }
            if (n > m)
                return true;
            return false;
        }

        /// <summary>
        /// 得到一种道具，判断需要多少格子
        /// </summary>
        /// <param name="Content">包袱列表</param>
        /// <param name="goodsid">道具id</param>
        /// <param name="number0">非绑定道具数量</param>
        /// <param name="number1">绑定道具数量</param>
        /// <returns>需要空格子数量</returns>
        public static int NeedSpace(IList Content, string goodsid, Variant v, int sc)
        {
            int space = 0;
            for (int isbinding = 0; isbinding <= 1; isbinding++)
            {
                //0为非绑定,1为绑定
                int number = v.GetIntOrDefault("Number" + isbinding);
                if (number <= 0)
                    continue;

                int cur = 0;//当前该物品所占的格子数
                foreach (Variant k in Content)
                {
                    if (k.GetStringOrDefault("G") != goodsid)
                        continue;
                    if (k.GetIntOrDefault("H") != isbinding)
                        continue;
                    if (v.ContainsKey("EndTime"))
                    {
                        Variant t = k.GetVariantOrDefault("T");
                        if (t == null)
                            continue;
                        if (t.GetStringOrDefault("EndTime") != v.GetStringOrDefault("EndTime"))
                            continue;
                    }
                    number += k.GetIntOrDefault("A");
                    cur++;
                }
                int m = 0;//余数
                int n = Math.DivRem(number, sc, out m);
                n = m > 0 ? n + 1 : n;
                space += (n > cur) ? (n - cur) : 0;
            }
            return space;
        }

        /// <summary>
        /// 得到空格数量
        /// </summary>
        /// <param name="burden"></param>
        /// <returns></returns>
        public static int BurdenSpace(PlayerEx burden)
        {
            Variant v = burden.Value;
            if (v == null) return 0;
            IList c = v.GetValue<IList>("C");
            return BurdenSpace(c);
        }

        /// <summary>
        /// 包袱空格数量
        /// </summary>
        /// <param name="content">包裕列表</param>
        /// <returns></returns>
        public static int BurdenSpace(IList content)
        {
            int number = 0;
            if (content != null)
            {
                foreach (Variant v in content)
                {
                    string soleid = v.GetStringOrDefault("E");
                    if (string.IsNullOrEmpty(soleid))
                    {
                        number++;
                    }
                }
            }
            return number;
        }

        /// <summary>
        /// 得到指定商品的数量
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="goodsid"></param>
        /// <returns></returns>
        public static int BurdenGoodsCount(IList Content, string goodsid)
        {
            int number = 0;
            foreach (Variant d in Content)
            {
                if (d.GetStringOrDefault("G") == goodsid)
                {
                    number += d.GetIntOrDefault("A");
                }
            }
            return number;
        }

        /// <summary>
        /// 得到第一个空格
        /// </summary>
        /// <param name="Content"></param>
        /// <returns></returns>
        public static Variant GetBurdenSpace(IList Content)
        {
            foreach (Variant v in Content)
            {
                if (string.IsNullOrEmpty(v.GetStringOrDefault("E")))
                    return v;
            }
            return null;
        }

        /// <summary>
        /// 请先判断包袱是否足够存放道具,否则如果包袱满将不能再得到道具
        /// </summary>
        /// <param name="c"></param>
        /// <param name="goodsList"></param>
        /// <param name="issave">是否保存</param>
        public static Dictionary<string, int> BurdenBatchInsert(PlayerEx burden, Dictionary<string, Variant> goodsList, bool issave = true)
        {
            //成功加入道具数量
            try
            {
                Dictionary<string, int> dic = new Dictionary<string, int>(goodsList.Count);
                IList c = burden.Value.GetValue<IList>("C");

                foreach (var item1 in goodsList)
                {
                    GameConfig gc = GameConfigAccess.Instance.FindOneById(item1.Key);
                    if (gc == null)
                        continue;

                    int sc = GameConfigAccess.Instance.GetStactCount(item1.Key);
                    if (sc <= 0) break;

                    //表示得到道具
                    Variant v = goodsList[item1.Key] as Variant;
                    if (v == null) continue;
                    //int number = 0;
                    //int isbinding = 0;
                    for (int isbinding = 0; isbinding <= 1; isbinding++)
                    {
                        //0为非绑定,1为绑定
                        int number = v.GetIntOrDefault("Number" + isbinding);
                        if (number <= 0)
                            continue;
                        int num = 0;//当前分配数量
                        foreach (Variant m in c)
                        {
                            if (sc == 1)
                            {
                                #region 唯一道具
                                if (m.GetStringOrDefault("E") == string.Empty)
                                {
                                    Variant m_vc = gc.Value;
                                    //Variant m_ui = gc.UI;

                                    Variant cv = new Variant();
                                    foreach (var item in m_vc)
                                    {
                                        if (item.Key == "Affix")
                                        {
                                            Variant affix = new Variant();
                                            GetAffix((Variant)item.Value, affix);
                                            if (affix == null || affix.Count == 0)
                                                continue;
                                            //cv.Add(item.Key, affix);
                                            cv[item.Key] = affix;
                                        }
                                        else
                                        {
                                            cv[item.Key] = item.Value;
                                        }
                                    }

                                    //过期时间
                                    if (v.ContainsKey("EndTime"))
                                    {
                                        cv["EndTime"] = v.GetDateTimeOrDefault("EndTime");
                                    }
                    
                                    Goods g = Goods.Create();
                                    g.ID = ObjectId.GenerateNewId().ToString();
                                    g.GoodsID = gc.ID;
                                    g.Name = gc.Name;
                                    g.PlayerID = burden.PlayerID;
                                    g.Value = cv;
                                    g.Created = DateTime.UtcNow;
                                    g.Save();
                                    m["E"] = g.ID;
                                    m["G"] = gc.ID;
                                    m["S"] = gc.Value.GetIntOrDefault("Sort");
                                    m["H"] = isbinding;
                                    m["D"] = 0;
                                    m["A"] = 1;

                                    Variant tv = new Variant();
                                    if (v.ContainsKey("EndTime"))
                                    {
                                        //过期时间
                                        tv["EndTime"] = v.GetDateTimeOrDefault("EndTime");
                                    }
   
                                    if (tv.Count != 0)
                                    {
                                        m["T"] = tv;
                                    }
                                    num += 1;

                                    if (num == number)
                                        break;
                                }
                                #endregion
                            }
                            else
                            {
                                #region  可堆叠道具
                                if (m.GetStringOrDefault("E") == string.Empty)
                                {
                                    m["E"] = gc.ID;
                                    m["G"] = gc.ID;
                                    m["S"] = gc.Value.GetIntOrDefault("Sort");
                                    m["H"] = isbinding;
                                    m["D"] = 1;

                                    if ((number - num) >= sc)
                                    {
                                        m["A"] = sc;
                                        num += sc;
                                        //dic.SetOrInc(gc.ID, sc);
                                    }
                                    else
                                    {
                                        m["A"] = number - num;
                                        //dic.SetOrInc(gc.ID, number - num);
                                        num = number;
                                    }

                                    if (v.ContainsKey("EndTime"))
                                    {
                                        Variant vt = new Variant();
                                        vt.Add("EndTime", v.GetDateTimeOrDefault("EndTime"));
                                        m["T"] = vt;
                                    }
                                    else
                                    {
                                        m["T"] = null;
                                    }

                                    if (num == number)
                                        break;
                                }
                                else
                                {
                                    if (m.GetStringOrDefault("E") != gc.ID)
                                        continue;
                                    int a = m.GetIntOrDefault("A");
                                    if (a >= sc || m.GetIntOrDefault("H") != isbinding)
                                        continue;

                                    Variant t = m.GetValueOrDefault<Variant>("T");
                                    if (t != null)
                                    {
                                        if (v.ContainsKey("EndTime"))
                                        {
                                            if (t.GetDateTimeOrDefault("EndTime") != v.GetDateTimeOrDefault("EndTime"))
                                                continue;
                                        }
                                        else
                                        {
                                            if (t.ContainsKey("EndTime"))
                                                continue;
                                        }
                                    }

                                    if ((number - num + a) >= sc)
                                    {
                                        m["A"] = sc;
                                        //dic.SetOrInc(gc.ID, sc - a);
                                        num += (sc - a);
                                    }
                                    else
                                    {
                                        m["A"] = number - num + a;
                                        //dic.SetOrInc(gc.ID, (number - num + a));
                                        num = number;
                                    }
                                    if (num == number)
                                        break;
                                }
                                #endregion
                            }
                        }
                        if (num > 0)
                        {
                            if (dic.ContainsKey(gc.ID))
                            {
                                dic[gc.ID] += num;
                            }
                            else
                            {
                                dic.Add(gc.ID, num);
                            }
                        }
                    }
                }

                if (issave)
                {
                    burden.Save();
                }
                return dic;
            }
            catch (Exception e)
            {
                LogWrapper.Warn("BurdenBatchInsert:" + burden.PlayerID, e);
                return new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// 得到装被附加属性
        /// </summary>
        /// <param name="affix">附加属性</param>
        /// <param name="v">产生的附加属性</param>
        public static void GetAffix(Variant affix, Variant v)
        {
            if (affix == null)
                return;
            int min = affix.GetIntOrDefault("Min");
            int max = affix.GetIntOrDefault("Max");
            if (min > max)
                return;

            Variant lift = affix.GetVariantOrDefault("Life");

            if (lift == null)
                return;

            foreach (var item in lift)
            {
                if (v.ContainsKey(item.Key))
                    continue;

                Variant tmp = (Variant)item.Value;
                if (tmp == null)
                    continue;

                int m = tmp.GetIntOrDefault("Min");
                int n = tmp.GetIntOrDefault("Max");
                double range = tmp.GetDoubleOrDefault("Range");

                if (NumberRandom.RandomHit(range))
                {
                    //表示成功
                    v.Add(item.Key, NumberRandom.Next(m, n + 1));
                    if (v.Count >= max)
                        break;
                }
            }

            if (lift.Count > min && v.Count < min)
            {
                //回调
                GetAffix(affix, v);
            }
        }

        /// <summary>
        /// 判断包袱是否已经满了
        /// </summary>
        /// <param name="burdenlist">包袱[包袱名称,包袱基本信息]</param>
        /// <param name="number">物品数量[物品ID，数量]</param>
        /// <param name="IsBinding">物品绑定状态[物品ID,绑定状态]</param>
        /// <returns>true表示满,false表示没有满</returns>
        public static bool BurdenIsFull(Dictionary<string, PlayerEx> burdenlist, Dictionary<string, int> number, Dictionary<string, int> IsBinding)
        {
            Dictionary<string, GameConfig> goodsList = new Dictionary<string, GameConfig>();
            #region 得到物品信息
            foreach (string k in number.Keys)
            {
                GameConfig gc = GameConfigAccess.Instance.FindOneById(k);
                if (gc != null)
                {
                    goodsList.Add(gc.ID, gc);
                }
            }
            #endregion

            //<包袱名称,包袱空格子数量>
            Dictionary<string, int> burdenEmpty = new Dictionary<string, int>();
            #region 得到B0,B1,B3空格子数量
            foreach (string d in burdenlist.Keys)
            {
                int num = 0;
                PlayerEx ex = burdenlist[d];
                IList c = ex.Value.GetValue<IList>("C");
                foreach (Variant v in c)
                {
                    if (v["E"].ToString() == string.Empty)
                        num++;
                }
                burdenEmpty.Add(d, num);
            }
            #endregion

            //<包袱名称,物品要求占的空格子数量>
            Dictionary<string, int> needGrid = new Dictionary<string, int>();
            #region 商品所有物品占据空格子的数量
            //判断商品存放位置            
            foreach (string k in goodsList.Keys)
            {
                GameConfig gc = goodsList[k];
                string name = string.Empty;
                int m = 0;//商品堆叠数
                if (gc.MainType == "Pet")
                {
                    name = "B1";
                    m = 1;
                }
                else
                {
                    Variant sc = gc.Value["StactCount"] as Variant;
                    for (int i = 0; i < 4; i++)
                    {
                        name = "B" + i;
                        object o;
                        if (sc.TryGetValueT(name, out o) && (int)o > 0)
                        {
                            m = (int)o;
                            break;
                        }
                    }
                }
                //表示商品配置有问题
                if (m == 0 || name == string.Empty)
                    return true;

                PlayerEx p = burdenlist[name];
                IList c = p.Value.GetValue<IList>("C");

                int a = 0, grid = 0;//[a表示商品数量,grid表示占格子数]
                foreach (Variant con in c)
                {
                    if (con["G"].ToString() == gc.ID && Convert.ToInt32(con["H"]) == IsBinding[k])
                    {
                        a += Convert.ToInt32(con["A"]);
                        grid++;
                    }
                }
                if (m > 1)
                {
                    int l = 0;
                    if (grid * m < a + number[k])
                    {
                        int n = (number[k] + a) - (grid * m);
                        int y = 0;
                        int s = Math.DivRem(n, m, out y);
                        l = y > 0 ? s + 1 : s;
                    }
                    if (needGrid.ContainsKey(name))
                        needGrid[name] += l;
                    else
                        needGrid.Add(name, l);
                }
                else
                {
                    if (needGrid.ContainsKey(name))
                        needGrid[name] += number[k];
                    else
                        needGrid.Add(name, number[k]);
                }
            }
            #endregion

            foreach (string k in needGrid.Keys)
            {
                if (!burdenEmpty.ContainsKey(k))
                    return true;
                if (needGrid[k] > burdenEmpty[k])
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 批量插入物品信息
        /// </summary>
        /// <param name="burdens">包袱列表B0,B1,B3</param>
        /// <param name="number">物品数量</param>
        /// <param name="isbinding">物品绑定状态</param>
        /// <returns></returns>
        public static List<Variant> TaskGoodsInsert(Dictionary<string, PlayerEx> burdens, Dictionary<string, int> number, Dictionary<string, int> isbinding, string playerid)
        {
            List<Variant> goodsInfo = new List<Variant>();
            foreach (string d in number.Keys)
            {
                GameConfig gc = GameConfigAccess.Instance.FindOneById(d);
                if (gc.MainType == "Pet")
                    continue;
                #region 判断物品属于什么包袱同时得到堆叠数
                string name = string.Empty;
                int m = 0;

                Variant sc = gc.Value["StactCount"] as Variant;
                for (int i = 0; i < 4; i++)
                {
                    if (i == 2) continue;
                    string k = "B" + i;
                    object o;
                    if (sc.TryGetValueT(k, out o) && (int)o > 0)
                    {
                        name = k;
                        m = (int)o;
                        break;
                    }
                }

                #endregion

                if (m <= 0 || name == string.Empty)
                    continue;

                if (!burdens.ContainsKey(name))
                    continue;

                PlayerEx burden = burdens[name];
                IList c = burden.Value.GetValue<IList>("C");

                #region 需要存放的数量
                int cur = number[d];
                foreach (Variant con in c)
                {
                    if (cur == 0)
                        break;
                    if (m > 1)
                    {
                        if (con["G"].ToString() == string.Empty)
                        {
                            if (m <= cur)
                            {
                                con["E"] = gc.ID;
                                con["G"] = gc.ID;
                                con["S"] = gc.Value["Sort"];
                                con["H"] = isbinding[d];
                                con["D"] = m > 1 ? 1 : 0;
                                con["A"] = m;
                                con["T"] = gc.Value["GoodsType"];
                                cur -= m;
                            }
                            else
                            {
                                con["E"] = gc.ID;
                                con["G"] = gc.ID;
                                con["S"] = gc.Value["Sort"];
                                con["H"] = isbinding[d];
                                con["D"] = m > 1 ? 1 : 0;
                                con["A"] = cur;
                                con["T"] = gc.Value["GoodsType"];
                                cur = 0;
                                break;
                            }
                        }
                        else if (con["G"].ToString() == gc.ID && Convert.ToInt32(con["H"]) == isbinding[d] && Convert.ToInt32(con["A"]) < m)
                        {
                            if (cur + Convert.ToInt32(con["A"]) <= m)
                            {
                                con["A"] = Convert.ToInt32(con["A"]) + cur;
                                cur = 0;
                                break;
                            }
                            else
                            {
                                cur = cur + Convert.ToInt32(con["A"]) - m;
                                con["A"] = m;
                            }
                        }
                    }
                    else
                    {
                        //表示不能堆叠的物品
                        if (con["G"].ToString() == string.Empty)
                        {
                            Goods g = Goods.Create();
                            g.ID = ObjectId.GenerateNewId().ToString();
                            g.GoodsID = gc.ID;
                            g.Name = gc.Name;
                            g.PlayerID = playerid;

                            if (!gc.Value.ContainsKey("UI"))
                            {
                                gc.Value.Add("UI", gc.UI);
                            }
                            g.Value = gc.Value;

                            g.Created = DateTime.UtcNow;
                            g.Save();

                            con["E"] = g.ID;
                            con["G"] = gc.ID;
                            con["S"] = gc.Value["Sort"];
                            con["A"] = 1;
                            con["H"] = isbinding[d];
                            con["D"] = m > 1 ? 1 : 0;
                            con["T"] = gc.Value["GoodsType"];
                            cur -= 1;
                            if (cur == 0)
                                break;
                        }
                    }
                }

                #endregion

                burden.Save();

                Variant v = new Variant();
                v.Add("ID", gc.ID);
                v.Add("Name", gc.Name);
                v.Add("GoodsID", gc.ID);
                v.Add("Count", number[d]);
                v.Add("IsSet", 1);//1表示得到
                goodsInfo.Add(v);
            }
            return goodsInfo;
        }

        /// <summary>
        /// 道具堆叠数
        /// </summary>
        /// <param name="goodsid">道具堆叠数</param>
        /// <returns></returns>
        public static int StactCount(string goodsid)
        {
            GameConfig gc = GameConfigAccess.Instance.FindOneById(goodsid);
            return StactCount(gc);
        }
        /// <summary>
        /// 道具堆叠数
        /// </summary>
        /// <param name="v">道具堆叠数</param>
        /// <returns></returns>
        public static int StactCount(GameConfig gc)
        {
            if (gc == null) return 0;
            return gc.Value.GetIntOrDefault("StactCount");
        }

        /// <summary>
        /// 计算一个道具的数量G
        /// </summary>
        /// <param name="burden">包袱</param>
        /// <param name="goodsid">G道具ID</param>
        /// <returns></returns>
        public static int GoodsCount(PlayerEx burden, string goodsid)
        {
            int count = 0;
            IList c = burden.Value.GetValue<IList>("C");
            foreach (Variant k in c)
            {
                if (k.GetStringOrDefault("G") == goodsid)
                {
                    count += k.GetIntOrDefault("A");
                }
            }
            return count;
        }

        /// <summary>
        /// 判断有过期时间的物品数量
        /// </summary>
        /// <param name="burden">包袱</param>
        /// <param name="goodsid">道具ID</param>
        /// <param name="endtime">过期时间</param>
        /// <returns></returns>
        public static int GoodsCount(PlayerEx burden, string goodsid, DateTime endtime)
        {
            int count = 0;
            IList c = burden.Value.GetValue<IList>("C");
            foreach (Variant k in c)
            {
                Variant t = k["T"] as Variant;
                if (t != null)
                {
                    if (t.ContainsKey("EndTime"))
                    {
                        if (t.GetDateTimeOrDefault("EndTime") == endtime && k.GetStringOrDefault("G") == goodsid)
                        {
                            count += k.GetIntOrDefault("A");
                        }
                    }
                }
            }
            return count;
        }

 
        /// <summary>
        /// 判断需要的物品是否为绑定
        /// </summary>
        /// <param name="burden"></param>
        /// <param name="goodsid">物品</param>
        /// <param name="number">需要数量</param>
        /// <returns>0表示非绑定,1绑定</returns>
        public static int IsBinding(PlayerEx burden, string goodsid, int number)
        {

            if (string.IsNullOrEmpty(goodsid))
                return 1;
            Variant bv = burden.Value;
            if (bv == null)
                return 1;
            IList c = bv.GetValue<IList>("C");
            int n = 0;
            foreach (Variant v in c)
            {
                string gid = v.GetStringOrDefault("G");
                if (gid != goodsid)
                    continue;
                int a = v.GetIntOrDefault("A");
                int h = v.GetIntOrDefault("H");
                n += a;
                if (n < number && h == 1)
                {
                    return 1;
                }
                if (n >= a)
                {
                    return h;
                }
            }
            if (n < number)
                return 1;
            return 1;
        }

        /// <summary>
        /// 移除指定时间的物品
        /// </summary>
        /// <param name="burden"></param>
        /// <param name="goodsid">道具</param>
        /// <param name="endtime">结束时间</param>
        /// <param name="number">数量</param>
        /// <returns></returns>
        public static bool Remove(PlayerEx burden, string goodsid, DateTime endtime, int number)
        {
            IList c = burden.Value.GetValue<IList>("C");
            int m = 0;
            foreach (Variant v in c)
            {
                if (v.GetStringOrDefault("G") == goodsid)
                {
                    Variant t = v["T"] as Variant;
                    if (t == null) continue;
                    if (!t.ContainsKey("EndTime"))
                        continue;
                    if (t.GetDateTimeOrDefault("EndTime") != endtime)
                        continue;
                    int num = t.GetIntOrDefault("A");
                    if ((number - m) >= num)
                    {
                        BurdenClear(v);
                        m += num;
                    }
                    else
                    {
                        t["A"] = num - (number - m);
                        m = number;
                    }
                    if (m == number)
                        break;
                }
            }
            burden.Save();
            return true;
        }

        /// <summary>
        /// 移除一个指定类型的物品
        /// </summary>
        /// <param name="burden"></param>
        /// <param name="goodsid"></param>
        /// <returns></returns>
        public static bool Remove(PlayerEx burden, string goodsid)
        {
            return Remove(burden, goodsid, 1);
        }

        /// <summary>
        /// 移除道具
        /// </summary>
        /// <param name="burden">包袱</param>
        /// <param name="goodsid">物品</param>
        /// <param name="number">数量</param>
        /// <returns></returns>
        public static bool Remove(PlayerEx burden, string goodsid, int number)
        {
            if (string.IsNullOrEmpty(goodsid))
                return false;
            IList c = burden.Value.GetValue<IList>("C");
            int num = 0;

            int count = 0;
            foreach (Variant v in c)
            {
                if (v.GetStringOrDefault("G") == goodsid)
                {
                    count += v.GetIntOrDefault("A");
                }
            }

            //表示数量不足
            if (count < number)
                return false;

            foreach (Variant v in c)
            {
                if (v.GetStringOrDefault("G") != goodsid)
                    continue;

                if ((number - num) >= v.GetIntOrDefault("A"))
                {
                    num += v.GetIntOrDefault("A");
                    BurdenClear(v);
                }
                else
                {
                    v["A"] = v.GetIntOrDefault("A") - (number - num);
                    num = number;
                }
                if (num == number)
                    break;
            }
            burden.Save();
            return true;
        }

        /// <summary>
        /// 批量移除对应位置的道具,主要用来交易使用
        /// </summary>
        /// <param name="burden"></param>
        /// <param name="goods"></param>
        public static bool Remove(PlayerEx burden, IList goods)
        {
            IList c = burden.Value.GetValue<IList>("C");
            foreach (Variant d in goods)
            {
                for (int i = 0; i < c.Count; i++)
                {
                    Variant v = c[i] as Variant;
                    if (v == null)
                        continue;
                    if (d.GetIntOrDefault("P") == v.GetIntOrDefault("P"))
                    {
                        if (d.GetStringOrDefault("ID") != v.GetStringOrDefault("E"))
                        {
                            return false;
                        }

                        if (d.GetIntOrDefault("Count") != v.GetIntOrDefault("A"))
                        {
                            return false;
                        }

                        if (d.GetIntOrDefault("Count") == v.GetIntOrDefault("A"))
                        {
                            BurdenClear(v);
                        }
                        else if (v.GetIntOrDefault("Count") > d.GetIntOrDefault("A"))
                        {
                            v["A"] = v.GetIntOrDefault("Count") - d.GetIntOrDefault("A");
                        }
                    }
                }
            }
            burden.Save();
            return true;
        }

        /// <summary>
        /// 请先判断包袱道具是否满足,再进行批量移除道具
        /// </summary>
        public static void Remove(PlayerEx burden, Dictionary<string, int> goods, out int isbinding)
        {
            int isb = 0;
            if (burden != null && goods.Count != 0)
            {
                foreach (var item in goods)
                {
                    //移除道具数量
                    int number = goods[item.Key];
                    if (number <= 0)
                    {
                        isbinding = 1;
                        return;
                    }

                    IList c = burden.Value.GetValue<IList>("C");
                    int curr = number;//当前还要移除的数量
                    foreach (Variant v in c)
                    {
                        if (v.GetStringOrDefault("G") == item.Key)
                        {
                            int A = v.GetIntOrDefault("A");
                            if (A > curr)
                            {
                                v["A"] = A - curr;
                                curr = 0;
                                if (v.GetIntOrDefault("H") == 1)
                                {
                                    isb = 1;
                                }
                                break;
                            }
                            else
                            {
                                curr = curr - A;
                                if (v.GetStringOrDefault("E") != v.GetStringOrDefault("G"))
                                {
                                    GoodsAccess.Instance.RemoveOneById(v.GetStringOrDefault("E"), SafeMode.False);
                                }
                                if (v.GetIntOrDefault("H") == 1)
                                {
                                    isb = 1;
                                }
                                BurdenManager.BurdenClear(v);
                                if (curr == 0)
                                    break;
                            }
                        }
                    }
                }
                burden.Save();
            }
            isbinding = isb;

            //InvokeClientNote note = new InvokeClientNote(MessageCommand.InvokeClientByUserID, new object[] { isBinding });
            //note.IDList = new List<string> { burden.PlayerID };
            //note.Type = "方法名";
            //Notifier.Instance.Publish(note);
        }

        /// <summary>
        /// 移除所有指定道具
        /// </summary>
        /// <param name="burden"></param>
        /// <param name="goodsid"></param>
        public static int RemoveAll(PlayerEx burden, string goodsid)
        {
            IList c = burden.Value.GetValue<IList>("C");
            int count = 0;
            foreach (Variant v in c)
            {
                if (v.GetStringOrDefault("G") == goodsid)
                {
                    count += v.GetIntOrDefault("A");
                    BurdenClear(v);
                }
            }
            burden.Save();
            return count;
        }

        /// <summary>
        /// 移除过期的物品
        /// </summary>
        /// <param name="burden"></param>
        public static Dictionary<string, int> Remove(PlayerEx burden)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            IList c = burden.Value.GetValue<IList>("C");
            for (int i = 0; i < c.Count; i++)
            {
                Variant v = c[i] as Variant;
                if (v.GetStringOrDefault("E") == string.Empty)
                    continue;
                if (v.GetStringOrDefault("E") != v.GetStringOrDefault("G"))
                {
                    Goods g = GoodsAccess.Instance.FindOneById(v.GetStringOrDefault("E"));
                    if (g == null) continue;

                    Variant Timelines = g.Value.GetVariantOrDefault("TimeLines");
                    if (Timelines != null)
                    {
                        //永久有效0
                        //获得时计时1
                        //使用后计时2
                        if (Timelines.GetIntOrDefault("Type") == 0)
                            continue;
                        DateTime dt = DateTime.UtcNow;
                        TimeSpan ts = dt - g.Created;
                        if (ts.TotalHours >= Timelines.GetIntOrDefault("Hour"))
                        {
                            //表示物品已经过期，移除
                            BurdenManager.BurdenClear(v);
                            if (dic.ContainsKey(g.Name))
                            {
                                dic[g.Name] += v.GetIntOrDefault("A");
                            }
                            else
                            {
                                dic.Add(g.Name, v.GetIntOrDefault("A"));
                            }
                        }
                    }
                }
            }
            burden.Save();
            return dic;
        }

        /// <summary>
        /// 交易判断包袱能否装入下面物品
        /// </summary>
        /// <param name="burdens"></param>
        /// <param name="goods"></param>
        /// <returns>true表示包袱格子不够</returns>
        public static bool BurdenDealFull(PlayerEx burdens, IList goods)
        {
            IList c = burdens.Value.GetValue<IList>("C");
            if (BurdenSpace(c) >= goods.Count)
                return false;
            return true;
        }

        /// <summary>
        ///交易使用
        /// </summary>
        /// <param name="burden"></param>
        /// <param name="goods"></param>
        public static void BurdenInsert(PlayerEx burden, IList goods)
        {
            IList c = burden.Value.GetValue<IList>("C");
            //交易得到的物品
            foreach (Variant k in goods)
            {
                Variant v = GetBurdenSpace(c);
                //得到第一个空格子
                Goods g = GoodsAccess.Instance.FindOneById(k.GetStringOrDefault("ID"));
                if (g != null)
                {
                    v["E"] = g.ID;
                    v["G"] = g.GoodsID;
                    v["S"] = g.Value.GetIntOrDefault("Sort");
                    v["H"] = 0;
                    v["A"] = 1;
                    v["D"] = 0;
                    v["T"] = CreateT(g);
                    g.PlayerID = burden.PlayerID;
                    g.Save();
                }
                else
                {
                    GameConfig gc = GameConfigAccess.Instance.FindOneById(k.GetStringOrDefault("ID"));
                    if (gc == null)
                        continue;
                    v["E"] = gc.ID;
                    v["G"] = gc.ID;
                    v["S"] = gc.Value.GetIntOrDefault("Sort");
                    v["H"] = 0;
                    v["A"] = k.GetIntOrDefault("Count");
                    v["D"] = 1;
                    v["T"] = null;
                }
            }
            burden.Save();
        }

        /// <summary>
        /// 移除一件物品[0]
        /// </summary>
        /// <param name="burdens">物品所在包袱</param>
        /// <param name="p">物品所在位置</param>
        /// <param name="isall">true表示移除整个格子,false表示移除1个</param>
        public static bool RemoveGoods(PlayerEx burdens, int p, bool isall, out string goodsid,out int number)
        {
            IList c = burdens.Value.GetValue<IList>("C");
            foreach (Variant k in c)
            {                
                if (k.GetIntOrDefault("P") == p)
                {
                    string soleid = k.GetStringOrDefault("E");
                    if (string.IsNullOrEmpty(soleid))
                    {
                        goodsid = "";
                        number = 0;
                        return false;
                    }
                    string goods = k.GetStringOrDefault("G");
                    int num = k.GetIntOrDefault("A");
                    if (num <= 0) 
                    {
                        //数量不足
                        goodsid = "";
                        number = 0;
                        return false;
                    }

                    if (soleid != goods)
                    {
                        BurdenClear(k);
                        burdens.Save();

                        goodsid = goods;
                        number = 1;      
                        GoodsAccess.Instance.Remove(soleid, burdens.PlayerID);
                        return true;
                    }
                    else 
                    {
                        //批量使用或物品只剩1个的时候
                        if (isall || num == 1)
                        {
                            BurdenClear(k);
                            burdens.Save();

                            goodsid = goods;
                            number = num;
                            return true;
                        }
                        else
                        {
                            k["A"] = num - 1;
                            burdens.Save();
                            goodsid = goods;
                            number = 1;
                            return true;
                        }
                    }

                    //if (isall)
                    //{
                    //    goodsid = gid;
                    //    number = num;
                    //    BurdenClear(d);
                    //    burdens.Save();

                    //    if (sid != gid) 
                    //    {
                    //        GoodsAccess.Instance.Remove(sid, burdens.PlayerID);
                    //    }
                    //    return true;
                    //}
                    //else
                    //{
                    //    if (num == 1)
                    //    {
                    //        if (sid != gid)
                    //        {
                    //            GoodsAccess.Instance.Remove(sid, burdens.PlayerID);
                    //        }
                    //        goodsid = gid;
                    //        number = 1;
                    //        BurdenClear(d);
                    //        burdens.Save();
                    //        return true;
                    //    }
                    //    else if (num > 1)
                    //    {
                    //        d["A"] = num - 1;
                    //        burdens.Save();
                    //        goodsid = gid;
                    //        number = 1;
                    //        return true;
                    //    }
                    //    goodsid = "";
                    //    number = 0;
                    //    return false;
                    //}
                }
            }
            goodsid = "";
            number = 0;
            return false;
        }

        /// <summary>
        /// 得到对应包袱位置的信息
        /// </summary>
        /// <param name="burden"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Variant BurdenPlace(PlayerEx burden, int p)
        {
            IList c = burden.Value.GetValue<IList>("C");
            Variant tmp = null;
            foreach (Variant v in c)
            {
                if (v.GetIntOrDefault("P") == p)
                {
                    tmp = v;
                    break;
                }
            }
            return tmp;
        }

        /// <summary>
        /// 得到包袱信息
        /// </summary>
        /// <param name="burden"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Variant BurdenPlace(PlayerEx burden, string id)
        {
            IList c = burden.Value.GetValue<IList>("C");
            Variant tmp = null;
            foreach (Variant v in c)
            {
                if (v.GetStringOrDefault("E") == id)
                {
                    tmp = v;
                    break;
                }
            }
            return tmp;
        }

        /// <summary>
        /// 创建道具变更信息
        /// </summary>
        /// <param name="gc"></param>
        /// <returns></returns>
        public static Variant CreateT(Goods g)
        {
            Variant v = g.Value;
            Variant msg = new Variant();
            //if (v.ContainsKey("Stamina"))
            //{
            //    Variant Stamina = v["Stamina"] as Variant;
            //    msg.Add("Stamina", Stamina["V"]);
            //}
            //if (v.ContainsKey("BaoShiInfo"))
            //{
            //    Variant bs = v["BaoShiInfo"] as Variant;
            //    foreach (string p in bs.Keys)
            //    {
            //        if (msg.ContainsKey(p))
            //            continue;
            //        msg.Add(p, bs[p]);
            //    }
            //}
            //if (v.ContainsKey("PetsWild"))
            //{
            //    //野性值
            //    msg.Add("PetsWild", v["PetsWild"]);
            //}
            //msg.Add("Created", g.Created);
            if (v.ContainsKey("EndTime"))
            {
                msg.Add("EndTime", v["EndTime"]);
            }
            return msg;
        }
    }
}
