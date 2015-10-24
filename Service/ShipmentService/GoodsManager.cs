using System.Collections.Generic;
using System.IO;
using System.Text;
using Sinan.FastConfig;
using Sinan.FastJson;
using Sinan.Util;
using Sinan.Data;

namespace Sinan.ShipmentService
{
    /// <summary>
    /// 用于验证物品价格.
    /// </summary>
    sealed public class GoodsManager : IConfigManager
    {
        readonly static GoodsManager m_instance = new GoodsManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static GoodsManager Instance
        {
            get { return m_instance; }
        }
        private GoodsManager() { }

        /// <summary>
        /// 角色升级获得技能的配置
        /// </summary>
        Dictionary<string, int> m_forntServers;

        public void Load(string path)
        {
            Variant roleConfig = VariantWapper.LoadVariant(path);
            Dictionary<string, int> forntServers = new Dictionary<string, int>();
            foreach (var v in roleConfig)
            {
                forntServers.Add(v.Key, int.Parse(v.Value.ToString()));
            }
            m_forntServers = forntServers;
        }

        public int GetValue(string name)
        {
            int t;
            m_forntServers.TryGetValue(name, out t);
            return t;
        }

        public void Unload(string fullPath)
        {
        }
    }
}
