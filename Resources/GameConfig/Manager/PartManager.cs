using System.Collections.Generic;
using Sinan.Entity;
using Sinan.Util;
#if mono
using Sinan.Collections;
#else

#endif

namespace Sinan.GameModule
{
    /// <summary>
    /// 活动管理
    /// </summary>
    sealed public class PartManager : ConfigManager<Part>
    {
        readonly static PartManager m_instance = new PartManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static PartManager Instance
        {
            get { return m_instance; }
        }

        PartManager() { }

        protected override void CreateT(Variant v)
        {
            Part apc = new Part(v);
            m_configs[apc.ID] = apc;
        }

        /// <summary>
        /// 得到指定子类型的活动
        /// </summary>
        /// <param name="subtype"></param>
        /// <returns></returns>
        public List<Part> FindSub(List<string> subtype) 
        {
            List<Part> part = new List<Part>();
            foreach (Part p in m_configs.Values) 
            {
                if (subtype.Contains(p.SubType))
                {
                    part.Add(p);
                }
            }
            return part;
        }
    }
}
