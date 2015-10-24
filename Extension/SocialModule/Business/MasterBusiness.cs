using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.FrontServer;
using Sinan.Entity;
using Sinan.Util;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Data;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Sinan.SocialModule.Business
{
    class MasterBusiness
    {
        /// <summary>
        /// 拜师列表
        /// </summary>
        static Dictionary<string, List<Variant>> MasterList = new Dictionary<string, List<Variant>>();
        /// <summary>
        /// 拜师或收徒
        /// </summary>
        /// <param name="note"></param>
        public static void MasterApply(UserNote note)
        {
            //true拜师,false表示收徒
            bool IsMaster = note.GetBoolean(0);
            string name = note.GetString(1);



            if (note.Player.Name == name)
            {
                note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.NoSelf));
                return;
            }

            PlayerBusiness masterBase = PlayersProxy.FindPlayerByName(name);
            if (masterBase == null)
            {
                note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.UserInfoError));
                return;
            }

            if (!masterBase.Online)
            {
                //判断被申请者是否在线,如果不在线不能成功
                note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.NoOnLine));
                return;
            }

            if (MasterList.ContainsKey(note.PlayerID))
            {
                List<Variant> list = MasterList[note.PlayerID];
                int Number = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    Variant k = list[i];
                    DateTime EndDate;
                    if (!DateTime.TryParse(k.GetStringOrDefault("EndDate"), out EndDate))
                        continue;
                    if (EndDate < DateTime.UtcNow)
                    {
                        //过期
                        list.Remove(k);
                    }
                    else if (k.GetBooleanOrDefault("IsMaster") == IsMaster)
                    {
                        if (k.GetStringOrDefault("PlayerID") == masterBase.ID)
                        {
                            note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(IsMaster ? SocialReturn.IsMaster : SocialReturn.IsApprentice));
                            return;
                        }
                        Number += 1;
                    }
                }
                if (Number >= 10)
                {
                    note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.AppNumber));
                    return;
                }
            }

            //师傅信息
            PlayerEx MasterSocial = IsMaster ? masterBase.Social : note.Player.Social;
            //徒弟信息
            PlayerEx AppSocial = IsMaster ? note.Player.Social : masterBase.Social;

            if (MasterSocial == null || AppSocial == null)
            {
                note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.UserInfoError));
                return;
            }

            int MasterLevel = 0;//师傅等级
            int AppLevel = 0;//徒弟等级
            if (IsMaster)
            {
                MasterLevel = masterBase.Level;
                AppLevel = note.Player.Level;
            }
            else
            {
                MasterLevel = note.Player.Level;
                AppLevel = masterBase.Level;
            }



            if (AppLevel > 39)
            {
                //徒弟必须小于或等于39级
                note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.MasterNoLevel));
                return;
            }


            if (MasterLevel < 40)
            {
                //师傅发须大于39级
                note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.MasterLevelGap));
                return;
            }



            Variant MasterMentor = MasterSocial.Value.GetValueOrDefault<Variant>("Mentor");
            Variant AppMentor = AppSocial.Value.GetValueOrDefault<Variant>("Mentor");
            if (FreezeDate(MasterMentor))
            {
                //你申请的师傅正处在冷冻期
                note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.MasterApply3));
                return;
            }
            if (FreezeDate(AppMentor))
            {
                //徒弟是否是在冻结期
                note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.MasterApply4));
                return;
            }

            //判断师傅中是否已经是仇人,师傅和徒弟
            if (SocialBusiness.IsLet(MasterSocial, AppSocial.PlayerID, new List<string> { "Enemy" }))
            {
                note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.Enemy));
                return;
            }
            if (SocialBusiness.IsLet(MasterSocial, AppSocial.PlayerID, new List<string> { "Master" }))
            {
                note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.Master));
                return;
            }
            if (SocialBusiness.IsLet(MasterSocial, AppSocial.PlayerID, new List<string> { "Apprentice" }))
            {
                note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.Apprentice));
                return;
            }



            if (SocialBusiness.IsLet(AppSocial, MasterSocial.PlayerID, new List<string> { "Enemy" }))
            {
                note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.Enemy));
                return;
            }
            if (SocialBusiness.IsLet(AppSocial, MasterSocial.PlayerID, new List<string> { "Master" }))
            {
                note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.Master));
                return;
            }
            if (SocialBusiness.IsLet(AppSocial, MasterSocial.PlayerID, new List<string> { "Apprentice" }))
            {
                note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.Apprentice));
                return;
            }


            //徒弟师傅数量
            IList AppMaster = AppMentor.GetValue<IList>("Master");

            if (AppMaster != null)
            {
                if (AppMaster.Count >= 1)
                {
                    note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.MasterCount));
                    return;
                }
            }

            //师傅徒弟数量
            IList MasterApp = MasterMentor.GetValue<IList>("Apprentice");
            if (MasterApp != null)
            {
                if (!CheckApp(MasterApp))
                {
                    note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.ApprenticeCount));
                    return;
                }
                //if (MasterApp.Count >= 4)
                //{
                //    note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.ApprenticeCount));
                //    return;
                //}
            }


            Variant p = new Variant();
            p.Add("ID", note.PlayerID);
            p.Add("IsMaster", IsMaster);
            //发布时间
            p.Add("EndDate", DateTime.UtcNow.AddHours(72));
            p.Add("PlayerID", masterBase.ID);

            if (!MasterList.ContainsKey(note.PlayerID))
            {
                MasterList.Add(note.PlayerID, new List<Variant> { p });
            }
            else
            {
                List<Variant> list = MasterList[note.PlayerID];
                list.Add(p);
            }
            string str = note.PlayerID + "," + masterBase.ID;
            Variant v = new Variant();
            v.Add("ID", str);
            if (IsMaster)
            {
                v.Add("Message", string.Format(TipManager.GetMessage(SocialReturn.MasterApply1), note.Player.Name));                
            }
            else
            {
                v.Add("Message", string.Format(TipManager.GetMessage(SocialReturn.MasterApply2), note.Player.Name));                
            }
            masterBase.Call(SocialCommand.MasterApplyR, true, IsMaster, v);
        }

        /// <summary>
        /// 拜师或收徒回复
        /// </summary>
        /// <param name="note"></param>
        public static void MasterBack(UserNote note)
        {
            //true表示拜师，false表示收徒
            bool IsMaster = note.GetBoolean(0);
            string[] strs = note.GetString(1).Split(',');
            string plasyerid = strs[0];

            PlayerBusiness OnLineBusiness = PlayersProxy.FindPlayerByID(plasyerid);
            if (OnLineBusiness == null || (!OnLineBusiness.Online))
            {
                //判断被申请者是否在线,如果不在线不能成功
                note.Call(SocialCommand.MasterBackR, false, IsMaster, TipManager.GetMessage(SocialReturn.NoOnLine));
                return;
            }

            //师傅信息
            PlayerEx MasterSocial = IsMaster ? note.Player.Social : OnLineBusiness.Social;
            //徒弟信息
            PlayerEx AppSocial = IsMaster ? OnLineBusiness.Social : note.Player.Social;

            if (MasterSocial == null || AppSocial == null)
            {
                note.Call(SocialCommand.MasterBackR, false, IsMaster, TipManager.GetMessage(SocialReturn.UserInfoError));
                return;
            }

            int MasterLevel = 0;//师傅等级
            int AppLevel = 0;//徒弟等级
            if (IsMaster)
            {
                MasterLevel = note.Player.Level;
                AppLevel = OnLineBusiness.Level;
            }
            else
            {
                MasterLevel = OnLineBusiness.Level;
                AppLevel = note.Player.Level;
            }

            if (AppLevel > 39)
            {
                note.Call(SocialCommand.MasterBackR, false, IsMaster, TipManager.GetMessage(SocialReturn.MasterNoLevel));
                return;
            }


            if (MasterLevel < 40)
            {
                note.Call(SocialCommand.MasterBackR, false, IsMaster, TipManager.GetMessage(SocialReturn.MasterLevelGap));
                return;
            }




            Variant MasterMentor = MasterSocial.Value.GetValueOrDefault<Variant>("Mentor"); ;
            Variant AppMentor = AppSocial.Value.GetValueOrDefault<Variant>("Mentor");
            if (FreezeDate(MasterMentor))
            {
                //师傅正处在冷冻期
                note.Call(SocialCommand.MasterBackR, false, IsMaster, TipManager.GetMessage(SocialReturn.MasterApply3));
                return;
            }
            if (FreezeDate(AppMentor))
            {
                //徒弟正处在冷冻期
                note.Call(SocialCommand.MasterBackR, false, IsMaster, TipManager.GetMessage(SocialReturn.MasterApply4));
                return;
            }

            //判断师傅中是否已经是仇人,师傅和徒弟
            if (SocialBusiness.IsLet(MasterSocial, AppSocial.PlayerID, new List<string> { "Enemy" }))
            {
                note.Call(SocialCommand.MasterBackR, false, IsMaster, TipManager.GetMessage(SocialReturn.Enemy));
                return;
            }
            if (SocialBusiness.IsLet(MasterSocial, AppSocial.PlayerID, new List<string> { "Master" }))
            {
                note.Call(SocialCommand.MasterBackR, false, IsMaster, TipManager.GetMessage(SocialReturn.Master));
                return;
            }
            if (SocialBusiness.IsLet(MasterSocial, AppSocial.PlayerID, new List<string> { "Apprentice" }))
            {
                note.Call(SocialCommand.MasterBackR, false, IsMaster, TipManager.GetMessage(SocialReturn.Apprentice));
                return;
            }



            if (SocialBusiness.IsLet(AppSocial, MasterSocial.PlayerID, new List<string> { "Enemy" }))
            {
                note.Call(SocialCommand.MasterBackR, false, IsMaster, TipManager.GetMessage(SocialReturn.Enemy));
                return;
            }
            if (SocialBusiness.IsLet(AppSocial, MasterSocial.PlayerID, new List<string> { "Master" }))
            {
                note.Call(SocialCommand.MasterBackR, false, IsMaster, TipManager.GetMessage(SocialReturn.Master));
                return;
            }
            if (SocialBusiness.IsLet(AppSocial, MasterSocial.PlayerID, new List<string> { "Apprentice" }))
            {
                note.Call(SocialCommand.MasterBackR, false, IsMaster, TipManager.GetMessage(SocialReturn.Apprentice));
                return;
            }

            //徒弟师傅数量
            IList AppMaster = AppMentor.GetValue<IList>("Master");

            if (AppMaster != null)
            {
                if (AppMaster.Count >= 1)
                {
                    note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.MasterCount));
                    return;
                }
            }

            //师傅徒弟数量
            IList MasterApp = MasterMentor.GetValue<IList>("Apprentice");
            if (MasterApp != null)
            {
                if (!CheckApp(MasterApp))
                {
                    note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.ApprenticeCount));
                    return;
                }
                //if (MasterApp.Count >= 4)
                //{
                //    note.Call(SocialCommand.MasterApplyR, false, IsMaster, TipManager.GetMessage(SocialReturn.MasterCount));
                //    return;
                //}
            }

            DateTime dt = DateTime.UtcNow;
            Variant v = new Variant();
            v.Add("PlayerID", MasterSocial.PlayerID);
            v.Add("Created", dt);
            if (AppMaster == null)
            {
                AppMentor["Master"] = new List<Variant> { v };
            }
            else
            {
                AppMaster.Add(v);
            }

            Variant app = new Variant();
            app.Add("PlayerID", AppSocial.PlayerID);
            app.Add("Created", dt);

            IList MasterApprentice = MasterMentor.GetValue<IList>("Apprentice");
            if (MasterApprentice == null)
            {
                MasterMentor["Apprentice"] = new List<Variant>() { app };
            }
            else
            {
                MasterApprentice.Add(app);
            }

            MasterSocial.Save();
            AppSocial.Save();

            if (IsMaster)
            {
                string MasterStr = string.Format(TipManager.GetMessage(SocialReturn.MasterBack1), OnLineBusiness.Name);
                // "你已经收【" + OnLineBusiness.Name + "】为徒！";
                Variant mt = new Variant();
                mt.Add("Message", MasterStr);
                mt.Add("Member", new PlayerSimple(OnLineBusiness, 3));

                note.Call(SocialCommand.MasterBackR, true, true, mt);


                string AppStr = string.Format(TipManager.GetMessage(SocialReturn.MasterBack2), note.Player.Name);
                // "你已经拜【" + note.Player.Name + "】为师！";
                Variant at = new Variant();
                at.Add("Message", AppStr);
                at.Add("Member", new PlayerSimple(note.Player, 3));
                OnLineBusiness.Call(SocialCommand.MasterBackR, true, false, at);
            }
            else
            {
                string MasterStr = string.Format(TipManager.GetMessage(SocialReturn.MasterBack3), note.Player.Name);
                //"你已经收【" + note.Player.Name + "】为徒！";
                Variant mt = new Variant();
                mt.Add("Message", MasterStr);
                mt.Add("Member", new PlayerSimple(note.Player, 3));
                OnLineBusiness.Call(SocialCommand.MasterBackR, true, true, mt);


                string AppStr = string.Format(TipManager.GetMessage(SocialReturn.MasterBack2), OnLineBusiness.Name);
                //"你已经拜【" + OnLineBusiness.Name+ "】为师！";
                Variant at = new Variant();
                at.Add("Message", AppStr);
                at.Add("Member", new PlayerSimple(OnLineBusiness, 3));
                note.Call(SocialCommand.MasterBackR, true, false, at);
            }
        }

        /// <summary>
        /// 解除师傅关系
        /// </summary>
        /// <param name="note"></param>
        public static void DelMaster(UserNote note)
        {
            ///被解除的用户ID
            string playerid = note.GetString(0);
            PlayerEx Social = note.Player.Social;

            Variant Mentor = Social.Value.GetValueOrDefault<Variant>("Mentor");

            IList list = null;
            bool IsMaster = true;
            Variant tmp = null;

            #region 是否是师傅
            if (Mentor.GetValue<IList>("Master") != null)
            {
                IList Master = Mentor.GetValue<IList>("Master");
                foreach (Variant d in Master)
                {
                    if (d.GetStringOrDefault("PlayerID") == playerid)
                    {
                        //解除师傅
                        IsMaster = true;
                        tmp = d;
                        list = Master;
                        break;
                    }
                }
            }
            #endregion

            #region 是否是徒弟
            if (list == null)
            {
                IList App = Mentor.GetValue<IList>("Apprentice");
                if (App != null)
                {
                    foreach (Variant d in App)
                    {
                        if (d.GetStringOrDefault("PlayerID") == playerid)
                        {
                            //解除徒弟
                            IsMaster = false;
                            tmp = d;
                            list = Mentor.GetValue<IList>("Apprentice");
                            break;
                        }
                    }
                }
            }
            #endregion

            if (list == null || tmp == null)
            {
                note.Call(SocialCommand.DelMasterR, false, IsMaster, TipManager.GetMessage(SocialReturn.UserInfoError));
                return;
            }

            bool IsFreeze = true;
            PlayerBusiness OnLineBusiness = PlayersProxy.FindPlayerByID(playerid);
            //被解除者
            PlayerEx AppSocial = OnLineBusiness.Social;
            TimeSpan ts = DateTime.UtcNow - OnLineBusiness.Modified;
            if (ts.TotalDays > 3) IsFreeze = false;

            #region 移除与对应的关系
            Variant OnMentor = AppSocial.Value.GetValueOrDefault<Variant>("Mentor");
            IList OnList = null;
            Variant OnTmp = null;
            IList tmpMaster = OnMentor.GetValue<IList>("Master");
            if (OnMentor != null && tmpMaster != null)
            {
                foreach (Variant m in tmpMaster)
                {
                    if (m.GetStringOrDefault("PlayerID") == note.PlayerID)
                    {
                        OnTmp = m;
                        OnList = tmpMaster;
                        break;
                    }
                }
            }
            if (OnTmp == null)
            {
                IList Apprent = OnMentor.GetValue<IList>("Apprentice");
                if (OnMentor != null && Apprent != null)
                {
                    foreach (Variant n in Apprent)
                    {
                        if (n.GetStringOrDefault("PlayerID") == note.PlayerID)
                        {
                            OnTmp = n;
                            OnList = Apprent;
                            break;
                        }
                    }
                }
            }
            if (OnTmp != null && OnList != null)
            {
                OnList.Remove(OnTmp);
                AppSocial.Save();
            }
            #endregion

            list.Remove(tmp);
            //执行者冻结时间为6小时
            if (IsFreeze)
            {
                (Social.Value["Mentor"] as Variant)["FreezeDate"] = DateTime.UtcNow.AddHours(72);
            }
            Social.Save();



            if (IsMaster)
            {
                Variant v = new Variant();
                v.Add("ID", OnLineBusiness.ID);
                v.Add("Message", string.Format(TipManager.GetMessage(SocialReturn.DelMaster1), OnLineBusiness.Name));
                // "你与【" + OnLineBusiness.Name + "】成功解除师徒关系！";
                //移除师傅
                note.Call(SocialCommand.DelMasterR, true, false, v);

                Variant v1 = new Variant();
                v1.Add("ID", note.Player.ID);
                v1.Add("Message", string.Format(TipManager.GetMessage(SocialReturn.DelMaster2), note.Player.Name));
                //"师徒【" + note.Player.Name + "】终止与你的师徒关系!"
                OnLineBusiness.Call(SocialCommand.DelMasterR, true, true, v1);
            }
            else
            {
                Variant v = new Variant();
                v.Add("ID", OnLineBusiness.ID);
                //移除徒弟
                v.Add("Message", string.Format(TipManager.GetMessage(SocialReturn.DelMaster3), OnLineBusiness.Name));
                //"你与【" + OnLineBusiness.Name + "】成功解除师徒关系！"
                //移除师傅
                note.Call(SocialCommand.DelMasterR, true, true, v);

                Variant v1 = new Variant();
                v1.Add("ID", note.PlayerID);
                v1.Add("Message", string.Format(TipManager.GetMessage(SocialReturn.DelMaster4), note.Player.Name));
                //"师傅【" + note.Player.Name + "】终止与你的师徒关系!";
                OnLineBusiness.Call(SocialCommand.DelMasterR, true, false, v1);
            }
        }


        /// <summary>
        /// 晶币变化
        /// </summary>
        /// <param name="note"></param>
        public static void ConsumeCoin(UserNote note)
        {
            PlayerEx ex = note.Player.Social;
            if (ex == null)
                return;
            Variant m = ex.Value.GetValueOrDefault<Variant>("Mentor");
            if (m == null)
                return;
            IList master = m.GetValue<IList>("Master");
            if (master == null || master.Count <= 0)            
                return;
            
            Variant v = master[0] as Variant;
            if (v == null) return;

            PlayerBusiness pb = PlayersProxy.FindPlayerByID(v.GetStringOrDefault("PlayerID"));
            if (pb == null) return;

            int coin = note.GetInt32(0);
            FinanceType ft = note.GetValue<FinanceType>(1);
            int bond = 0;//得到
            int owe = 0;
            if (coin < 0)
            {
                if (note.Player.Level > 39)
                {
                    //高徒                    
                    bond = Convert.ToInt32(Math.Ceiling(-coin * 0.05));
                    
                }
                else
                {
                    //学徒
                    bond = Convert.ToInt32(Math.Ceiling(-coin * 0.1));
                }
                owe = Convert.ToInt32(Math.Ceiling(-coin * 0.5));
            }
            int reTime = Convert.ToInt32(TipManager.GetMessage(EmailReturn.HameDay));
            if (bond > 0)
            {
                if (pb.AddBond(bond, FinanceType.ConsumeCoin))
                {
                    string msg = string.Format(TipManager.GetMessage(SocialReturn.ConsumeCoin1), note.Player.Name, bond);
                    //"你的徒弟【" + note.Player.Name + "】消费晶币，为感谢师傅的栽培，为师傅送上了【" + bond + "】点劵";
                    //徒弟消费奖励点劵
                    
                    if (EmailAccess.Instance.SendEmail(TipManager.GetMessage(SocialReturn.ConsumeCoin2), TipManager.GetMessage(SocialReturn.FriendsBless8), pb.ID, pb.Name, msg, string.Empty, null, reTime))
                    {
                        if (pb.Online)
                        {
                            pb.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(pb.ID));
                        }
                    }
                }
            }

            if (owe > 0) 
            {
                if (pb.AddOwe(owe, FinanceType.ConsumeCoin))
                {
                    string msg = string.Format(TipManager.GetMessage(SocialReturn.ConsumeCoin3), owe);
                    // "你获得了【" + owe + "】点感恩值";
                    //徒弟消费奖励感恩值
                    if (EmailAccess.Instance.SendEmail(TipManager.GetMessage(SocialReturn.ConsumeCoin4), TipManager.GetMessage(SocialReturn.FriendsBless8), pb.ID, pb.Name, msg, string.Empty, null, reTime)) 
                    {
                        if (pb.Online)
                        {
                            pb.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(pb.ID));
                            pb.Call(ClientCommand.SendActivtyR, new object[] { "T02", msg });
                        }
                    }
                }
            }

            //晶币消费总量
            //long gce = note.Player.GCE;
        }

        /// <summary>
        /// 召唤师傅
        /// </summary>
        /// <param name="note"></param>
        public static void SummonMaster(UserNote note)
        {
            PlayerEx ex = note.Player.Social;
            if (ex == null)
                return;
            Variant m = ex.Value.GetValueOrDefault<Variant>("Mentor");
            if (m == null)
                return;
            IList master = m.GetValue<IList>("Master");
            if (master == null || master.Count <= 0)
            {
                note.Call(SocialCommand.SummonMasterR, false, TipManager.GetMessage(SocialReturn.SummonMaster1), note.Player.Name);
                return;
            }

            string goodsid = "G_d000684";
 
            Variant v = master[0] as Variant;
            if (v != null)
            {
                PlayerBusiness pb = PlayersProxy.FindPlayerByID(v.GetStringOrDefault("PlayerID"));
                if (pb.Online)
                {
                    if (note.Player.RemoveGoods(goodsid, 1,GoodsSource.SummonMaster))
                    {
                        pb.Call(SocialCommand.SummonMasterR, true, note.PlayerID, note.Player.Name);
                        note.Call(SocialCommand.SummonMasterR, true, note.PlayerID, note.Player.Name);                        
                    }
                    else 
                    {
                        //你没有【召唤羽毛】，无法召唤你的师傅
                        note.Call(SocialCommand.SummonMasterR, false,TipManager.GetMessage(SocialReturn.SummonMaster2), note.Player.Name);
                    }
                }
                else
                {
                    note.Call(SocialCommand.SummonMasterR, false, TipManager.GetMessage(SocialReturn.SummonMaster3), note.Player.Name);
                }
            }
            else
            {
                note.Call(SocialCommand.SummonMasterR, false, TipManager.GetMessage(SocialReturn.SummonMaster1), note.Player.Name);
            }
        }

        #region 删除
        /// <summary>
        /// 升级解除师傅关系
        /// </summary>
        /// <param name="note"></param>
        //public static void UpDelMentor(UserNote note)
        //{
        //    PlayerEx Social = note.Player.Social;

        //    Variant Mentor = Social.Value.GetVariantOrDefault("Mentor");

        //    if (Mentor == null)
        //        return;

        //    IList Apprentice = Mentor.GetValue<IList>("Apprentice");
        //    IList Master = Mentor.GetValue<IList>("Master");
        //    if (Master == null && Apprentice == null)
        //        return;

        //    if (Master != null && Master.Count > 0)
        //    {
        //        string playerid = string.Empty;
        //        foreach (Variant m in Master)
        //        {
        //            playerid = m.GetStringOrDefault("PlayerID");
        //        }
        //        if (!string.IsNullOrEmpty(playerid))
        //        {
        //            PlayerBusiness OnLineBusiness = PlayersProxy.FindPlayerByID(playerid);
        //            if (OnLineBusiness.Level - note.Player.Level < 3)
        //            {
        //                PlayerEx AppSocial = OnLineBusiness.Social;
        //                IList App = AppSocial.Value.GetValue<IList>("Apprentice");
        //                Variant tmp = null;
        //                foreach (Variant k in App)
        //                {
        //                    if (k.GetStringOrDefault("PlayerID") == note.PlayerID)
        //                    {
        //                        tmp = k;
        //                        break;
        //                    }
        //                }

        //                if (tmp != null)
        //                {
        //                    //移除徒弟
        //                    App.Remove(tmp);
        //                    AppSocial.Save();
        //                    Mentor["Master"] = null;
        //                    Social.Save();


        //                    Variant v = new Variant();
        //                    v.Add("ID", OnLineBusiness.ID);
        //                    v.Add("Message", string.Format(TipManager.GetMessage(SocialReturn.UpDelMentor1), OnLineBusiness.Name));
        //                    //"因你等级上升与【" + OnLineBusiness.Name + "】成功解除师徒关系！"
        //                    //移除师傅
        //                    note.Call(SocialCommand.DelMasterR, true, false, v);

        //                    Variant v1 = new Variant();
        //                    v1.Add("ID", note.Player.ID);
        //                    v1.Add("Message", string.Format(TipManager.GetMessage(SocialReturn.UpDelMentor2), note.Player.Name));
        //                    //"因徒弟【" + note.Player.Name + "】等级上升,终止与你的师徒关系!"
        //                    OnLineBusiness.Call(SocialCommand.DelMasterR, true, true, v1);
        //                }
        //            }
        //        }
        //    }

        //    if (Apprentice != null && Apprentice.Count > 0)
        //    {
        //        for (int i = 0; i < Apprentice.Count; i++)
        //        {
        //            Variant vt = Apprentice[i] as Variant;
        //            if (!vt.ContainsKey("PlayerID"))
        //            {
        //                //Console.WriteLine("PlayerID不存在");
        //                continue;
        //            }
        //            PlayerBusiness OnLineBusiness = PlayersProxy.FindPlayerByID(vt["PlayerID"].ToString());
        //            if (OnLineBusiness == null)
        //                continue;
        //            PlayerEx MSocial = OnLineBusiness.Social;
        //            if (note.Player.Level - OnLineBusiness.Level < 3)
        //            {
        //                (MSocial.Value["Mentor"] as Variant)["Master"] = null;
        //                MSocial.Save();

        //                Apprentice.Remove(vt);
        //                Social.Save();

        //                Variant v = new Variant();
        //                v.Add("ID", OnLineBusiness.ID);
        //                v.Add("Message", string.Format(TipManager.GetMessage(SocialReturn.UpDelMentor3), OnLineBusiness.Name));
        //                //"因你等级上升与【" + OnLineBusiness.Name + "】成功解除师徒关系！"
        //                //移除师傅
        //                note.Call(SocialCommand.DelMasterR, true, false, v);

        //                Variant v1 = new Variant();
        //                v1.Add("ID", note.Player.ID);
        //                v1.Add("Message", string.Format(TipManager.GetMessage(SocialReturn.UpDelMentor4), note.Player.Name));
        //                //"因师傅【" + note.Player.Name + "】等级上升,终止与你的师徒关系!"
        //                OnLineBusiness.Call(SocialCommand.DelMasterR, true, false, v1);
        //            }
        //        }
        //    }
        //}
        #endregion

        /// <summary>
        /// 达到高徒
        /// </summary>
        /// <param name="note"></param>
        public static void UpMaster(UserNote note)
        {
            if (note.Player.Level != 40)
                return;

            PlayerEx social = note.Player.Social;
            Variant v = social.Value.GetValueOrDefault<Variant>("Mentor");
            if (v == null)
                return;

            IList master = v.GetValue<IList>("Master");
            if (master == null)
                return;


            Variant tmp = master[0] as Variant;
            if (tmp == null)
                return;
            //师傅账号
            string playerid = tmp.GetStringOrDefault("PlayerID");
            PlayerBusiness pb = PlayersProxy.FindPlayerByID(playerid);
            if (pb == null)
                return;
            string[] awards = TipManager.GetMessage(SocialReturn.OutMasterAward).Split('|');

            int reTime = Convert.ToInt32(TipManager.GetMessage(EmailReturn.HameDay));

            Variant gs0 = new Variant();
            gs0.Add("G", awards[0]);
            gs0.Add("A", 1);
            gs0.Add("E", awards[0]);

            List<Variant> gl1 = new List<Variant>();
            gl1.Add(gs0);

            GameConfig gc1 = GameConfigAccess.Instance.FindOneById(awards[0]);
            if (gc1 == null)
                return;

            string con1 = string.Format(TipManager.GetMessage(SocialReturn.UpMaster1), note.Player.Name, gc1.Name);
            if (EmailAccess.Instance.SendEmail(TipManager.GetMessage(SocialReturn.UpMaster2), TipManager.GetMessage(SocialReturn.UpMaster3), pb.ID, pb.Name, con1, string.Empty, gl1, reTime))
            {
                note.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(pb.ID));
                note.Call(ClientCommand.SendActivtyR, new object[] { "T04", TipManager.GetMessage(SocialReturn.UpMaster7) });
            }

            Variant gs2 = new Variant();
            gs2.Add("G", awards[1]);
            gs2.Add("A", 1);
            gs2.Add("E", awards[1]);

            List<Variant> gl2 = new List<Variant>();
            gl2.Add(gs2);

            GameConfig gc2 = GameConfigAccess.Instance.FindOneById(awards[1]);
            if (gc2 == null)
                return;
            string con2 = string.Format(TipManager.GetMessage(SocialReturn.UpMaster4), note.Player.Name, gc2.Name);
            if (EmailAccess.Instance.SendEmail(TipManager.GetMessage(SocialReturn.UpMaster5), TipManager.GetMessage(SocialReturn.UpMaster3), pb.ID, pb.Name, con2, string.Empty, gl2, reTime))
            {
                if (pb.Online)
                {
                    pb.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(pb.ID));
                    pb.Call(ClientCommand.SendActivtyR, new object[] { "T04", string.Format(TipManager.GetMessage(SocialReturn.UpMaster6), note.Player.Name) });
                }
            }
        }
        

        /// <summary>
        /// 是否在冻结期
        /// </summary>
        /// <param name="v"></param>
        /// <returns>true表示在冻结期</returns>
        private static bool FreezeDate(Variant v)
        {
            if (v.ContainsKey("FreezeDate") && v["FreezeDate"] != null)
            {
                DateTime freezeDate = v.GetDateTimeOrDefault("FreezeDate");

                if (freezeDate > DateTime.UtcNow)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查学徒数量
        /// </summary>
        /// <param name="list"></param>
        /// <returns>true允许收徒</returns>
        private static bool CheckApp(IList list)
        {
            if (list == null || list.Count == 0)
                return true;
            int num = 0;//学徒数量
            foreach (Variant v in list)
            {
                string playerid = v.GetStringOrDefault("PlayerID");
                if (string.IsNullOrEmpty(playerid))
                    continue;
                PlayerBusiness pb = PlayersProxy.FindPlayerByID(playerid);
                if (pb != null && pb.Level < 40)
                    num++;
                if (num >= 4) return false;
            }
            return true;
        }

        /// <summary>
        /// 回复招呼
        /// </summary>
        /// <param name="note"></param>
        internal static void ReplySummon(UserNote note)
        {
            //徒弟ID
            string pid = note.GetString(0);
            PlayerBusiness pb = PlayersProxy.FindPlayerByID(pid);
            if (!pb.Online)
            {
                return;
            }
            //徒弟所在场景
            SceneBusiness pScene = pb.Scene;
            if (pScene == null)
            {
                return;
            }
            if ((pScene.SceneType != SceneType.City) && (pScene.SceneType != SceneType.Outdoor))
            {
                return;
            }

            PlayerBusiness player = note.Player;
            //师傅所在场景
            SceneBusiness mScene = player.Scene;
            if ((mScene.SceneType != SceneType.City) && (mScene.SceneType != SceneType.Outdoor))
            {
                return;
            }

            if (player.Team != null && player.TeamJob != TeamJob.Away)
            {
                return;
            }

            if (pScene == mScene)
            {
                //同一场景招唤
                player.X = pb.X;
                player.Y = pb.Y;
                player.CallAll(ClientCommand.FastToR, player.ID, mScene.ID, player.X, player.Y);
            }
            else
            {
                //不同场景招唤
                if (mScene.ExitScene(player))
                {
                    UserNote note2 = new UserNote(player, ClientCommand.IntoSceneSuccess, new object[] { pb.X, pb.Y });
                    pScene.Execute(note2);
                }
            }
        }
    }
}
