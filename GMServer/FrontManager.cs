using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.FastConfig;
using System.IO;
using Sinan.Util;
using Sinan.FastJson;

namespace Sinan.GMServer
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
        /// Key:分区ID, Value:游戏前端服务器地址
        /// </summary>
        Dictionary<int, string> m_forntServers;

        public void Load(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            {
                string x = sr.ReadToEnd();
                Variant roleConfig = JsonConvert.DeserializeObject<Variant>(x);
                Dictionary<int, string> forntServers = new Dictionary<int, string>();
                foreach (var v in roleConfig)
                {
                    int zoneid;
                    if (int.TryParse(v.Key, out zoneid))
                    {
                        forntServers.Add(zoneid, v.Value.ToString());
                    }
                }
                m_forntServers = forntServers;
            }
        }

        public string GetValue(int zoneid)
        {
            string t;
            m_forntServers.TryGetValue(zoneid, out t);
            return t;
        }

        public void Unload(string fullPath)
        {
            m_forntServers.Clear();
        }
    }
}
