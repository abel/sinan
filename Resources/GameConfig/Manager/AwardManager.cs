using System.Collections.Generic;
using System.IO;
using System.Text;
using Sinan.Data;
using Sinan.FastConfig;
using Sinan.FastJson;
using Sinan.Util;

namespace Sinan.GameModule
{
    sealed public class AwardManager : IConfigManager
    {
        readonly static AwardManager m_instance = new AwardManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static AwardManager Instance
        {
            get { return m_instance; }
        }

        Dictionary<string, Variant> m_award;
        private AwardManager() { }

        public void Load(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            {
                string key = sr.ReadToEnd();
                Variant v = JsonConvert.DeserializeObject<Variant>(key);
                Dictionary<string, Variant> award = new Dictionary<string, Variant>();
                foreach (string k in v.Keys)
                {
                    if (!award.ContainsKey(k))
                    {
                        award.Add(k, v[k] as Variant);
                    }
                }
                m_award = award;
            }
        }

        /// <summary>
        /// 奖励信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Variant FindOne(string id)
        {
            Variant v;
            m_award.TryGetValue(id, out v);
            return v;
        }

        public void Unload(string fullPath)
        {
        }
    }
}
