using System;
using System.Collections.Generic;
using System.Drawing;
using Sinan.ArenaModule.Business;
using Sinan.ArenaModule.Detail;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Util;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif


namespace Sinan.ArenaModule.Fight
{
    public class ArenaFight
    {
        /// <summary>
        /// 战斗信息
        /// </summary>
        public static void FightInfo()
        {
            if (ArenaBusiness.ArenaList.Count == 0)
                return;
            ArenaBase[] ab = new ArenaBase[ArenaBusiness.ArenaList.Count];
            ArenaBusiness.ArenaList.Values.CopyTo(ab, 0);
            for (int i = 0; i < ab.Length; i++)
            {
                ArenaBase model = ab[i];

                foreach (Pet p in model.Pets.Values)
                {
                    CurPoint(p);
                }

                if (model.Status == 0)
                {
                    if (model.StartTime <= DateTime.UtcNow)
                    {
                        CheckArenaStart(model);
                    }
                }
                else if (model.Status == 1)
                {
                    if (FightEnd(model))
                    {
                        model.Status = 2;

                        //竞技场结束,计算战斗结果                        
                        model.CallAll(ArenaCommand.ArenaEndR, SettleMessage(model));
                        ArenaBase tmp;
                        if (ArenaBusiness.ArenaList.TryRemove(model.SoleID, out tmp)) 
                        {
                            PlayerOut(tmp);
                        }
                        //foreach (PlayerBusiness user in tmp.Players.Values)
                        //{
                        //    user.GroupName = "";
                        //    user.SoleID = "";
                        //    user.SetActionState(ActionState.Standing);
                        //}
                    }
                    else
                    {
                        //战斗计算
                        Fight(model);
                    }
                }
            }
        }

        /// <summary>
        /// 竞技场结算
        /// </summary>
        /// <param name="model"></param>
        private static void ArenaSettle(ArenaBase model)
        {
            //判断组的胜负平关系,如果在结束时存在多组还有存活的宠物

            Dictionary<string, int> dic = new Dictionary<string, int>();
            foreach (string groupName in model.GroupName)
            {
                bool groupWin = false;

                foreach (Pet p in model.Pets.Values)
                {
                    Variant v = p.Value;
                    Variant shengMing = v.GetValueOrDefault<Variant>("ShengMing");
                    if (groupName == p.GroupName && shengMing.GetIntOrDefault("V") > 0)
                    {
                        groupWin = true;
                        break;
                    }
                }
                int win = groupWin ? 1 : 0;
                if (!dic.ContainsKey(groupName))
                {
                    dic.Add(groupName, win);
                }
            }

            int group = 0;//表示存在多少组存活的情况 
            foreach (var item in dic)
            {
                if (item.Value == 1)
                {
                    group++;
                }
            }

            GameConfig gc = GameConfigAccess.Instance.FindOneById(model.ArenaID);
            if (gc == null) return;
            Variant t = gc.Value;
            Variant petLevel = t.GetValueOrDefault<Variant>("PetLevel").GetValueOrDefault<Variant>(model.PetMin + "-" + model.PetMax);
            if (petLevel == null)
                return;

            foreach (Settle s in model.SettleInfo.Values)
            {
                int otherFight = 0;
                int m = 0;
                int isType = 0;
                if (dic.TryGetValue(s.GroupName, out m))
                {
                    if (m == 0)
                    {
                        //输
                        otherFight = Convert.ToInt32(s.WinFight * 0.15 + s.LossFight * 1.5) - petLevel.GetIntOrDefault("LossFight");
                        isType = 0;
                    }
                    else if (m == 1 && group == 1)
                    {
                        //胜利
                        otherFight = Convert.ToInt32(s.WinFight * 0.25) + petLevel.GetIntOrDefault("WinFight");
                        isType = 1;
                    }
                    else
                    {
                        //平局
                        otherFight = Convert.ToInt32(s.WinFight * 0.1 + s.LossFight * 0.1);
                        isType = 2;
                    }
                }

                s.ResultType = isType;
                //附加战绩值
                s.OtherFight = otherFight;
                s.TotalFight = s.OtherFight + s.WinFight + s.LossFight;

                PlayerBusiness user;
                if (model.Players.TryGetValue(s.PlayerID, out user))
                {
                    user.AddFightValue(s.TotalFight, false, FinanceType.AuctionSell);
                    AddFatigue(model, user, s.ResultType);

                    if (s.ResultType == 1)
                    {
                        //竞技场胜利次数
                        user.FinishNote(FinishCommand.ArenaWin);
                    }
                }
            }
        }

