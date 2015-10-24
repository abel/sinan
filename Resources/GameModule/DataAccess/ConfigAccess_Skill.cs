using System.Collections;
using System.Collections.Generic;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.GameModule
{
    /// <summary>
    /// 技能,访问配置文件.
    /// </summary>
    sealed public partial class GameConfigAccess
    {        
        Dictionary<string, List<GameConfig>> m_skill = new Dictionary<string, List<GameConfig>>();
        /// <summary>
        /// 得到所有技能列表
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<GameConfig>> FindSkill()
        {
            if (m_skill.Count > 0) 
                return m_skill;
            List<GameConfig> list = Find(MainType.Skill);
            foreach (GameConfig gc in list) 
            {
                if (gc.SubType == SkillSub.AdditionJ) 
                    continue;
                Variant v = gc.Value;
                string RoleLimit = v.GetStringOrDefault("RoleLimit");
                List<GameConfig> sk;
                if (m_skill.TryGetValue(RoleLimit, out sk))
                {
                    sk.Add(gc);
                }
                else 
                {
                    sk = new List<GameConfig>();
                    sk.Add(gc);
                    m_skill.Add(RoleLimit, sk);
                }
            }
            return m_skill;
        }

        /// <summary>
        /// 当前御宠图鉴总数
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public int PetSkillCount(Variant v)
        {
            if (v == null) return 0;
            int A = 0;
            foreach (IList k in v.Values)
            {
                A += k.Count;
            }
            return A;
        }

        /// <summary>
        /// 得到某职业技能数量
        /// </summary>
        /// <param name="v"></param>
        /// <param name="role">职业</param>
        /// <returns></returns>
        public int PetJobSkillCount(Variant v, string role)
        {
            if (v == null) return 0;
            int A = 0;
            foreach (string k in v.Keys) 
            {
                GameConfig gc = FindOneById(k);
                if (gc == null || gc.Value == null) 
                    continue;
                string RoleLimit = gc.Value.GetStringOrDefault("RoleLimit");
                if (RoleLimit == "0" || RoleLimit == role)
                {
                    A += (v[k] as IList).Count;
                }
            }
            return 0;
        }

        /// <summary>
        /// 判断是否存在某等级的技能
        /// </summary>
        /// <param name="v"></param>
        /// <param name="skillid">技能ID</param>
        /// <param name="level">技能等级</param>
        /// <returns>true表示存在，false表示不存在</returns>
        public bool PetSkillIDLevel(Variant v, string skillid, int level)
        {
            if (v == null) return false;
            IList ls;
            if (v.TryGetValueT(skillid, out ls))
            {
                if (ls.Contains(level))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 宠物职业各等级技能个数
        /// </summary>
        /// <param name="v">御宠图鉴</param>
        /// <param name="role">职业</param>
        /// <param name="level">技能等级</param>
        /// <returns></returns>
        public int PetJobSkillLevel(Variant v, string role, int level)
        {
            if (v == null) return 0;
            int A = 0;
            foreach (string k in v.Keys)
            {
                GameConfig gc = FindOneById(k);
                if (gc == null || gc.Value == null)
                    continue;
                string RoleLimit = gc.Value.GetStringOrDefault("RoleLimit");
                if (RoleLimit == "0" || RoleLimit == role)
                {
                    IList ls = v[k] as IList;//相关等级
                    if (ls.Contains(level)) A++;
                }
            }
            return A;
        }

        /// <summary>
        /// 宠物不同技能达到某一等级
        /// </summary>
        /// <param name="v">御宠图鉴</param>
        /// <param name="list">达到条件</param>
        public bool PetSkillsToLevel(Variant v, IList list) 
        {
            if (v == null || list == null || list.Count == 0) 
                return false;
            foreach (Variant k in list) 
            {
                string skillID = k.GetStringOrDefault("SkillID");
                int level = k.GetIntOrDefault("A");
                if (!PetSkillIDLevel(v, skillID, level))
                    return false;
            }
            return true;
        }
    }
}
