using Sinan.Entity;
using Sinan.Util;

namespace Sinan.GameModule
{
    sealed public class ApcManager : ConfigManager<Apc>
    {
        readonly static ApcManager m_instance = new ApcManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static ApcManager Instance
        {
            get { return m_instance; }
        }

        ApcManager() { }

        protected override void CreateT(Variant v)
        {
            Apc APC = new Apc(v);
            m_configs[APC.ID] = APC;
        }
    }
}