        /// <summary>
        /// 检查竞技场是否结束
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static bool FightEnd(ArenaBase model)
        {
            List<string> group = new List<string>();
            foreach (Pet p in model.Pets.Values)
            {
                Variant v = p.Value;
                if (v.GetValueOrDefault<Variant>("ShengMing").GetIntOrDefault("V") > 0)
                {
                    if (!group.Contains(p.GroupName))
                    {
                        group.Add(p.GroupName);
                    }
                }
            }

            //表示还有多组，没有分出胜负
            if (model.EndTime > DateTime.UtcNow && group.Count > 1)
                return false;
            ArenaSettle(model);
            return true;
        }

        /// <summary>
        /// 战斗计算
        /// </summary>
        private static void Fight(ArenaBase model)
        {
            ArenaScene scene = model.SceneSession;

            ConcurrentDictionary<string, Pet> doc = model.Pets;
            foreach (Pet p in doc.Values)
            {
                Variant root = p.Value;
                Variant rsm = root.GetValueOrDefault<Variant>("ShengMing");
                //不能再攻击别人
                if (rsm.GetIntOrDefault("V") <= 0)
                {
                    p.CoolingTime = 0;
                    continue;
                }

                if (p.CoolingTime > 0)
                {
                    p.CoolingTime -= 500;
                    if (p.CoolingTime > 0) 
                        continue;
                }

                Variant rmf = root.GetValueOrDefault<Variant>("MoFa");

                FightBase fightBase = new FightBase(model);
                fightBase.ID = p.ID;
                fightBase.Name = p.Name;
                fightBase.PlayerID = p.PlayerID;
                fightBase.Persons = 1;

                int CoolingTime = 0;

                if (!string.IsNullOrEmpty(p.CurSkill))
                {
                    GameConfig gc = GameConfigAccess.Instance.FindOneById(p.CurSkill);
                    if (gc != null)
                    {
                        //得到技能基本信息
                        ArenaSkill askill = new ArenaSkill(gc);
                        askill.BaseLevel(p.CurLevel);
                        if (rmf.GetIntOrDefault("V") >= askill.MPCost)
                        {
                            fightBase.Persons += askill.AddTargets;
                            //攻击方魔法消耗值
                            fightBase.MPCost = askill.MPCost;
                            fightBase.InjuryType = askill.InjuryType;
                            fightBase.A = askill.A;
                            fightBase.B = askill.B;
                            fightBase.CurSkill = p.CurSkill;
                            CoolingTime = askill.CoolingTime;

                            p.Range = askill.Range;//技能攻击范围
                        }
                        else
                        {
                            p.Range = 100;
                        }
                    }
                    else
                    {
                        p.Range = 100;
                    }
                }
                else
                {
                    p.Range = 100;
                }


                List<PetFightDetail> list = new List<PetFightDetail>();

                bool isRangetPet = false;
                if (!string.IsNullOrEmpty(p.RangePet))
                {
                    //主动攻击的宠物
                    Pet pt;
                    if (doc.TryGetValue(p.RangePet, out pt))
                    {
                        if (ChechFight(p, pt))
                        {
                            //主要攻击目标
                            fightBase.RangePet = pt.ID;
                            isRangetPet = true;
                            list.Add(fightBase.FightObject(p, pt));
                        }
                    }
                }


                //循环所有宠物，寻找可攻击的目标
                if (fightBase.Persons > list.Count)
                {
                    foreach (Pet pet in doc.Values)
                    {
                        if (ChechFight(p, pet))
                        {
                            if (!isRangetPet)
                            {
                                //主要攻击目标
                                fightBase.RangePet = pet.ID;
                            }
                            else
                            {
                                //主要攻击目标已经被攻击
                                if (pet.ID == fightBase.RangePet)
                                    continue;
                            }
                            list.Add(fightBase.FightObject(p, pet));
                            //攻击对象已经满，不需要再找
                            if (list.Count == fightBase.Persons)
                                break;
                        }
                    }
                }

                if (list.Count > 0)
                {
                    if (CoolingTime <= 0)
                    {
                        //攻击成功添加冷却时间
                        CoolingTime = 3000;
                    }
                    p.CoolingTime = CoolingTime;
                    //当前
                    rmf["V"] = rmf.GetIntOrDefault("V") - fightBase.MPCost;
                    fightBase.MoFa = rmf;
                    fightBase.ShengMing = rsm;

                    PetFightDetail gongji = new PetFightDetail(fightBase);
                    model.CallAll(ArenaCommand.ArenaFightR, gongji, list);
                }
            }
        }

