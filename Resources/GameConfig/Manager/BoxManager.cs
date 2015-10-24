using Sinan.Entity;
using Sinan.Util;

namespace Sinan.GameModule
{
    sealed public class BoxManager : ConfigManager<Box>
    {
        readonly static BoxManager m_instance = new BoxManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static BoxManager Instance
        {
            get { return m_instance; }
        }

        BoxManager() { }
      

        protected override void CreateT(Variant v)
        {
            Box box = new Box(v);
            m_configs[box.ID] = box;
        }

    }
}
