using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sinan.Extensions;
using Sinan.FastConfig;

namespace Sinan.GameModule
{
    sealed public class WatchPlayerManager : IConfigManager
    {
        readonly static WatchPlayerManager m_instance = new WatchPlayerManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static WatchPlayerManager Instance
        {
            get { return m_instance; }
        }
        private WatchPlayerManager() { }


        HashSet<int> m_watchPlayer = new HashSet<int>();

        public void Load(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            {
                HashSet<int> watchPlayer = new HashSet<int>();

                string x = sr.ReadLine();
                while (x != null)
                {
                    string pid = x.Trim();
                    if (pid.Length > 0)
                    {
                        int id;
                        if (pid[0] == '\"')
                        {
                            id = StringFormat.ToHexNumber(pid.Trim('\"'));
                        }
                        else
                        {
                            int.TryParse(pid, out id);
                        }
                        if (id > 0)
                        {
                            watchPlayer.Add(id);
                        }
                    }
                    x = sr.ReadLine();
                }
                m_watchPlayer = watchPlayer;
            }
        }

        public void Unload(string path)
        {
            m_watchPlayer.Clear();
        }

        /// <summary>
        /// 是否在监听列表
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public bool Contains(int pid)
        {
            return m_watchPlayer.Contains(pid);
        }
    }
}