        /// <summary>
        /// 检查竞技场是否可以开始
        /// </summary>
        /// <param name="model">竞技场</param>
        /// <returns></returns>
        private static void CheckArenaStart(ArenaBase model)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            foreach (Pet p in model.Pets.Values)
            {
                if (!dic.ContainsKey(p.GroupName))
                {
                    dic.Add(p.GroupName, 1);
                }
                else
                {
                    dic[p.GroupName]++;
                }

                PlayerBusiness user;
                if (model.Players.TryGetValue(p.PlayerID, out user))
                {
                    Settle settle;
                    if (!model.SettleInfo.TryGetValue(p.PlayerID, out settle))
                    {
                        settle = new Settle();
                        settle.PlayerID = p.PlayerID;
                        settle.GroupName = p.GroupName;
                        settle.PlayerName = user.Name;
                        settle.ResultType = 0;
                        model.SettleInfo.TryAdd(p.PlayerID, settle);
                    }
                }
            }

            if (dic.Count <= 1)
            {
                //竞技场不满足开始条件
                model.CallAll(ArenaCommand.ArenaStartR, false,TipManager.GetMessage(ArenaReturn.CheckArenaStart1));
                ArenaBase tmp;
                if (ArenaBusiness.ArenaList.TryRemove(model.SoleID, out tmp)) 
                {
                    PlayerOut(tmp);
                }    
                return;
            }

            model.Status = 1;
            //竞技场开始
            model.CallAll(ArenaCommand.ArenaStartR, true, TipManager.GetMessage(ArenaReturn.CheckArenaStart2));
        }

        /// <summary>
        /// 退出竞技场
        /// </summary>
        /// <param name="model"></param>
        private static void PlayerOut(ArenaBase model) 
        {
            foreach (PlayerBusiness user in model.Players.Values)
            {
                user.GroupName = "";
                user.SoleID = "";
                user.SetActionState(ActionState.Standing);
            }
        }

        /// <summary>
        /// 竞技场宠物疲劳值增加
        /// </summary>
        /// <param name="model"></param>
        /// <param name="user">角色</param>
        /// <param name="type">0表示负,1胜,2平</param>
        public static void AddFatigue(ArenaBase model, PlayerBusiness user, int type)
        {
            GameConfig gc = GameConfigAccess.Instance.FindOneById(model.ArenaID);
            if (gc == null)
                return;

            Variant t = gc.Value;
            Variant petLevel = t.GetValueOrDefault<Variant>("PetLevel").GetValueOrDefault<Variant>(model.PetMin + "-" + model.PetMax);
            if (petLevel == null)
                return;

            int addFatigue = 0;//战斗增加疲劳值
            switch (type)
            {
                case 0:
                    addFatigue = petLevel.GetIntOrDefault("Lose");
                    break;
                case 1:
                    addFatigue = petLevel.GetIntOrDefault("Win");
                    break;
                case 2:
                    addFatigue = petLevel.GetIntOrDefault("Ping");
                    break;
            }

            if (user.ID == model.PlayerID)
            {
                addFatigue = Convert.ToInt32(addFatigue * (1 - petLevel.GetDoubleOrDefault("FarmLv")));
            }

            foreach (Pet p in model.Pets.Values)
            {
                if (p.PlayerID != user.ID)
                    continue;
                Variant v = p.Value;
                //取得宠物最大疲劳值
                int max = PetAccess.Instance.MaxFatigue(v.GetIntOrDefault("PetsLevel"));

                if (p.PlayerID == user.ID)
                {
                    if (v.GetIntOrDefault("Fatigue") + addFatigue >= max)
                    {
                        v["Fatigue"] = max;
                    }
                    else
                    {
                        v["Fatigue"] = v.GetIntOrDefault("Fatigue") + addFatigue;
                    }
                    
                    p.SaveFatigue();

                    Variant tmp = new Variant();
                    tmp.Add("ID",p.ID);
                    tmp.Add("Fatigue", v.GetIntOrDefault("Fatigue"));
                    user.Call(PetsCommand.UpdatePetR, true, tmp);
                    //Console.WriteLine("宠物名称:" + p.Name + ",当前疲劳值:" + v.GetIntOrDefault("Fatigue") + ",增加量:" + addFatigue + ",输赢:" + type);
                }
            }


        }

        /// <summary>
        /// 通知所有场景用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<SettleDetail> SettleMessage(ArenaBase model)
        {
            List<SettleDetail> list = new List<SettleDetail>();
            foreach (Settle s in model.SettleInfo.Values)
            {
                SettleDetail sd = new SettleDetail(s);
                list.Add(sd);
            }
            return list;
        }

