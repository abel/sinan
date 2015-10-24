using System;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Util;
using Sinan.Log;

#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FamilyModule.Business
{
    class FamilyBusiness
    {
        static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 入族申请记录
        /// </summary>
        static Dictionary<string, Variant> applyList = new Dictionary<string, Variant>();
        /// <summary>
        /// 邀请入族
        /// </summary>
        static Dictionary<string, Variant> inviteList = new Dictionary<string, Variant>();

        // new string[] { "族长", "副族长", "族员" };
        static string[] StrsRole = TipManager.GetMessage(FamilyReturn.FamilyRole).Split('|');
        /// <summary>
        /// 得到所有家族列表
        /// </summary>
        /// <param name="note"></param>
        public static void FamilyList(UserNote note)
        {
            int pageSize = note.GetInt32(0);
            int pageIndex = note.GetInt32(1);
            string familName = note.GetString(2);
            int totalCount = 0;
            int currCount = 0;
            List<Family> list = FamilyAccess.Instance.FamilyPage(pageSize, pageIndex, familName, out totalCount, out currCount);
            List<Variant> tmp = new List<Variant>();
            int a = 0;
            foreach (Family model in list)
            {
                a++;
                Variant v = new Variant();
                v.Add("ID", model.ID);
                v.Add("Name", model.Name);
                v.Add("Level", model.Value.GetIntOrDefault("Level"));

                IList persons = model.Value.GetValue<IList>("Persons");
                string bossName = string.Empty;
                foreach (Variant dy in persons)
                {
                    if (dy.GetIntOrDefault("RoleID") == 0)
                    {
                        //PlayerBusiness pb = PlayersProxy.FindPlayerByID(dy.GetStringOrDefault("PlayerID"));
                        ////族长名称
                        //if (pb != null)
                        //{
                        //    ;// pb.Name;
                        //}
                        bossName = PlayerAccess.Instance.GetPlayerName(Convert.ToInt32(dy.GetStringOrDefault("PlayerID"), 16));
                        break;
                    }
                }

                Variant number = new Variant();
                number.Add("V", persons.Count);
                Variant max = FamilyBase.FamilyCount(model.Value.GetIntOrDefault("Level"));
                number.Add("M", max == number ? 20 : max.GetIntOrDefault("Persons"));
                v.Add("Number", number);
                v.Add("BossName", bossName);
                v.Add("Notice", model.Value.GetStringOrDefault("Notice"));
                v.Add("Sort", (currCount * pageSize) + a);
                tmp.Add(v);
            }
            note.Call(FamilyCommand.FamilyListR, totalCount, tmp);
        }

        /// <summary>
        /// 得到家族成员列表信息
        /// </summary>
        /// <param name="note"></param>
        public static void FamilyMembers(UserNote note)
        {
            string soleid = note.PlayerID + "FamilyMembers";
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                //得到玩家家族信息
                PlayerEx fx = note.Player.Family;
                if (fx == null)
                {
                    note.Call(FamilyCommand.FamilyMembersR, new Variant());
                    return;
                }

                if (fx.Value.GetStringOrDefault("FamilyID") == string.Empty || fx.Value["FamilyID"] == null)
                {
                    note.Call(FamilyCommand.FamilyMembersR, new Variant());
                    return;
                }
                //得到家族信息
                Family model = FamilyAccess.Instance.FindOneById(fx.Value.GetStringOrDefault("FamilyID"));
                if (model == null)
                {
                    note.Call(FamilyCommand.FamilyMembersR, new Variant());
                    return;
                }

                IList list = model.Value.GetValue<IList>("Persons");

                Variant t = new Variant();
                t.Add("FamilyID", model.ID);
                t.Add("FamilyName", model.Name);
                int level = model.Value.GetIntOrDefault("Level");
                //fv.Add("FamilySort", FamilyAccess.Instance.FamilySort(family.ID));
                //得到家族当前排名
                t.Add("FamilySort", FamilyAccess.Instance.FamilySort(level, model.Modified, model.Created));

                List<Variant> persons = new List<Variant>();
                string boss = "";
                int myRoleID = 2;

                var item = PlayerAccess.Instance.GetPlayersByFamliy(model.Name);

                Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
                foreach (Variant v in item)
                {
                    string playerid = v.GetIntOrDefault("_id").ToHexString();
                    Variant tmp = new Variant();
                    tmp["PlayerID"] = playerid;
                    tmp["Name"] = v.GetStringOrDefault("Name");
                    tmp["Level"] = v.GetStringOrDefault("Level");
                    tmp["RoleID"] = v.GetStringOrDefault("RoleID");
                    tmp["Online"] = v.GetBooleanOrDefault("Online");

                    if (!dic.ContainsKey(playerid))
                    {
                        dic.Add(playerid, tmp);
                    }
                }

                foreach (Variant d in list)
                {
                    string playerid = d.GetStringOrDefault("PlayerID");
                    Variant v = null;
                    if (dic.TryGetValue(playerid, out v))
                    { 
                        if (d.GetIntOrDefault("RoleID") == 0)
                        {
                            boss = v.GetStringOrDefault("Name");
                        }
                        if (playerid == note.PlayerID)
                        {
                            myRoleID = d.GetIntOrDefault("RoleID");
                        }
                        v.Add("AddDate", d.GetValue<object>("AddDate"));
                        v.Add("Devote", d.GetIntOrDefault("Devote"));
                        v.Add("Header", d.GetIntOrDefault("RoleID"));//家族职业
                        v.Add("Sort", 0);
                        persons.Add(v);
                    }
                }

                //角色在家族中排序
                //FamilyAccess.Instance.MembersSort(persons);
                t.Add("FamilyLevel", level);
                Variant number = new Variant();
                number.Add("V", list.Count);      
                Variant max = FamilyBase.FamilyCount(level);                
                number.Add("M", max == null ? 20 : max.GetIntOrDefault("Persons"));
                t.Add("Number", number);
                t.Add("Boss", string.IsNullOrEmpty(boss) ? "" : boss);
                t.Add("Persons", persons);
                t.Add("Notice", model.Value.GetStringOrDefault("Notice"));
                t.Add("MyRoleID", myRoleID);
                t.Add("DayDev", FamilyAccess.Instance.FamilyDev(model));
                note.Call(FamilyCommand.FamilyMembersR, t);
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }
     
        /// <summary>
        /// 家族的创建
        /// </summary>
        /// <param name="note"></param>
        public static void FamilyCreate(UserNote note)
        {
            //家族名称
            string name = note.GetString(0).Trim();

            string npcid = note.GetString(1);//NPC
            if (!note.Player.EffectActive(npcid, ""))
                return;

            string checkmsg = NameManager.Instance.CheckName(name);
            if (!string.IsNullOrEmpty(checkmsg))
            {
                note.Call(FamilyCommand.FamilyCreateR, false, checkmsg);
                return;
            }

            //得到玩家家族信息
            PlayerEx family = note.Player.Family;
            if (family.Value.GetStringOrDefault("FamilyID") != string.Empty)
            {
                note.Call(FamilyCommand.FamilyCreateR, false, TipManager.GetMessage(FamilyReturn.FamilyExist));
                return;
            }
            if (family.Value.GetStringOrDefault("FreezeDate") != string.Empty)
            {
                DateTime FreezeDate = family.Value.GetDateTimeOrDefault("FreezeDate");
                if (FreezeDate.ToLocalTime() > DateTime.Now)
                {
                    note.Call(FamilyCommand.FamilyCreateR, false, string.Format(TipManager.GetMessage(FamilyReturn.FreezeDate), FreezeDate.ToLocalTime().ToString()));
                    return;
                }
            }
            if (name.Length > 7)
            {
                note.Call(FamilyCommand.FamilyCreateR, false, TipManager.GetMessage(FamilyReturn.NameOutLength));
                return;
            }

            if (FamilyAccess.Instance.FamilyIsExist(name))
            {
                note.Call(FamilyCommand.FamilyCreateR, false, TipManager.GetMessage(FamilyReturn.FamilyNameExist));
                return;
            }

            if (note.Player.Level < 10)
            {
                //玩家等级不足
                note.Call(FamilyCommand.FamilyCreateR, false, TipManager.GetMessage(FamilyReturn.NoLevel));
                return;
            }

            if (family.Value.GetStringOrDefault("FamilyID") != string.Empty)
            {
                note.Call(FamilyCommand.FamilyCreateR, false, TipManager.GetMessage(FamilyReturn.ExistFamily));
                return;
            }


            //创建家族需要2000000石币
            if (note.Player.Score < 400000 || (!note.Player.AddScore(-400000, FinanceType.FamilyCreate)))
            {
                note.Call(FamilyCommand.FamilyCreateR, false, TipManager.GetMessage(FamilyReturn.NoScore));
                return;
            }

            string[] msg = TipManager.GetMessage(FamilyReturn.CreateFamilyEmail).Split('|');
            if (msg.Length < 3)
                return;


            WordAccess.Instance.SetUsed(name);

            Family model = Family.Create(FamilyBase.FamilyValue(note));
            model.ID = ObjectId.GenerateNewId().ToString();
            model.Name = name;
            model.Created = DateTime.UtcNow;
            model.Modified = DateTime.UtcNow;
            //model.Save();

            FamilyAccess.Instance.Insert(model);


            //更新用户家族信息
            family.Value["FamilyID"] = model.ID;
            family.Value["FamilyName"] = name;

            //是否在冻结期
            family.Value["FreezeDate"] = DateTime.UtcNow;
            family.Value["FamilyRoleID"] = 0;
            family.Value["Devote"] = 0;//当前对家族的贡献值
            family.Save();


            Email email = new Email();
            email.ID = ObjectId.GenerateNewId().ToString();
            email.Name = string.Format(msg[1], model.Name);//msg[1].Replace("FamilyName", model.Name);
            email.Status = 0;
            email.Ver = 1;
            email.MainType = EmailCommand.System;
            email.Created = DateTime.UtcNow;
            Variant v = new Variant();
            v.Add("mailMess", string.Format(msg[2], model.Name));
            v.Add("reTime", 30);
            email.Value = EmailAccess.Instance.CreateEmailValue(EmailCommand.System, msg[0], note.PlayerID, note.Player.Name, v);
            email.Save();

            Variant self = new Variant();
            self.Add("Family", note.Player.Family);

            note.Call(FamilyCommand.FamilyCreateR, true, self);
            note.Player.SetFamilyName(name, StrsRole[0]);
            note.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(note.PlayerID));

            UserNote note1 = new UserNote(note.Player, FamilyCommand.AddFamily, new object[] { 0 });
            Notifier.Instance.Publish(note1);

            Variant os = new Variant();
            os["ID"] = model.ID;
            os["Name"] = model.Name;
            os["TargetID"] = note.PlayerID;
            os["TargetName"] = note.Player.Name;
            os["Status"] = 0;
            note.Player.AddLogVariant(Actiontype.FamilyLog, null, null, os);
        }

        /// <summary>
        /// 申请入族
        /// </summary>
        /// <param name="note"></param>
        public static void FamilyApply(UserNote note)
        {
            PlayerEx familyEx = note.Player.Family;
            if (familyEx == null)
            {
                note.Call(FamilyCommand.FamilyApplyR, false, TipManager.GetMessage(FamilyReturn.FamilyError));
                return;
            }
            if (familyEx == null || familyEx.Value.GetStringOrDefault("FamilyID") != string.Empty)
            {
                note.Call(FamilyCommand.FamilyApplyR, false, TipManager.GetMessage(FamilyReturn.FamilyExist));
                return;
            }


            if (familyEx.Value.GetStringOrDefault("FreezeDate") != string.Empty)
            {
                DateTime FreezeDate = familyEx.Value.GetDateTimeOrDefault("FreezeDate");
                //if ((DateTime.TryParse(familyEx.Value.GetStringOrDefault("FreezeDate"), out FreezeDate)))
                //{
                if (FreezeDate.ToLocalTime() > DateTime.Now)
                {
                    note.Call(FamilyCommand.FamilyCreateR, false, string.Format(TipManager.GetMessage(FamilyReturn.FreezeDate), FreezeDate.ToLocalTime().ToString()));
                    return;
                }
                //}
            }

            //申请的家族
            string familyid = note.GetString(0);

            Family family = FamilyAccess.Instance.FindOneById(familyid);
            if (family == null)
            {
                note.Call(FamilyCommand.FamilyApplyR, false, TipManager.GetMessage(FamilyReturn.NoFamily));
                return;
            }

            int ApplyCount = 0;//当前申请入族次数
            foreach (Variant d in applyList.Values)
            {
                if (d.GetStringOrDefault("ID") == note.PlayerID) ApplyCount++;
            }

            if (ApplyCount >= 10)
            {
                note.Call(FamilyCommand.FamilyApplyR, false, TipManager.GetMessage(FamilyReturn.InApplyLimit));
                return;
            }

            FamilyExpired();

            IList Persons = family.Value.GetValue<IList>("Persons");
            Variant dy = FamilyBase.FamilyCount(family.Value.GetIntOrDefault("Level"));
            //家族人数达到上限，不能再申请
            if (Persons.Count >= dy.GetIntOrDefault("Persons"))
            {
                note.Call(FamilyCommand.FamilyApplyR, false, TipManager.GetMessage(FamilyReturn.PersonsLimit));
                return;
            }

            string appid = family.ID + "," + note.PlayerID;
            if (applyList.ContainsKey(appid))
            {
                Variant v = applyList[appid];
                if (v.GetDateTimeOrDefault("DateTime") > DateTime.UtcNow)
                {
                    note.Call(FamilyCommand.FamilyApplyR, false, TipManager.GetMessage(FamilyReturn.IsApply));
                    return;
                }
                applyList.Remove(appid);

            }

            Variant t = new Variant();
            t.Add("DateTime", DateTime.UtcNow.AddHours(1));
            t.Add("SoleID", appid);
            t.Add("ID", note.PlayerID);
            t.Add("Name", note.Player.Name);
            t.Add("FamilyID", family.ID);
            t.Add("FamilyName", family.Name);
            if (!applyList.ContainsKey(appid))
                applyList.Add(appid, t);

            Variant tmp = new Variant();
            tmp.Add("ID", t.GetStringOrDefault("SoleID"));
            tmp.Add("Message", string.Format(TipManager.GetMessage(FamilyReturn.ApplyFamilySuccess), note.Player.Name, family.Name));

            foreach (Variant d in Persons)
            {
                if (d.GetIntOrDefault("RoleID") == 2)
                    continue;
                PlayerBusiness pb = PlayersProxy.FindPlayerByID(d.GetStringOrDefault("PlayerID"));

                if (pb != null && pb.Online)
                {
                    pb.Call(FamilyCommand.FamilyApplyR, true, tmp);
                }
            }
        }

        /// <summary>
        /// 申请入族回复
        /// </summary>
        /// <param name="note"></param>
        public static void FamilyApplyBack(UserNote note)
        {
            string str = note.GetString(1);
            PlayerEx family = note.Player.Family;
            if (family == null)
            {
                applyList.Remove(str);
                note.Call(FamilyCommand.FamilyApplyBackR, false, TipManager.GetMessage(FamilyReturn.FamilyError));
                return;
            }
            bool isSame = false;
            Boolean.TryParse(note.GetString(0), out isSame);
            string[] strs = str.Split(',');
            PlayerBusiness pb = PlayersProxy.FindPlayerByID(strs[1]);

            if (!isSame)
            {
                applyList.Remove(str);
                Variant t = new Variant();
                t["msg"] = "<f  color='ff3300'>" + TipManager.GetMessage(FamilyReturn.ApplyRefuse) + "</f>";
                t["level"] = 0;
                t["showType"] = 0;
                t["date"] = DateTime.UtcNow;
                pb.Call(ClientCommand.SendMsgToAllPlayerR, string.Empty, t);
                return;
            }

            applyList.Remove(str);
            FamilyExpired();

            if (family.Value.GetStringOrDefault("FamilyID") == string.Empty)
            {
                note.Call(FamilyCommand.FamilyApplyBackR, false, TipManager.GetMessage(FamilyReturn.ApplyBackError));
                return;
            }
            if (family.Value.GetStringOrDefault("FamilyID") != strs[0])
            {
                note.Call(FamilyCommand.FamilyApplyBackR, false, TipManager.GetMessage(FamilyReturn.ApplyBackError));
                return;
            }

            if (family.Value.GetIntOrDefault("FamilyRoleID") == 2)
            {
                note.Call(FamilyCommand.FamilyApplyBackR, false, TipManager.GetMessage(FamilyReturn.NoPower));
                return;
            }

            PlayerEx player = pb.Family;

            if (player == null)
            {
                note.Call(FamilyCommand.FamilyApplyBackR, false, TipManager.GetMessage(FamilyReturn.ApplyBackError));
                return;
            }
            if (player.Value.GetStringOrDefault("FamilyID") != string.Empty)
            {
                note.Call(FamilyCommand.FamilyInviteBackR, false, TipManager.GetMessage(FamilyReturn.FamilyExist));
                return;
            }

            Family model = FamilyAccess.Instance.FindOneById(family.Value.GetStringOrDefault("FamilyID"));
            if (model == null)
            {
                note.Call(FamilyCommand.FamilyApplyBackR, false, TipManager.GetMessage(FamilyReturn.ApplyBackError));
                return;
            }
            IList Persons = model.Value.GetValue<IList>("Persons");

            Variant d = FamilyBase.FamilyCount(model.Value.GetIntOrDefault("Level"));
            if (Persons.Count >= d.GetIntOrDefault("Persons"))
            {
                note.Call(FamilyCommand.FamilyApplyBackR, false, TipManager.GetMessage(FamilyReturn.PersonsLimit));
                return;
            }
            if (FamilyBase.IsExist(Persons, strs[1]))
            {
                note.Call(FamilyCommand.FamilyApplyBackR, false, TipManager.GetMessage(FamilyReturn.FamilyExist));
                return;
            }

            Persons.Add(FamilyBase.PersonInfo(strs[1], 2));

            //更新用户家族信息
            player.Value["FamilyID"] = model.ID;
            player.Value["FamilyName"] = model.Name;


            //是否在冻结期
            player.Value["FreezeDate"] = DateTime.UtcNow;
            player.Value["FamilyRoleID"] = 2;
            player.Value["Devote"] = 0;
            model.Save();
            player.Save();

            Variant v = new Variant();
            v.Add("ID", pb.ID);
            v.Add("Family", pb.Family);
            v.Add("Message", string.Format(TipManager.GetMessage(FamilyReturn.ApplyBackSuccess), pb.Name, model.Name));

            pb.SetFamilyName(model.Name, StrsRole[2]);
            note.Player.FamilyCall(FamilyCommand.FamilyApplyBackR,model.Name, true, v);
            //for (int i = 0; i < Persons.Count; i++)
            //{
            //    Variant p = Persons[i] as Variant;
            //    PlayerBusiness PlayerOnLine = PlayersProxy.FindPlayerByID(p.GetStringOrDefault("PlayerID"));
            //    if (PlayerOnLine != null && PlayerOnLine.Online)
            //    {
            //        PlayerOnLine.Call(FamilyCommand.FamilyApplyBackR, true, v);
            //    }
            //}

            UserNote note1 = new UserNote(pb, FamilyCommand.AddFamily, new object[] { 1 });
            Notifier.Instance.Publish(note1);


            Variant os = new Variant();
            os["ID"] = model.ID;
            os["Name"] = model.Name;
            os["TargetID"] = note.PlayerID;
            os["TargetName"] = note.Player.Name;
            os["Status"] = 1;
            os["SourceID"] = pb.ID;
            os["SourceName"] = pb.Name;
            note.Player.AddLogVariant(Actiontype.FamilyLog, null, null, os);
        }

        /// <summary>
        /// 邀请入家族
        /// </summary>
        /// <param name="note"></param>
        public static void FamilyInvite(UserNote note)
        {
            //被邀请的玩家
            string playerid = note.GetString(0);
            PlayerEx family = note.Player.Family;

            if (family.Value.GetStringOrDefault("FamilyID") == string.Empty || family.Value.GetStringOrDefault("FamilyID") == null)
            {
                note.Call(FamilyCommand.FamilyInviteR, false, TipManager.GetMessage(FamilyReturn.NoAddFamily));
                return;
            }
            //副簇长，簇长
            if (family.Value.GetIntOrDefault("FamilyRolueID") > 1 || family.Value.GetIntOrDefault("FamilyRolueID") < 0)
            {
                note.Call(FamilyCommand.FamilyInviteR, false, TipManager.GetMessage(FamilyReturn.NoPower));
                return;
            }
            Family model = FamilyAccess.Instance.FindOneById(family.Value.GetStringOrDefault("FamilyID"));
            if (model == null)
            {
                note.Call(FamilyCommand.FamilyInviteR, false, TipManager.GetMessage(FamilyReturn.FamilyError));
                return;
            }
            IList Persons = model.Value.GetValue<IList>("Persons");
            //表示已经存在
            if (FamilyBase.IsExist(Persons, playerid))
            {
                note.Call(FamilyCommand.FamilyInviteR, false, TipManager.GetMessage(FamilyReturn.FamilyExist));
                return;
            }
            Variant dy = FamilyBase.FamilyCount(model.Value.GetIntOrDefault("Level"));
            //家族人数达到上限，不能再邀请
            if (Persons.Count >= dy.GetIntOrDefault("Persons"))
            {
                note.Call(FamilyCommand.FamilyInviteR, false, TipManager.GetMessage(FamilyReturn.PersonsLimit));
                return;
            }

            PlayerBusiness OnLineBusiness = PlayersProxy.FindPlayerByID(playerid);
            if (OnLineBusiness == null)
            {
                //邀请入族
                //note.Call(FamilyCommand.FamilyInviteR, false, TipManager.GetMessage(FamilyReturn.FamilyExist));
                return;
            }

            PlayerEx player = OnLineBusiness.Family;
            if (player == null)
            {
                note.Call(FamilyCommand.FamilyInviteR, false, TipManager.GetMessage(FamilyReturn.FamilyError));
                return;
            }
            if (player.Value.GetStringOrDefault("FamilyID") != string.Empty)
            {
                note.Call(FamilyCommand.FamilyInviteR, false, TipManager.GetMessage(FamilyReturn.FamilyExist));
                return;
            }

            if (family.Value.GetStringOrDefault("FreezeDate") != string.Empty)
            {
                DateTime FreezeDate;
                //被邀请的玩家还处于冻结期
                if (DateTime.TryParse(player.Value.GetStringOrDefault("FreezeDate"), out FreezeDate))
                {
                    note.Call(FamilyCommand.FamilyInviteR, false, string.Format(TipManager.GetMessage(FamilyReturn.FreezeDate), FreezeDate.ToLocalTime().ToString()));
                    return;
                }
            }

            string invid = family.Value.GetStringOrDefault("FamilyID") + "," + playerid;
            if (inviteList.ContainsKey(invid))
            {
                Variant v = inviteList[invid] as Variant;

                if (DateTime.Parse(v.GetStringOrDefault("DateTime")) > DateTime.UtcNow)
                {
                    //表示已经邀请还没有过期
                    note.Call(FamilyCommand.FamilyInviteR, false, TipManager.GetMessage(FamilyReturn.IsInvite));
                    return;
                }
                inviteList.Remove(invid);
            }

            Variant t = new Variant();
            t.Add("SoleID", invid);
            t.Add("ID", note.PlayerID);
            t.Add("Name", note.Player.Name);
            t.Add("FamliyID", family.PlayerID);
            t.Add("FamliyName", family.Name);
            t.Add("DateTime", DateTime.UtcNow.AddHours(1));

            inviteList.Add(invid, t);
            Variant tmp = new Variant();
            tmp.Add("ID", t.GetStringOrDefault("SoleID"));
            tmp.Add("Message", string.Format(TipManager.GetMessage(FamilyReturn.InviteFamilySuccess), note.Player.Name, family.Name));

            // "【" + t["Name"].ToString() + "】邀请你加入家族【" + t["FamliyName"].ToString() + "】");
            if (OnLineBusiness.Online)
            {
                //邀请入族
                OnLineBusiness.Call(FamilyCommand.FamilyInviteR, true, tmp);
            }
        }

        /// <summary>
        /// 邀请回复
        /// </summary>
        /// <param name="note"></param>
        public static void FamilyInviteBack(UserNote note)
        {
            PlayerEx player = note.Player.Family;
            if (player == null)
            {
                note.Call(FamilyCommand.FamilyInviteBackR, false, TipManager.GetMessage(FamilyReturn.FamilyError));
                return;
            }
            bool isSame = false;
            Boolean.TryParse(note.GetString(0), out isSame);

            //申请人
            string str = note.GetString(1);
            string[] strs = str.Split(',');

            if (player.Value.GetStringOrDefault("FamilyID") != string.Empty)
            {
                note.Call(FamilyCommand.FamilyInviteBackR, false, TipManager.GetMessage(FamilyReturn.FamilyExist));
                return;
            }

            if (note.PlayerID != strs[1])
            {
                note.Call(FamilyCommand.FamilyInviteBackR, false, TipManager.GetMessage(FamilyReturn.InviteError));
                return;
            }

            if (player.Value.GetStringOrDefault("FreezeDate") != string.Empty)
            {
                DateTime FreezeDate;
                if (DateTime.TryParse(player.Value.GetStringOrDefault("FreezeDate"), out FreezeDate))
                {
                    //被邀请的玩家还处于冻结期
                    if (FreezeDate.ToLocalTime() > DateTime.Now)
                    {
                        note.Call(FamilyCommand.FamilyInviteBackR, false, string.Format(TipManager.GetMessage(FamilyReturn.FreezeDate), FreezeDate.ToLocalTime().ToString()));
                        return;
                    }
                }
                //else
                //{
                //    note.Call(FamilyCommand.FamilyInviteBackR, false, TipAccess.GetMessage(FamilyReturn.FreezeDate));
                //    return;
                //}
            }

            if (!inviteList.ContainsKey(str))
            {
                note.Call(FamilyCommand.FamilyInviteBackR, false, TipManager.GetMessage(FamilyReturn.InviteError));
                return;
            }

            inviteList.Remove(str);
            if (!isSame)
            {
                note.Call(FamilyCommand.FamilyInviteBackR, false, TipManager.GetMessage(FamilyReturn.FamilyError));
                return;
            }


            //家族信息
            Family family = FamilyAccess.Instance.FindOneById(strs[0]);
            if (family == null)
            {
                note.Call(FamilyCommand.FamilyInviteBackR, false, TipManager.GetMessage(FamilyReturn.FamilyError));
                return;
            }

            Variant dy = FamilyBase.FamilyCount(family.Value.GetIntOrDefault("Level"));

            IList Persons = family.Value.GetValue<IList>("Persons");
            if (Persons.Count >= dy.GetIntOrDefault("Persons"))
            {
                //家族人数不能大于最大数量
                note.Call(FamilyCommand.FamilyInviteBackR, false, TipManager.GetMessage(FamilyReturn.PersonsLimit));
                return;
            }

            if (FamilyBase.IsExist(Persons, note.PlayerID))
            {
                note.Call(FamilyCommand.FamilyInviteBackR, false, TipManager.GetMessage(FamilyReturn.FamilyExist));
                return;
            }
            Persons.Add(FamilyBase.PersonInfo(note.PlayerID, 2));
            //更新用户家族信息
            player.Value["FamilyID"] = family.ID;
            player.Value["FamilyName"] = family.Name;
            //是否在冻结期
            player.Value["FreezeDate"] = DateTime.UtcNow;
            player.Value["FamilyRoleID"] = 2;
            player.Value["Devote"] = 0;
            player.Save();

            family.Save();

            Variant v = new Variant();
            v.Add("ID", note.PlayerID);
            v.Add("Family", note.Player.Family);
            v.Add("Message", string.Format(TipManager.GetMessage(FamilyReturn.InviteBackSuccess), note.Player.Name, family.Name));
            // "<f  color='ff3300'>【" + note.Player.Name + "】成功加入家族【" + family.Name + "】</f>");
            note.Player.FamilyCall(FamilyCommand.FamilyInviteBackR,family.Name, true, v);

            //for (int i = 0; i < Persons.Count; i++)
            //{
            //    Variant p = Persons[i] as Variant;
            //    PlayerBusiness PlayerOnLine = PlayersProxy.FindPlayerByID(p.GetStringOrDefault("PlayerID"));
            //    if (PlayerOnLine != null && PlayerOnLine.Online)
            //    {
            //        note.Call(FamilyCommand.FamilyInviteBackR, true, v);
            //    }
            //}
            
            note.Player.SetFamilyName(family.Name, StrsRole[2]);

            //
            UserNote note1 = new UserNote(note.Player, FamilyCommand.AddFamily, new object[] { 2 });
            Notifier.Instance.Publish(note1);



            Variant os = new Variant();
            os["ID"] = family.ID;
            os["Name"] = family.Name;
            os["TargetID"] = note.PlayerID;
            os["TargetName"] = note.Player.Name;
            os["Status"] = 2;
            //os["SourceID"] = pb.ID;
            //os["SourceName"] = pb.Name;
            note.Player.AddLogVariant(Actiontype.FamilyLog, null, null, os);
        }

        /// <summary>
        /// 退出家族
        /// </summary>
        /// <param name="note"></param>
        public static void ExitFamily(UserNote note)
        {
            PlayerEx familyEx = note.Player.Family;
            if (familyEx.Value.GetStringOrDefault("FamilyID") == string.Empty)
            {
                note.Call(FamilyCommand.ExitFamilyR, false, TipManager.GetMessage(FamilyReturn.NoFamily));
                return;
            }

            if (familyEx.Value.GetIntOrDefault("FamilyRoleID") == 0)
            {
                note.Call(FamilyCommand.ExitFamilyR, false, TipManager.GetMessage(FamilyReturn.NoExitBoss));
                return;
            }

            Family model = FamilyAccess.Instance.FindOneById(familyEx.Value.GetStringOrDefault("FamilyID"));
            IList Persons = model.Value.GetValue<IList>("Persons");

            bool IsExist = false;
            for (int i = 0; i < Persons.Count; i++)
            {
                Variant p = Persons[i] as Variant;
                if (p.GetStringOrDefault("PlayerID") == note.PlayerID)
                {
                    Persons.Remove(p);
                    IsExist = true;
                    break;
                }
            }
            if (IsExist)
            {
                familyEx.Value["FamilyID"] = string.Empty;
                familyEx.Value["FamilyName"] = string.Empty;
                //是否在冻结期
                familyEx.Value["FreezeDate"] = DateTime.UtcNow.AddHours(12);
                //familyEx.Value["FreezeDate"] = DateTime.UtcNow.AddMinutes(10);
                familyEx.Value["FamilyRoleID"] = 0;
                familyEx.Save();
            }
            model.Save();

            Variant self = new Variant();
            self.Add("ID", note.PlayerID);
            self.Add("Message", string.Format(TipManager.GetMessage(FamilyReturn.ExitFamilySuccess), note.Player.Name, model.Name));

   
            note.Player.FamilyCall(FamilyCommand.ExitFamilyR, model.Name, true, self);

            note.Player.SetFamilyName(string.Empty, string.Empty);

            Pet pet = note.Player.Pet;
            if (pet != null)
            {
                PetAccess.PetReset(pet, null, false,note.Player.Mounts);
            }
            note.Call(FamilyCommand.ExitFamilyR, true, self);

            Variant os = new Variant();
            os["ID"] = model.ID;
            os["Name"] = model.Name;
            os["TargetID"] = note.PlayerID;
            os["TargetName"] = note.Player.Name;
            os["Status"] = 3;
            //os["SourceID"] = pb.ID;
            //os["SourceName"] = pb.Name;
            note.Player.AddLogVariant(Actiontype.FamilyLog, null, null, os);
        }

        /// <summary>
        /// 开除成员
        /// </summary>
        /// <param name="note"></param>
        public static void FamilyFire(UserNote note)
        {
            string playerid = note.GetString(0);
            if (playerid == note.PlayerID)
            {
                note.Call(FamilyCommand.FamilyFireR, false, TipManager.GetMessage(FamilyReturn.NoFireSelf));
                return;
            }
            PlayerEx familyEx = note.Player.Family;
            Variant v = familyEx.Value;
            if (string.IsNullOrEmpty(v.GetStringOrDefault("FamilyID")))
            {
                note.Call(FamilyCommand.FamilyFireR, false, TipManager.GetMessage(FamilyReturn.NoPower));
                return;
            }

            if (v.GetIntOrDefault("FamilyRoleID") != 0)
            {
                note.Call(FamilyCommand.FamilyFireR, false, TipManager.GetMessage(FamilyReturn.NoPower));
                return;
            }

            //开除玩家的扩展,族长或副族长才有开除权限

            PlayerBusiness pb = PlayersProxy.FindPlayerByID(playerid);
            PlayerEx firePlayer = pb.Family;

            if (firePlayer == null)
            {
                note.Call(FamilyCommand.FamilyFireR, false, TipManager.GetMessage(FamilyReturn.NoFamily));
                return;
            }

            if (firePlayer.Value.GetStringOrDefault("FamilyID") != familyEx.Value.GetStringOrDefault("FamilyID"))
            {
                note.Call(FamilyCommand.FamilyFireR, false, TipManager.GetMessage(FamilyReturn.NoPower));
                return;
            }

            if (firePlayer.Value.GetIntOrDefault("FamilyRoleID") == 0)
            {
                note.Call(FamilyCommand.FamilyFireR, false, TipManager.GetMessage(FamilyReturn.NoFireBoss));
                return;
            }

            Family model = FamilyAccess.Instance.FindOneById(familyEx.Value.GetStringOrDefault("FamilyID"));
            IList Persons = model.Value.GetValue<IList>("Persons");

            bool IsExist = false;
            for (int i = 0; i < Persons.Count; i++)
            {
                Variant p = Persons[i] as Variant;
                if (p.GetStringOrDefault("PlayerID") == playerid)
                {
                    Persons.Remove(p);
                    IsExist = true;
                    break;
                }
            }
            model.Save();
            if (IsExist)
            {
                firePlayer.Value["FamilyID"] = string.Empty;
                firePlayer.Value["FamilyName"] = string.Empty;
                //是否在冻结期
                firePlayer.Value["FreezeDate"] = DateTime.UtcNow.AddHours(12);
                firePlayer.Value["FamilyRoleID"] = 0;
                firePlayer.Save();
            }

            Variant self = new Variant();
            self.Add("ID", pb.ID);
            self.Add("Message", string.Format(TipManager.GetMessage(FamilyReturn.FireFamilySuccess), pb.Name, model.Name));


            note.Player.FamilyCall(FamilyCommand.FamilyFireR, model.Name, true, self);
            //for (int i = 0; i < Persons.Count; i++)
            //{
            //    Variant p = Persons[i] as Variant;

            //    //修改
            //    PlayerBusiness[] pbs = PlayersProxy.Players;
            //    foreach (PlayerBusiness pb in pbs) 
            //    {
            //        if (pb.FamilyName == model.Name) 
            //        {
            //            pb.Call(FamilyCommand.FamilyFireR, true, self);                        
            //        }
            //    }
            //    //PlayerBusiness PlayerOnLine = PlayersProxy.FindPlayerByID(p.GetStringOrDefault("PlayerID"));
            //    //if (PlayerOnLine != null && PlayerOnLine.Online && PlayerOnLine.ID != playerid)
            //    //{
            //    //    PlayerOnLine.Call(FamilyCommand.FamilyFireR, true, self);
            //    //}
            //}
            pb.SetFamilyName(string.Empty, string.Empty);
            if (pb.Pet != null)
            {
                PetAccess.PetReset(pb.Pet, null, false,pb.Mounts);
            }

            if (pb.Online)
            {
                Variant sn = new Variant();
                sn.Add("ID", pb.ID);
                sn.Add("Message", string.Format(TipManager.GetMessage(FamilyReturn.FireFamily), note.Player.Name, model.Name));
                pb.Call(FamilyCommand.FamilyFireR, true, sn);
            }


            Variant os = new Variant();
            os["ID"] = model.ID;
            os["Name"] = model.Name;
            os["TargetID"] = note.PlayerID;
            os["TargetName"] = note.Player.Name;
            os["Status"] = 4;
            os["SourceID"] = pb.ID;
            os["SourceName"] = pb.Name;
            note.Player.AddLogVariant(Actiontype.FamilyLog, null, null, os);
        }

        /// <summary>
        /// 任命副族长
        /// </summary>
        /// <param name="note"></param>
        public static void AppointedNegative(UserNote note)
        {
            string playerid = note.GetString(0);
            if (playerid == note.PlayerID)
            {
                note.Call(FamilyCommand.AppointedNegativeR, false, TipManager.GetMessage(FamilyReturn.NoAppointedSelf));
                return;
            }
            PlayerEx familyEx = note.Player.Family;
            if (familyEx.Value.GetStringOrDefault("FamilyID") == string.Empty)
            {
                note.Call(FamilyCommand.AppointedNegativeR, false, TipManager.GetMessage(FamilyReturn.NoFamily));
                return;
            }

            if (familyEx.Value.GetIntOrDefault("FamilyRoleID") != 0)
            {
                note.Call(FamilyCommand.AppointedNegativeR, false, TipManager.GetMessage(FamilyReturn.NoPower));
                return;
            }

            PlayerBusiness pb = PlayersProxy.FindPlayerByID(playerid);

            PlayerEx f = pb.Family;
            if (f.Value.GetIntOrDefault("FamilyRoleID") != 2)
            {
                note.Call(FamilyCommand.AppointedNegativeR, false, TipManager.GetMessage(FamilyReturn.NoAppointedMember));
                return;
            }

            Family model = FamilyAccess.Instance.FindOneById(familyEx.Value.GetStringOrDefault("FamilyID"));
            IList Persons = model.Value.GetValue<IList>("Persons");
            int count = 0;
            foreach (Variant d in Persons)
            {
                if (d.GetIntOrDefault("RoleID") == 1)
                    count++;
            }
            if (count >= 3)
            {
                note.Call(FamilyCommand.AppointedNegativeR, false, TipManager.GetMessage(FamilyReturn.AppointedCount));
                return;
            }
            for (int i = 0; i < Persons.Count; i++)
            {
                Variant p = Persons[i] as Variant;
                if (p.GetStringOrDefault("PlayerID") == playerid)
                {
                    p["RoleID"] = 1;
                    break;
                }
            }
            model.Save();

            f.Value["FamilyRoleID"] = 1;
            f.Save();
            //NegativeSuccess
            Variant self = new Variant();
            self.Add("ID", pb.ID);
            self.Add("Message", string.Format(TipManager.GetMessage(FamilyReturn.NegativeSuccess), pb.Name, model.Name));
            //self.Add("Message", "<f  color='ff3300'>【" + OnlineBusiness.Name + "】被任命为【" + model.Name + "】家族副族长</f>");

            note.Player.FamilyCall(FamilyCommand.AppointedNegativeR, model.Name, true, self);
            //FamilyCall(FamilyCommand.AppointedNegativeR, model.Name, true, self);
            //for (int i = 0; i < Persons.Count; i++)
            //{
            //    Variant p = Persons[i] as Variant;
            //    PlayerBusiness PlayerOnLine = PlayersProxy.FindPlayerByID(p.GetStringOrDefault("PlayerID"));
            //    if (PlayerOnLine != null && PlayerOnLine.Online)
            //    {
            //        PlayerOnLine.Call(FamilyCommand.AppointedNegativeR, true, self);
            //    }
            //}
            
            pb.SetFamilyName(model.Name, StrsRole[1]);

            Variant os = new Variant();
            os["ID"] = model.ID;
            os["Name"] = model.Name;
            os["TargetID"] = note.PlayerID;
            os["TargetName"] = note.Player.Name;
            os["Status"] = 5;
            os["SourceID"] = pb.ID;
            os["SourceName"] = pb.Name;
            note.Player.AddLogVariant(Actiontype.FamilyLog, null, null, os);
        }

        /// <summary>
        /// 撤消副族长
        /// </summary>
        /// <param name="note"></param>
        public static void FireNegative(UserNote note)
        {
            string playerid = note.GetString(0);
            if (playerid == note.PlayerID)
            {
                note.Call(FamilyCommand.FireNegativeR, false, TipManager.GetMessage(FamilyReturn.FireNegativeSelf));
                return;
            }
            PlayerEx familyEx = note.Player.Family;

            if (familyEx.Value.GetStringOrDefault("FamilyID") == string.Empty)
            {
                note.Call(FamilyCommand.FireNegativeR, false, TipManager.GetMessage(FamilyReturn.NoFamily));
                return;
            }

            if (familyEx.Value.GetIntOrDefault("FamilyRoleID") != 0)
            {
                note.Call(FamilyCommand.FireNegativeR, false, TipManager.GetMessage(FamilyReturn.NoPower));
                return;
            }

            PlayerBusiness pb = PlayersProxy.FindPlayerByID(playerid);
            PlayerEx player = pb.Family;
            if (player == null)
            {
                note.Call(FamilyCommand.FireNegativeR, false, TipManager.GetMessage(FamilyReturn.NoFamily));
                return;
            }
            if (player.Value.GetIntOrDefault("FamilyRoleID") != 1)
            {
                note.Call(FamilyCommand.FireNegativeR, false, TipManager.GetMessage(FamilyReturn.NoFireNegative));
                return;
            }
            Family model = FamilyAccess.Instance.FindOneById(familyEx.Value.GetStringOrDefault("FamilyID"));
            IList Persons = model.Value.GetValue<IList>("Persons");
            for (int i = 0; i < Persons.Count; i++)
            {
                Variant p = Persons[i] as Variant;
                if (p.GetStringOrEmpty("PlayerID") == playerid)
                {
                    p["RoleID"] = 2;
                    break;
                }
            }

            player.Value["FamilyRoleID"] = 2;
            player.Save();
            model.Save();
            pb.SetFamilyName(model.Name, StrsRole[2]);

            Variant self = new Variant();
            self.Add("ID", pb.ID);
            self.Add("Message",
                string.Format(TipManager.GetMessage(FamilyReturn.FireNegativeClientSuccess), pb.Name, model.Name));
            //self.Add("Message", "<f  color='ff3300'>【" + OnLineBusiness.Name + "】被撤消【" + model.Name + "】家族副族长</f>");
            //FireNegativeClientSuccess

            note.Player.FamilyCall(FamilyCommand.FireNegativeR, model.Name, true, self);

            Variant os = new Variant();
            os["ID"] = model.ID;
            os["Name"] = model.Name;
            os["TargetID"] = note.PlayerID;
            os["TargetName"] = note.Player.Name;
            os["Status"] = 6;
            os["SourceID"] = pb.ID;
            os["SourceName"] = pb.Name;
            note.Player.AddLogVariant(Actiontype.FamilyLog, null, null, os);
        }

        /// <summary>
        /// 更新家族公告
        /// </summary>
        /// <param name="note"></param>
        public static void UpdateFamilyNotice(UserNote note)
        {
            string notice = note.GetString(0);
            if (notice.Length > 35)
            {
                note.Call(FamilyCommand.UpdateFamilyNoticeR, false, TipManager.GetMessage(FamilyReturn.NoticeLength));
                return;
            }
            PlayerEx familyEx = note.Player.Family;
            if (familyEx.Value.GetStringOrDefault("FamilyID") == string.Empty)
            {
                note.Call(FamilyCommand.UpdateFamilyNoticeR, false, TipManager.GetMessage(FamilyReturn.NoFamily));
                return;
            }

            if (familyEx.Value.GetIntOrDefault("FamilyRoleID") >= 2 || familyEx.Value.GetIntOrDefault("FamilyRoleID") < 0)
            {
                //族员不能够修改公告
                note.Call(FamilyCommand.UpdateFamilyNoticeR, false, TipManager.GetMessage(FamilyReturn.NoPower));
                return;
            }

            Family model = FamilyAccess.Instance.FindOneById(familyEx.Value.GetStringOrDefault("FamilyID"));
            model.Value["Notice"] = notice;
            model.Save();

            note.Player.FamilyCall(FamilyCommand.UpdateFamilyNoticeR, model.Name, true, notice);

            //IList Persons = model.Value.GetValue<IList>("Persons");
            //for (int i = 0; i < Persons.Count; i++)
            //{
            //    Variant p = Persons[i] as Variant;
            //    PlayerBusiness PlayerOnLine = PlayersProxy.FindPlayerByID(p.GetStringOrDefault("PlayerID"));
            //    if (PlayerOnLine != null && PlayerOnLine.Online)
            //    {
            //        PlayerOnLine.Call(FamilyCommand.UpdateFamilyNoticeR, true, notice);
            //    }
            //}
            Variant os = new Variant();
            os["ID"] = model.ID;
            os["Name"] = model.Name;
            os["TargetID"] = note.PlayerID;
            os["TargetName"] = note.Player.Name;
            os["Status"] = 7;
            //os["SourceID"] = pb.ID;
            //os["SourceName"] = pb.Name;
            note.Player.AddLogVariant(Actiontype.FamilyLog, null, null, os);
        }

        /// <summary>
        /// 解散家族
        /// </summary>
        /// <param name="note"></param>
        public static void DissolveFamily(UserNote note)
        {
            PlayerEx familyEx = note.Player.Family;
            string familyid = familyEx.Value.GetStringOrDefault("FamilyID");
            if (familyEx.Value.GetIntOrDefault("FamilyRoleID") != 0)
            {
                note.Call(FamilyCommand.DissolveFamilyR, false, TipManager.GetMessage(FamilyReturn.NoPower));
                return;
            }
            //得到家族信息
            Family model = FamilyAccess.Instance.FindOneById(familyid);
            IList persons = model.Value.GetValue<IList>("Persons");

            Variant v = new Variant();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("PlayerName", note.Player.Name);
            dic.Add("FamilyName", model.Name);
            v.Add("Message", string.Format(TipManager.GetMessage(FamilyReturn.DissolveClientSuccess), note.Player.Name, model.Name));

            foreach (Variant d in persons)
            {
                string playerid = d.GetStringOrDefault("PlayerID");
                if (string.IsNullOrEmpty(playerid))
                    continue;
                PlayerBusiness pb = PlayersProxy.FindPlayerByID(playerid);
                if (pb == null)
                    continue;

                PlayerEx ex = pb.Family;
                if (ex == null)
                    continue;

                ex.Value["FamilyID"] = "";
                ex.Value["FamilyName"] = "";
                ex.Value["FreezeDate"] = DateTime.UtcNow;
                ex.Value["FamilyRoleID"] = 0;

                if (ex.Save())
                {
                    pb.SetFamilyName(string.Empty, string.Empty);

                    if (pb.Pet != null)
                    {
                        PetAccess.PetReset(pb.Pet, null, false, null);
                    }
                    pb.Call(FamilyCommand.DissolveFamilyR, true, v);
                }
            }

            FamilyAccess.Instance.DelFamily(familyid);

            Variant os = new Variant();
            os["ID"] = model.ID;
            os["Name"] = model.Name;
            os["TargetID"] = note.PlayerID;
            os["TargetName"] = note.Player.Name;
            os["Status"] = 9;
            //os["SourceID"] = pb.ID;
            //os["SourceName"] = pb.Name;
            note.Player.AddLogVariant(Actiontype.FamilyLog, null, null, os);
        }

        /// <summary>
        /// 移交族长 
        /// </summary>
        /// <param name="note"></param>
        public static void TransferBoss(UserNote note)
        {
            string playerid = note.GetString(0);
            if (playerid == note.PlayerID)
            {
                note.Call(FamilyCommand.TransferBossR, false, TipManager.GetMessage(FamilyReturn.TransferBossSelf));
                return;
            }
            PlayerEx familyEx = note.Player.Family;
            if (familyEx.Value.GetStringOrDefault("FamilyID") == string.Empty)
            {
                note.Call(FamilyCommand.TransferBossR, false, TipManager.GetMessage(FamilyReturn.NoFamily));
                return;
            }

            if (familyEx.Value.GetIntOrDefault("FamilyRoleID") != 0)
            {
                note.Call(FamilyCommand.TransferBossR, false, TipManager.GetMessage(FamilyReturn.NoPower));
                return;
            }
            PlayerBusiness pb = PlayersProxy.FindPlayerByID(playerid);

            PlayerEx player = pb.Family;
            if (player.Value.GetStringOrDefault("FamilyID") != familyEx.Value.GetStringOrDefault("FamilyID"))
            {
                note.Call(FamilyCommand.TransferBossR, false, TipManager.GetMessage(FamilyReturn.NoTransferBoss));
                return;
            }
            if (player.Value.GetIntOrDefault("FamilyRoleID") == 0)
            {
                note.Call(FamilyCommand.TransferBossR, false, TipManager.GetMessage(FamilyReturn.NoTransferBoss));
                return;
            }


            Family model = FamilyAccess.Instance.FindOneById(familyEx.Value.GetStringOrDefault("FamilyID"));


            familyEx.Value["FamilyRoleID"] = 2;
            player.Value["FamilyRoleID"] = 0;

            pb.SetFamilyName(model.Name, StrsRole[0]);
            note.Player.SetFamilyName(model.Name, StrsRole[2]);

            string str = string.Format(TipManager.GetMessage(FamilyReturn.TransferBossSuccess), note.Player.Name, model.Name, pb.Name);

            IList Persons = model.Value.GetValue<IList>("Persons");
            for (int i = 0; i < Persons.Count; i++)
            {
                Variant p = Persons[i] as Variant;
                string id = p.GetStringOrEmpty("PlayerID");
                PlayerBusiness PlayerOnLine = PlayersProxy.FindPlayerByID(id);
                Variant self = new Variant();
                if (id == note.PlayerID)
                {
                    self.Add("ID", note.PlayerID);
                    self.Add("Family", note.Player.Family);
                    p["RoleID"] = 2;
                }
                else if (id == pb.ID)
                {
                    self.Add("ID", pb.ID);
                    self.Add("Family", pb.Family);
                    p["RoleID"] = 0;
                }
                else
                {
                    self.Add("ID", note.PlayerID);
                }
                self.Add("Message", str);
                PlayerOnLine.Call(FamilyCommand.TransferBossR, true, self);
            }
            model.Save();
            player.Save();
            familyEx.Save();

            Variant os = new Variant();
            os["ID"] = model.ID;
            os["Name"] = model.Name;
            os["TargetID"] = note.PlayerID;
            os["TargetName"] = note.Player.Name;
            os["Status"] = 8;
            os["SourceID"] = pb.ID;
            os["SourceName"] = pb.Name;
            note.Player.AddLogVariant(Actiontype.FamilyLog, null, null, os);
        }

        /// <summary>
        /// 得到家族经验
        /// </summary>
        /// <param name="noti"></param>
        public static void FamilyExperience(INotification noti)
        {
            Variant d = noti.Body[0] as Variant;
            if (d.GetStringOrEmpty("Type") != "TaskServer")
                return;
            int number = d.GetIntOrDefault("Experience");

            Family model = FamilyAccess.Instance.FindOneById(d.GetStringOrDefault("FamilyID"));
            if (model == null)
                return;
            Variant mf = model.Value;
            if (mf == null)
                return;
            //当前家族等级
            int level = mf.GetIntOrDefault("Level");
            Variant dy = FamilyBase.FamilyCount(level + 1);
            if (dy != null)
            {
                mf.SetOrInc("Experience", number);
            }

            DateTime dt = DateTime.UtcNow;


            DateTime devTime;
            if (mf.TryGetValueT("DevTime", out devTime))
            {
                if (devTime.Date != dt.Date)
                {
                    mf["DayDev"] = number;
                }
                else
                {
                    mf.SetOrInc("DayDev", number);
                }
                mf["DevTime"] = dt;
            }
            else
            {
                //每日贡献度
                mf.SetOrInc("DayDev", number);
                mf["DevTime"] = dt;
            }

            PlayerBusiness pb = PlayersProxy.FindPlayerByID(d.GetStringOrDefault("PlayerID"));
            if (pb == null)
                return;
            PlayerEx fx = pb.Family;
            if (fx == null)
                return;
            fx.Value.SetOrInc("Devote", number);

            IList persons = mf.GetValue<IList>("Persons");
            for (int i = 0; i < persons.Count; i++)
            {
                Variant v = persons[i] as Variant;
                if (v.GetStringOrDefault("PlayerID") == d.GetStringOrDefault("PlayerID"))
                {
                    v.SetOrInc("Devote", number);
                    break;
                }
            }
            FamilyUp(model);
            fx.Save();
        }

        /// <summary>
        /// 家族模块信息通知
        /// </summary>
        /// <param name="note"></param>
        public static void LoginSuccess(UserNote note)
        {
            PlayerEx family = note.Player.Family;

            if (family == null) return;
            int FamilyRoleID = family.Value.GetIntOrDefault("FamilyRoleID");
            string FamilyName = family.Value.GetStringOrDefault("FamilyName");
            if (string.IsNullOrEmpty(FamilyName))
            {
                note.Player.SetFamilyName(string.Empty, string.Empty);
            }
            else
            {
                note.Player.SetFamilyName(FamilyName, StrsRole[FamilyRoleID]);
            }

            if (string.IsNullOrEmpty(family.Value.GetStringOrDefault("FamilyID")))
            {
                //表示没有家族的玩家，检查是否有人邀请
                foreach (string k in inviteList.Keys)
                {
                    Variant d = inviteList[k];
                    if (d.GetStringOrDefault("PlayerID") == note.PlayerID)
                    {
                        Variant tmp = new Variant();
                        tmp.Add("ID", d.GetStringOrDefault("SoleID"));
                        //"【{0}】邀请加入【{1}】"
                        //string msg = string.Format(TipManager.GetMessage(FamilyReturn.LoginSuccess1), d.GetStringOrDefault("Name"), FamilyName);
                        tmp.Add("Message", string.Format(TipManager.GetMessage(FamilyReturn.LoginSuccess1), d.GetStringOrDefault("Name"), FamilyName));
                        //Variant v = new Variant();
                        //v.Add("ID", k);
                        //v.Add("Message", note.Player.Name + "申请加入" + family.Name);
                        note.Call(FamilyCommand.FamilyInviteR, true, tmp);
                    }
                }
            }

            if (FamilyRoleID >= 0 && FamilyRoleID < 2)
            {
                //族长和副族长是否有人申请入族
                foreach (string k in applyList.Keys)
                {
                    Variant d = applyList[k];
                    Variant tmp = new Variant();
                    tmp.Add("ID", d.GetStringOrDefault("SoleID"));
                    tmp.Add("Message", string.Format(TipManager.GetMessage(FamilyReturn.LoginSuccess2), d.GetStringOrDefault("Name"), FamilyName));
                    //"【" + d.GetStringOrDefault("Name") + "】申请加入【" + FamilyName + "】");
                    if (d.GetStringOrDefault("FamilyID") == family.Value.GetStringOrDefault("FamilyID"))
                    {
                        note.Call(FamilyCommand.FamilyApplyR, true, tmp);
                    }
                }
            }
        }

        /// <summary>
        /// 得到家族技能
        /// </summary>
        /// <param name="note"></param>
        public static void FamilySkill(UserNote note)
        {
            PlayerEx model = note.Player.Family;
            if (model == null)
            {
                note.Call(FamilyCommand.FamilySkillR, false, TipManager.GetMessage(FamilyReturn.NoAddFamily));
                return;
            }
            if (string.IsNullOrEmpty(model.Value.GetStringOrDefault("FamilyID")))
            {
                note.Call(FamilyCommand.FamilySkillR, false, TipManager.GetMessage(FamilyReturn.NoAddFamily));
                return;
            }

            //得到家族信息
            Family family = FamilyAccess.Instance.FindOneById(model.Value.GetStringOrDefault("FamilyID"));
            if (family == null)
            {
                note.Call(FamilyCommand.FamilyMembersR, false, TipManager.GetMessage(FamilyReturn.FamilyError));
                return;
            }

            PlayerEx skill = note.Player.Skill;
            Variant v = skill.Value;

            Variant d = new Variant();
            d.Add("Devote", model.Value.GetIntOrDefault("Devote"));
            int level = family.Value.GetIntOrDefault("Level");
            d.Add("FamilyLevel", level);

            Variant maxFamily = FamilyBase.FamilyCount(level + 1);
            Variant exp = new Variant();
            if (maxFamily != null)
            {
                int maxExp = maxFamily.GetIntOrDefault("Exp");
                exp.Add("V", family.Value.GetIntOrDefault("Experience"));
                exp.Add("M", maxExp);
            }
            else
            {
                exp.Add("V", 0);
                exp.Add("M", 0);
            }
            d.Add("Experience", exp);

            Variant m;
            List<Variant> skillList = new List<Variant>();
            foreach (var item in v)
            {
                GameConfig gc = GameConfigAccess.Instance.FindOneById(item.Key);
                if (gc != null && gc.SubType == SkillSub.AdditionJ)
                {
                    //得到已经学习了的技能
                    m = new Variant(4);
                    m.Add("ID", gc.ID);
                    m.Add("UI", gc.UI);
                    m.Add("Name", gc.Name);
                    m.Add("Level", item.Value);
                    skillList.Add(m);
                }
            }

            List<GameConfig> tmp = GameConfigAccess.Instance.Find("Skill", SkillSub.AdditionJ);
            foreach (GameConfig gc in tmp)
            {
                bool isexist = true;//不存在
                foreach (Variant n in skillList)
                {
                    if (gc.ID == n.GetStringOrDefault("ID"))
                    {
                        isexist = false;
                        break;
                    }
                }
                if (isexist)
                {
                    m = new Variant();
                    m.Add("ID", gc.ID);
                    m.Add("UI", gc.UI);
                    m.Add("Name", gc.Name);
                    m.Add("Level", 0);//表示还没有学习的技能
                    skillList.Add(m);
                }
            }
            d.Add("SkillList", skillList);

            note.Call(FamilyCommand.FamilySkillR, true, d);

        }


        /// <summary>
        /// 家族技能学习
        /// </summary>
        /// <param name="note"></param>
        public static void StudyFamilySkill(UserNote note)
        {
            //学习技能
            string skillid = note.GetString(0);
            PlayerEx skill = note.Player.Skill;
            Variant v = skill.Value;
            int Level = 0;//升级等级
            if (v.ContainsKey(skillid))
                Level = v.GetIntOrDefault(skillid) + 1;
            else
                Level = 1;

            //得到技能相关信息
            GameConfig gc = GameConfigAccess.Instance.FindOneById(skillid);
            Variant d = gc.UI;
            if (!d.ContainsKey(Level.ToString()))
            {
                note.Call(FamilyCommand.StudyFamilySkillR, false, TipManager.GetMessage(FamilyReturn.SkillMax));
                return;
            }
            //家族扩展
            PlayerEx fex = note.Player.Family;
            if (fex.Value.GetStringOrDefault("FamilyID") == string.Empty)
            {
                note.Call(FamilyCommand.StudyFamilySkillR, false, TipManager.GetMessage(FamilyReturn.NoFamily));
                return;
            }
            int Devote = d.GetVariantOrDefault(Level.ToString()).GetIntOrDefault("Devote");
            if (fex.Value.GetIntOrDefault("Devote") < Devote)
            {
                note.Call(FamilyCommand.StudyFamilySkillR, false, TipManager.GetMessage(FamilyReturn.NoDevote));
                return;
            }
            //家族相关信息
            Family family = FamilyAccess.Instance.FindOneById(fex.Value.GetStringOrDefault("FamilyID"));
            if (family == null)
            {
                note.Call(FamilyCommand.StudyFamilySkillR, false, TipManager.GetMessage(FamilyReturn.FamilyError));
                return;
            }

            int FamilyLevel = d.GetVariantOrDefault(Level.ToString()).GetIntOrDefault("FamilyLevel");

            if (family.Value.GetIntOrDefault("Level") < FamilyLevel)
            {
                note.Call(FamilyCommand.StudyFamilySkillR, false, TipManager.GetMessage(FamilyReturn.NoFamilyLevel));
                return;
            }
            fex.Value.SetOrInc("Devote", -Devote);
            v.SetOrInc(skillid, 1);

            fex.Save();
            skill.Save();
            Variant self = new Variant();
            self.Add("ID", note.PlayerID);
            self.Add("Family", note.Player.Family);
            note.Call(FamilyCommand.StudyFamilySkillR, true, self);
            note.Player.RefeshSkill();

            if (note.Player.Pet != null)
            {
                PetAccess.PetReset(note.Player.Pet, note.Player.Skill, false,note.Player.Mounts);
                note.Call(PetsCommand.UpdatePetR, true, note.Player.Pet);
            }
        }

        /// <summary>
        /// 角色删除
        /// </summary>
        /// <param name="note"></param>
        public static void DeletePlayerSuccess(UserNote note)
        {
            PlayerEx family = note.Player.Family;
            if (family == null)
                return;

            Variant mv = family.Value;
            if (mv == null)
                return;

            string familyID = mv.GetStringOrDefault("FamilyID");
            //表示不存在家族
            if (string.IsNullOrEmpty(familyID))
                return;

            int familyRoleID = mv.GetIntOrDefault("FamilyRoleID");

            Family model = FamilyAccess.Instance.FindOneById(familyID);
            bool issave = true;
            if (model != null)
            {
                IList ps = model.Value.GetValue<IList>("Persons");
                if (ps == null) return;
                Variant t = null;
                foreach (Variant p in ps)
                {
                    if (p.GetStringOrDefault("PlayerID") == note.PlayerID)
                    {
                        t = p;
                        break;
                    }
                }

                if (t != null)
                {
                    ps.Remove(t);
                }

                if (ps.Count == 0)
                {
                    issave = false;
                    FamilyAccess.Instance.DelFamily(familyID);
                }

                if (familyRoleID == 0)
                {
                    Variant tmp = null;
                    foreach (Variant p in ps)
                    {
                        if (p.GetIntOrDefault("RoleID") == 1)
                        {
                            tmp = p;
                            break;
                        }
                    }

                    if (tmp == null && ps.Count > 0)
                    {
                        tmp = ps[0] as Variant;
                    }
                    if (tmp != null)
                    {
                        //设置为族长
                        tmp["RoleID"] = 0;
                        string playerid = tmp.GetStringOrDefault("PlayerID");
                        PlayerBusiness pb = PlayersProxy.FindPlayerByID(playerid);
                        if (pb != null)
                        {
                            PlayerEx pf = pb.Family;
                            pf.Value["FamilyRoleID"] = 0;
                            pb.SetFamilyName(model.Name, StrsRole[0]);
                            pf.Save();
                        }
                    }
                }
            }
            if (issave)
            {
                model.Save();
            }
            mv["FamilyID"] = "";
            mv["FamilyRoleID"] = "";
            mv["FamilyName"] = "";
            mv["Devote"] = 0;
            family.Save();
        }




        /// <summary>
        /// 经验回调函数
        /// </summary>
        /// <param name="family"></param>
        private static void FamilyUp(Family model)
        {
            Variant mf = model.Value;
            while (true)
            {
                int lv = mf.GetIntOrDefault("Level");
                Variant v = FamilyBase.FamilyCount(lv + 1);
                if (v == null)
                    break;

                //家族已经升到最高级
                int exp = mf.GetIntOrDefault("Experience");
                if (v.GetIntOrDefault("Exp") > exp)
                    break;
                mf["Experience"] = exp - v.GetIntOrDefault("Exp");
                mf.SetOrInc("Level", 1);
                //升级时间
                model.Modified = DateTime.UtcNow;
            }
            model.Save();
        }

        /// <summary>
        /// 移除过期申请
        /// </summary>
        private static void FamilyExpired()
        {
            //更新申请列表
            List<string> apply = new List<string>();

            foreach (string k in applyList.Keys)
            {
                Variant d = applyList[k];
                DateTime dt;
                if (!DateTime.TryParse(d.GetStringOrDefault("DateTime"), out dt))
                    continue;

                if (dt < DateTime.UtcNow)
                    apply.Add(k);
            }

            foreach (string k in apply)
            {
                if (applyList.ContainsKey(k))
                    applyList.Remove(k);
            }

            //更新邀请列表
            List<string> invite = new List<string>();
            foreach (string k in inviteList.Keys)
            {
                Variant d = inviteList[k];
                DateTime dt;
                if (!DateTime.TryParse(d.GetStringOrDefault("DateTime"), out dt))
                    continue;
                if (dt < DateTime.UtcNow)
                    invite.Add(k);
            }

            foreach (string k in invite)
            {
                if (inviteList.ContainsKey(k))
                    inviteList.Remove(k);
            }
        }
    }
}

