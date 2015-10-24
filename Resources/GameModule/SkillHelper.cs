using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.Util;
using Sinan.GameModule;
using Sinan.Entity;

namespace Sinan.GameModule
{
    public class SkillHelper
    {
        /// <summary>
        /// 宠物技能后缀
        /// </summary>
        public static string PetSkillSuffix = "[宠]";

        /// <summary>
        /// 初始化固定技能
        /// </summary>
        /// <param name="skills"></param>
        /// <returns></returns>
        public static Dictionary<string, Tuple<int, GameConfig>> InitFixBuffer(Variant skills)
        {
            Dictionary<string, Tuple<int, GameConfig>> fixBuffer = null;
            if (skills != null)
            {
                fixBuffer = new Dictionary<string, Tuple<int, GameConfig>>();
                foreach (var s in skills)
                {
                    GameConfig gs = GameConfigAccess.Instance.FindOneById(s.Key);
                    if (gs == null || gs.Name == null) continue;

                    string name = gs.Name.Replace(PetSkillSuffix, string.Empty);
                    if (fixBuffer.ContainsKey(name)) continue;

                    if (gs.SubType == SkillSub.AddAttack || gs.SubType == SkillSub.AddBuffer || gs.SubType == SkillSub.IncreaseBuffer)
                    {
                        //发动类型 被动0/ 主动大于1(冷切回合数) 
                        if (gs.UI.GetIntOrDefault("MaxUse") == 0)
                        {
                            fixBuffer.Add(name, new Tuple<int, GameConfig>(Convert.ToInt32(s.Value), gs));
                        }
                    }
                }
            }
            return fixBuffer;
        }
    }
}
