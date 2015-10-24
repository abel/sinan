using System;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Util;
using MongoDB.Bson;

namespace Sinan.GoodsModule
{
    partial class GoodsMediator : AysnSubscriber
    {
        #region 使用限制检查

        /// <summary>
        /// 使用者等级限制
        /// </summary>
        /// <param name="player"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        private static int CheckLevel(Player player, Variant limit)
        {
            int needLev = limit.GetIntOrDefault("LevelRequire");
            if (needLev > player.Level)
            {
                return GoodsReturn.NoLevel;
            }
            return 0;
        }

        /// <summary>
        /// 性别限制
        /// </summary>
        /// <param name="player"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        private static int CheckSex(Player player, Variant limit)
        {
            if (limit.ContainsKey("Sex"))
            {
                int sex = Convert.ToInt32(limit["Sex"]);
                if (sex != 2 && sex != player.Sex)
                {
                    return GoodsReturn.NoSex;
                }
            }
            return 0;
        }

        /// <summary>
        /// 角色限制
        /// </summary>
        /// <param name="player"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        private static int CheckRole(Player player, Variant limit)
        {
            if (limit.ContainsKey("RoleID"))
            {
                string RoleID = limit.GetStringOrDefault("RoleID");
                if (RoleID != "0" && RoleID != player.RoleID)
                {
                    return GoodsReturn.NoRoleID;
                }
            }
            return 0;
        }

        /// <summary>
        /// 限时使用
        /// </summary>
        /// <param name="player"></param>
        /// <param name="limit">限制条件</param>
        /// <param name="created">开始计时时间</param>
        /// <returns></returns>
        private static int CheckTimeLines(Player player, Goods g)
        {
            Variant TimeLines = g.Value.GetVariantOrDefault("TimeLines");
            //表示没有时间限制
            if (TimeLines != null && TimeLines.GetIntOrDefault("Type") > 0)
            {
                //Type 永久有效0
                //获得时计时1
                //使用后计时2
                DateTime dt = DateTime.UtcNow;
                TimeSpan ts = dt - g.Created;
                if (ts.TotalHours >= TimeLines.GetIntOrDefault("Hour"))
                {
                    //表示道具已经过期不能使用
                    //IsDestroy是否消失
                    return GoodsReturn.TimeLines;
                }
            }
            return 0;
        }

        /// <summary>
        /// 场景限制
        /// </summary>
        /// <param name="player"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        private static int CheckScene(Player player, Variant limit)
        {
            Variant sceneLimit = limit.GetVariantOrDefault("Scene");
            if (sceneLimit != null)
            {
                string needScene = sceneLimit.GetStringOrDefault("SceneID");
                if (!string.IsNullOrEmpty(needScene))
                {
                    if (player.SceneID != needScene)
                    {
                        return GoodsReturn.NoSex;
                    }
                    #region 区域限制
                    //Variant r = sceneLimit.GetVariantOrDefault("P");
                    //if (r != null)
                    //{
                    //    int x = r.GetIntOrDefault("X");
                    //    int y = r.GetIntOrDefault("Y");
                    //    int w = r.GetIntOrDefault("W");
                    //    int h = r.GetIntOrDefault("H");
                    //    if (note.Player.X < x || note.Player.Y < y || note.Player.X > x + w || note.Player.Y > y + h)
                    //    {
                    //         player.UseGoodsR( false, TipAccess.GetMessage(GoodsReturn.NoSex));
                    //        return false;
                    //    }
                    //}
                    #endregion
                }
            }
            return 0;
        }

        /// <summary>
        /// 物品使用时间限制
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private static int DateLimit(PlayerBusiness player, Variant p)
        {
            Variant v = BurdenManager.BurdenPlace(player.B0, p.GetIntOrDefault("P"));
            if (v == null)
            {
                return GoodsReturn.DateLimit1;
            }
            string goodsid0 = p.GetStringOrDefault("E");
            string goodsid1 = v.GetStringOrDefault("E");
            if (string.IsNullOrEmpty(goodsid1))
            {
                return GoodsReturn.DateLimit2;
            }
            if (string.IsNullOrEmpty(goodsid0))
            {
                return GoodsReturn.DateLimit3;
            }
            if (goodsid0 != goodsid1)
            {
                return GoodsReturn.DateLimit4;
            }

            if (v.ContainsKey("T"))
            {
                //判断物品是否过期
                Variant t = v["T"] as Variant;
                if (t != null && t.ContainsKey("EndTime"))
                {
                    if (t.GetDateTimeOrDefault("EndTime") <= DateTime.UtcNow)
                    {
                        return GoodsReturn.DateLimit5;
                    }
                }
            }
            return 0;
        }
        #endregion

