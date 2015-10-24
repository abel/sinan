using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.FrontServer;
using Sinan.Entity;
using Sinan.Util;
using Sinan.Command;
using Sinan.Data;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Observer;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.SocialModule.Business
{
    class FriendsBusiness
    {
        static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="note"></param>
        public static void AddFriends(UserNote note)
        {
            string name = note.GetString(0);
            if (note.Player.Name == name)
            {
                return;
            }
            PlayerEx social = note.Player.Social;
            if (social == null)
            {
                note.Call(SocialCommand.AddFriendsR, false, TipManager.GetMessage(SocialReturn.UserInfoError));
                return;
            }

            PlayerBusiness player = PlayersProxy.FindPlayerByName(name);
            if (player == null)
            {
                note.Call(SocialCommand.AddFriendsR, false, TipManager.GetMessage(SocialReturn.UserInfoError));
                return;
            }

            if (social.Value != null)
            {
                if (SocialBusiness.IsLet(social, player.ID, new List<string> { "Friends" }))
                {
                    note.Call(SocialCommand.AddFriendsR, false, TipManager.GetMessage(SocialReturn.Friends));
                    return;
                }

                if (SocialBusiness.IsLet(social, player.ID, new List<string> { "Enemy" }))
                {
                    note.Call(SocialCommand.AddFriendsR, false, TipManager.GetMessage(SocialReturn.Master));
                    return;
                }
            }

            Variant v = new Variant();
            v.Add("PlayerID", player.ID);
            v.Add("Created", DateTime.UtcNow);
            IList list = social.Value.GetValue<IList>("Friends");
            int num = 0;
            if (list == null)
            {
                social.Value["Friends"] = new List<Variant>() { v };
                num = 1;
            }
            else
            {
                list.Add(v);
                num = list.Count;
            }
            social.Save();

            note.Call(SocialCommand.AddFriendsR, true, new PlayerSimple(player, 3));

            //添加好友
            note.Player.FinishNote(FinishCommand.Friends, num);
        }

        /// <summary>
        /// 删除好友
        /// </summary>
        /// <param name="note"></param>
        public static void DelFriends(UserNote note)
        {
            string playerid = note.GetString(0);


            PlayerEx Social = note.Player.Social;
            IList list = Social.Value.GetValue<IList>("Friends");
            if (list != null)
            {
                Variant msg = null;
                foreach (Variant d in list)
                {
                    if (d.GetStringOrDefault("PlayerID") == playerid)
                    {
                        msg = d;
                        break;
                    }
                }

                if (msg != null)
                {
                    list.Remove(msg);
                    Social.Save();
                }
            }

            PlayerBusiness player = PlayersProxy.FindPlayerByID(playerid);

            if (player != null)
            {
                PlayerEx Social1 = player.Social;
                IList list1 = Social1.Value.GetValue<IList>("Friends");
                if (list1 != null)
                {
                    Variant msg1 = null;
                    foreach (Variant d in list1)
                    {
                        if (d.GetStringOrDefault("PlayerID") == note.PlayerID)
                        {
                            msg1 = d;
                            break;
                        }
                    }

                    if (msg1 != null)
                    {
                        list1.Remove(msg1);
                        Social1.Save();
                    }
                }

                if (player.Online)
                {
                    Variant v1 = new Variant();
                    v1.Add("ID", note.PlayerID);
                    v1.Add("Message", string.Format(TipManager.GetMessage(SocialReturn.DelFriends1), note.Player.Name));
                    //"【" + note.Player.Name + "】解除与你的好友关系");
                    player.Call(SocialCommand.DelFriendsR, true, v1);
                }
            }

            Variant v = new Variant();
            v.Add("ID", playerid);
            //"你解除与【" + player.Name + "】的好友关系");
            v.Add("Message", string.Format(TipManager.GetMessage(SocialReturn.DelFriends2), player.Name));
            //得到好友列表信息
            note.Call(SocialCommand.DelFriendsR, true, v);
        }


        /// <summary>
        /// 申请加为好友
        /// </summary>
        /// <param name="note"></param>
        public static void FriendsApply(UserNote note)
        {
            string name = note.GetString(0);
            PlayerBusiness player = PlayersProxy.FindPlayerByName(name);
            if (player == null)
            {
                note.Call(SocialCommand.FriendsApplyR, false, TipManager.GetMessage(SocialReturn.UserInfoError), string.Empty);
                return;
            }
            if (!CheckFriends(note.Player, player, SocialCommand.FriendsApplyR, 0))
            {
                return;
            }
            //"等待【" + name + "】回复"
            note.Call(SocialCommand.FriendsApplyR, true, string.Format(TipManager.GetMessage(SocialReturn.FriendsApply), name), note.PlayerID);
            player.Call(SocialCommand.FriendsApplyR, true, note.Player.Name, note.PlayerID);
        }

        /// <summary>
        /// 得到申请回应
        /// </summary>
        /// <param name="note"></param>
        public static void FriendsBack(UserNote note)
        {
            string id = note.GetString(0);
            bool isagree = note.GetBoolean(1);
            PlayerBusiness pb = PlayersProxy.FindPlayerByID(id);
            if (!isagree)
            {
                pb.Call(SocialCommand.FriendsBackR, false, string.Format(TipManager.GetMessage(SocialReturn.FriendsBack1), note.Player.Name));
                //"【" + note.Player.Name + "】拒绝加你为好友", "");
                return;
            }

            if (!CheckFriends(note.Player, pb, SocialCommand.FriendsBackR, 1))
                return;

            Variant v = new Variant();
            v.Add("PlayerID", pb.ID);
            v.Add("Created", DateTime.UtcNow);

            PlayerEx social = note.Player.Social;
            IList enemy = social.Value.GetValue<IList>("Friends");
            int m = 0;
            int n = 0;
            if (enemy != null)
            {
                enemy.Add(v);
                m = enemy.Count;
            }
            else
            {
                social.Value["Friends"] = new List<Variant>() { v };
                m = 1;
            }


            Variant tmp = new Variant();
            tmp.Add("PlayerID", note.PlayerID);
            tmp.Add("Created", DateTime.UtcNow);

            PlayerEx social1 = pb.Social;
            IList enemy1 = social1.Value.GetValue<IList>("Friends");
            
            if (enemy1 != null)
            {
                enemy1.Add(tmp);
                n = enemy1.Count;
            }
            else
            {
                social1.Value["Friends"] = new List<Variant>() { tmp };
                n = 1;
            }
            social.Save();
            social1.Save();
            //"【" + note.Player.Name + "】与你成为好友,愿你们友谊在石器宝贝中长存"
            pb.Call(SocialCommand.FriendsBackR, true, string.Format(TipManager.GetMessage(SocialReturn.FriendsBack2), note.Player.Name), string.Empty);
            //"【" + user.Name + "】与你成为好友,愿你们友谊在石器宝贝中长存"
            note.Call(SocialCommand.FriendsBackR, true, string.Format(TipManager.GetMessage(SocialReturn.FriendsBack3), pb.Name), string.Empty);
            
            note.Player.FinishNote(FinishCommand.Friends, m);
            pb.FinishNote(FinishCommand.Friends, n);
        }

        /// <summary>
        /// 好友祝福
        /// </summary>
        /// <param name="note"></param>
        public static void FriendsBless(UserNote note)
        {
            string soleid = note.PlayerID + "FriendsBless";
            try
            {
                if (!m_dic.TryAdd(soleid, soleid))
                    return;

                string playerid = note.GetString(0);
                string goodsid = note.GetString(1);
                GameConfig gc = GameConfigAccess.Instance.FindOneById("BL_0001");
                if (gc == null || gc.Value == null)
                {
                    note.Call(SocialCommand.FriendsBlessR, false, TipManager.GetMessage(SocialReturn.FriendsBless1));
                    return;
                }

                Variant bless = gc.Value.GetValueOrDefault<Variant>("Bless");

                Variant tmp = null;
                foreach (var k in bless)
                {
                    string[] strs = k.Key.Split('-');
                    if (strs.Length < 2)
                    {
                        note.Call(SocialCommand.FriendsBlessR, false, TipManager.GetMessage(SocialReturn.FriendsBless1));
                        return;
                    }
                    int min = Convert.ToInt32(strs[0]);
                    int max = Convert.ToInt32(strs[1]);

                    if (note.Player.Level >= min && note.Player.Level <= max)
                    {
                        tmp = k.Value as Variant;
                        break;
                    }
                }

                if (tmp == null || (!tmp.ContainsKey(goodsid)))
                {
                    note.Call(SocialCommand.FriendsBlessR, false, TipManager.GetMessage(SocialReturn.FriendsBless1));
                    return;
                }


                Variant goods = tmp.GetValueOrDefault<Variant>(goodsid);

                Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
                Variant vv = new Variant();
                vv.Add("Number1", goods.GetIntOrDefault("Count"));
                dic.Add(goodsid, vv);

                if (BurdenManager.IsFullBurden(note.Player.B0, dic))
                {
                    note.Call(SocialCommand.FriendsBlessR, false, TipManager.GetMessage(SocialReturn.FriendsBless2));
                    return;
                }

                Variant beBless = gc.Value.GetValueOrDefault<Variant>("BeBless");
                Variant beTmp = null;
                foreach (var k in beBless)
                {
                    string[] strs = k.Key.Split('-');
                    if (strs.Length < 2)
                    {
                        note.Call(SocialCommand.FriendsBlessR, false, TipManager.GetMessage(SocialReturn.FriendsBless1));
                        return;
                    }
                    int min = Convert.ToInt32(strs[0]);
                    int max = Convert.ToInt32(strs[1]);

                    if (note.Player.Level >= min && note.Player.Level <= max)
                    {
                        beTmp = k.Value as Variant;
                        break;
                    }
                }

                if (beTmp == null)
                {
                    note.Call(SocialCommand.FriendsBlessR, false, TipManager.GetMessage(SocialReturn.FriendsBless1));
                    return;
                }
                int mn = 0;
                string beGoods = BeBless(beTmp, out mn);
                if (string.IsNullOrEmpty(beGoods) || mn <= 0)
                {
                    note.Call(SocialCommand.FriendsBlessR, false, TipManager.GetMessage(SocialReturn.FriendsBless1));
                    return;
                }
                GameConfig gc1 = GameConfigAccess.Instance.FindOneById(beGoods);
                if (gc1 == null)
                {
                    note.Call(SocialCommand.FriendsBlessR, false, TipManager.GetMessage(SocialReturn.FriendsBless1));
                    return;
                }

                PlayerBusiness pb = PlayersProxy.FindPlayerByID(playerid);
                if (pb == null)
                {
                    //"好友不存在");
                    note.Call(SocialCommand.FriendsBlessR, false, TipManager.GetMessage(SocialReturn.UserInfoError));
                    return;
                }

                PlayerEx ex;
                Variant v;
                if (!note.Player.Value.TryGetValueT("Bless", out ex))
                {
                    ex = new PlayerEx(note.PlayerID, "Bless");
                    v = new Variant();
                    v.Add("Total", 10);//允许祝福次数                    
                    v.Add("BlessList", null);
                    ex.Value = v;
                    //祝福时间
                    note.Player.Value.Add("Bless", ex);
                }
                v = ex.Value;
                Variant blessList = v.GetValueOrDefault<Variant>("BlessList");
                if (blessList != null)
                {
                    int count = 0;//今日祝福次数
                    foreach (DateTime k in blessList.Values)
                    {
                        if (k.ToLocalTime().Date == DateTime.Now.Date)
                            count++;
                        if (count >= v.GetIntOrDefault("Total"))
                        {
                            //"今日祝福次已经用完,每日最多能祝福好友10次"
                            note.Call(SocialCommand.FriendsBlessR, false, TipManager.GetMessage(SocialReturn.FriendsBless3));
                            return;
                        }
                    }

                    if (blessList.ContainsKey(playerid))
                    {
                        DateTime blessDate = blessList.GetDateTimeOrDefault(playerid);
                        if (blessDate.ToLocalTime().Date == DateTime.Now.Date)
                        {
                            //"每日同一好友只能祝福一次"
                            note.Call(SocialCommand.FriendsBlessR, false, TipManager.GetMessage(SocialReturn.FriendsBless4));
                            return;
                        }
                    }
                }
                else
                {
                    v["BlessList"] = new Variant();
                    blessList = v.GetValueOrDefault<Variant>("BlessList");
                }

                string gid = "G_d000683";

                if (!note.Player.RemoveGoods(gid, 1, GoodsSource.FriendsBless))
                {
                    //"你没有鲜花，无法祝福"
                    note.Call(SocialCommand.FriendsBlessR, false, TipManager.GetMessage(SocialReturn.FriendsBless5));
                    return;
                }

                //更新祝福时间
                blessList[playerid] = DateTime.UtcNow;
                ex.Save();

                note.Player.AddGoods(dic, GoodsSource.FriendsBless);
                note.Player.AddAcivity(ActivityType.FriendsBless, 1);

                if (mn > 0)
                {
                    Variant gs = new Variant();
                    gs.Add("G", beGoods);
                    gs.Add("A", mn);
                    gs.Add("E", beGoods);
                    gs.Add("H", 1);

                    List<Variant> goodsList = new List<Variant>();
                    goodsList.Add(gs);
                    int reTime = Convert.ToInt32(TipManager.GetMessage(EmailReturn.HameDay));
                    //"因为你好友【" + note.Player.Name + "】对你的祝福，你获得了【" + gc1.Name+ "】，为了你们的友谊，你也快祝福祝福他吧！";
                    string msg = string.Format(TipManager.GetMessage(SocialReturn.FriendsBless6), note.Player.Name, gc1.Name);
                    //if (EmailAccess.Instance.SendEmail("好友祝福", "系统邮件", pb.ID, pb.Name, msg, string.Empty, goodsList))
                    if (EmailAccess.Instance.SendEmail(TipManager.GetMessage(SocialReturn.FriendsBless7), TipManager.GetMessage(SocialReturn.FriendsBless8), pb.ID, pb.Name, msg, string.Empty, goodsList, reTime))
                    {
                        if (pb.Online)
                        {
                            pb.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(pb.ID));
                            pb.Call(SocialCommand.FriendsBlessR, true, note.Player.Name);
                        }
                    }
                }
                note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(ex));
                note.Call(SocialCommand.FriendsBlessR, true, TipManager.GetMessage(SocialReturn.FriendsBless9));
                note.Player.FinishNote(FinishCommand.WishFriends);

                //日志记录
                //note.Player.AddLog(Log.Actiontype.GoodsUse, gid, 1, GoodsSource.FriendsBless, "", 0);
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }


        /// <summary>
        /// 好友邀请
        /// </summary>
        /// <param name="note"></param>
        public static void InvitedFriends(UserNote note)
        {
            note.Player.AddAcivity(ActivityType.InvitedFriends, 1);
        }


        /// <summary>
        /// 好友分享
        /// </summary>
        /// <param name="note"></param>
        public static void FriendShare(UserNote note)
        {
            string id = note.GetString(0);
            GameConfig gc = GameConfigAccess.Instance.FindOneById(id);
            if (gc == null)
                return;
            Variant v = gc.Value;
            if (v == null)
                return;
            PlayerEx effort = note.Player.Effort;

            IList share;

            if (effort.Value.TryGetValueT("Share", out share))
            {
                //表示已经分享
                if (share.Contains(gc.ID))
                    return;
                share.Add(gc.ID);
            }
            else
            {
                share = new List<string>() { gc.ID };
                effort.Value.Add("Share", share);
            }

            if (!effort.Save())
                return;

            int bond = v.GetIntOrDefault("Bond");
            if (bond > 0)
            {
                note.Player.AddBond(bond, FinanceType.FriendShare, gc.ID);
            }
            note.Call(SocialCommand.FriendShareR, true, bond);
        }

        /// <summary>
        /// 添加好友检查
        /// </summary>
        /// <returns></returns>
        private static bool CheckFriends(PlayerBusiness model, PlayerBusiness player, string comm, int app)
        {
            if (player == null)
            {
                model.Call(comm, false, TipManager.GetMessage(SocialReturn.UserInfoError), string.Empty);
                return false;
            }

            if (model.ID == player.ID)
            {
                //"不能将自己加为好友"
                model.Call(comm, false, TipManager.GetMessage(SocialReturn.CheckFriends1), string.Empty);
                return false;
            }

            if (!player.Online)
            {
                //"【" + player.Name + "】不在线,不能加为好友"
                model.Call(comm, false, string.Format(TipManager.GetMessage(SocialReturn.CheckFriends2), player.Name), "");
                return false;
            }

            PlayerEx social = model.Social;
            if (social == null)
            {
                model.Call(comm, false, TipManager.GetMessage(SocialReturn.UserInfoError), string.Empty);
                return false;
            }

            IList enemy = social.Value.GetValue<IList>("Enemy");
            if (enemy != null)
            {
                if (enemy.Contains(player.ID))
                {
                    //"请先解除与【" + player.Name + "】的仇人关系再添加为好友"
                    model.Call(comm, false, string.Format(TipManager.GetMessage(SocialReturn.CheckFriends3), player.Name), string.Empty);
                    return false;
                }
            }

            IList friends = social.Value.GetValue<IList>("Friends");
            if (friends != null)
            {
                foreach (Variant v in friends)
                {
                    if (v.GetStringOrDefault("PlayerID") == player.ID)
                    {
                        //"【" + player.Name + "】与你已经是好友不能再添加"
                        model.Call(comm, false, string.Format(TipManager.GetMessage(SocialReturn.CheckFriends4), player.Name), string.Empty);
                        return false;
                    }
                }

                if (friends.Count >= 20)
                {
                    if (app == 0)
                    {
                        //"你的好友已满，其他玩家添加你好友失败"
                        model.Call(comm, false, TipManager.GetMessage(SocialReturn.CheckFriends5), string.Empty);
                    }
                    else
                    {
                        //TipManager.GetMessage("对方好友已满，无法添加")
                        model.Call(comm, false, TipManager.GetMessage(SocialReturn.CheckFriends6), string.Empty);
                    }
                    return false;
                }
            }



            PlayerEx social1 = player.Social;
            IList enemy1 = social.Value.GetValue<IList>("Enemy");
            if (enemy1 != null)
            {
                if (enemy.Contains(model.ID))
                {
                    //"【 + player.Name + "】已经将你加为仇人不能加为好代友"
                    model.Call(comm, false, string.Format(TipManager.GetMessage(SocialReturn.CheckFriends7), player.Name), string.Empty);
                    return false;
                }
            }

            IList friends1 = social1.Value.GetValue<IList>("Friends");
            if (friends1 != null)
            {
                foreach (Variant v in friends1)
                {
                    if (v.GetStringOrDefault("PlayerID") == player.ID)
                    {
                        //【 + player.Name + 】与你已经是好友不能再添加
                        model.Call(comm, false, string.Format(TipManager.GetMessage(SocialReturn.CheckFriends8), player.Name), string.Empty);
                        return false;
                    }
                }

                if (friends1.Count >= 20)
                {
                    if (app == 0)
                    {
                        //对方好友已满，无法添加
                        model.Call(comm, false, TipManager.GetMessage(SocialReturn.CheckFriends9), string.Empty);
                    }
                    else
                    {
                        //你的好友已满，其他玩家添加你好友失败
                        model.Call(comm, false, TipManager.GetMessage(SocialReturn.CheckFriends10), string.Empty);
                    }
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// 被祝福者得到的道具与数量
        /// </summary>
        /// <param name="beBless"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static string BeBless(Variant beBless, out int count)
        {
            int min = beBless.GetIntOrDefault("Min");
            int max = beBless.GetIntOrDefault("Max");
            Variant goods = beBless.GetValueOrDefault<Variant>("GoodsID");
            int rate = NumberRandom.Next(0, 100);
            string goodsid = "";
            foreach (var k in goods)
            {
                string[] strs = k.Value.ToString().Split('-');
                int m = Convert.ToInt32(strs[0]);
                int n = Convert.ToInt32(strs[1]);
                if (rate >= m && rate <= n)
                {
                    goodsid = k.Key;
                    break;
                }
            }
            count = NumberRandom.Next(min, max + 1);
            return goodsid;
        }
    }
}
