using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.FrontServer;
using Sinan.Entity;
using Sinan.Data;
using Sinan.Util;
using Sinan.GameModule;
using Sinan.Command;

#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.MemberModule.Bussiness
{
    class MemberBussiness
    {
        static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 会员增加
        /// </summary>
        /// <param name="note"></param>
        public static void MemberAdd(UserNote note)
        {
            int coin = note.GetInt32(0);
            PlayerBusiness pb = note.Player;
            pb.AddCZD(coin, GoodsSource.Recharge);
        }

        /// <summary>
        /// 会员升级,赠送兽栏
        /// </summary>
        /// <param name="note"></param>
        public static void MemberUp(UserNote note)
        {
            int mlevel = note.GetInt32(0);
            Variant mv = MemberAccess.MemberInfo(mlevel);
            if (mv == null)
                return;

            //宠物兽栏增加数量
            int cote = mv.GetIntOrDefault("Cote");
            if (cote <= 0) return;

            PlayerEx b3 = note.Player.B3;
            if (b3 == null)
                return;

            Variant v = b3.Value;
            if (v == null)
                return;

            bool ischange = false;
            IList c = v.GetValue<IList>("C");
            for (int i = 0; i < cote; i++)
            {
                Variant tmp = null;
                foreach (Variant t in c)
                {
                    if (t.GetStringOrDefault("E") != "-1")
                        continue;

                    if (tmp == null)
                    {
                        tmp = t;
                    }
                    else if (tmp.GetIntOrDefault("P") > t.GetIntOrDefault("P"))
                    {
                        tmp = t;
                    }
                }
                if (tmp == null)
                {
                    break;
                }
                else
                {
                    ischange = true;
                    tmp["E"] = "";
                }
            }
            if (ischange && b3.Save())
            {
                note.Player.UpdateBurden("B3");
            }
        }

        /// <summary>
        /// 在线如果存在跨天的情况
        /// </summary>
        /// <param name="pb"></param>
        public static void LoginCZD(UserNote note)
        {
            LoginCZD(note.Player);
        }

        /// <summary>
        /// 登录可以得到的成长度数量
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public static void LoginCZD(PlayerBusiness pb)
        {
            string soleid = pb.ID + "LoginCZD";
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                //上次登陆时间
                PlayerEx member = pb.Member;
                Variant v = member.Value;
                int czd = 0;

                int num = 0;//天数
                DateTime dt = DateTime.UtcNow;
                
                if (v.ContainsKey("MemberTime"))
                {
                    DateTime mt = v.GetDateTimeOrDefault("MemberTime");
                    num = Convert.ToInt32((dt.Date - mt.Date).TotalDays);
                }
                else
                {
                    DateTime created = pb.Created;
                    num = Convert.ToInt32((dt.Date - created.Date).TotalDays);
                }
                if (num < 1)
                    return;

                Variant mv = MemberAccess.MemberInfo(pb.MLevel);
                if (mv != null)
                {
                    //每天增加量
                    int b = mv.GetIntOrDefault("B");
                    czd = b * num;
                    if (czd > 0)
                    {
                        pb.AddCZD(czd, GoodsSource.LoginCZD);
                    }
                }
                v["MemberTime"] = DateTime.UtcNow;
                member.Save();

                //更新会员扩展
                pb.Call(ClientCommand.UpdateActorR, new PlayerExDetail(member));
            }
            finally 
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }
    }
}
