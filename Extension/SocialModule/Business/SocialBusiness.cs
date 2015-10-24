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

namespace Sinan.SocialModule.Business
{
    class SocialBusiness
    {
        /// <summary>
        /// 得到社会基础信息
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<PlayerSimple> SocialInfo(IList list)
        {
            List<PlayerSimple> socialList = new List<PlayerSimple>();
            if (list == null)
                return socialList;
            List<string> ps = new List<string>();
            foreach (Variant d in list)
            {                                
                ps.Add(d.GetStringOrDefault("PlayerID"));
            }

            List<Player> playerList = PlayerAccess.Instance.GetPlayers(ps);
            if (playerList.Count > 0) 
            {
                foreach (Player player in playerList) 
                {
                    if (player != null)
                    {
                        socialList.Add(new PlayerSimple(player, 3));
                    }
                }
            }
            return socialList;
        }

        /// <summary>
        /// 判断是否允许
        /// </summary>
        /// <param name="note"></param>
        /// <param name="id">id</param>
        /// <param name="key">类型</param>
        /// <returns></returns>
        public static bool IsLet(PlayerEx social, string id, List<string> k)
        {
            foreach (string n in k)
            {
                IList list = null;
                if (n == "Master" || n == "Apprentice")
                {
                    Variant mentor = social.Value.GetValueOrDefault<Variant>("Mentor");
                    if (mentor == null) 
                        continue;
                    list = mentor[n] as IList;
                    if (list == null)
                        continue;
                }
                else
                {
                    list = social.Value.GetValue<IList>(n);
                }
                if (list != null)
                {
                    foreach (Variant d in list)
                    {
                        if (d == null) continue;
                        if (d.GetStringOrDefault("PlayerID") == id)
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 得到社交用户列表信息
        /// </summary>
        /// <param name="note"></param>
        public static void SocialList(UserNote note)
        {
            string name = note.GetString(0);
            PlayerEx soc = note.Player.Social;
            List<PlayerSimple> list = new List<PlayerSimple>();
            if (soc != null && soc.Value != null)
            {
                IList ls = null;
                if (name == "Master" || name == "Apprentice")
                {
                    Variant d = soc.Value.GetVariantOrDefault("Mentor");
                    if (d != null) ls = d[name] as IList;
                }
                else
                {
                    ls = soc.Value[name] as IList;
                }

                if (ls != null && ls.Count != 0)
                {
                    list = SocialBusiness.SocialInfo(ls);
                }
            }
            
            Variant tmp = new Variant();
            tmp.Add(name, list);
            note.Call(SocialCommand.SocialListR, true, tmp);
        }


        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="note"></param>
        public static void DeletePlayerSuccess(UserNote note)
        {
            PlayerEx social = note.Player.Social;
            Variant mv = social.Value;
            //得到需要解除的用户列表
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var item in mv)
            {
                if (item.Key == "Friends")
                {
                    IList list = item.Value as IList;
                    if (list != null && list.Count > 0)
                    {
                        foreach (Variant k in list)
                        {
                            dic.Add(k.GetStringOrDefault("PlayerID"), item.Key);
                        }
                    }
                }
                else if (item.Key == "Mentor")
                {
                    Variant v = item.Value as Variant;
                    if (v == null) 
                        continue;

                    IList app = v.GetValue<IList>("Apprentice");
                    if (app != null && app.Count>0) 
                    {
                        foreach (Variant k in app)
                        {
                            dic.Add(k.GetStringOrDefault("PlayerID"), "Apprentice");
                        }
                    }

                    IList mas = v.GetValue<IList>("Master");
                    if (mas != null && mas.Count>0)
                    {
                        foreach (Variant k in mas)
                        {
                            dic.Add(k.GetStringOrDefault("PlayerID"), "Master");
                        }
                    }
                }
            }

            if (dic.Count > 0)
            {
                foreach (var item in dic)
                {
                    //解除好友
                    string comm = SocialCommand.DelFriends;
                    if (item.Value != "Friends")
                    {
                        //解除师徒关系
                        comm = SocialCommand.DelMaster;
                    }

                    UserNote note2 = new UserNote(note.Player, comm, new object[] { item.Key });
                    Notifier.Instance.Publish(note2);
                }
            }

            //mv["Friends"] = new List<Variant>();
            //mv["Enemy"] = null;
            //Variant mt = mv.GetValueOrDefault<Variant>("Mentor");
            //mt["Apprentice"] = null;
            //mt["FreezeDate"] = null;
            //mt["Master"] = null;
            //social.Save();
        }
    }
}