        /// <summary>
        /// 装备使用限制
        /// </summary>
        /// <param name="player"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        private static int DressLimit(Player player, Goods g)
        {
            Variant limit = g.Value.GetVariantOrDefault("UserLimit");
            if (limit == null) return 0;

            int fail = CheckRole(player, limit);
            if (fail != 0)
            {
                return fail;
            }
            fail = CheckSex(player, limit);
            if (fail != 0)
            {
                return fail;
            }
            fail = CheckLevel(player, limit);
            if (fail != 0)
            {
                return fail;
            }
            fail = CheckTimeLines(player, g);
            if (fail != 0)
            {
                return fail;
            }
            return 0;
        }

        /// <summary>
        /// 补充类使用限制
        /// </summary>
        /// <param name="player"></param>
        /// <param name="gc"></param>
        /// <returns></returns>
        private static int SupplyLimit(Player player, GameConfig gc)
        {
            Variant limit = gc.Value.GetVariantOrDefault("UserLimit");
            if (limit == null) return 0;
            return CheckLevel(player, limit);
        }

        /// <summary>
        /// 物品使用
        /// </summary>
        /// <param name="note"></param>
        /// <param name="package"></param>
        /// <param name="gc"></param>
        /// <param name="isall"></param>
        private static void UseGoods(UserNote note, Variant package, GameConfig gc, bool isall)
        {
            string goodsid = gc.ID;
            string pakcageType = note.Count > 1 ? note.GetString(1) : "B0";
            int p = package.GetIntOrDefault("P");
            PlayerBusiness player = note.Player;
            if (player.AState == ActionState.Fight)
            {
                //战斗状态不能使用
                player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.UseGoods1));
                return;
            }

            PlayerEx b0 = player.B0;
            if (b0 == null) return;

