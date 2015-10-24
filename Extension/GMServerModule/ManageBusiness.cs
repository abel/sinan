using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Util;
using MongoDB.Driver;
using MongoDB.Bson;
using Sinan.Log;

namespace Sinan.GMServerModule
{
    public class ManageBusiness
    {
        static int x = 0;
        static DateTime dt;

        static bool runing = false;
        /// <summary>
        /// 断开所有用户
        /// </summary>
        /// <param name="note"></param>
        public static object ExitAll(Notification note)
        {
            x++;
            if (x == 1)
            {
                dt = DateTime.UtcNow;
            }
            TimeSpan ts = DateTime.UtcNow - dt;
            if (ts.TotalSeconds > 10)
            {
                x = 1;
                dt = DateTime.UtcNow;
            }
            else
            {
                if (x >= 2)
                {
                    FrontApplication.Instance.Stop();
                    string str = string.Format("服务已停止");
                    return str;
                    //Environment.Exit(0);
                }
            }
            string str2 = string.Format("确定要停止应用程序,请在10秒内重新发该指令");
            return str2;
        }

        internal static object ReStart(Notification note)
        {
            string str;
            if (FrontApplication.Instance.Restart())
            {
                str = "服务重启成功";
            }
            else
            {
                str = "服务运行中..";
            }
            return str;
        }


        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="note"></param>
        public static object AddCoin(Notification note)
        {
            string[] strs = GMBusiness.GetCommand(note);
            if (strs.Length < 2) return null;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }
            int coin = Convert.ToInt32(strs[1]);
            player.AddCoin(coin, FinanceType.GM);

