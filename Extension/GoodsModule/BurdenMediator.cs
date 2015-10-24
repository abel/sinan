using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Sinan.Command;
using Sinan.Core;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Util;
using MongoDB.Bson;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.GoodsModule
{
    /// <summary>
    /// 包袱或仓库拖动,堆叠，扩展操作
    /// </summary>
    sealed public class BurdenMediator : AysnSubscriber
    {
        private static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();
        public override IList<string> Topics()
        {
            return new string[]
            {
                UserPlayerCommand.CreatePlayerSuccess,
                LoginCommand.PlayerLoginSuccess,
                BurdenCommand.BurdenExtend,
                BurdenCommand.BurdenList,
                BurdenCommand.BurdenDrag,
                BurdenCommand.BurdenFinishing,
                BurdenCommand.BurdenSplit,
                BurdenCommand.BurdenOut,
                BurdenCommand.SaveShowInfo
            };
        }
        public override void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;

            // 需验证玩家登录的操作.....
            if (note.Name == UserPlayerCommand.CreatePlayerSuccess)
            {
                //创建角色时得到初始化道具
                CreateGoods(note);
                return;
            }
            if (note != null)
            {
                ExecuteNote(note);
            }
        }

        void ExecuteNote(UserNote note)
        {
            // 需验证玩家登录的操作.....
            if (note.Player == null) return;

            switch (note.Name)
            {

                case LoginCommand.PlayerLoginSuccess:
                    //CreateBurden(note);                    
                    break;
                case BurdenCommand.BurdenList:
                    BurdenList(note, note.GetString(0));
                    break;

                case BurdenCommand.BurdenExtend:
                    BurdenExtend(note);
                    break;

                case BurdenCommand.BurdenFinishing:
                    BurdenFinishing(note);
                    break;

                case BurdenCommand.BurdenDrag:
                    BurdenDrag(note);
                    break;
                case BurdenCommand.BurdenOut:
                    BurdenOut(note);
                    break;

                case BurdenCommand.SaveShowInfo:
                    SaveShowInfo(note);
                    break;
            }
        }
        /// <summary>
        /// 判断角色是否存在包袱与仓库
        /// </summary>
        /// <param name="note"></param>
        private void CreateBurden(UserNote note)
        {
            for (int i = 0; i < 2; i++)
            {
                string key = "B" + i;
                if (!note.Player.Value.ContainsKey(key))
                {
                    int max = 0;
                    int cur = 0;
                    switch (i)
                    {
                        case 0:
                            max = 64;
                            cur = 64;
                            break;
                        case 1:
                            max = 105;
                            cur = 35;
                            break;
                    }

                    PlayerEx p = new PlayerEx(note.PlayerID, key);
                    p.Value = new Variant();
                    Variant v = p.Value;
                    v.Add("Max", max);
                    v.Add("Cur", cur);
                    List<Variant> list = new List<Variant>();
                    for (int j = 0; j < v.GetIntOrDefault("Cur"); j++)
                    {
                        Variant d = new Variant();
                        d.Add("P", j);
                        d.Add("E", string.Empty);
                        d.Add("G", string.Empty);//道具ID
                        d.Add("A", 0);
                        d.Add("S", 0);//排序
                        d.Add("H", 0);//0非绑定,1绑定是否绑定
                        d.Add("D", 0);//0不能堆叠,1可以
                        d.Add("T", null);//物品类型 
                        list.Add(d);
                    }
                    v.Add("C", list);
                    note.Player.Value.Add(key, p);
                    p.Save();
                }
            }
        }

        /// <summary>
        /// 创建角色时得到道具
        /// </summary>
        /// <param name="note"></param>
        private void CreateGoods(UserNote note)
        {
            PlayerBusiness player = note[0] as PlayerBusiness;
            PlayerEx burden = player.B0;
            string goodsid = RoleManager.Instance.GetRoleConfig(player.RoleID, "Goods");
            Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
            string[] goods = goodsid.Split('|');
            for (int i = 0; i < goods.Length; i++)
            {
                string[] strs = goods[i].Split(',');
                if (strs.Length != 2)
                    continue;
                int number = 0;
                if (!int.TryParse(strs[1], out number))
                    continue;

                Variant v = new Variant();
                v.Add("Number1", number);
                dic.Add(strs[0], v);
            }
            BurdenManager.BurdenBatchInsert(burden, dic);
        }

        /// <summary>
        /// 得到包袱与仓库列表
        /// </summary>
        /// <param name="note"></param>
        private void BurdenList(UserNote note, string info)
        {
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            //sw.Start();
            PlayerBusiness pb = note.Player;
            string[] key = info.Split(',');
            Variant v = new Variant();
            for (int i = 0; i < key.Length; i++)
            {
                string name = key[i];
                if (string.IsNullOrEmpty(name))
                    continue;
                PlayerEx bx;
                if (pb.Value.TryGetValueT(name, out bx) && bx != null)
                {
                    v[name] = bx;
                }
            }
            pb.Call(BurdenCommand.BurdenListR, v);
            //sw.Stop();
            //Console.WriteLine("ElapsedMilliseconds:" + sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// 包袱或仓库扩展[还要求判断是否要求晶币消费]
        /// </summary>
        /// <param name="note"></param>
        /// <param name="exname">包袱或仓库名</param>
        /// <param name="count">扩展数量</param>
        private void BurdenExtend(UserNote note)
        {
            PlayerEx burden = note.Player.B1;
            if (burden != null)
            {
                Variant v = burden.Value;
                //最大值
                int max = v.GetIntOrDefault("Max");
                //当前数量
                int cur = v.GetIntOrDefault("Cur");
                if (cur >= max)
                {
                    note.Call(BurdenCommand.BurdenExtendR, false, TipManager.GetMessage(BurdenReturn.BurdenMaxEx));
                    return;
                }  

                int number = note.GetInt32(0);
                string goodsid = "";
                double lv = GameConfigAccess.Instance.FindExtend(ExtendType.B1, cur.ToString(), out number, out goodsid);


                if (string.IsNullOrEmpty(goodsid) || number <= 0)
                {
                    note.Call(PetsCommand.ShockSkillR, false, TipManager.GetMessage(ExtendReturn.B12));
                    return;
                }

                if (!note.Player.RemoveGoods(goodsid, number, GoodsSource.BurdenExtend))
                {
                    note.Call(PetsCommand.ShockSkillR, false, TipManager.GetMessage(ExtendReturn.B12));
                    return;
                }

                note.Player.UpdateBurden();
                if (NumberRandom.RandomHit(lv))
                {
                    v["Cur"] = cur + 7;
                    IList c = v.GetValue<IList>("C");
                    for (int i = cur; i < cur + 7; i++)
                    {
                        Variant d = new Variant();
                        d["P"] = i;
                        d["E"] = "";
                        d["G"] = "";
                        d["A"] = 0;
                        d["S"] = 0;
                        d["H"] = 0;
                        d["D"] = 0;
                        d["T"] = null;
                        c.Add(d);
                    }

                    burden.Save();
                    note.Call(BurdenCommand.BurdenExtendR, true, TipManager.GetMessage(BurdenReturn.Success));
                    note.Player.FinishNote(FinishCommand.ExtendLib);
                }
                else
                {
                    note.Call(PetsCommand.ShockSkillR, false, TipManager.GetMessage(ExtendReturn.B13));
                }
            }
        }

        /// <summary>
        /// 整理操作
        /// </summary>
        /// <param name="note"></param>
        private void BurdenFinishing(UserNote note)
        {
            string sid = note.PlayerID + "BurdenFinishing";
            if (!m_dic.TryAdd(sid, sid))
            {
                return;
            }
            try
            {
                string[] keys = note.GetString(0).Split(',');
                
                List<string> mn = new List<string>() 
                { 
                    "B0",  "B1" 
                };

                Variant b = new Variant();
                for (int i = 0; i < keys.Length; i++)
                {
                    if (!mn.Contains(keys[i]))
                        continue;

                    PlayerEx bx = note.Player.Value[keys[i]] as PlayerEx;
                    if (bx == null)
                        return;
                    Variant bv = bx.Value;
                    if (bv == null)
                        return;

                    IList c = bv.GetValue<IList>("C");

                    SortedDictionary<int, SortedDictionary<string, int>> dic = new SortedDictionary<int, SortedDictionary<string, int>>();
                    foreach (Variant m in c)
                    {
                        //道具ID
                        string soleid = m.GetStringOrDefault("E");
                        if (string.IsNullOrEmpty(soleid))
                            continue;
                        string goodsid = m.GetStringOrDefault("G");
                        int sort = m.GetIntOrDefault("S");
                        int isbinding = m.GetIntOrDefault("H");
                        int number = m.GetIntOrDefault("A");
                        //过期时间
                        string endtime = "";
                        if (m["T"] != null)
                        {
                            Variant t = m["T"] as Variant;
                            //判断道具是否存在过期时间
                            if (t.ContainsKey("EndTime"))
                            {
                                endtime = t.GetLocalTimeOrDefault("EndTime").ToString();
                            }
                        }

                        string id = goodsid + "|" + isbinding + "|" + soleid + "|" + endtime;

                        SortedDictionary<string, int> info;
                        if (dic.TryGetValue(-sort, out info))
                        {
                            if (info.ContainsKey(id))
                            {
                                info[id] += number;
                            }
                            else
                            {
                                info.Add(id, number);
                            }
                        }
                        else
                        {
                            info = new SortedDictionary<string, int>();
                            info.Add(id, number);
                            dic.Add(-sort, info);
                        }
                    }

                    if (dic.Count > 0)
                    {
                        //清理包袱列表
                        for (int m = 0; m < c.Count; m++)
                        {
                            Variant cv = c[m] as Variant;
                            if (string.IsNullOrEmpty(cv.GetStringOrDefault("E")))
                                continue;
                            BurdenManager.BurdenClear(cv);
                        }
                    }


                    foreach (int k in dic.Keys)
                    {
                        SortedDictionary<string, int> info = dic[k];
                        foreach (string key in info.Keys)
                        {
                            string[] sr = key.Split('|');
                            int num = info[key];
                            if (num <= 0) 
                                continue;
                            if (sr[0] == sr[2])
                            {
                                Dictionary<string, Variant> goods = new Dictionary<string, Variant>();
                                Variant v = new Variant();
                                v.Add("Number" + sr[1], num);
                                if (!string.IsNullOrEmpty(sr[3]))
                                {
                                    DateTime dt;
                                    if (DateTime.TryParse(sr[3], out dt))
                                    {
                                        v.Add("EndTime", dt.ToUniversalTime());
                                    }
                                }
                                goods.Add(sr[0], v);
                                BurdenManager.BurdenBatchInsert(bx, goods, false);
                            }
                            else
                            {
                                //得到空格
                                Variant tmp = BurdenManager.GetBurdenSpace(c);
                                tmp["H"] = sr[1];
                                tmp["E"] = sr[2];
                                tmp["G"] = sr[0];
                                tmp["A"] = 1;
                                tmp["S"] = -k;
                                tmp["D"] = 0;

                                Variant t = new Variant();
                                if (!string.IsNullOrEmpty(sr[3]))
                                {
                                    DateTime dt;
                                    if (DateTime.TryParse(sr[3], out dt))
                                    {
                                        t.Add("EndTime", dt.ToUniversalTime());
                                    }                                    
                                }
                                tmp["T"] = t;
                            }
                        }
                    }
                    bx.Save();
                    b.Add(keys[i], bx);
                }
                note.Call(BurdenCommand.BurdenListR, b);               
            }
            finally
            {
                m_dic.TryRemove(sid, out sid);
            }
        }

        /// <summary>
        /// 拖动拆分操作
        /// </summary>
        /// <param name="note"></param>
        private void BurdenDrag(UserNote note)
        {
            string soleid = note.PlayerID + "BurdenDrag";
            if (!m_dic.TryAdd(soleid, soleid))
            {
                return;
            }

            try
            {
                string b0 = note.GetString(0);
                string b1 = note.GetString(2);
                if (string.IsNullOrEmpty(b0) || string.IsNullOrEmpty(b1))
                {
                    note.Call(BurdenCommand.BurdenDragR, false, TipManager.GetMessage(BurdenReturn.BurdenDrag1));
                    return;
                }
                List<string> lt = new List<string>() { "B0", "B1" };
                if (!lt.Contains(b0))
                    return;
                if (!lt.Contains(b1))
                    return;

                int p0 = note.GetInt32(1);
                int p1 = note.GetInt32(3);
                int num = note.GetInt32(4);
                if (num <= 0)
                    return;
                
                Variant v0 = null;
                Variant v1 = null;

                PlayerEx burden0 = note.Player.Value[b0] as PlayerEx;
                PlayerEx burden1 = note.Player.Value[b1] as PlayerEx;
                if (burden0 == null || burden1 == null)
                {
                    note.Call(BurdenCommand.BurdenDragR, false, TipManager.GetMessage(BurdenReturn.BurdenDrag1));
                    return;
                }


                IList c0 = burden0.Value.GetValue<IList>("C");
                foreach (Variant d in c0)
                {
                    if (d.GetIntOrDefault("P") == p0)
                    {
                        v0 = d;
                        break;
                    }
                }

                if (v0 == null)
                {
                    note.Call(BurdenCommand.BurdenDragR, false, TipManager.GetMessage(BurdenReturn.BurdenDrag2));
                    return;
                }

                IList c1 = burden1.Value.GetValue<IList>("C");
                foreach (Variant d in c1)
                {
                    if (d.GetIntOrDefault("P") == p1)
                    {
                        v1 = d;
                        break;
                    }
                }


                if (v1 == null)
                {
                    note.Call(BurdenCommand.BurdenDragR, false, TipManager.GetMessage(BurdenReturn.BurdenDrag2));
                    return;
                }

                if (b0 == b1 && p0 == p1)
                {
                    note.Call(BurdenCommand.BurdenDragR, false, TipManager.GetMessage(BurdenReturn.NoSame));
                    return;
                }

                if (b0 == "B0" && b1 == "B1")
                {
                    GameConfig m_gc = GameConfigAccess.Instance.FindOneById(v0.GetStringOrDefault("G"));
                    if (m_gc != null)
                    {
                        //任务道具不能放入仓库中
                        if (m_gc.Value.GetStringOrDefault("GoodsType") == "112006")
                        {
                            note.Call(BurdenCommand.BurdenDragR, false, TipManager.GetMessage(BurdenReturn.TaskGoods));
                            return;
                        }
                    }
                }


                string pid0 = v0.GetStringOrDefault("E");

                string pid1 = v1.GetStringOrDefault("E");

                if (string.IsNullOrEmpty(pid0))
                {
                    //起始格子数据不正确
                    note.Call(BurdenCommand.BurdenDragR, false, TipManager.GetMessage(BurdenReturn.NoStartGrid));
                    return;
                }
                int n = v0.GetIntOrDefault("A");
                if (num > n)
                {
                    //起始格子数量小于想折分数量,数据不正确
                    note.Call(BurdenCommand.BurdenDragR, false, TipManager.GetMessage(BurdenReturn.NoNumber));
                    return;
                }

                if (!string.IsNullOrEmpty(pid1))
                    return;

                if (b0 == b1)
                {
                    //if (string.IsNullOrEmpty(pid1))
                    //{
                    if (num == n)
                    {
                        v1["E"] = v0["E"];
                        v1["G"] = v0["G"];
                        v1["S"] = v0["S"];
                        v1["A"] = v0["A"];
                        v1["H"] = v0["H"];
                        v1["D"] = v0["D"];
                        v1["T"] = v0["T"];
                        BurdenManager.BurdenClear(v0);
                    }
                    else if (num < n)
                    {
                        //分拆包袱中的道具数量
                        v0["A"] = (n - num);

                        v1["E"] = v0["E"];
                        v1["G"] = v0["G"];
                        v1["S"] = v0["S"];
                        v1["A"] = num;
                        v1["H"] = v0["H"];
                        v1["D"] = v0["D"];
                        v1["T"] = v0["T"];
                    }
                    else
                    {
                        note.Call(BurdenCommand.BurdenDragR, false, TipManager.GetMessage(BurdenReturn.NoNumber));
                        return;
                    }
                    //}
                    //else
                    //{
                    //    return;
                    //    //不相同就交换位置
                    //    //BurdenJiaoHuan(v0, v1);
                    //}
                }
                else
                {

                    //必须目标格子为空
                    //if (!string.IsNullOrEmpty(pid1))
                    //{
                    //    note.Call(BurdenCommand.BurdenDragR, false, TipManager.GetMessage(BurdenReturn.NoNumber));
                    //    return;
                    //}

                    v1["E"] = v0["E"];
                    v1["G"] = v0["G"];
                    v1["S"] = v0["S"];
                    v1["A"] = v0["A"];
                    v1["H"] = v0["H"];
                    v1["D"] = v0["D"];
                    v1["T"] = v0["T"];
                    BurdenManager.BurdenClear(v0);
                }

                burden0.Save();
                burden1.Save();

                Variant list = new Variant();
                if (b0 == b1)
                {
                    list.Add(b0, burden0);
                }
                else
                {
                    list.Add(b0, burden0);
                    list.Add(b1, burden1);
                }

                note.Call(BurdenCommand.BurdenListR, list);


                string goodsid = string.Empty;
                if ((b0 == "B0" || b1 == "B0") && v1 != null)
                {
                    //表示普通包袱减少
                    goodsid = v1.GetStringOrDefault("G");
                }



                if (!string.IsNullOrEmpty(goodsid) && note.Player.TaskGoods.Contains(goodsid))
                {
                    //通知任务系统
                    note.Player.UpdateTaskGoods(goodsid);
                }
            }
            finally
            {                
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 交换
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        private void BurdenJiaoHuan(Variant v0, Variant v1)
        {
            Variant tmp = new Variant();
            tmp.Add("E", v0["E"]);
            tmp.Add("G", v0["G"]);
            tmp.Add("A", v0["A"]);
            tmp.Add("S", v0["S"]);
            tmp.Add("H", v0["H"]);
            tmp.Add("D", v0["D"]);
            tmp.Add("T", v0["T"]);


            v0["E"] = v1["E"];
            v0["G"] = v1["G"];
            v0["S"] = v1["S"];
            v0["A"] = v1["A"];
            v0["H"] = v1["H"];
            v0["D"] = v1["D"];
            v0["T"] = v1["T"];


            v1["E"] = tmp["E"];
            v1["G"] = tmp["G"];
            v1["S"] = tmp["S"];
            v1["A"] = tmp["A"];
            v1["H"] = tmp["H"];
            v1["D"] = tmp["D"];
            v1["T"] = tmp["T"];
        }

        /// <summary>
        /// 将仓库中取出道具        
        /// </summary>
        /// <param name="note"></param>
        private void BurdenOut(UserNote note)
        {
            string soleid = note.PlayerID + "BurdenOut";
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                int p = note.GetInt32(0);
                string name = note.GetString(1);
                if (string.IsNullOrEmpty(name))
                {
                    note.Call(BurdenCommand.BurdenOutR, false, TipManager.GetMessage(BurdenReturn.BurdenOut1));
                    return;
                }

                List<string> ls = new List<string>() { "B0", "B1" };
                if (!ls.Contains(name))
                {
                    note.Call(BurdenCommand.BurdenOutR, false, TipManager.GetMessage(BurdenReturn.BurdenOut1));
                    return;
                }
                //源
                PlayerEx b1 = name == "B0" ? note.Player.B0 : note.Player.B1;
                Variant v = BurdenManager.BurdenPlace(b1, p);
                if (v == null)
                {
                    note.Call(BurdenCommand.BurdenOutR, false, TipManager.GetMessage(BurdenReturn.BurdenOut1));
                    return;
                }

                string goodsid = v.GetStringOrDefault("E");
                if (string.IsNullOrEmpty(goodsid))
                {
                    note.Call(BurdenCommand.BurdenOutR, false, TipManager.GetMessage(BurdenReturn.BurdenOut2));
                    return;
                }

                //目标
                PlayerEx b0 = name == "B0" ? note.Player.B1 : note.Player.B0;
                IList c = b0.Value.GetValue<IList>("C");
                Variant m = BurdenManager.GetBurdenSpace(c);
                if (m == null)
                {
                    note.Call(BurdenCommand.BurdenOutR, false, TipManager.GetMessage(BurdenReturn.BurdenOut3));
                    return;
                }

                m["E"] = v["E"];
                m["G"] = v["G"];
                m["A"] = v["A"];
                m["S"] = v["S"];
                m["D"] = v["D"];
                m["H"] = v["H"];
                m["T"] = v["T"];
                BurdenManager.BurdenClear(v);
                b0.Save();
                b1.Save();

                Variant tmp = new Variant();
                tmp.Add("B0", note.Player.B0);
                tmp.Add("B1", note.Player.B1);
                note.Player.UpdateTaskGoods(m.GetStringOrDefault("G"));

                note.Call(BurdenCommand.BurdenListR, tmp);
                note.Call(BurdenCommand.BurdenOutR, true, string.Empty);
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }


        /// <summary>
        /// 更新显示信息
        /// </summary>
        /// <param name="note"></param>
        private void SaveShowInfo(UserNote note)
        {
            int showInfo = note.GetInt32(0);
            note.Player.ShowInfo = showInfo;
            PlayerAccess.Instance.SaveValue(note.Player.PID, "ShowInfo", showInfo);
        }
    }
}
