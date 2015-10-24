using Sinan.Data;
using Sinan.Util;

namespace Sinan.ArenaModule.Business
{
    public class ArenaSkill
    {
        private int m_injurytype;
        private int m_rolelimit;
        private int m_hpcost;
        private int m_mpcost;
        private int m_addtargets;
        private int m_range;
        private int m_coolingtime;
        private int m_a;
        private int m_b;

  
        /// <summary>
        /// 0物理攻击,1魔法攻击
        /// </summary>
        public int InjuryType
        {
            get { return m_injurytype; }
            set { m_injurytype = value; }
        }

        /// <summary>
        /// 职业限制
        /// </summary>
        public int RoleLimit 
        {
            get { return m_rolelimit; }
            set { m_rolelimit = value; }
        }

        /// <summary>
        /// 生命需要
        /// </summary>
        public int HPCost 
        {
            get { return m_hpcost; }
            set { m_hpcost = value; }
        }


        /// <summary>
        /// 魔法需要
        /// </summary>
        public int MPCost 
        {
            get { return m_mpcost; }
            set { m_mpcost = value; }
        }

        /// <summary>
        /// 附加受害人数
        /// </summary>
        public int AddTargets 
        {
            get { return m_addtargets; }
            set { m_addtargets = value; }
        }

        /// <summary>
        /// 攻击范围
        /// </summary>
        public int Range 
        {
            get { return m_range; }
            set { m_range = value; }
        }

        /// <summary>
        /// 技能冷却时间
        /// </summary>
        public int CoolingTime 
        {
            get { return m_coolingtime; }
            set { m_coolingtime = value; }
        }

        /// <summary>
        /// 技能受害起始值
        /// </summary>
        public int A 
        {
            get { return m_a; }
            set { m_a = value; }
        }

        /// <summary>
        /// 技能受害最大值
        /// </summary>
        public int B 
        {
            get { return m_b; }
            set { m_b = value; }
        }


        private Variant m_v;
        public ArenaSkill(GameConfig gc)
        {
            m_v = gc.Value;
            m_injurytype = m_v.GetIntOrDefault("InjuryType");
            m_rolelimit = m_v.GetIntOrDefault("RoleLimit");
            m_range = m_v.GetIntOrDefault("Range");
            m_coolingtime = m_v.GetIntOrDefault("CoolingTime");
        }

        /// <summary>
        /// 得到对应等级技能信息
        /// </summary>
        /// <param name="level"></param>
        public void BaseLevel(int level)
        {
            Variant mv = m_v.GetValueOrDefault<Variant>(level.ToString());
            m_hpcost = mv.GetIntOrDefault("HPCost");
            m_mpcost = mv.GetIntOrDefault("MPCost");
            m_addtargets = mv.GetIntOrDefault("AddTargets");
            m_a = mv.GetIntOrDefault("A");
            m_b = mv.GetIntOrDefault("B");
        }
    }
}