            UserNote note1 = new UserNote(player, PartCommand.Recharge, new object[] { coin });
            Notifier.Instance.Publish(note1);
            return "为【" + player.Name + "】充值晶币:" + coin;
        }

        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="note"></param>
        public static object AddScore(Notification note)
        {
            string[] strs = GMBusiness.GetCommand(note);
            if (strs.Length < 2) return null;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }
            int score = Convert.ToInt32(strs[1]);
            player.AddScore(score, FinanceType.GM);
            return "为【" + player.Name + "】充值游戏币:" + score;
        }

        /// <summary>
        /// 星力
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public static object AddPower(Notification note)
        {
            string[] strs = GMBusiness.GetCommand(note);
            if (strs.Length < 2)
                return null;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }

            GameConfig gc = GameConfigAccess.Instance.FindOneById("ME_00001");
            if (gc == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }
            Variant v = gc.Value;
            int max = v.GetIntOrDefault("Max");
            Variant mv = MemberAccess.MemberInfo(player.MLevel);
            if (mv != null)
            {
                max = mv.GetIntOrDefault("StarMax");
            }

            //星力达到最大值,不能再增加
            if (player.StarPower >= max)
            {
                return string.Format(TipManager.GetMessage("星力达到最大值,不能再增加"), strs[0]);
            }
            
            int power = Convert.ToInt32(strs[1]);
            int m = 0;
            if (player.StarPower + power >= max)
            {
                m = max - player.StarPower;
            }
            else 
            {
                m = power;
            }

            player.AddStarPower(m, FinanceType.GM);
            return "为【" + player.Name + "】添加星力:" + m;
        }

        /// <summary>
        /// 添加经验
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static object Exp(Notification note)
        {
            string[] strs = GMBusiness.GetCommand(note);
            if (strs.Length < 2) return null;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }
            int exp = 0;
            if (int.TryParse(strs[1], out exp))
            {
                player.AddExperience(exp, FinanceType.GM);
                return string.Format(TipManager.GetMessage(GMReturn.AddExp), player.Name);
            }
            return null;
        }

        /// <summary>
        /// 得到任务ID
        /// </summary>
        /// <param name="note"></param>
        public static object GetTaskID(Notification note)
        {
            string[] strs = GMBusiness.GetCommand(note);
            GameConfig gc = GameConfigAccess.Instance.Find(MainType.Task, null, strs[0].Trim());
            if (gc == null) return null;
            string str = "任务【" + gc.Name + "】配置ID为【" + gc.ID + "】";
            return str;
        }

        /// <summary>
        /// 移除某角色某个任务
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static object TaskRemove(Notification note)
        {
            string[] strs = GMBusiness.GetCommand(note);
            if (strs.Length < 2) return null;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }
            List<string> list = new List<string>();
            list.Add(strs[1].Trim());
            List<Task> tasks = TaskAccess.Instance.FindConfigList(player.ID, list);
            foreach (Task k in tasks)
            {
                TaskAccess.Instance.Remove(player.ID, new List<string> { k.Value.GetStringOrDefault("TaskID") });
                TaskManage.TaskGiveup(player, k, false);
                return ("移除任务【" + k.Value.GetStringOrDefault("TaskID") + "】成功");
            }
            return null;
        }

        /// <summary>
        /// 触发某个任务
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static object TaskAct(Notification note)
        {
            string[] strs = GMBusiness.GetCommand(note);
            if (strs.Length < 2) return null;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }
            GameConfig gc = GameConfigAccess.Instance.FindOneById(strs[1]);
            if (gc == null) return null;

            TaskManage.TaskBack(player, gc);
            string str = string.Format("任务【{0}】触发成功", gc.Name);
            return str;
        }

        /// <summary>
        /// 任务重置
        /// </summary>
        /// <param name="note"></param>
        public static object TaskReset(Notification note)
        {
            string connectionString = ConfigLoader.Config.DbPlayer;
            var db = MongoDatabase.Create(connectionString);
            //取得角色列表
            var mc = db.GetCollection("Player");

            IList nowtask = note.GetValue<IList>(0);
            if (nowtask == null || nowtask.Count == 0)
                return null;
            string nowactid = note.GetString(1);//新触发的任务
            var v = mc.FindAll();
            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            List<string> tt = new List<string>()
            {
                "G_s120101","G_s120401","G_s120102","G_s120402","G_s120103","G_s120403"
            };
            dic.Add("1", tt);
            tt = new List<string>()
            {
                "G_s110101","G_s110401","G_s110102","G_s110402","G_s110103","G_s110403"
            };
            dic.Add("2", tt);
            tt = new List<string>()
            {
                "G_s130101","G_s130401","G_s130102","G_s130402","G_s130103","G_s130403"
            };
            dic.Add("3", tt);

            foreach (var item in v)
            {
                BsonValue o;
                if (item.TryGetValue("_id", out o))
                {
                    //将角色ID转换为十六进制
                    try
                    {
                        PlayerBusiness pb = PlayersProxy.FindPlayerByID(Convert.ToInt32(o));

                        if (pb == null)
                            continue;

                        int level = pb.Level;

                        List<Task> list = TaskAccess.Instance.GetTaskList(pb.ID);

                        bool isact = false;//是否需要触发新任务
                        string t = "";
                        foreach (Task task in list)
                        {
                            Variant tv = task.Value;
                            string taskid = tv.GetStringOrDefault("TaskID");
                            int tasktype = tv.GetIntOrDefault("TaskType");
                            if (tasktype != 0)
                                continue;
                            if (tv.GetIntOrDefault("Status") >= 3)
                                continue;
                            if (taskid == nowactid)
                                continue;
                            //表示存在这个新任务，则将这个移除
                            if (nowtask.Contains(taskid))
                            {
                                tv["Status"] = 3;
                                task.Save();
                                t = taskid;
                                isact = true;
                            }
                        }

                        if (isact)
                        {
                            GameConfig gc = GameConfigAccess.Instance.FindOneById(nowactid);
                            if (gc == null)
                                return null;
                            TaskManage.TaskBack(pb, gc);
                            LogWrapper.Warn(pb.UserID + "," + pb.Name + "," + t + "," + level);

                            List<Variant> goodsList = new List<Variant>();
                            List<string> g = dic[pb.RoleID];
                            for (int i = 0; i < g.Count; i++)
                            {
                                Variant gs = new Variant();
                                gs.Add("G", g[i]);
                                gs.Add("A", 1);
                                gs.Add("E", g[i]);
                                gs.Add("H", 1);
                                goodsList.Add(gs);
                            }

                            EmailAccess.Instance.SendEmail
                                (
                                "新手补偿",
                                TipManager.GetMessage(PetsReturn.StealPet12),
                                pb.ID,
                                pb.Name,
                                "因新手任务修改，可能会导致您的任务出错，请凉解!因此重置了您的任务，并且将您没有做的任务的道具通过邮件发送给您，因邮件存在过期时间，请尽快领取;特此申明!",
                                string.Empty,
                                goodsList, 15
                                );
                        }

                        if (pb.Level < 11)
                        {
                            pb.AddExperience(8200, FinanceType.GM, "2");
                        }
                        //移除任务内存
                        TaskAccess.Instance.Remove(pb.ID);
                    }
                    catch
                    {

                    }
                }
            }
            return "重置成功";
        }



        /// <summary>
        /// 得到道具ID
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static object GoodID(Notification note)
        {
            string[] strs = GMBusiness.GetCommand(note);
            if (strs.Length < 2) return null;
            GameConfig gc = GameConfigAccess.Instance.Find(MainType.Goods, null, strs[0].Trim());
            if (gc == null)
                return null;
            string str = "道具【" + gc.Name + "】配置ID为【" + gc.ID + "】";
            return str;
        }

        /// <summary>
        /// 道具赠送
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static object GetGood(Notification note)
        {
            string[] strs = GMBusiness.GetCommand(note);
            if (strs.Length < 3) return null;

            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }
            string goodsid = strs[1];
            int number = 0;
            if (!int.TryParse(strs[2], out number))
                return null;
            Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
            Variant v = new Variant();
            if (strs.Length == 4)
            {
                if (strs[3] == "0")
                {
                    v.Add("Number0", number);
                }
                else
                {
                    v.Add("Number1", number);
                }
            }
            else
            {
                v.Add("Number1", number);
            }
            dic.Add(goodsid, v);
            player.AddGoods(dic, GoodsSource.GMGet);
            return ("道具赠送成功");
        }

        /// <summary>
        /// 道具移除
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static object GoodRemove(Notification note)
        {
            string[] strs = GMBusiness.GetCommand(note);
            if (strs.Length < 3) return null;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }
            string goodsid = strs[1];
            int number = 0;
            if (!int.TryParse(strs[2], out number))
                return null;


            PlayerEx b0 = player.B0;
            PlayerEx b1 = player.B1;
            PlayerEx eq = player.Equips;
            IList c = null;
            if (number == 0)
            {
                c = b0.Value.GetValue<IList>("C");
                foreach (Variant v in c)
                {
                    if (v.GetStringOrDefault("G") == goodsid)
                    {
                        BurdenManager.BurdenClear(v);
                    }
                }

                c = b1.Value.GetValue<IList>("C");
                foreach (Variant v in c)
                {
                    if (v.GetStringOrDefault("G") == goodsid)
                    {
                        BurdenManager.BurdenClear(v);
                    }
                }

                foreach (Variant v in eq.Value.Values)
                {
                    if (v.GetStringOrDefault("G") == goodsid)
                    {
                        GMUninstall(player, v);
                        BurdenManager.BurdenClear(v);
                    }
                }
            }
            else
            {
                int total = 0;
                c = b0.Value.GetValue<IList>("C");
                foreach (Variant v in c)
                {
                    if (v.GetStringOrDefault("G") == goodsid)
                    {
                        int m = number - total;
                        int n = v.GetIntOrDefault("A");
                        if (m >= n)
                        {
                            BurdenManager.BurdenClear(v);
                            total += n;
                        }
                        else
                        {
                            v["A"] = n - m;
                            total += m;
                        }

                        if (total >= number)
                            break;
                    }
                }

                if (total < number)
                {
                    c = b1.Value.GetValue<IList>("C");
                    foreach (Variant v in c)
                    {
                        if (v.GetStringOrDefault("G") == goodsid)
                        {
                            int m = number - total;
                            int n = v.GetIntOrDefault("A");
                            if (m >= n)
                            {
                                BurdenManager.BurdenClear(v);
                                total += n;
                            }
                            else
                            {
                                v["A"] = n - m;
                                total += m;
                            }

                            if (total >= number)
                                break;
                        }
                    }
                }

                if (total < number)
                {
                    foreach (Variant v in eq.Value.Values)
                    {
                        if (v.GetStringOrDefault("G") == goodsid)
                        {
                            GMUninstall(player, v);
                            total++;
                            BurdenManager.BurdenClear(v);
                            if (total >= number)
                                break;
                        }
                    }
                }
            }
            b0.Save();
            b1.Save();
            eq.Save();

            if (player.Online)
            {
                player.UpdateBurden("B0", "B1");
                player.Call(GoodsCommand.GetEquipPanelR, true, eq);
            }
            return "道具移除成功";
        }

        /// <summary>
        /// 添加宠物经验
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static object PetExp(Notification note)
        {
            string[] strs = GMBusiness.GetCommand(note);
            if (strs.Length < 2) return null;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }
            if (player.Pet == null)
            {
                return ("没有出征宠物!");
            }

            int number = 0;
            if (!int.TryParse(strs[1], out number))
                return null;

            player.AddPetExp(player.Pet, number, true, (int)FinanceType.GM);
            string str = string.Format("成功给【{0}】的宠物添加{1}点经验", player.Name, number);
            return str;
        }


        /// <summary>
        /// 邮件查询
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static object SelectEmail(Notification note)
        {
            string[] strs = GMBusiness.GetCommand(note);
            try
            {
                if (runing) return null;
                runing = true;
                if (strs.Length < 3)
                    return null;

                PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
                if (player == null)
                    return null;
                int type = 0;
                if (!int.TryParse(strs[1].Trim(), out type))
                    return null;
                int pageSize = 0;
                if (!int.TryParse(strs[2].Trim(), out pageSize))
                    return null;
                int pageIndex = 0;
                if (!int.TryParse(strs[3].Trim(), out pageIndex))
                    return null;
                int status = 0;
                if (!int.TryParse(strs[4].Trim(), out status))
                    return null;

                int total = 0;
                List<Email> emailList = EmailAccess.Instance.GMEmailList(player.ID, type, pageSize, pageIndex, status, out total);
                List<Variant> list = new List<Variant>();
                foreach (Email model in emailList)
                {
                    Variant mv = model.Value;
                    if (mv == null)
                        continue;
                    Variant v = new Variant();
                    foreach (var item in mv)
                    {
                        v.Add(item.Key, item.Value);
                    }
                    v.Add("ID", model.ID);
                    v.Add("Name", model.Name);
                    v.Add("MainType", model.MainType);
                    v.Add("Status", model.Status);
                    list.Add(v);
                }

                return new object[] { total, list };
            }
            finally
            {
                runing = false;
            }
        }

        /// <summary>
        /// 家族设置
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public static object FamilySite(Notification note)
        {
            //家族名称
            string familyname = note.GetString(0);
            //角色名称
            string playername = note.GetString(1);
            //家族职业
            int roleid = note.GetInt32(2);

            //FamilyAccess.Instance.f

            PlayerBusiness player = PlayersProxy.FindPlayerByName(playername);
            if (player == null)
                return null;
            return null;
        }

        /// <summary>
        /// GM删除邮件
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static object GMDelEmail(Notification note)
        {
            string[] strs = GMBusiness.GetCommand(note);
            if (strs.Length < 2)
                return null;

            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
                return null;
            string[] de = strs[1].Trim().Split('|');

            return EmailAccess.Instance.GMRemoveEmail(de, player.ID);
        }


        /// <summary>
        /// 添加角色技能
        /// </summary>
        /// <param name="note"></param>
        public static object Skill(Notification note)
        {
            string[] strs = GMBusiness.GetCommand(note);
            if (strs.Length < 3) return null;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }
            string skillid = strs[1].Trim();
            int level = Convert.ToInt32(strs[2]);//等级
            GameConfig gc = GameConfigAccess.Instance.FindOneById(skillid);
            if (gc == null || gc.Value == null)
                return null;
            PlayerEx sk = player.Skill;
            if (sk == null)
                return null;
            Variant v = sk.Value;
            if (v == null)
                return null;
            object o;
            if (v.TryGetValueT(skillid, out o))
            {

            }
            return null;
        }



        /// <summary>
        /// 添加宠物技能
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static object PSkill(Notification note)
        {
            string[] strs = GMBusiness.GetCommand(note);
            if (strs.Length < 3) return null;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }
            string skillid = strs[1].Trim();
            int level = Convert.ToInt32(strs[2]);//等级
            GameConfig gc = GameConfigAccess.Instance.FindOneById(skillid);

            if (gc == null || gc.Value == null)
                return null;

            if (!gc.Value.ContainsKey(level.ToString()))
                return null;

            PlayerEx petBook = player.PetBook;
            Variant v = petBook.Value;
            object o;
            if (v.TryGetValueT(skillid, out o))
            {
                IList list = o as IList;
                if (list.Contains(level))
                {
                    return ("该技能已经成存!");
                }
                list.Add(level);
            }
            else
            {
                v.Add(skillid, new List<int> { level });
            }
            petBook.Save();
            player.Call(ClientCommand.UpdateActorR, new PlayerExDetail(petBook));
            string str = string.Format("【{0}】学习宠物技能【{1}】成功", player.Name, gc.Name);
            return str;
        }

        internal static object AddBond(Notification note)
        {
            string[] strs = GMBusiness.GetCommand(note);
            if (strs.Length < 2) return null;

            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                return string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]);
            }
            int bond = Convert.ToInt32(strs[1]);
            if (player.AddBond(bond, FinanceType.GM, "GM"))
            {
                return ("为【" + player.Name + "】充入点券:" + bond);
            }
            return null;

        }

        /// <summary>
        /// GM卸装
        /// </summary>
        /// <param name="player"></param>
        /// <param name="v"></param>
        private static void GMUninstall(PlayerBusiness player, Variant v)
        {
            Goods g = GoodsAccess.Instance.GetGoodsByID(v.GetStringOrDefault("E"), player.ID);
            if (g != null)
            {
                string goodsType = g.Value.GetStringOrDefault("GoodsType");
                Variant shengTi = RoleManager.Instance.GetAllRoleConfig(player.RoleID);
                string name = string.Empty;
                string value = string.Empty;
                bool ischange = false;
                switch (goodsType)
                {
                    case "111000":
                        //时装
                        name = "Coat";
                        value = shengTi.GetStringOrDefault("Coat");
                        player.Coat = value;
                        ischange = true;
                        break;
                    case "111001":
                        //武器
                        name = "Weapon";
                        value = shengTi.GetStringOrDefault("Weapon");
                        player.Weapon = value;
                        ischange = true;
                        break;
                    case "111003":
                        //衣服
                        name = "Body";
                        value = shengTi.GetStringOrDefault("Body");
                        player.Body = value;
                        ischange = true;
                        break;
                    case "111010":
                        //坐骑
                        name = "Mount";
                        value = shengTi.GetStringOrDefault("Mount");
                        player.Mount = value;
                        ischange = true;
                        break;
                }

                if (ischange)
                {
                    player.SaveClothing();
                }
                player.RefreshPlayer(name, value);
            }
            player.Call(GoodsCommand.UninstallR, true, g.GoodsID);
        }

    }
}
