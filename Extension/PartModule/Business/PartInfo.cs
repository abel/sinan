using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Data;
using Sinan.Util;
using Sinan.Command;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.PartModule.Business
{
   
    class PartInfo
    {
        static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 充值活动
        /// </summary>
        /// <param name="note"></param>
        public static void Recharge(UserNote note)
        {
            //充值成功
            int coin = note.GetInt32(0);
            if (coin <= 0)
                return;

            List<Variant> list = MallAccess.CoinPart();
            foreach (Variant gc in list)
            {
                Variant v = gc.GetVariantOrDefault("Value");
                if (v == null)
                    continue;
                DateTime dt = DateTime.UtcNow;
                if (v.ContainsKey("OpenTime"))
                {
                    //开服天数
                    int day = v.GetIntOrDefault("OpenTime");

                    //开服时间
                    DateTime zep = ConfigLoader.Config.ZoneEpoch.ToUniversalTime();

                    //表示活动还没有开始
                    if (zep > dt)
                        continue;
                    //表示活动已经结束
                    if (zep.AddDays(day) < dt)
                        continue;
                }
                else
                {
                    DateTime startTime = v.GetDateTimeOrDefault("StartTime");
                    //表示活动还没有开始
                    if (startTime > dt)
                        continue;
                    DateTime endTime = v.GetDateTimeOrDefault("EndTime");
                    //表示活动已经结束
                    if (endTime < dt)
                        continue;
                }

                string id = gc.GetStringOrDefault("ID");
                string name = gc.GetStringOrDefault("Name");
                string subType = gc.GetStringOrDefault("SubType");

                if (subType == PartType.CoinSupp || subType == PartType.CoinAchieve)
                {
                    string soleid = note.PlayerID + id;
                    PartBase model = PartAccess.Instance.FindOneById(soleid);
                    if (model == null)
                    {
                        model = new PartBase();
                        model.ID = soleid;
                        model.Name = name;
                        model.SubType = subType;
                        model.Created = DateTime.UtcNow;
                        model.PlayerID = note.PlayerID;
                        model.PartID = id;
                        model.Value = new Variant();
                    }
                    //晶币充值总数
                    model.Value.SetOrInc("Total", coin);
                    model.Value.SetOrInc("Cur", coin);
                    model.Save();
                }
            }
        }


        /// <summary>
        /// 活动相关兑换
        /// </summary>
        /// <param name="note"></param>
        public static void PartExchange(UserNote note)
        {
            string sole = note.PlayerID + "PartExchange";
            if (!m_dic.TryAdd(sole, sole))
                return;
            try
            {
                string partid = note.GetString(0);
                //Part part = PartManager.Instance.FindOne(partid);
                Variant part = MallAccess.PartInfo(partid);
                if (part == null)
                {
                    //该活动不存在
                    note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part1));
                    return;
                }
                Variant v = part.GetVariantOrDefault("Value");
                if (v == null)
                {
                    note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part1));
                    return;
                }
                DateTime dt = DateTime.UtcNow;

                if (v.ContainsKey("OpenTime"))
                {
                    //开服天数
                    int day = v.GetIntOrDefault("OpenTime");
                    if (day <= 0)
                        return;
                    //开服时间
                    DateTime zep = ConfigLoader.Config.ZoneEpoch.ToUniversalTime();
                    //表示活动还没有开始
                    if (zep > dt)
                    {
                        //该活动还没有开始不能兑奖
                        note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part2));
                        return;
                    }
                    DateTime ote = zep.AddDays(day);
                    //表示活动已经结束
                    if (ote < dt)
                    {
                        //表示活动领奖时间已经结束,不能再领奖
                        note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part3));
                        return;
                    }
                }
                else
                {
                    if (v.ContainsKey("StartTime"))
                    {
                        DateTime startTime = v.GetDateTimeOrDefault("StartTime");

                        //表示活动还没有开始
                        if (startTime > dt)
                        {
                            //该活动还没有开始不能兑奖
                            note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part2));
                            return;
                        }
                    }


                    if (v.ContainsKey("EndTime") || v.ContainsKey("Cutoff"))
                    {
                        DateTime endTime = v.ContainsKey("Cutoff") ? v.GetDateTimeOrDefault("Cutoff") : v.GetDateTimeOrDefault("EndTime");
                        //表示活动领奖时间已经结束
                        if (endTime < dt)
                        {
                            //表示活动领奖时间已经结束,不能再领奖
                            note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part3));
                            return;
                        }
                    }
                }

                string subtype = part.GetStringOrDefault("SubType");

                switch (subtype)
                {
                    case PartType.CoinSupp:
                    case PartType.CoinAchieve:
                    case PartType.LevelAchieve:
                        PartAward(note, part);
                        break;
                    case PartType.LevelRank:
                    case PartType.LevelSupp:
                        PlayerRank(note, part);
                        break;
                    case PartType.Exchange:
                        Exchange(note, part);
                        break;
                    case PartType.Yellow:
                        Yellow(note, part);
                        break;
                    case PartType.NowAward:
                        NowAward(note, part);
                        break;
                }
            }
            finally
            {
                m_dic.TryRemove(sole, out sole);
            }
        }

        /// <summary>
        /// 得到活动领取情况
        /// </summary>
        /// <param name="note"></param>
        public static void PartReceive(UserNote note)
        {
            string partid = note.GetString(0);
            //Part part = PartManager.Instance.FindOne(pardid);

            Variant part = MallAccess.PartInfo(partid ?? string.Empty);
            if (part == null)
            {
                note.Call(PartCommand.PartReceiveR, partid, null);
                return;
            }

            string soleid = "";
            string subtype = part.GetStringOrDefault("SubType");
            switch (subtype)
            {

                case PartType.Yellow:
                    soleid = note.Player.UserID + partid;
                    break;
                case PartType.CoinAchieve:
                case PartType.CoinSupp:
                case PartType.LevelAchieve:
                    soleid = note.PlayerID + partid;
                    break;

            }

            if (subtype == PartType.LevelSupp)
            {
                int oldpid;
                Variant config = part.GetVariantOrDefault("Value");
                string tableName = config.GetStringOrDefault("TableName");
                if (string.IsNullOrEmpty(tableName))
                    return;
                note.Call(PartCommand.PartReceiveR, partid, PlayerSortAccess.Instance.PlayerOld(note.Player.UserID, tableName, out oldpid));
                return;
            }

            if (subtype == PartType.LevelRank)
            {
                //冠军排名赛
                note.Call(PartCommand.PartReceiveR, partid, PlayerSortAccess.Instance.GetAwardMsg(note.Player));
                return;
            }

            if (!string.IsNullOrEmpty(soleid))
            {
                PartBase model = PartAccess.Instance.FindOneById(soleid);
                if (model != null)
                {
                    note.Call(PartCommand.PartReceiveR, partid, new Detail.PartDetail(model));
                    return;
                }
            }
            note.Call(PartCommand.PartReceiveR, partid, null);
        }

        /// <summary>
        /// 充值奖励
        /// </summary>
        /// <param name="note"></param>
        /// <param name="part"></param>
        private static void PartAward(UserNote note, Variant part)
        {
            string msg = note.GetString(1);
            int index = note.GetInt32(2);

            string id = part.GetStringOrDefault("ID");
            string name = part.GetStringOrDefault("Name");
            string subtype = part.GetStringOrDefault("SubType");

            string soleid = note.PlayerID + id;
            //取得到活动信息
            PartBase model = PartAccess.Instance.FindOneById(soleid);
            if (model == null)
            {
                model = new PartBase();
                model.ID = note.PlayerID + id;
                model.Name = name;
                model.SubType = subtype;
                model.Created = DateTime.UtcNow;
                model.PlayerID = note.PlayerID;
                model.PartID = id;
                model.Value = new Variant();
            }
            Variant info = model.Value;
            int cur = 0;
            int num = 0;

            if (subtype == PartType.CoinAchieve || subtype == PartType.CoinSupp)
            {
                num = Convert.ToInt32(msg);
                cur = info.GetIntOrDefault("Cur");
                if (cur < num)
                {
                    //活动其间充值金额不足
                    note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part4));
                    return;
                }

                //不能重复领取
                if (subtype == PartType.CoinAchieve)
                {
                    if (info.ContainsKey(msg))
                    {
                        //该项活动已经领取，不能重复领取
                        note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part5));
                        return;
                    }
                }
            }


            string rs = note.Player.RoleID + note.Player.Sex;

            Variant config = part.GetVariantOrDefault("Value");
            if (config == null)
            {
                //活动配置数据不正确
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }

            Variant award = config.GetVariantOrDefault("Award");

            if (award == null)
            {
                //活动配置数据不正确
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }
            Variant tmp = null;
            //选择的奖励
            if (subtype == PartType.LevelAchieve)
            {
                if (info.Count > 0)
                {
                    //该项活动已经领取，不能重复领取
                    note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part5));
                    return;
                }

                foreach (var item in award)
                {
                    string[] strs = item.Key.Split('-');
                    int min = Convert.ToInt32(strs[0]);
                    int max = Convert.ToInt32(strs[1]);
                    if (note.Player.Level >= min && note.Player.Level <= max)
                    {
                        tmp = item.Value as Variant;
                        break;
                    }
                }
            }
            else
            {
                tmp = award.GetVariantOrDefault(msg);
            }

            if (tmp == null)
            {
                //活动配置数据不正确
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }


            IList select = tmp.GetValue<IList>(rs);
            if (select == null)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }
            Variant goods = select[index] as Variant;
            if (goods == null)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }
            Dictionary<string, Variant> dic = PartAccess.Instance.GetPartAward(goods);
            if (dic.Count == 0)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }

            if (BurdenManager.IsFullBurden(note.Player.B0, dic))
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part7));
                return;
            }
            if (subtype == PartType.CoinSupp)
            {
                info.SetOrInc("Cur", -num);
                //记录兑换次数
                info.SetOrInc(msg, 1);
            }
            else
            {
                info[msg] = DateTime.UtcNow;
            }
            GoodsSource gs;
            if (subtype == PartType.CoinSupp)
            {
                gs = GoodsSource.CoinSupp;
            }
            else if (subtype == PartType.CoinAchieve)
            {
                gs = GoodsSource.CoinAchieve;
            }
            else
            {
                gs = GoodsSource.LevelAchieve;
            }

            if (model.Save())
            {
                Dictionary<string, int> gos = note.Player.AddGoods(dic, gs);
                note.Call(PartCommand.PartExchangeR, true, TipManager.GetMessage(PartReturn.Part15));
                //活动通告
                PartApprise(note, gos, tmp.GetStringOrDefault("T"), tmp.GetStringOrDefault("M"));
            }
            else
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part8));
            }
        }

        /// <summary>
        /// 等级排行(冠军赛),老区用户奖励
        /// </summary>
        /// <param name="note"></param>
        /// <param name="part"></param>
        private static void PlayerRank(UserNote note, Variant part)
        {
            string msg = note.GetString(1);
            int index = note.GetInt32(2);

            string rs = note.Player.RoleID + note.Player.Sex;

            string subtype = part.GetStringOrDefault("SubType");

            Variant config = part.GetVariantOrDefault("Value");
            if (config == null)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }

            Variant award = null;
            Variant tmp = null;
            int oldpid = 0;
            int ranking = 0;

            string tableName = "";
            if (subtype == PartType.LevelSupp)
            {
                //tableName = "PlayerOld";
                tableName = config.GetStringOrDefault("TableName");
                //判断是不是老服用户
                ranking = PlayerSortAccess.Instance.PlayerOld(note.Player.UserID, tableName, out oldpid);
                if (ranking == 0)
                {
                    //非老区用户不能领取该奖励
                    note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part9));
                    return;
                }
                else if (ranking == -1)
                {
                    //该奖励已经领取，不能重复领取
                    note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part5));
                    return;
                }
                award = config.GetVariantOrDefault("Award");
                foreach (var item in award)
                {
                    string[] strs = item.Key.Split('-');
                    int min = Convert.ToInt32(strs[0]);
                    int max = Convert.ToInt32(strs[1]);
                    if (ranking >= min && ranking <= max)
                    {
                        tmp = item.Value as Variant;
                        msg = item.Key;
                        break;
                    }
                }
            }
            else
            {
                List<string> awardType = new List<string>() 
                { 
                    "MonthAward",
                    "WeekAward", 
                    "DayAward" 
                };

                if (!awardType.Contains(msg))
                {
                    note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part10));
                    return;
                }

                award = config.GetVariantOrDefault(msg);

                if (award == null)
                {
                    note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part10));
                    return;
                }

                string str = PlayerSortAccess.Instance.GetTableName(msg);

                if (string.IsNullOrEmpty(str))
                {
                    note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part10));
                    return;
                }

                tableName = "Player" + str;
                if (PlayerSortAccess.Instance.IsAward(note.Player, tableName, msg))
                {
                    //该项活动奖励已经参加
                    note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part5));
                    return;
                }
                //得到排行
                ranking = PlayerSortAccess.Instance.GetMyRank(note.Player, tableName);
                if (ranking <= 0)
                {
                    note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part10));
                    return;
                }

                foreach (var item in award)
                {
                    string[] strs = item.Key.Split('-');
                    int min = Convert.ToInt32(strs[0]);
                    int max = Convert.ToInt32(strs[1]);
                    if (ranking >= min && ranking <= max)
                    {
                        tmp = item.Value as Variant;
                        break;
                    }
                }
            }

            if (tmp == null)
            {
                //没有达到领励排行
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part10));
                return;
            }


            IList select = tmp.GetValue<IList>(rs);
            if (select == null)
            {
                //活动配置数据不正确
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }
            Variant goods = select[index] as Variant;
            if (goods == null)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }
            Dictionary<string, Variant> dic = PartAccess.Instance.GetPartAward(goods);
            if (dic.Count == 0)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }

            if (BurdenManager.IsFullBurden(note.Player.B0, dic))
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part7));
                return;
            }

            //冠军排行
            if (subtype == PartType.LevelRank)
            {
                string t = award.GetStringOrDefault("T");
                string m = award.GetStringOrDefault("M");
                if (PlayerSortAccess.Instance.UpdateInfo(note.Player, tableName, msg))
                {
                    Dictionary<string, int> gos = note.Player.AddGoods(dic, GoodsSource.LevelRank);
                    note.Call(PartCommand.PartExchangeR, true, TipManager.GetMessage(PartReturn.Part15));
                    PartApprise(note, gos, t, m);
                    return;
                }
            }

            //老区用户活动
            if (subtype == PartType.LevelSupp)
            {
                string t = tmp.GetStringOrDefault("T");
                string m = tmp.GetStringOrDefault("M");
                if (PlayerSortAccess.Instance.UpdateOld(oldpid, tableName))
                {
                    Dictionary<string, int> gos = note.Player.AddGoods(dic, GoodsSource.LevelSupp);
                    note.Call(PartCommand.PartExchangeR, true, TipManager.GetMessage(PartReturn.Part15));
                    PartApprise(note, gos, t, m);
                    return;
                }
            }
            //没有达到领励条件
            note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part13));
        }

        /// <summary>
        /// 物品兑换
        /// </summary>
        /// <param name="note"></param>
        /// <param name="part"></param>
        private static void Exchange(UserNote note, Variant part)
        {
            string msg = note.GetString(1);
            int index = note.GetInt32(2);

            string id = part.GetStringOrDefault("ID");
            string name = part.GetStringOrDefault("Name");
            string subtype = part.GetStringOrDefault("SubType");

            string soleid = note.PlayerID + id;
            //取得到活动信息
            PartBase model = PartAccess.Instance.FindOneById(soleid);
            if (model == null)
            {
                model = new PartBase();
                model.ID = note.PlayerID + id;
                model.Name = name;
                model.SubType = subtype;
                model.Created = DateTime.UtcNow;
                model.PlayerID = note.PlayerID;
                model.PartID = id;
                model.Value = new Variant();
            }
            Variant info = model.Value;

            string rs = note.Player.RoleID + note.Player.Sex;

            Variant config = part.GetVariantOrDefault("Value");
            if (config == null)
            {
                //活动配置数据不正确
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }

            Variant award = config.GetVariantOrDefault("Award");

            if (award == null)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }
            Variant tmp = award.GetVariantOrDefault(msg);
            //选择的奖励


            if (tmp == null)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }


            IList select = tmp.GetValue<IList>(rs);
            if (select == null)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }
            Variant goods = select[index] as Variant;
            if (goods == null)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }
            Dictionary<string, Variant> dic = PartAccess.Instance.GetPartAward(goods);
            if (dic.Count == 0)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }
            PlayerEx b0 = note.Player.B0;
            if (BurdenManager.IsFullBurden(b0, dic))
            {
                //包袱满不能领取
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part7));
                return;
            }


            string goodsid = config.GetStringOrDefault("GoodsID");
            int number = Convert.ToInt32(msg);
            int total = BurdenManager.GoodsCount(b0, goodsid);
            if (total < number)
            {
                //兑换需要的道具数量不足
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part12));
                return;
            }
            if (note.Player.RemoveGoods(goodsid, number, GoodsSource.Exchange))
            {

                //总共兑换次数
                info.SetOrInc(msg, 1);
                string t = award.GetStringOrDefault("T");
                string m = award.GetStringOrDefault("M");
                if (model.Save())
                {
                    Dictionary<string, int> gos = note.Player.AddGoods(dic, GoodsSource.Exchange);
                    note.Call(PartCommand.PartExchangeR, true, TipManager.GetMessage(PartReturn.Part15));
                    PartApprise(note, gos, t, m);
                }
                else
                {
                    //没有达到兑换条件
                    note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part13));
                }

            }
            else
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part12));
            }
        }

        /// <summary>
        /// 黄钻奖励
        /// </summary>
        /// <param name="note"></param>
        /// <param name="part"></param>
        /// <returns></returns>
        private static void Yellow(UserNote note, Variant part)
        {
            //note.Player.Yellow = 81;
            string msg = note.GetString(1);

            string id = part.GetStringOrDefault("ID");
            string name = part.GetStringOrDefault("Name");
            string subtype = part.GetStringOrDefault("SubType");

            string soleid = note.Player.UserID + id;
            PartBase model = PartAccess.Instance.FindOneById(soleid);
            if (model == null)
            {
                model = new PartBase();
                model.ID = soleid;
                model.Name = name;
                model.SubType = subtype;
                model.Created = DateTime.UtcNow;
                model.PlayerID = note.Player.UserID;
                model.PartID = id;
                model.Value = new Variant();
            }
            Variant info = model.Value;

            DateTime dt = DateTime.UtcNow;

            Variant v = part.GetVariantOrDefault("Value");
            if (v == null)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }
            Variant award = v.GetVariantOrDefault("Award");
            if (award == null)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }
            if (note.Player.Yellow <= 0)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part14));
                return;
            }

            if (msg == "n")
            {
                //表示领取年费奖励
                if (note.Player.Yellow % 10 != 1)
                {
                    note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part16));
                    return;
                }
            }
            else
            {
                //int lv = 0;
                //if (!int.TryParse(msg, out lv))
                //{
                //    note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part11));
                //    return;
                //}                
                //得到黄钻等级
                msg = (note.Player.Yellow / 10).ToString();
            }

            if (info.ContainsKey(msg))
            {
                DateTime awardTime = info.GetDateTimeOrDefault(msg);
                //表示今日已经领取
                if (awardTime.ToLocalTime().Date == dt.ToLocalTime().Date)
                {
                    note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part5));
                    return;
                }
            }

            IList select = award.GetValue<IList>(msg);

            if (select == null)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part14));
                return;
            }

            Variant goods = select[0] as Variant;

            if (goods == null)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part14));
                return;
            }

            Dictionary<string, Variant> dic = PartAccess.Instance.GetPartAward(goods);
            if (dic.Count == 0)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }
            PlayerEx b0 = note.Player.B0;
            if (BurdenManager.IsFullBurden(b0, dic))
            {
                //包袱满不能领取
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part7));
                return;
            }

            info[msg] = dt;
            string t = award.GetStringOrDefault("T");
            string m = award.GetStringOrDefault("M");
            if (model.Save())
            {
                Dictionary<string, int> gos = note.Player.AddGoods(dic, GoodsSource.Yellow);
                note.Call(PartCommand.PartExchangeR, true, TipManager.GetMessage(PartReturn.Part15));
                PartApprise(note, gos, t, m, false);
            }
            else
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part14));
            }
        }

        /// <summary>
        /// 领取奖励
        /// </summary>
        /// <param name="note"></param>
        /// <param name="part">活动</param>
        private static void NowAward(UserNote note, Variant part)
        {
            int index = note.GetInt32(2);
            string soleid = note.PlayerID + part.GetStringOrDefault("ID");
            PartBase model = PartAccess.Instance.FindOneById(soleid);
            if (model == null)
            {
                model = new PartBase();
                model.ID = soleid;
                model.Name = part.GetStringOrDefault("Name");
                model.SubType = part.GetStringOrDefault("SubType");
                model.Created = DateTime.UtcNow;
                model.PlayerID = note.PlayerID;
                model.PartID = part.GetStringOrDefault("ID");
                model.Value = new Variant();
            }

            Variant v = part.GetVariantOrDefault("Value");
            if (v == null)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }
            DateTime dt = DateTime.Now;
            string name = dt.ToString("yyyyddMM");
            if (model.Value.ContainsKey(name))
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part5));
                return;
            }
            Variant dayaward = v.GetVariantOrDefault("DayAward");
            if (dayaward == null)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }
            Variant info = null;
            foreach (var tt in dayaward) 
            {
                string[] strs = tt.Key.Split('-');
                int min = Convert.ToInt32(strs[0]);
                int max = Convert.ToInt32(strs[1]);
                if (note.Player.Level >= min && note.Player.Level <= max) 
                {
                    info = tt.Value as Variant;
                    break;
                }
            }
            if (info == null)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }

            IList ls = info.GetValue<IList>(note.Player.RoleID + "" + note.Player.Sex);
            if (ls == null)
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part6));
                return;
            }
            Variant goods = ls[index] as Variant;
            

            
            Dictionary<string, Variant> dic = PartAccess.Instance.GetPartAward(goods);

            PlayerEx b0 = note.Player.B0;
            if (BurdenManager.IsFullBurden(b0, dic))
            {
                //包袱满不能领取
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part7));
                return;
            }

            model.Value[name] = dt;
            string t = dayaward.GetStringOrDefault("T");
            string m = dayaward.GetStringOrDefault("M");
            if (model.Save())
            {
                Dictionary<string, int> gos = note.Player.AddGoods(dic, GoodsSource.Yellow);
                note.Call(PartCommand.PartExchangeR, true, TipManager.GetMessage(PartReturn.Part15));
                PartApprise(note, gos, t, m, false);
            }
            else
            {
                note.Call(PartCommand.PartExchangeR, false, TipManager.GetMessage(PartReturn.Part14));
            }
            
        }
        /// <summary>
        /// 活动通知
        /// </summary>
        /// <param name="note"></param>
        /// <param name="dic">得到的道具</param>
        /// <param name="t">显示位置</param>
        /// <param name="m">内容</param>
        /// <param name="isall">是否所有有都发送</param>
        private static void PartApprise(UserNote note, Dictionary<string, int> dic, string t, string m, bool isall = true)
        {
            if (string.IsNullOrEmpty(t) || string.IsNullOrEmpty(m))
                return;

            StringBuilder sb = new StringBuilder();
            foreach (var item in dic)
            {
                var gc = GameConfigAccess.Instance.FindOneById(item.Key);
                if (gc == null)
                    continue;
                sb.Append(gc.Name);
                sb.Append(",");
            }

            string msg = string.Format(m, sb.ToString().Trim(','));
            if (isall)
            {
                if (msg != null)
                {
                    PlayersProxy.CallAll(ClientCommand.SendActivtyR, new object[] { t, msg });
                }
            }
            else
            {
                note.Call(ClientCommand.SendActivtyR, new object[] { t, msg });
            }
        }
    }
}
