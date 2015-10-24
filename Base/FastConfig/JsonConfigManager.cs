using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sinan.FastJson;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FastConfig
{
    public class JsonConfigManager<T> : IConfigManager
         where T : class,IDictionary<string, object>, new()
    {
        protected readonly ConcurrentDictionary<string, T> m_configs = new ConcurrentDictionary<string, T>();

        /// <summary>
        /// 所有的值
        /// </summary>
        public T[] Values
        {
            get { return m_configs.Values.ToArray(); }
        }

        public virtual void Load(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            {
                string x = sr.ReadToEnd();
                T v = JsonConvert.DeserializeObject<T>(x);
                if (v != null)
                {
                    string id = Path.GetFileNameWithoutExtension(path);
                    m_configs[id] = v;
                }
            }
        }

        public virtual void Unload(string path)
        {
            string id = Path.GetFileNameWithoutExtension(path);
            T t;
            m_configs.TryRemove(id, out t);
        }

        public virtual bool TryGetValue(string id, out T t)
        {
            return m_configs.TryGetValue(id, out t);
        }

        public virtual T FindOne(string id)
        {
            T t;
            m_configs.TryGetValue(id, out t);
            return t;
        }
    }
}
