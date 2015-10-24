using System;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Util;
using Sinan.Log;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.EmailModule.Business
{
    class EmailBusiness
    {
        static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 得到邮件列表
        /// </summary>
        /// <param name="note"></param>
        public static void EmailList(UserNote note)
        {
            int pageIndex = note.GetInt32(0);
            int pageSize = 6;//note.GetInt32(1);
            //0所有按时间，1未读，2有附件,3到期时间
            int number = note.GetInt32(2);
            if (pageIndex < 0 || pageSize < 1)
            {
                note.Call(EmailCommand.EmailListR, 0, new List<Variant>());
                return;
            }
            int total = 0;
            List<Email> emailList = EmailAccess.Instance.EmailPage(note.PlayerID, pageSize, pageIndex, number, out total);

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

            note.Call(EmailCommand.EmailListR, total, list);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="note"></param>
        public static void SendEmail(UserNote note)
        {
            Variant d = note.GetVariant(0);
            //标题
            string title = d.GetStringOrDefault("mailtitle");
            if (title.Length > 20)
            {
                note.Call(EmailCommand.SendEmailR, false, TipManager.GetMessage(EmailReturn.EmailTitalLength));
                return;
            }
            if (d.GetStringOrEmpty("mailMess").Length > 300)
            {
                note.Call(EmailCommand.SendEmailR, false, TipManager.GetMessage(EmailReturn.EmailContent));
                return;
            }
            //mailMess
            IList goodsList = d.GetValue<IList>("goodsList");
            int fee = 5;//要扣除的邮费
            int score = 0;
            if (d.ContainsKey("moneyGoods"))
            {
                Variant mg = d.GetVariantOrDefault("moneyGoods");
                score = mg.GetIntOrDefault("Score");
                if (mg != null)
                {
                    fee += Convert.ToInt32(Math.Ceiling(score * 0.01));
                }
            }

            if (goodsList.Count > 0)
            {
                fee += goodsList.Count * 10;
            }
            //发送物品物数量

            if (note.Player.Score < (fee + score))
            {
                //游戏币不足
                note.Call(EmailCommand.SendEmailR, false, TipManager.GetMessage(EmailReturn.NoScore));
                return;
            }

            PlayerEx b0 = note.Player.B0;
            IList c = b0.Value.GetValue<IList>("C");

            if (!IsCheck(note.Player, goodsList, c))
                return;



            string name = d.GetStringOrDefault("playerName");
            if (note.Player.Name == name)
            {
                note.Call(EmailCommand.SendEmailR, false, TipManager.GetMessage(EmailReturn.NoSelf));
                return;
            }

            PlayerBusiness pb = PlayersProxy.FindPlayerByName(name);
            if (pb == null)
            {
                note.Call(EmailCommand.SendEmailR, false, TipManager.GetMessage(EmailReturn.NoExists));
                return;
            }



            if (!note.Player.AddScore(-(fee + score), FinanceType.EmailFee, string.Format("{0},{1},{2}", fee, score, pb.ID)))
            {
                note.Call(EmailCommand.SendEmailR, false, TipManager.GetMessage(EmailReturn.NoScore));
                return;
            }


            Variant dic = new Variant();
            //bool isChange = false;
            foreach (Variant gs in goodsList)
            {
                foreach (Variant con in c)
                {
                    if (con.GetIntOrDefault("P") != gs.GetIntOrDefault("P"))
                        continue;
                    //如果道具非绑定不能发送
                    string goodsid = con.GetStringOrDefault("G");
                    int num = con.GetIntOrDefault("A");
                   
                    BurdenManager.BurdenClear(con);
                    note.Player.UpdateTaskGoods(goodsid);

                    //记录邮寄道具情况
                    dic.SetOrInc(goodsid, num);
                }
            }

            string mid = "";
            if (b0.Save())
            {
                Email model = new Email();
                model.ID = ObjectId.GenerateNewId().ToString();
                model.Name = string.IsNullOrEmpty(title) ? string.Format(TipManager.GetMessage(EmailReturn.SendEmail1), note.Player.Name) : title;
                model.Status = 0;
                model.Ver = 1;
                model.MainType = EmailCommand.Personal;
                model.Created = DateTime.UtcNow;
                model.Value = EmailBase.CreateEmailValue(note.PlayerID, note.Player.Name, pb.ID, pb.Name, d);
                model.Save();


                if (pb.Online)
                {
                    //得到新邮件请查收
                    pb.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(pb.ID));
                }

                if (goodsList != null && goodsList.Count > 0)
                {
                    note.Player.UpdateBurden();
                }
                note.Call(EmailCommand.SendEmailR, true, EmailReturn.SendEmailSuccess);
                mid = model.ID;
            }

            if (dic.Count > 0) 
            {
                foreach (var item in dic) 
                {
                    note.Player.AddLog(Actiontype.GoodsUse, item.Key, Convert.ToInt32(item.Value), GoodsSource.SendEmail, pb.Name, 0);
                }
            }
        }

        /// <summary>
        /// 更新邮件
        /// </summary>
        /// <param name="note"></param>
        public static void UpdateEmail(UserNote note)
        {
            bool isUpdate = EmailAccess.Instance.UpdateEmail(note.GetString(0));
            note.Call(EmailCommand.UpdateEmailR, isUpdate);
        }

        /// <summary>
        /// 新邮件条数
        /// </summary>
        /// <param name="note"></param>
        public static void NewEmailTotal(UserNote note)
        {
            int Total = EmailAccess.Instance.NewTotal(note.PlayerID);
            note.Call(EmailCommand.NewEmailTotalR, Total);
        }

        /// <summary>
        /// 邮件删除
        /// </summary>
        /// <param name="note"></param>
        public static void DelEmail(UserNote note)
        {
            IList id = note[0] as IList;

            if (id.Count > 0)
            {

                if (EmailAccess.Instance.RemoveEmail(id, note.PlayerID))
                {
                    note.Call(EmailCommand.DelEmailR, true);
                }
                else
                {
                    note.Call(EmailCommand.DelEmailR, false, TipManager.GetMessage(EmailReturn.EmailFuJian));
                }
            }
            else
            {
                note.Call(EmailCommand.DelEmailR, false, TipManager.GetMessage(EmailReturn.DataError));
            }
        }

        #region
        /// <summary>
        /// 邮件物品提取
        /// </summary>
        /// <param name="note"></param>
        public static void ExtractGoods(UserNote note)
        {
            if (!m_dic.TryAdd(note.PlayerID, note.PlayerID))
                return;
            try
            {
                string EmailID = note.GetString(0);
                string GoodsID = note.GetString(1);
                Email email = EmailAccess.Instance.FindOneById(EmailID);
                if (email == null)
                {
                    note.Call(EmailCommand.ExtractGoodsR, false, TipManager.GetMessage(EmailReturn.NoEmail));
                    return;
                }
                if (note.PlayerID != email.Value.GetStringOrDefault("ReceiveID"))
                {
                    note.Call(EmailCommand.ExtractGoodsR, false, TipManager.GetMessage(EmailReturn.NoReceiveID));
                    return;
                }


                //得到邮件的物品列表
                IList GoodsList = email.Value.GetValue<IList>("GoodsList");

                if (GoodsID == string.Empty)
                {
                    int s0 = email.Value.GetIntOrDefault("Score");//石币
                    if (s0 <= 0)
                    {
                        return;
                    }
                    email.Value["Coin"] = 0;
                    email.Value["Score"] = 0;
                    if (GoodsList != null)
                    {
                        if (GoodsList.Count == 0)
                        {
                            email.Value["IsHave"] = 0;
                        }
                    }
                    else
                    {
                        email.Value["IsHave"] = 0;
                    }
                    email.Save();
                    note.Player.AddScore(s0, FinanceType.ExtractGoods);
                    note.Call(EmailCommand.ExtractGoodsR, true, GoodsID);
                    return;
                }


                Variant v = null;
                foreach (Variant d in GoodsList)
                {
                    if (d.GetStringOrDefault("SoleID") == GoodsID)
                    {
                        v = d;
                        break;
                    }
                }

                if (v == null)
                {
                    note.Call(EmailCommand.ExtractGoodsR, false, TipManager.GetMessage(EmailReturn.NoReceiveID));
                    return;
                }
                //int Coin = v.GetIntOrDefault("Coin");
                int Score = v.GetIntOrDefault("Score");
                string SoleID = v.GetStringOrDefault("SoleID");
                //if (note.Player.Coin < Coin)
                //{
                //    //晶币不足
                //    note.Call(EmailCommand.ExtractGoodsR, false, TipManager.GetMessage(EmailReturn.NoCoin));
                //    return;
                //}

                if (note.Player.Score < Score)
                {
                    //游戏币不足
                    note.Call(EmailCommand.ExtractGoodsR, false, TipManager.GetMessage(EmailReturn.NoScore));
                    return;
                }

                //货物提取后，

                Email ev = new Email();
                ev.ID = ObjectId.GenerateNewId().ToString();
                ev.Name = email.Name;
                ev.Status = 0;
                ev.Ver = 1;
                ev.MainType = EmailCommand.System;
                ev.Created = DateTime.UtcNow;

                Variant gs = new Variant();
                Variant moneyGoods = new Variant();
                //moneyGoods.Add("Coin", Coin);
                moneyGoods.Add("Score", Score);
                gs.Add("moneyGoods", moneyGoods);
                gs.Add("GoodsList", new List<Variant>());
                gs.Add("mailMess", "");
                gs.Add("reTime", 15);
                ev.Value = EmailBase.CreateEmailValue(EmailCommand.System, TipManager.GetMessage(EmailReturn.ExtractGoods1), email.Value.GetStringOrDefault("SendID"), email.Value.GetStringOrDefault("SendName"), gs);
                Goods g = GoodsAccess.Instance.FindOneById(GoodsID);
                //普通包袱
                PlayerEx burden = note.Player.B0;
                IList c = burden.Value.GetValue<IList>("C");
                if (g != null)
                {
                    Variant n = BurdenManager.GetBurdenSpace(c);
                    if (n == null)
                    {
                        note.Call(EmailCommand.ExtractGoodsR, false, TipManager.GetMessage(EmailReturn.BurdenFull));
                        return;
                    }

                    if (!note.Player.AddScore(-Score, FinanceType.ExtractGoods))
                    {
                        note.Call(EmailCommand.ExtractGoodsR, false, TipManager.GetMessage(EmailReturn.NoScore));
                        return;
                    }

                    n["E"] = g.ID;
                    n["G"] = g.GoodsID;
                    n["A"] = 1;
                    n["S"] = g.Value.GetIntOrDefault("Sort");
                    n["H"] = 0;
                    n["D"] = 0;


                    Variant tmp = new Variant();
                    if (g.Value.ContainsKey("BaoShiInfo"))
                    {
                        tmp.Add("BaoShiInfo", g.Value.GetValue<object>("BaoShiInfo"));
                    }
                    if (g.Value.ContainsKey("Stamina"))
                    {
                        Variant Stamina = g.Value.GetVariantOrDefault("Stamina");
                        tmp.Add("Stamina", Stamina.GetIntOrDefault("V"));
                    }
                    if (g.Value.ContainsKey("PetsWild"))
                    {
                        tmp.Add("PetsWild", g.Value.GetIntOrDefault("PetsWild"));
                    }

                    n["T"] = tmp;

                    g.PlayerID = note.PlayerID;
                    g.Save();
                    burden.Save();
                    GoodsList.Remove(v);
                    if (email.Value.GetIntOrDefault("Coin") <= 0 && email.Value.GetIntOrDefault("Score") <= 0 && GoodsList.Count == 0)
                    {
                        email.Value["IsHave"] = 0;
                    }
                    email.Save();


                    note.Call(EmailCommand.ExtractGoodsR, true, GoodsID);
                    if (Score != 0)
                    {
                        ev.Save();
                        PlayerBusiness OnLineBusiness = PlayersProxy.FindPlayerByID(email.Value.GetStringOrDefault("SendID"));
                        if (OnLineBusiness != null && OnLineBusiness.Online)
                        {
                            OnLineBusiness.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(note.PlayerID));
                        }
                    }
                    note.Player.UpdateTaskGoods(g.GoodsID);
                    //Variant list = new Variant();
                    //list.Add("B0", burden);
                    //note.Call(BurdenCommand.BurdenListR, list);
                    note.Player.UpdateBurden();
                    return;
                }

                GameConfig gc = GameConfigAccess.Instance.FindOneById(SoleID);
                if (gc != null)
                {
                    //道具数量
                    int num = v.GetIntOrDefault("Number");
                    //是否绑定
                    int h = v.GetIntOrDefault("H");
                    Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
                    Variant mv = new Variant();
                    if (h == 0)
                    {
                        mv.Add("Number0", num);
                    }
                    else
                    {
                        mv.Add("Number1", num);
                    }
                    dic.Add(SoleID, mv);
                    //得到堆叠数

                    if (BurdenManager.IsFullBurden(note.Player.B0, dic))
                    {
                        note.Call(EmailCommand.ExtractGoodsR, false, TipManager.GetMessage(EmailReturn.BurdenFull));
                        return;
                    }

                    if (Score > 0)
                    {
                        if (!note.Player.AddScore(-Score, FinanceType.ExtractGoods))
                        {
                            note.Call(EmailCommand.ExtractGoodsR, false, TipManager.GetMessage(EmailReturn.NoScore));
                            return;
                        }
                    }

                    GoodsList.Remove(v);
                    if (email.Value.GetIntOrDefault("Coin") <= 0 && email.Value.GetIntOrDefault("Score") <= 0 && GoodsList.Count == 0)
                    {
                        email.Value["IsHave"] = 0;
                    }
                    email.Save();
                    note.Call(EmailCommand.ExtractGoodsR, true, GoodsID);
                    note.Player.AddGoods(dic, GoodsSource.GM);
                    return;
                }
                note.Call(EmailCommand.ExtractGoodsR, false, TipManager.GetMessage(EmailReturn.DataError));
            }
            finally
            {
                string n;
                m_dic.TryRemove(note.PlayerID, out n);
            }
        }
        #endregion


        /// <summary>
        /// 取出邮件物品
        /// </summary>
        /// <param name="note"></param>
        public static void GetEmailGoods(UserNote note)
        {
            string soleid = note.PlayerID + "GetEmailGoods";
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                string emailid = note.GetString(0);
                Email model = EmailAccess.Instance.FindOneById(emailid);
                if (model == null)
                {
                    note.Call(EmailCommand.ExtractGoodsR, false, TipManager.GetMessage(EmailReturn.NoEmail));
                    return;
                }
                //是否操作邮件
                if (model.Ver != 10)
                {
                    if (model.Status > 1)
                    {
                        note.Call(EmailCommand.ExtractGoodsR, false, TipManager.GetMessage(EmailReturn.GetEmailGoods1));
                        // 物品已经提取,不能重复领取
                        return;
                    }
                }
                
                Variant v = model.Value;
                if (note.PlayerID != v.GetStringOrDefault("ReceiveID"))
                {
                    note.Call(EmailCommand.ExtractGoodsR, false, TipManager.GetMessage(EmailReturn.NoReceiveID));
                    return;
                }
                if (v.GetIntOrDefault("IsHave") != 1)
                {
                    note.Call(EmailCommand.ExtractGoodsR, false, TipManager.GetMessage(EmailReturn.GetEmailGoods2));
                    return;
                }
                PlayerEx b0 = note.Player.B0;
                Variant bv = b0.Value;
                IList c = bv.GetValue<IList>("C");
                IList goodslist = v.GetValue<IList>("GoodsList");
                if (goodslist != null)
                {
                    int count = BurdenManager.BurdenSpace(c);
                    if (goodslist.Count > count)
                    {
                        note.Call(EmailCommand.ExtractGoodsR, false, TipManager.GetMessage(EmailReturn.BurdenFull));
                        return;
                    }
                }

                model.Status = 2;
                if (model.Save())
                {
                    int score = v.GetIntOrDefault("Score");
                    if (score > 0)
                    {
                        note.Player.AddScore(score, FinanceType.ExtractGoods);

                    }

                    if (goodslist != null)
                    {
                        Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
                        foreach (Variant item in goodslist)
                        {
                            string gid = item.GetStringOrDefault("SoleID");
                            if (gid != item.GetStringOrDefault("GoodsID"))
                            {
                                Goods g = GoodsAccess.Instance.FindOneById(item.GetStringOrDefault("SoleID"));
                                if (g == null)
                                    continue;
                                g.PlayerID = note.PlayerID;
                                if (!g.Save())
                                    continue;
                                Variant gv = g.Value;
                                Variant m = BurdenManager.GetBurdenSpace(c);
                                if (m == null)
                                    continue;
                                m["E"] = g.ID;
                                m["G"] = g.GoodsID;
                                m["A"] = 1;//只能一件物品
                                m["S"] = gv.GetIntOrDefault("Sort");
                                m["H"] = 0;
                                m["D"] = 0;
                                m["T"] = null;
                                //任务更新
                                note.Player.UpdateTaskGoods(g.GoodsID);

                                //邮件日志
                                PlayerLog log = new PlayerLog(ServerLogger.zoneid, Actiontype.AddGoods);
                                log.itemcnt = 1;
                                log.itemtype = g.GoodsID;
                                log.reserve_1 = (int)GoodsSource.ExtractGoods;
                                
                                log.remark = emailid;                                
                                note.Player.WriteLog(log);
                            }
                            else
                            {
                                GameConfig gc = GameConfigAccess.Instance.FindOneById(gid);
                                if (gc == null)
                                    continue;
                                int num = item.GetIntOrDefault("Number");
                                int h = item.GetIntOrDefault("H");
                                h = h > 0 ? 1 : 0;
                                Variant tmp;
                                if (dic.TryGetValue(gid, out tmp))
                                {
                                    tmp.SetOrInc("Number" + h, num);
                                }
                                else
                                {
                                    tmp = new Variant();
                                    tmp.SetOrInc("Number" + h, num);
                                    dic.Add(gid, tmp);
                                }
                            }
                        }
                        //判断邮件状态
                        if (model.Ver == 10)
                        {                            
                            BurdenManager.BurdenBatchInsert(note.Player.B0, dic);
                            model.Ver = 5;
                            model.Save();
                        }
                        else
                        {
                            note.Player.AddGoods(dic, GoodsSource.ExtractGoods, emailid);
                        }
                    }
                    note.Call(EmailCommand.ExtractGoodsR, true, emailid);

                    if (goodslist != null && goodslist.Count > 0)
                    {
                        note.Player.UpdateBurden();
                    }
                }
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        } 

        /// <summary>
        /// 创建角色新建邮件
        /// </summary>
        /// <param name="note"></param>
        public static void SystemEmail(UserNote note)
        {
            Player player = note[0] as Player;

            string[] msg = TipManager.GetMessage(EmailReturn.SendNewEmail).Split('|');
            //角色创建成功
            if (msg.Length > 2)
            {
                int reTime = Convert.ToInt32(TipManager.GetMessage(EmailReturn.HameDay));
                EmailAccess.Instance.SendEmail(msg[0], msg[1], player.ID, player.Name, msg[2], player.Name, null, reTime);
            }
        }


        public static void CallEmail(UserNote note)
        {
            //MailAddress address = new MailAddress("", string.Empty, Encoding.UTF8);
            //MailMessage mail = new MailMessage();
            //mail.Subject = "asdfasdf";
            //mail.From = address;
            //Dictionary<string, string> dic = new Dictionary<string, string>();
            //foreach (string k in dic.Keys)
            //{
            //    mail.To.Add(new MailAddress(k, dic[k]));
            //}
            //mail.CC.Add(new MailAddress("Manage@hotmail.com", "尊敬的领导"));
            //mail.Body = string.Empty;
            //mail.BodyEncoding = Encoding.UTF8;
            //mail.IsBodyHtml = true;
            //mail.Priority = MailPriority.Normal;
            //mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;

            //SmtpClient client = new SmtpClient();
            //client.Host = "smtp.hotmail.com";
            //client.Port = 25;
            //client.UseDefaultCredentials = false;
            //client.Credentials = new NetworkCredential("", string.Empty);
            //client.Send(mail);
        }

        /// <summary>
        /// 判断数据道具是否合法
        /// </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        private static bool IsCheck(PlayerBusiness pb, IList gs, IList c)
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();

            foreach (Variant n in gs)
            {
                foreach (Variant k in c)
                {
                    int p = n.GetIntOrDefault("P");
                    if (p != k.GetIntOrDefault("P"))
                        continue;

                    if (k.GetIntOrDefault("H") == 1)
                    {
                        pb.Call(EmailCommand.SendEmailR, false, TipManager.GetMessage(DealReturn.IsBinding));
                        return false;
                    }

                    if (n.GetStringOrDefault("E") != k.GetStringOrDefault("E"))
                    {
                        pb.Call(EmailCommand.SendEmailR, false, TipManager.GetMessage(DealReturn.NumberError));
                        return false;
                    }

                    if (n.GetIntOrDefault("A") != k.GetIntOrDefault("A"))
                    {
                        pb.Call(EmailCommand.SendEmailR, false, TipManager.GetMessage(DealReturn.NumberError));
                        return false;
                    }

                    Variant t = k.GetVariantOrDefault("T");
                    if (t != null)
                    {
                        if (t.ContainsKey("EndTime"))
                        {
                            pb.Call(EmailCommand.SendEmailR, false, TipManager.GetMessage(DealReturn.IsCheck1));
                            return false;
                        }
                    }
                    n["T"] = t;//物品变更信息
                    if (dic.ContainsKey(p))
                    {
                        pb.Call(EmailCommand.SendEmailR, false, TipManager.GetMessage(DealReturn.IsCheck2));
                        return false;
                    }
                    dic.Add(p, p);
                    break;
                }
            }
            return true;
        }



        /// <summary>
        /// 夺宝奇兵家族奖励分配
        /// </summary>
        /// <param name="note"></param>
        public static void RobFamilyAward(UserNote note)
        {
            Variant v = note.GetVariant(0);
            PlayerEx familyEx = note.Player.Family;
            string familyID = familyEx.Value.GetStringOrDefault("FamilyID");
            if (string.IsNullOrEmpty(familyID))
                return;
            Family model = FamilyAccess.Instance.FindOneById(familyID);
            if (model == null)
                return;
            IList persons = model.Value.GetValue<IList>("Persons");
            long TotalDevote = 0;//总的共献度
            foreach (Variant k in persons)
            {
                TotalDevote += k.GetIntOrDefault("Devote");
            }

            foreach (Variant k in persons)
            {
                long countL = (k.GetIntOrDefault("Devote") * v.GetIntOrDefault("Count")) / TotalDevote;
                int count = (int)countL;
                if (count > 0)
                {
                    PlayerBusiness pb = PlayersProxy.FindPlayerByID(k.GetStringOrDefault("PlayerID"));
                    if (pb != null)
                    {
                        string title = TipManager.GetMessage(EmailReturn.RobFamilyAward1);
                        string content = TipManager.GetMessage(EmailReturn.RobFamilyAward2);
                        int reTime = Convert.ToInt32(TipManager.GetMessage(EmailReturn.HameDay));
                        Variant gs = new Variant();
                        gs.Add("G", v.GetStringOrDefault("Goods"));
                        gs.Add("A", count);
                        gs.Add("E", v.GetStringOrDefault("Goods"));

                        List<Variant> goodsList = new List<Variant>() { gs };

                        if (EmailAccess.Instance.SendEmail(title, TipManager.GetMessage(EmailReturn.RobFamilyAward3), pb.ID, pb.Name, content, string.Empty, goodsList, reTime))
                        {
                            if (pb.Online)
                            {
                                pb.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(pb.ID));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 退回邮件
        /// </summary>
        /// <param name="note"></param>
        public static void ExitEmail(UserNote note)
        {
            string emailid = note.GetString(0);
            Email model = EmailAccess.Instance.FindOneById(emailid);
            if (model != null)
            {
                if (model.MainType == EmailCommand.System)
                {
                    note.Call(EmailCommand.ExitEmailR, false, TipManager.GetMessage(EmailReturn.IsSystem));
                    return;
                }
                IList goodsList = model.Value.GetValue<IList>("GoodsList");
                if (model.Value.GetIntOrDefault("Coin") == 0 && model.Value.GetIntOrDefault("Score") == 0 && goodsList.Count == 0)
                {
                    note.Call(EmailCommand.ExitEmailR, false, TipManager.GetMessage(EmailReturn.NoRider));
                    return;
                }

                //发送者
                string sendid = model.Value.GetStringOrDefault("SendID");
                string sendname = model.Value.GetStringOrDefault("SendName");
                //接收者
                string receiveid = model.Value.GetStringOrDefault("ReceiveID");
                string receivename = model.Value.GetStringOrDefault("ReceiveName");

                model.MainType = EmailCommand.System;
                model.Value["SendID"] = EmailCommand.System;
                model.Value["SendName"] = EmailCommand.System;
                model.Name = model.Name;
                model.Value["ReceiveID"] = sendid;
                model.Value["ReceiveName"] = sendname;
                model.Status = 0;
                model.Ver = 10;
                DateTime dt = DateTime.UtcNow;
                model.Value["UpdateDate"] = dt;

                int reTime = Convert.ToInt32(TipManager.GetMessage(EmailReturn.HameDay));
                //过期时间
                model.Value["EndDate"] = dt.AddDays(reTime);
                model.Save();

                PlayerBusiness pb = PlayersProxy.FindPlayerByID(sendid);

                if (pb != null && pb.Online)
                {
                    //得到新邮件请查收
                    pb.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(sendid));
                }
                note.Call(EmailCommand.ExitEmailR, true, TipManager.GetMessage(EmailReturn.ExitSuccess));
            }
        }


    }
}
