using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Sinan.FastConfig;
using Sinan.FastJson;
using Sinan.Util;
using Sinan.Log;
using Sinan.Data;

namespace Sinan.ShipmentService
{
    sealed public class FrontManager : IConfigManager
    {
        readonly static FrontManager m_instance = new FrontManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static FrontManager Instance
        {
            get { return m_instance; }
        }
        private FrontManager() { }

        /// <summary>
        /// Key:分区ID, Value:游戏前端服务器发货地址
        /// </summary>
        Dictionary<int, IPEndPoint> m_forntServers;

        public void Load(string path)
        {
            Variant roleConfig = VariantWapper.LoadVariant(path);
            Dictionary<int, IPEndPoint> forntServers = new Dictionary<int, IPEndPoint>();
            foreach (var v in roleConfig)
            {
                int zoneid;
                if (int.TryParse(v.Key, out zoneid))
                {
                    string[] ap = v.Value.ToString().Split(':');
                    IPEndPoint p = new IPEndPoint(IPAddress.Parse(ap[0]), int.Parse(ap[1]));
                    forntServers.Add(zoneid, p);
                }
            }
            m_forntServers = forntServers;
            //LogWrapper.Warn("Load:" + path);
        }

        public IPEndPoint GetValue(int zoneid)
        {
            IPEndPoint t;
            m_forntServers.TryGetValue(zoneid, out t);
            return t;
        }

        public void Unload(string fullPath)
        {
            m_forntServers.Clear();
        }
    }
}
