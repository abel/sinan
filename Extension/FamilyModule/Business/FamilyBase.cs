using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Data;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.FamilyModule.Business
{
    class FamilyBase
    {
        /// <summary>
        /// 创建家族的动态值
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public static Variant FamilyValue(UserNote note)
        {
            Variant d = new Variant();
            d.Add("Level", 1);
            d.Add("Experience", 0);
            d.Add("Notice", string.Empty);
            d.Add("Persons", new List<Variant>() { PersonInfo(note.PlayerID, 0) });
            d.Add("Skill", null);
            //d.Add("Level", 1);
            //d.Level = 1;
            //d.Experience = 0;
            //d.Notice = string.Empty;
            //d.Persons = new List<Variant>() { PersonInfo(note.PlayerID,0) };
            //d.Skill = null;//技能            
            return d;
        }

        /// <summary>
        /// 加入家族
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public static Variant PersonInfo(string playerid, int roleid)
        {
            Variant d = new Variant();
            d.Add("PlayerID", playerid);//用户ID
            d.Add("AddDate", DateTime.UtcNow);//添加时间
            d.Add("Devote", 0);//玩家贡献
            d.Add("RoleID", roleid);//职业0族长，1副族长，2族员
            d.Add("Fire", 0);//开除权限，0没有，1有
            //d.PlayerID = playerid;//用户ID
            //d.AddDate = DateTime.UtcNow;//添加时间
            //d.Devote = 0;//玩家贡献
            //d.RoleID = roleid;//职业0族长，1副族长，2族员
            //d.Fire = 0;//开除权限，0没有，1有
            return d;
        }

        /// <summary>
        /// 判断用户是否已经在该家族中
        /// </summary>
        /// <param name="Persons"></param>
        /// <param name="playerid"></param>
        /// <returns></returns>
        public static bool IsExist(IList Persons, string playerid)
        {
            foreach (Variant d in Persons)
            {
                if (d.GetStringOrEmpty("PlayerID") == playerid)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 得到家族对应等级家族基本信息
        /// </summary>
        /// <param name="level"></param>
        /// <returns>null表示不存在该等级</returns>
        public static Variant FamilyCount(int level)
        {
            GameConfig gc = GameConfigAccess.Instance.FindOneById("Family_Config");
            Variant v = null;
            IList list = gc.Value.GetValue<IList>("FamilyInfo");
            foreach (Variant d in list)
            {
                if (d.GetIntOrDefault("L") == level)
                {
                    v = d;
                    break;
                }
            }
            return v;
        }
    }
}
