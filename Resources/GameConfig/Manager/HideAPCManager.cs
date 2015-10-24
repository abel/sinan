using Sinan.Entity;
using Sinan.Util;

namespace Sinan.GameModule
{
    sealed public class HideApcManager : ConfigManager<HideApc>
    {
        readonly static HideApcManager m_instance = new HideApcManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static HideApcManager Instance
        {
            get { return m_instance; }
        }

        HideApcManager() { }

        protected override void CreateT(Variant v)
        {
            HideApc apc = new HideApc(v);
            if (!string.IsNullOrEmpty(apc.SceneID))
            {
                m_configs[apc.ID] = apc;
            }
        }

        public HideApc Check(string sceneID, int x, int y)
        {
            foreach (var apc in m_configs.Values)
            {
                if (apc.SceneID == sceneID && apc.Range.Contains(x, y))
                {
                    return apc;
                }
            }
            return null;
        }
    }
}
