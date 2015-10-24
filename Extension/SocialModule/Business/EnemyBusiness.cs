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
    class EnemyBusiness
    {
        /// <summary>
        /// 添加仇人
        /// </summary>
        /// <param name="note"></param>
        public static void AddEnemy(UserNote note)
        {            
            string name = note.GetString(0);
           
            PlayerEx Social = note.Player.Social;
            if (Social == null)
            {
                note.Call(SocialCommand.AddEnemyR, false, TipManager.GetMessage(SocialReturn.UserInfoError));
                return;
            }
            PlayerBusiness player = PlayersProxy.FindPlayerByName(name);
            if (player == null)
            {
                note.Call(SocialCommand.AddEnemyR, false, TipManager.GetMessage(SocialReturn.UserInfoError));
                return;
            }
            if (SocialBusiness.IsLet(Social, player.ID, new List<string> { "Enemy" }))
            {
                note.Call(SocialCommand.AddEnemyR, false, TipManager.GetMessage(SocialReturn.Enemy));
                return;
            }
            if (SocialBusiness.IsLet(Social, player.ID, new List<string> {  "Friends" }))
            {
                note.Call(SocialCommand.AddEnemyR, false, TipManager.GetMessage(SocialReturn.Friends));
                return;
            }
            if (SocialBusiness.IsLet(Social, player.ID, new List<string> {  "Apprentice" }))
            {
                note.Call(SocialCommand.AddEnemyR, false, TipManager.GetMessage(SocialReturn.Apprentice));
                return;
            }
            if (SocialBusiness.IsLet(Social, player.ID, new List<string> {  "Master" }))
            {
                note.Call(SocialCommand.AddEnemyR, false, TipManager.GetMessage(SocialReturn.Master));
                return;
            }

            IList Enemy = Social.Value.GetValue<IList>("Enemy");
            Variant v = new Variant();
            v.Add("PlayerID", player.ID);
            v.Add("Created", DateTime.UtcNow);
            if (Enemy == null)
            {
                Social.Value["Enemy"] = new List<Variant> { v };
            }
            else 
            {
                Enemy.Add(v);
            }
            Social.Save();
            note.Call(SocialCommand.AddEnemyR, true, new PlayerSimple(player, 3));
        }

        /// <summary>
        /// 删除仇人
        /// </summary>
        /// <param name="note"></param>
        public static void DelEnemy(UserNote note)
        {
            string playerid = note.GetString(0);
            PlayerEx Social = note.Player.Social;
            IList Enemy = Social.Value.GetValue<IList>("Enemy");
            if (Enemy != null)
            {
                Variant msg = null;
                foreach (Variant d in Enemy)
                {
                    if (d.GetStringOrDefault("PlayerID") == playerid)
                    {
                        msg = d;
                        break;
                    }
                }
                if (msg != null)
                {
                    ///移除成功
                    Enemy.Remove(msg);
                    Social.Save();
                }
            }
            Variant v = new Variant();
            v.Add("ID",playerid);
            note.Call(SocialCommand.DelEnemyR, true, v);
        }
    }
}