            if (isall)
            {
                if (gc.Value.GetIntOrDefault("AllUse") != 1)
                {
                    player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.AllUseGoods1));
                    return;
                }
            }

            Variant t = BurdenManager.BurdenPlace(b0, p);
            if (t == null || string.IsNullOrEmpty(t.GetStringOrDefault("E")))
            {
                player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.UseGoods2));
                return;
            }

            int number = t.GetIntOrDefault("A");

            if (number < 1)
            {
                player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.UseGoods2));
                return;
            }
            if (!isall) //只使用1个
            {
                number = 1;
            }

            switch (gc.SubType)
            {
                //自动补满物品
                case GoodsSub.AutoFull:
                    if (!note.Player.RemoveGoods(p, GoodsSource.DoubleUse, isall))
                        return;
                    player.SetAutoFull(gc.Value, number);
                    player.UseGoodsR(true, goodsid);
                    break;

                //双倍经验物品
                case GoodsSub.DoubleExp:
                    if (!note.Player.RemoveGoods(p, GoodsSource.DoubleUse, isall))
                        return;
                    player.SetDoubleExp(gc.Value, number);
                    player.UseGoodsR(true, goodsid);

                    break;

                case GoodsSub.FightPro:
                    if (!note.Player.RemoveGoods(p, GoodsSource.DoubleUse, isall))
                        return;
                    player.SetFightPro(gc.Value, number);
                    player.UseGoodsR(true, goodsid);

                    break;
                //战斗记时器
                case GoodsSub.AutoFight:
                    if (!note.Player.RemoveGoods(p, GoodsSource.DoubleUse, isall))
                        return;
                    player.SetAutoFight(gc.Value, number);
                    player.UseGoodsR(true, goodsid);
                    if (player.AutoFight > 0)
                    {
                        int count = player.StartAutoFight();
                        note.Call(FightCommand.AutoFightR, new object[] { player.ID, count });
                    }

                    break;

                case GoodsSub.AddHideApc:
                    if (!note.Player.RemoveGoods(p, GoodsSource.DoubleUse, isall))
                        return;
                    player.SetAddHideApc(gc.Value, number);
                    player.UseGoodsR(true, goodsid);

                    break;

                //VIP基本功能
                case GoodsSub.VIPGoods:
                    Business.GoodsBusiness.VIPGoods(note, gc);
                    return;

                case GoodsSub.Supply:
                    if (!UseSupply(note, gc))
                        return;
                    if (!note.Player.RemoveGoods(p, GoodsSource.DoubleUse))
                        return;
                    player.UseGoodsR(true, goodsid);

                    break;

                case GoodsSub.PetBook:
                    if (!player.AddPetBook(gc.Value))
                    {
                        player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.UseGoods4));
                        return;
                    }
                    if (!note.Player.RemoveGoods(p, GoodsSource.DoubleUse))
                        return;

                    player.UseGoodsR(true, goodsid);
                    player.FinishNote(FinishCommand.PetSkill);
                    break;

                //使用回城羽毛
                case GoodsSub.TownGate:
                    int result = TownGate(player);
                    if (result != 0)
                    {
                        player.UseGoodsR(false, TipManager.GetMessage(result));
                        return;
                    }

                    if (!player.RemoveGoods(p, GoodsSource.DoubleUse, false))
                        return;
                    player.UseGoodsR(true, goodsid);

                    break;


                //经验宝珠
                case GoodsSub.AddExp:
                    if (!UseAddExp(player, gc, number))
                        return;
                    if (!note.Player.RemoveGoods(p, GoodsSource.DoubleUse, isall))
                        return;
                    player.UseGoodsR(true, goodsid);
                    break;

                //日常任务双倍经验
                case GoodsSub.TaskDoubleExp:
                    PlayerEx assist = note.Player.Assist;
                    if (!note.Player.RemoveGoods(p, GoodsSource.DoubleUse, isall))
                        return;

                    assist.Value.SetOrInc("TSP", number);
                    assist.Save();
                    note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(assist));
                    player.UseGoodsR(true, goodsid);

                    break;

                case GoodsSub.Bond:
                    int bond = gc.Value.GetIntOrDefault("Bond");
                    if (bond <= 0)
                        return;
                    if (!note.Player.RemoveGoods(p, GoodsSource.DoubleUse))
                        return;
                    note.Player.AddBond(bond, FinanceType.UseGoods);
                    player.UseGoodsR(true, goodsid);

                    break;

                //增加日常任务操作次数
                case GoodsSub.AddDayTask:
                    result = AddDayTask(player, gc, p);
                    if (result != 0)
                    {
                        player.UseGoodsR(false, TipManager.GetMessage(result));
                        return;
                    }
                    player.UseGoodsR(true, goodsid);

                    break;

                case GoodsSub.Bottle:
                    Bottles(player, gc, t);
                    break;

                case GoodsSub.FullBottle:
                    FullBottle(player, gc, t);
                    break;
                case GoodsSub.Lottery:
                    Lottery(player, gc, t);
                    break;

                case GoodsSub.PKProtect:
                    if (BurdenManager.GoodsCount(player.B0, gc.ID) >= number)
                    {
                        if (player.SetPKProtect(gc.Value, number))
                        {
                            player.RemoveGoods(p, GoodsSource.DoubleUse);
                            player.UseGoodsR(true, goodsid);
                        }
                        else
                        {
                            player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.UserLimit));
                        }
                    }
                    break;
                case GoodsSub.Mountegg:

                    if (string.IsNullOrEmpty(gc.Value.GetStringOrDefault("MountsID")))
                    {
                        player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.UseGoods9));
                        return;
                    }

                    if (!player.RemoveGoods(p, GoodsSource.DoubleUse))
                        return;
                    Mountegg(note.Player, gc);
                    player.UseGoodsR(true, goodsid);
                    break;

                default:
                    //不能双击使用
                    player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.UseGoods9));
                    return;
            }
        }


        /// <summary>
        /// 道具摇奖
        /// </summary>
        /// <param name="player"></param>
        /// <param name="gc"></param>
        /// <param name="p"></param>
        private static void Lottery(PlayerBusiness player, GameConfig gc, Variant p)
        {
            PlayerEx ly = player.Lottery;
            Variant ex = ly.Value;

            Variant v = gc.Value;
            Variant libao = v.GetVariantOrDefault("LiBao");
            if (libao == null)
                return;
            if (ex != null && ex.Count > 0)
            {
                player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.Lottery1));
                return;
            }

            if (player.RemoveGoods(p.GetIntOrDefault("P"), GoodsSource.DoubleUse))
            {
                Variant con = new Variant();
                int n = NumberRandom.Next(0, 12);
                //抽奖

                GetLotteryAward(con, libao, n, p.GetIntOrDefault("H"));


                ly.Value = con;
                ly.Save();
                player.UseGoodsR(true, gc.ID);
                player.Call(ClientCommand.UpdateActorR, new PlayerExDetail(ly));
            }
            else
            {
                player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.UseGoods2));
            }
        }

        /// <summary>
        /// 产生新的道具
        /// </summary>
        /// <param name="con"></param>
        /// <param name="libao"></param>
        /// <param name="n"></param>
        private static void GetLotteryAward(Variant con, Variant libao, int n, int h)
        {

            Variant award = libao.GetVariantOrDefault("Award");
            Dictionary<string, int> dic = new Dictionary<string, int>();
            Award.GetPackets(award, dic);
            if (dic.Count > 0)
            {
                foreach (var item in dic)
                {
                    Variant v = new Variant();
                    v.Add("G", item.Key);
                    v.Add("A", item.Value);
                    if (con.Count == n)
                    {
                        v.Add("L", 1);
                    }
                    else
                    {
                        v.Add("L", 0);
                    }
                    v.Add("H", h);

                    con.Add(con.Count.ToString(), v);
                    if (con.Count >= 12)
                        return;
                }
            }
            GetLotteryAward(con, libao, n, h);
        }

        /// <summary>
        /// 星力空瓶的使用
        /// </summary>
        /// <param name="player"></param>
        /// <param name="gc"></param>
        /// <param name="p"></param>
        private static void Bottles(PlayerBusiness player, GameConfig gc, Variant p)
        {
            Variant v = gc.Value;
            //需要星力值
            int outstar = v.GetIntOrDefault("OutStar");
            if (player.StarPower < outstar)
            {
                player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.GoodsWashing6));
                return;
            }
            Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
            //目标道具
            string goodsid = v.GetStringOrDefault("GoodsID");

            Variant tmp = new Variant();
            tmp.SetOrInc("Number" + p.GetIntOrDefault("H"), 1);
            dic.Add(goodsid, tmp);

            if (BurdenManager.IsFullBurden(player.B0, dic))
            {
                player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.BurdenB0Full));
                return;
            }

            if (!player.AddStarPower(-outstar, FinanceType.UseGoods))
            {
                player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.GoodsWashing6));
                return;
            }

            if (player.RemoveGoods(p.GetIntOrDefault("P"), GoodsSource.DoubleUse))
            {
                player.AddGoods(dic, GoodsSource.Bottles);
                player.UseGoodsR(true, goodsid);
                player.FinishNote(FinishCommand.StarBottle);
            }
            else
            {
                player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.UseGoods2));
            }
        }

        /// <summary>
        /// 添加星力
        /// </summary>
        /// <param name="player"></param>
        /// <param name="gc"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private static void FullBottle(PlayerBusiness player, GameConfig gc, Variant p)
        {
            Variant v = gc.Value;

            int addStar = v.GetIntOrDefault("AddStar");

            int max = PartAccess.Instance.PowerMax();

            Variant mv = MemberAccess.MemberInfo(player.MLevel);
            if (mv != null)
            {
                max = mv.GetIntOrDefault("StarMax");
            }

            if (player.StarPower + addStar > max)
            {
                player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.StarFull));
                return;
            }
            if (player.RemoveGoods(p.GetIntOrDefault("P"), GoodsSource.DoubleUse))
            {
                player.AddStarPower(addStar, FinanceType.UseGoods);
                player.UseGoodsR(true, gc.ID);
            }
            else
            {
                player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.UseGoods2));
            }
        }

        /// <summary>
        /// 增加每日日常任务
        /// </summary>
        /// <param name="player"></param>
        /// <param name="gc"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private static int AddDayTask(PlayerBusiness player, GameConfig gc, int p)
        {
            PlayerEx dt = player.Value["DayTask"] as PlayerEx;
            Variant v_dt = dt.Value;
            //道具
            int daily = gc.Value.GetIntOrDefault("Daily");
            int total = v_dt.GetIntOrDefault("Total");
            int max = v_dt.GetIntOrDefault("Max");


            Variant mv = MemberAccess.MemberInfo(player.MLevel);
            if (mv != null)
            {
                max += mv.GetIntOrDefault("ShiBan") * 5;
            }


            if (total >= max)
            {
                //您今天的日常任务已经扩充到上限最大值，请明天再使用探险者石板
                return GoodsReturn.UseGoods8;
            }
            if (player.RemoveGoods(p, GoodsSource.DoubleUse))
            {
                if (total + daily <= max)
                {
                    v_dt.SetOrInc("Total", daily);
                }
                else
                {
                    v_dt["Total"] = max;
                }

                //如果不存在日常任务
                if (!TaskAccess.Instance.IsDayTask(player.ID, 2))
                {
                    GameConfig gn = GameConfigAccess.Instance.GetDayTaskOne(player.Level, 2);
                    if (gn != null)
                    {
                        DayTask(player, true);
                    }
                }
                dt.Save();
                player.Call(ClientCommand.UpdateActorR, new PlayerExDetail(dt));
                return 0;
            }
            return GoodsReturn.AnswerNoGood;
        }

        /// <summary>
        /// 回城羽毛
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private static int TownGate(PlayerBusiness player)
        {
            SceneBusiness scene = player.Scene;
            if (scene.SceneType == SceneType.Pro || scene.SceneType == SceneType.Rob)
            {
                //守护战争或夺宝奇兵中不能使用回城羽毛
                return GoodsReturn.UseGoods5;
            }

            if (player.Team != null && player.TeamJob == TeamJob.Member)
            {
                //只有队长可以使用回城羽毛
                return GoodsReturn.UseGoods6;
            }
            if (scene.TownGate(player, TransmitType.UseProp))
            {
                return 0;
            }
            //本场景不可使用回城羽毛
            return GoodsReturn.UseGoods7;
        }

        /// <summary>
        /// 经验宝珠
        /// </summary>
        /// <param name="player"></param>
        /// <param name="c"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        private static bool UseAddExp(PlayerBusiness player, GameConfig c, int number = 1)
        {
            //检查使用限制..
            //if (!SupplyLimit(note, c)) return;
            int p1exp = c.Value.GetIntOrDefault("P1exp") * number;
            if (p1exp > 0)
            {
                player.AddExperience(p1exp, FinanceType.UseGoods);
                return true;
            }
            int p2exp = c.Value.GetIntOrDefault("P2exp") * number;
            if (p2exp > 0)
            {
                if (player.Pet == null)
                {
                    //没有出战的宠物不能使用
                    player.Call(GoodsCommand.UseGoodsR, false, TipManager.GetMessage(GoodsReturn.UseAddExp));
                    return false;
                }
                return player.AddPetExp(player.Pet, p2exp, true,(int)FinanceType.UseGoods);
            }
            int m1exp = c.Value.GetIntOrDefault("M1exp") * number;
            if (m1exp > 0)
            {
                if (player.Mounts == null)
                {
                    player.Call(GoodsCommand.UseGoodsR, false, TipManager.GetMessage(GoodsReturn.NoMounts));
                    return false;
                }
                return player.AddMounts(m1exp, GoodsSource.DoubleUse);
            }
            return false;
        }


        /// <summary>
        /// 食品类.补充HP或MP
        /// </summary>
        /// <param name="note"></param>
        /// <param name="gc"></param>
        /// <param name="player"></param>
        private static bool UseSupply(UserNote note, GameConfig gc)
        {
            PlayerBusiness player = note.Player;
            int check = SupplyLimit(note.Player, gc);
            if (check != 0)
            {
                player.UseGoodsR(false, TipManager.GetMessage(check));
                return false;
            }

            bool supp = false;
            string target = note.GetString(2);
            if (string.IsNullOrEmpty(target) || target == note.PlayerID)
            {
                supp = player.SupplyRole(gc.Value);
            }
            else if (target == note.Player.Pet.ID)
            {
                supp = player.SupplyPet(gc.Value);
            }
            if (!supp)
            {
                //"不需要补充"
                player.UseGoodsR(false, TipManager.GetMessage(GoodsReturn.UseGoods3));
            }
            return supp;
        }


        /// <summary>
        /// 重置日常任务
        /// </summary>
        /// <param name="player">当前在线</param>
        /// <param name="iscall"></param>
        private static void DayTask(PlayerBusiness player, bool iscall = true)
        {
            string[] strs = TipManager.GetMessage(TaskReturn.DayTaskConfig).Split('|');
            if (strs.Length < 2) return;
            //移除以前的日常任务            
            int total = Convert.ToInt32(strs[0]);
            int max = Convert.ToInt32(strs[1]);
            PlayerEx ex;
            Variant v = null;
            if (player.Value.ContainsKey("DayTask"))
            {
                ex = player.Value["DayTask"] as PlayerEx;
                v = ex.Value;
                //表示时间到,更新日常任务的当前值
                if (v.GetLocalTimeOrDefault("NowData").Date != DateTime.Now.Date)
                {
                    v["Cur"] = 0;
                    v["Total"] = total;
                    v["Max"] = max;
                    v["NowData"] = DateTime.UtcNow;
                }
            }
            else
            {
                ex = new PlayerEx(player.ID, "DayTask");
                v = new Variant();
                v.Add("Cur", 0);//当前完成数量
                v.Add("Total", total);//默认可以执行次数
                v.Add("Max", max);//每天最多允许执行次数
                v.Add("NowData", DateTime.UtcNow);//谋一天
                ex.Value = v;
            }

            if (v == null) return;
            //表示当天日常任务已经完成
            if (v.GetIntOrDefault("Cur") == v.GetIntOrDefault("Total"))
                return;

            if (TaskAccess.Instance.IsDayTask(player.ID, 2))
            {
                v["Cur"] = v.GetIntOrDefault("Cur") == 0 ? 1 : v.GetIntOrDefault("Cur");
                ex.Save();
                player.Call(ClientCommand.UpdateActorR, new PlayerExDetail(ex));
                return;
            }

            GameConfig gc = GameConfigAccess.Instance.GetDayTaskOne(player.Level, 2);
            if (gc == null)
                return;

            Task t = TaskAccess.Instance.TaskBase(player.ID, player.RoleID, gc, 0, 0);
            if (t != null)
            {
                v.SetOrInc("Cur", 1);
                ex.Save();
                if (iscall)
                {
                    player.Call(TaskCommand.TaskActivationR, TaskAccess.Instance.GetTaskInfo(t));
                    player.Call(ClientCommand.UpdateActorR, new PlayerExDetail(ex));
                }
            }
        }

        /// <summary>
        /// 陪伴兽
        /// </summary>
        /// <param name="pb"></param>
        private static void Mountegg(PlayerBusiness pb, GameConfig gc)
        {
            if (pb.Mounts != null)
            {
                pb.Call(GoodsCommand.UseGoodsR, false, TipManager.GetMessage(GoodsReturn.IsMounst));
                return;
            }
            Mounts m = new Mounts();
            m.ID = ObjectId.GenerateNewId().ToString();
            m.PlayerID = pb.ID;
            m.MountsID = gc.Value.GetStringOrDefault("MountsID");
            m.Level = 1;
            m.Rank = 1;
            m.Experience = 0;
            DateTime dt = DateTime.UtcNow;
            m.Update = dt;
            m.Created = dt;
            m.Name = gc.Name;
            m.Status = 1;
            m.ZhuFu = 0;
            m.FailCount = 0;
            m.FailTime = dt;
            if (m.Save())
            {
                pb.MountsUpdate(m, null);
                pb.MountsInfo();
            }
        }
    }
}