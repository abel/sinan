using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sinan.FastConfig;
using Sinan.FastJson;
using Sinan.Util;

#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.GameModule
{
    public class SqlScriptManager : IConfigManager
    {
        readonly static SqlScriptManager m_instance = new SqlScriptManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static SqlScriptManager Instance
        {
            get { return m_instance; }
        }

        SqlScriptManager() { }


        protected readonly ConcurrentDictionary<string, string> m_scripts = new ConcurrentDictionary<string, string>();

        public void Load(string path)
        {
            string id = Path.GetFileNameWithoutExtension(path);
            using (FileStream fs = File.OpenRead(path))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            {
                m_scripts[id] = sr.ReadToEnd();
            }
        }

        public void Unload(string path)
        {
            string id = Path.GetFileNameWithoutExtension(path);
            string t;
            m_scripts.TryRemove(id, out t);
        }

        /// <summary>
        /// 获取脚本
        /// </summary>
        /// <param name="name"></param>
        /// <param name="script"></param>
        /// <returns></returns>
        public bool TryGetScript(string name, out string script)
        {
            return m_scripts.TryRemove(name, out script);
        }
    }
}
