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

namespace Sinan.ManageModule
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
        public static void ExitAll(GMNote note)
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
                    note.Call(GMCommand.GMR, str);
                    //Environment.Exit(0);
                    return;
                }
            }
            string str2 = string.Format("确定要停止应用程序,请在10秒内重新发该指令");
            note.Call(GMCommand.GMR, (str2));
        }

        internal static void ReStart(GMNote note, string[] comm)
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
            note.Call(GMCommand.GMR, str);
        }


        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void AddCoin(GMNote note, string[] strs)
        {
            if (strs.Length < 2) return;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]));
                return;
            }
            int coin = Convert.ToInt32(strs[1]);
            player.AddCoin(coin, FinanceType.GM);

            UserNote note1 = new UserNote(player, PartCommand.Recharge, new object[] { coin });
            Notifier.Instance.Publish(note1);
            note.Call(GMCommand.GMR, "为【" + player.Name + "】充值晶币:" + coin);
        }

        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void AddScore(GMNote note, string[] strs)
        {
            if (strs.Length < 2) return;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]));
                return;
            }
            int score = Convert.ToInt32(strs[1]);
            player.AddScore(score, FinanceType.GM);
            note.Call(GMCommand.GMR, "为【" + player.Name + "】充值游戏币:" + score);
        }

        /// <summary>
        /// 添加经验
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void Exp(GMNote note, string[] strs)
        {
            if (strs.Length < 2) return;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]));
                return;
            }
            int exp = 0;
            if (int.TryParse(strs[1], out exp))
            {
                player.AddExperience(exp, FinanceType.GM);
                string str = string.Format(TipManager.GetMessage(GMReturn.AddExp), player.Name);
                note.Call(GMCommand.GMR, str);
            }
        }

        /// <summary>
        /// 得到任务ID
        /// </summary>
        /// <param name="note"></param>
        public static void GetTaskID(GMNote note, string[] strs)
        {
            GameConfig gc = GameConfigAccess.Instance.Find(MainType.Task, null, strs[0].Trim());
            if (gc == null) return;
            string str = "任务【" + gc.Name + "】配置ID为【" + gc.ID + "】";
            note.Call(GMCommand.GMR, str);
        }

        /// <summary>
        /// 移除某角色某个任务
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void TaskRemove(GMNote note, string[] strs)
        {
            if (strs.Length < 2) return;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]));
                return;
            }
            List<string> list = new List<string>();
            list.Add(strs[1].Trim());
            List<Task> tasks = TaskAccess.Instance.FindConfigList(player.ID, list);
            foreach (Task k in tasks)
            {
                TaskAccess.Instance.Remove(player.ID, new List<string> { k.Value.GetStringOrDefault("TaskID") });
                TaskManage.TaskGiveup(player, k, false);
                note.Call(GMCommand.GMR, ("移除任务【" + k.Value.GetStringOrDefault("TaskID") + "】成功"));
            }
        }

        /// <summary>
        /// 触发某个任务
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void TaskAct(GMNote note, string[] strs)
        {
            if (strs.Length < 2) return;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]));
                return;
            }
            GameConfig gc = GameConfigAccess.Instance.FindOneById(strs[1]);
            if (gc == null) return;

            TaskManage.TaskBack(player, gc);
            string str = string.Format("任务【{0}】触发成功", gc.Name);
            note.Call(GMCommand.GMR, str);
        }

        

        /// <summary>
        /// 任务重置
        /// </summary>
        /// <param name="note"></param>
        public static void TaskReset(GMNote note)
        {
            string connectionString = ConfigLoader.Config.DbPlayer;
            var db = MongoDatabase.Create(connectionString);
            //取得角色列表
            var mc = db.GetCollection("Player");

            IList nowtask = note.GetValue<IList>(1);
            if (nowtask == null || nowtask.Count == 0)
                return;
            string nowactid = note.GetString(2);//新触发的任务
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
                                return;
                            TaskManage.TaskBack(pb, gc);
                            LogWrapper.Warn(pb.UserID + "," + pb.Name + "," + t + "," + level);

                            List<Variant> goodsList = new List<Variant>(); 
                            List<string> g=  dic[pb.RoleID];
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
                                goodsList,15
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
            Console.WriteLine("任务重置处理完成...");
            note.Call(GMCommand.GMR, "重置成功");
        }
        


        /// <summary>
        /// 得到道具ID
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void GoodID(GMNote note, string[] strs)
        {
            if (strs.Length < 2) return;
            GameConfig gc = GameConfigAccess.Instance.Find(MainType.Goods, null, strs[0].Trim());
            if (gc == null)
                return;
            string str = "道具【" + gc.Name + "】配置ID为【" + gc.ID + "】";
            note.Call(GMCommand.GMR, str);
        }

        /// <summary>
        /// 道具赠送
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void GetGood(GMNote note, string[] strs)
        {
            if (strs.Length < 3) return;

            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]));
                return;
            }
            string goodsid = strs[1];
            int number = 0;
            if (!int.TryParse(strs[2], out number))
                return;
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
            player.AddGoods(dic,GoodsSource.GMGet);
            note.Call(GMCommand.GMR, ("道具赠送成功"));
        }

        /// <summary>
        /// 道具移除
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void GoodRemove(GMNote note, string[] strs)
        {
            if (strs.Length < 3) return;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]));
                return;
            }
            string goodsid = strs[1];
            int number = 0;
            if (!int.TryParse(strs[2], out number))
                return;


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
            note.Call(GMCommand.GMR, ("道具移除成功"));
        }

        /// <summary>
        /// 添加宠物经验
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void PetExp(GMNote note, string[] strs)
        {
            if (strs.Length < 2) return;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]));
                return;
            }
            if (player.Pet == null)
            {
                note.Call(GMCommand.GMR, ("没有出征宠物!"));
                return;
            }

            int number = 0;
            if (!int.TryParse(strs[1], out number))
                return;

            player.AddPetExp(player.Pet, number, true, (int)FinanceType.GM);
            string str = string.Format("成功给【{0}】的宠物添加{1}点经验", player.Name, number);
            note.Call(GMCommand.GMR, str);

        }

        
        /// <summary>
        /// 邮件查询
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void SelectEmail(GMNote note, string[] strs)
        {
            try
            {
                if (runing) return;
                runing = true;
                if (strs.Length < 3)
                    return;

                PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
                if (player == null)
                    return;
                int type = 0;
                if (!int.TryParse(strs[1].Trim(), out type))
                    return;
                int pageSize = 0;
                if (!int.TryParse(strs[2].Trim(), out pageSize))
                    return;
                int pageIndex = 0;
                if (!int.TryParse(strs[3].Trim(), out pageIndex))
                    return;
                int status = 0;
                if (!int.TryParse(strs[4].Trim(), out status))
                    return;

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

                note.Call(GMCommand.SelectEmailR, total, list);
            }
            finally 
            {
                runing = false;
            }
        }

        /// <summary>
        /// GM删除邮件
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void GMDelEmail(GMNote note, string[] strs)
        {
            if (strs.Length < 2)
                return;

            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
                return;
            string[] de = strs[1].Trim().Split('|');

            EmailAccess.Instance.GMRemoveEmail(de, player.ID);
        }




        /// <summary>
        /// 添加角色技能
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void Skill(GMNote note, string[] strs)
        {
            if (strs.Length < 3) return;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]));
                return;
            }
            string skillid = strs[1].Trim();
            int level = Convert.ToInt32(strs[2]);//等级
            GameConfig gc = GameConfigAccess.Instance.FindOneById(skillid);
            if (gc == null || gc.Value == null)
                return;
            PlayerEx sk = player.Skill;
            if (sk == null)
                return;
            Variant v = sk.Value;
            if (v == null)
                return;
            object o;
            if (v.TryGetValueT(skillid, out o))
            {

            }
        }

        /// <summary>
        /// 添加宠物技能
        /// </summary>
        /// <param name="note"></param>
        /// <param name="strs"></param>
        public static void PSkill(GMNote note, string[] strs)
        {
            if (strs.Length < 3) return;
            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]));
                return;
            }
            string skillid = strs[1].Trim();
            int level = Convert.ToInt32(strs[2]);//等级
            GameConfig gc = GameConfigAccess.Instance.FindOneById(skillid);

            if (gc == null || gc.Value == null)
                return;

            if (!gc.Value.ContainsKey(level.ToString()))
                return;

            PlayerEx petBook = player.PetBook;
            Variant v = petBook.Value;
            object o;
            if (v.TryGetValueT(skillid, out o))
            {
                IList list = o as IList;
                if (list.Contains(level))
                {
                    note.Call(GMCommand.GMR, ("该技能已经成存!"));
                    return;
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
            note.Call(GMCommand.GMR, str);
        }

        internal static void AddBond(GMNote note, string[] strs)
        {
            if (strs.Length < 2) return;

            PlayerBusiness player = PlayersProxy.FindPlayerByName(strs[0].Trim());
            if (player == null)
            {
                note.Call(GMCommand.GMR, string.Format(TipManager.GetMessage(GMReturn.NoName), strs[0]));
                return;
            }
            int bond = Convert.ToInt32(strs[1]);
            if (player.AddBond(bond, FinanceType.GM, "GM"))
            {
                string name = player.Name;
                note.Call(GMCommand.GMR, ("为【" + name + "】充入点券:" + bond));
            }
        }

        /// <summary>
        /// GM卸装
        /// </summary>
        /// <param name="player"></param>
        /// <param name="v"></param>
        private static void GMUninstall(PlayerBusiness player,Variant v) 
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
