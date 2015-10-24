using System.IO;
using System.Text;
using Sinan.FastConfig;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.GMServer
{
    /// <summary>
    /// GM列表()
    /// </summary>
    public class GMManager : IConfigManager
    {
        readonly static GMManager m_instance = new GMManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static GMManager Instance
        {
            get { return m_instance; }
        }

        ConcurrentDictionary<string, string> m_keys;


        GMManager() { }

        public void Load(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            {
                ConcurrentDictionary<string, string> keys = new ConcurrentDictionary<string, string>();
                string key = sr.ReadLine();
                while (key != null)
                {
                    if (key != string.Empty)
                    {
                        string[] xx = key.Split(' ', ',', ';');
                        if (xx.Length == 2)
                        {
                            keys.TryAdd(xx[0], xx[1]);
                        }
                    }
                    key = sr.ReadLine();
                }
                m_keys = keys;
            }
        }

        /// <summary>
        /// GM登录
        /// </summary>
        /// <returns></returns>
        public bool Login(string name, string pwd)
        {
            string p;
            if (m_keys.TryGetValue(name, out p))
            {
                return p == pwd;
            }
            return false;
        }

        public void Unload(string fullPath)
        {
        }
    }
}