        /// <summary>
        /// 中途断线处理
        /// </summary>
        public static void UseDis(ArenaBase model, string playerid)
        {

            PlayerBusiness user;
            if (!model.Players.TryGetValue(playerid, out user))
                return;
            //表示没有参战竞技场
            if (string.IsNullOrEmpty(user.GroupName))
                return;
            GameConfig gc = GameConfigAccess.Instance.FindOneById(model.ArenaID);
            if (gc == null) return;
            Variant t = gc.Value;
            Variant petLevel = t.GetValueOrDefault<Variant>("PetLevel").GetValueOrDefault<Variant>(model.PetMin + "-" + model.PetMax);
            if (petLevel == null)
                return;


            Settle s;
            if (model.SettleInfo.TryGetValue(playerid, out s))
            {
                s.ResultType = 0;
                s.GroupName = user.GroupName;
            }
            else
            {
                s = new Settle();
                s.PlayerID = playerid;
                s.ResultType = 0;
                s.GroupName = user.GroupName;
                model.SettleInfo.TryAdd(playerid, s);
            }
            s.OtherFight = Convert.ToInt32(s.WinFight * 0.15 - s.LossFight * 1.5) - petLevel.GetIntOrDefault("LossFight");
            s.TotalFight = s.OtherFight + s.WinFight + s.LossFight;
            user.AddFightValue(s.TotalFight, false, FinanceType.AuctionSell);

            AddFatigue(model, user, 0);

            string title = "";
            string content = "";
            if (s.TotalFight >= 0)
            {
                // "致光荣的战场逃亡者";
                title = TipManager.GetMessage(ArenaReturn.UseDis1);
                //"你在竞技场的脱逃让人失望，不过凭着卓越的战斗技巧，仍然获得了X点附加战绩奖励。希望勇者不再以战败者的方式离开战场！";
                content = TipManager.GetMessage(ArenaReturn.UseDis2);
            }
            else
            {
                //"致可耻的战场逃亡者";
                title = TipManager.GetMessage(ArenaReturn.UseDis3);
                //"你在竞技场的脱逃行为被人唾弃，你在这次战斗中被判定为失败者，扣除了X点附加战绩惩罚。你的行为背离了竞技场精神！";
                content = TipManager.GetMessage(ArenaReturn.UseDis4);
            }
            int reTime = Convert.ToInt32(TipManager.GetMessage(EmailReturn.HameDay));
            if (EmailAccess.Instance.SendEmail(title, TipManager.GetMessage(ArenaReturn.UseDis5), user.ID, user.Name, content, string.Empty, null, reTime))
            {
                if (user.Online)
                {
                    user.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(user.ID));
                }
            }
        }

        /// <summary>
        /// 计算得到当前屏幕坐标
        /// </summary>
        /// <param name="p"></param>
        private static void CurPoint(Pet p)
        {
            Point begin = p.BeginPoint;
            Point end = p.EndPoint;

            if (p.PetStatus == 0)
            {
                p.CurPoint = begin;
                return;
            }
            TimeSpan ts = DateTime.UtcNow - p.StartTime;
            //总的毫秒数
            double ms = ts.TotalMilliseconds;
            //两点间的距离
            double s = Spacing(begin, end);
            //移动实际需要时间
            double t = s / p.Speed;
            if (t > p.Speed)
            {
                p.CurPoint = end;
                return;
            }
            //移动的距离
            double s0 = Math.Abs(ms * p.Speed * 0.001);
            int x = Convert.ToInt32(s0 * (end.X - begin.X) + s * begin.X);
            int y = Convert.ToInt32(s0 * (end.Y - begin.Y) + s * begin.Y);
            p.CurPoint = new Point(x, y);
        }

        /// <summary>
        /// 得到两点间的距离
        /// </summary>
        /// <param name="begin">起点</param>
        /// <param name="end">终点</param>
        /// <returns></returns>
        private static double Spacing(Point begin, Point end)
        {
            return Math.Sqrt(Math.Pow((begin.X - end.X), 2) + Math.Pow((begin.Y - end.Y), 2));
        }

        /// <summary>
        /// 判断是否允许攻击
        /// </summary>
        /// <param name="root">攻击者</param>
        /// <param name="target">被攻击者</param>
        /// <returns></returns>
        private static bool ChechFight(Pet root, Pet target)
        {
            //同组不能攻击
            if (root.GroupName == target.GroupName)
                return false;

            Variant v_target = target.Value;
            Variant shengMing = v_target.GetValueOrDefault<Variant>("ShengMing");
            //攻击目标已经死亡
            if (shengMing.GetIntOrDefault("V") <= 0)
            {                
                return false;
            }
            //表示不在攻击范围
            double spacing = Spacing(root.CurPoint, target.CurPoint);
            //宠物之前距离大于100，小于15都不攻击
            if (spacing > root.Range || spacing < 15)
            {                
                return false;
            }
            return true;
        }
    }
}
