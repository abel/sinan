using Sinan.Entity;
using Sinan.Util;

namespace Sinan.GameModule
{
    /// <summary>
    /// 明雷工厂
    /// </summary>
    sealed public class VisibleAPCManager : ConfigManager<VisibleApc>
    {
        readonly static VisibleAPCManager m_instance = new VisibleAPCManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static VisibleAPCManager Instance
        {
            get { return m_instance; }
        }

        VisibleAPCManager() { }

        protected override void CreateT(Variant v)
        {
            VisibleApc apc = new VisibleApc(v);
            m_configs[apc.ID] = apc;
        }

    }
}
