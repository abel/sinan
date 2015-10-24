using System;
using System.IO;
using System.Text;
using Sinan.FastConfig;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.GameModule
{
    /// <summary>
    /// 黑名单.
    /// </summary>
    public class BlackListManager : IConfigManager
    {
        readonly static BlackListManager m_instance = new BlackListManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static BlackListManager Instance
        {
            get { return m_instance; }
        }

        readonly ConcurrentDictionary<string, DateTime> m_keys = new ConcurrentDictionary<string, DateTime>();

        BlackListManager() { }

        public void Load(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            {
                string key = sr.ReadLine();
                while (key != null)
                {
                    if (key != string.Empty)
                    {
                        m_keys.TryAdd(key, DateTime.MaxValue);
                    }
                    key = sr.ReadLine();
                }
            }
        }

        /// <summary>
        /// 检查是否包含
        /// </summary>
        /// <returns></returns>
        public bool Contains(string key)
        {
            DateTime time;
            if (m_keys.TryGetValue(key, out time))
            {
                if (time > (time.Kind == DateTimeKind.Utc ? DateTime.UtcNow : DateTime.Now))
                {
                    return true;
                }
                m_keys.TryRemove(key, out time);
            }
            return false;
        }

        public bool AddBlack(string key, DateTime time)
        {
            m_keys[key] = time;
            return true;
        }

        public bool Remove(string key)
        {
            DateTime time;
            return m_keys.TryRemove(key, out time);
        }

        public void Unload(string fullPath)
        {
        }
    }
}