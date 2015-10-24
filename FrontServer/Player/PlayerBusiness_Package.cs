using System;
using System.Collections.Generic;
using MongoDB.Bson;
using Sinan.Data;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Observer;
using Sinan.Util;
using Sinan.Log;

namespace Sinan.FrontServer
{
    partial class PlayerBusiness
    {
        /// <summary>
        /// 添加物品,包袱是否已经满各模块自己检查
        /// </summary>
        /// <param name="goods">道具</param>
        /// <param name="source">来源</param>
        /// <param name="remark"></param>
        /// <param name="count">
        /// 商城购卖时，记录消费晶币,骨币,战绩数量
        /// 也可以存其它类型
        /// </param>
        /// <returns></returns>
        public Dictionary<string, int> AddGoods(Dictionary<string, Variant> goods, GoodsSource source, string remark = null, int count = 0)
        {
            Dictionary<string, int> v = BurdenManager.BurdenBatchInsert(m_b0, goods);
            if (v == null || v.Count == 0)
            {
                return v;
            }
            UpdateBurden();

            foreach (var item in v)
            {
                PlayerLog log = new PlayerLog(ServerLogger.zoneid, Actiontype.AddGoods);
                log.itemcnt = item.Value;
                log.itemtype = item.Key;
                log.reserve_1 = (int)source;
                log.remark = remark;
                log.reserve_2 = count;
                this.WriteLog(log);
                UpdateTaskGoods(item.Key);
            }

            var scene = this.Scene;
            if (scene != null && this.Online && source != GoodsSource.GMGet)
            {
                foreach (string gid in v.Keys)
                {
                    //珍稀物品发送通知:
                    string position = RareGoodsManager.Instance.GetMsgTo(gid);
                    if (!string.IsNullOrEmpty(position))
                    {
                        string msg = RareGoodsManager.Instance.GetMsg(scene.Name, this.Name, gid, source);
                        if (msg != null)
                        {
                            PlayersProxy.CallAll(ClientCommand.SendActivtyR, new object[] { position, msg });
                        }
                    }
                }
            }
            return v;
        }

        /// <summary>
        /// 添加1个非绑定的物品
        /// </summary>
        /// <param name="goodsid">道具ID</param>
        /// <returns></returns>
        public bool AddGoodsNobingOne(string goodsid, GoodsSource source = GoodsSource.None)
        {
            Dictionary<string, Variant> dic = new Dictionary<string, Variant>(1);
            Variant v = new Variant(1);
            v.Add("Number0", 1);
            dic.Add(goodsid, v);
            Dictionary<string, int> list = AddGoods(dic, source);
            return list.Count > 0;
        }

        /// <summary>
        /// 添加1个非绑定的物品
        /// </summary>
        /// <param name="goodsid"></param>
        /// <param name="endTime"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool AddGoodsNobingOne(string goodsid, DateTime endTime, GoodsSource source = GoodsSource.None)
        {
            Dictionary<string, Variant> dic = new Dictionary<string, Variant>(1);
            Variant v = new Variant(2);
            v.Add("Number0", 1);
            v.Add("EndTime", endTime);
            dic.Add(goodsid, v);
            Dictionary<string, int> list = AddGoods(dic, source);
            return list.Count > 0;
        }

        /// <summary>
        /// 移除指定道具
        /// </summary>
        /// <param name="goodsid">道具ID</param>
        /// <returns></returns>
        public bool RemoveGoods(string goodsid,GoodsSource gs)
        {
            if (BurdenManager.Remove(m_b0, goodsid))
            {
                UpdateTaskGoods(goodsid);
                AddLog(Actiontype.GoodsUse, goodsid, 1, gs, "", 0);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 移除指定数量道具
        /// </summary>
        /// <param name="goodsid">道具ID</param>
        /// <param name="number">数量</param>
        /// <returns></returns>
        public bool RemoveGoods(string goodsid, int number, GoodsSource gs)
        {
            if (BurdenManager.Remove(m_b0, goodsid, number))
            {
                UpdateTaskGoods(goodsid);
                AddLog(Actiontype.GoodsUse, goodsid, number, gs, "", 0);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 移除指定位置的道具
        /// </summary>
        /// <param name="p">格子位置</param>
        /// <param name="isall">false移除一个，true使用该格子中所有道具</param>
        /// <returns></returns>
        public bool RemoveGoods(int p, GoodsSource gs, bool isall = false)
        {
            string goodsid;
            int number = 0;//移除数量
            if (BurdenManager.RemoveGoods(m_b0, p, isall, out goodsid, out number))
            {
                if (string.IsNullOrEmpty(goodsid) || number <= 0)
                    return false;
                UpdateTaskGoods(goodsid);
                AddLog(Actiontype.GoodsUse, goodsid, number, gs, "", 0);
                return true;
            }
            return false;
        }


        /// <summary>
        /// 移除指定时间的道具
        /// </summary>
        /// <param name="goodsid">道具</param>
        /// <param name="endtime">时间</param>
        /// <param name="number">数量</param>
        /// <returns></returns>
        public bool RemoveGoods(string goodsid, DateTime endtime, int number, GoodsSource gs)
        {
            if (BurdenManager.Remove(m_b0, goodsid, endtime, number))
            {
                UpdateTaskGoods(goodsid);
                AddLog(Actiontype.GoodsUse, goodsid, number, gs, "", 0);
                return true;
            }
            return false;
        }


        /// <summary>
        /// 移除指定的所有道具
        /// </summary>
        /// <param name="goodsid"></param>
        /// <returns></returns>
        public int RemoveGoodsAll(string goodsid,GoodsSource gs)
        {
            int count = BurdenManager.RemoveAll(m_b0, goodsid);
            if (count > 0)
            {
                UpdateTaskGoods(goodsid);
                AddLog(Actiontype.GoodsUse, goodsid, count, gs, "", 0);
            }
            return count;
        }

        /// <summary>
        /// 移除装备在角色身上的坐骑与时装
        /// </summary>
        /// <param name="goodsid"></param>
        /// <param name="number"></param>
        /// <param name="gs"></param>
        /// <returns></returns>
        public bool RemoveEquips(string goodsid, GoodsSource gs) 
        {
            AddLog(Actiontype.GoodsUse, goodsid, 1, gs, "", 0);
            return true;
        }


        /// <summary>
        /// 每日活跃度
        /// </summary>
        /// <param name="at"></param>
        /// <param name="count"></param>
        public void AddAcivity(ActivityType at, int count)
        {
            PlayerEx ac;
            if (!Value.TryGetValueT("Activity", out ac))
                return;
            Variant v = ac.Value;
            if (v == null) return;
            List<string> list = ActivityManager.ActivityList((int)at);
            if (list.Count > 0)
            {
                Variant fv = v.GetValueOrDefault<Variant>("Finish");
                foreach (string key in list)
                {
                    if (fv.ContainsKey(key))
                    {
                        fv[key] = fv.GetIntOrDefault(key) + count;
                    }
                }
                ac.Save();
                Call(ClientCommand.UpdateActorR, new PlayerExDetail(ac));
            }
        }

        /// <summary>
        /// 得到或者减少道具通知任务系统
        /// </summary>
        /// <param name="goodsid">道具</param>
        public void UpdateTaskGoods(string goodsid)
        {
            if (string.IsNullOrEmpty(goodsid))
                return;
            if (TaskGoods.Contains(goodsid))
            {
                //通知任务系统
                UserNote note = new UserNote(this, TaskCommand.TaskGoods, new object[] { goodsid });
                Notifier.Instance.Publish(note);
            }
        }

        /// <summary>
        /// 是否存在VIP,判断是不是VIP
        /// </summary>
        /// <param name="m">判断是否是vip</param>
        /// <returns></returns>
        public void IsVIP()
        {
            PlayerEx isVIP;
            if (this.Value.TryGetValueT("VIPBase", out isVIP))
            {
                Variant v = isVIP.Value;
                if (v.GetDateTimeOrDefault("EndTime") > DateTime.UtcNow)
                {
                    this.VIP = 1;
                }
                else
                {
                    this.VIP = 0;
                }
            }
            else
            {
                this.VIP = 0;
            }
            PlayerAccess.Instance.SaveValue(_id, new Tuple<string, BsonValue>("VIP", this.VIP));
        }

        /// <summary>
        /// 更新包袱(B0)
        /// </summary>
        public void UpdateBurden()
        {
            Variant list = new Variant(1);
            list.Add("B0", m_b0);
            Call(BurdenCommand.BurdenListR, list);
        }

        public void UseGoodsR(bool scuess, string msg)
        {
            this.Call(GoodsCommand.UseGoodsR, scuess, msg);
            if (scuess)
            {
                Variant list = new Variant(1);
                list.Add("B0", m_b0);
                Call(BurdenCommand.BurdenListR, list);
            }
        }

        /// <summary>
        /// 更新包袱
        /// B0表示更新包袱,B1仓库,B2家园,B3兽栏
        /// </summary>
        public void UpdateBurden(string f, params string[] bs)
        {
            Variant list = new Variant(1);
            list.Add(f, this.Value.GetValue<object>(f));
            if (bs != null)
            {
                foreach (var m in bs)
                {
                    list.Add(m, this.Value.GetValue<object>(m));
                }
            }
            Call(BurdenCommand.BurdenListR, list);
        }



        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="t">操作类型</param>
        /// <param name="itemtype"></param>
        /// <param name="itemcnt"></param>
        /// <param name="source">来源</param>
        /// <param name="remark">备注</param>
        /// <param name="reserve_2"></param>
        public void AddLog(Actiontype t, string itemtype, int itemcnt, GoodsSource source, string remark, int reserve_2)
        {
            PlayerLog log = new PlayerLog(ServerLogger.zoneid, t);
            log.itemtype = itemtype;
            log.itemcnt = itemcnt;
            log.reserve_1 = (int)source;
            log.remark = remark;
            log.reserve_2 = reserve_2;
            
            this.WriteLog(log);
        }

        /// <summary>
        /// 动态日志记录
        /// </summary>
        /// <param name="t">类型</param>
        /// <param name="us">使用道具数量</param>
        /// <param name="gs">得到的道具</param>        
        /// <param name="os">其它</param>
        public void AddLogVariant(Actiontype t, Variant us, Variant gs, Variant os)
        {
            Variant info = new Variant();
            if (us != null && us.Count > 0)
            {
                info["UseGoods"] = us;
            }
            if (gs != null && gs.Count > 0)
            {
                info["GetGoods"] = gs;
            }  
            if (os != null && os.Count > 0)
            {
                foreach (var item in os)
                {
                    info[item.Key] = item.Value;
                }
            }

            LogVariant log = new LogVariant(ServerLogger.zoneid, t);
            log.Value = info;
            WriteLog(log);
        }
    }
}